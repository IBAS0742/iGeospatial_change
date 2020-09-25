#region License
// <copyright>
//         iGeospatial Geometries Package
//       
// This is part of the Open Geospatial Library for .NET.
// 
// Package Description:
// This is a collection of C# classes that implement the fundamental 
// operations required to validate a given geo-spatial data set to 
// a known topological specification.
// It aims to provide a complete implementation of the Open Geospatial
// Consortium (www.opengeospatial.org) specifications for Simple 
// Feature Geometry.
// 
// Contact Information:
//     Paul Selormey (paulselormey@gmail.com or paul@toolscenter.org)
//     
// Credits:
// This library is based on the JTS Topology Suite, a Java library by
// 
//     Vivid Solutions Inc. (www.vividsolutions.com)  
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;
using System.Collections;
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Visitors;

namespace iGeospatial.Geometries
{
	/// <summary>   
	/// Summary description for Arc.
	/// </summary>
    [Serializable]
    public abstract class Arc : Curve
    {
        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Arc"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this curve instance.
        /// </param>
        protected Arc(GeometryFactory factory) : base(factory)
        {
        }
        
        #endregion

        #region Public Properties

        public abstract Coordinate Center
        {
            get;
            set;
        }

        #endregion

        #region Public Methods

        #endregion
    }
}
