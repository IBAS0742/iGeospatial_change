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
using iGeospatial.Geometries.Noding;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Operations.Buffer
{
	/// <summary> 
	/// Creates all the raw offset curves for a Buffer of a Geometry.
	/// Raw curves need to be noded together and polygonized to form the final Buffer area.
	/// </summary>
	internal class OffsetCurveSetBuilder
	{
        #region Private Members

        private Geometry inputGeom;
        private double m_dDistance;
        private OffsetCurveBuilder curveBuilder;
		
        private ArrayList curveList;
		
        #endregion

        #region Constructors and Destructor

        public OffsetCurveSetBuilder(Geometry inputGeom, 
            double distance, OffsetCurveBuilder curveBuilder)
        {
            curveList = new ArrayList();

            this.inputGeom    = inputGeom;
            m_dDistance       = distance;
            this.curveBuilder = curveBuilder;
        }

        #endregion
		
        #region Public Properties

		/// <summary> Computes the set of raw offset curves for the Buffer.
		/// Each offset curve has an attached {@link Label} indicating
		/// its left and right location.
		/// </summary>
		/// <returns> a Collection of SegmentStrings representing the raw Buffer curves
		/// </returns>
		public ArrayList Curves
		{
			get
			{
				Add(inputGeom);
				return curveList;
			}
		}
        
        #endregion
		
        #region Private Methods

		private void AddCurves(IList lineList, int leftLoc, int rightLoc)
		{
			for (IEnumerator i = lineList.GetEnumerator(); i.MoveNext(); )
			{
				ICoordinateList coords = (ICoordinateList) i.Current;
				AddCurve(coords, leftLoc, rightLoc);
			}
		}
		
		/// <summary> 
		/// Creates a <see cref="SegmentString"/> for a coordinate list which is a raw offset curve,
		/// and adds it to the list of Buffer curves.
		/// The SegmentString is tagged with a Label giving the topology of the curve.
		/// The curve may be oriented in either direction.
		/// If the curve is oriented CW, the locations will be:
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// Left: LocationType.Exterior
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Right: LocationType.Interior
		/// </description>
		/// </item>
		/// </list>
		/// </summary>
		private void AddCurve(Coordinate[] coord, int leftLoc, int rightLoc)
		{
			// don't Add null curves!
			if (coord.Length < 2)
				return;

			// Add the edge for a coordinate list which is a raw offset curve
			SegmentString e = new SegmentString(new CoordinateCollection(coord), 
                new Label(0, LocationType.Boundary, leftLoc, rightLoc));

			curveList.Add(e);
		}
		
		private void AddCurve(ICoordinateList coords, int leftLoc, int rightLoc)
		{
			// don't Add null curves!
			if (coords.Count < 2)
				return;

			// Add the edge for a coordinate list which is a raw offset curve
			SegmentString e = new SegmentString(coords, 
                new Label(0, LocationType.Boundary, leftLoc, rightLoc));

			curveList.Add(e);
		}
		
		
		private void Add(Geometry g)
		{
			if (g.IsEmpty)
				return;
			
            GeometryType geomType = g.GeometryType;

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
				throw new System.NotSupportedException(g.GetType().FullName);
		}

		private void AddCollection(GeometryCollection gc)
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
			if (m_dDistance <= 0.0)
				return ;

			ICoordinateList coord = p.Coordinates;
			IList lineList = curveBuilder.GetLineCurve(coord, m_dDistance);
			AddCurves(lineList, LocationType.Exterior, LocationType.Interior);
		}

		private void  AddLineString(LineString line)
		{
			if (m_dDistance <= 0.0)
				return ;
			ICoordinateList coord = CoordinateCollection.RemoveRepeatedCoordinates(line.Coordinates);
			IList lineList = curveBuilder.GetLineCurve(coord, m_dDistance);
			AddCurves(lineList, LocationType.Exterior, LocationType.Interior);
		}
		
		private void AddPolygon(Polygon p)
		{
			double offsetDistance = m_dDistance;
			int offsetSide = Position.Left;
			if (m_dDistance < 0.0)
			{
				offsetDistance = -m_dDistance;
				offsetSide = Position.Right;
			}
			
			LinearRing shell = p.ExteriorRing;
			ICoordinateList shellCoord = 
                CoordinateCollection.RemoveRepeatedCoordinates(shell.Coordinates);

			// optimization - don't bother computing Buffer
			// if the polygon would be completely eroded
			if (m_dDistance < 0.0 && IsErodedCompletely(shell, m_dDistance))
				return ;
			
			AddPolygonRing(shellCoord, offsetDistance, 
                offsetSide, LocationType.Exterior, LocationType.Interior);
			
			for (int i = 0; i < p.NumInteriorRings; i++)
			{
				
				LinearRing hole = (LinearRing) p.InteriorRing(i);
				ICoordinateList holeCoord = 
                    CoordinateCollection.RemoveRepeatedCoordinates(hole.Coordinates);
				
				// optimization - don't bother computing Buffer for this hole
				// if the hole would be completely covered
				if (m_dDistance > 0.0 && IsErodedCompletely(hole, -m_dDistance))
					continue;
				
				// Holes are topologically labelled opposite to the shell, since
				// the interior of the polygon lies on their opposite side
				// (on the left, if the hole is oriented CCW)
				AddPolygonRing(holeCoord, offsetDistance, 
                    Position.Opposite(offsetSide), LocationType.Interior, LocationType.Exterior);
			}
		}

		/// <summary> Add an offset curve for a ring.
		/// The side and left and right topological location arguments
		/// assume that the ring is oriented CW.
		/// If the ring is in the opposite orientation,
		/// the left and right locations must be interchanged and the side flipped.
		/// 
		/// </summary>
		/// <param name="coord">the coordinates of the ring (must not contain repeated points)
		/// </param>
		/// <param name="offsetDistance">the distance at which to create the Buffer
		/// </param>
		/// <param name="side">the side of the ring on which to construct the Buffer line
		/// </param>
		/// <param name="cwLeftLoc">the location on the L side of the ring (if it is CW)
		/// </param>
		/// <param name="cwRightLoc">the location on the R side of the ring (if it is CW)
		/// </param>
		private void AddPolygonRing(ICoordinateList coord, double offsetDistance, int side, int cwLeftLoc, int cwRightLoc)
		{
			int leftLoc = cwLeftLoc;
			int rightLoc = cwRightLoc;
			if (CGAlgorithms.IsCCW(coord))
			{
				leftLoc = cwRightLoc;
				rightLoc = cwLeftLoc;
				side = Position.Opposite(side);
			}

            IList lineList = curveBuilder.GetRingCurve(coord, side, offsetDistance);
			AddCurves(lineList, leftLoc, rightLoc);
		}
		
		/// <summary> The ringCoord is assumed to contain no repeated points.
		/// It may be degenerate (i.e. contain only 1, 2, or 3 points).
		/// In this case it has no area, and hence has a minimum diameter of 0.
		/// 
		/// </summary>
		/// <param name="">ringCoord
		/// </param>
		/// <param name="">offsetDistance
		/// </param>
		/// <returns>
		/// </returns>
		private bool IsErodedCompletely(LinearRing ring, double bufferDistance)
		{                     
            ICoordinateList ringCoord = ring.Coordinates;

//			double minDiam = 0.0;
			// degenerate ring has no area
			if (ringCoord.Count < 4)
				return bufferDistance < 0;
			
			// important test to eliminate inverted triangle bug
			// also optimizes erosion test for triangles
			if (ringCoord.Count == 4)
				return IsTriangleErodedCompletely(ringCoord, bufferDistance);
			
            // if envelope is narrower than twice the buffer distance, ring is eroded
            Envelope env = ring.Bounds;
            double envMinDimension = Math.Min(env.Height, env.Width);
            if (bufferDistance < 0.0 && 2 * Math.Abs(bufferDistance) > 
                envMinDimension)
                return true;
			
            return false;
			
//			// The following is a heuristic test to determine whether an
//			// inside Buffer will be eroded completely.
//			// It is based on the fact that the minimum diameter of the ring pointset
//			// provides an upper bound on the Buffer distance which would erode the
//			// ring.
//			// If the Buffer distance is less than the minimum diameter, the ring
//			// may still be eroded, but this will be determined by
//			// a full topological computation.
//			LinearRing ring    = inputGeom.Factory.CreateLinearRing(ringCoord);
//			MinimumDiameter md = new MinimumDiameter(ring);
//			minDiam            = md.Length;
//
//			return minDiam < 2 * Math.Abs(bufferDistance);
		}
		
		/// <summary> Tests whether a triangular ring would be eroded completely by the given
		/// Buffer distance.
		/// This is a precise test.  It uses the fact that the inner Buffer of a
		/// triangle converges on the inCentre of the triangle (the point
		/// equidistant from all sides).  If the Buffer distance is greater than the
		/// distance of the inCentre from a side, the triangle will be eroded completely.
		/// 
		/// This test is important, since it removes a problematic case where
		/// the Buffer distance is slightly larger than the inCentre distance.
		/// In this case the triangle Buffer curve "inverts" with incorrect topology,
		/// producing an incorrect hole in the Buffer.
		/// 
		/// </summary>
		/// <param name="">triangleCoord
		/// </param>
		/// <param name="">bufferDistance
		/// </param>
		/// <returns>
		/// </returns>
		private bool IsTriangleErodedCompletely(ICoordinateList triangleCoord, 
            double bufferDistance)
		{
			Triangle tri = new Triangle(triangleCoord[0], 
                triangleCoord[1], triangleCoord[2], inputGeom.Factory);
			Coordinate inCentre = tri.InCentre();
			double distToCentre = CGAlgorithms.DistancePointLine(inCentre, tri.P1, tri.P2);

			return distToCentre < Math.Abs(bufferDistance);
		}
        
        #endregion
    }
}