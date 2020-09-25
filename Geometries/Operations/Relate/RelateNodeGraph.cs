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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Relate
{
	/// <summary> 
	/// Implements the simple graph of Nodes and EdgeEnd which is all that is
	/// required to determine topological relationships between geometries.
	/// Also supports building a topological graph of a single Geometry, to
	/// allow verification of valid topology.
	/// </summary>
	/// <remarks>
	/// It is not necessary to create a fully linked
	/// PlanarGraph to determine relationships, since it is sufficient
	/// to know how the Geometries interact locally around the nodes.
	/// In fact, this is not even feasible, since it is not possible to compute
	/// exact intersection points, and hence the topology around those nodes
	/// cannot be computed robustly.
	/// <para>
	/// The only Nodes that are created are for improper intersections;
	/// that is, nodes which occur at existing vertices of the Geometries.
	/// Proper intersections (e.g. ones which occur between the interior of line segments)
	/// have their topology determined implicitly, without creating a Node object
	/// to represent them.
	/// </para>
	/// </remarks>
	internal class RelateNodeGraph
	{
        #region Private Fields

        private NodeMap nodes;
        
        #endregion
		
        #region Constructors and Destructor

        public RelateNodeGraph()
        {
            nodes = new NodeMap(new RelateNodeFactory());
        }

        #endregion
		
        #region Public Methods

		public IEnumerator NodeIterator()
		{
            return nodes.Iterator();
		}
		
		public void Build(GeometryGraph geomGraph)
		{
			// compute nodes for intersections between previously noded edges
			ComputeIntersectionNodes(geomGraph, 0);

			// Copy the labelling for the nodes in the parent Geometry.  These override
			// any labels determined by intersections.
			CopyNodesAndLabels(geomGraph, 0);
			
			//Build EdgeEnds for all intersections.
			EdgeEndBuilder eeBuilder = new EdgeEndBuilder();
			ArrayList eeList = eeBuilder.ComputeEdgeEnds(geomGraph.EdgeIterator);
			InsertEdgeEnds(eeList);
		}
		
		/// <summary> 
		/// Insert nodes for all intersections on the edges of a Geometry.
		/// Label the created nodes the same as the edge label if they do not already 
		/// have a label.
		/// This allows nodes created by either self-intersections or
		/// mutual intersections to be labelled.
		/// Endpoint nodes will already be labelled from when they were inserted.
		/// <para>
		/// Precondition: edge intersections have been computed.
		/// </para>
		/// </summary>
		public void ComputeIntersectionNodes(GeometryGraph geomGraph, int argIndex)
		{
			for (IEdgeEnumerator edgeIt = geomGraph.EdgeIterator; edgeIt.MoveNext(); )
			{
				Edge e   = edgeIt.Current;
				int eLoc = e.Label.GetLocation(argIndex);

                for (IEnumerator eiIt = e.EdgeIntersectionList.Iterator(); eiIt.MoveNext(); )
				{
					EdgeIntersection ei = (EdgeIntersection) eiIt.Current;
					RelateNode n = (RelateNode) nodes.AddNode(ei.coord);
					if (eLoc == LocationType.Boundary)
                    {
                        n.SetLabelBoundary(argIndex);
                    }
					else
					{
						if (n.Label.IsNull(argIndex))
							n.SetLabel(argIndex, LocationType.Interior);
					}
				}
			}
		}
		
		/// <summary> Copy all nodes from an arg geometry into this graph.
		/// The node label in the arg geometry overrides any previously computed
		/// label for that argIndex.
		/// (E.g. a node may be an intersection node with
		/// a computed label of BOUNDARY,
		/// but in the original arg Geometry it is actually
		/// in the interior due to the Boundary Determination Rule)
		/// </summary>
		public void CopyNodesAndLabels(GeometryGraph geomGraph, int argIndex)
		{
			for (IEnumerator nodeIt = geomGraph.NodeIterator; 
                nodeIt.MoveNext(); )
			{
				Node graphNode = (Node) nodeIt.Current;
				Node newNode = nodes.AddNode(graphNode.Coordinate);
				newNode.SetLabel(argIndex, graphNode.Label.GetLocation(argIndex));
			}
		}
		
		public void InsertEdgeEnds(ArrayList ee)
		{
			for (IEnumerator i = ee.GetEnumerator(); i.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) i.Current;
				nodes.Add(e);
			}
		}
        
        #endregion
    }
}