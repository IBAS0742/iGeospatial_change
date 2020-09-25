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

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding.SnapRounds
{
	/// <summary> 
	/// Uses Snap Rounding to compute a rounded, fully noded arrangement 
	/// from a set of <see cref="SegmentString"/>s.
	/// </summary>
	/// <remarks>
	/// Implements the Snap Rounding technique described in Hobby, Guibas & Marimont,
	/// and Goodrich et al.
	/// <para>
	/// Snap Rounding assumes that all vertices lie on a uniform grid
	/// (hence the precision model of the input must be fixed precision,
	/// and all the input vertices must be rounded to that precision).
	/// </para>
	/// <para>
	/// This implementation uses simple iteration over the line segments.
	/// </para>
	/// This implementation appears to be fully robust using an integer precision model.
	/// It will function with non-integer precision models, but the
	/// results are not 100% guaranteed to be correctly noded.
	/// </remarks>
	internal class SimpleSnapRounder : INoder
	{
        #region Private Fields

        private PrecisionModel  pm;
        private LineIntersector li;
        private double          scaleFactor;
        private IList           nodedSegStrings;
        
        #endregion
		
        #region Constructors and Destructor

        public SimpleSnapRounder(PrecisionModel pm)
        {
            this.pm           = pm;
            this.scaleFactor  = pm.Scale;

            li = new RobustLineIntersector();
			
            li.PrecisionModel = pm;
        }
        
        #endregion
		
        #region Public Methods
		
        /// <summary> 
        /// Computes nodes introduced as a result of snapping segments to 
        /// vertices of other segments
        /// </summary>
        /// <param name="segStrings">
        /// The list of segment strings to snap together.
        /// </param>
        public void ComputeVertexSnaps(IList edges)
        {
            for (IEnumerator i0 = edges.GetEnumerator(); i0.MoveNext(); )
            {
                SegmentString edge0 = (SegmentString) i0.Current;
                for (IEnumerator i1 = edges.GetEnumerator(); i1.MoveNext(); )
                {
                    SegmentString edge1 = (SegmentString) i1.Current;
					
                    ComputeVertexSnaps(edge0, edge1);
                }
            }
        }
		
        /// <summary> 
        /// Adds a new node (equal to the snap pt) to the segment
        /// if the segment passes through the hot pixel
        /// </summary>
        /// <param name="hotPix">
        /// </param>
        /// <param name="segStr">
        /// </param>
        /// <param name="segIndex">
        /// </param>
        /// <returns> <see langword="true"/> if a node was added
        /// </returns>
        public static bool AddSnappedNode(HotPixel hotPix, 
            SegmentString segStr, int segIndex)
        {
            Coordinate p0 = segStr.GetCoordinate(segIndex);
            Coordinate p1 = segStr.GetCoordinate(segIndex + 1);
			
            if (hotPix.Intersects(p0, p1))
            {
                segStr.AddIntersection(hotPix.Coordinate, segIndex);
				
                return true;
            }

            return false;
        }
        
        #endregion
		
        #region Private Methods

		private void CheckCorrectness(IList inputSegmentStrings)
		{
			IList resultSegStrings = 
                SegmentString.GetNodedSubstrings(inputSegmentStrings);
			NodingValidator nv           = 
                new NodingValidator(resultSegStrings);
			
            try
			{
				nv.CheckValid();
			}
			catch (Exception ex)
			{
                ExceptionManager.Publish(ex);
			}
		}

		private void SnapRound(IList segStrings, LineIntersector li)
		{
			IList intersections = FindInteriorIntersections(segStrings, li);
			
            ComputeSnaps(segStrings, intersections);
			
            ComputeVertexSnaps(segStrings);
		}
		
		/// <summary> 
		/// Computes all interior intersections in the collection of 
		/// <see cref="SegmentString"/>s, and returns their 
		/// <see cref="Coordinate"/>s.
		/// </summary>
		/// <returns> a list of Coordinates for the intersections
		/// </returns>
		/// <remarks>
		/// Does NOT node the segStrings.
		/// </remarks>
		private IList FindInteriorIntersections(IList segStrings, LineIntersector li)
		{
			IntersectionFinderAdder intFinderAdder = 
                new IntersectionFinderAdder(li);
			SinglePassNoder noder    = new MCIndexNoder();
			noder.SegmentIntersector = intFinderAdder;

			noder.ComputeNodes(segStrings);
			
            return intFinderAdder.InteriorIntersections;
		}
		
		
		/// <summary> 
		/// Computes nodes introduced as a result of snapping segments 
		/// to snap points (hot pixels).
		/// </summary>
		/// <param name="li">
		/// </param>
		private void ComputeSnaps(IList segStrings, IList snapPts)
		{
			for (IEnumerator i0 = segStrings.GetEnumerator(); i0.MoveNext(); )
			{
				SegmentString ss = (SegmentString)i0.Current;
				ComputeSnaps(ss, snapPts);
			}
		}
		
		private void ComputeSnaps(SegmentString ss, IList snapPts)
		{
			for (IEnumerator it = snapPts.GetEnumerator(); it.MoveNext(); )
			{
				Coordinate snapPt = (Coordinate) it.Current;
				HotPixel hotPixel = new HotPixel(snapPt, scaleFactor, li);
				for (int i = 0; i < ss.Count - 1; i++)
				{
					AddSnappedNode(hotPixel, ss, i);
				}
			}
		}
		
		/// <summary>   
		/// Performs a brute-force comparison of every segment in each 
		/// <see cref="SegmentString"/>. This has n^2 performance.
		/// </summary>
		private void ComputeVertexSnaps(SegmentString e0, SegmentString e1)
		{
			ICoordinateList pts0 = e0.Coordinates;
			ICoordinateList pts1 = e1.Coordinates;

            int nCount0 = pts0.Count;
            int nCount1 = pts1.Count;

			for (int i0 = 0; i0 < nCount0 - 1; i0++)
			{
				HotPixel hotPixel = new HotPixel(pts0[i0], scaleFactor, li);
				for (int i1 = 0; i1 < nCount1 - 1; i1++)
				{
					// don't snap a vertex to itself
					if (e0 == e1)
					{
						if (i0 == i1)
							continue;
					}

					bool isNodeAdded = AddSnappedNode(hotPixel, e1, i1);
					
                    // if a node is created for a vertex, that vertex must be noded too
					if (isNodeAdded)
					{
						e0.AddIntersection(pts0[i0], i0);
					}
				}
			}
		}
        
        #endregion
		
        #region INoder Members

		public IList NodedSubstrings
		{
			get
			{
				return SegmentString.GetNodedSubstrings(nodedSegStrings);
			}
		}

		public void ComputeNodes(IList inputSegmentStrings)
		{
			this.nodedSegStrings = inputSegmentStrings;

			SnapRound(inputSegmentStrings, li);
			
			// testing purposes only - remove in final version
			//checkCorrectness(inputSegmentStrings);
		}
        
        #endregion
    }
}