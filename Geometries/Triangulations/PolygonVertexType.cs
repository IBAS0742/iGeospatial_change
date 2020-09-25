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
// The Polygon Triangulation is based on codes developed by
//          Frank Shen                                    
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;

namespace iGeospatial.Geometries.Triangulations
{
	/// <summary>
	/// Summary description for PolygonVertexType.
	/// </summary>
    [Serializable]
    public enum PolygonVertexType
    {
        /// <summary>
        /// 
        /// </summary>
        ErrorPoint   = 0,

        /// <summary>
        /// 
        /// </summary>
        ConvexPoint  = 1,

        /// <summary>
        /// 
        /// </summary>
        ConcavePoint = 2		
    }
}
