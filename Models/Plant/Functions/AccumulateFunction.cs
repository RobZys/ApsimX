﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Models.Core;
using Models.PMF.Phen;

namespace Models.PMF.Functions
{
    [Serializable]
    [Description("Adds the value of all childern functions to the previous days accumulation between start and end phases")]
    public class AccumulateFunction : Function
    {
        //Class members
        private double AccumulatedValue = 0;
        private Model[] ChildFunctions;

        [Link]
        Phenology Phenology = null;

        public string StartStageName = "";
        public string EndStageName = "";
        private double FractionRemovedOnCut = 0; //FIXME: This should be passed from teh manager when "cut event" is called. Must be made general to other events.

        public override void OnSimulationCommencing()
        {
            AccumulatedValue = 0;
        }

        [EventSubscribe("NewWeatherDataAvailable")]
        private void OnNewWeatherDataAvailable(object sender, EventArgs e)
        {
            if (ChildFunctions == null)
                ChildFunctions = Children.MatchingMultiple(typeof(Function));

            if (Phenology.Between(StartStageName, EndStageName))
            {
                double DailyIncrement = 0.0;
                foreach (Function F in ChildFunctions)
                {
                    DailyIncrement = DailyIncrement + F.Value;
                }
                AccumulatedValue += DailyIncrement;
            }

        }

        
        public override double Value
        {
            get
            {
                if (ChildFunctions == null)
                    ChildFunctions = Children.MatchingMultiple(typeof(Function));

                return AccumulatedValue;
            }
        }

        [EventSubscribe("Cutting")]
        private void OnCut(object sender, EventArgs e)
        {
            AccumulatedValue -= FractionRemovedOnCut * AccumulatedValue;
        }

    }
}