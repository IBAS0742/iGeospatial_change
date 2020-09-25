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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Simplifications
{
	/// <summary> 
	/// Simplifies a linestring (sequence of points) using the standard 
	/// Douglas-Peucker algorithm.
	/// </summary>
    [Serializable]
    public class DouglasPeuckerLineSimplifier
	{
        #region Private Fields

        private Coordinate[] m_arrPoints;
        private bool[]       usePt;
        private double       distanceTolerance;
		
        private LineSegment  seg;
        
        #endregion
		
        #region Constructors and Destructor

        public DouglasPeuckerLineSimplifier(Coordinate[] points)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            m_arrPoints = points;
            seg = new LineSegment((GeometryFactory)null);
        }
		
        public DouglasPeuckerLineSimplifier(Coordinate[] points, 
            double tolerance)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (tolerance < 0.0)
                throw new ArgumentException("Tolerance must be non-negative");				

            seg                    = new LineSegment((GeometryFactory)null);
            m_arrPoints            = points;
            this.distanceTolerance = tolerance;
        }
        
        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Gets or sets the distance tolerance for the simplification.
		/// All vertices in the simplified linestring will be within this
		/// distance of the original linestring.
		/// </summary>
		/// <value>
		/// A number specifying the approximation tolerance to use.
		/// </value>
		public double DistanceTolerance
		{
            get
            {
                return this.distanceTolerance;
            }

			set
			{
                if (value < 0.0)
                    throw new ArgumentException("Tolerance must be non-negative");				

                this.distanceTolerance = value;
			}
		}
        
        #endregion
			
        #region Public Methods

		public static Coordinate[] Simplify(Coordinate[] points, 
            double tolerance)
		{
			DouglasPeuckerLineSimplifier simp = 
                new DouglasPeuckerLineSimplifier(points, tolerance);

            return simp.Simplify();
		}
		
		public Coordinate[] Simplify()
		{
            int nLength = m_arrPoints.Length;

			usePt = new bool[nLength];
			for (int i = 0; i < nLength; i++)
			{
				usePt[i] = true;
			}

			SimplifySection(0, nLength - 1);
			CoordinateCollection coordList = new CoordinateCollection();
			for (int i = 0; i < nLength; i++)
			{
				if (usePt[i])
					coordList.Add(new Coordinate(m_arrPoints[i]));
			}

			return coordList.ToArray();
		}
        
        #endregion
		
        #region Private Methods

		private void SimplifySection(int i, int j)
		{
			if ((i + 1) == j)
			{
				return ;
			}

			seg.p0 = m_arrPoints[i];
			seg.p1 = m_arrPoints[j];
			double maxDistance = -1.0;
			int maxIndex       = i;
			
            for (int k = i + 1; k < j; k++)
			{
				double distance = seg.Distance(m_arrPoints[k]);
				if (distance > maxDistance)
				{
					maxDistance = distance;
					maxIndex    = k;
				}
			}

			if (maxDistance <= distanceTolerance)
			{
				for (int k = i + 1; k < j; k++)
				{
					usePt[k] = false;
				}
			}
			else
			{
				SimplifySection(i, maxIndex);
				SimplifySection(maxIndex, j);
			}
		}
        
        #endregion
	}
}