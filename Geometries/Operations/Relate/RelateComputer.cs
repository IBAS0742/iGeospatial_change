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
using iGeospatial.Geometries.Graphs.Index;

namespace iGeospatial.Geometries.Operations.Relate
{
	/// <summary> 
	/// Computes the topological relationship between two geometries.
	/// </summary>
	/// <remarks>
	/// RelateComputer does not need to build a complete graph structure to compute
	/// the IntersectionMatrix.  The relationship between the geometries can
	/// be computed by simply examining the labelling of edges incident on each node.
	/// <para>
	/// RelateComputer does not currently support arbitrary GeometryCollections.
	/// This is because GeometryCollections can contain overlapping Polygons.
	/// In order to correct compute relate on overlapping Polygons, they
	/// would first need to be noded and merged (if not explicitly, at least
	/// implicitly).
	/// </para>
	/// </remarks>
	internal class RelateComputer
	{
        #region Private Fields

		private LineIntersector li;
		private PointLocator ptLocator;
		private GeometryGraph[] arg; // the arg(s) of the operation
		private NodeMap nodes;
		private EdgeCollection isolatedEdges;
        
        #endregion
		  
        #region Constructors and Destructor

		public RelateComputer(GeometryGraph[] arg)
		{
            li            = new RobustLineIntersector();
            ptLocator     = new PointLocator();
            nodes         = new NodeMap(new RelateNodeFactory());
            isolatedEdges = new EdgeCollection();

			this.arg      = arg;
		}

        #endregion
		
        #region Public Methods

		public IntersectionMatrix ComputeIM()
		{
			IntersectionMatrix im = new IntersectionMatrix();
			// since Geometries are finite and embedded in a 2-D space, the EE element must always be 2
			im.SetValue(LocationType.Exterior, LocationType.Exterior, 2);
			
			// if the Geometries don't overlap there is nothing to do
			if (!arg[0].Geometry.Bounds.Intersects(arg[1].Geometry.Bounds))
			{
				ComputeDisjointIM(im);
				return im;
			}
			arg[0].ComputeSelfNodes(li, false);
			arg[1].ComputeSelfNodes(li, false);
			
			// compute intersections between edges of the two input geometries
			SegmentIntersector intersector = arg[0].ComputeEdgeIntersections(arg[1], li, false);
			//System.out.println("computeIM: # segment intersection tests: " + intersector.numTests);
			ComputeIntersectionNodes(0);
			ComputeIntersectionNodes(1);
			// Copy the labelling for the nodes in the parent Geometries.  These override
			// any labels determined by intersections between the geometries.
			CopyNodesAndLabels(0);
			CopyNodesAndLabels(1);
			
			// complete the labelling for any nodes which only have a label for a single geometry
			LabelIsolatedNodes();
			
			// If a proper intersection was found, we can set a lower bound on the IM.
			ComputeProperIntersectionIM(intersector, im);
			
			// Now process improper intersections
			// (eg where one or other of the geometries has a vertex at the intersection point)
			// We need to compute the edge graph at all nodes to determine the IM.
			
			// build EdgeEnds for all intersections
			EdgeEndBuilder eeBuilder = new EdgeEndBuilder();
			ArrayList ee0 = eeBuilder.ComputeEdgeEnds(arg[0].EdgeIterator);
			InsertEdgeEnds(ee0);
			ArrayList ee1 = eeBuilder.ComputeEdgeEnds(arg[1].EdgeIterator);
			InsertEdgeEnds(ee1);
			
			LabelNodeEdges();
			
			// Compute the labeling for isolated components
			// Isolated components are components that do not touch any 
            // other components in the graph.
			// They can be identified by the fact that they will
			// contain labels containing ONLY a single element, the one 
            // for their parent geometry.
			// We only need to check components contained in the input graphs, since
			// isolated components will not have been replaced by new components 
            // formed by intersections.
			LabelIsolatedEdges(0, 1);

            LabelIsolatedEdges(1, 0);
			
			// update the IM from all components
			UpdateIM(im);
			return im;
		}
        
        #endregion
		
        #region Private Methods

		private void InsertEdgeEnds(ArrayList ee)
		{
			for (IEnumerator i = ee.GetEnumerator(); i.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) i.Current;
				nodes.Add(e);
			}
		}
		
		private void ComputeProperIntersectionIM(SegmentIntersector intersector, 
            IntersectionMatrix im)
		{
			// If a proper intersection is found, we can set a lower bound on the IM.
			int dimA = (int)arg[0].Geometry.Dimension;
			int dimB = (int)arg[1].Geometry.Dimension;
			bool hasProper = intersector.HasProperIntersection();
			bool hasProperInterior = intersector.HasProperInteriorIntersection();
			
			// For Geometry's of dim 0 there can never be proper intersections.
			
			//If edge segments of Areas properly intersect, the areas 
            // must properly overlap.
			if (dimA == 2 && dimB == 2)
			{
				if (hasProper)
					im.SetAtLeast("212101212");
			}
			// If an line segment properly intersects an edge segment of an area,
			// it follows that the interior of the line intersects the Boundary of the area.
			// If the intersection is a proper interior intersection, then
			// there is an Interior-Interior intersection too.
			// Note that it does not follow that the Interior of the line 
            // Intersects the exterior of the area, since there may be another 
            // area component which Contains the rest of the line.
			else if (dimA == 2 && dimB == 1)
			{
				if (hasProper)
					im.SetAtLeast("FFF0FFFF2");
				if (hasProperInterior)
					im.SetAtLeast("1FFFFF1FF");
			}
			else if (dimA == 1 && dimB == 2)
			{
				if (hasProper)
					im.SetAtLeast("F0FFFFFF2");
				if (hasProperInterior)
					im.SetAtLeast("1F1FFFFFF");
			}
			// If edges of LineStrings properly intersect *in an interior point*, all
			// we can deduce is that
			// the interiors intersect.  (We can NOT deduce that the exteriors intersect,
			// since some other segments in the geometries might cover the points in the
			// neighbourhood of the intersection.)
			// It is important that the point be known to be an interior point of
			// both Geometries, since it is possible in a self-intersecting geometry to
			// have a proper intersection on one segment that is also a boundary 
            // point of another segment.
			else if (dimA == 1 && dimB == 1)
			{
				if (hasProperInterior)
					im.SetAtLeast("0FFFFFFFF");
			}
		}
		
		/// <summary> 
		/// Copy all nodes from an arg geometry into this graph.
		/// The node label in the arg geometry overrides any previously computed
		/// label for that argIndex.
		/// (E.g. a node may be an intersection node with a computed label of BOUNDARY,
		/// but in the original arg Geometry it is actually
		/// in the interior due to the Boundary Determination Rule)
		/// </summary>
		private void  CopyNodesAndLabels(int argIndex)
		{
			for (IEnumerator i = arg[argIndex].NodeIterator; i.MoveNext(); )
			{
				Node graphNode = (Node) i.Current;
				Node newNode = nodes.AddNode(graphNode.Coordinate);
				newNode.SetLabel(argIndex, graphNode.Label.GetLocation(argIndex));
			}
		}

		/// <summary> 
		/// Insert nodes for all intersections on the edges of a Geometry.
		/// Label the created nodes the same as the edge label if they do not 
		/// already have a label.
		/// This allows nodes created by either self-intersections or
		/// mutual intersections to be labelled.
		/// Endpoint nodes will already be labelled from when they were inserted.
		/// </summary>
		private void  ComputeIntersectionNodes(int argIndex)
		{
			for (IEdgeEnumerator i = arg[argIndex].EdgeIterator; i.MoveNext(); )
			{
				Edge e   = i.Current;
				int eLoc = e.Label.GetLocation(argIndex);

                for (IEnumerator eiIt = e.EdgeIntersectionList.Iterator(); eiIt.MoveNext(); )
				{
					EdgeIntersection ei = (EdgeIntersection) eiIt.Current;
					RelateNode n = (RelateNode) nodes.AddNode(ei.coord);
					if (eLoc == LocationType.Boundary)
						n.SetLabelBoundary(argIndex);
					else
					{
						if (n.Label.IsNull(argIndex))
							n.SetLabel(argIndex, LocationType.Interior);
					}
				}
			}
		}

		/// <summary> 
		/// If the Geometries are Disjoint, we need to enter their dimension and
		/// boundary dimension in the Ext rows in the IM
		/// </summary>
		private void ComputeDisjointIM(IntersectionMatrix im)
		{
			Geometry ga = arg[0].Geometry;
			if (!ga.IsEmpty)
			{
				im.SetValue(LocationType.Interior, LocationType.Exterior, 
                    (int)ga.Dimension);
				im.SetValue(LocationType.Boundary, LocationType.Exterior, 
                    (int)ga.BoundaryDimension);
			}

			Geometry gb = arg[1].Geometry;
			if (!gb.IsEmpty)
			{
				im.SetValue(LocationType.Exterior, LocationType.Interior, 
                    (int)gb.Dimension);
				im.SetValue(LocationType.Exterior, LocationType.Boundary, 
                    (int)gb.BoundaryDimension);
			}
		}
		
		private void  LabelNodeEdges()
		{
			for (IEnumerator ni = nodes.Iterator(); ni.MoveNext(); )
			{
				RelateNode node = (RelateNode) ni.Current;
				node.Edges.ComputeLabelling(arg);
			}
		}
		
		/// <summary> 
		/// Update the IM with the sum of the IMs for each component.
		/// </summary>
		private void  UpdateIM(IntersectionMatrix im)
		{
			for (IEdgeEnumerator ei = isolatedEdges.GetEnumerator(); ei.MoveNext(); )
			{
				Edge e = ei.Current;
				e.UpdateIM(im);
			}

			for (IEnumerator ni = nodes.Iterator(); ni.MoveNext(); )
			{
				RelateNode node = (RelateNode) ni.Current;
				node.UpdateIM(im);

                node.updateIMFromEdges(im);
			}
		}
		
		/// <summary> 
		/// Processes isolated edges by computing their labelling and adding them
		/// to the isolated edges list.
		/// Isolated edges are guaranteed not to touch the boundary of the target 
		/// (since if they did, they would have caused an intersection to be 
		/// computed and hence would not be isolated)
		/// </summary>
		private void  LabelIsolatedEdges(int thisIndex, int targetIndex)
		{
			for (IEdgeEnumerator ei = arg[thisIndex].EdgeIterator; ei.MoveNext(); )
			{
				Edge e = ei.Current;
				if (e.Isolated)
				{
					LabelIsolatedEdge(e, targetIndex, arg[targetIndex].Geometry);
					isolatedEdges.Add(e);
				}
			}
		}
		/// <summary> 
		/// Label an isolated edge of a graph with its relationship to the target geometry.
		/// If the target has dim 2 or 1, the edge can either be in the interior 
		/// or the exterior.
		/// If the target has dim 0, the edge must be in the exterior
		/// </summary>
		private void  LabelIsolatedEdge(Edge e, int targetIndex, Geometry target)
		{
			// this won't work for GeometryCollections with both dim 2 and 1 geoms
			if (target.Dimension > 0)
			{
				// since edge is not in boundary, may not need the full generality of PointLocator?
				// Possibly should use ptInArea locator instead?  We probably know here
				// that the edge does not touch the bdy of the target Geometry
				int loc = ptLocator.Locate(e.Coordinate, target);
				e.Label.SetAllLocations(targetIndex, loc);
			}
			else
			{
				e.Label.SetAllLocations(targetIndex, LocationType.Exterior);
			}
		}
		
		/// <summary> Isolated nodes are nodes whose labels are incomplete
		/// (e.g. the location for one Geometry is null).
		/// This is the case because nodes in one graph which don't intersect
		/// nodes in the other are not completely labelled by the initial process
		/// of adding nodes to the nodeList.
		/// To complete the labelling we need to check for nodes that lie in the
		/// interior of edges, and in the interior of areas.
		/// </summary>
		private void  LabelIsolatedNodes()
		{
			for (IEnumerator ni = nodes.Iterator(); ni.MoveNext(); )
			{
				Node n      = (Node) ni.Current;
				Label label = n.Label;
				// isolated nodes should always have at least one geometry in their label
				Debug.Assert(label.GeometryCount > 0, "node with empty label found");
				if (n.Isolated)
				{
					if (label.IsNull(0))
						LabelIsolatedNode(n, 0);
					else
						LabelIsolatedNode(n, 1);
				}
			}
		}
		
		/// <summary> 
		/// Label an isolated node with its relationship to the target geometry.
		/// </summary>
		private void  LabelIsolatedNode(Node n, int targetIndex)
		{
			int loc = ptLocator.Locate(n.Coordinate, arg[targetIndex].Geometry);
			n.Label.SetAllLocations(targetIndex, loc);
		}
        
        #endregion
    }
}