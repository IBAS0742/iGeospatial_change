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
using iGeospatial.Collections;
using iGeospatial.Collections.Sets;

using iGeospatial.Geometries.Graphs;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Operations;
using iGeospatial.Geometries.Operations.Valid;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> 
	/// Implements the algorithsm required to compute the IsValid operation 
	/// for <see cref="Geometry"/>s.
	/// </summary>
	public sealed class IsValidOp
	{                      
        #region Private Fields

        private Geometry parentGeometry; // the base Geometry to be validated
//		private bool isChecked;
		private ValidationError validErr;

        /// <summary> 
        /// If the following condition is TRUE JTS will validate inverted shells and exverted holes
        /// (the ESRI SDE model)
        /// </summary>
        private bool isSelfTouchingRingFormingHoleValid;
        
        #endregion
		
        #region Constructors and Destructor
		
        public IsValidOp(Geometry parentGeometry)
		{
            this.parentGeometry = parentGeometry;
		}
        
        #endregion

        #region Public Properties

        public ValidationError ValidationError
        {
            get
            {
                CheckValid(parentGeometry);

                return validErr;
            }
        }
        
        /// <summary> 
        /// Gets or sets whether polygons using <b>Self-Touching Rings</b> to form
        /// holes are reported as valid.
        /// </summary>
        /// <value>states whether geometry with this condition is valid
        /// </value>
        /// <remarks>
        /// If this flag is set, the following Self-Touching conditions
        /// are treated as being valid:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// The shell ring self-touches to create a hole touching the shell
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// A hole ring self-touches to create two holes touching at a point
        /// </description>
        /// </item>
        /// </list>
        /// The default (following the OGC SFS standard)
        /// is that this condition is <b>not</b> valid (<c>false</c>).
        /// <para>
        /// This does not affect whether Self-Touching Rings
        /// disconnecting the polygon interior are considered valid
        /// (these are considered to be <b>invalid</b> under the SFS, and many other
        /// spatial models as well).
        /// </para>
        /// This includes "bow-tie" shells,
        /// which self-touch at a single point causing the interior to
        /// be disconnected,
        /// and "C-shaped" holes which self-touch at a single point causing an island to be formed.
        /// </remarks>
        public bool SelfTouchingRingFormingHoleValid
        {
            get
            {
                return isSelfTouchingRingFormingHoleValid;
            }

            set
            {
                isSelfTouchingRingFormingHoleValid = value;
            }   			
        }

        #endregion
		
        #region Public Methods

		public bool IsValid()
		{                    
            validErr = null;
            CheckValid(parentGeometry);

            return (validErr == null);
		}
		
        public bool IsValid(Geometry geom)
        {                    
            validErr       = null;
            parentGeometry = geom;

            CheckValid(parentGeometry);

            return (validErr == null);
        }

        public static bool Valid(Geometry geom)
        {
            IsValidOp validator = new IsValidOp(geom);

            return validator.IsValid();
        }
        
        #endregion
		
        #region Private Methods

        /// <summary> 
        /// Checks whether a coordinate is valid for processing.
        /// Coordinates are valid iff their x and y ordinates are in the
        /// range of the floating point representation.
        /// </summary>
        /// <param name="coord">the coordinate to validate
        /// </param>
        /// <returns> <see langword="true"/> if the coordinate is valid
        /// </returns>
        private static bool IsValidCoordinate(Coordinate coord)
        {
            if (System.Double.IsNaN(coord.X))
                return false;

            if (System.Double.IsInfinity(coord.X))
                return false;
            
            if (System.Double.IsNaN(coord.Y))
                return false;
            
            if (System.Double.IsInfinity(coord.Y))
                return false;
            
            return true;
        }

		/// <summary> 
		/// Find a point from the list of testCoords
		/// that is NOT a node in the edge for the list of searchCoords
		/// </summary>
		/// <returns> the point found, or null if none found
		/// </returns>
		internal static Coordinate FindPointNotNode(ICoordinateList testCoords, 
            LinearRing searchRing, GeometryGraph graph)
		{
			// find edge corresponding to searchRing.
			Edge searchEdge = graph.FindEdge(searchRing);
			// find a point in the testCoords which is not a node of the searchRing
			EdgeIntersectionList eiList = searchEdge.EdgeIntersectionList;
			// somewhat inefficient - is there a better way? (Use a node map, for instance?)
			for (int i = 0; i < testCoords.Count; i++)
			{
				Coordinate pt = testCoords[i];
				if (!eiList.IsIntersection(pt))
					return pt;
			}
			return null;
		}
		
		private void CheckValid(Geometry g)
		{
//			if (IsChecked)
//				return;

			validErr = null;
            // empty geometries are always valid!
            if (g.IsEmpty)
				return;
            
            GeometryType geomType = g.GeometryType;

            if (geomType == GeometryType.Point)
                CheckValid((Point) g);
            else if (geomType == GeometryType.MultiPoint)
                CheckValid((MultiPoint) g);
			else if (geomType == GeometryType.LinearRing) // LineString also handles LinearRings
				CheckValid((LinearRing) g);
			else if (geomType == GeometryType.LineString)
				CheckValid((LineString) g);
			else if (geomType == GeometryType.Polygon)
				CheckValid((Polygon) g);
			else if (geomType == GeometryType.MultiPolygon)
				CheckValid((MultiPolygon) g);
            else if (geomType == GeometryType.MultiLineString || 
                geomType == GeometryType.GeometryCollection)
                CheckValid((GeometryCollection) g);
			else
				throw new NotSupportedException(g.GetType().FullName);
		}
		
        /// <summary> Checks validity of a Point.</summary>
        private void CheckValid(Point g)
        {
            CheckInvalidCoordinates(g.Coordinates);
        }

        /// <summary> Checks validity of a MultiPoint.</summary>
        private void CheckValid(MultiPoint g)
        {
            CheckInvalidCoordinates(g.Coordinates);
        }

		/// <summary> 
		/// Checks validity of a LineString.  Almost anything goes for linestrings!
		/// </summary>
		private void CheckValid(LineString g)
		{
            CheckInvalidCoordinates(g.Coordinates);
            if (validErr != null)
                return;

            GeometryGraph graph = new GeometryGraph(0, g);
			CheckTooFewPoints(graph);
		}
		
        /// <summary> Checks validity of a LinearRing.</summary>
		private void CheckValid(LinearRing g)
		{
            CheckInvalidCoordinates(g.Coordinates);
            if (validErr != null)
                return;

            CheckClosedRing(g);
            if (validErr != null)
                return;
			
            GeometryGraph graph = new GeometryGraph(0, g);
			CheckTooFewPoints(graph);
			
            if (validErr != null)
				return;

			LineIntersector li = new RobustLineIntersector();
			graph.ComputeSelfNodes(li, true);
			CheckNoSelfIntersectingRings(graph);
		}
		
		/// <summary> Checks the validity of a polygon.
		/// Sets the validErr flag.
		/// </summary>
		private void CheckValid(Polygon g)
		{
            CheckInvalidCoordinates(g);
            if (validErr != null)
                return;
            
            CheckClosedRings(g);
            if (validErr != null)
                return;
			
            GeometryGraph graph = new GeometryGraph(0, g);
			
			CheckTooFewPoints(graph);
			if (validErr != null)
				return;

			CheckConsistentArea(graph);
			if (validErr != null)
				return;
            if (!isSelfTouchingRingFormingHoleValid)
            {
                CheckNoSelfIntersectingRings(graph);
                if (validErr != null)
                    return;
            }
			CheckHolesInShell(g, graph);
			if (validErr != null)
				return ;
			//SLOWcheckHolesNotNested(g);
			CheckHolesNotNested(g, graph);
			if (validErr != null)
				return ;
			CheckConnectedInteriors(graph);
		}

		private void CheckValid(MultiPolygon g)
		{
            for (int i = 0; i < g.NumGeometries; i++)
            {
                Polygon p = (Polygon) g.GetGeometry(i);
                CheckInvalidCoordinates(p);
                if (validErr != null)
                    return ;
                
                CheckClosedRings(p);
                if (validErr != null)
                    return ;
            }
			
            GeometryGraph graph = new GeometryGraph(0, g);
			
            CheckTooFewPoints(graph);
            if (validErr != null)
                return ;
            
            CheckConsistentArea(graph);
            if (validErr != null)
                return;

            if (!isSelfTouchingRingFormingHoleValid)
            {
                CheckNoSelfIntersectingRings(graph);
                if (validErr != null)
                    return ;
            }
            
            for (int i = 0; i < g.NumGeometries; i++)
            {
                Polygon p = (Polygon) g.GetGeometry(i);
                CheckHolesInShell(p, graph);
                if (validErr != null)
                    return ;
            }

            for (int i = 0; i < g.NumGeometries; i++)
            {
                Polygon p = (Polygon) g.GetGeometry(i);
                CheckHolesNotNested(p, graph);
                if (validErr != null)
                    return ;
            }

            CheckShellsNotNested(g, graph);
            if (validErr != null)
                return;  

            CheckConnectedInteriors(graph);
        }
		
		private void CheckValid(GeometryCollection gc)
		{
			for (int i = 0; i < gc.NumGeometries; i++)
			{
				Geometry g = gc.GetGeometry(i);
				CheckValid(g);
				if (validErr != null)
					return;
			}
		}
		
        private void CheckInvalidCoordinates(Coordinate[] coords)
        {
            for (int i = 0; i < coords.Length; i++)
            {
                if (!IsValidCoordinate(coords[i]))
                {
                    validErr = new ValidationError(
                        ValidationErrorType.InvalidCoordinate, coords[i]);

                    return;
                }
            }
        }
		
        private void  CheckInvalidCoordinates(ICoordinateList coords)
        {
            for (int i = 0; i < coords.Count; i++)
            {
                if (!IsValidCoordinate(coords[i]))
                {
                    validErr = new ValidationError(
                        ValidationErrorType.InvalidCoordinate, coords[i]);

                    return;
                }
            }
        }

        private void CheckInvalidCoordinates(Polygon poly)
        {
            CheckInvalidCoordinates(poly.ExteriorRing.Coordinates);
            if (validErr != null)
                return ;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckInvalidCoordinates(poly.InteriorRing(i).Coordinates);
                if (validErr != null)
                    return;
            }
        }
		
        private void CheckClosedRings(Polygon poly)
        {
            CheckClosedRing((LinearRing) poly.ExteriorRing);
            if (validErr != null)
                return ;
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                CheckClosedRing((LinearRing)poly.InteriorRing(i));
                if (validErr != null)
                    return ;
            }
        }
		
        private void CheckClosedRing(LinearRing ring)
        {
            if (!ring.IsClosed)
                validErr = new ValidationError(
                    ValidationErrorType.RingNotClosed, ring.GetCoordinate(0));
        }
		
		private void CheckTooFewPoints(GeometryGraph graph)
		{
			if (graph.HasTooFewPoints())
			{
				validErr = new ValidationError(
                    ValidationErrorType.TooFewPoints, graph.InvalidPoint);
				return ;
			}
		}
		
        /// <summary> Checks that the arrangement of edges in a polygonal geometry graph
        /// forms a consistent area.
        /// 
        /// </summary>
        /// <param name="graph">*
        /// </param>
        /// <seealso cref="ConsistentAreaTester">
        /// </seealso>
        private void CheckConsistentArea(GeometryGraph graph)
		{
			ConsistentAreaTester cat = new ConsistentAreaTester(graph);
			bool isValidArea = cat.IsNodeConsistentArea();
			if (!isValidArea)
			{
				validErr = new ValidationError(
                    ValidationErrorType.SelfIntersection, cat.InvalidPoint);
				return ;
			}
			if (cat.HasDuplicateRings())
			{
				validErr = new ValidationError(
                    ValidationErrorType.DuplicateRings, cat.InvalidPoint);
			}
		}
		
        /// <summary> 
        /// Check that there is no ring which self-intersects (except of course at its endpoints).
        /// This is required by OGC topology rules (but not by other models
        /// such as ESRI SDE, which allow inverted shells and exverted holes).
        /// 
        /// </summary>
        /// <param name="graph">the topology graph of the geometry
        /// </param>
        private void CheckNoSelfIntersectingRings(GeometryGraph graph)
		{
			for (IEdgeEnumerator i = graph.EdgeIterator; i.MoveNext(); )
			{
				Edge e = i.Current;
				CheckNoSelfIntersectingRing(e.EdgeIntersectionList);
				if (validErr != null)
					return ;
			}
		}
		
		/// <summary> 
		/// Check that a ring does not self-intersect, except at its endpoints.
		/// Algorithm is to count the number of times each node along edge occurs.
		/// If any occur more than once, that must be a self-intersection.
		/// </summary>
		private void CheckNoSelfIntersectingRing(EdgeIntersectionList eiList)
		{
			ISet nodeSet = new HashedSet();
			bool isFirst = true;

            for (IEnumerator i = eiList.Iterator(); i.MoveNext(); )
			{
				EdgeIntersection ei = (EdgeIntersection) i.Current;
				if (isFirst)
				{
					isFirst = false;
					continue;
				}
				if (nodeSet.Contains(ei.coord))
				{
					validErr = new ValidationError(
                        ValidationErrorType.RingSelfIntersection, ei.coord);

					return;
				}
				else
				{
					nodeSet.Add(ei.coord);
				}
			}
		}
		
		/// <summary> Tests that each hole is inside the polygon shell.
		/// This routine assumes that the holes have previously been tested
		/// to ensure that all vertices lie on the shell or inside it.
		/// A simple test of a single point in the hole can be used,
		/// provide the point is chosen such that it does not lie on the
		/// boundary of the shell.
		/// 
		/// </summary>
		/// <param name="p">the polygon to be tested for hole inclusion
		/// </param>
		/// <param name="graph">a GeometryGraph incorporating the polygon
		/// </param>
		private void CheckHolesInShell(Polygon p, GeometryGraph graph)
		{
			LinearRing shell = p.ExteriorRing;
			
			IPointInRing pir = new MonotoneChainPointInRing(shell);
			
			for (int i = 0; i < p.NumInteriorRings; i++)
			{                       				
				LinearRing hole   = p.InteriorRing(i);
				Coordinate holePt = FindPointNotNode(hole.Coordinates, shell, graph);

                // If no non-node hole vertex can be found, the hole must
                // split the polygon into disconnected interiors.
                // This will be caught by a subsequent check.
                if (holePt == null) 
                    return;

				bool outside = !pir.IsInside(holePt);
				if (outside)
				{
					validErr = new ValidationError(
                        ValidationErrorType.HoleOutsideShell, holePt);

					return;
				}
			}
		}
		
		/// <summary> 
		/// Tests that no hole is nested inside another hole. This routine assumes 
		/// that the holes are Disjoint. To ensure this, holes have previously 
		/// been tested to ensure that:
		/// <list type="number">
		/// <item>
		/// <description>
		/// they do not partially overlap (checked by checkRelateConsistency)
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// they are not identical (checked by checkRelateConsistency)
		/// </description>
		/// </item>
		/// </list>
		/// </summary>
		private void CheckHolesNotNested(Polygon p, GeometryGraph graph)
		{
			QuadtreeNestedRingTester nestedTester = new QuadtreeNestedRingTester(graph);
			
			for (int i = 0; i < p.NumInteriorRings; i++)
			{
				LinearRing innerHole = p.InteriorRing(i);
				nestedTester.Add(innerHole);
			}
			bool isNonNested = nestedTester.IsNonNested();
			if (!isNonNested)
			{
				validErr = new ValidationError(
                    ValidationErrorType.NestedHoles, nestedTester.NestedPoint);
			}
		}
		
		/// <summary> 
		/// Tests that no element polygon is wholly in the interior of 
		/// another element polygon.
		/// </summary>
		/// <remarks>
		/// Preconditions:
		/// <list type="number">
		/// <item>
		/// <description>
		/// shells do not partially overlap
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// shells do not touch along an edge
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// no duplicate rings exist
		/// </description>
		/// </item>
		/// </list>
		/// This routine relies on the fact that while polygon shells may touch at one or
		/// more vertices, they cannot touch at ALL vertices.
		/// </remarks>
		private void CheckShellsNotNested(MultiPolygon mp, GeometryGraph graph)
		{
			for (int i = 0; i < mp.NumGeometries; i++)
			{
				Polygon p        = mp[i];
				LinearRing shell = p.ExteriorRing;
				for (int j = 0; j < mp.NumGeometries; j++)
				{
					if (i == j)
						continue;
					Polygon p2 = mp[j];
					CheckShellNotNested(shell, p2, graph);
					if (validErr != null)
						return ;
				}
			}
		}
		
		/// <summary> 
		/// Check if a shell is incorrectly nested Within a polygon.  This is the case
		/// if the shell is inside the polygon shell, but not inside a polygon hole.
		/// (If the shell is inside a polygon hole, the nesting is valid.)
		/// <para>
		/// The algorithm used relies on the fact that the rings must be properly contained.
		/// E.g. they cannot partially overlap (this has been previously checked by
		/// checkRelateConsistency )
		/// </para>
		/// </summary>
		private void CheckShellNotNested(LinearRing shell, Polygon p, GeometryGraph graph)
		{
			ICoordinateList shellPts = shell.Coordinates;
			// test if shell is inside polygon shell
			LinearRing polyShell    = p.ExteriorRing;
			ICoordinateList polyPts = polyShell.Coordinates;
			Coordinate shellPt = FindPointNotNode(shellPts, polyShell, graph);
			// if no point could be found, we can assume that the shell is outside the polygon
			if (shellPt == null)
				return ;
			bool insidePolyShell = CGAlgorithms.IsPointInRing(shellPt, polyPts);
			if (!insidePolyShell)
				return;
			
			// if no holes, this is an error!
			if (p.NumInteriorRings <= 0)
			{
				validErr = new ValidationError(
                    ValidationErrorType.NestedShells, shellPt);
				return;
			}

            // Check if the shell is inside one of the holes.
            // This is the case if one of the calls to checkShellInsideHole
            // returns a null coordinate.
            // Otherwise, the shell is not properly contained in a hole, which is an error.
            Coordinate badNestedPt = null;
            for (int i = 0; i < p.NumInteriorRings; i++) 
            {
                LinearRing hole = p.InteriorRing(i);
                badNestedPt     = CheckShellInsideHole(shell, hole, graph);
                if (badNestedPt == null)
                    return;
            }

            validErr = new ValidationError(
                ValidationErrorType.NestedShells, badNestedPt);
        }

        /// <summary>
        /// This routine checks to see if a shell is properly contained in a hole.
        /// It assumes that the edges of the shell and hole do not
        /// properly intersect.
        /// </summary>
        /// <param name="shell"></param>
        /// <param name="hole"></param>
        /// <param name="graph"></param>
        /// <returns>
        /// null if the shell is properly contained, or a Coordinate which 
        /// is not inside the hole if it is not
        /// </returns>
        private Coordinate CheckShellInsideHole(LinearRing shell, LinearRing hole, GeometryGraph graph)
        {
            ICoordinateList shellPts = shell.Coordinates;
            ICoordinateList holePts  = hole.Coordinates;
            // TODO: improve performance of this - by sorting pointlists for instance?
            Coordinate shellPt = FindPointNotNode(shellPts, hole, graph);
            // if point is on shell but not hole, check that the shell is inside the hole
            if (shellPt != null) 
            {
                bool insideHole = CGAlgorithms.IsPointInRing(shellPt, holePts);
                if (! insideHole) 
                {
                    return shellPt;
                }
            }

            Coordinate holePt = FindPointNotNode(holePts, shell, graph);
            // if point is on hole but not shell, check that the hole is outside the shell
            if (holePt != null) 
            {
                bool insideShell = CGAlgorithms.IsPointInRing(holePt, shellPts);
                if (insideShell) 
                {
                    return holePt;
                }

                return null;
            }
			
            Debug.Assert(false, "Should never reach here: points in shell and hole appear to be equal");
            
            return null;
        }
		
		private void CheckConnectedInteriors(GeometryGraph graph)
		{
			ConnectedInteriorTester cit = new ConnectedInteriorTester(graph);
			if (!cit.IsInteriorsConnected())
				validErr = new ValidationError(
                    ValidationErrorType.DisconnectedInterior, cit.Coordinate);
		}
        
        #endregion
	}
}