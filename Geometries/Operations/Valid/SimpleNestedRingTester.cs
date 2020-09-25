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

namespace iGeospatial.Geometries.Operations.Valid
{
	/// <summary> 
	/// Tests whether any of a set of <see cref="LinearRing"/>s are
	/// nested inside another ring in the set, using a simple O(n^2)
	/// comparison.
	/// </summary>
	internal class SimpleNestedRingTester
	{
        #region Private Fields

		private GeometryGraph graph; // used to find non-node vertices
		private ArrayList rings;
		private Coordinate nestedPt;
        
        #endregion
		
        #region Constructors and Destructor

        public SimpleNestedRingTester(GeometryGraph graph)
        {
            rings = new ArrayList();

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
			for (int i = 0; i < rings.Count; i++)
			{
				LinearRing innerRing = (LinearRing) rings[i];
				ICoordinateList innerRingPts = innerRing.Coordinates;
				
				for (int j = 0; j < rings.Count; j++)
				{
					LinearRing searchRing = (LinearRing) rings[j];
					ICoordinateList searchRingPts = searchRing.Coordinates;
					
					if (innerRing == searchRing)
						continue;
					
					if (!innerRing.Bounds.Intersects(searchRing.Bounds))
						continue;
					
					Coordinate innerRingPt = IsValidOp.FindPointNotNode(innerRingPts, searchRing, graph);
					Debug.Assert(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
					//Coordinate innerRingPt = innerRingPts[0];
					
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
		}
        
        #endregion
	}
}