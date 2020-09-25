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
	/// Computes the <see cref="LinearLocation"/> of the point on a linear 
	/// <see cref="Geometry"/> nearest a given <see cref="Coordinate"/>.
	/// </summary>
	/// <remarks>
	/// The nearest point is not necessarily unique; this class always computes 
	/// the nearest point closest to the start of the geometry.
	/// </remarks>
    [Serializable]
    internal sealed class LocationIndexOfPoint
	{
        #region Private Fields

        private Geometry linearGeom;
        
        #endregion
		
        #region Constructors and Destructor

        public LocationIndexOfPoint(Geometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }
        
        #endregion
		
        #region Public Methods

		public static LinearLocation IndexOf(Geometry linearGeom, 
            Coordinate inputPt)
		{
			LocationIndexOfPoint locater = new LocationIndexOfPoint(linearGeom);

			return locater.IndexOf(inputPt);
		}
		
		/// <summary> 
		/// Find the nearest location along a linear <see cref="Geometry"/> 
		/// to a given point.
		/// </summary>
		/// <param name="inputPt">The coordinate to locate.</param>
		/// <returns>The location of the nearest point.</returns>
		public LinearLocation IndexOf(Coordinate inputPt)
		{
			return IndexOfFromStart(inputPt, null);
		}
		
		/// <summary> 
		/// Find the nearest <see cref="LinearLocation"/> along the linear 
		/// <see cref="Geometry"/> to a given <see cref="Coordinate"/>
		/// after the specified minimum <see cref="LinearLocation"/>.
		/// </summary>
		/// <param name="inputPt">the coordinate to locate
		/// </param>
		/// <param name="minLocation">the minimum location for the point location
		/// </param>
		/// <returns>The location of the nearest point.</returns>
		/// <remarks>
		/// If possible the location returned will be strictly greater than the
		/// <c>minLocation</c>.
		/// If this is not possible, the
		/// value returned will equal <c>minLocation</c>.
		/// (An example where this is not possible is when
		/// minLocation = [end of line] ).
		/// </remarks>
		public LinearLocation IndexOfAfter(Coordinate inputPt, 
            LinearLocation minIndex)
		{
			if (minIndex == null)
				return IndexOf(inputPt);
			
			// sanity check for minLocation at or past end of line
			LinearLocation endLoc = LinearLocation.GetEndLocation(linearGeom);
			if (endLoc.CompareTo(minIndex) <= 0)
				return endLoc;
			
			LinearLocation closestAfter = IndexOfFromStart(inputPt, minIndex);

            // Return the minDistanceLocation found.
			// This will not be null, since it was initialized to minLocation
            Debug.Assert(closestAfter.CompareTo(minIndex) >= 0, 
                    "computed location is before specified minimum location");
			
            return closestAfter;
		}
		
        public static double SegmentFraction(LineSegment seg, Coordinate inputPt)
        {
            double segFrac = seg.ProjectionFactor(inputPt);

            if (segFrac < 0.0)
                segFrac = 0.0;
            else if (segFrac > 1.0)
                segFrac = 1.0;

            return segFrac;
        }
        
        #endregion
		
        #region Private Methods

		private LinearLocation IndexOfFromStart(Coordinate inputPt, 
            LinearLocation minIndex)
		{
			double minDistance    = Double.MaxValue;
			int minComponentIndex = 0;
			int minSegmentIndex   = 0;
			double minFrac        = - 1.0;
			
			LineSegment seg = new LineSegment(linearGeom.Factory);

			for (LinearIterator it = new LinearIterator(linearGeom); 
                it.HasNext(); it.Next())
			{
				if (!it.EndOfLine)
				{
					seg.p0 = it.SegmentStart;
					seg.p1 = it.SegmentEnd;
					double segDistance = seg.Distance(inputPt);
					double segFrac     = SegmentFraction(seg, inputPt);
					
					int candidateComponentIndex = it.ComponentIndex;
					int candidateSegmentIndex   = it.VertexIndex;
					if (segDistance < minDistance)
					{
						// ensure after minLocation, if any
						if (minIndex == null || minIndex.CompareLocationValues(
                            candidateComponentIndex, candidateSegmentIndex, 
                            segFrac) < 0)
						{
							// otherwise, save this as new minimum
							minComponentIndex = candidateComponentIndex;
							minSegmentIndex   = candidateSegmentIndex;
							minFrac           = segFrac;
							minDistance       = segDistance;
						}
					}
				}
			}

			LinearLocation loc = new LinearLocation(minComponentIndex, 
                minSegmentIndex, minFrac);
			
            return loc;
		}
        
        #endregion
    }
}