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
using iGeospatial.Geometries.Indexers.StrTree;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> Implements <see cref="IPointInRing"/> using a <see cref="SIRTree"/> 
	/// index to increase performance.
	/// </summary>
	public class SIRtreePointInRing : IPointInRing
	{
		private LinearRing ring;
		private SIRTree sirTree;
		private int crossings; // number of segment/ray crossings
		
		public SIRtreePointInRing(LinearRing ring)
		{
			this.ring = ring;
			BuildIndex();
		}
		
		private void  BuildIndex()
		{
//			Envelope env = ring.Bounds;
			sirTree = new SIRTree();
			
			ICoordinateList pts = ring.Coordinates;
            int nCount          = pts.Count;
			for (int i = 1; i < nCount; i++)
			{
				if (pts[i - 1].Equals(pts[i]))
				{
					continue;
				} //Optimization suggested by MD. [Jon Aquino]
				LineSegment seg = new LineSegment(ring.Factory, pts[i - 1], pts[i]);
				sirTree.Insert(seg.p0.Y, seg.p1.Y, seg);
			}
		}
		
		public virtual bool IsInside(Coordinate pt)
		{
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            crossings = 0;
			
			// test all segments intersected by vertical ray at pt
			
			ArrayList segs = sirTree.Query(pt.Y);
			
			for (IEnumerator i = segs.GetEnumerator(); i.MoveNext(); )
			{
				LineSegment seg = (LineSegment) i.Current;
				TestLineSegment(pt, seg);
			}
			
			//  p is inside if number of crossings is odd.
			if ((crossings % 2) == 1)
			{
				return true;
			}
			return false;
		}
		
		private void  TestLineSegment(Coordinate p, LineSegment seg)
		{
			double xInt; // x intersection of segment with ray
			double x1;   // translated coordinates
			double y1;
			double x2;
			double y2;
			
			// Test if segment Crosses ray from test point in positive x direction.
			Coordinate p1 = seg.p0;
			Coordinate p2 = seg.p1;
			x1 = p1.X - p.X;
			y1 = p1.Y - p.Y;
			x2 = p2.X - p.X;
			y2 = p2.Y - p.Y;
			
			if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
			{
				//  segment straddles x axis, so compute intersection.
				xInt = RobustDeterminant.SignOfDeterminant(x1, y1, x2, y2) / (y2 - y1);
				//xsave = xInt;

				//  Crosses ray if strictly positive intersection.
				if (0.0 < xInt)
				{
					crossings++;
				}
			}
		}
	}
}