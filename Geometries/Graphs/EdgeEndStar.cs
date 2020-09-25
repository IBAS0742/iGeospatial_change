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

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A EdgeEndStar is an ordered list of EdgeEnds around a node.
	/// They are maintained in CCW order (starting with the positive x-axis) around the node
	/// for efficient lookup and topology building.
	/// </summary>
	[Serializable]
    internal abstract class EdgeEndStar
	{
		/// <summary> A map which maintains the edges in sorted order around the node</summary>
		internal IDictionary edgeMap;
		
        /// <summary> A list of all outgoing edges in the result, in CCW order</summary>
		internal ArrayList edgeList;
		/// <summary> The location of the point for this star in Geometry i Areas</summary>

        private int[] ptInAreaLocation;
		
		public EdgeEndStar()
		{
            edgeMap = new SortedList();

            ptInAreaLocation = new int[]{LocationType.None, LocationType.None};
        }
		
		/// <returns> the coordinate for the node this star is based at
		/// </returns>
		public Coordinate Coordinate
		{
			get
			{
				IEnumerator it = Iterator();

                if (!it.MoveNext())
					return null;

                EdgeEnd e = (EdgeEnd) it.Current;
				return e.Coordinate;
			}
		}
		
        public int Degree
		{
			get
			{
				return edgeMap.Count;
			}
		}

		public IList Edges
		{
			get
			{
				if (edgeList == null)
				{
					edgeList = new ArrayList(edgeMap.Values);
				}
				return edgeList;
			}
		}

		public bool AreaLabelsConsistent
		{
			get
			{
				ComputeEdgeEndLabels();
				return CheckAreaLabelsConsistent(0);
			}
		}
		
		/// <summary> Insert a EdgeEnd into this EdgeEndStar</summary>
		public abstract void Insert(EdgeEnd e);
		
		/// <summary> Insert an EdgeEnd into the map, and clear the edgeList cache,
		/// since the list of edges has now changed
		/// </summary>
		protected virtual void InsertEdgeEnd(EdgeEnd e, object obj)
		{
//			object tempObject;
//			tempObject = obj;
//			edgeMap[e] = tempObject;
//			object generatedAux = tempObject;
			edgeMap[e] = obj;
			edgeList = null; // edge list has changed - clear the cache
		}
		
		/// <summary> Iterator access to the ordered list of edges is optimized by
		/// copying the map collection to a list.  (This assumes that
		/// once an iterator is requested, it is likely that insertion into
		/// the map is complete).
		/// </summary>
		public IEnumerator Iterator()
		{
			return Edges.GetEnumerator();
		}

		public EdgeEnd GetNextCW(EdgeEnd ee)
		{
			IList generatedAux = Edges;
            if (generatedAux == null)
            {
                return null;
            }

            int i = edgeList.IndexOf(ee);
			int iNextCW = i - 1;
			if (i == 0)
				iNextCW = edgeList.Count - 1;

			return (EdgeEnd) edgeList[iNextCW];
		}
		
		public virtual void ComputeLabelling(GeometryGraph[] geom)
		{
			ComputeEdgeEndLabels();
			// Propagate side labels  around the edges in the star
			// for each parent Geometry

            PropagateSideLabels(0);

            PropagateSideLabels(1);
			
			// If there are edges that still have null labels for a geometry
			// this must be because there are no area edges for that geometry incident on this node.
			// In this case, to label the edge for that geometry we must test whether the
			// edge is in the interior of the geometry.
			// To do this it suffices to determine whether the node for the edge is in the interior of an area.
			// If so, the edge has location INTERIOR for the geometry.
			// In all other cases (e.g. the node is on a line, on a point, or not on the geometry at all) the edge
			// has the location EXTERIOR for the geometry.
			// <p>
			// Note that the edge cannot be on the BOUNDARY of the geometry, since then
			// there would have been a parallel edge from the Geometry at this node also labelled BOUNDARY
			// and this edge would have been labelled in the previous step.
			// <p>
			// This code causes a problem when dimensional collapses are present, since it may try and
			// determine the location of a node where a dimensional collapse has occurred.
			// The point should be considered to be on the EXTERIOR
			// of the polygon, but Locate() will return INTERIOR, since it is passed
			// the original Geometry, not the collapsed version.
			// 
			// If there are incident edges which are Line edges labelled BOUNDARY,
			// then they must be edges resulting from dimensional collapses.
			// In this case the other edges can be labelled EXTERIOR for this Geometry.
			// 
			// MD 8/11/01 - NOT TRUE!  The collapsed edges may in fact be in the interior of the Geometry,
			// which means the other edges should be labelled INTERIOR for this Geometry.
			// Not sure how solve this...  Possibly labelling needs to be split into several phases:
			// area label propagation, symLabel merging, then finally null label resolution.
			bool[] hasDimensionalCollapseEdge = new bool[]{false, false};

			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				Label label = e.Label;
				for (int geomi = 0; geomi < 2; geomi++)
				{
					if (label.IsLine(geomi) && label.GetLocation(geomi) == LocationType.Boundary)
						hasDimensionalCollapseEdge[geomi] = true;
				}
			}

			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				Label label = e.Label;

                for (int geomi = 0; geomi < 2; geomi++)
				{
					if (label.IsAnyNull(geomi))
					{
						int loc = LocationType.None;
						if (hasDimensionalCollapseEdge[geomi])
						{
							loc = LocationType.Exterior;
						}
						else
						{
							Coordinate p = e.Coordinate;
							loc = GetLocation(geomi, p, geom);
						}
						label.SetAllLocationsIfNull(geomi, loc);
					}
				}
			}
		}
		
		private void ComputeEdgeEndLabels()
		{
			// Compute edge label for each EdgeEnd
			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd ee = (EdgeEnd) it.Current;
				ee.ComputeLabel();
			}
		}

		internal virtual int GetLocation(int geomIndex, Coordinate p, GeometryGraph[] geom)
		{
			// compute location only on demand
			if (ptInAreaLocation[geomIndex] == LocationType.None)
			{
				ptInAreaLocation[geomIndex] = SimplePointInAreaLocator.Locate(p, geom[geomIndex].Geometry);
			}
			return ptInAreaLocation[geomIndex];
		}
		
		private bool CheckAreaLabelsConsistent(int geomIndex)
		{
			// Since edges are stored in CCW order around the node,
			// As we move around the ring we move from the right to the left side of the edge
			IList edges = this.Edges;
			// if no edges, trivially consistent
			if (edges.Count <= 0)
				return true;

			// initialize startLoc to location of last L side (if any)
			int lastEdgeIndex = edges.Count - 1;
			Label startLabel = ((EdgeEnd) edges[lastEdgeIndex]).Label;
			int startLoc = startLabel.GetLocation(geomIndex, Position.Left);
			Debug.Assert(startLoc != LocationType.None, "Found unlabelled area edge");
			
			int currLoc = startLoc;

            for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				Label label = e.Label;
				// we assume that we are only checking a area
				Debug.Assert(label.IsArea(geomIndex), "Found non-area edge");
				int leftLoc = label.GetLocation(geomIndex, Position.Left);
				int rightLoc = label.GetLocation(geomIndex, Position.Right);

				// check that edge is really a boundary between inside and outside!
				if (leftLoc == rightLoc)
				{
					return false;
				}
				// check side location conflict
				//Assert.isTrue(rightLoc == currLoc, "side location conflict " + locStr);
				if (rightLoc != currLoc)
				{
					return false;
				}
				currLoc = leftLoc;
			}
			return true;
		}

		internal void PropagateSideLabels(int geomIndex)
		{
			// Since edges are stored in CCW order around the node,
			// As we move around the ring we move from the right to the left side of the edge
			int startLoc = LocationType.None;

			// initialize loc to location of last L side (if any)
			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				Label label = e.Label;
				if (label.IsArea(geomIndex) && label.GetLocation(geomIndex, Position.Left) != LocationType.None)
					startLoc = label.GetLocation(geomIndex, Position.Left);
			}

			// no labelled sides found, so no labels to propagate
			if (startLoc == LocationType.None)
				return ;
			
			int currLoc = startLoc;

			for (IEnumerator it = Iterator(); it.MoveNext(); )
			{
				EdgeEnd e = (EdgeEnd) it.Current;
				Label label = e.Label;
				// set null ON values to be in current location
				if (label.GetLocation(geomIndex, Position.On) == LocationType.None)
					label.SetLocation(geomIndex, Position.On, currLoc);
				// set side labels (if any)
				// if (label.IsArea()) {   //ORIGINAL
				if (label.IsArea(geomIndex))
				{
					int leftLoc = label.GetLocation(geomIndex, Position.Left);
					int rightLoc = label.GetLocation(geomIndex, Position.Right);
					// if there is a right location, that is the next location to propagate
					if (rightLoc != LocationType.None)
					{
						//Debug.Print(rightLoc != currLoc, this);
						if (rightLoc != currLoc)
							throw new GeometryException("side location conflict", e.Coordinate);
						if (leftLoc == LocationType.None)
						{
							Debug.Assert(false, "Should never reach here: found single null side (at " + e.Coordinate + ")");
						}
						currLoc = leftLoc;
					}
					else
					{
						/// <summary>RHS is null - LHS must be null too.
						/// This must be an edge from the other geometry, which has no location
						/// labelling for this geometry.  This edge must lie wholly inside or outside
						/// the other geometry (which is determined by the current location).
						/// Assign both sides to be the current location.
						/// </summary>
						Debug.Assert(label.GetLocation(geomIndex, Position.Left) == LocationType.None, "found single null side");
						label.SetLocation(geomIndex, Position.Right, currLoc);
						label.SetLocation(geomIndex, Position.Left, currLoc);
					}
				}
			}
		}
		
		public int FindIndex(EdgeEnd eSearch)
		{
			Iterator(); // force edgelist to be computed

			for (int i = 0; i < edgeList.Count; i++)
			{
				EdgeEnd e = (EdgeEnd) edgeList[i];
				if (e == eSearch)
					return i;
			}
			return - 1;
		}
	}
}