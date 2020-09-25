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

namespace iGeospatial.Geometries.Indexers.BinTree
{
	
	/// <summary> 
	/// Represents an (1-dimensional) closed interval on the Real number line.
	/// </summary>
    [Serializable]
    internal class Interval
	{
        private double m_dMin; 
        private double m_dMax;
		
        public Interval()
        {
        }
		
        public Interval(double min, double max)
        {
            Initialize(min, max);
        }

        public Interval(Interval interval)
        {
            if (interval == null)
            {
                throw new ArgumentNullException("interval");
            }

            Initialize(interval.m_dMin, interval.m_dMax);
        }
		
		public double Min
		{
			get
			{
				return m_dMin;
			}

            set
            {
                m_dMin = value;
            }
		}

		public double Max
		{
			get
			{
				return m_dMax;
			}

            set
            {
                m_dMax = value;
            }
		}

		public double Width
		{
			get
			{
				return m_dMax - m_dMin;
			}
		}
		
        public void Initialize(double min, double max)
		{
			m_dMin = min;
			m_dMax = max;

			if (min > max)
			{
				m_dMin = max;
				m_dMax = min;
			}
		}
		
		public void ExpandToInclude(Interval interval)
		{
            if (interval == null)
            {
                throw new ArgumentNullException("interval");
            }

            if (interval.m_dMax > m_dMax)
				m_dMax = interval.m_dMax;

			if (interval.m_dMin < m_dMin)
				m_dMin = interval.m_dMin;
		}

		public bool Overlaps(Interval interval)
		{
            if (interval == null)
            {
                throw new ArgumentNullException("interval");
            }

            return Overlaps(interval.m_dMin, interval.m_dMax);
		}
		
		public bool Overlaps(double min, double max)
		{
			if (m_dMin > max || m_dMax < min)
				return false;

			return true;
		}
		
		public bool Contains(Interval interval)
		{
            if (interval == null)
            {
                throw new ArgumentNullException("interval");
            }

            return Contains(interval.m_dMin, interval.m_dMax);
		}

		public bool Contains(double min, double max)
		{
			return (min >= m_dMin && max <= m_dMax);
		}
		
        public bool Contains(double p)
		{
			return (p >= m_dMin && p <= m_dMax);
		}
		
        public override System.String ToString()
        {
            return "[" + m_dMin + ", " + m_dMax + "]";
        }
    }
}