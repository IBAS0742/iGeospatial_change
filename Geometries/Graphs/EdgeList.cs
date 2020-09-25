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
using iGeospatial.Geometries.Indexers;
using iGeospatial.Geometries.Indexers.QuadTree;

namespace iGeospatial.Geometries.Graphs
{
	
	/// <summary> 
	/// A EdgeList is a list of Edges.  It supports locating edges
	/// that are pointwise equals to a target edge.
	/// </summary>
	[Serializable]
    internal class EdgeList
	{
        private EdgeCollection edges;

		/// <summary> 
		/// An index of the edges, for fast lookup.
		/// a Quadtree is used, because this index needs to be dynamic
		/// (e.g. allow insertions after queries).
		/// An alternative would be to use an ordered set based on the values
		/// of the edge coordinates
		/// 
		/// </summary>
		private ISpatialIndex index;

        public EdgeList()
        {
            edges = new EdgeCollection();
            index = new Quadtree();
        }
		
        public Edge this[int i]
        {
            get
            {
                return edges[i];
            }
        }
		
		public EdgeCollection Edges
		{
			get
			{
				return edges;
			}
		}
		
		/// <summary> Insert an edge unless it is already in the list</summary>
		public void Add(Edge e)
		{
			edges.Add(e);
			index.Insert(e.Envelope, e);
		}
		
		//TODO--PAUL
		public void Replace(Edge eOld, Edge eNew)
		{
			edges.Remove(eOld);
			//index.remove?

            Add(eNew);
		}

		//TODO--PAUL
		public void Remove(Edge e)
		{
			edges.Remove(e);
			//index.remove?
		}

		public void AddAll(EdgeCollection edgeColl)
		{
			for (IEdgeEnumerator i = edgeColl.GetEnumerator(); i.MoveNext();)
			{
				Add(i.Current);
			}
		}
		
		// <FIX> fast lookup for edges
		/// <summary> If there is an edge equal to e already in the list, return it.
		/// Otherwise return null.
		/// </summary>
		/// <returns>  equal edge, if there is one already in the list
		/// null otherwise
		/// </returns>
		public Edge FindEqualEdge(Edge e)
		{
			IList testEdges = index.Query(e.Envelope);
			
            int nCount = testEdges.Count;
			for (int i = 0; i < nCount; i++)
			{
				Edge testEdge = (Edge)testEdges[i];
				if (testEdge.Equals(e))
					return testEdge;
			}
			
			return null;
		}
		
		public IEdgeEnumerator Iterator()
		{
			return edges.GetEnumerator();
		}
		
		/// <summary> If the edge e is already in the list, return its index.</summary>
		/// <returns>  index, if e is already in the list
		/// -1 otherwise
		/// </returns>
		public int FindEdgeIndex(Edge e)
		{
            int nCount = edges.Count;
			for (int i = 0; i < nCount; i++)
			{
				if (edges[i].Equals(e))
					return i;
			}

			return -1;
		}
	}
}