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

namespace iGeospatial.Geometries.Indexers.StrTree
{
	
	/// <summary> A contiguous portion of 1D-space. Used internally by SIRTree.</summary>
	/// <seealso cref="SIRTree">
	/// </seealso>
	internal class Interval
	{
        private double min;
        private double max;
		
        public Interval(Interval other) : this(other.min, other.max)
        {
        }
		
        public Interval(double min, double max)
        {
            Debug.Assert(min <= max);
            this.min = min;
            this.max = max;
        }
		
		public double Centre
		{
			get
			{
				return (min + max) / 2;
			}
		}
		
		/// <returns> this
		/// </returns>
		public Interval ExpandToInclude(Interval other)
		{
			max = Math.Max(max, other.max);
			min = Math.Min(min, other.min);
			return this;
		}
		
		public bool Intersects(Interval other)
		{
			return !(other.min > max || other.max < min);
		}

		public override bool Equals(object o)
		{
            Interval other = o as Interval;
			if (other == null)
			{
				return false;
			}

			return min == other.min && max == other.max;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}