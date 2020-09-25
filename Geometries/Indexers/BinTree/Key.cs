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
using iGeospatial.Geometries.Indexers.QuadTree;

namespace iGeospatial.Geometries.Indexers.BinTree
{
	/// <summary> 
	/// A Key is a unique identifier for a node in a tree.
	/// It Contains a lower-left point and a level number. The level number
	/// is the power of two for the size of the node envelope
	/// </summary>
    [Serializable]
    internal class Key
	{
		// the fields which make up the key
		private double pt;
		private int level;

		// auxiliary data which is derived from the key for use in computation
		private Interval m_objInterval;
		
		public Key(Interval interval)
		{
			ComputeKey(interval);
		}
		
		public double Point
		{
			get
			{
				return pt;
			}
		}
			
		public int Level
		{
			get
			{
				return level;
			}
		}
			
		public Interval Interval
		{
			get
			{
				return m_objInterval;
			}
		}
		
		public static int ComputeLevel(Interval interval)
		{
			double dx = interval.Width;
			//int level = BinaryPower.exponent(dx) + 1;
			int level = DoubleBits.Exponent(dx) + 1;

			return level;
		}
		
		
		/// <summary> return a square envelope containing the argument envelope,
		/// whose extent is a power of two and which is based at a power of 2
		/// </summary>
		public void ComputeKey(Interval itemInterval)
		{
			level         = ComputeLevel(itemInterval);
			m_objInterval = new Interval();
			ComputeInterval(level, itemInterval);
			// MD - would be nice to have a non-iterative form of this algorithm
			while (!m_objInterval.Contains(itemInterval))
			{
				level += 1;
				ComputeInterval(level, itemInterval);
			}
		}
		
		private void ComputeInterval(int level, Interval itemInterval)
		{
			double size = DoubleBits.PowerOf2(level);
			pt = Math.Floor(itemInterval.Min / size) * size;
			m_objInterval.Initialize(pt, pt + size);
		}
	}
}