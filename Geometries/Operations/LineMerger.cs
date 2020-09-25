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
using iGeospatial.Geometries.PlanarGraphs;
using iGeospatial.Geometries.Visitors;

using iGeospatial.Geometries.Operations.LineMerge;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Sews together a set of fully noded <see cref="LineString"/>s. 
	/// </summary>
	/// <remarks>
	/// Sewing stops at nodes of degree 1 or 3 or more -- the exception 
	/// is an isolated loop, which only has degree-2 nodes, in which 
	/// case a node is simply chosen as a starting point. The direction of each
	/// merged <see cref="LineString"/> will be that of the majority of 
	/// the <see cref="LineString"/>s from which it was derived.
	/// <para>
	/// Any dimension of geometry is handled -- the constituent linework 
	/// is extracted to form the edges. 
	/// The edges must be correctly noded; that is, they must only meet
	/// at their endpoints.  The LineMerger will still run on incorrectly 
	/// noded input but will not form polygons from incorrected noded edges.
	/// </para>
	/// </remarks>
	public sealed class LineMerger
	{
        #region Private Fields

        private LineMergeGraph  graph;
        private ArrayList       mergedLineStrings;
        private GeometryFactory factory;
        private ArrayList       edgeStrings;
        
        #endregion
		
        #region Constructors and Destructor
		
        public LineMerger()
		{
            graph = new LineMergeGraph();
        }
        
        #endregion

        #region Public Properties

		/// <summary> 
		/// Returns the LineStrings built by the merging process.
		/// </summary>
		public ArrayList MergedLineStrings
		{
			get
			{
				Merge();

				return mergedLineStrings;
			}
		}
        
        #endregion

        #region Public Methods

		/// <summary> 
		/// Adds a collection of Geometries to be processed. May be called multiple times.
		/// Any dimension of Geometry may be added; the constituent linework will be
		/// extracted.
		/// </summary>
		public void Add(ArrayList geometries)
		{
            if (geometries != null)
            {
                int nCount = geometries.Count;

                for (int i = 0; i < nCount; i++)
                {
                    Geometry geometry = (Geometry)geometries[i];

                    Add(geometry);
                }
            }
		}

		/// <summary> 
		/// Adds a Geometry to be processed. May be called multiple times.
		/// Any dimension of Geometry may be added; the constituent linework will be
		/// extracted.
		/// </summary>
		public void Add(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            geometry.Apply(new LineMergerComponentVisitor(this));
		}
		
		public void Add(LineString lineString)
		{
			if (factory == null)
			{
				this.factory = lineString.Factory;
			}
			graph.AddEdge(lineString);
		}

		public void Merge()
		{
			if (mergedLineStrings != null)
			{
				return;
			}

			edgeStrings = new ArrayList();
			BuildEdgeStringsForObviousStartNodes();
			BuildEdgeStringsForIsolatedLoops();
			mergedLineStrings = new ArrayList();

			for (IEnumerator i = edgeStrings.GetEnumerator(); i.MoveNext(); )
			{
				EdgeString edgeString = (EdgeString) i.Current;
				mergedLineStrings.Add(edgeString.ToLineString());
			}
		}
        
        #endregion

        #region Private Methods

		private void BuildEdgeStringsForObviousStartNodes()
		{
			BuildEdgeStringsForNonDegree2Nodes();
		}
		
		private void BuildEdgeStringsForIsolatedLoops()
		{
			BuildEdgeStringsForUnprocessedNodes();
		}
		
		private void BuildEdgeStringsForUnprocessedNodes()
		{
			for (IEnumerator i = graph.Nodes.GetEnumerator(); i.MoveNext(); )
			{
				Node node = (Node) i.Current;
				if (!node.Marked)
				{
					Debug.Assert(node.Degree == 2);
					BuildEdgeStringsStartingAt(node);
					node.Marked = true;
				}
			}
		}
		
		private void BuildEdgeStringsForNonDegree2Nodes()
		{
			for (IEnumerator i = graph.Nodes.GetEnumerator(); i.MoveNext(); )
			{
				Node node = (Node) i.Current;
				if (node.Degree != 2)
				{
					BuildEdgeStringsStartingAt(node);
					node.Marked = true;
				}
			}
		}
		
		private void BuildEdgeStringsStartingAt(Node node)
		{
			for (IEnumerator i = node.OutEdges.Iterator(); i.MoveNext(); )
			{
				LineMergeDirectedEdge directedEdge = (LineMergeDirectedEdge) i.Current;
				if (directedEdge.Edge.Marked)
				{
					continue;
				}
				edgeStrings.Add(BuildEdgeStringStartingWith(directedEdge));
			}
		}
		
		private EdgeString BuildEdgeStringStartingWith(LineMergeDirectedEdge start)
		{
			EdgeString edgeString = new EdgeString(factory);
			LineMergeDirectedEdge current = start;
			do 
			{
				edgeString.Add(current);
				current.Edge.Marked = true;
				current = current.Next;
			}
			while (current != null && current != start);

			return edgeString;
		}
        
        #endregion

        #region LineMergerComponentVisitor Class

        private sealed class LineMergerComponentVisitor : IGeometryComponentVisitor
        {
            private LineMerger m_objLineMerger;
			
            public LineMergerComponentVisitor(LineMerger lineMerger)
            {
                if (lineMerger == null)
                {
                    throw new ArgumentNullException("lineMerger");
                }

                this.m_objLineMerger = lineMerger;
            }

            public LineMerger LineMerger
            {
                get
                {
                    return m_objLineMerger;
                }
            }

            public void Visit(Geometry component)
            {
                if (component == null)
                {
                    throw new ArgumentNullException("component");
                }

                GeometryType geomType = component.GeometryType;

                if (geomType == GeometryType.LineString||
                    geomType == GeometryType.LinearRing)
                {
                    m_objLineMerger.Add((LineString)component);
                }
            }
        }
        
        #endregion
	}
}