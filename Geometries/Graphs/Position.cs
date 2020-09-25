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

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A Position indicates the position of a location relative to a graph component
	/// (Node, Edge, or Area).
	/// </summary>
	internal sealed class Position
	{
		/// <summary>
		/// An indicator that a location is on a GraphComponent. 
		/// </summary>
		public const int On = 0;

		/// <summary>
		/// An indicator that a location is to the left of a GraphComponent. 
		/// </summary> 		
		public const int Left = 1;

		/// <summary>
		/// An indicator that a location is to the right of a GraphComponent. 
		/// </summary>
		public const int Right = 2;

        private Position()
        {
        }

		/// <summary> 
		/// Returns Left if the position is Right, Right if the position 
		/// is Left, or the position otherwise.
		/// </summary>		 
		public static int Opposite(int position)
		{
			if (position == Left)
				return Right;

			if (position == Right)
				return Left;

			return position;
		}
	}
}