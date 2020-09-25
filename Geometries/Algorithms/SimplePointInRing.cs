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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Tests whether a Coordinate lies inside a ring, using a linear-time algorithm.
	/// </summary>
	public class SimplePointInRing : IPointInRing
	{
        private ICoordinateList pts;
		
		public SimplePointInRing(LinearRing ring)
		{
            if (ring == null)
            {
                throw new ArgumentNullException("ring");
            }

            pts = ring.Coordinates;
		}
		
		public bool IsInside(Coordinate pt)
		{
			return CGAlgorithms.IsPointInRing(pt, pts);
		}
	}
}