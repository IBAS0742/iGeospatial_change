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

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary>  
	/// Constants representing the location of a point relative to a geometry. 
	/// </summary>
	/// <remarks>
	/// They can also be thought of as the row or column index of a DE-9IM matrix. For a
	/// description of the DE-9IM, see the OpenGIS Simple Features Specification for SQL.
	/// </remarks>
    [Serializable]
    public sealed class LocationType
	{
		/// <summary>  Used for uninitialized location values.</summary>
		public const int None     = -1;
		
		/// <summary>  
		/// DE-9IM row index of the interior of the first geometry and column index of
		/// the interior of the second geometry. Location value for the interior of a
		/// geometry.
		/// </summary>
		public const int Interior = 0;

		/// <summary>  
		/// DE-9IM row index of the boundary of the first geometry and column index of
		/// the boundary of the second geometry. Location value for the boundary of a
		/// geometry.
		/// </summary>
		public const int Boundary = 1;

		/// <summary>  
		/// DE-9IM row index of the exterior of the first geometry and column index of
		/// the exterior of the second geometry. Location value for the exterior of a
		/// geometry.
		/// </summary>
		public const int Exterior = 2;

        private LocationType()
        {
        }

        /// <summary>  
        /// Converts the location value to a location symbol, for example, Exterior => 'e'.
        /// </summary>
        /// <param name="locationValue"> 
        /// Either Exterior, Boundary, Interior or Null
        /// </param>
        /// <returns> Returns either 'e', 'b', 'i' or '-'.</returns>
        public static char ToLocationSymbol(int locationValue)
        {             
            switch (locationValue)
            {
                case Exterior: 
                    return 'e';
        		
                case Boundary: 
                    return 'b';
        		
                case Interior: 
                    return 'i';
        		
                case None: 
                    return '-';
            }

            throw new System.ArgumentException("Unknown location value: " + locationValue);
        }
    }
}