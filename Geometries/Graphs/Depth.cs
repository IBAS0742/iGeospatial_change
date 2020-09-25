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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A Depth object records the topological depth of the sides
	/// of an Edge for up to two Geometries.
	/// </summary>
	[Serializable]
    internal class Depth
	{
        private int[][] depth;
		
		private const int NullValue = - 1;
		
		public static int DepthAtLocation(int location)
		{
			if (location == LocationType.Exterior)
				return 0;
			if (location == LocationType.Interior)
				return 1;

			return NullValue;
		}
		
		public Depth()
		{
            depth = new int[2][];
            for (int i = 0; i < 2; i++)
            {
                depth[i] = new int[3];
            }

            // initialize depth array to a sentinel value
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					depth[i][j] = NullValue;
				}
			}
		}
		
		public int GetDepth(int geomIndex, int posIndex)
		{
			return depth[geomIndex][posIndex];
		}

		public void SetDepth(int geomIndex, int posIndex, int depthValue)
		{
			depth[geomIndex][posIndex] = depthValue;
		}
		
        public int GetLocation(int geomIndex, int posIndex)
		{
			if (depth[geomIndex][posIndex] <= 0)
				return LocationType.Exterior;
			return LocationType.Interior;
		}
		
        public void Add(int geomIndex, int posIndex, int location)
		{
			if (location == LocationType.Interior)
				depth[geomIndex][posIndex]++;
		}
		
        /// <summary> 
        /// A Depth object is null (has never been initialized) if all depths are null.
        /// </summary>
		public bool IsNull()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (depth[i][j] != NullValue)
						return false;
				}
			}

			return true;
		}

		public bool IsNull(int geomIndex)
		{
			return (depth[geomIndex][1] == NullValue);
		}
		
        public bool IsNull(int geomIndex, int posIndex)
		{
			return (depth[geomIndex][posIndex] == NullValue);
		}

		public void Add(Label lbl)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 1; j < 3; j++)
				{
					int loc = lbl.GetLocation(i, j);
					if (loc == LocationType.Exterior || loc == LocationType.Interior)
					{
						// initialize depth if it is null, otherwise Add this location value
						if (IsNull(i, j))
						{
							depth[i][j] = DepthAtLocation(loc);
						}
						else
                        {
                            depth[i][j] += DepthAtLocation(loc);
                        }
					}
				}
			}
		}

		public int GetDelta(int geomIndex)
		{
			return depth[geomIndex][Position.Right] - 
                depth[geomIndex][Position.Left];
		}
		
        /// <summary> Normalize the depths for each geometry, if they are non-null.
		/// A normalized depth
		/// has depth values in the set { 0, 1 }.
		/// Normalizing the depths
		/// involves reducing the depths by the same amount so that at least
		/// one of them is 0.  If the remaining value is > 0, it is set to 1.
		/// </summary>
		public void Normalize()
		{
			for (int i = 0; i < 2; i++)
			{
				if (!IsNull(i))
				{
					int minDepth = depth[i][1];
					if (depth[i][2] < minDepth)
						minDepth = depth[i][2];
					
					if (minDepth < 0)
						minDepth = 0;
					for (int j = 1; j < 3; j++)
					{
						int newValue = 0;
						if (depth[i][j] > minDepth)
							newValue = 1;
						depth[i][j] = newValue;
					}
				}
			}
		}
		
		public override string ToString()
		{
			return "A: " + depth[0][1] + "," + depth[0][2] + " B: " + depth[1][1] + "," + depth[1][2];
		}
	}
}