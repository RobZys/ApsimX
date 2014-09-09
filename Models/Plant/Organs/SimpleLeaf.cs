using System;
using System.Collections.Generic;
using System.Text;

using Models.Core;
using Models.PMF.Functions;
using Models.PMF.Functions.SupplyFunctions;
using System.Xml.Serialization;

namespace Models.PMF.Organs
{
    [Serializable]
    public class SimpleLeaf : BaseOrgan, AboveGround
    {
        #region Class Links
        [Link]
        Plant Plant = null;
        [Link(IsOptional = true)]
        Structure structure = null;
        [Link]
        ISummary Summary = null;
        #endregion
        
        #region Parameters
              [Link] Function  FRGRFunction = null;   // VPD effect on Growth Interpolation Set
              [Link(IsOptional = true)] Function PotentialBiomass = null;
              [Link] Function DMDemandFunction = null;
              [Link(IsOptional = true)] Function CoverFunction = null;
              [Link(IsOptional = true)] Function NitrogenDemandSwitch = null;
              [Link] Function NConc = null;
              [Link] Function LAIFunction = null;
              [Link] Function ExtinctionCoefficientFunction = null;
              [Link(IsOptional = true)]  RUEModel Photosynthesis = null;
              [Link] Function HeightFunction = null;
              [Link (IsOptional = true)] Function LaiDeadFunction = null;
              [Link(IsOptional = true)]  Function StructuralFraction = null;
        #endregion
                
        #region States and variables
               private double _WaterAllocation;
               private double NShortage = 0;   //if an N Shoratge how Much;
               public double BiomassYesterday = 0;
               private double _StructuralFraction = 1;

               private double EP { get; set; }
               public double K {get; set;}                      // Extinction Coefficient (Green)
               public double KDead { get; set; }                  // Extinction Coefficient (Dead)
               public double DeltaBiomass {get; set;}
               [Units("mm")]
               public override double WaterDemand
               {
                   get
                   {
                       return Plant.PotentialEP;
                   }
                   set
                   {
                       Plant.PotentialEP = value;
                   }
               }
               public double Transpiration { get { return EP; } }
               [Units("mm")]
               public override double FRGR { get; set; }
               public double Fw
               {
                   get
            {
                double F = 0;
                if (WaterDemand > 0)
                    F = EP / WaterDemand;
                else
                    F = 1;
                return F;
            }
               }
               public double Fn
               {
                   get 
                   {
                       double MaxNContent = Live.Wt * NConc.Value;
                       return Live.N/MaxNContent; 
                   } //FIXME: Nitrogen stress factor should be implemented in simple leaf.
               }
               public double LAI { get; set; }
               public double LAIDead { get; set; }
               [Units("mm")]
               public double Height { get; set; }
               public double CoverGreen
               {
                   get
                   {
                       if (CoverFunction == null)
                           return 1.0 - Math.Exp((-1 * ExtinctionCoefficientFunction.Value) * LAI);
                       return Math.Min(Math.Max(CoverFunction.Value, 0), 1);
                   }
               }
               public double CoverTotal
               {
                   get { return 1.0 - (1 - CoverGreen) * (1 - CoverDead); }
               }
               public double CoverDead
               {
                   get { return 1.0 - Math.Exp(-KDead * LAIDead); }
               }
               [Units("MJ/m^2/day")]
               [Description("This is the intercepted radiation value that is passed to the RUE class to calculate DM supply")]
               public double RadIntTot
        {
            get
            {
                return CoverGreen * MetData.Radn;
            }
        }
        #endregion

        #region Arbitrator Methods
             public override double WaterAllocation
               {
                   get { return _WaterAllocation; }
                   set
                   {
                       _WaterAllocation = value;
                       EP += _WaterAllocation;
                   }
               }
             public override BiomassPoolType DMDemand
        {
            get
            {
                double Demand = 0;
                if (DMDemandFunction != null)
                    Demand = DMDemandFunction.Value;
                else
                    Demand = 1;
                return new BiomassPoolType { Structural = Demand };
            }
        }
             public override BiomassSupplyType DMSupply
             {
                 get
                 {
                     if (Photosynthesis != null)
                         DeltaBiomass = Photosynthesis.Growth(RadIntTot);
                     return new BiomassSupplyType { Fixation = DeltaBiomass, Retranslocation = 0, Reallocation = 0 };
                 }
             }
             public override BiomassAllocationType DMAllocation
        {
            set
            {
                Live.StructuralWt += value.Structural;
            }
        }
             public override BiomassPoolType NDemand
             {
                 get
                 {
                     double StructuralDemand = 0; 
                     double NDeficit = 0;
                     if (NitrogenDemandSwitch == null)
                         NDeficit = 0;
                     if (NitrogenDemandSwitch != null)
                     {
                         if (NitrogenDemandSwitch.Value == 0)
                             NDeficit = 0;
                     }
                     
                     if (NConc == null)
                         NDeficit = 0;
                     else
                     {
                         StructuralDemand = NConc.Value * DeltaBiomass * _StructuralFraction;
                         NDeficit = Math.Max(0.0, NConc.Value * (Live.Wt + DeltaBiomass) - Live.N) - StructuralDemand;
                     } return new BiomassPoolType { Structural = StructuralDemand, NonStructural = NDeficit };
                 }
             }
             public override BiomassAllocationType NAllocation
        {
            set
            {
                if (NDemand.Structural == 0)
                    if (value.Structural == 0) { }//All OK
                    else
                        throw new Exception("Invalid allocation of N");

                if (value.Structural == 0.0)
                { }// do nothing
                else
                {
                    double NSupplyValue = value.Structural + value.NonStructural;

                    if ((NSupplyValue > 0))
                    {
                        //What do we need to meat demand;
                        double ReqN = NDemand.Structural + NDemand.NonStructural;

                        if (ReqN == NSupplyValue)
                        {
                            // All OK add and leave
                            NShortage = 0;

                            Live.StructuralN += ReqN * StructuralFraction.Value;
                            Live.MetabolicN += 0;
                            Live.NonStructuralN += ReqN * (1 - StructuralFraction.Value);
                            return;

                        }

                        if (NSupplyValue > ReqN)
                            throw new Exception("N allocated to Leaf left over after allocation");

                        //Thorecticaly only option left
                        if (NSupplyValue < ReqN)
                        {
                            NShortage = ReqN - NSupplyValue;
                            Live.StructuralN += Math.Min(NSupplyValue, NDemand.Structural);
                            Live.MetabolicN += 0;
                            Live.NonStructuralN += Math.Max(0,NSupplyValue - NDemand.Structural);
                            return;
                        }

                        throw new Exception("UnKnown Leaf N allocation problem");
                    }
                }
            }
        }
             public override double MinNconc
             {
                 get
                 {
                     return NConc.Value * StructuralFraction.Value;
                 }
             }
        #endregion

        #region Evnets
              public event NewCanopyDelegate NewCanopy;
              
              [EventSubscribe("DoDailyInitialisation")]
              private void OnDoDailyInitialisation(object sender, EventArgs e)
              {
                  if (PotentialBiomass != null)
                  {
                      //FIXME.  Have changed potential Biomass function to give delta rather than accumulation.  MCSP will need to be altered
                      DeltaBiomass = PotentialBiomass.Value; //- BiomassYesterday; //Over the defalt DM supply of 1 if there is a photosynthesis function present
                      //BiomassYesterday = PotentialBiomass.Value;
                  }
              
                  EP = 0;
              }
        #endregion
        
        #region Component Process Functions
              protected virtual void PublishNewCanopyEvent()
              {
                  if (NewCanopy != null)
                  {
                      Plant.LocalCanopyData.sender = Plant.Name;
                      Plant.LocalCanopyData.lai = (float)LAI;
                      Plant.LocalCanopyData.lai_tot = (float)(LAI + LAIDead);
                      Plant.LocalCanopyData.height = (float)Height;
                      Plant.LocalCanopyData.depth = (float)Height;
                      Plant.LocalCanopyData.cover = (float)CoverGreen;
                      Plant.LocalCanopyData.cover_tot = (float)CoverTotal;
                      NewCanopy.Invoke(Plant.LocalCanopyData);
                  }
              }
              public override void OnCut()
        {
            Summary.WriteMessage(FullPath, "Cutting " + Name + " from " + Plant.Name);
            Live.Clear();
            Dead.Clear();
        }
              public override void OnSow(SowPlant2Type Data)
              {
                  if (StructuralFraction != null)
                      _StructuralFraction = StructuralFraction.Value;
                
                  PublishNewCanopyEvent();
              }
        #endregion

        #region Top Level time step functions
             public override void DoPotentialDM()
             {
                 FRGR = FRGRFunction.Value;
                 if (CoverFunction != null)
                     LAI = (Math.Log(1 - CoverGreen) / (ExtinctionCoefficientFunction.Value * -1));
                 if (LAIFunction != null)
                     LAI = LAIFunction.Value;
                 
                 Height = HeightFunction.Value;
                
                 if (LaiDeadFunction != null)
                     LAIDead = LaiDeadFunction.Value;
                 else
                     LAIDead = 0;
                 PublishNewCanopyEvent();
             }
        #endregion

    }
}