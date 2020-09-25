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

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary> 
	/// A robust implementation of <see cref="LineIntersector"/>, lines intersection
	/// determination abstract class.
	/// </summary>
	/// <seealso cref="RobustDeterminant"/>
	/// <seealso cref="LineIntersector"/>
    [Serializable]
    public sealed class RobustLineIntersector : LineIntersector
	{
        #region Constructors and Destructor
		
        /// <summary>
        /// Initializes a new instance of the <see cref="RobustLineIntersector"/>
        /// class.
        /// </summary>
        public RobustLineIntersector()
		{
		}

        #endregion
		
        #region Public Methods
        
        /// <summary>
        /// Compute the intersection of a point p and the line p1-p2.
        /// </summary>
        /// <param name="p">The test point.</param>
        /// <param name="p1">The starting coordinate of the line.</param>
        /// <param name="p2">The ending coordinate of the line.</param>
        /// <remarks>
        /// This function computes the boolean value of the HasIntersection test.
        /// The actual value of the intersection (if there is one)
        /// is equal to the value of p.
        /// </remarks>
        public override void ComputeIntersection(Coordinate p, Coordinate p1, Coordinate p2)
		{
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }
            if (p == null)
            {
                throw new ArgumentNullException("p");
            }

            m_bIsProper = false;
			// do between check first, since it is faster than the orientation test
			if (Envelope.Intersects(p1, p2, p))
			{
				if ((RobustCGAlgorithms.OrientationIndex(p1, p2, p) == 0) && 
                    (RobustCGAlgorithms.OrientationIndex(p2, p1, p) == 0))
				{
					m_bIsProper = true;
					if (p.Equals(p1) || p.Equals(p2))
					{
						m_bIsProper = false;
					}
					result = (int)IntersectState.DoIntersect;

					return;
				}
			}

			result = (int)IntersectState.DonotIntersect;
		}
        
        #endregion

        #region Protected Methods
		
        /// <summary>
        /// Computes the intersection of the lines p1-p2 and q1-q2.
        /// </summary>
        /// <param name="p1">The starting coordinate of first line.</param>
        /// <param name="p2">The ending coordinate of first line.</param>
        /// <param name="q1">The starting coordinate of seocnd line.</param>
        /// <param name="q2">The ending coordinate of second line.</param>
        /// <returns>
        /// A integer, which is one of the <see cref="IntersectState"/> values, 
        /// specifying the intersection state of the lines.
        /// </returns>
        protected override int ComputeIntersect(Coordinate p1, Coordinate p2, 
            Coordinate q1, Coordinate q2)
		{
            if (p1 == null)
            {
                throw new ArgumentNullException("p1");
            }
            if (p2 == null)
            {
                throw new ArgumentNullException("p2");
            }
            if (q1 == null)
            {
                throw new ArgumentNullException("q1");
            }
            if (q2 == null)
            {
                throw new ArgumentNullException("q2");
            }

            m_bIsProper = false;
			
			// first try a fast test to see if the envelopes of the lines intersect
			if (!Envelope.Intersects(p1, p2, q1, q2))
				return (int)IntersectState.DonotIntersect;
			
			// for each endpoint, compute which side of the other segment it lies
			// if both endpoints lie on the same side of the other segment,
			// the segments do not intersect
			int Pq1 = RobustCGAlgorithms.OrientationIndex(p1, p2, q1);
			int Pq2 = RobustCGAlgorithms.OrientationIndex(p1, p2, q2);
			
			if ((Pq1 > 0 && Pq2 > 0) || (Pq1 < 0 && Pq2 < 0))
			{
				return (int)IntersectState.DonotIntersect;
			}
			
			int Qp1 = RobustCGAlgorithms.OrientationIndex(q1, q2, p1);
			int Qp2 = RobustCGAlgorithms.OrientationIndex(q1, q2, p2);
			
			if ((Qp1 > 0 && Qp2 > 0) || (Qp1 < 0 && Qp2 < 0))
			{
				return (int)IntersectState.DonotIntersect;
			}
			
			bool collinear = Pq1 == 0 && Pq2 == 0 && Qp1 == 0 && Qp2 == 0;
			if (collinear)
			{
				return ComputeCollinearIntersection(p1, p2, q1, q2);
			}

			// Check if the intersection is an endpoint. If it is, copy the endpoint as
			// the intersection point. Copying the point rather than computing it
			// ensures the point has the exact value, which is important for
			// robustness. It is sufficient to simply check for an endpoint which is on
			// the other line, since at this point we know that the inputLines must
			// intersect.
			if (Pq1 == 0 || Pq2 == 0 || Qp1 == 0 || Qp2 == 0)
			{
				m_bIsProper = false;
				if (Pq1 == 0)
				{
					intPt[0] = new Coordinate(q1);
				}

				if (Pq2 == 0)
				{
					intPt[0] = new Coordinate(q2);
				}
				
                if (Qp1 == 0)
				{
					intPt[0] = new Coordinate(p1);
				}
				
                if (Qp2 == 0)
				{
					intPt[0] = new Coordinate(p2);
				}
			}
			else
			{
				m_bIsProper = true;
				intPt[0] = Intersection(p1, p2, q1, q2);
			}

			return (int)IntersectState.DoIntersect;
		}
        
        #endregion
		
        #region Private Methods

		private int ComputeCollinearIntersection(Coordinate p1, Coordinate p2, 
            Coordinate q1, Coordinate q2)
		{
			bool p1q1p2 = Envelope.Intersects(p1, p2, q1);
			bool p1q2p2 = Envelope.Intersects(p1, p2, q2);
			bool q1p1q2 = Envelope.Intersects(q1, q2, p1);
			bool q1p2q2 = Envelope.Intersects(q1, q2, p2);
			
			if (p1q1p2 && p1q2p2)
			{
				intPt[0] = q1;
				intPt[1] = q2;
				
                return (int)IntersectState.Collinear;
			}
			
            if (q1p1q2 && q1p2q2)
			{
				intPt[0] = p1;
				intPt[1] = p2;

				return (int)IntersectState.Collinear;
			}
			
            if (p1q1p2 && q1p1q2)
			{
				intPt[0] = q1;
				intPt[1] = p1;

				return q1.Equals(p1) && !p1q2p2 && !q1p2q2 ? 
                    (int)IntersectState.DoIntersect : (int)IntersectState.Collinear;
			}
			
            if (p1q1p2 && q1p2q2)
			{
				intPt[0] = q1;
				intPt[1] = p2;
				return q1.Equals(p2) && !p1q2p2 && !q1p1q2 ? 
                    (int)IntersectState.DoIntersect : (int)IntersectState.Collinear;
			}
			
            if (p1q2p2 && q1p1q2)
			{
				intPt[0] = q2;
				intPt[1] = p1;
				return q2.Equals(p1) && !p1q1p2 && !q1p2q2 ? 
                    (int)IntersectState.DoIntersect : (int)IntersectState.Collinear;
			}
			
            if (p1q2p2 && q1p2q2)
			{
				intPt[0] = q2;
				intPt[1] = p2;
				return q2.Equals(p2) && !p1q1p2 && !q1p1q2 ? 
                    (int)IntersectState.DoIntersect : (int)IntersectState.Collinear;
			}

			return (int)IntersectState.DonotIntersect;
		}
		
        /// <summary>
        /// This method computes the actual value of the intersection point.
        /// </summary>
        /// <param name="p1">The starting coordinate of first line.</param>
        /// <param name="p2">The ending coordinate of first line.</param>
        /// <param name="q1">The starting coordinate of seocnd line.</param>
        /// <param name="q2">The ending coordinate of second line.</param>
        /// <returns>The coordinate of the intersection point.</returns>
        /// <remarks>
		/// To obtain the maximum precision from the intersection calculation,
		/// the coordinates are normalized by subtracting the minimum
		/// ordinate values (in absolute value).  This has the effect of
		/// removing common significant digits from the calculation to
		/// maintain more bits of precision.
        /// </remarks>
        private Coordinate Intersection(Coordinate p1, Coordinate p2, 
            Coordinate q1, Coordinate q2)
		{
			Coordinate n1 = new Coordinate(p1);
			Coordinate n2 = new Coordinate(p2);
			Coordinate n3 = new Coordinate(q1);
			Coordinate n4 = new Coordinate(q2);
			Coordinate normPt = new Coordinate();
			NormalizeToEnvCentre(n1, n2, n3, n4, normPt);
			
			Coordinate intPt = null;
			try
			{
				intPt = HCoordinate.Intersection(n1, n2, n3, n4);
			}
			catch (AlgorithmException ex)
			{
                ExceptionManager.Publish(ex);
            }
			
			intPt.X += normPt.X;
			intPt.Y += normPt.Y;

            /**
             *
             * MD - May 4 2005 - This is still a problem.  Here is a failure case:
             *
             * LINESTRING (2089426.5233462777 1180182.3877339689, 2085646.6891757075 1195618.7333999649)
             * LINESTRING (1889281.8148903656 1997547.0560044837, 2259977.3672235999 483675.17050843034)
             * int point = (2097408.2633752143,1144595.8008114607)
             */
            if (!IsInSegmentEnvelopes(intPt)) 
            {
                Trace.WriteLine("Intersection outside segment envelopes: " 
                    + intPt.ToString());
            }
			
			if (m_objPrecisionModel != null)
			{
				intPt.MakePrecise(m_objPrecisionModel);
			}
			
			return intPt;
		}
		
		private void Normalize(Coordinate n1, Coordinate n2, 
            Coordinate n3, Coordinate n4, Coordinate normPt)
		{
			normPt.X = SmallestInAbsValue(n1.X, n2.X, n3.X, n4.X);
			normPt.Y = SmallestInAbsValue(n1.Y, n2.Y, n3.Y, n4.Y);
			n1.X -= normPt.X; 
            n1.Y -= normPt.Y;
			n2.X -= normPt.X; 
            n2.Y -= normPt.Y;
			n3.X -= normPt.X; 
            n3.Y -= normPt.Y;
			n4.X -= normPt.X; 
            n4.Y -= normPt.Y;
		}
        
        /// <summary>
        /// Normalize the supplied coordinates so that
        /// their minimum ordinate values lie at the origin.
        /// NOTE: this normalization technique appears to cause
        /// large errors in the position of the intersection point for some cases.
        /// </summary>
        /// <param name="n1"></param>
        /// <param name="n2"></param>
        /// <param name="n3"></param>
        /// <param name="n4"></param>
        /// <param name="normPt"></param>
        private void NormalizeToMinimum(Coordinate n1, Coordinate n2,
            Coordinate n3, Coordinate n4, Coordinate normPt)
        {
            normPt.X = SmallestInAbsValue(n1.X, n2.X, n3.X, n4.X);
            normPt.Y = SmallestInAbsValue(n1.Y, n2.Y, n3.Y, n4.Y);
            n1.X -= normPt.X;    
            n1.Y -= normPt.Y;
            n2.X -= normPt.X;    
            n2.Y -= normPt.Y;
            n3.X -= normPt.X;    
            n3.Y -= normPt.Y;
            n4.X -= normPt.X;    
            n4.Y -= normPt.Y;
        }

        /// <summary>
        /// Normalize the supplied coordinates to
        /// so that the midpoint of their intersection envelope
        /// lies at the origin.
        /// </summary>
        /// <param name="n00"></param>
        /// <param name="n01"></param>
        /// <param name="n10"></param>
        /// <param name="n11"></param>
        /// <param name="normPt"></param>
        private void NormalizeToEnvCentre(Coordinate n00,
            Coordinate n01, Coordinate n10, Coordinate n11,
            Coordinate normPt)
        {
            double minX0 = n00.X < n01.X ? n00.X : n01.X;
            double minY0 = n00.Y < n01.Y ? n00.Y : n01.Y;
            double maxX0 = n00.X > n01.X ? n00.X : n01.X;
            double maxY0 = n00.Y > n01.Y ? n00.Y : n01.Y;

            double minX1 = n10.X < n11.X ? n10.X : n11.X;
            double minY1 = n10.Y < n11.Y ? n10.Y : n11.Y;
            double maxX1 = n10.X > n11.X ? n10.X : n11.X;
            double maxY1 = n10.Y > n11.Y ? n10.Y : n11.Y;

            double intMinX = minX0 > minX1 ? minX0 : minX1;
            double intMaxX = maxX0 < maxX1 ? maxX0 : maxX1;
            double intMinY = minY0 > minY1 ? minY0 : minY1;
            double intMaxY = maxY0 < maxY1 ? maxY0 : maxY1;

            double intMidX = (intMinX + intMaxX) / 2.0;
            double intMidY = (intMinY + intMaxY) / 2.0;
            normPt.X = intMidX;
            normPt.Y = intMidY;

            /*
            // equilavalent code using more modular but slower method
            Envelope env0 = new Envelope(n00, n01);
            Envelope env1 = new Envelope(n10, n11);
            Envelope intEnv = env0.intersection(env1);
            Coordinate intMidPt = intEnv.centre();

            normPt.X = intMidPt.X;
            normPt.Y = intMidPt.Y;
            */

            n00.X -= normPt.X;    n00.Y -= normPt.Y;
            n01.X -= normPt.X;    n01.Y -= normPt.Y;
            n10.X -= normPt.X;    n10.Y -= normPt.Y;
            n11.X -= normPt.X;    n11.Y -= normPt.Y;
        }

        private double SmallestInAbsValue(double x1, double x2, double x3, double x4)
		{
			double x    = x1;
			double xabs = Math.Abs(x);
			
            if (Math.Abs(x2) < xabs)
			{
				x = x2;
				xabs = Math.Abs(x2);
			}
			
            if (Math.Abs(x3) < xabs)
			{
				x = x3;
				xabs = Math.Abs(x3);
			}
			
            if (Math.Abs(x4) < xabs)
			{
				x = x4;
			}

			return x;
		}
		
		/// <summary> Test whether a point lies in the envelopes of both input segments.
		/// A correctly computed intersection point should return true
		/// for this test.
		/// Since this test is for debugging purposes only, no attempt is
		/// made to optimize the envelope test.
		/// 
		/// </summary>
		/// <returns> true if the input point lies Within both input segment envelopes
		/// </returns>
		private bool IsInSegmentEnvelopes(Coordinate intPt)
		{
			Envelope env0 = new Envelope(inputLines[0][0], inputLines[0][1]);
			Envelope env1 = new Envelope(inputLines[1][0], inputLines[1][1]);

			return env0.Contains(intPt) && env1.Contains(intPt);
		}

        #endregion
	}
}