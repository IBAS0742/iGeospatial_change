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

using iGeospatial.Geometries;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Indexers.QuadTree
{
	/// <summary> 
	/// A Key is a unique identifier for a node in a quadtree.
	/// It Contains a lower-left point and a level number. The level number
	/// is the power of two for the size of the node envelope
	/// </summary>
	[Serializable]
    internal class Key
	{
		// the fields which make up the key
		private Coordinate pt;
		private int level;
		// auxiliary data which is derived from the key for use in computation
		private Envelope env;
		
        public Key(Envelope itemEnv)
        {
            pt = new Coordinate();

            ComputeKey(itemEnv);
        }

		public Coordinate Point
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

		public Envelope Envelope
		{
			get
			{
				return env;
			}
		}

		public Coordinate Center
		{
			get
			{
                return env.Center;

//				return new Coordinate((env.MinX + env.MaxX) / 2, (env.MinY + env.MaxY) / 2);
			}
		}
		
		public static int ComputeQuadLevel(Envelope env)
		{
			double dx   = env.Width;
			double dy   = env.Height;
			double dMax = dx > dy ? dx : dy;
			int level   = DoubleBits.Exponent(dMax) + 1;

			return level;
		}
		
		/// <summary> 
		/// return a square envelope containing the argument envelope,
		/// whose extent is a power of two and which is based at a power of 2
		/// </summary>
		public void ComputeKey(Envelope itemEnv)
		{
			level = ComputeQuadLevel(itemEnv);
			env = new Envelope();
			ComputeKey(level, itemEnv);
			// MD - would be nice to have a non-iterative form of this algorithm
			while (!env.Contains(itemEnv))
			{
				level += 1;
				ComputeKey(level, itemEnv);
			}
		}
		
		private void ComputeKey(int level, Envelope itemEnv)
		{
			double quadSize = DoubleBits.PowerOf2(level);
			pt.X = Math.Floor(itemEnv.MinX / quadSize) * quadSize;
			pt.Y = Math.Floor(itemEnv.MinY / quadSize) * quadSize;
			env.Initialize(pt.X, pt.X + quadSize, pt.Y, pt.Y + quadSize);
		}
	}
}