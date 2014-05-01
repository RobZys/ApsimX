﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Models.Core;

namespace Models.PMF.Functions
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    [Description("From the value of the first child function, subtract the values of the subsequent children functions")]
    public class SubtractFunction : Function
    {
        private List<Function> Children { get { return ModelsMatching<Function>(); } }
        public override double Value
        {
            get
            {
                double returnValue = 0.0;
                if (Children.Count > 0)
                {
                    Function F = Children[0] as Function;
                    returnValue = F.Value;

                    if (Children.Count > 1)
                        for (int i = 1; i < Children.Count; i++)
                        {
                            F = Children[i] as Function;
                            returnValue = returnValue - F.Value;
                        }

                }
                return returnValue;
            }
        }

    }
}