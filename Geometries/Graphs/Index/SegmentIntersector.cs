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

namespace iGeospatial.Geometries.Graphs.Index
{
    [Serializable]
    internal class SegmentIntersector
	{
        #region Private Fields

		/// <summary> These variables keep track of what types of intersections were
		/// found during ALL edges that have been intersected.
		/// </summary>
		private bool hasIntersection;
		private bool hasProper;
		private bool hasProperInterior;
		// the proper intersection point found
		private Coordinate properIntersectionPoint;
		
		private LineIntersector li;
		private bool includeProper;
		private bool recordIsolated;
		private int numIntersections;
		
		// testing only
		private int numTests;
		
		private NodeCollection[] bdyNodes;
        
        #endregion
 		
        #region Constructors and Destructor

        public SegmentIntersector(LineIntersector li, bool includeProper, bool recordIsolated)
        {
            this.li             = li;
            this.includeProper  = includeProper;
            this.recordIsolated = recordIsolated;
        }

        #endregion

        #region Public Properties

		/// <returns> the proper intersection point, or null if none was found
		/// </returns>
		public Coordinate ProperIntersectionPoint
		{
			get
			{
				return properIntersectionPoint;
			}
		}
		
		public bool HasIntersection
		{
            get
            {
                return hasIntersection;
            }
		}
        
        #endregion
		
        #region Public Methods

        public static bool IsAdjacentSegments(int i1, int i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }
		
        public void SetBoundaryNodes(NodeCollection bdyNodes0, NodeCollection bdyNodes1)
        {
            bdyNodes    = new NodeCollection[2];
            bdyNodes[0] = bdyNodes0;
            bdyNodes[1] = bdyNodes1;
        }

		/// <summary> 
		/// A proper intersection is an intersection which is interior to at least two
		/// line segments.  Note that a proper intersection is not necessarily
		/// in the interior of the entire Geometry, since another edge may have
		/// an endpoint equal to the intersection, which according to SFS semantics
		/// can result in the point being on the Boundary of the Geometry.
		/// </summary>
		public bool HasProperIntersection()
		{
			return hasProper;
		}

		/// <summary> A proper interior intersection is a proper intersection which is not
		/// contained in the set of boundary nodes set for this SegmentIntersector.
		/// </summary>
		public bool HasProperInteriorIntersection()
		{
			return hasProperInterior;
		}
		
		/// <summary> This method is called by clients of the EdgeIntersector class to test for and Add
		/// intersections for two segments of the edges being intersected.
		/// Note that clients (such as MonotoneChainEdges) may choose not to intersect
		/// certain pairs of segments for efficiency reasons.
		/// </summary>
		public void AddIntersections(Edge e0, int segIndex0, Edge e1, int segIndex1)
		{
			if (e0 == e1 && segIndex0 == segIndex1)
				return;

			numTests++;
			Coordinate p00 = e0.Coordinates[segIndex0];
			Coordinate p01 = e0.Coordinates[segIndex0 + 1];
			Coordinate p10 = e1.Coordinates[segIndex1];
			Coordinate p11 = e1.Coordinates[segIndex1 + 1];
			
			li.ComputeIntersection(p00, p01, p10, p11);
			//if (li.HasIntersection() && li.isProper()) Debug.println(li);
			// Always record any non-proper intersections.
			// If includeProper is true, record any proper intersections as well.
			if (li.HasIntersection)
			{
				if (recordIsolated)
				{
					e0.Isolated = false;
					e1.Isolated = false;
				}

				//intersectionFound = true;
				numIntersections++;
				// if the segments are adjacent they have at least one trivial intersection,
				// the shared endpoint.  Don't bother adding it if it is the
				// only intersection.
				if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
				{
					hasIntersection = true;
					if (includeProper || !li.Proper)
					{
						e0.AddIntersections(li, segIndex0, 0);
						e1.AddIntersections(li, segIndex1, 1);
					}

					if (li.Proper)
					{
						properIntersectionPoint = (Coordinate) li.GetIntersection(0).Clone();
						hasProper = true;
						if (!IsBoundaryPoint(li, bdyNodes))
							hasProperInterior = true;
					}
				}
			}
		}
        
        #endregion
		
        #region Private Methods

		/// <summary> A trivial intersection is an apparent self-intersection which in fact
		/// is simply the point shared by adjacent line segments.
		/// Note that closed edges require a special check for the point shared by the beginning
		/// and end segments.
		/// </summary>
		private bool IsTrivialIntersection(Edge e0, int segIndex0, Edge e1, int segIndex1)
		{
			if (e0 == e1)
			{
				if (li.IntersectionNum == 1)
				{
					if (IsAdjacentSegments(segIndex0, segIndex1))
						return true;
					if (e0.IsClosed)
					{
						int maxSegIndex = e0.NumPoints - 1;
						if ((segIndex0 == 0 && segIndex1 == maxSegIndex) || 
                            (segIndex1 == 0 && segIndex0 == maxSegIndex))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
		
		private bool IsBoundaryPoint(LineIntersector li, NodeCollection[] bdyNodes)
		{
			if (bdyNodes == null)
				return false;
			if (IsBoundaryPoint(li, bdyNodes[0]))
				return true;
			if (IsBoundaryPoint(li, bdyNodes[1]))
				return true;
			return false;
		}
		
		private bool IsBoundaryPoint(LineIntersector li, NodeCollection bdyNodes)
		{
			for (INodeEnumerator i = bdyNodes.GetEnumerator(); i.MoveNext(); )
			{
				Node node     = i.Current;
				Coordinate pt = node.Coordinate;
				if (li.IsIntersection(pt))
					return true;
			}
			return false;
		}
        
        #endregion
	}
}