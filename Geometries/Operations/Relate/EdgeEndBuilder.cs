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

namespace iGeospatial.Geometries.Operations.Relate
{
	/// <summary> 
	/// Computes the <see cref="EdgeEnd"/> class instances, which arise 
	/// from a noded <see cref="Edge"/>.
	/// </summary>
	internal class EdgeEndBuilder
	{
        #region Constructors and Destructor

		public EdgeEndBuilder()
		{
		}

        #endregion
		
        #region Public Methods

		public ArrayList ComputeEdgeEnds(IEdgeEnumerator edges)
		{
			ArrayList l = new ArrayList();

            for (IEdgeEnumerator i = edges; i.MoveNext(); )
			{
				Edge e = i.Current;
				ComputeEdgeEnds(e, l);
			}
			return l;
		}
		
		/// <summary> 
		/// Creates stub edges for all the intersections in this
		/// Edge (if any) and inserts them into the graph.
		/// </summary>
		public void ComputeEdgeEnds(Edge edge, ArrayList l)
		{
			EdgeIntersectionList eiList = edge.EdgeIntersectionList;
			// ensure that the list has entries for the first and last point of the edge
			eiList.AddEndpoints();
			
			IEnumerator it = eiList.Iterator();
			EdgeIntersection eiPrev = null;
			EdgeIntersection eiCurr = null;

			// no intersections, so there is nothing to do
			if (!it.MoveNext())
				return ;

            EdgeIntersection eiNext = (EdgeIntersection) it.Current;
			do 
			{
				eiPrev = eiCurr;
				eiCurr = eiNext;
				eiNext = null;

                if (it.MoveNext())
				{
					eiNext = (EdgeIntersection) it.Current;
				}
				
				if (eiCurr != null)
				{
					CreateEdgeEndForPrev(edge, l, eiCurr, eiPrev);
					CreateEdgeEndForNext(edge, l, eiCurr, eiNext);
				}
			}
			while (eiCurr != null);
		}
        
        #endregion
		
        #region Internal Methods

		/// <summary> 
		/// Create a EdgeStub for the edge before the intersection eiCurr.
		/// The previous intersection is provided
		/// in case it is the endpoint for the stub edge.
		/// Otherwise, the previous point from the parent edge will be the endpoint.
		/// <br>
		/// eiCurr will always be an EdgeIntersection, but eiPrev may be null.
		/// </summary>
		internal void  CreateEdgeEndForPrev(Edge edge, 
            ArrayList l, EdgeIntersection eiCurr, EdgeIntersection eiPrev)
		{
			
			int iPrev = eiCurr.segmentIndex;
			if (eiCurr.dist == 0.0)
			{
				// if at the start of the edge there is no previous edge
				if (iPrev == 0)
					return ;
				iPrev--;
			}
			Coordinate pPrev = edge.GetCoordinate(iPrev);
			// if prev intersection is past the previous vertex, use it instead
			if (eiPrev != null && eiPrev.segmentIndex >= iPrev)
				pPrev = eiPrev.coord;
			
			Label label = new Label(edge.Label);
			// since edgeStub is oriented opposite to it's parent edge, have to flip sides for edge label
			label.Flip();

			EdgeEnd e = new EdgeEnd(edge, eiCurr.coord, pPrev, label);

            l.Add(e);
		}

		/// <summary> 
		/// Create a StubEdge for the edge after the intersection eiCurr.
		/// The next intersection is provided in case it is the endpoint for the stub edge.
		/// Otherwise, the next point from the parent edge will be the endpoint.
		/// <para>
		/// eiCurr will always be an EdgeIntersection, but eiNext may be null.
		/// </para>
		/// </summary>
		internal void CreateEdgeEndForNext(Edge edge, 
            ArrayList l, EdgeIntersection eiCurr, EdgeIntersection eiNext)
		{
			
			int iNext = eiCurr.segmentIndex + 1;
			// if there is no next edge there is nothing to do
			if (iNext >= edge.NumPoints && eiNext == null)
				return ;
			
			Coordinate pNext = edge.GetCoordinate(iNext);
			
			// if the next intersection is in the same segment as the current,
            // use it as the endpoint
			if (eiNext != null && eiNext.segmentIndex == eiCurr.segmentIndex)
				pNext = eiNext.coord;
			
			EdgeEnd e = new EdgeEnd(edge, eiCurr.coord, pNext, new Label(edge.Label));

			l.Add(e);
		}
        
        #endregion
    }
}