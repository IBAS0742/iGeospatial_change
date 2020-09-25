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
using iGeospatial.Geometries.Indexers.QuadTree;

namespace iGeospatial.Geometries.Operations.Valid
{
	/// <summary> Tests whether any of a set of {@link LinearRing}s are
	/// nested inside another ring in the set, using a {@link Quadtree}
	/// index to speed up the comparisons.
	/// </summary>
	internal class QuadtreeNestedRingTester
	{
        #region Private Fields

		private GeometryGraph graph; // used to find non-node vertices
		private GeometryList rings;
		private Envelope totalEnv;
		private Quadtree quadtree;
		private Coordinate nestedPt;
        
        #endregion
		
        #region Constructors and Destructor

        public QuadtreeNestedRingTester(GeometryGraph graph)
        {
            rings    = new GeometryList();
            totalEnv = new Envelope();

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
            BuildQuadtree();
				
            int nCount = rings.Count;

            for (int i = 0; i < nCount; i++)
            {
                LinearRing innerRing = (LinearRing) rings[i];
                ICoordinateList innerRingPts = innerRing.Coordinates;
					
                IList results = quadtree.Query(innerRing.Bounds);
                int nResultCount  = results.Count;
                for (int j = 0; j < nResultCount; j++)
                {
                    LinearRing searchRing = (LinearRing) results[j];
                    ICoordinateList searchRingPts = searchRing.Coordinates;
						
                    if (innerRing == searchRing)
                        continue;
						
                    if (!innerRing.Bounds.Intersects(searchRing.Bounds))
                        continue;
						
                    Coordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
                    Debug.Assert(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
						
                    bool IsInside = CGAlgorithms.IsPointInRing(innerRingPt, searchRingPts);
                    if (IsInside)
                    {
                        nestedPt = innerRingPt;
                        return false;
                    }
                }
            }

            return true;
		}

		public void Add(LinearRing ring)
		{
			rings.Add(ring);
			totalEnv.ExpandToInclude(ring.Bounds);
		}
        
        #endregion
		
        #region Private Methods

		private void BuildQuadtree()
		{
			quadtree = new Quadtree();
			
            int nCount = rings.Count;
			for (int i = 0; i < nCount; i++)
			{
				LinearRing ring = (LinearRing) rings[i];
				Envelope env = ring.Bounds;
				quadtree.Insert(env, ring);
			}
		}
        
        #endregion
	}
}