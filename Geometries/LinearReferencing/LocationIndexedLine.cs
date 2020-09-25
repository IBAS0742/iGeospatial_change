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
	/// using <see cref="LinearLocation"/>s as the index.
	/// </summary>
	[Serializable]
    public class LocationIndexedLine
	{
        #region Private Fields

        private Geometry linearGeom;
        
        #endregion
		
        #region Constructors and Destructor

        /// <summary> Constructs an object which allows linear referencing along
        /// a given linear <see cref="Geometry"/>.
        /// 
        /// </summary>
        /// <param name="linearGeom">the linear geometry to reference along
        /// </param>
        public LocationIndexedLine(Geometry linearGeom)
        {
            this.linearGeom = linearGeom;
			
            CheckGeometryType();
        }
        
        #endregion
		
        #region Public Properties

        /// <summary>
		/// Gets the index of the start of the line.
		/// </summary>
		/// <value>
		/// A <see cref="LinearLocation"/> object specifying the index of the 
		/// start of the line.
		/// </value>
		public LinearLocation StartIndex
		{
			get
			{
				return new LinearLocation();
			}
		}
			
		/// <summary> 
		/// Gets the index of the end of the line</summary>
		/// <value>
		/// A <see cref="LinearLocation"/> object specifying the index of the 
		/// end of the line.
		/// </value>
		public LinearLocation EndIndex
		{
			get
			{
				return LinearLocation.GetEndLocation(linearGeom);
			}
		}
        
        #endregion

        #region Public Methods

		/// <summary> Computes the <see cref="Coordinate"/> for the point
		/// on the line at the given index.
		/// If the index is out of range the first or last point on the
		/// line will be returned.
		/// 
		/// </summary>
		/// <param name="length">the index of the desired point
		/// </param>
		/// <returns> the Coordinate at the given index
		/// </returns>
		public Coordinate ExtractPoint(LinearLocation index)
		{
			return index.GetCoordinate(linearGeom);
		}
		
		/// <summary> Computes the <see cref="LineString"/> for the interval
		/// on the line between the given indices.
		/// 
		/// </summary>
		/// <param name="startIndex">the index of the start of the interval
		/// </param>
		/// <param name="endIndex">the index of the end of the interval
		/// </param>
		/// <returns> the linear interval between the indices
		/// </returns>
		public Geometry ExtractLine(LinearLocation startIndex, 
            LinearLocation endIndex)
		{
			return ExtractLineByLocation.Extract(linearGeom, 
                startIndex, endIndex);
		}
		
		/// <summary> Computes the index for a given point on the line.
		/// <p>
		/// The supplied point does not <i>necessarily</i> have to lie precisely
		/// on the line, but if it is far from the line the accuracy and
		/// performance of this function is not guaranteed.
		/// Use {@link #project} to compute a guaranteed result for points
		/// which may be far from the line.
		/// 
		/// </summary>
		/// <param name="pt">a point on the line
		/// </param>
		/// <returns> the index of the point
		/// </returns>
		/// <seealso cref="project">
		/// </seealso>
		public LinearLocation IndexOf(Coordinate pt)
		{
			return LocationIndexOfPoint.IndexOf(linearGeom, pt);
		}
		
		/// <summary> Computes the indices for a subline of the line.
		/// (The subline must <i>conform</i> to the line; that is,
		/// all vertices in the subline (except possibly the first and last)
		/// must be vertices of the line and occcur in the same order).
		/// 
		/// </summary>
		/// <param name="subLine">a subLine of the line
		/// </param>
		/// <returns> a pair of indices for the start and end of the subline.
		/// </returns>
		public LinearLocation[] IndicesOf(Geometry subLine)
		{
			return LocationIndexOfLine.IndicesOf(linearGeom, subLine);
		}
		
		/// <summary> Computes the index for the closest point on the line to the given point.
		/// If more than one point has the closest distance the first one along the line
		/// is returned.
		/// (The point does not necessarily have to lie precisely on the line.)
		/// 
		/// </summary>
		/// <param name="pt">a point on the line
		/// </param>
		/// <returns> the index of the point
		/// </returns>
		public LinearLocation Project(Coordinate pt)
		{
			return LocationIndexOfPoint.IndexOf(linearGeom, pt);
		}
		
		/// <summary> Tests whether an index is in the valid index range for the line.
		/// 
		/// </summary>
		/// <param name="length">the index to test
		/// </param>
		/// <returns> <see langword="true"/> if the index is in the valid range
		/// </returns>
		public bool IsValidIndex(LinearLocation index)
		{
			return index.IsValid(linearGeom);
		}
		
		/// <summary> Computes a valid index for this line
		/// by clamping the given index to the valid range of index values
		/// 
		/// </summary>
		/// <returns> a valid index value
		/// </returns>
		public LinearLocation ClampIndex(LinearLocation index)
		{
			LinearLocation loc = index.Clone();
			loc.Clamp(linearGeom);

			return loc;
		}
        
        #endregion
			
        #region Private Methods

        private void CheckGeometryType()
        {
            GeometryType type = linearGeom.GeometryType;

            if (type != GeometryType.LineString ||
                type != GeometryType.LinearRing || 
                type != GeometryType.MultiLineString)
            {
                throw new ArgumentException("Input geometry must be linear");
            }
        }
        
        #endregion
    }
}