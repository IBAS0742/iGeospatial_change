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
	/// Supports linear referencing along a linear <see cref="Geometry"/>
	/// using the length along the line as the index.
	/// </summary>
	/// <remarks>
	/// Negative length values are taken as measured in the reverse direction
	/// from the end of the geometry.
	/// Out-of-range index values are handled by clamping
	/// them to the valid range of values.
	/// Non-simple lines (i.e. which loop back to cross or touch
	/// themselves) are supported.
	/// </remarks>
    [Serializable]
    public class LengthIndexedLine
	{
        #region Private Fields

        private Geometry linearGeom;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> Constructs an object which allows a linear <see cref="Geometry"/>
		/// to be linearly referenced using length as an index.
		/// 
		/// </summary>
		/// <param name="linearGeom">the linear geometry to reference along
		/// </param>
		public LengthIndexedLine(Geometry linearGeom)
		{
			this.linearGeom = linearGeom;
		}
        
        #endregion
		
        #region Public Properties

		/// <summary>Gets the index of the start of the line.</summary>
		/// <value> A number specifying the start index.</value>
		public virtual double StartIndex
		{
			get
			{
				return 0.0;
			}
		}
			
		/// <summary>Gets the index of the end of the line.</summary>
		/// <value> A number specifying the end index.</value>
		public virtual double EndIndex
		{
			get
			{
				return linearGeom.Length;
			}
		}
        
        #endregion
			
		/// <summary> 
		/// Computes the <see cref="Coordinate"/> for the point on the line 
		/// at the given index.
		/// </summary>
		/// <param name="index">The index of the desired point.</param>
		/// <returns>The Coordinate at the given index.</returns>
		/// <remarks>
		/// If the index is out of range the first or last point on the
		/// line will be returned.
		/// </remarks>
		public virtual Coordinate ExtractPoint(double index)
		{
			LinearLocation loc = LengthLocationMap.GetLocation(
                linearGeom, index);

			return loc.GetCoordinate(linearGeom);
		}
		
		/// <summary> 
		/// Computes the <see cref="LineString"/> for the interval on the line 
		/// between the given indices.
		/// </summary>
		/// <param name="startIndex">
		/// The index of the start of the interval.
		/// </param>
		/// <param name="endIndex">
		/// The index of the end of the interval.
		/// </param>
		/// <returns>The linear interval between the indices.</returns>
		/// <remarks>
		/// If the endIndex lies before the startIndex, the computed 
		/// geometry is reversed.
		/// </remarks>
		public virtual Geometry ExtractLine(double startIndex, double endIndex)
		{
//			LocationIndexedLine lil = new LocationIndexedLine(linearGeom);

			LinearLocation startLoc = LocationOf(startIndex);
			LinearLocation endLoc   = LocationOf(endIndex);

			return ExtractLineByLocation.Extract(linearGeom, 
                startLoc, endLoc);
		}
		
		private LinearLocation LocationOf(double index)
		{
			return LengthLocationMap.GetLocation(linearGeom, index);
		}
		
		/// <summary> 
		/// Computes the minimum index for a point on the line.
		/// </summary>
		/// <param name="point">A point on the line.</param>
		/// <returns>The minimum index of the point.</returns>
		/// <seealso cref="project"/>
		/// <remarks>
		/// If the line is not simple (i.e. loops back on itself)
		/// a single point may have more than one possible index.
		/// In this case, the smallest index is returned.
		/// <para>
		/// The supplied point does not <b>necessarily</b> have to lie precisely
		/// on the line, but if it is far from the line the accuracy and
		/// performance of this function is not guaranteed.
		/// Use <see cref="Project"/> to compute a guaranteed result for points
		/// which may be far from the line.
		/// </para>
		/// </remarks>
		public virtual double IndexOf(Coordinate point)
		{
			return LengthIndexOfPoint.IndexOf(linearGeom, point);
		}
		
		/// <summary> 
		/// Finds the index for a point on the line which is greater than 
		/// the given index.
		/// </summary>
		/// <param name="point">A point on the line.</param>
		/// <param name="minIndex">
		/// The value the returned index must be greater than.
		/// </param>
		/// <returns>
		/// The index of the point greater than the given minimum index.
		/// </returns>
		/// <remarks>
		/// If no such index exists, returns <c>minIndex</c>.
		/// This method can be used to determine all indexes for
		/// a point which occurs more than once on a non-simple line.
		/// It can also be used to disambiguate cases where the given point lies
		/// slightly off the line and is equidistant from two different
		/// points on the line.
		/// <para>
		/// The supplied point does not <b>necessarily</b> have to lie precisely
		/// on the line, but if it is far from the line the accuracy and
		/// performance of this function is not guaranteed.
		/// Use <see cref="Project"/> to compute a guaranteed result for points
		/// which may be far from the line.
		/// </para>
		/// </remarks>
		/// <seealso cref="Project"/>
		public virtual double IndexOfAfter(Coordinate point, double minIndex)
		{
			return LengthIndexOfPoint.IndexOfAfter(linearGeom, point, minIndex);
		}
		
		/// <summary> 
		/// Computes the indices for a subline of the line.
		/// </summary>
		/// <param name="subLine">A subLine of the line.</param>
		/// <returns>
		/// A pair of indices for the start and end of the subline.
		/// </returns>
		/// <remarks>
		/// (The subline must <b>conform</b> to the line; that is,
		/// all vertices in the subline (except possibly the first and last)
		/// must be vertices of the line and occcur in the same order).
		/// </remarks>
		public virtual double[] IndicesOf(Geometry subLine)
		{
			LinearLocation[] locIndex = LocationIndexOfLine.IndicesOf(
                linearGeom, subLine);

			double[] index = new double[]{LengthLocationMap.GetLength(
                  linearGeom, locIndex[0]), 
                  LengthLocationMap.GetLength(linearGeom, locIndex[1])};
			
            return index;
		}
		
		
		/// <summary> 
		/// Computes the index for the closest point on the line to the 
		/// given point.
		/// </summary>
		/// <param name="point">A point on the line.</param>
		/// <returns>The index of the point.</returns>
		/// <remarks>
		/// If more than one point has the closest distance the first one along the line
		/// is returned.
		/// (The point does not necessarily have to lie precisely on the line.)
		/// </remarks>
		public virtual double Project(Coordinate point)
		{
			return LengthIndexOfPoint.IndexOf(linearGeom, point);
		}
		
		/// <summary> 
		/// Tests whether an index is in the valid index range for the line.
		/// </summary>
		/// <param name="length">The index to test.</param>
		/// <returns> 
		/// This returns <see langword="true"/> if the index is in the 
		/// valid range, otherwise; <see langword="false"/>.
		/// </returns>
		public virtual bool IsValidIndex(double index)
		{
			return (index >= this.StartIndex && index <= this.EndIndex);
		}
		
		/// <summary> 
		/// Computes a valid index for this line
		/// by clamping the given index to the valid range of index values
		/// </summary>
		/// <returns>A valid index value.</returns>
		public virtual double ClampIndex(double index)
		{
			double startIndex = StartIndex;
			if (index < startIndex)
				return startIndex;
			
			double endIndex = EndIndex;
			if (index > endIndex)
				return endIndex;
			
			return index;
		}
	}
}