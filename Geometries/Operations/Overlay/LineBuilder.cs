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
using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Overlay
{
	/// <summary> 
	/// Forms <see cref="LineString"/>s out of a the graph of <see cref="DirectedEdge"/>s
	/// created by an <see cref="OverlayOp"/>.
	/// </summary>
	internal class LineBuilder
	{
        #region Private Fields

		private OverlayOp       op;
		private GeometryFactory geometryFactory;
		private PointLocator    ptLocator;
		
		private ArrayList       lineEdgesList;
		private GeometryList    resultLineList;
        
        #endregion
		
        #region Constructors and Destructor

		public LineBuilder(OverlayOp op, GeometryFactory geometryFactory, 
            PointLocator ptLocator)
		{
            lineEdgesList        = new ArrayList();
            resultLineList       = new GeometryList();

			this.op              = op;
			this.geometryFactory = geometryFactory;
			this.ptLocator       = ptLocator;
		}

        #endregion

        #region Public Methods

		/// <returns> 
		/// a list of the LineStrings in the result of the specified overlay operation
		/// </returns>
		public GeometryList Build(OverlayType opCode)
		{
			FindCoveredLineEdges();
			CollectLines(opCode);
			//LabelIsolatedLines(lineEdgesList);
			BuildLines(opCode);

			return resultLineList;
		}
        
        #endregion

        #region Private Methods

		/// <summary> 
		/// Find and mark L edges which are "covered" by the result area (if any).
		/// L edges at nodes which also have A edges can be checked by checking
		/// their depth at that node.
		/// L edges at nodes which do not have A edges can be checked by doing a
		/// point-in-polygon test with the previously computed result areas.
		/// </summary>
		private void  FindCoveredLineEdges()
		{
			// first set covered for all L edges at nodes which have A edges too
			for (IEnumerator nodeit = op.Graph.Nodes.GetEnumerator(); nodeit.MoveNext(); )
			{
				Node node = (Node) nodeit.Current;

                ((DirectedEdgeStar) node.Edges).FindCoveredLineEdges();
			}
			
			// For all L edges which weren't handled by the above,
			// use a point-in-poly test to determine whether they are covered
			for (IEnumerator it = op.Graph.EdgeEnds.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				Edge e = de.Edge;
				if (de.LineEdge && !e.CoveredSet)
				{
					bool isCovered = op.IsCoveredByA(de.Coordinate);
					e.Covered = isCovered;
				}
			}
		}
		
		private void CollectLines(OverlayType opCode)
		{
			for (IEnumerator it = op.Graph.EdgeEnds.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				CollectLineEdge(de, opCode, lineEdgesList);
				CollectBoundaryTouchEdge(de, opCode, lineEdgesList);
			}
		}
        
        /// <summary>
        /// Collect line edges which are in the result.
        /// Line edges are in the result if they are not part of
        /// an area boundary, if they are in the result of the overlay operation,
        /// and if they are not covered by a result area.
        /// </summary>
        /// <param name="de">the directed edge to test</param>
        /// <param name="opCode">the overlap operation</param>
        /// <param name="edges">the list of included line edges</param>
        private void CollectLineEdge(DirectedEdge de, OverlayType opCode, ArrayList edges)
		{
			Label label = de.Label;
			Edge e = de.Edge;

			// include L edges which are in the result
			if (de.LineEdge)
			{
				if (!de.Visited && OverlayOp.IsResultOfOp(label, opCode) && !e.Covered)
				{
					edges.Add(e);
					de.VisitedEdge = true;
				}
			}
		}

		/// <summary> 
		/// Collect edges from area inputs which should be in the result but
		/// which have not been included in a result area.
		/// </summary>
		/// <remarks>
		/// This happens ONLY:
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// During an intersection when the boundaries of two areas touch 
		/// in a line segment.
		/// </description>
		/// </item>
		/// <item>
		/// <description>OR as a result of a dimensional collapse.</description>
		/// </item>
		/// </list>
		/// </remarks>
		private void CollectBoundaryTouchEdge(DirectedEdge de, 
            OverlayType opCode, ArrayList edges)
		{
            Label label = de.Label;
            if (de.LineEdge) 
                return;  // only interested in area edges
            if (de.Visited) 
                return;  // already processed
            if (de.InteriorAreaEdge) 
                return;  // added to handle dimensional collapses
            if (de.Edge.InResult) 
                return;  // if the edge linework is already included, don't include it again

            // sanity check for labelling of result edgerings
            Debug.Assert(!(de.InResult || de.Sym.InResult) || !de.Edge.InResult);

            // include the linework if it's in the result of the operation
            if (OverlayOp.IsResultOfOp(label, opCode)
                && opCode == OverlayType.Intersection)
            {
                edges.Add(de.Edge);
                de.VisitedEdge = true;
            }
        }
		
		private void BuildLines(OverlayType opCode)
		{
			// need to simplify lines?
			for (IEnumerator it = lineEdgesList.GetEnumerator(); it.MoveNext(); )
			{
				Edge e = (Edge) it.Current;

                LineString line = geometryFactory.CreateLineString(e.Coordinates);
				resultLineList.Add(line);
				e.InResult = true;
			}
		}
		
		private void  LabelIsolatedLines(ArrayList edgesList)
		{
			for (IEnumerator it = edgesList.GetEnumerator(); it.MoveNext(); )
			{
				Edge e = (Edge) it.Current;
				Label label = e.Label;

                if (e.Isolated)
				{
					if (label.IsNull(0))
						LabelIsolatedLine(e, 0);
					else
						LabelIsolatedLine(e, 1);
				}
			}
		}

		/// <summary> 
		/// Label an isolated node with its relationship to the target geometry.
		/// </summary>
		private void  LabelIsolatedLine(Edge e, int targetIndex)
		{
			int loc = ptLocator.Locate(e.Coordinate, 
                op.GetArgGeometry(targetIndex));
			e.Label.SetLocation(targetIndex, loc);
		}
        
        #endregion
	}
}