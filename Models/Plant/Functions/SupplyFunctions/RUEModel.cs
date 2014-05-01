using System;
using System.Collections.Generic;
using System.Text;

using Models.Core;

namespace Models.PMF.Functions.SupplyFunctions
{
    [Serializable]
    public class RUEModel : ModelCollection
    {
        [Link]
        Plant Plant = null;

        [Link]
        Function RUE = null;

        [Link]
        Function FCO2 = null;

        [Link]
        Function FN = null;

        [Link]
        public Function FT = null;

        [Link]
        Function FW = null;

        [Link]
        public Function FVPD = null;

        [Link]
        WeatherFile MetData = null;


        #region Class Data Members
        //[Input]
        //public NewMetType MetData;

        
        public event NewPotentialGrowthDelegate NewPotentialGrowth;
        #endregion

        #region Associated variables
        
        public double VPD
        {
            get
            {
                const double SVPfrac = 0.66;

                double VPDmint = Utility.Met.svp((float)MetData.MinT) - MetData.vp;
                VPDmint = Math.Max(VPDmint, 0.0);

                double VPDmaxt = Utility.Met.svp((float)MetData.MaxT) - MetData.vp;
                VPDmaxt = Math.Max(VPDmaxt, 0.0);

                return SVPfrac * VPDmaxt + (1 - SVPfrac) * VPDmint;
            }
        }

        #endregion

        /// <summary>
        /// Total plant "actual" radiation use efficiency (for the day) corrected by reducing factors (g biomass/MJ global solar radiation) CHCK-EIT 
        /// </summary>
        [Units("gDM/MJ")]
        private double RueAct
        {
            get
            {
                double RueReductionFactor = Math.Min(FT.Value, Math.Min(FN.Value, FVPD.Value)) * FW.Value * FCO2.Value;
                return RUE.Value * RueReductionFactor;
            }
        }
        /// <summary>
        /// Daily growth increment of total plant biomass
        /// </summary>
        /// <param name="RadnInt">intercepted radiation</param>
        /// <returns>g dry matter/m2 soil/day</returns>
        public double Growth(double RadnInt)
        {
            return RadnInt * RueAct;
        }

        private void PublishNewPotentialGrowth()
        {
            // Send out a NewPotentialGrowthEvent.
            if (NewPotentialGrowth != null)
            {
                NewPotentialGrowthType GrowthType = new NewPotentialGrowthType();
                GrowthType.sender = Plant.Name;
                GrowthType.frgr = (float)Math.Min(FT.Value, Math.Min(FN.Value, FVPD.Value));
                NewPotentialGrowth.Invoke(GrowthType);
            }
        }
        [EventSubscribe("StartOfDay")]
        private void OnPrepare(object sender, EventArgs e)
        {
            if (Plant.InGround)
                PublishNewPotentialGrowth();
        }
    }
}