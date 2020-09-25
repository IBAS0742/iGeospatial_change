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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Operations.Overlay;

namespace iGeospatial.Geometries.Operations.Valid
{
	/// <summary> 
	/// This class tests that the interior of an area <see cref="Geometry"/>
	/// (<see cref="Polygon"/>  or <see cref="MultiPolygon"/>)
	/// is connected.  
	/// </summary>
	/// <remarks>
	/// An area Geometry is invalid if the interior is disconnected.
	/// This can happen if:
	/// <list type="number">
	/// <item>
	/// <description>
	/// one or more holes either form a chain touching the shell at two places
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// one or more holes form a ring around a portion of the interior
	/// </description>
	/// </item>
	/// </list>
	/// If an inconsistency if found the location of the problem is recorded.
	/// </remarks>
	internal class ConnectedInteriorTester
	{
        #region Private Fields

		private GeometryFactory geometryFactory;
		
		private GeometryGraph geomGraph;

		// save a coordinate for any disconnected interior found
		// the coordinate will be somewhere on the ring surrounding the 
        // disconnected interior
		private Coordinate disconnectedRingcoord;
        
        #endregion
		
        #region Constructors and Destructor

        public ConnectedInteriorTester(GeometryGraph geomGraph)
        {
            geometryFactory = GeometryFactory.GetInstance();

            this.geomGraph  = geomGraph;
        }

        #endregion
		
        #region Public Properties

		public Coordinate Coordinate
		{
			get
			{
				return disconnectedRingcoord;
			}
		}
        
        #endregion
		
        #region Public Methods

        public bool IsInteriorsConnected()
		{
			// node the edges, in case holes touch the shell
			EdgeCollection splitEdges = new EdgeCollection();
			geomGraph.ComputeSplitEdges(splitEdges);
			
			// form the edges into rings
			PlanarGraph graph = new PlanarGraph(new OverlayNodeFactory());
			graph.AddEdges(splitEdges);
			SetInteriorEdgesInResult(graph);
			graph.LinkResultDirectedEdges();

            ArrayList edgeRings = BuildEdgeRings(graph.EdgeEnds);
			
			// Mark all the edges for the edgeRings corresponding to the shells
			// of the input polygons.  Note only ONE ring gets marked for each shell.
			VisitShellInteriors(geomGraph.Geometry, graph);
			
			// If there are any unvisited shell edges
			// (i.e. a ring which is not a hole and which has the interior
			// of the parent area on the RHS)
			// this means that one or more holes must have split the interior of the
			// polygon into at least two pieces.  The polygon is thus invalid.
			return !HasUnvisitedShellEdge(edgeRings);
		}
		
        public static Coordinate FindDifferentPoint(ICoordinateList coord, 
            Coordinate pt)
        {
            int nCount = coord.Count;
            for (int i = 0; i < nCount; i++)
            {
                if (!coord[i].Equals(pt))
                    return coord[i];
            }

            return null;
        }
        
        #endregion

        #region Private Methods

		private void SetInteriorEdgesInResult(PlanarGraph graph)
		{
            for (IEnumerator it = graph.EdgeEnds.GetEnumerator(); 
                it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.Label.GetLocation(0, Position.Right) == 
                    LocationType.Interior)
                {
                    de.InResult = true;
                }
            }
		}
		
        /// <summary> 
        /// Form DirectedEdges in graph into Minimal EdgeRings.
        /// (Minimal Edgerings must be used, because only they are guaranteed to provide
        /// a correct isHole computation)
        /// </summary>
        private ArrayList BuildEdgeRings(ArrayList dirEdges)
		{
			ArrayList edgeRings = new ArrayList();

            for (IEnumerator it = dirEdges.GetEnumerator(); it.MoveNext(); )
			{
                DirectedEdge de = (DirectedEdge) it.Current;
				// if this edge has not yet been processed
                if (de.InResult && de.EdgeRing == null)
                {
                    MaximalEdgeRing er = new MaximalEdgeRing(de, 
                        geometryFactory);
					
                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    IList minEdgeRings = er.BuildMinimalRings();

                    int nRings = minEdgeRings.Count;
                    for (int i = 0; i < nRings; i++)
                    {
                        edgeRings.Add(minEdgeRings[i]);
                    }
                }
            }

			return edgeRings;
		}
		
        /// <summary> 
        /// Mark all the edges for the edgeRings corresponding to the shells
        /// of the input polygons.
        /// Only ONE ring gets marked for each shell - if there are others which remain unmarked
        /// this indicates a disconnected interior.
        /// </summary>
        private void VisitShellInteriors(Geometry g, PlanarGraph graph)
		{
            GeometryType geomType = g.GeometryType;

            if (geomType == GeometryType.Polygon)
			{
				Polygon p = (Polygon) g;
				VisitInteriorRing(p.ExteriorRing, graph);
			}

			if (geomType == GeometryType.MultiPolygon)
			{
				MultiPolygon mp = (MultiPolygon) g;
				for (int i = 0; i < mp.NumGeometries; i++)
				{
					Polygon p = (Polygon) mp.GetGeometry(i);
					VisitInteriorRing(p.ExteriorRing, graph);
				}
			}
		}
		
		private void VisitInteriorRing(LineString ring, PlanarGraph graph)
		{
			ICoordinateList pts = ring.Coordinates;
			Coordinate pt0   = pts[0];
			// Find first point in coord list different to initial point.
			// Need special check since the first point may be repeated.
			Coordinate pt1 = FindDifferentPoint(pts, pt0);
			Edge e = graph.FindEdgeInSameDirection(pt0, pt1);
			DirectedEdge de = (DirectedEdge) graph.FindEdgeEnd(e);
			DirectedEdge intDe = null;
			if (de.Label.GetLocation(0, Position.Right) == LocationType.Interior)
			{
				intDe = de;
			}
			else if (de.Sym.Label.GetLocation(0, Position.Right) == LocationType.Interior)
			{
				intDe = de.Sym;
			}
			Debug.Assert(intDe != null, "unable to find dirEdge with Interior on RHS");
			
			VisitLinkedDirectedEdges(intDe);
		}

		private void VisitLinkedDirectedEdges(DirectedEdge start)
		{
			DirectedEdge startDe = start;
			DirectedEdge de = start;
			do 
			{
				Debug.Assert(de != null, "found null Directed Edge");
				de.Visited = true;
				de = de.Next;
			}
			while (de != startDe);
		}
		
		/// <summary> Check if any shell ring has an unvisited edge.
		/// A shell ring is a ring which is not a hole and which has the interior
		/// of the parent area on the RHS.
		/// (Note that there may be non-hole rings with the interior on the LHS,
		/// since the interior of holes will also be polygonized into CW rings
		/// by the LinkAllDirectedEdges() step)
		/// </summary>
		/// <returns> true if there is an unvisited edge in a non-hole ring
		/// </returns>
		private bool HasUnvisitedShellEdge(ArrayList edgeRings)
		{
            int nRings = edgeRings.Count;
			for (int i = 0; i < nRings; i++)
			{
				EdgeRing er = (EdgeRing) edgeRings[i];
                // don't check hole rings
				if (er.IsHole)
					continue;
				ArrayList edges = er.Edges;
				DirectedEdge de = (DirectedEdge) edges[0];
				// don't check CW rings which are holes
				if (de.Label.GetLocation(0, 
                    Position.Right) != LocationType.Interior)
					continue;
				
                // the edgeRing is CW ring which surrounds the INT of the area, so check all
                // edges have been visited.  If any are unvisited, this is a disconnected part of the interior
                int nEdges = edges.Count;
				for (int j = 0; j < nEdges; j++)
				{
					de = (DirectedEdge) edges[j];
					if (!de.Visited)
					{
						disconnectedRingcoord = de.Coordinate;
						return true;
					}
				}
			}
			return false;
		}
        
        #endregion
    }
}