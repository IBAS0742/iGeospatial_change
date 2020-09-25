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

namespace iGeospatial.Geometries.Indexers.SweepLine
{
	internal sealed class SweepLineInterval
	{
        private double min, max;
        private object item;
		
        public SweepLineInterval(double min, double max) :
            this(min, max, null)
        {
        }
		
        public SweepLineInterval(double min, double max, object item)
        {
            this.min  = min < max ? min : max;
            this.max  = max > min ? max : min;
            this.item = item;
        }
		
		public double Min
		{
			get
			{
				return min;
			}
		}
			
		public double Max
		{
			get
			{
				return max;
			}
		}
			
		public object Item
		{
			get
			{
				return item;
			}
		}
	}
}