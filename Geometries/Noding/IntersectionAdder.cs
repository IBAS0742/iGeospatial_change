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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Computes the intersections between two line segments in 
	/// <see cref="SegmentString"/>s and adds them to each string.
	/// </summary> 
	/// <remarks>     
	/// The <see cref="ISegmentIntersector"/> is passed to a <see cref="INoder"/>.
	/// The <see cref="AddIntersections"/> method is called 
	/// whenever the <see cref="INoder"/> detects that two SegmentStrings 
	/// <c>might</c> intersect. This class is an example of the <c>Strategy</c> pattern.
	/// </remarks>
	[Serializable]
    internal class IntersectionAdder : ISegmentIntersector
	{
        #region Private Fields

		/// <summary> 
		/// These variables keep track of what types of intersections were
		/// found during ALL edges that have been intersected.
		/// </summary>
		private bool hasIntersection = false;
		private bool hasProper = false;
		private bool hasProperInterior = false;
		private bool hasInterior = false;
		
		// the proper intersection point found
		private Coordinate properIntersectionPoint = null;
		
		private LineIntersector li;
//		private bool isSelfIntersection;

        public int numIntersections = 0;
		public int numInteriorIntersections = 0;
		public int numProperIntersections = 0;
        
        #endregion
		
        #region Constructors and Destructor

		public IntersectionAdder(LineIntersector li)
		{
			this.li = li;
		}
        
        #endregion
		
        #region Public Properties

		public LineIntersector LineIntersector
		{
			get
			{
				return this.li;
			}
		}
			
		/// <returns> 
		/// The proper intersection point, or <see langword="null"/> if 
		/// none was found.
		/// </returns>
		public Coordinate ProperIntersectionPoint
		{
			get
			{
				return this.properIntersectionPoint;
			}
		}
		
		public bool HasIntersection
		{
            get
            {
                return this.hasIntersection;
            }
		}

		/// <summary> A proper intersection is an intersection which is interior to at least two
		/// line segments.  Note that a proper intersection is not necessarily
		/// in the interior of the entire Geometry, since another edge may have
		/// an endpoint equal to the intersection, which according to SFS semantics
		/// can result in the point being on the Boundary of the Geometry.
		/// </summary>
		public bool HasProperIntersection
		{
            get
            {
                return this.hasProper;
            }
		}

		/// <summary> 
		/// A proper interior intersection is a proper intersection which is <b>not</b>
		/// contained in the set of boundary nodes set for this SegmentIntersector.
		/// </summary>
		public bool HasProperInteriorIntersection
		{
            get
            {
                return this.hasProperInterior;
            }
		}

		/// <summary> 
		/// An interior intersection is an intersection which is in the 
		/// interior of some segment.
		/// </summary>
		public bool HasInteriorIntersection
		{
            get
            {
                return this.hasInterior;
            }
		}
        
        #endregion
			
        #region Public Methods

        public static bool IsAdjacentSegments(int i1, int i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }
		
		/// <summary> 
		/// A trivial intersection is an apparent self-intersection which in fact
		/// is simply the point shared by adjacent line segments.
		/// Note that closed edges require a special check for the point shared by the beginning
		/// and end segments.
		/// </summary>
		private bool IsTrivialIntersection(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1)
		{
			if (e0 == e1)
			{
				if (li.IntersectionNum == 1)
				{
					if (IsAdjacentSegments(segIndex0, segIndex1))
						return true;

					if (e0.Closed)
					{
						int maxSegIndex = e0.Count - 1;
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
		
		/// <summary> 
		/// This method is called by clients of the <see cref="ISegmentIntersector"/> class to process
		/// intersections for two segments of the {@link SegmentStrings} being intersected.
		/// Note that some clients (such as {@link MonotoneChain}s) may optimize away
		/// this call for segment pairs which they have determined do not intersect
		/// (e.g. by an disjoint envelope test).
		/// </summary>
		public void ProcessIntersections(SegmentString e0, int segIndex0, 
            SegmentString e1, int segIndex1)
		{
			if (e0 == e1 && segIndex0 == segIndex1)
				return;

			Coordinate p00 = e0.Coordinates[segIndex0];
			Coordinate p01 = e0.Coordinates[segIndex0 + 1];
			Coordinate p10 = e1.Coordinates[segIndex1];
			Coordinate p11 = e1.Coordinates[segIndex1 + 1];
			
			li.ComputeIntersection(p00, p01, p10, p11);

            if (li.HasIntersection)
			{
				//intersectionFound = true;
				numIntersections++;
				if (li.IsInteriorIntersection())
				{
					numInteriorIntersections++;
					this.hasInterior = true;
				}

				// if the segments are adjacent they have at least one trivial intersection,
				// the shared endpoint.  Don't bother adding it if it is the
				// only intersection.
				if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
				{
					this.hasIntersection = true;
					e0.AddIntersections(li, segIndex0, 0);
					e1.AddIntersections(li, segIndex1, 1);

					if (li.Proper)
					{
						numProperIntersections++;

                        this.hasProper         = true;
						this.hasProperInterior = true;
					}
				}
			}
		}
        
        #endregion
    }
}