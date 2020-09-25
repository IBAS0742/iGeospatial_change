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

namespace iGeospatial.Geometries.LinearReferencing
{
	/// <summary> 
	/// Computes the <see cref="LinearLocation"/> for a given length
	/// along a linear <see cref="Geometry"/>.
	/// </summary>
	/// <remarks>
	/// Negative lengths are measured in reverse from end of the linear geometry.
	/// Out-of-range values are clamped.
	/// </remarks>
	[Serializable]
    public sealed class LengthLocationMap
	{
        // TODO: cache computed cumulative length for each vertex
		// TODO: support user-defined measures
		// TODO: support measure index for fast mapping to a location
		
        private Geometry linearGeom;
		
        public LengthLocationMap(Geometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }
		
		/// <summary> 
		/// Computes the <see cref="LinearLocation"/> for a given length along 
		/// a linear <see cref="Geometry"/>.
		/// </summary>
		/// <param name="line">The linear geometry to use.</param>
		/// <param name="length">The length index of the location.</param>
		/// <returns> 
		/// This returns the <see cref="LinearLocation"/> for the length.
		/// </returns>
		public static LinearLocation GetLocation(Geometry linearGeom, 
            double length)
		{
			LengthLocationMap locater = new LengthLocationMap(linearGeom);

			return locater.GetLocation(length);
		}
		
		/// <summary> 
		/// Computes the length for a given <see cref="LinearLocation"/>
		/// on a linear <see cref="Geometry"/>.
		/// </summary>
		/// <param name="line">The linear geometry to use.</param>
		/// <param name="loc">
		/// The <see cref="LinearLocation"/> index of the location.
		/// </param>
		/// <returns>
		/// This returns the length for the <see cref="LinearLocation"/>.
		/// </returns>
		public static double GetLength(Geometry linearGeom, LinearLocation loc)
		{
			LengthLocationMap locater = new LengthLocationMap(linearGeom);

			return locater.GetLength(loc);
		}
		
		/// <summary> 
		/// Compute the <see cref="LinearLocation"/> corresponding to a length.
		/// </summary>
		/// <param name="length">The length index.</param>
		/// <returns>
		/// This returns the corresponding <see cref="LinearLocation"/>.
		/// </returns>
		/// <remarks>
		/// Negative lengths are measured in reverse from end of the linear geometry.
		/// Out-of-range values are clamped.
		/// </remarks>
		public LinearLocation GetLocation(double length)
		{
			double forwardLength = length;
			if (length < 0.0)
			{
				double lineLen = linearGeom.Length;
				forwardLength  = lineLen + length;
			}

			return GetLocationForward(forwardLength);
		}
		
		private LinearLocation GetLocationForward(double length)
		{
			if (length <= 0.0)
				return new LinearLocation();
			
			double totalLength = 0.0;
			
			LinearIterator it = new LinearIterator(linearGeom);
			while (it.HasNext())
			{
				if (!it.EndOfLine)
				{
					Coordinate p0 = it.SegmentStart;
					Coordinate p1 = it.SegmentEnd;
					double segLen = p1.Distance(p0);

					// length falls in this segment
					if ((totalLength + segLen) > length)
					{
						double frac   = (length - totalLength) / segLen;
						int compIndex = it.ComponentIndex;
						int segIndex  = it.VertexIndex;

						return new LinearLocation(compIndex, segIndex, frac);
					}

					totalLength += segLen;
				}

				it.Next();
			}

			// length is longer than line - return end location
			return LinearLocation.GetEndLocation(linearGeom);
		}
		
		public double GetLength(LinearLocation loc)
		{
			double totalLength = 0.0;
			
			LinearIterator it = new LinearIterator(linearGeom);
			while (it.HasNext())
			{
				if (!it.EndOfLine)
				{
					Coordinate p0 = it.SegmentStart;
					Coordinate p1 = it.SegmentEnd;
					double segLen = p1.Distance(p0);

					// length falls in this segment
					if (loc.ComponentIndex == it.ComponentIndex && 
                        loc.SegmentIndex == it.VertexIndex)
					{
						return totalLength + segLen * loc.SegmentFraction;
					}
					totalLength += segLen;
				}

				it.Next();
			}

			return totalLength;
		}
	}
}