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

using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Graphs.Index;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Overlay
{
	/// <summary> Nodes a set of edges.
	/// Takes one or more sets of edges and constructs a
	/// new set of edges consisting of all the split edges created by
	/// noding the input edges together
	/// </summary>
	internal class EdgeSetNoder
	{
        #region Private Fields

        private LineIntersector li;
        private EdgeCollection inputEdges;
        
        #endregion
		
        #region Constructors and Destructor

        public EdgeSetNoder(LineIntersector li)
        {
            inputEdges = new EdgeCollection();

            this.li = li;
        }

        #endregion
		
        #region Public Properties

		public EdgeCollection NodedEdges
		{
			get
			{
				EdgeSetIntersector esi = new SimpleMCSweepLineIntersector();
				SegmentIntersector si = new SegmentIntersector(li, true, false);
				esi.ComputeIntersections(inputEdges, si, true);
				
				EdgeCollection splitEdges = new EdgeCollection();

                for (IEdgeEnumerator i = inputEdges.GetEnumerator(); i.MoveNext(); )
				{
					Edge e = i.Current;
					e.EdgeIntersectionList.AddSplitEdges(splitEdges);
				}

				return splitEdges;
			}          			
		}
        
        #endregion
		
        #region Public Methods

		public void AddEdges(EdgeCollection edges)
		{
			inputEdges.AddRange(edges);
		}
        
        #endregion
	}
}