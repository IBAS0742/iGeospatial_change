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
using System.Diagnostics;

using iGeospatial.Coordinates;           

namespace iGeospatial.Geometries.LinearReferencing
{
	/// <summary> 
	/// Extracts the subline of a linear <see cref="Geometry"/> between
	/// two <see cref="LinearLocation"/>s on the line.
	/// </summary>
    [Serializable]
    internal sealed class ExtractLineByLocation
	{
        #region Private Fields

        private Geometry line;
        
        #endregion
		
        #region Constructors and Destructor

        public ExtractLineByLocation(Geometry line)
        {
            this.line = line;
        }

        #endregion
		
		/// <summary> 
		/// Computes the subline of a <see cref="LineString"/> between
		/// two <see cref="LineStringLocation"/>s on the line.
		/// If the start location is after the end location,
		/// the computed geometry is reversed.
		/// </summary>
		/// <param name="line">The line to use as the baseline.</param>
		/// <param name="start">The start location.</param>
		/// <param name="end">The end location.</param>
		/// <returns> Returns the extracted subline.</returns>
		public static Geometry Extract(Geometry line, LinearLocation start, 
            LinearLocation end)
		{
			ExtractLineByLocation ls = new ExtractLineByLocation(line);

			return ls.Extract(start, end);
		}
		
		/// <summary> 
		/// This extracts a subline of the input.
		/// If <c>end &lt; start</c> the linear geometry computed will be reversed.
		/// </summary>
		/// <param name="start">The start location.</param>
		/// <param name="end">The end location.</param>
		/// <returns>A linear geometry.</returns>
		public Geometry Extract(LinearLocation start, LinearLocation end)
		{
			if (end.CompareTo(start) < 0)
			{
				return Reverse(ComputeLinear(end, start));
			}

			return ComputeLinear(start, end);
		}
		
		private Geometry Reverse(Geometry linear)
		{
            GeometryType geomType = linear.GeometryType;
                     
            if (geomType == GeometryType.LineString)
                return ((LineString)linear).ReverseAll();
            if (geomType == GeometryType.LinearRing)
                return ((LinearRing)linear).ReverseAll();
            if (geomType == GeometryType.MultiLineString)
                return ((MultiLineString)linear).ReverseAll();

            Debug.Assert(false, "non-linear geometry encountered");
			
            return null;
		}
		/// <summary> 
		/// Assumes input is valid (e.g. start &lt;= end)
		/// </summary>
		/// <param name="start">
		/// </param>
		/// <param name="end">
		/// </param>
		/// <returns>A linear geometry.</returns>
		private LineString ComputeLine(LinearLocation start, LinearLocation end)
		{
            ICoordinateList coordinates         = line.Coordinates;
            CoordinateCollection newCoordinates = new CoordinateCollection();
			
			int startSegmentIndex = start.SegmentIndex;
			if (start.SegmentFraction > 0.0)
				startSegmentIndex += 1;
			int lastSegmentIndex = end.SegmentIndex;
			if (end.SegmentFraction == 1.0)
				lastSegmentIndex += 1;
			if (lastSegmentIndex >= coordinates.Count)
				lastSegmentIndex = coordinates.Count - 1;
			// not needed - LinearLocation values should always be correct
			//Assert.isTrue(end.getSegmentFraction() <= 1.0, "invalid segment fraction value");
			
			if (!start.Vertex)
				newCoordinates.Add(start.GetCoordinate(line));

			for (int i = startSegmentIndex; i <= lastSegmentIndex; i++)
			{
				newCoordinates.Add(coordinates[i]);
			}
			if (!end.Vertex)
				newCoordinates.Add(end.GetCoordinate(line));
			
			// ensure there is at least one coordinate in the result
			if (newCoordinates.Count <= 0)
				newCoordinates.Add(start.GetCoordinate(line));
			
			Coordinate[] newCoordinateArray = newCoordinates.ToArray();

            // Ensure there is enough coordinates to build a valid line.
			// Make a 2-point line with duplicate coordinates, if necessary.
			// There will always be at least one coordinate in the coordList.
			if (newCoordinateArray.Length <= 1)
			{
				newCoordinateArray = new Coordinate[]{newCoordinateArray[0], 
                                                         newCoordinateArray[0]};
			}

			return line.Factory.CreateLineString(newCoordinateArray);
		}
		
		/// <summary> 
		/// Assumes input is valid (e.g. start &lt;= end)
		/// </summary>
		/// <param name="start">
		/// </param>
		/// <param name="end">
		/// </param>
		/// <returns> a linear geometry
		/// </returns>
		private Geometry ComputeLinear(LinearLocation start, LinearLocation end)
		{
			LinearGeometryBuilder builder = new LinearGeometryBuilder(line.Factory);
			builder.FixInvalidLines = true;
			
			if (!start.Vertex)
				builder.Add(start.GetCoordinate(line));
			
			for (LinearIterator it = new LinearIterator(line, start); 
                it.HasNext(); it.Next())
			{
				if (end.CompareLocationValues(it.ComponentIndex, 
                    it.VertexIndex, 0.0) < 0)
					break;
				
				Coordinate pt = it.SegmentStart;
				builder.Add(pt);
				if (it.EndOfLine)
					builder.EndLine();
			}
			if (!end.Vertex)
				builder.Add(end.GetCoordinate(line));
			
			return builder.Geometry;
		}
	}
}