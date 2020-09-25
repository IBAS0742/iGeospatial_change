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
using System.Diagnostics;
using System.Collections;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding.SnapRounds
{
	/// <summary> 
	/// Implements a "hot pixel" as used in the Snap Rounding algorithm.
	/// </summary>
	/// <remarks>
	/// A hot pixel contains the interior of the tolerance square and
	/// the boundary <b>minus</b> the top and right segments.
	/// <para>
	/// The hot pixel operations are all computed in the integer domain
	/// to avoid rounding problems.
	/// </para>
	/// </remarks>
	internal class HotPixel
	{
        #region Private Fields

		private LineIntersector li;
		
		private Coordinate pt;
		private Coordinate originalPt;
//		private Coordinate ptScaled;
		
		private Coordinate p0Scaled;
		private Coordinate p1Scaled;
		
		private double scaleFactor;
		
		private double minx;
		private double maxx;
		private double miny;
		private double maxy;
		/// <summary> The corners of the hot pixel, in the order:
		/// 10
		/// 23
		/// </summary>
		private Coordinate[] corner = new Coordinate[4];
		
		private Envelope safeEnv = null;
        
        #endregion
		
        #region Constructors and Destructor

		public HotPixel(Coordinate pt, double scaleFactor, LineIntersector li)
		{
			originalPt = pt;
			this.pt = pt;
			this.scaleFactor = scaleFactor;
			this.li = li;
			//tolerance = 0.5;
			if (scaleFactor != 1.0)
			{
				this.pt  = new Coordinate(Scale(pt.X), Scale(pt.Y));
				p0Scaled = new Coordinate();
				p1Scaled = new Coordinate();
			}

			InitCorners(this.pt);
		}
        
        #endregion
		
        #region Public Properties

		public Coordinate Coordinate
		{
			get
			{
				return originalPt;
			}
		}
			
		/// <summary> 
		/// Gets a "safe" envelope that is guaranteed to contain the 
		/// hot pixel.
		/// </summary>
		/// <returns>
		/// </returns>
		public Envelope SafeEnvelope
		{
			get
			{
				if (safeEnv == null)
				{
					double safeTolerance = .75 / scaleFactor;

					safeEnv = new Envelope(originalPt.X - safeTolerance, 
                        originalPt.X + safeTolerance, 
                        originalPt.Y - safeTolerance, 
                        originalPt.Y + safeTolerance);
				}
				return safeEnv;
			}
		}
        
        #endregion
			
        #region Public Methods

		public bool Intersects(Coordinate p0, Coordinate p1)
		{
			if (scaleFactor == 1.0)
				return IntersectsScaled(p0, p1);
			
			CopyScaled(p0, p0Scaled);
			CopyScaled(p1, p1Scaled);
			
            return IntersectsScaled(p0Scaled, p1Scaled);
		}
		
        public bool IntersectsScaled(Coordinate p0, Coordinate p1)
        {
            double segMinx = Math.Min(p0.X, p1.X);
            double segMaxx = Math.Max(p0.X, p1.X);
            double segMiny = Math.Min(p0.Y, p1.Y);
            double segMaxy = Math.Max(p0.Y, p1.Y);
			
            bool isOutsidePixelEnv = maxx < segMinx || 
                minx > segMaxx || maxy < segMiny || miny > segMaxy;

            if (isOutsidePixelEnv)
                return false;
			
            bool intersects = IntersectsToleranceSquare(p0, p1);
			
            Debug.Assert(!(isOutsidePixelEnv && intersects), "Found bad envelope test");
			
            return intersects;
        }
        
        #endregion
		
        #region Private Methods

        private void InitCorners(Coordinate pt)
        {
            double tolerance = 0.5;
            minx = pt.X - tolerance;
            maxx = pt.X + tolerance;
            miny = pt.Y - tolerance;
            maxy = pt.Y + tolerance;
			
            corner[0] = new Coordinate(maxx, maxy);
            corner[1] = new Coordinate(minx, maxy);
            corner[2] = new Coordinate(minx, miny);
            corner[3] = new Coordinate(maxx, miny);
        }
		
        private double Scale(double val)
        {
            return Math.Round(val * scaleFactor);
        }
		
		private void CopyScaled(Coordinate p, Coordinate pScaled)
		{
			pScaled.X = Scale(p.X);
			pScaled.Y = Scale(p.Y);
		}
		
		/// <summary> Tests whether the segment p0-p1 intersects the hot pixel tolerance square.
		/// Because the tolerance square point set is partially open (along the
		/// top and right) the test needs to be more sophisticated than
		/// simply checking for any intersection.  However, it
		/// can take advantage of the fact that because the hot pixel edges
		/// do not lie on the coordinate grid.  It is sufficient to check
		/// if there is at least one of:
		/// <ul>
		/// <li>a proper intersection with the segment and any hot pixel edge
		/// <li>an intersection between the segment and both the left and bottom edges
		/// <li>an intersection between a segment endpoint and the hot pixel coordinate
		/// </ul>
		/// 
		/// </summary>
		/// <param name="p0">
		/// </param>
		/// <param name="p1">
		/// </param>
		/// <returns>
		/// </returns>
		private bool IntersectsToleranceSquare(Coordinate p0, Coordinate p1)
		{
			bool intersectsLeft   = false;
			bool intersectsBottom = false;
			
			li.ComputeIntersection(p0, p1, corner[0], corner[1]);
			if (li.Proper)
				return true;
			
			li.ComputeIntersection(p0, p1, corner[1], corner[2]);
			if (li.Proper)
				return true;
			if (li.HasIntersection)
				intersectsLeft = true;
			
			li.ComputeIntersection(p0, p1, corner[2], corner[3]);
			if (li.Proper)
				return true;
			if (li.HasIntersection)
				intersectsBottom = true;
			
			li.ComputeIntersection(p0, p1, corner[3], corner[0]);
			if (li.Proper)
				return true;
			
			if (intersectsLeft && intersectsBottom)
				return true;
			
			if (p0.Equals(pt))
				return true;

			if (p1.Equals(pt))
				return true;
			
			return false;
		}

		/// <summary> Test whether the given segment intersects
		/// the closure of this hot pixel.
		/// This is NOT the test used in the standard snap-rounding
		/// algorithm, which uses the partially closed tolerance square
		/// instead.
		/// This routine is provided for testing purposes only.
		/// 
		/// </summary>
		/// <param name="p0">the start point of a line segment
		/// </param>
		/// <param name="p1">the end point of a line segment
		/// </param>
		/// <returns> <see langword="true"/> if the segment intersects the closure of the pixel's tolerance square
		/// </returns>
		private bool IntersectsPixelClosure(Coordinate p0, Coordinate p1)
		{
			li.ComputeIntersection(p0, p1, corner[0], corner[1]);
			
            if (li.HasIntersection)
				return true;
			li.ComputeIntersection(p0, p1, corner[1], corner[2]);
			
            if (li.HasIntersection)
				return true;
			li.ComputeIntersection(p0, p1, corner[2], corner[3]);
			
            if (li.HasIntersection)
				return true;

			li.ComputeIntersection(p0, p1, corner[3], corner[0]);
			if (li.HasIntersection)
				return true;
			
			return false;
		}
        
        #endregion
    }
}