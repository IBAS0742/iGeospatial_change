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
	/// Determines the location of a subline along a linear <see cref="Geometry"/>.
	/// The location is reported as a pair of <see cref="LinearLocation"/>s.
	/// </summary>
	/// <remarks>
	/// <note>
	/// Currently this algorithm is not guaranteed to return the correct 
	/// substring in some situations where an endpoint of the test line occurs 
	/// more than once in the input line. (However, the common case of a ring 
	/// is always handled correctly).
	/// </note>
	/// </remarks>
    [Serializable]
    internal sealed class LocationIndexOfLine
	{
        #region Private Fields

        private Geometry linearGeom;
        
        #endregion
		
        #region Constructors and Destructor

        public LocationIndexOfLine(Geometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// MD - this algorithm has been extracted into a class because it is 
		/// intended to validate that the subline truly is a subline, and also 
		/// to use the internal vertex information to unambiguously locate 
		/// the subline.
		/// </summary>
		public static LinearLocation[] IndicesOf(Geometry linearGeom, 
            Geometry subLine)
		{
			LocationIndexOfLine locater = new LocationIndexOfLine(linearGeom);

			return locater.IndicesOf(subLine);
		}
		
		public LinearLocation[] IndicesOf(Geometry subLine)
		{
			Coordinate startPt  = 
                ((LineString)subLine.GetGeometry(0)).Coordinates[0];
			LineString lastLine = 
                (LineString)subLine.GetGeometry(subLine.NumGeometries - 1);
			Coordinate endPt    = lastLine.Coordinates[lastLine.NumPoints - 1];
			
			LocationIndexOfPoint locPt  = new LocationIndexOfPoint(linearGeom);
			LinearLocation[] subLineLoc = new LinearLocation[2];
			subLineLoc[0] = locPt.IndexOf(startPt);
			
			// check for case where subline is zero length
			if (subLine.Length == 0.0)
			{
				subLineLoc[1] = subLineLoc[0].Clone();
			}
			else
			{
				subLineLoc[1] = locPt.IndexOfAfter(endPt, subLineLoc[0]);
			}

			return subLineLoc;
		}
        
        #endregion
	}
}