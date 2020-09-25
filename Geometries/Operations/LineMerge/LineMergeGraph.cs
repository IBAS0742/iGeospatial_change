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

using iGeospatial.Geometries;
using iGeospatial.Geometries.PlanarGraphs;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Operations.LineMerge
{
	/// <summary> 
	/// A planar graph of edges that is analyzed to sew the edges together. The 
	/// marked flag on <see cref="Edge"/>s 
	/// and <see cref="Node"/>s indicates whether they have been
	/// logically deleted from the graph.
	/// </summary>
	internal sealed class LineMergeGraph : PlanarGraph
	{
        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="LineMergeGraph"/> class.
        /// </summary>
        public LineMergeGraph()
        {
        }

        #endregion

        #region Public Methods

		/// <summary> 
		/// Adds an Edge, DirectedEdges, and Nodes for the given LineString representation
		/// of an edge. 
		/// </summary>
		public void AddEdge(LineString lineString)
		{
			if (lineString.IsEmpty)
			{
				return;
			}

			ICoordinateList coordinates = CoordinateCollection.RemoveRepeatedCoordinates(lineString.Coordinates);
			Coordinate startCoordinate = coordinates[0];
			Coordinate endCoordinate = coordinates[coordinates.Count - 1];
			Node startNode = GetNode(startCoordinate);
			Node endNode = GetNode(endCoordinate);
			DirectedEdge directedEdge0 = new LineMergeDirectedEdge(startNode, endNode, coordinates[1], true);
			DirectedEdge directedEdge1 = new LineMergeDirectedEdge(endNode, startNode, coordinates[coordinates.Count - 2], false);
			Edge edge = new LineMergeEdge(lineString);
			edge.SetDirectedEdges(directedEdge0, directedEdge1);

			Add(edge);
		}
        
        #endregion
		
        #region Private Methods

		private Node GetNode(Coordinate coordinate)
		{
			Node node = FindNode(coordinate);
			if (node == null)
			{
				node = new Node(coordinate);
				Add(node);
			}
			
			return node;
		}
        
        #endregion
	}
}