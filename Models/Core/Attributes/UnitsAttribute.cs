﻿// -----------------------------------------------------------------------
// <copyright file="UnitsAttribute.cs" company="APSIM Initiative">
//     Copyright (c) APSIM Initiative
// </copyright>
// -----------------------------------------------------------------------
namespace Models.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    /// <summary>
    /// Specifies the units of the related field or property. Units must conform to the specification in Section 2.6.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UnitsAttribute : System.Attribute
    {
        /// <summary>
        /// The units passed through the constructor
        /// </summary>
        private string unitsString;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitsAttribute" /> class.
        /// </summary>
        /// <param name="units">The units of the associated field or property</param>
        public UnitsAttribute(string units)
        {
            this.unitsString = units;
        }

        /// <summary>
        /// Returns the units.
        /// </summary>
        /// <returns>The units string</returns>
        public string ToString()
        {
            return this.unitsString;
        }
    }
}
