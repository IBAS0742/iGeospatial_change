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
using iGeospatial.Geometries.Noding;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Operations.Overlay;

namespace iGeospatial.Geometries.Operations.Buffer
{
	/// <summary> 
	/// Builds the Buffer geometry for a given input geometry and precision model.
	/// </summary>
	/// <remarks>
	/// Allows setting the level of approximation for circular arcs,
	/// and the precision model in which to carry out the computation.
	/// <para>
	/// When computing buffers in floating point double-precision
	/// it can happen that the process of iterated noding can fail to converge (terminate).
	/// In this case a GeometryException will be thrown.
	/// Retrying the computation in a fixed precision
	/// can produce more robust results.
	/// </para>
	/// </remarks>
	[Serializable]
    internal class BufferBuilder
	{
        #region Private Fields

        private int             quadrantSegments;
        private BufferCapType   endCapStyle;
		
        private PrecisionModel  workingPrecisionModel;
        private GeometryFactory geomFact;
        private INoder          workingNoder;
        private PlanarGraph     graph;
        private EdgeList        edgeList;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> Creates a new BufferBuilder</summary>
		public BufferBuilder()
		{
            quadrantSegments = OffsetCurveBuilder.DefaultQuadrantSegments;
            endCapStyle      = BufferCapType.Round;
            edgeList         = new EdgeList();
        }
        
        #endregion
		
        #region Public Properties

        /// <summary> 
        /// Gets or sets the <see cref="iGeospatial.Geometries.Noding.Node"/> 
        /// to use during noding.
        /// This allows choosing fast but non-robust noding, or slower
        /// but robust noding.
        /// </summary>
        /// <value>the noder to use
        /// </value>
        public INoder Noder
        {
            get
            {
                return this.workingNoder;
            }

            set
            {
                this.workingNoder = value;
            }
        }
		
		/// <summary> 
		/// Gets or sets the number of segments used to approximate a 
		/// angle fillet. 
		/// </summary>
		/// <value>the number of segments in a fillet for a quadrant </value>
		public int QuadrantSegments
		{
            get
            {
                return this.quadrantSegments;
            }

			set
			{
				this.quadrantSegments = value;
			}
		}

		/// <summary> 
		/// Sets the precision model to use during the curve computation and noding,
		/// if it is different to the precision model of the Geometry.
		/// If the precision model is less than the precision of the Geometry precision model,
		/// the Geometry must have previously been rounded to that precision.
		/// 
		/// </summary>
		/// <value>the precision model to use </value>
		public PrecisionModel WorkingPrecisionModel
		{
            get
            {
                return this.workingPrecisionModel;
            }

			set
			{
				this.workingPrecisionModel = value;
			}
		}

		public BufferCapType EndCapStyle
		{
            get 
            {
                return this.endCapStyle;
            }

			set
			{
				this.endCapStyle = value;
			}
		}
        
        #endregion

        #region Public Methods

		public virtual Geometry Buffer(Geometry g, double distance)
		{
			PrecisionModel precisionModel = workingPrecisionModel;
			if (precisionModel == null)
				precisionModel = g.PrecisionModel;
			
			// factory must be the same as the one used by the input
			geomFact = g.Factory;
			
			OffsetCurveBuilder curveBuilder = 
                new OffsetCurveBuilder(precisionModel, quadrantSegments);
			curveBuilder.EndCapStyle = endCapStyle;

			OffsetCurveSetBuilder curveSetBuilder = 
                new OffsetCurveSetBuilder(g, distance, curveBuilder);
			
			ArrayList bufferSegStrList = curveSetBuilder.Curves;
			
			// short-circuit test
			if (bufferSegStrList.Count <= 0)
			{
				Geometry emptyGeom = 
                    geomFact.CreateGeometryCollection(new Geometry[0]);

				return emptyGeom;
			}
			
			ComputeNodedEdges(bufferSegStrList, precisionModel);
			graph = new PlanarGraph(new OverlayNodeFactory());
			graph.AddEdges(edgeList.Edges);
			
			IList subgraphList         = CreateSubgraphs(graph);
			PolygonBuilder polyBuilder = new PolygonBuilder(geomFact);
			
            BuildSubgraphs(subgraphList, polyBuilder);

			GeometryList resultPolyList = polyBuilder.Build();
			
			Geometry resultGeom = geomFact.BuildGeometry(resultPolyList);

			return resultGeom;
		}
        
        #endregion
		
        #region Protected Methods

		/// <summary> 
		/// Inserted edges are checked to see if an identical edge already exists.
		/// If so, the edge is not inserted, but its label is merged
		/// with the existing edge.
		/// </summary>
		protected virtual void InsertEdge(Edge e)
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
				existingLabel.Merge(labelToMerge);
				
				// compute new depth delta of sum of edges
				int mergeDelta = DepthDelta(labelToMerge);
				int existingDelta = existingEdge.DepthDelta;
				int newDelta = existingDelta + mergeDelta;
				existingEdge.DepthDelta = newDelta;
			}
			else
			{
				// no matching existing edge was found
				// Add this new edge to the list of edges in this graph
				//e.setName(name + edges.Size());
				edgeList.Add(e);
				e.DepthDelta = DepthDelta(e.Label);
			}
		}
        
        #endregion
		
        #region Private Methods

		private void ComputeNodedEdges(IList bufferSegStrList, PrecisionModel precisionModel)
		{
            INoder noder = CreateNoder(precisionModel);
            noder.ComputeNodes(bufferSegStrList);

            IList nodedSegStrings = noder.NodedSubstrings;
	
            int nCount = nodedSegStrings.Count;
			for (int i = 0; i < nCount; i++)
			{
				SegmentString segStr = (SegmentString)nodedSegStrings[i];
                Label oldLabel       = (Label)segStr.Data;
                Edge edge = new Edge(segStr.Coordinates, new Label(oldLabel));

				InsertEdge(edge);
			}
		}
		
		private IList CreateSubgraphs(PlanarGraph graph)
		{
			ArrayList subgraphList = new ArrayList();

            for (IEnumerator i = graph.Nodes.GetEnumerator(); i.MoveNext(); )
			{
				Node node = (Node) i.Current;
				if (!node.Visited)
				{
					BufferSubgraph subgraph = new BufferSubgraph();
					subgraph.Create(node);

                    subgraphList.Add(subgraph);
				}
			}

			// Sort the subgraphs in descending order of their rightmost coordinate.
			// This ensures that when the Polygons for the subgraphs are built,
			// subgraphs for shells will have been built before the subgraphs for
			// any holes they contain.
            subgraphList.Sort(new ReverseComparator());

			return subgraphList;
		}
		
		/// <summary> 
		/// Completes the building of the input subgraphs by depth-labelling them,
		/// and adds them to the PolygonBuilder.
		/// The subgraph list must be sorted in rightmost-coordinate order.
		/// </summary>
		/// <param name="subgraphList">the subgraphs to build
		/// </param>
		/// <param name="polyBuilder">the PolygonBuilder which will build the final polygons
		/// </param>
		private void BuildSubgraphs(IList subgraphList, PolygonBuilder polyBuilder)
		{
			ArrayList processedGraphs = new ArrayList();

            int nCount = subgraphList.Count;

            for (int i = 0; i < nCount; i++)
			{
				BufferSubgraph subgraph = (BufferSubgraph)subgraphList[i];
				Coordinate p = subgraph.RightmostCoordinate;

                SubgraphDepthLocater locater = new SubgraphDepthLocater(processedGraphs);
				int outsideDepth = locater.GetDepth(p);

                subgraph.ComputeDepth(outsideDepth);
				subgraph.FindResultEdges();
				processedGraphs.Add(subgraph);
				polyBuilder.Add(subgraph.DirectedEdges, subgraph.Nodes);
			}
		}
		
        private INoder CreateNoder(PrecisionModel precisionModel)
        {
            if (workingNoder != null)
                return workingNoder;
			
            // otherwise use a fast (but non-robust) noder
            MCIndexNoder noder       = new MCIndexNoder();
            LineIntersector li       = new RobustLineIntersector();
            li.PrecisionModel        = precisionModel;
            noder.SegmentIntersector = new IntersectionAdder(li);

            return noder;
        }

		/// <summary> 
		/// Compute the change in depth as an edge is crossed from R to L.
		/// </summary>
		private static int DepthDelta(Label label)
		{
			int lLoc = label.GetLocation(0, Position.Left);
			int rLoc = label.GetLocation(0, Position.Right);

			if (lLoc == LocationType.Interior && 
                rLoc == LocationType.Exterior)
				return 1;
			else if (lLoc == LocationType.Exterior && 
                rLoc == LocationType.Interior)
				return - 1;

			return 0;
		}
        
        #endregion
		
        #region ReverseComparator Class
		
        private class ReverseComparator : IComparer
        {
            public virtual int Compare(object o1, object o2)
            {
                System.IComparable c1 = (System.IComparable) o1;
                System.IComparable c2 = (System.IComparable) o2;
				
                int cmp = c1.CompareTo(c2);
					
                // We cannot simply return -cmp, as -Integer.MinValue == Integer.MinValue.
                int bits = 1;	
                if (cmp >= 0)
                    return -(cmp >> bits);
                else
                    return -((cmp >> bits) + (2 << ~bits));

                //				return -(cmp | (URShift(cmp, 1)));
            }
        }
        
        #endregion
	}
}