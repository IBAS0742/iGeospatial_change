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

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Finds proper and interior intersections in a set of SegmentStrings,
	/// and adds them as nodes.
	/// </summary>
	[Serializable]
    internal class IntersectionFinderAdder : ISegmentIntersector
	{
		private LineIntersector li;
		private IList interiorIntersections;
		
		
		/// <summary> 
		/// Creates an intersection finder which finds all proper 
		/// intersections
		/// </summary>
		/// <param name="li">the LineIntersector to use
		/// </param>
		public IntersectionFinderAdder(LineIntersector li)
		{
			this.li               = li;
			interiorIntersections = new ArrayList();
		}
		
        public IList InteriorIntersections
        {
            get
            {
                return interiorIntersections;
            }
        }
			
		/// <summary> This method is called by clients
		/// of the <see cref="ISegmentIntersector"/> class to process
		/// intersections for two segments of the {@link SegmentStrings} being intersected.
		/// Note that some clients (such as {@link MonotoneChain}s) may optimize away
		/// this call for segment pairs which they have determined do not intersect
		/// (e.g. by an disjoint envelope test).
		/// </summary>
		public void ProcessIntersections(SegmentString e0, int segIndex0, 
            SegmentString e1, int segIndex1)
		{
			// don't bother intersecting a segment with itself
			if (e0 == e1 && segIndex0 == segIndex1)
				return;
			
			Coordinate p00 = e0.Coordinates[segIndex0];
			Coordinate p01 = e0.Coordinates[segIndex0 + 1];
			Coordinate p10 = e1.Coordinates[segIndex1];
			Coordinate p11 = e1.Coordinates[segIndex1 + 1];
			
			li.ComputeIntersection(p00, p01, p10, p11);
			
			if (li.HasIntersection)
			{
				if (li.IsInteriorIntersection())
				{
					for (int intIndex = 0; intIndex < li.IntersectionNum; intIndex++)
					{
						interiorIntersections.Add(li.GetIntersection(intIndex));
					}

					e0.AddIntersections(li, segIndex0, 0);
					e1.AddIntersections(li, segIndex1, 1);
				}
			}
		}
	}
}