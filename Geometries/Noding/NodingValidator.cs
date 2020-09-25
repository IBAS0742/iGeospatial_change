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
	/// Validates that a collection of <see cref="SegmentString"/>s is 
	/// correctly noded.
	/// Throws an appropriate exception if an noding error is found.
	/// </summary>
	[Serializable]
    internal class NodingValidator
	{
        #region Private Fields

        private LineIntersector li;
		
        private IList segStrings;
        
        #endregion
		
        #region Constructors and Destructor

		public NodingValidator(IList segStrings)
		{
            li = new RobustLineIntersector();

			this.segStrings = segStrings;
		}
        
        #endregion
		
        #region Public Methods

		public void CheckValid()
		{
            CheckEndPtVertexIntersections();
            CheckInteriorIntersections();
            CheckCollapses();
        }
        
        #endregion
		
        #region Private Methods
				
        private LineString ToLine(Coordinate p0, Coordinate p1, 
            Coordinate p2)
        {
            GeometryFactory fact = GeometryFactory.GetInstance();

            return fact.CreateLineString(new Coordinate[]{p0, p1, p2});
        }

        /// <summary> Checks if a segment string contains a segment pattern a-b-a (which implies a self-intersection)</summary>
        private void CheckCollapses()
        {
            for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
            {
                SegmentString ss = (SegmentString) i.Current;
                CheckCollapses(ss);
            }
        }
		
        private void CheckCollapses(SegmentString ss)
        {
            ICoordinateList pts = ss.Coordinates;
            int nCount          = pts.Count;

            for (int i = 0; i < nCount - 2; i++)
            {
                CheckCollapse(pts[i], pts[i + 1], pts[i + 2]);
            }
        }
		
        private void CheckCollapse(Coordinate p0, Coordinate p1, Coordinate p2)
        {
            if (p0.Equals(p2))
            {
                throw new GeometryException("Found non-noded collapse at " 
                    + ToLine(p0, p1, p2));
            }
        }

		private void CheckInteriorIntersections()
		{
			for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
			{
				SegmentString ss0 = (SegmentString) i.Current;

                for (IEnumerator j = segStrings.GetEnumerator(); j.MoveNext(); )
				{
					SegmentString ss1 = (SegmentString) j.Current;
					
					CheckInteriorIntersections(ss0, ss1);
				}
			}
		}
		
		private void CheckInteriorIntersections(SegmentString ss0, SegmentString ss1)
		{
			ICoordinateList pts0 = ss0.Coordinates;
			ICoordinateList pts1 = ss1.Coordinates;
			for (int i0 = 0; i0 < pts0.Count - 1; i0++)
			{
				for (int i1 = 0; i1 < pts1.Count - 1; i1++)
				{
					CheckInteriorIntersections(ss0, i0, ss1, i1);
				}
			}
		}
		
		private void CheckInteriorIntersections(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1)
		{
			if (e0 == e1 && segIndex0 == segIndex1)
				return ;
			//numTests++;
			Coordinate p00 = e0.Coordinates[segIndex0];
			Coordinate p01 = e0.Coordinates[segIndex0 + 1];
			Coordinate p10 = e1.Coordinates[segIndex1];
			Coordinate p11 = e1.Coordinates[segIndex1 + 1];
			
			li.ComputeIntersection(p00, p01, p10, p11);
			if (li.HasIntersection)
			{
				
				if (li.Proper || HasInteriorIntersection(li, p00, p01) || 
                    HasInteriorIntersection(li, p10, p11))
				{
					throw new GeometryException("found non-node based intersection at " 
                        + p00 + "-" + p01 + " and " + p10 + "-" + p11);
				}
			}
		}

		/// <returns> true if there is an intersection point which is not an endpoint of the segment p0-p1
		/// </returns>
		private bool HasInteriorIntersection(LineIntersector li, Coordinate p0, Coordinate p1)
		{
			for (int i = 0; i < li.IntersectionNum; i++)
			{
				Coordinate intPt = li.GetIntersection(i);
				if (!(intPt.Equals(p0) || intPt.Equals(p1)))
					return true;
			}
			return false;
		}
		
        /// <summary> 
        /// Checks for intersections between an endpoint of a segment string
        /// and an interior vertex of another segment string
        /// </summary>
        private void CheckEndPtVertexIntersections()
        {
			for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
			{
				SegmentString ss = (SegmentString) i.Current;
				ICoordinateList pts = ss.Coordinates;
				CheckEndPtVertexIntersections(pts[0], segStrings);
				CheckEndPtVertexIntersections(pts[pts.Count - 1], segStrings);
			}
		}
		
        private void CheckEndPtVertexIntersections(Coordinate testPt, 
            IList segStrings)
        {
			for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
			{
				SegmentString ss    = (SegmentString) i.Current;
				ICoordinateList pts = ss.Coordinates;
                int nCount          = pts.Count;

				for (int j = 1; j < nCount - 1; j++)
				{
					if (pts[j].Equals(testPt))
					{
						throw new GeometryException("Found endpt/interior pt intersection at index " 
                            + j + " :pt " + testPt);
					}
				}
			}
		}
        
        #endregion
	}
}