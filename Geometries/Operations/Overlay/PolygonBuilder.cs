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
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Graphs;

namespace iGeospatial.Geometries.Operations.Overlay
{
	/// <summary> 
	/// Forms Polygons out of a graph of <see cref="DirectedEdge"/>s.
	/// The edges to use are marked as being in the result Area.
	/// </summary>
	internal class PolygonBuilder
	{
        #region Private Fields

		private GeometryFactory geometryFactory;
		private EdgeRingCollection shellList;
        
        #endregion

        #region Constructors and Destructor

        public PolygonBuilder(GeometryFactory geometryFactory)
        {
            shellList = new EdgeRingCollection();

            this.geometryFactory = geometryFactory;
        }

        #endregion
		
        #region Public Methods

        public GeometryList Build()
        {
            GeometryList resultPolyList = ComputePolygons(shellList);

            return resultPolyList;
        }

		/// <summary> Add a complete graph.
		/// The graph is assumed to contain one or more polygons,
		/// possibly with holes.
		/// </summary>
		public void Add(PlanarGraph graph)
		{
			Add(graph.EdgeEnds, graph.Nodes);
		}
		
		/// <summary> Add a set of edges and nodes, which form a graph.
		/// The graph is assumed to contain one or more polygons,
		/// possibly with holes.
		/// </summary>
		public void Add(ArrayList dirEdges, ArrayList nodes)
		{
			PlanarGraph.LinkResultDirectedEdges(nodes);
			EdgeRingCollection maxEdgeRings = BuildMaximalEdgeRings(dirEdges);
			EdgeRingCollection freeHoleList = new EdgeRingCollection();
			EdgeRingCollection edgeRings    = BuildMinimalEdgeRings(maxEdgeRings, 
                shellList, freeHoleList);
			SortShellsAndHoles(edgeRings, shellList, freeHoleList);
			PlaceFreeHoles(shellList, freeHoleList);
			//Assert: every hole on freeHoleList has a shell assigned to it
		}
		
		public void Add(ArrayList dirEdges, ICollection nodes)
		{
			PlanarGraph.LinkResultDirectedEdges(nodes);
			EdgeRingCollection maxEdgeRings = BuildMaximalEdgeRings(dirEdges);
			EdgeRingCollection freeHoleList = new EdgeRingCollection();
			EdgeRingCollection edgeRings    = BuildMinimalEdgeRings(maxEdgeRings, 
                shellList, freeHoleList);
			SortShellsAndHoles(edgeRings, shellList, freeHoleList);
			PlaceFreeHoles(shellList, freeHoleList);
			//Assert: every hole on freeHoleList has a shell assigned to it
		}
		
        /// <summary> Checks the current set of shells (with their associated holes) to
        /// see if any of them contain the point.
        /// </summary>
        public bool ContainsPoint(Coordinate p)
        {
            for (IEdgeRingEnumerator it = shellList.GetEnumerator(); it.MoveNext(); )
            {
                EdgeRing er = it.Current;
                if (er.ContainsPoint(p))
                    return true;
            }

            return false;
        }
        
        #endregion
		
        #region Private Methods

		/// <summary> 
		/// For all DirectedEdges in result, form them into MaximalEdgeRings
		/// </summary>
		private EdgeRingCollection BuildMaximalEdgeRings(ArrayList dirEdges)
		{
			EdgeRingCollection maxEdgeRings = new EdgeRingCollection();

			for (IEnumerator it = dirEdges.GetEnumerator(); it.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) it.Current;
				if (de.InResult && de.Label.IsArea())
				{
					// if this edge has not yet been processed
					if (de.EdgeRing == null)
					{
						MaximalEdgeRing er = new MaximalEdgeRing(de, 
                            geometryFactory);

						maxEdgeRings.Add(er);
                        er.SetInResult();
					}
				}
			}
			return maxEdgeRings;
		}
		
		private EdgeRingCollection BuildMinimalEdgeRings(EdgeRingCollection maxEdgeRings, 
            EdgeRingCollection shellList, EdgeRingCollection freeHoleList)
		{
			EdgeRingCollection edgeRings = new EdgeRingCollection();

            for (IEdgeRingEnumerator it = maxEdgeRings.GetEnumerator(); it.MoveNext(); )
			{
				MaximalEdgeRing er = (MaximalEdgeRing) it.Current;
				if (er.MaxNodeDegree > 2)
				{
					er.LinkDirectedEdgesForMinimalEdgeRings();

                    EdgeRingCollection minEdgeRings = er.BuildMinimalRings();

					// at this point we can go ahead and attempt to place holes, if this EdgeRing is a polygon
					EdgeRing shell = FindShell(minEdgeRings);
					if (shell != null)
					{
						PlacePolygonHoles(shell, minEdgeRings);
						shellList.Add(shell);
					}
					else
					{
						freeHoleList.AddRange(minEdgeRings);
					}
				}
				else
				{
					edgeRings.Add(er);
				}
			}

			return edgeRings;
		}
		
		/// <summary> 
		/// This method takes a list of MinimalEdgeRings derived from a MaximalEdgeRing,
		/// and tests whether they form a Polygon.  This is the case if there is a single shell
		/// in the list.  In this case the shell is returned.
		/// The other possibility is that they are a series of connected holes, in which case
		/// no shell is returned.
		/// </summary>
		/// <returns> Returns the shell EdgeRing, if there is one, 
		/// or null, if all the rings are holes,
		/// </returns>
		private EdgeRing FindShell(EdgeRingCollection minEdgeRings)
		{
			int shellCount = 0;
			EdgeRing shell = null;

            for (IEdgeRingEnumerator it = minEdgeRings.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing er = it.Current;
				if (!er.IsHole)
				{
					shell = er;
					shellCount++;
				}
			}
			Debug.Assert(shellCount <= 1, "found two shells in MinimalEdgeRing list");

			return shell;
		}

		/// <summary> 
		/// This method assigns the holes for a <see cref="Polygon"/> (formed from a list of
		/// MinimalEdgeRings) to its shell.
		/// </summary>
		/// <remarks>
		/// Determining the holes for a MinimalEdgeRing polygon serves two purposes:
		/// <list type="number">
		/// <item>
		/// <description>
		/// it is faster than using a point-in-polygon check later on.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// it ensures correctness, since if the PIP test was used the point
		/// chosen might lie on the shell, which might return an incorrect result from the
		/// PIP test
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		private void  PlacePolygonHoles(EdgeRing shell, EdgeRingCollection minEdgeRings)
		{
			for (IEdgeRingEnumerator it = minEdgeRings.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing er = it.Current;
				if (er.IsHole)
				{
					er.Shell = shell;
				}
			}
		}

		/// <summary> For all rings in the input list,
		/// determine whether the ring is a shell or a hole
		/// and Add it to the appropriate list.
		/// Due to the way the DirectedEdges were linked,
		/// a ring is a shell if it is oriented CW, a hole otherwise.
		/// </summary>
		private void  SortShellsAndHoles(EdgeRingCollection edgeRings, 
            EdgeRingCollection shellList, EdgeRingCollection freeHoleList)
		{
			for (IEdgeRingEnumerator it = edgeRings.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing er = it.Current;
//				er.SetInResult();
				if (er.IsHole)
				{
					freeHoleList.Add(er);
				}
				else
				{
					shellList.Add(er);
				}
			}
		}

		/// <summary> 
		/// This method determines finds a containing shell for all holes
		/// which have not yet been assigned to a shell.
		/// These "free" holes should
		/// all be properly contained in their parent shells, so it is safe to use the
		/// findEdgeRingContaining method.
		/// (This is the case because any holes which are NOT
		/// properly contained (i.e. are connected to their
		/// parent shell) would have formed part of a MaximalEdgeRing
		/// and been handled in a previous step).
		/// </summary>
		private void  PlaceFreeHoles(EdgeRingCollection shellList, 
            EdgeRingCollection freeHoleList)
		{
			for (IEdgeRingEnumerator it = freeHoleList.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing hole = it.Current;

				// only place this hole if it doesn't yet have a shell
				if (hole.Shell == null)
				{
					EdgeRing shell = FindEdgeRingContaining(hole, shellList);
					Debug.Assert(shell != null, "unable to assign hole to a shell");
					hole.Shell = shell;
				}
			}
		}

		/// <summary> 
		/// Find the innermost enclosing shell EdgeRing containing the argument EdgeRing, 
		/// if any. The innermost enclosing ring is the smallest enclosing ring.
		/// The algorithm used depends on the fact that:
		/// <para>
		/// ring A contains ring B iff envelope(ring A) contains envelope(ring B)
		/// </para>
		/// This routine is only safe to use if the chosen point of the hole
		/// is known to be properly contained in a shell
		/// (which is guaranteed to be the case if the hole does not touch its shell)
		/// </summary>
		/// <returns> Returns the containing EdgeRing, if there is one or 
		/// <see langword="null"/> if no containing EdgeRing is found.
		/// </returns>
		private EdgeRing FindEdgeRingContaining(EdgeRing testEr, 
            EdgeRingCollection shellList)
		{
			LinearRing testRing = testEr.Ring;
			Envelope testEnv    = testRing.Bounds;
			Coordinate testPt   = testRing.GetCoordinate(0);
			
			EdgeRing minShell = null;
			Envelope minEnv   = null;

			for (IEdgeRingEnumerator it = shellList.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing tryShell  = it.Current;
				LinearRing tryRing = tryShell.Ring;
				Envelope tryEnv    = tryRing.Bounds;

				if (minShell != null)
					minEnv = minShell.Ring.Bounds;

				bool isContained = false;
				
                if (tryEnv.Contains(testEnv) && 
                    CGAlgorithms.IsPointInRing(testPt, tryRing.Coordinates))
					isContained = true;
				// check if this new containing ring is smaller than the current minimum ring
				
                if (isContained)
				{
					if (minShell == null || minEnv.Contains(tryEnv))
					{
						minShell = tryShell;
					}
				}
			}
			return minShell;
		}

		private GeometryList ComputePolygons(EdgeRingCollection shellList)
		{
			GeometryList resultPolyList = new GeometryList();

			// Add Polygons for all shells
			for (IEdgeRingEnumerator it = shellList.GetEnumerator(); it.MoveNext(); )
			{
				EdgeRing er  = it.Current;
				Polygon poly = er.ToPolygon(geometryFactory);
				resultPolyList.Add(poly);
			}

			return resultPolyList;
		}
        
        #endregion
	}
}