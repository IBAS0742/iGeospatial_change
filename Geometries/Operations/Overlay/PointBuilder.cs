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
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Operations.Overlay
{
	/// <summary> 
	/// Constructs <see cref="Point"/>s from the nodes of an overlay graph.
	/// </summary>
	internal class PointBuilder
	{
        #region Private Fields

		private OverlayOp       op;
		private GeometryFactory geometryFactory;
        private GeometryList    resultPointList;
        
        #endregion
		
        #region Constructors and Destructor

		public PointBuilder(OverlayOp op, GeometryFactory geometryFactory)
		{
			this.op              = op;
			this.geometryFactory = geometryFactory;
            this.resultPointList = new GeometryList();
		}
        
        #endregion

        #region Public Methods

        /// <summary> 
        /// Computes the Point geometries which will appear in the result,
        /// given the specified overlay operation.
        /// </summary>
        /// <returns>
        /// A list of the Points objects in the result.
        /// </returns>
        public IGeometryList Build(OverlayType opCode)
		{
            ExtractNonCoveredResultNodes(opCode);
            
            // It can happen that connected result nodes are still covered by
            // result geometries, so must perform this filter.
            // (For instance, this can happen during topology collapse).
            return resultPointList;
        }
        
        #endregion
		
        #region Private Methods

        /// <summary> 
        /// Determines nodes which are in the result, and creates 
        /// {@link Point}s for them.
        /// 
        /// This method determines nodes which are candidates for the result via their
        /// labelling and their graph topology.
        /// 
        /// </summary>
        /// <param name="opCode">the overlay operation
        /// </param>
        private void ExtractNonCoveredResultNodes(OverlayType opCode)
		{
			// Add nodes from edge intersections which have not already been included in the result
			for (IEnumerator nodeit = op.Graph.Nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node n = (Node) nodeit.Current;
				
                // filter out nodes which are known to be in the result
                if (n.InResult)
                    continue;
                // if an incident edge is in the result, then the node coordinate is included already
                if (n.IsIncidentEdgeInResult)
                    continue;

                if (n.Edges.Degree == 0 || 
                    opCode == OverlayType.Intersection)
                {
					
                    // For nodes on edges, only INTERSECTION can result in 
                    // edge nodes being included even
                    // if none of their incident edges are included
                    Label label = n.Label;
                    if (OverlayOp.IsResultOfOp(label, opCode))
                    {
                        FilterCoveredNodeToPoint(n);
                    }
                }
			}
		}

        /// <summary> 
        /// Converts non-covered nodes to Point objects and adds them to 
        /// the result.
        /// 
        /// A node is covered if it is contained in another element Geometry
        /// with higher dimension (e.g. a node point might be contained in a polygon,
        /// in which case the point can be eliminated from the result).
        /// 
        /// </summary>
        /// <param name="n">the node to test
        /// </param>
        private void FilterCoveredNodeToPoint(Node n)
		{
            Coordinate coord = n.Coordinate;
            if (!op.IsCoveredByLA(coord))
            {
                Point pt = geometryFactory.CreatePoint(coord);

                resultPointList.Add(pt);
            }
		}
        
        #endregion
    }
}