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
using iGeospatial.Geometries.Indexers.Chain;
using iGeospatial.Geometries.Indexers.StrTree;
using iGeospatial.Geometries.Indexers.BinTree;
using Interval = iGeospatial.Geometries.Indexers.BinTree.Interval;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// Implements IPointInRing using MonotoneChain and a BinTree index 
	/// to increase performance.
	/// </summary>
	[Serializable]
    public class MonotoneChainPointInRing : IPointInRing
	{
        #region Private Fields

		private LinearRing ring;
		private Bintree tree;
		private int crossings; // number of segment/ray crossings
		
        private Interval interval;
        
        #endregion
		
        #region Constructors and Destructor

		public MonotoneChainPointInRing(LinearRing ring)
		{
            interval = new Interval();
            this.ring = ring;

			BuildIndex();
		}
        
        #endregion
		
        #region Public Methods

		public bool IsInside(Coordinate pt)
		{
            if (pt == null)
            {
                throw new ArgumentNullException("pt");
            }

            crossings = 0;
			
			// test all segments intersected by ray from pt in positive x direction
			Envelope rayEnv = new Envelope(Double.NegativeInfinity, 
                Double.PositiveInfinity, pt.Y, pt.Y);
			
			interval.Min = pt.Y;
			interval.Max = pt.Y;

            ArrayList segs = tree.Query(interval);
			
			MCSelecter mcSelecter = new MCSelecter(this, pt);
			for (IEnumerator i = segs.GetEnumerator(); i.MoveNext();)
			{
				MonotoneChain mc = (MonotoneChain) i.Current;
				TestMonotoneChain(rayEnv, mcSelecter, mc);
			}
			
			/*
			*  p is inside if number of crossings is odd.
			*/
			if ((crossings % 2) == 1)
			{
				return true;
			}
			return false;
		}
        
        #endregion
		
        #region Private Methods
		
        private void BuildIndex()
        {
            tree = new Bintree();
			
            ICoordinateList pts = CoordinateCollection.RemoveRepeatedCoordinates(ring.Coordinates);

            IList mcList = MonotoneChainBuilder.GetChains(pts);
			
            for (int i = 0; i < mcList.Count; i++)
            {
                MonotoneChain mc = (MonotoneChain) mcList[i];
                Envelope mcEnv = mc.Envelope;
                interval.Min = mcEnv.MinY;
                interval.Max = mcEnv.MaxY;
                tree.Insert(interval, mc);
            }
        }
		
		private void  TestMonotoneChain(Envelope rayEnv, MCSelecter mcSelecter, MonotoneChain mc)
		{
			mc.Select(rayEnv, mcSelecter);
		}
		
		private void  TestLineSegment(Coordinate p, LineSegment seg)
		{
			double xInt; // x intersection of segment with ray
			double x1; // translated coordinates
			double y1;
			double x2;
			double y2;
			
			/*
			*  Test if segment Crosses ray from test point in positive x direction.
			*/
			Coordinate p1 = seg.p0;
			Coordinate p2 = seg.p1;
			x1 = p1.X - p.X;
			y1 = p1.Y - p.Y;
			x2 = p2.X - p.X;
			y2 = p2.Y - p.Y;
			
			if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
			{
				/*
				*  segment straddles x axis, so compute intersection.
				*/
				xInt = RobustDeterminant.SignOfDeterminant(x1, y1, x2, y2) / (y2 - y1);
				//xsave = xInt;
				/*
				*  Crosses ray if strictly positive intersection.
				*/
				if (0.0 < xInt)
				{
					crossings++;
				}
			}
		}
        
        #endregion

        #region MCSelecter Class

        [Serializable]
        internal class MCSelecter : MonotoneChainSelectAction
        {
            internal Coordinate p;
			
            private MonotoneChainPointInRing m_objPointInRing;
			
            public MonotoneChainPointInRing PointInRing
            {
                get
                {
                    return m_objPointInRing;
                }
				
            }

            public MCSelecter(MonotoneChainPointInRing pointInRing, Coordinate p)
            {
                this.m_objPointInRing = pointInRing;

                this.p = p;
            }
			
            public override void Select(LineSegment ls)
            {
                m_objPointInRing.TestLineSegment(p, ls);
            }
        }
        
        #endregion
	}
}