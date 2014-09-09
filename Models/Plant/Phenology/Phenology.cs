using System;
using System.Collections.Generic;
using System.Text;
using Models.Core;
using Models.PMF.Functions;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.IO;

namespace Models.PMF.Phen
{
    [Serializable]
    public class PhaseChangedType
    {
        public String OldPhaseName = "";
        public String NewPhaseName = "";
    }
    /// <summary>
    /// Perform daily increase of phenology.
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    [Serializable]
    public class Phenology : Model
    {
        #region Links
        [Link]
        private ISummary Summary = null;
        [Link]
        private Clock Clock = null;
        [Link(IsOptional = true)]
        private Function RewindDueToBiomassRemoved = null;
        [Link(IsOptional = true)]
        private Function AboveGroundPeriod = null;

        #endregion

        #region Events
        public delegate void PhaseChangedDelegate(PhaseChangedType Data);
        public event PhaseChangedDelegate PhaseChanged;
        public event NullTypeDelegate GrowthStage;
        #endregion

        #region Parameters
        [Link] public Function StageCode = null;
        [Link] public Function ThermalTime = null;

        private List<Phase> Phases;

        #endregion

        #region States
        private int CurrentPhaseIndex;
        private double _AccumulatedTT = 0;
        private string CurrentlyOnFirstDayOfPhase = "";
        private bool JustInitialised = true;
        private double FractionBiomassRemoved = 0;
        private DateTime SowDate = DateTime.MinValue;
        public bool Emerged = false;

        [XmlIgnore]
        /// <summary>
        /// A one based stage number.
        /// </summary>
        public double Stage { get; set;}

        public void Clear()
        {
            Stage = 1;
            _AccumulatedTT = 0;
            JustInitialised = true;
            Emerged = false;
            SowDate = Clock.Today;
            CurrentlyOnFirstDayOfPhase = "";
            CurrentPhaseIndex = 0;
            FractionBiomassRemoved = 0;
            foreach (Phase phase in Phases)
                phase.ResetPhase();
        }

        #endregion

        #region Outputs
        /// <summary>
        /// This property is used to retrieve or set the current phase name.
        /// </summary>
        
        [XmlIgnore]
        public string CurrentPhaseName
        {
            get
            {
                if (CurrentPhase == null)
                    return "";
                else
                    return CurrentPhase.Name;
            }
            set
            {
                int PhaseIndex = IndexOfPhase(value);
                if (PhaseIndex == -1)
                    throw new Exception("Cannot jump to phenology phase: " + value + ". Phase not found.");
                CurrentPhase = Phases[PhaseIndex];
            }
        }

        /// <summary>
        /// Return current stage name.
        /// </summary>
        
        public string CurrentStageName
        {
            get
            {
                if (OnDayOf(CurrentPhase.Start))
                    return CurrentPhase.Start;
                else
                    return "?";
            }
        }

        
        public double FractionInCurrentPhase
        {
            get
            {
                return Stage - (int)Stage;
            }
        }

        public int DaysAfterSowing
        {
            get
            {
                if (SowDate == DateTime.MinValue)
                    return 0;
                else
                    return (Clock.Today - SowDate).Days;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public Phenology() { }
        /// <summary>
        /// Initialize the phase list of phenology. 
        /// </summary>
        public override void OnLoaded()
        {
            Phases = new List<Phase>();
            foreach (Phase phase in Children.MatchingMultiple(typeof(Phase)))
                Phases.Add(phase);
        }

        public override void OnSimulationCommencing()
        {
            Clear();
        }

        public void OnSow()
        {
            Clear();
        }

        public void OnHarvest()
        {
            //Jump phenology to the end
            
            string OldPhaseName = CurrentPhase.Name;
            int EndPhase = Phases.Count;
            PhaseChangedType PhaseChangedData = new PhaseChangedType();
            PhaseChangedData.OldPhaseName = OldPhaseName;
            PhaseChangedData.NewPhaseName = Phases[EndPhase - 1].Name;
            PhaseChanged.Invoke(PhaseChangedData);
            CurrentPhaseName = Phases[EndPhase - 1].Name;
        }

        /// <summary>
        /// Look for a particular phase and return it's index or -1 if not found.
        /// </summary>
        public int IndexOfPhase(string Name)
        {
            for (int P = 0; P < Phases.Count; P++)
                if (Phases[P].Name.ToLower() == Name.ToLower())
                    return P;
            return -1;
        }

        /// <summary>
        /// Perform our daily timestep function. Get the current phase to do its
        /// development for the day. If TT is leftover after Phase is progressed, 
        /// and the timestep for the subsequent phase is calculated using leftover TT
        /// </summary>
        public void DoTimeStep()
        {
            // If this is the first time through here then setup some variables.
            if (Phases == null || Phases.Count == 0)
                OnSimulationCommencing();

            CurrentlyOnFirstDayOfPhase = "";
            if (JustInitialised)
            {
                CurrentlyOnFirstDayOfPhase = Phases[0].Start;
                JustInitialised = false;
            }
            double FractionOfDayLeftOver = CurrentPhase.DoTimeStep(1.0);

            if (FractionOfDayLeftOver > 0)
            {
                // Transition to the next phase.
                if (CurrentPhaseIndex + 1 >= Phases.Count)
                    throw new Exception("Cannot transition to the next phase. No more phases exist");

                if (CurrentPhase is EmergingPhase)
                    Emerged = true;
                
                CurrentPhase = Phases[CurrentPhaseIndex + 1];
                if (GrowthStage != null)
                    GrowthStage.Invoke();
 
   
                // Tell the new phase to use the fraction of day left.
                FractionOfDayLeftOver = CurrentPhase.AddTT(FractionOfDayLeftOver);
                Stage = CurrentPhaseIndex + 1;
            }
            else
                Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete;

            _AccumulatedTT += CurrentPhase.TTForToday;
            Util.Debug("Phenology.CurrentPhaseName=%s", CurrentPhase.Name.ToLower());
            Util.Debug("Phenology.CurrentStage=%f", Stage);
        }

        /// <summary>
        /// A utility property to return the current phase.
        /// </summary>
        [XmlIgnore]
        public Phase CurrentPhase
        {
            get
            {
                if (CurrentPhaseIndex >= Phases.Count)
                    return null;
                else
                    return Phases[CurrentPhaseIndex];
            }
                    
            private set
            {
                string OldPhaseName = CurrentPhase.Name;

                CurrentPhaseIndex = IndexOfPhase(value.Name);
                if (CurrentPhaseIndex == -1)
                    throw new Exception("Cannot jump to phenology phase: " + value + ". Phase not found.");

                CurrentlyOnFirstDayOfPhase = CurrentPhase.Start;

                // If the new phase is a rewind phase then reinitialise all phases and rewind back to the
                // first phase.
                if (Phases[CurrentPhaseIndex] is GotoPhase)
                {
                    foreach (Phase P in Phases)
                    {
                        P.ResetPhase();
                    }
                    GotoPhase GotoP = (GotoPhase)Phases[CurrentPhaseIndex];
                    CurrentPhaseIndex = IndexOfPhase(GotoP.PhaseNameToGoto);
                    if (CurrentPhaseIndex == -1)
                        throw new Exception("Cannot goto phase: " + GotoP.PhaseNameToGoto + ". Phase not found.");
                }
                CurrentPhase.ResetPhase();

                // Send a PhaseChanged event.
                if (PhaseChanged != null)
                {
                    //_AccumulatedTT += CurrentPhase.TTinPhase;
                    PhaseChangedType PhaseChangedData = new PhaseChangedType();
                    PhaseChangedData.OldPhaseName = OldPhaseName;
                    PhaseChangedData.NewPhaseName = CurrentPhase.Name;
                    PhaseChanged.Invoke(PhaseChangedData);
                    //Fixme, make this work again MyPaddock.Publish(CurrentPhase.Start);
                }
            }
        }

        /// <summary>
        /// A utility function to return true if the simulation is on the first day of the 
        /// specified stage.
        /// </summary>
        public bool OnDayOf(String StageName)
        {
            return (StageName.Equals(CurrentlyOnFirstDayOfPhase, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// A utility function to return true if the simulation is currently in the 
        /// specified phase.
        /// </summary>
        public bool InPhase(String PhaseName)
        {
            return CurrentPhase.Name.ToLower() == PhaseName.ToLower();
        }

        /// <summary>
        /// A utility function to return true if the simulation is currently between
        /// the specified start and end stages.
        /// </summary>
        public bool Between(String Start, String End)
        {
            string StartFractionSt = Utility.String.SplitOffBracketedValue(ref Start, '(', ')');
            double StartFraction = 0;
            if (StartFractionSt != "")
                StartFraction = Convert.ToDouble(StartFractionSt);

            string EndFractionSt = Utility.String.SplitOffBracketedValue(ref Start, '(', ')');
            double EndFraction = 0;
            if (EndFractionSt != "")
                EndFraction = Convert.ToDouble(EndFractionSt);

            int StartPhaseIndex = Phases.IndexOf(PhaseStartingWith(Start));
            int EndPhaseIndex = Phases.IndexOf(PhaseEndingWith(End));
            int CurrentPhaseIndex = Phases.IndexOf(CurrentPhase);

            if (StartPhaseIndex == -1 || EndPhaseIndex == -1)
                throw new Exception("Cannot test between stages " + Start + " " + End);

            if (CurrentPhaseIndex == StartPhaseIndex)
                return CurrentPhase.FractionComplete >= StartFraction;

            else if (CurrentPhaseIndex == EndPhaseIndex)
                return CurrentPhase.FractionComplete <= EndPhaseIndex;

            else
                return CurrentPhaseIndex >= StartPhaseIndex && CurrentPhaseIndex <= EndPhaseIndex;
        }

        /// <summary>
        /// A utility function to return the phenological phase that starts with
        /// the specified start stage name.
        /// </summary>
        public Phase PhaseStartingWith(String Start)
        {
            foreach (Phase P in Phases)
                if (P.Start == Start)
                    return P;
            throw new Exception("Unable to find phase starting with " + Start);
        }

        /// <summary>
        /// A utility function to return the phenological phase that ends with
        /// the specified start stage name.
        /// </summary>
        public Phase PhaseEndingWith(String End)
        {
            foreach (Phase P in Phases)
                if (P.End == End)
                    return P;
            throw new Exception("Unable to find phase ending with " + End);
        }

        /// <summary>
        /// A utility function to return true if a phenological phase is valid.
        /// </summary>
        public bool IsValidPhase(String Start)
        {
            foreach (Phase P in Phases)
                if (P.Start == Start)
                    return true;
            return false;
        }

        /// <summary>
        /// A utility function to return true if a phenological phase is valid.
        /// </summary>
        public bool IsValidPhase2(String PhaseName)
        {
            foreach (Phase P in Phases)
                if (P.Name == PhaseName)
                    return true;
            return false;
        }

        /// <summary>
        /// Write phenology info to summary file.
        /// </summary>
        internal void WriteSummary(TextWriter writer)
        {
            writer.WriteLine("   Phases:");
            foreach (Phase P in Phases)
                P.WriteSummary(writer);
        }

        /// <summary>
        /// Respond to a remove biomass event.
        /// </summary>
        internal void OnRemoveBiomass(double removeBiomPheno)
        {
            string existingStage = CurrentStageName;
            if (RewindDueToBiomassRemoved != null)
            {
                FractionBiomassRemoved = removeBiomPheno; // The RewindDueToBiomassRemoved function will use this.

                double ttCritical = TTInAboveGroundPhase;
                double removeFractPheno = RewindDueToBiomassRemoved.Value;
                double removeTTPheno = ttCritical * removeFractPheno;

                string msg;
                msg = "Phenology change:-\r\n";
                msg += "    Fraction DM removed  = " + removeBiomPheno.ToString() + "\r\n";
                msg += "    Fraction TT removed  = " + removeFractPheno.ToString() + "\r\n";
                msg += "    Critical TT          = " + ttCritical.ToString() + "\r\n";
                msg += "    Remove TT            = " + removeTTPheno.ToString() + "\r\n";

                double ttRemaining = removeTTPheno;
                for (int i = Phases.Count - 1; i >= 0; i--)
                {
                    Phase Phase = Phases[i];
                    if (Phase.TTinPhase > 0)
                    {
                        double ttCurrentPhase = Phase.TTinPhase;
                        if (ttRemaining > ttCurrentPhase)
                        {
                            Phase.ResetPhase();
                            ttRemaining -= ttCurrentPhase;
                            CurrentPhaseIndex -= 1;
                            if (CurrentPhaseIndex < 4)  //FIXME - hack to stop onEmergence being fired which initialises biomass parts
                            {
                                CurrentPhaseIndex = 4;
                                break;
                            }
                        }
                        else
                        {
                            Phase.Add(-ttRemaining);
                            // Return fraction of thermal time we are through the current
                            // phenological phase (0-1)
                            //double frac = Phase.FractionComplete;
                            //if (frac > 0.0 && frac < 1.0)  // Don't skip out of this stage - some have very low targets, eg 1.0 in "maturity"
                            //    currentStage = frac + floor(currentStage);

                            break;
                        }
                    }
                    else
                    { // phase is empty - not interested in it
                    }
                }
                Stage = (CurrentPhaseIndex + 1) + CurrentPhase.FractionComplete;

                if (existingStage != CurrentStageName)
                {
                    PhaseChangedType PhaseChangedData = new PhaseChangedType();
                    PhaseChangedData.OldPhaseName = existingStage;
                    PhaseChangedData.NewPhaseName = CurrentPhase.Name;
                    PhaseChanged.Invoke(PhaseChangedData);
                    //Fixme MyPaddock.Publish(CurrentPhase.Start);
                }
            }
        }

        private double TTInAboveGroundPhase
        {
            get
            {
                if (AboveGroundPeriod == null)
                    throw new Exception("Cannot find Phenology.AboveGroundPeriod function in xml file");

                int SavedCurrentPhaseIndex = CurrentPhaseIndex;
                double TTInPhase = 0.0;
                for (CurrentPhaseIndex = 0; CurrentPhaseIndex < Phases.Count; CurrentPhaseIndex++)
                {
                    if (AboveGroundPeriod.Value == 1)
                        TTInPhase += Phases[CurrentPhaseIndex].TTinPhase;
                }
                CurrentPhaseIndex = SavedCurrentPhaseIndex;
                return TTInPhase;
            }
        }
    }
}