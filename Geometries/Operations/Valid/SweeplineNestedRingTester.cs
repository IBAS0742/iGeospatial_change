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
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Indexers.SweepLine;

namespace iGeospatial.Geometries.Operations.Valid
{
	/// <summary> 
	/// Tests whether any of a set of <see cref="LinearRing"/>s are
	/// nested inside another ring in the set, using a <see cref="SweepLineIndex"/>
	/// index to speed up the comparisons.
	/// </summary>
	internal class SweeplineNestedRingTester
	{
        #region Private Fields

		private GeometryGraph graph; // used to find non-node vertices
		private GeometryList rings;
		private SweepLineIndex sweepLine;
		private Coordinate nestedPt;
        
        #endregion
		
        #region Constructors and Destructor

        public SweeplineNestedRingTester(GeometryGraph graph)
        {
            rings    = new GeometryList();

            this.graph = graph;
        }

        #endregion
		
        #region Public Properties

		public Coordinate NestedPoint
		{
			get
			{
				return nestedPt;
			}
		}
        
        #endregion

        #region Public Methods

		public bool IsNonNested()
		{
            BuildIndex();
				
            OverlapAction action = new OverlapAction(this);
				
            sweepLine.ComputeOverlaps(action);

            return action.isNonNested;
		}
		
		public void Add(LinearRing ring)
		{
			rings.Add(ring);
		}
        
        #endregion
		
        #region Private Methods

		private void BuildIndex()
		{
			sweepLine = new SweepLineIndex();
			
            int nCount = rings.Count;
			for (int i = 0; i < nCount; i++)
			{
				LinearRing ring = (LinearRing)rings[i];
				Envelope env    = ring.Bounds;

				SweepLineInterval sweepInt = new SweepLineInterval(env.MinX, env.MaxX, ring);
				sweepLine.Add(sweepInt);
			}
		}
		
		private bool IsInside(LinearRing innerRing, LinearRing searchRing)
		{
			ICoordinateList innerRingPts = innerRing.Coordinates;
			ICoordinateList searchRingPts = searchRing.Coordinates;
			
			if (!innerRing.Bounds.Intersects(searchRing.Bounds))
				return false;
			
			Coordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
			Debug.Assert(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
			
			bool IsInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
			if (IsInside)
			{
				nestedPt = innerRingPt;
				return true;
			}

			return false;
		}
        
        #endregion
		
        #region OverlapAction Class

		internal class OverlapAction : ISweepLineOverlapAction
		{
            private SweeplineNestedRingTester m_objRingTester;
            internal bool isNonNested = true;
			
			public OverlapAction(SweeplineNestedRingTester ringTester)
			{
                this.m_objRingTester = ringTester;
            }

            public SweeplineNestedRingTester RingTester
			{
				get
				{
					return m_objRingTester;
				}         				
			}
			
			public virtual void  Overlap(SweepLineInterval s0, SweepLineInterval s1)
			{
				LinearRing innerRing  = (LinearRing) s0.Item;
				LinearRing searchRing = (LinearRing) s1.Item;
				if (innerRing == searchRing)
					return;
				
				if (m_objRingTester.IsInside(innerRing, searchRing))
					isNonNested = false;
			}
		}
        
        #endregion
	}
}