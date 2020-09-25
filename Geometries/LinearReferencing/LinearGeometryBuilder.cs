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
using System.Collections;

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;           
	
namespace iGeospatial.Geometries.LinearReferencing
{
	/// <summary> 
	/// Builds a linear geometry (<see cref="LineString"/> or 
	/// <see cref="MultiLineString"/>) incrementally (point-by-point).    
	/// </summary>
	[Serializable]
    public class LinearGeometryBuilder
	{
        private GeometryFactory      geomFact;
        private GeometryList         lines;
        private CoordinateCollection coordList;
		
        private bool ignoreInvalidLines;
        private bool fixInvalidLines;
		
        private Coordinate lastPt;
		
        public LinearGeometryBuilder(GeometryFactory geomFact)
        {
            lines = new GeometryList();

            this.geomFact = geomFact;
        }
		
        /// <summary> 
		/// Gets or sets a value that indicates whether to allows invalid lines 
		/// to be ignored rather than causing Exceptions.
		/// </summary>
		/// <remarks>
		/// An invalid line is one which has only one unique point.
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if short lines are to be ignored.
		/// </value>
		public bool IgnoreInvalidLines
		{
            get
            {
                return this.ignoreInvalidLines;
            }

			set
			{
				this.ignoreInvalidLines = value;
			}
		}
			
		/// <summary> Allows invalid lines to be ignored rather than causing Exceptions.
		/// An invalid line is one which has only one unique point.
		/// 
		/// </summary>
		/// <value>
		/// <see langword="true"/> if short lines are to be ignored.
		/// </value>
		public bool FixInvalidLines
		{
            get
            {
                return this.fixInvalidLines;
            }

			set
			{
				this.fixInvalidLines = value;
			}
		}
			
		public Coordinate LastCoordinate
		{
			get
			{
				return lastPt;
			}
		}
			
		public Geometry Geometry
		{
			get
			{
				// end last line in case it was not done by user
				EndLine();

				return geomFact.BuildGeometry(lines);
			}             
		}

        /// <summary> 
        /// This adds a point to the current line.
		/// </summary>
		/// <param name="pt">the Coordinate to add
		/// </param>
		public void Add(Coordinate pt)
		{
			Add(pt, true);
		}
		
		/// <summary> 
		/// This adds a point to the current line. 
		/// </summary>
		/// <param name="pt">the Coordinate to add
		/// </param>
		public void Add(Coordinate pt, bool allowRepeatedPoints)
		{
			if (coordList == null)
				coordList = new CoordinateCollection();
			
            coordList.Add(pt, allowRepeatedPoints);

			lastPt = pt;
		}
		
		/// <summary> Terminate the current LineString.</summary>
		public void EndLine()
		{
			if (coordList == null)
			{
				return ;
			}

			if (ignoreInvalidLines && coordList.Count < 2)
			{
				coordList = null;
				return ;
			}
			Coordinate[] rawPts = coordList.ToArray();
			Coordinate[] pts    = rawPts;

			if (fixInvalidLines)
				pts = ValidCoordinateSequence(rawPts);
			
			coordList = null;
			LineString line = null;
			try
			{
				line = geomFact.CreateLineString(pts);
			}
			catch (ArgumentException ex)
			{
                ExceptionManager.Publish(ex);

                // exception is due to too few points in line.
				// only propagate if not ignoring short lines
				if (!ignoreInvalidLines)
					throw ex;
			}
			
			if (line != null)
				lines.Add(line);
		}
		
		private Coordinate[] ValidCoordinateSequence(Coordinate[] pts)
		{
			if (pts.Length >= 2)
				return pts;

			Coordinate[] validPts = new Coordinate[]{pts[0], pts[0]};
			
            return validPts;
		}
	}
}