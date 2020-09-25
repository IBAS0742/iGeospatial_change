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
using iGeospatial.Geometries.Graphs.Index;
using iGeospatial.Geometries.Operations.Overlay;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Computes the overlay of two <see cref="Geometry"/> instances.  
	/// </summary>
	/// <remarks>
	/// The overlay can be used to determine any boolean combination 
	/// of the geometries.
	/// <para>
	/// The four geometry overlap operations of intersection, union,
	/// difference and symmetric difference are supported.
	/// </para>
	/// </remarks>
	/// <seealso cref="OverlayType"/>
	public sealed class OverlayOp : GraphGeometryOp
	{
        #region Private Fields

		/// <summary> The spatial functions supported by this class.
		/// These operations implement various boolean combinations of the resultants of the overlay.
		/// </summary>
        private PointLocator ptLocator;
        private GeometryFactory geomFact;
        private Geometry resultGeom;
		
        private PlanarGraph graph;
        private EdgeList edgeList;
		
        private IGeometryList resultPolyList;
        private IGeometryList resultLineList;
        private IGeometryList resultPointList;
        
        #endregion
		
        #region Constructors and Destructor
		
        public OverlayOp(Geometry g0, Geometry g1) : base(g0, g1)
		{
            if (g0 == null)
            {
                throw new ArgumentNullException("g0");
            }
            if (g1 == null)
            {
                throw new ArgumentNullException("g1");
            }

            ptLocator       = new PointLocator();
            edgeList        = new EdgeList();
            resultPolyList  = new GeometryList();
            resultLineList  = new GeometryList();
            resultPointList = new GeometryList();

			graph = new PlanarGraph(new OverlayNodeFactory());
			// Use factory of primary geometry.
			// Note that this does NOT handle mixed-precision arguments
			// where the second arg has greater precision than the first.
			geomFact = g0.Factory;
		}
        
        #endregion
		
        #region Public Properties

		internal PlanarGraph Graph
		{
			get
			{
				return graph;
			}
		}
        
        #endregion

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom0"></param>
        /// <param name="geom1"></param>
        /// <param name="opCode"></param>
        /// <returns></returns>
		public static Geometry Overlay(Geometry geom0, Geometry geom1, 
            OverlayType opCode)
		{
			OverlayOp gov   = new OverlayOp(geom0, geom1);
			Geometry geomOv = gov.Overlay(opCode);

			return geomOv;
		}
		
        /// <summary>
        /// 
        /// </summary>
        /// <param name="funcCode"></param>
        /// <returns></returns>
        public Geometry Overlay(OverlayType funcCode)
        {
            ComputeOverlay(funcCode);

            return resultGeom;
        }
        
        #endregion
		
        #region Private Methods

		internal static bool IsResultOfOp(Label label, OverlayType opCode)
		{
			int loc0 = label.GetLocation(0);
			int loc1 = label.GetLocation(1);
			return IsResultOfOp(loc0, loc1, opCode);
		}
		
		/// <summary> 
		/// This method will handle arguments of 
		/// LocationType.None correctly.
		/// </summary>
		/// <returns> true if the locations correspond to the opCode
		/// </returns>
		internal static bool IsResultOfOp(int loc0, int loc1, OverlayType opCode)
		{
			if (loc0 == LocationType.Boundary)
				loc0 = LocationType.Interior;
			if (loc1 == LocationType.Boundary)
				loc1 = LocationType.Interior;

			switch (opCode)
			{
				case OverlayType.Intersection: 
					return loc0 == LocationType.Interior && 
                        loc1 == LocationType.Interior;
				
				case OverlayType.Union: 
					return loc0 == LocationType.Interior || 
                        loc1 == LocationType.Interior;
				
				case OverlayType.Difference: 
					return loc0 == LocationType.Interior && 
                        loc1 != LocationType.Interior;
				
				case OverlayType.SymmetricDifference: 
					return (loc0 == LocationType.Interior && loc1 != LocationType.Interior) || 
                        (loc0 != LocationType.Interior && loc1 == LocationType.Interior);
			}

			return false;
		}
		
		private void ComputeOverlay(OverlayType opCode)
		{
			// copy points from input Geometries.
			// This ensures that any Point geometries
			// in the input are considered for inclusion in the result set
			CopyPoints(0);
			CopyPoints(1);
			
			// node the input Geometries
			arg[0].ComputeSelfNodes(li, false);
			arg[1].ComputeSelfNodes(li, false);
			
			// compute intersections between edges of the two input geometries
			arg[0].ComputeEdgeIntersections(arg[1], li, true);
			
			EdgeCollection baseSplitEdges = new EdgeCollection();
			arg[0].ComputeSplitEdges(baseSplitEdges);
			arg[1].ComputeSplitEdges(baseSplitEdges);
//			EdgeCollection splitEdges = baseSplitEdges;
			// Add the noded edges to this result graph
			InsertUniqueEdges(baseSplitEdges);
			
			ComputeLabelsFromDepths();
			ReplaceCollapsedEdges();
			
			graph.AddEdges(edgeList.Edges);
			ComputeLabelling();

            LabelIncompleteNodes();
			
			// The ordering of building the result Geometries is important.
			// Areas must be built before lines, which must be built before points.
			// This is so that lines which are covered by areas are not included
			// explicitly, and similarly for points.
			FindResultAreaEdges(opCode);
			CancelDuplicateResultEdges();
			PolygonBuilder polyBuilder = new PolygonBuilder(geomFact);
			polyBuilder.Add(graph);
			resultPolyList = polyBuilder.Build();
			
			LineBuilder lineBuilder = new LineBuilder(this, geomFact, ptLocator);
			resultLineList = lineBuilder.Build(opCode);
			
			PointBuilder pointBuilder = new PointBuilder(this, geomFact);
			resultPointList = pointBuilder.Build(opCode);
			
			// gather the results from all calculations into a single Geometry for the result set
			resultGeom = ComputeGeometry(resultPointList, resultLineList, resultPolyList);
		}
		
		private void InsertUniqueEdges(EdgeCollection edges)
		{
			for (IEdgeEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
			{
				Edge e = i.Current;
				InsertUniqueEdge(e);
			}
		}
		/// <summary> 
		/// Insert an edge from one of the noded input graphs.
		/// Checks edges that are inserted to see if an
		/// identical edge already exists.
		/// If so, the edge is not inserted, but its label is merged
		/// with the existing edge.
		/// </summary>
		private void InsertUniqueEdge(Edge e)
		{
            //<FIX> MD 8 Oct 03  speed up identical edge lookup
            // fast lookup
            Edge existingEdge = edgeList.FindEqualEdge(e);
			
			// If an identical edge already exists, simply update its label
			if (existingEdge != null)
			{
				Label existingLabel = existingEdge.Label;
				
				Label labelToMerge = e.Label;
				// check if new edge is in Reverse direction to existing edge
				// if so, must flip the label before merging it
				if (!existingEdge.IsPointwiseEqual(e))
				{
					labelToMerge = new Label(e.Label);
					labelToMerge.Flip();
				}
				Depth depth = existingEdge.Depth;
				// if this is the first duplicate found for this edge, initialize the depths
				///*
				if (depth.IsNull())
				{
					depth.Add(existingLabel);
				}
				//*/
				depth.Add(labelToMerge);
				existingLabel.Merge(labelToMerge);
			}
			else
			{
				// no matching existing edge was found
				// Add this new edge to the list of edges in this graph
				//e.setName(name + edges.Size());
				//e.GetDepth().Add(e.getLabel());
				edgeList.Add(e);
			}
		}
		
		/// <summary> 
		/// Update the labels for edges according to their depths.
		/// For each edge, the depths are first normalized.
		/// Then, if the depths for the edge are equal,
		/// this edge must have collapsed into a line edge.
		/// If the depths are not equal, update the label
		/// with the locations corresponding to the depths
		/// (i.e. a depth of 0 corresponds to a Location of EXTERIOR,
		/// a depth of 1 corresponds to INTERIOR)
		/// </summary>
		private void ComputeLabelsFromDepths()
		{
			for (IEdgeEnumerator it = edgeList.Iterator(); it.MoveNext(); )
			{
				Edge e    = it.Current;
				Label lbl = e.Label;
				Depth depth = e.Depth;
				// Only check edges for which there were duplicates,
				// since these are the only ones which might
				// be the result of dimensional collapses.
				if (!depth.IsNull())
				{
					depth.Normalize();
					for (int i = 0; i < 2; i++)
					{
						if (!lbl.IsNull(i) && lbl.IsArea() && !depth.IsNull(i))
						{
							// if the depths are equal, this edge is the result of
							// the dimensional collapse of two or more edges.
							// It has the same location on both sides of the edge,
							// so it has collapsed to a line.
							if (depth.GetDelta(i) == 0)
							{
								lbl.ToLine(i);
							}
							else
							{
								// This edge may be the result of a dimensional collapse,
								// but it still has different locations on both sides.  The
								// label of the edge must be updated to reflect the resultant
								// side locations indicated by the depth values.
								Debug.Assert(!depth.IsNull(i, Position.Left), "depth of LEFT side has not been initialized");
								lbl.SetLocation(i, Position.Left, depth.GetLocation(i, Position.Left));
								Debug.Assert(!depth.IsNull(i, Position.Right), "depth of RIGHT side has not been initialized");
								lbl.SetLocation(i, Position.Right, depth.GetLocation(i, Position.Right));
							}
						}
					}
				}
			}
		}

		/// <summary> 
		/// If edges which have undergone dimensional collapse are found,
		/// replace them with a new edge which is a L edge
		/// </summary>
		private void ReplaceCollapsedEdges()
		{
            EdgeCollection replacableEdges = new EdgeCollection();

			for (IEdgeEnumerator it = edgeList.Iterator(); it.MoveNext();)
			{
				Edge e = it.Current;
				if (e.IsCollapsed)
				{
                    replacableEdges.Add(e);
				}
			}

            int nCount = replacableEdges.Count;
            for (int i = 0; i < nCount; i++)
            {
                Edge e = replacableEdges[i];
                edgeList.Replace(e, e.CollapsedEdge);
            }
		}

		/// <summary> 
		/// Copy all nodes from an arg geometry into this graph.
		/// The node label in the arg geometry overrides any previously computed
		/// label for that argIndex.
		/// (E.g. a node may be an intersection node with
		/// a previously computed label of BOUNDARY,
		/// but in the original arg Geometry it is actually
		/// in the interior due to the Boundary Determination Rule)
		/// </summary>
		private void CopyPoints(int argIndex)
		{
			for (IEnumerator i = arg[argIndex].NodeIterator; i.MoveNext(); )
			{
				Node graphNode = (Node) i.Current;
				Node newNode = graph.AddNode(graphNode.Coordinate);
				newNode.SetLabel(argIndex, graphNode.Label.GetLocation(argIndex));
			}
		}
		
		/// <summary> 
		/// Compute initial labelling for all DirectedEdges at each node.
		/// In this step, DirectedEdges will acquire a complete labelling
		/// (i.e. one with labels for both Geometries)
		/// only if they
		/// are incident on a node which has edges for both Geometries
		/// </summary>
		private void ComputeLabelling()
		{
			for (IEnumerator nodeit = graph.Nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;

                node.Edges.ComputeLabelling(arg);
			}

			MergeSymLabels();
			UpdateNodeLabelling();
		}

		/// <summary> 
		/// For nodes which have edges from only one Geometry incident on them,
		/// the previous step will have left their dirEdges with no labelling for the other
		/// Geometry.  However, the sym dirEdge may have a labelling for the other
		/// Geometry, so merge the two labels.
		/// </summary>
		private void MergeSymLabels()
		{
			for (IEnumerator nodeit = graph.Nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;
				((DirectedEdgeStar) node.Edges).MergeSymLabels();
			}
		}

		private void  UpdateNodeLabelling()
		{
			// update the labels for nodes
			// The label for a node is updated from the edges incident on it
			// (Note that a node may have already been labelled
			// because it is a point in one of the input geometries)
			for (IEnumerator nodeit = graph.Nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;
				Label lbl = ((DirectedEdgeStar) node.Edges).Label;
				node.Label.Merge(lbl);
			}
		}
		
		/// <summary> 
		/// Incomplete nodes are nodes whose labels are incomplete.
		/// (e.g. the location for one Geometry is null).
		/// These are either isolated nodes,
		/// or nodes which have edges from only a single Geometry incident on them.
		/// 
		/// Isolated nodes are found because nodes in one graph which don't intersect
		/// nodes in the other are not completely labelled by the initial process
		/// of adding nodes to the nodeList.
		/// To complete the labelling we need to check for nodes that lie in the
		/// interior of edges, and in the interior of areas.
		/// <para>
		/// When each node labelling is completed, the labelling of the incident
		/// edges is updated, to complete their labelling as well.
		/// </para>
		/// </summary>
		private void  LabelIncompleteNodes()
		{
			for (IEnumerator ni = graph.Nodes.GetEnumerator(); ni.MoveNext(); )
			{
				Node n = (Node) ni.Current;
				Label label = n.Label;
				if (n.Isolated)
				{
					if (label.IsNull(0))
						LabelIncompleteNode(n, 0);
					else
						LabelIncompleteNode(n, 1);
				}
				// now update the labelling for the DirectedEdges incident on this node
				((DirectedEdgeStar) n.Edges).UpdateLabelling(label);
			}
		}
		
		/// <summary> 
		/// Label an isolated node with its relationship to the target geometry.
		/// </summary>
		private void LabelIncompleteNode(Node n, int targetIndex)
		{
			int loc = ptLocator.Locate(n.Coordinate, arg[targetIndex].Geometry);
			n.Label.SetLocation(targetIndex, loc);
		}
		
		/// <summary> 
		/// Find all edges whose label indicates that they are in the result area(s),
		/// according to the operation being performed.  Since we want polygon shells to be
		/// oriented CW, choose dirEdges with the interior of the result on the RHS.
		/// Mark them as being in the result.
		/// Interior Area edges are the result of dimensional collapses.
		/// They do not form part of the result area boundary.
		/// </summary>
		private void FindResultAreaEdges(OverlayType opCode)
		{
			for (IEnumerator it = graph.EdgeEnds.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				// mark all dirEdges with the appropriate label
				Label label = de.Label;
				if (label.IsArea() && !de.InteriorAreaEdge && 
                    IsResultOfOp(label.GetLocation(0, Position.Right), 
                    label.GetLocation(1, Position.Right), opCode))
				{
					de.InResult = true;
				}
			}
		}

		/// <summary> 
		/// If both a dirEdge and its sym are marked as being in the 
		/// result, cancel them out.
		/// </summary>
		private void CancelDuplicateResultEdges()
		{
			// remove any dirEdges whose sym is also included
			// (they "cancel each other out")
			for (IEnumerator it = graph.EdgeEnds.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				DirectedEdge sym = de.Sym;
				if (de.InResult && sym.InResult)
				{
					de.InResult = false;
					sym.InResult = false;
				}
			}
		}

		/// <summary> 
		/// This method is used to decide if a point node should be included in 
		/// the result or not.
		/// 
		/// </summary>
		/// <returns> true if the coord point is covered by a result Line or Area geometry
		/// </returns>
		internal bool IsCoveredByLA(Coordinate coord)
		{
			if (IsCovered(coord, resultLineList))
				return true;
			if (IsCovered(coord, resultPolyList))
				return true;

			return false;
		}

		/// <summary> 
		/// This method is used to decide if an L edge should be included in 
		/// the result or not.
		/// </summary>
		/// <returns> true if the coord point is covered by a result Area geometry
		/// </returns>
		internal bool IsCoveredByA(Coordinate coord)
		{
			if (IsCovered(coord, resultPolyList))
				return true;

			return false;
		}

		/// <returns> true if the coord is located in the interior or boundary of
		/// a geometry in the list.
		/// </returns>
		private bool IsCovered(Coordinate coord, IGeometryList geomList)
		{
			for (IGeometryEnumerator it = geomList.GetEnumerator(); it.MoveNext(); )
			{
				Geometry geom = it.Current;
				int loc = ptLocator.Locate(coord, geom);
				if (loc != LocationType.Exterior)
					return true;
			}
			return false;
		}
		
		private Geometry ComputeGeometry(IGeometryList resultPointList, 
            IGeometryList resultLineList, IGeometryList resultPolyList)
		{
			GeometryList geomList = new GeometryList();
			// element geometries of the result are always in the order P,L,A
			geomList.AddRange(resultPointList);
			geomList.AddRange(resultLineList);
			geomList.AddRange(resultPolyList);

			// build the most specific geometry possible
			return geomFact.BuildGeometry(geomList);
		}
        
        #endregion
	}
}