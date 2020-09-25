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

namespace iGeospatial.Geometries.Operations.Overlay
{
	/// <summary> 
	/// A ring of edges which may contain nodes of degree greater than two (2).
	/// </summary>
	/// <remarks>
	/// A MaximalEdgeRing may represent two different spatial entities:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// a single polygon possibly containing inversions (if the ring is oriented CW)
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// a single hole possibly containing exversions (if the ring is oriented CCW)
	/// </description>
	/// </item>
	/// </list>
	/// If the MaximalEdgeRing represents a polygon,
	/// the interior of the polygon is strongly connected.
	/// <para>
	/// These are the form of rings used to define polygons under some spatial data models.
	/// However, under the OGC SFS model, <see cref="MinimalEdgeRing"/>s are required.
	/// A MaximalEdgeRing can be converted to a list of MinimalEdgeRings using the
	/// <see cref="MaximalEdgeRing.buildMinimalRings"/> method.
	/// </para>
	/// <seealso cref="iGeospatial.Geometries.Operations.Overlay.MinimalEdgeRing">
	/// </seealso>
	/// </remarks>
	internal class MaximalEdgeRing : EdgeRing
	{
        #region Constructors and Destructor

		public MaximalEdgeRing(DirectedEdge start, 
            GeometryFactory geometryFactory) : base(start, geometryFactory)
		{
		}

        #endregion
		
        #region Public Methods

		public override DirectedEdge GetNext(DirectedEdge de)
		{
			return de.Next;
		}

		public override void  SetEdgeRing(DirectedEdge de, EdgeRing er)
		{
			de.EdgeRing = er;
		}
		
		/// <summary> 
		/// For all nodes in this EdgeRing, link the DirectedEdges at the node to form 
		/// minimalEdgeRings.
		/// </summary>
		public void LinkDirectedEdgesForMinimalEdgeRings()
		{
			DirectedEdge de = startDe;
			do 
			{
				Node node = de.Node;
				((DirectedEdgeStar) node.Edges).LinkMinimalDirectedEdges(this);
				de = de.Next;
			}
			while (de != startDe);
		}
		
		public EdgeRingCollection BuildMinimalRings()
		{
			EdgeRingCollection minEdgeRings = new EdgeRingCollection();
			DirectedEdge de        = startDe;
			do 
			{
				if (de.MinEdgeRing == null)
				{
					EdgeRing minEr = new MinimalEdgeRing(de, geometryFactory);
					minEdgeRings.Add(minEr);
				}
				de = de.Next;
			}
			while (de != startDe);

			return minEdgeRings;
		}
        
        #endregion
	}
}