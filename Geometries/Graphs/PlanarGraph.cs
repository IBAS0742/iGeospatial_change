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
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// The computation of the <see cref="IntersectionMatrix"/> relies on the use 
	/// of a structure called a "topology graph".  
	/// </summary>
	/// <remarks>
	/// The topology graph contains nodes and edges corresponding to the nodes 
	/// and line segments of a <see cref="Geometry"/>. Each node and edge in the 
	/// graph is labeled with its topological location relative to the source geometry.
	/// <para>
	/// Note that there is no requirement that points of self-intersection be a vertex.
	/// Thus to obtain a correct topology graph, <see cref="Geometry"/> instances must be
	/// self-noded before constructing their graphs.
	/// </para>
	/// Two fundamental operations are supported by topology graphs:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// Computing the intersections between all the edges and nodes of a single graph
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Computing the intersections between the edges and nodes of two different graphs
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
    [Serializable]
    internal class PlanarGraph
	{
        #region Private Fields

        internal EdgeCollection edges;
        internal NodeMap        m_objNodes;
        internal ArrayList      edgeEndList;
        
        #endregion
		
        #region Constructors and Destructor

        public PlanarGraph(NodeFactory nodeFact)
        {
            edges       = new EdgeCollection();
            edgeEndList = new ArrayList();
            m_objNodes  = new NodeMap(nodeFact);
        }
		
        public PlanarGraph()
        {
            edges       = new EdgeCollection();
            edgeEndList = new ArrayList();
            m_objNodes  = new NodeMap(new NodeFactory());
        }

        #endregion
		
        #region Public Properties

		public IEdgeEnumerator EdgeIterator
		{
			get
			{
				return edges.GetEnumerator();
			}
		}

		public ArrayList EdgeEnds
		{
			get
			{
				return edgeEndList;
			}
		}

		public IEnumerator NodeIterator
		{
			get
			{
				return m_objNodes.Iterator();
			}
		}

		public ICollection Nodes
		{
			get
			{
				return m_objNodes.Values();
			}
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// For nodes in the Collection, link the DirectedEdges at the node that are 
		/// in the result.
		/// This allows clients to link only a subset of nodes in the graph, for
		/// efficiency (because they know that only a subset is of interest).
		/// </summary>
		public static void LinkResultDirectedEdges(NodeCollection nodes)
		{
			for (INodeEnumerator nodeit = nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node node = nodeit.Current;
				((DirectedEdgeStar) node.Edges).LinkResultDirectedEdges();
			}
		}
		
		public static void LinkResultDirectedEdges(ICollection nodes)
		{
			for (IEnumerator nodeit = nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;
				((DirectedEdgeStar) node.Edges).LinkResultDirectedEdges();
			}
		}
		
		public bool IsBoundaryNode(int geomIndex, Coordinate coord)
		{
			Node node = m_objNodes.Find(coord);
			if (node == null)
				return false;

			Label label = node.Label;
			if (label != null && label.GetLocation(geomIndex) == LocationType.Boundary)
				return true;

			return false;
		}

		public void Add(EdgeEnd e)
		{
			m_objNodes.Add(e);

            edgeEndList.Add(e);
		}

		public Node AddNode(Node node)
		{
			return m_objNodes.AddNode(node);
		}

		public Node AddNode(Coordinate coord)
		{
			return m_objNodes.AddNode(coord);
		}

		/// <returns> the node if found; null otherwise
		/// </returns>
		public Node Find(Coordinate coord)
		{
			return m_objNodes.Find(coord);
		}
		
		/// <summary> 
		/// Add a set of edges to the graph.  For each edge two DirectedEdges
		/// will be created.  DirectedEdges are NOT linked by this method.
		/// </summary>
		public void AddEdges(EdgeCollection edgesToAdd)
		{
			// create all the nodes for the edges
			for (IEdgeEnumerator it = edgesToAdd.GetEnumerator(); 
                it.MoveNext(); )
			{
				Edge e = it.Current;

                edges.Add(e);
				
				DirectedEdge de1 = new DirectedEdge(e, true);
				DirectedEdge de2 = new DirectedEdge(e, false);
				de1.Sym = de2;
				de2.Sym = de1;
				
				Add(de1);
				Add(de2);
			}
		}
		
		/// <summary> Link the DirectedEdges at the nodes of the graph.
		/// This allows clients to link only a subset of nodes in the graph, for
		/// efficiency (because they know that only a subset is of interest).
		/// </summary>
		public void LinkResultDirectedEdges()
		{
			for (IEnumerator nodeit = m_objNodes.Iterator(); 
                nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;
				((DirectedEdgeStar) node.Edges).LinkResultDirectedEdges();
			}
		}

		/// <summary> Link the DirectedEdges at the nodes of the graph.
		/// This allows clients to link only a subset of nodes in the graph, for
		/// efficiency (because they know that only a subset is of interest).
		/// </summary>
		public void LinkAllDirectedEdges()
		{
			for (IEnumerator nodeit = m_objNodes.Iterator(); 
                nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;
				((DirectedEdgeStar) node.Edges).LinkAllDirectedEdges();
			}
		}

		/// <summary> 
		/// Returns the EdgeEnd which has edge e as its base edge
		/// (MD 18 Feb 2002 - this should return a pair of edges)
		/// </summary>
		/// <returns> the edge, if found
		/// null if the edge was not found
		/// </returns>
		public EdgeEnd FindEdgeEnd(Edge e)
		{
			for (IEnumerator i = EdgeEnds.GetEnumerator(); i.MoveNext(); )
			{
				EdgeEnd ee = (EdgeEnd) i.Current;
				if (ee.Edge == e)
					return ee;
			}

			return null;
		}
		
		/// <summary> 
		/// Returns the edge whose first two coordinates are p0 and p1
		/// </summary>
		/// <returns> the edge, if found
		/// null if the edge was not found
		/// </returns>
		public Edge FindEdge(Coordinate p0, Coordinate p1)
		{
			for (int i = 0; i < edges.Count; i++)
			{
				Edge e = edges[i];
				ICoordinateList eCoord = e.Coordinates;
				if (p0.Equals(eCoord[0]) && p1.Equals(eCoord[1]))
					return e;
			}
			return null;
		}

		/// <summary> 
		/// Returns the edge which starts at p0 and whose first segment is parallel to p1.
		/// </summary>
		/// <returns> the edge, if found
		/// null if the edge was not found
		/// </returns>
		public Edge FindEdgeInSameDirection(Coordinate p0, Coordinate p1)
		{
			for (int i = 0; i < edges.Count; i++)
			{
				Edge e = edges[i];
				
				ICoordinateList eCoord = e.Coordinates;
				if (MatchInSameDirection(p0, p1, eCoord[0], eCoord[1]))
					return e;
				
				if (MatchInSameDirection(p0, p1, 
                    eCoord[eCoord.Count - 1], eCoord[eCoord.Count - 2]))
					return e;
			}

			return null;
		}
        
        #endregion

        #region Protected Methods

        protected void InsertEdge(Edge e)
        {
            edges.Add(e);
        }
        
        #endregion
		
        #region Private Methods

		/// <summary> 
		/// The coordinate pairs match if they define line segments lying in 
		/// the same direction.
		/// E.g. the segments are parallel and in the same quadrant
		/// (as opposed to parallel and opposite!).
		/// </summary>
		private bool MatchInSameDirection(Coordinate p0, Coordinate p1, 
            Coordinate ep0, Coordinate ep1)
		{
			if (!p0.Equals(ep0))
				return false;
			
			if (CGAlgorithms.ComputeOrientation(p0, p1, ep1) == 
                OrientationType.Collinear && 
                Quadrant.GetQuadrant(p0, p1) == 
                Quadrant.GetQuadrant(ep0, ep1))
            {
                return true;
            }

			return false;
		}
        
        #endregion
	}
}