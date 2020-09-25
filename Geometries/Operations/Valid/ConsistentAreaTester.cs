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
using iGeospatial.Geometries.Graphs.Index;
using iGeospatial.Geometries.Operations.Relate;

namespace iGeospatial.Geometries.Operations.Valid
{
    /// <summary> 
    /// Checks that a <see cref="GeometryGraph"/> representing an area
    /// (a <see cref="Polygon"/> or <see cref="MultiPolygon"/> )
    /// has consistent semantics for area geometries.
    /// </summary>
    /// <remarks>
    /// This check is required for any reasonable polygonal model
    /// (including the OGC-SFS model, as well as models which allow ring self-intersection at single points)
    /// <para>
    /// Checks include:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// test for rings which properly intersect
    /// (but not for ring self-intersection, or intersections at vertices)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// test for consistent labelling at all node points
    /// (this detects vertex intersections with invalid topology,
    /// i.e. where the exterior side of an edge lies in the interior of the area)
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// test for duplicate rings
    /// </description>
    /// </item>
    /// </list>
    /// If an inconsistency is found the location of the problem
    /// is recorded and is available to the caller.
    /// </remarks>
    internal class ConsistentAreaTester
	{
        #region Private Fields

		private LineIntersector li;
		private GeometryGraph geomGraph;
		private RelateNodeGraph nodeGraph;
		
		// the intersection point found (if any)
		private Coordinate invalidPoint;
        
        #endregion
		
        #region Constructors and Destructor

        /// <summary> 
        /// Creates a new tester for consistent areas.
        /// </summary>
        /// <param name="geomGraph">
        /// The topology graph of the area geometry
        /// </param>
        public ConsistentAreaTester(GeometryGraph geomGraph)
		{
            li = new RobustLineIntersector();
            nodeGraph = new RelateNodeGraph();

			this.geomGraph = geomGraph;
		}

        #endregion

        #region Public Properties
		
        /// <summary>
        /// Gets intersection point.
        /// </summary>
		/// <value> 
		/// the intersection point, or null if none was found
		/// </value>
        public Coordinate InvalidPoint
		{
			get
			{
				return invalidPoint;
			}
		}
        
        #endregion

        #region Public Methods

        /// <summary> 
        /// Check all nodes to see if their labels are consistent with 
        /// area topology.
        /// </summary>
        /// <returns> 
        /// <c>true</c> if this area has a consistent node labelling
        /// </returns>
        public bool IsNodeConsistentArea()
		{
			// To fully check validity, it is necessary to
			// compute ALL intersections, including self-intersections 
            // Within a single edge.
			SegmentIntersector intersector = 
                geomGraph.ComputeSelfNodes(li, true);
			if (intersector.HasProperIntersection())
			{
				invalidPoint = intersector.ProperIntersectionPoint;
				return false;
			}
			
			nodeGraph.Build(geomGraph);
			
			return IsNodeEdgeAreaLabelsConsistent();
		}
		
		/// <summary> 
		/// Checks for two duplicate rings in an area.
		/// Duplicate rings are rings that are topologically equal
		/// (that is, which have the same sequence of points up to point order).
		/// If the area is topologically consistent (determined by calling the
		/// isNodeConsistentArea,
		/// duplicate rings can be found by checking for EdgeBundles which contain
		/// more than one EdgeEnd.
		/// (This is because topologically consistent areas cannot have two rings sharing
		/// the same line segment, unless the rings are equal).
		/// The start point of one of the equal rings will be placed in
		/// invalidPoint.
		/// </summary>
		/// <returns> 
		/// true if this area Geometry is topologically consistent but has 
		/// two duplicate rings
		/// </returns>
		public bool HasDuplicateRings()
		{
			for (IEnumerator nodeIt = nodeGraph.NodeIterator(); 
                nodeIt.MoveNext(); )
			{
				RelateNode node = (RelateNode) nodeIt.Current;

                for (IEnumerator i = node.Edges.Iterator(); i.MoveNext(); )
				{
					EdgeEndBundle eeb = (EdgeEndBundle) i.Current;
					if (eeb.EdgeEnds.Count > 1)
					{
						invalidPoint = eeb.Edge.GetCoordinate(0);
						return true;
					}
				}
			}

			return false;
		}
        
        #endregion

        #region Private Methods

		/// <summary> 
		/// Check all nodes to see if their labels are consistent. 
		/// If any are not, return false
		/// </summary>
        /// <returns> 
        /// <c>true</c> if the edge area labels are consistent at this node
        /// </returns>
        private bool IsNodeEdgeAreaLabelsConsistent()
		{
            for (IEnumerator nodeIt = nodeGraph.NodeIterator(); 
                nodeIt.MoveNext(); )
            {
                RelateNode node = (RelateNode) nodeIt.Current;
                if (!node.Edges.AreaLabelsConsistent)
                {
                    invalidPoint = node.Coordinate.Clone();
                    return false;
                }
            }

            return true;
		}
        
        #endregion
	}
}