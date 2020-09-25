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
using iGeospatial.Geometries.Operations.Overlay;

namespace iGeospatial.Geometries.Operations.Buffer
{
	/// <summary> 
	/// A RightmostEdgeFinder find the DirectedEdge in a list which has the highest coordinate,
	/// and which is oriented L to R at that point. (I.e. the right side is on the RHS of the edge.)
	/// </summary>
	internal class RightmostEdgeFinder
	{
		//private Coordinate extremeCoord;
		private int minIndex            = -1;
		private Coordinate minCoord;
		private DirectedEdge minDe;
		private DirectedEdge orientedDe;

		/// <summary> 
		/// A RightmostEdgeFinder finds the DirectedEdge with the rightmost coordinate.
		/// The DirectedEdge returned is guaranteed to have the R of the world on its RHS.
		/// </summary>
		public RightmostEdgeFinder()
		{
		}
		
		public virtual DirectedEdge Edge
		{
			get
			{
				return orientedDe;
			}
		}
			
		public virtual Coordinate Coordinate
		{
			get
			{
				return minCoord;
			}
		}
		
		public virtual void  FindEdge(ArrayList dirEdgeList)
		{
			/// <summary> Check all forward DirectedEdges only.  This is still general,
			/// because each edge has a forward DirectedEdge.
			/// </summary>
			for (IEnumerator i = dirEdgeList.GetEnumerator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				if (!de.Forward)
					continue;

				CheckForRightmostCoordinate(de);
			}
			
			/// <summary> If the rightmost point is a node, we need to identify which of
			/// the incident edges is rightmost.
			/// </summary>
			Debug.Assert(minIndex != 0 || minCoord.Equals(minDe.Coordinate), "inconsistency in rightmost processing");

			if (minIndex == 0)
			{
				FindRightmostEdgeAtNode();
			}
			else
			{
				FindRightmostEdgeAtVertex();
			}
			/// <summary> now check that the extreme side is the R side.
			/// If not, use the sym instead.
			/// </summary>
			orientedDe = minDe;
			int rightmostSide = GetRightmostSide(minDe, minIndex);
			if (rightmostSide == Position.Left)
			{
				orientedDe = minDe.Sym;
			}
		}

		private void  FindRightmostEdgeAtNode()
		{
			Node node = minDe.Node;
			DirectedEdgeStar star = (DirectedEdgeStar) node.Edges;
			minDe = star.GetRightmostEdge();

			// the DirectedEdge returned by the previous call is not
			// necessarily in the forward direction. Use the sym edge if it isn't.
			if (!minDe.Forward)
			{
				minDe = minDe.Sym;
				minIndex = minDe.Edge.Coordinates.Count - 1;
			}
		}

		private void  FindRightmostEdgeAtVertex()
		{
			/// <summary> The rightmost point is an interior vertex, so it has a segment on either side of it.
			/// If these segments are both above or below the rightmost point, we need to
			/// determine their relative orientation to decide which is rightmost.
			/// </summary>
			ICoordinateList pts = minDe.Edge.Coordinates;
			Debug.Assert(minIndex > 0 && minIndex < pts.Count, "rightmost point expected to be interior vertex of edge");
			Coordinate pPrev = pts[minIndex - 1];
			Coordinate pNext = pts[minIndex + 1];
			OrientationType orientation = CGAlgorithms.ComputeOrientation(minCoord, pNext, pPrev);
			bool usePrev = false;
			// both segments are below min point
			if (pPrev.Y < minCoord.Y && pNext.Y < minCoord.Y && 
                orientation == OrientationType.CounterClockwise)
			{
				usePrev = true;
			}
			else if (pPrev.Y > minCoord.Y && pNext.Y > minCoord.Y && 
                orientation == OrientationType.Clockwise)
			{
				usePrev = true;
			}
			// if both segments are on the same side, do nothing - either is safe
			// to select as a rightmost segment
			if (usePrev)
			{
				minIndex = minIndex - 1;
			}
		}

		private void CheckForRightmostCoordinate(DirectedEdge de)
		{
			ICoordinateList coord = de.Edge.Coordinates;
			for (int i = 0; i < coord.Count - 1; i++)
			{
				// only check vertices which are the start or end point of a non-horizontal segment
				// <FIX> MD 19 Sep 03 - NO!  we can test all vertices, since the rightmost must have a non-horiz segment adjacent to it
				if (minCoord == null || coord[i].X > minCoord.X)
				{
					minDe = de;
					minIndex = i;
					minCoord = coord[i];
				}
			}
		}
		
		private int GetRightmostSide(DirectedEdge de, int index)
		{
			int side = GetRightmostSideOfSegment(de, index);
			if (side < 0)
				side = GetRightmostSideOfSegment(de, index - 1);
			if (side < 0)
			{
				// reaching here can indicate that segment is horizontal
				//Assert.shouldNeverReachHere("problem with finding rightmost side of segment at " + de.GetCoordinate());
				// testing only
				minCoord = null;
				CheckForRightmostCoordinate(de);
			}
			return side;
		}
		
		private int GetRightmostSideOfSegment(DirectedEdge de, int i)
		{
			Edge e = de.Edge;
			ICoordinateList coord = e.Coordinates;
			
			if (i < 0 || i + 1 >= coord.Count)
				return - 1;
			if (coord[i].Y == coord[i + 1].Y)
				return - 1; // indicates edge is parallel to x-axis
			
			int pos = Position.Left;
			if (coord[i].Y < coord[i + 1].Y)
				pos = Position.Right;
			return pos;
		}
	}
}