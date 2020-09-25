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
	/// Implements the Snap Rounding technique described in Hobby, 
	/// Guibas & Marimont, and Goodrich et al.
	/// <para>
	/// Snap Rounding assumes that all vertices lie on a uniform grid
	/// (hence the precision model of the input must be fixed precision,
	/// and all the input vertices must be rounded to that precision).
	/// </para>
	/// <para>
	/// This implementation uses a monotone chains and a spatial index to
	/// speed up the intersection tests.
	/// </para>
	/// This implementation appears to be fully robust using an integer precision model.
	/// It will function with non-integer precision models, but the
	/// results are not 100% guaranteed to be correctly noded.
	/// </remarks>
	internal class MCIndexSnapRounder : INoder
	{
        #region Private Fields

		private PrecisionModel pm;
		private LineIntersector li;
		private double scaleFactor;
		private MCIndexNoder noder;
		private MCIndexPointSnapper pointSnapper;
		private IList nodedSegStrings;
        
        #endregion
		
        #region Constructors and Destructor

		public MCIndexSnapRounder(PrecisionModel pm)
		{
			this.pm          = pm;
            this.scaleFactor = pm.Scale;

			li = new RobustLineIntersector();
			li.PrecisionModel = pm;
		}
        
        #endregion
			
        #region Public Methods
		
        /// <summary> 
        /// Computes nodes introduced as a result of snapping segments to 
        /// vertices of other segments.
        /// </summary>
        /// <param name="segStrings">
        /// The list of segment strings to snap together.
        /// </param>
        public void ComputeVertexSnaps(IList edges)
        {
            for (IEnumerator i0 = edges.GetEnumerator(); i0.MoveNext(); )
            {
                SegmentString edge0 = (SegmentString) i0.Current;
                ComputeVertexSnaps(edge0);
            }
        }
        
        #endregion
		
        #region Private Methods

		private void CheckCorrectness(IList inputSegmentStrings)
		{
			IList resultSegStrings = 
                SegmentString.GetNodedSubstrings(inputSegmentStrings);
			NodingValidator nv = new NodingValidator(resultSegStrings);
			
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
			
            ComputeIntersectionSnaps(intersections);
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
		private IList FindInteriorIntersections(IList segStrings, 
            LineIntersector li)
		{
			IntersectionFinderAdder intFinderAdder = 
                new IntersectionFinderAdder(li);
			
            noder.SegmentIntersector = intFinderAdder;
			noder.ComputeNodes(segStrings);
			
            return intFinderAdder.InteriorIntersections;
		}
		
		/// <summary> 
		/// Computes nodes introduced as a result of snapping segments 
		/// to snap points (hot pixels).
		/// </summary>
		private void ComputeIntersectionSnaps(IList snapPts)
		{
			for (IEnumerator it = snapPts.GetEnumerator(); it.MoveNext(); )
			{
				Coordinate snapPt = (Coordinate) it.Current;
				HotPixel hotPixel = new HotPixel(snapPt, scaleFactor, li);
				
                pointSnapper.Snap(hotPixel);
			}
		}
		
		/// <summary> 
		/// Performs a brute-force comparison of every segment in 
		/// each <see cref="SegmentString"/>. This has n^2 performance.
		/// </summary>
		private void ComputeVertexSnaps(SegmentString e)
		{
			ICoordinateList pts0 = e.Coordinates;
            int nCount           = pts0.Count;

			for (int i = 0; i < nCount - 1; i++)
			{
				HotPixel hotPixel = new HotPixel(pts0[i], scaleFactor, li);
				bool isNodeAdded  = pointSnapper.Snap(hotPixel, e, i);
				
                // if a node is created for a vertex, that vertex must be noded too
				if (isNodeAdded)
				{
					e.AddIntersection(pts0[i], i);
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
			noder                = new MCIndexNoder();
			pointSnapper         = new MCIndexPointSnapper(
                noder.MonotoneChains, noder.Index);
			
            SnapRound(inputSegmentStrings, li);
			
			// testing purposes only - remove in final version
			//checkCorrectness(inputSegmentStrings);
		}
        
        #endregion
	}
}