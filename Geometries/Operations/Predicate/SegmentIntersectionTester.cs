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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;
	
namespace iGeospatial.Geometries.Operations.Predicate
{
	/// <summary> 
	/// Tests if any line segments in two sets of CoordinateSequences 
	/// intersect.
	/// </summary>
	/// <remarks>
	/// Optimized for small geometry size. Short-circuited to return 
	/// as soon an intersection is found.
	/// </remarks>
	[Serializable]
    internal sealed class SegmentIntersectionTester
	{
		// for purposes of intersection testing, don't need to 
        // set precision model
		private LineIntersector m_objIntersector;
		private bool            m_bHasIntersection;
		
		public SegmentIntersectionTester()
		{
            m_objIntersector   = new RobustLineIntersector();
		}
		
		public bool HasIntersectionWithLineStrings(ICoordinateList seq, 
            IGeometryList lines)
		{
            int nGeometries = lines.Count;
            for (int i = 0; i < nGeometries; i++)
            {
                HasIntersection(seq, lines[i].Coordinates);

                if (m_bHasIntersection)
                    break;
            }

			return m_bHasIntersection;
		}
		
		public bool HasIntersection(ICoordinateList seq0, ICoordinateList seq1)
		{
            int nCount0 = seq0.Count;
            int nCount1 = seq1.Count;

			for (int i = 1; i < nCount0 && !m_bHasIntersection; i++)
			{
				Coordinate pt00 = seq0[i - 1];
				Coordinate pt01 = seq0[i];

				for (int j = 1; j < nCount1 && !m_bHasIntersection; j++)
				{
					Coordinate pt10 = seq1[j - 1];
					Coordinate pt11 = seq1[j];
					
					m_objIntersector.ComputeIntersection(pt00, pt01, 
                        pt10, pt11);

					if (m_objIntersector.HasIntersection)
						m_bHasIntersection = true;
				}
			}

			return m_bHasIntersection;
		}
	}
}