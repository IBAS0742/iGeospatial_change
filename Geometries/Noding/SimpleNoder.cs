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

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Nodes a set of <see cref="SegmentString"/>s by
	/// performing a brute-force comparison of every segment to every other one.
	/// This has n^2 performance, so is too slow for use on large numbers
	/// of segments.
	/// </summary>
	[Serializable]
    internal class SimpleNoder : SinglePassNoder
	{
        private IList nodedSegStrings;
		
        public SimpleNoder()
		{
		}
			
        /// <summary> 
        /// Returns a collection of fully noded <see cref="SegmentString"/>s.
        /// The SegmentStrings have the same context as their parent.
        /// </summary>
        /// <value> 
        /// A Collection of SegmentStrings.
        /// </value>
        public override IList NodedSubstrings
        {
            get
            {
                return SegmentString.GetNodedSubstrings(nodedSegStrings);
            }
        }
		
		public override void ComputeNodes(IList inputSegStrings)
		{
            this.nodedSegStrings = inputSegStrings;
            
            for (IEnumerator i0 = inputSegStrings.GetEnumerator(); i0.MoveNext(); )
			{
				SegmentString edge0 = (SegmentString) i0.Current;

                for (IEnumerator i1 = inputSegStrings.GetEnumerator(); i1.MoveNext(); )
				{
					SegmentString edge1 = (SegmentString) i1.Current;
					ComputeIntersects(edge0, edge1);
				}
			}
		}
		
		private void ComputeIntersects(SegmentString e0, SegmentString e1)
		{
			ICoordinateList pts0 = e0.Coordinates;
			ICoordinateList pts1 = e1.Coordinates;
			for (int i0 = 0; i0 < pts0.Count - 1; i0++)
			{
				for (int i1 = 0; i1 < pts1.Count - 1; i1++)
				{
					segInt.ProcessIntersections(e0, i0, e1, i1);
				}
			}
		}
	}
}