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

namespace iGeospatial.Geometries.Operations.Buffer
{
	/// <summary> Computes the raw offset curve for a
	/// single Geometry component (ring, line or point).
	/// </summary>
	/// <remarks>
	/// A raw offset curve line is not noded - it may contain self-intersections 
	/// (and usually will).
	/// <para>
	/// The final Buffer polygon is computed by forming a topological graph
	/// of all the noded raw curves and tracing outside contours.
	/// The points in the raw curve are rounded to the required precision model.
	/// </para>
	/// </remarks>
    [Serializable]
    internal class OffsetCurveBuilder
	{
        #region Private Fields

		/// <summary> 
		/// The default number of facets into which to divide a fillet of 90 degrees.
		/// A value of 8 gives less than 2% max error in the buffer distance.
		/// For a max error of less than 1%, use QS = 12
		/// </summary>
		public const int DefaultQuadrantSegments = 8;
		
		private LineIntersector li;
		
		/// <summary> The angle quantum with which to approximate a fillet curve
		/// (based on the input number of quadrant segments)
		/// </summary>
		private double filletAngleQuantum;

		/// <summary> 
		/// The max error of approximation between a quad segment and the 
		/// true fillet curve.
		/// </summary>
        private double maxCurveSegmentError;
		private ICoordinateList ptList;
		private double m_dDistance;
		private PrecisionModel precisionModel;
		private BufferCapType endCapStyle;
		
		private Coordinate s0, s1, s2;
		private LineSegment seg0;
		private LineSegment seg1;
		private LineSegment offset0;
		private LineSegment offset1;
		private int side;
        
        #endregion
		
        #region Constructors and Destructor

		public OffsetCurveBuilder(PrecisionModel precisionModel) : 
            this(precisionModel, DefaultQuadrantSegments)
		{
		}
		
		public OffsetCurveBuilder(PrecisionModel precisionModel, int quadrantSegments)
		{
            GeometryFactory factory = new GeometryFactory(precisionModel);
            endCapStyle = BufferCapType.Round;
            seg0 = new LineSegment(factory);
            seg1 = new LineSegment(factory);
            offset0 = new LineSegment(factory);
            offset1 = new LineSegment(factory);

			this.precisionModel = precisionModel;
			// compute intersections in full precision, to provide accuracy
			// the points are rounded as they are inserted into the curve line
			li = new RobustLineIntersector();
			
			int limitedQuadSegs = quadrantSegments < 1?1:quadrantSegments;
			filletAngleQuantum = Math.PI / 2.0 / limitedQuadSegs;
		}
        
        #endregion

        #region Public Properties

		public BufferCapType EndCapStyle
		{
            get 
            {
                return this.endCapStyle;
            }

			set
			{
				this.endCapStyle = value;
			}
		}
        
        #endregion
		
        #region Private Properties

        private ICoordinateList Coordinates
		{
			get
			{
				// check that points are a ring - Add the startpoint again 
                // if they are not
				if (ptList.Count > 1)
				{
					Coordinate start = ptList[0];
					Coordinate end   = ptList[ptList.Count - 1];
					if (!start.Equals(end))
						AddPoint(start);
				}
				
				return ptList;
			}
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// This method handles single points as well as lines. Lines are assumed 
		/// to not be closed (the function will not fail for closed lines, but 
		/// will generate superfluous line caps).
		/// </summary>
		/// <returns>A List of <see cref="ICoordinateList"/>.</returns>
		public IList GetLineCurve(ICoordinateList inputPts, double distance)
		{
			ArrayList lineList = new ArrayList();
			// a zero or negative width Buffer of a line/point is empty
			if (distance <= 0.0)
				return lineList;
			
			Initialize(distance);
			if (inputPts.Count <= 1)
			{
				switch (endCapStyle)
				{
					case BufferCapType.Round: 
						AddCircle(inputPts[0], distance);
						break;
					
					case BufferCapType.Square: 
						AddSquare(inputPts[0], distance);
						break;
						// default is for Buffer to be empty (e.g. for a butt line cap);
				}
			}
			else
            {
                ComputeLineBufferCurve(inputPts);
            }

			lineList.Add(this.Coordinates);

			return lineList;
		}

		/// <summary> 
		/// This method handles the degenerate cases of single points and lines,
		/// as well as rings.
		/// </summary>
		/// <returns>A List of <see cref="ICoordinateList"/></returns>
		public IList GetRingCurve(ICoordinateList inputPts, int side, double distance)
		{
			ArrayList lineList = new ArrayList();
			Initialize(distance);

			if (inputPts.Count <= 2)
				return GetLineCurve(inputPts, distance);
			
			// optimize creating ring for for zero distance
			if (distance == 0.0)
			{
				lineList.Add(CopyCoordinates(inputPts));

				return lineList;
			}

			ComputeRingBufferCurve(inputPts, side);
			lineList.Add(this.Coordinates);

			return lineList;
		}
        
        #endregion
		
        #region Private Methods

		private static ICoordinateList CopyCoordinates(ICoordinateList pts)
		{
			CoordinateCollection copy = new CoordinateCollection(pts.Count);
			for (int i = 0; i < pts.Count; i++)
			{
				copy.Add(new Coordinate(pts[i]));
			}
			return copy;
		}

		private void Initialize(double distance)
		{
			m_dDistance = distance;
			maxCurveSegmentError = distance * (1 - Math.Cos(filletAngleQuantum / 2.0));

			ptList = new CoordinateCollection();
		}
		
		private void ComputeLineBufferCurve(ICoordinateList inputPts)
		{
			int n = inputPts.Count - 1;
			
			// compute points for left side of line
			InitSideSegments(inputPts[0], inputPts[1], Position.Left);
			for (int i = 2; i <= n; i++)
			{
				AddNextSegment(inputPts[i], true);
			}
			AddLastSegment();
			// Add line cap for end of line
			AddLineEndCap(inputPts[n - 1], inputPts[n]);
			
			// compute points for right side of line
			InitSideSegments(inputPts[n], inputPts[n - 1], Position.Left);
			for (int i = n - 2; i >= 0; i--)
			{
				AddNextSegment(inputPts[i], true);
			}
			AddLastSegment();
			// Add line cap for start of line
			AddLineEndCap(inputPts[1], inputPts[0]);
			
			ClosePoints();
		}
		
		private void ComputeRingBufferCurve(ICoordinateList inputPts, int side)
		{
			int n = inputPts.Count - 1;
			InitSideSegments(inputPts[n - 1], inputPts[0], side);
			for (int i = 1; i <= n; i++)
			{
				bool addStartPoint = i != 1;
				AddNextSegment(inputPts[i], addStartPoint);
			}
			ClosePoints();
		}
		
		private void AddPoint(Coordinate pt)
		{
			Coordinate bufPt = new Coordinate(pt); 
			bufPt.MakePrecise(precisionModel);
			// don't Add duplicate points
			Coordinate lastPt = null;
			if (ptList.Count >= 1)
			{
				lastPt = ptList[ptList.Count - 1];
			}

			if (lastPt != null && bufPt.Equals(lastPt))
				return ;
			
			ptList.Add(bufPt);
		}

		private void ClosePoints()
		{
			if (ptList.Count < 1)
				return;

			Coordinate startPt = new Coordinate((Coordinate) ptList[0]);
			Coordinate lastPt  = ptList[ptList.Count - 1];
//			Coordinate last2Pt = null;
//
//            if (ptList.Count >= 2)
//			{
//				last2Pt = ptList[ptList.Count - 2];
//			}

			if (startPt.Equals(lastPt))
				return;

			ptList.Add(startPt);
		}
		
		private void InitSideSegments(Coordinate s1, Coordinate s2, int side)
		{
			this.s1   = s1;
			this.s2   = s2;
			this.side = side;

			seg1.SetCoordinates(s1, s2);
			
            ComputeOffsetSegment(seg1, side, m_dDistance, offset1);
		}
		
		private void AddNextSegment(Coordinate p, bool addStartPoint)
		{
			// s0-s1-s2 are the coordinates of the previous segment and the current one
			s0 = s1;
			s1 = s2;
			s2 = p;
			seg0.SetCoordinates(s0, s1);
			ComputeOffsetSegment(seg0, side, m_dDistance, offset0);
			seg1.SetCoordinates(s1, s2);
			ComputeOffsetSegment(seg1, side, m_dDistance, offset1);
			
			// do nothing if points are equal
			if (s1.Equals(s2))
				return ;
			
			OrientationType orientation = CGAlgorithms.ComputeOrientation(s0, s1, s2);
			bool outsideTurn = 
                (orientation == OrientationType.Clockwise && 
                side == Position.Left) || 
                (orientation == OrientationType.CounterClockwise && 
                side == Position.Right);
			
			if (orientation == 0)
			{
				// lines are collinear
				li.ComputeIntersection(s0, s1, s1, s2);
				int numInt = li.IntersectionNum;

				// if numInt < 2, the lines are parallel and in the same direction.
				// In this case the point can be ignored, since the offset lines will also be
				// parallel.
				if (numInt >= 2)
				{
					// segments are collinear but reversing.  Have to Add an "end-cap" fillet
					// all the way around to other direction
					// This case should ONLY happen for LineStrings, so the orientation is always CW.
					// (Polygons can never have two consecutive segments which are parallel but reversed,
					// because that would be a self intersection.
					AddFillet(s1, offset0.p1, offset1.p0, 
                        OrientationType.Clockwise, m_dDistance);
				}
			}
			else if (outsideTurn)
			{
				// Add a fillet to connect the endpoints of the offset segments
				if (addStartPoint)
					AddPoint(offset0.p1);
				// TESTING - comment out to produce beveled joins
				AddFillet(s1, offset0.p1, offset1.p0, orientation, m_dDistance);
				AddPoint(offset1.p0);
			}
			else
			{
				// inside turn
				// Add intersection point of offset segments (if any)
				li.ComputeIntersection(offset0.p0, offset0.p1, offset1.p0, offset1.p1);
				if (li.HasIntersection)
				{
					AddPoint(li.GetIntersection(0));
				}
				else
				{
					// If no intersection, it means the angle is so small and/or the offset so large
					// that the offsets segments don't intersect.
					// In this case we must Add a offset joining curve to make sure the Buffer line
					// is continuous and tracks the Buffer correctly around the corner.
					// Note that the joining curve won't appear in the final Buffer.
					// 
					// The intersection test above is vulnerable to robustness errors;
					// i.e. it may be that the offsets should intersect very close to their
					// endpoints, but don't due to rounding.  To handle this situation
					// appropriately, we use the following test:
					// If the offset points are very close, don't Add a joining curve
					// but simply use one of the offset points
					if (offset0.p1.Distance(offset1.p0) < m_dDistance / 1000.0)
					{
						AddPoint(offset0.p1);
					}
					else
					{
						// Add endpoint of this segment offset
						AddPoint(offset0.p1);
						// <FIX> MD - Add in centre point of corner, to make sure offset closer lines have correct topology
						AddPoint(s1);
						AddPoint(offset1.p0);
					}
				}
			}
		}

		/// <summary> Add last offset point</summary>
		private void AddLastSegment()
		{
			AddPoint(offset1.p1);
		}
		
		/// <summary> 
		/// Compute an offset segment for an input segment on a given side 
		/// and at a given distance.
		/// </summary>
		/// <param name="seg">the segment to offset
		/// </param>
        /// <param name="side">the side of the segment ({@link Position}) the offset lies on
        /// </param>
        /// <param name="distance">
		/// The offset distance.
		/// </param>
		/// <param name="offset">The points computed for the offset segment
		/// </param>
		/// <remarks>
		/// The offset points are computed in full double precision, 
		/// for accuracy.
		/// </remarks>
		private void ComputeOffsetSegment(LineSegment seg, int side, 
            double distance, LineSegment offset)
		{
			int sideSign = side == Position.Left ? 1 : -1;
			double dx = seg.p1.X - seg.p0.X;
			double dy = seg.p1.Y - seg.p0.Y;
			double len = Math.Sqrt(dx * dx + dy * dy);
			// u is the vector that is the length of the offset, in the direction of the segment
			double ux = sideSign * distance * dx / len;
			double uy = sideSign * distance * dy / len;
			offset.p0.X = seg.p0.X - uy;
			offset.p0.Y = seg.p0.Y + ux;
			offset.p1.X = seg.p1.X - uy;
			offset.p1.Y = seg.p1.Y + ux;
		}
		
		/// <summary> 
		/// Add an end cap around point p1, terminating a line segment coming from p0
		/// </summary>
		private void AddLineEndCap(Coordinate p0, Coordinate p1)
		{
            GeometryFactory factory = new GeometryFactory(precisionModel);

			LineSegment seg = new LineSegment(factory, p0, p1);
			
			LineSegment offsetL = new LineSegment(factory);
			ComputeOffsetSegment(seg, Position.Left, m_dDistance, offsetL);
			LineSegment offsetR = new LineSegment(factory);
			ComputeOffsetSegment(seg, Position.Right, m_dDistance, offsetR);
			
			double dx = p1.X - p0.X;
			double dy = p1.Y - p0.Y;
			double angle = Math.Atan2(dy, dx);
			
			switch (endCapStyle)
			{
				case BufferCapType.Round: 
					AddPoint(offsetL.p1);
					AddFillet(p1, angle + Math.PI / 2, angle - Math.PI / 2, 
                        OrientationType.Clockwise, m_dDistance);
					AddPoint(offsetR.p1);
					break;
				
				case BufferCapType.Flat: 
					AddPoint(offsetL.p1);
					AddPoint(offsetR.p1);
					break;
				
				case BufferCapType.Square: 
					Coordinate squareCapSideOffset = new Coordinate();
					squareCapSideOffset.X = Math.Abs(m_dDistance) * Math.Cos(angle);
					squareCapSideOffset.Y = Math.Abs(m_dDistance) * Math.Sin(angle);
					
					Coordinate squareCapLOffset = new Coordinate(offsetL.p1.X + squareCapSideOffset.X, 
                        offsetL.p1.Y + squareCapSideOffset.Y);
					Coordinate squareCapROffset = new Coordinate(offsetR.p1.X + squareCapSideOffset.X, 
                        offsetR.p1.Y + squareCapSideOffset.Y);

					AddPoint(squareCapLOffset);
					AddPoint(squareCapROffset);
					break;
			}
		}

		/// <param name="p">base point of curve
		/// </param>
		/// <param name="p0">start point of fillet curve
		/// </param>
		/// <param name="p1">endpoint of fillet curve
		/// </param>
		private void AddFillet(Coordinate p, Coordinate p0, Coordinate p1, 
            OrientationType direction, double distance)
		{
			double dx0 = p0.X - p.X;
			double dy0 = p0.Y - p.Y;
			double startAngle = Math.Atan2(dy0, dx0);
			double dx1 = p1.X - p.X;
			double dy1 = p1.Y - p.Y;
			double endAngle = Math.Atan2(dy1, dx1);
			
			if (direction == OrientationType.Clockwise)
			{
				if (startAngle <= endAngle)
					startAngle += 2.0 * Math.PI;
			}
			else
			{
				// direction == COUNTERCLOCKWISE
				if (startAngle >= endAngle)
					startAngle -= 2.0 * Math.PI;
			}

			AddPoint(p0);
			AddFillet(p, startAngle, endAngle, direction, distance);
			AddPoint(p1);
		}
		
		/// <summary> Adds points for a fillet.  The start and end point for the fillet are not added -
		/// the caller must Add them if required.
		/// 
		/// </summary>
		/// <param name="direction">is -1 for a CW angle, 1 for a CCW angle
		/// </param>
		private void AddFillet(Coordinate p, double startAngle, double endAngle, 
            OrientationType direction, double distance)
		{
			int directionFactor = (direction == 
                OrientationType.Clockwise) ? -1 : 1;
			
			double totalAngle = Math.Abs(startAngle - endAngle);

			int nSegs = (int) (totalAngle / filletAngleQuantum + 0.5);
			
			if (nSegs < 1)
				return; // no segments because angle is less than increment - nothing to do!
			
			double initAngle, currAngleInc;
			
			// choose angle increment so that each segment has equal length
			initAngle = 0.0;
			currAngleInc = totalAngle / nSegs;
			
			double currAngle = initAngle;
			Coordinate pt = new Coordinate();
			while (currAngle < totalAngle)
			{
				double angle = startAngle + directionFactor * currAngle;
				pt.X = p.X + distance * Math.Cos(angle);
				pt.Y = p.Y + distance * Math.Sin(angle);
				AddPoint(pt);
				currAngle += currAngleInc;
			}
		}
		
		/// <summary> Adds a CW circle around a point</summary>
		private void AddCircle(Coordinate p, double distance)
		{
			// Add start point
			Coordinate pt = new Coordinate(p.X + distance, p.Y);
			AddPoint(pt);
			AddFillet(p, 0.0, 2.0 * Math.PI, 
                OrientationType.Clockwise, distance);
		}
		
		/// <summary> Adds a CW square around a point</summary>
		private void AddSquare(Coordinate p, double distance)
		{
			// Add start point
			AddPoint(new Coordinate(p.X + distance, p.Y + distance));
			AddPoint(new Coordinate(p.X + distance, p.Y - distance));
			AddPoint(new Coordinate(p.X - distance, p.Y - distance));
			AddPoint(new Coordinate(p.X - distance, p.Y + distance));
			AddPoint(new Coordinate(p.X + distance, p.Y + distance));
		}
        
        #endregion
    }
}