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
using iGeospatial.Geometries.Graphs.Index;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A GeometryGraph is a graph that models a given Geometry
	/// </summary>
    [Serializable]
    internal sealed class GeometryGraph : PlanarGraph
	{
        #region Private Fields

		private Geometry parentGeom;

		// the precision model of the Geometry represented by this graph
		//private PrecisionModel precisionModel = null;
		//private int SRID;
		/// <summary> The lineEdgeMap is a map of the linestring components of the
		/// parentGeometry to the edges which are derived from them.
		/// This is used to efficiently perform FindEdge queries
		/// </summary>
		private IDictionary lineEdgeMap;
		
		//private PrecisionModel newPM = null;
		/// <summary> If this flag is true, the Boundary Determination Rule 
		/// will used when deciding
		/// whether nodes are in the boundary or not
		/// </summary>
		private bool useBoundaryDeterminationRule;

        /// <summary>
        /// the index of this geometry as an argument to a spatial function 
        /// (used for labelling)
        /// </summary>
		private int argIndex; 

		private NodeCollection boundaryNodes;
		
        private bool m_bHasTooFewPoints;
		
        private Coordinate invalidPoint;
        
        #endregion
		
        #region Constructors and Destructor
        
        public GeometryGraph(int argIndex, Geometry parentGeom)
        {
            lineEdgeMap = new Hashtable();

            this.argIndex = argIndex;
            this.parentGeom = parentGeom;
			
            if (parentGeom != null)
            {
                Add(parentGeom);
            }
        }
        
        #endregion
		
        #region Public Properties

		public Coordinate InvalidPoint
		{
			get
			{
				return invalidPoint;
			}
			
		}

		public Geometry Geometry
		{
			get
			{
				return parentGeom;
			}
			
		}

		public NodeCollection BoundaryNodes
		{
			get
			{
				if (boundaryNodes == null)
					boundaryNodes = m_objNodes.GetBoundaryNodes(argIndex);

				return boundaryNodes;
			}
			
		}
        
        #endregion

        #region Public Methods

        public Coordinate[] GetBoundaryPoints()
        {
            NodeCollection coll   = BoundaryNodes;
            Coordinate[] pts = new Coordinate[coll.Count];
            int i = 0;

            for (INodeEnumerator it = coll.GetEnumerator(); it.MoveNext(); )
            {
                Node node = it.Current;
                pts[i++] = node.Coordinate.Clone();
            }

            return pts;
        }
		
        public bool HasTooFewPoints()
        {
            return m_bHasTooFewPoints;
        }
		
        public Edge FindEdge(LineString line)
        {
            return (Edge) lineEdgeMap[line];
        }
		
        public void ComputeSplitEdges(EdgeCollection edgelist)
        {
            for (IEdgeEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
            {
                Edge e = i.Current;
                e.eiList.AddSplitEdges(edgelist);
            }
        }
		
		/// <summary> Add an Edge computed externally.  The label on the Edge is assumed
		/// to be correct.
		/// </summary>
		public void  AddEdge(Edge e)
		{
			InsertEdge(e);
			ICoordinateList coord = e.Coordinates;
			// insert the endpoint as a node, to mark that it is on the boundary
			InsertPoint(argIndex, coord[0], LocationType.Boundary);
			InsertPoint(argIndex, coord[coord.Count - 1], LocationType.Boundary);
		}
		
		/// <summary> 
		/// Add a point computed externally.  The point is assumed to be a
		/// Point Geometry part, which has a location of INTERIOR.
		/// </summary>
		public void  AddPoint(Coordinate pt)
		{
			InsertPoint(argIndex, pt, LocationType.Interior);
		}
		
		/// <summary> 
		/// Compute self-nodes, taking advantage of the Geometry type to
		/// minimize the number of intersection tests.  (E.g. rings are
		/// not tested for self-intersection, since they are assumed to be valid).
		/// </summary>
		/// <param name="li">the LineIntersector to use
		/// </param>
		/// <param name="computeRingSelfNodes">
		/// if false, intersection checks are optimized to not test rings 
		/// for self-intersection
		/// </param>
		/// <returns> 
		/// The SegmentIntersector used, containing information about the intersections found
		/// </returns>
		public SegmentIntersector ComputeSelfNodes(LineIntersector li, bool computeRingSelfNodes)
		{
			SegmentIntersector si = new SegmentIntersector(li, true, false);
			EdgeSetIntersector esi = CreateEdgeSetIntersector();
			// optimized test for Polygons and Rings
            GeometryType geomType = parentGeom.GeometryType;

            if (!computeRingSelfNodes && 
                (geomType == GeometryType.LinearRing || 
                geomType == GeometryType.Polygon     || 
                geomType == GeometryType.MultiPolygon))
			{
				esi.ComputeIntersections(edges, si, false);
			}
			else
			{
				esi.ComputeIntersections(edges, si, true);
			}

            AddSelfIntersectionNodes(argIndex);

			return si;
		}
		
		public SegmentIntersector ComputeEdgeIntersections(GeometryGraph g, 
            LineIntersector li, bool includeProper)
		{
			SegmentIntersector si = new SegmentIntersector(li, includeProper, true);
			si.SetBoundaryNodes(this.BoundaryNodes, g.BoundaryNodes);
			
			EdgeSetIntersector esi = CreateEdgeSetIntersector();
			esi.ComputeIntersections(edges, g.edges, si);

			return si;
		}
        
        #endregion

        #region Public Static Methods

		/// <summary> 
		/// This method implements the Boundary Determination Rule for determining 
		/// whether a component (node or edge) that appears multiple times in elements
		/// of a MultiGeometry is in the boundary or the interior of the Geometry
		/// <para>
		/// The SFS uses the "Mod-2 Rule", which this function implements
		/// </para>
		/// An alternative (and possibly more intuitive) rule would be
		/// the "At Most One Rule":
		/// IsInBoundary = (componentCount == 1)
		/// </summary>
		public static bool IsInBoundary(int boundaryCount)
		{
			// the "Mod-2 Rule"
			return boundaryCount % 2 == 1;
		}

		public static int DetermineBoundary(int boundaryCount)
		{
			return IsInBoundary(boundaryCount) ? LocationType.Boundary : LocationType.Interior;
		}
        
        #endregion
		
        #region Private Methods

		private EdgeSetIntersector CreateEdgeSetIntersector()
		{
			// various options for computing intersections, from slowest to fastest
			return new SimpleMCSweepLineIntersector();
		}

		private void  Add(Geometry g)
		{
			if (g.IsEmpty)
				return;
			
            GeometryType geomType = g.GeometryType;

            // check if this Geometry should obey the Boundary Determination Rule
			// all collections except MultiPolygons obey the rule
//			if (geomType == GeometryType.GeometryCollection && 
//                !(geomType == GeometryType.MultiPolygon))
//				useBoundaryDeterminationRule = true;
			if (g.IsCollection && !(geomType == GeometryType.MultiPolygon))
				useBoundaryDeterminationRule = true;
			
			if (geomType == GeometryType.Polygon)
				AddPolygon((Polygon) g);
			// LineString also handles LinearRings
			else if (geomType == GeometryType.LineString || 
                geomType == GeometryType.LinearRing)
				AddLineString((LineString) g);
			else if (geomType == GeometryType.Point)
				AddPoint((Point) g);
			else if (geomType == GeometryType.MultiPoint)
				AddCollection((MultiPoint) g);
			else if (geomType == GeometryType.MultiLineString)
				AddCollection((MultiLineString) g);
			else if (geomType == GeometryType.MultiPolygon)
				AddCollection((MultiPolygon) g);
			else if (geomType == GeometryType.GeometryCollection)
				AddCollection((GeometryCollection) g);
			else
				throw new System.NotSupportedException(g.Name);
		}
		
		private void  AddCollection(GeometryCollection gc)
		{
            int nCount = gc.NumGeometries;

			for (int i = 0; i < nCount; i++)
			{
				Geometry g = gc.GetGeometry(i);
				Add(g);
			}
		}

		/// <summary> Add a Point to the graph.</summary>
		private void  AddPoint(Point p)
		{
			Coordinate coord = p.Coordinate;
			InsertPoint(argIndex, coord, LocationType.Interior);
		}

		/// <summary> 
		/// The left and right topological location arguments assume that the ring is oriented CW.
		/// If the ring is in the opposite orientation,
		/// the left and right locations must be interchanged.
		/// </summary>
		private void  AddPolygonRing(LinearRing lr, int cwLeft, int cwRight)
		{
			ICoordinateList coord = CoordinateCollection.RemoveRepeatedCoordinates(lr.Coordinates);
			
			if (coord.Count < 4)
			{
				m_bHasTooFewPoints = true;
				invalidPoint = coord[0];
				return ;
			}
			
			int left = cwLeft;
			int right = cwRight;
			if (CGAlgorithms.IsCCW(coord))
			{
				left = cwRight;
				right = cwLeft;
			}
			Edge e = new Edge(coord, new Label(argIndex, LocationType.Boundary, left, right));
//			object tempObject;
//			tempObject = e;
//			lineEdgeMap[lr] = tempObject;
//			object generatedAux = tempObject;
			lineEdgeMap[lr] = e;
			
			InsertEdge(e);
			// insert the endpoint as a node, to mark that it is on the boundary
			InsertPoint(argIndex, coord[0], LocationType.Boundary);
		}
		
		private void AddPolygon(Polygon p)
		{
			AddPolygonRing((LinearRing) p.ExteriorRing, 
                LocationType.Exterior, LocationType.Interior);

            int nCount = p.NumInteriorRings;
			
			for (int i = 0; i < nCount; i++)
			{
				// Holes are topologically labelled opposite to the shell, since
				// the interior of the polygon lies on their opposite side
				// (on the left, if the hole is oriented CW)
				AddPolygonRing((LinearRing) p.InteriorRing(i), LocationType.Interior, LocationType.Exterior);
			}
		}
		
		private void AddLineString(LineString line)
		{
			ICoordinateList coord = CoordinateCollection.RemoveRepeatedCoordinates(line.Coordinates);
			
			if (coord.Count < 2)
			{
				m_bHasTooFewPoints = true;
				invalidPoint       = coord[0];
				return ;
			}
			
			// Add the edge for the LineString
			// line edges do not have locations for their left and right sides
			Edge e = new Edge(coord, new Label(argIndex, LocationType.Interior));
//			object tempObject;
//			tempObject = e;
			lineEdgeMap[line] = e;
//			lineEdgeMap[line] = tempObject;
//			object generatedAux = tempObject;

			InsertEdge(e);
			
            // Add the boundary points of the LineString, if any.
			// Even if the LineString is closed, Add both points as if they were endpoints.
			// This allows for the case that the node already exists and is a boundary point.
			Debug.Assert(coord.Count >= 2, "found LineString with single point");
			InsertBoundaryPoint(argIndex, coord[0]);
			InsertBoundaryPoint(argIndex, coord[coord.Count - 1]);
		}
		
		private void InsertPoint(int argIndex, Coordinate coord, int onLocation)
		{
			Node n    = m_objNodes.AddNode(coord);
			Label lbl = n.Label;

			if (lbl == null)
			{
				n.m_objLabel = new Label(argIndex, onLocation);
			}
			else
            {
                lbl.SetLocation(argIndex, onLocation);
            }
		}
		
		/// <summary> 
		/// Adds points using the mod-2 rule of SFS.  This is used to Add the boundary
		/// points of dim-1 geometries (Curves/MultiCurves).  According to the SFS,
		/// an endpoint of a Curve is on the boundary
		/// iff if it is in the boundaries of an odd number of Geometries
		/// </summary>
		private void InsertBoundaryPoint(int argIndex, Coordinate coord)
		{
			Node n = m_objNodes.AddNode(coord);
			Label lbl = n.Label;
			// the new point to insert is on a boundary
			int boundaryCount = 1;
			// determine the current location for the point (if any)
			int loc = LocationType.None;
			if (lbl != null)
				loc = lbl.GetLocation(argIndex, Position.On);
			if (loc == LocationType.Boundary)
				boundaryCount++;
			
			// determine the boundary status of the point according to the Boundary Determination Rule
			int newLoc = DetermineBoundary(boundaryCount);
			lbl.SetLocation(argIndex, newLoc);
		}
		
		private void AddSelfIntersectionNodes(int argIndex)
		{
			for (IEdgeEnumerator i = edges.GetEnumerator(); i.MoveNext(); )
			{
				Edge e = i.Current;
				int eLoc = e.Label.GetLocation(argIndex);

                for (IEnumerator eiIt = e.eiList.Iterator(); eiIt.MoveNext(); )
				{
					EdgeIntersection ei = (EdgeIntersection) eiIt.Current;
					AddSelfIntersectionNode(argIndex, ei.coord, eLoc);
				}
			}
		}

		/// <summary> 
		/// Add a node for a self-intersection.
		/// If the node is a potential boundary node (e.g. came from an edge which
		/// is a boundary) then insert it as a potential boundary node.
		/// Otherwise, just Add it as a regular node.
		/// </summary>
		private void  AddSelfIntersectionNode(int argIndex, Coordinate coord, int loc)
		{
			// if this node is already a boundary node, don't change it
			if (IsBoundaryNode(argIndex, coord))
				return;

			if (loc == LocationType.Boundary && useBoundaryDeterminationRule)
				InsertBoundaryPoint(argIndex, coord);
			else
				InsertPoint(argIndex, coord, loc);
		}
        
        #endregion
	}
}