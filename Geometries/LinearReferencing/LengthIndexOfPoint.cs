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
	/// Computes the length index of the point on a linear <see cref="Geometry"/> 
	/// nearest a given <see cref="Coordinate"/>.
	/// </summary>
	/// <remarks>
	/// The nearest point is not necessarily unique; this class
	/// always computes the nearest point closest to
	/// the start of the geometry.
	/// </remarks>
	[Serializable]
    internal sealed class LengthIndexOfPoint
	{
        private Geometry linearGeom;
		
        public LengthIndexOfPoint(Geometry linearGeom)
        {
            this.linearGeom = linearGeom;
        }
		
		public static double IndexOf(Geometry linearGeom, Coordinate inputPt)
		{
			LengthIndexOfPoint locater = new LengthIndexOfPoint(linearGeom);

			return locater.IndexOf(inputPt);
		}
		
		public static double IndexOfAfter(Geometry linearGeom, 
            Coordinate inputPt, double minIndex)
		{
			LengthIndexOfPoint locater = new LengthIndexOfPoint(linearGeom);

			return locater.IndexOfAfter(inputPt, minIndex);
		}
		
		/// <summary> 
		/// Find the nearest location along a linear <see cref="Geometry"/> 
		/// to a given point.
		/// </summary>
		/// <param name="inputPt">The coordinate to locate.</param>
		/// <returns>The location of the nearest point.</returns>
		public double IndexOf(Coordinate inputPt)
		{
			return IndexOfFromStart(inputPt, -1.0);
		}
		
		/// <summary> 
		/// Finds the nearest index along the linear <see cref="Geometry"/>
		/// to a given <see cref="Coordinate"/> after the specified minimum index.
		/// </summary>
		/// <remarks>
		/// If possible the location returned will be strictly greater than the
		/// <code>minLocation</code>.
		/// If this is not possible, the
		/// value returned will equal <code>minLocation</code>.
		/// (An example where this is not possible is when
		/// minLocation = [end of line] ).
		/// </remarks>
		/// <param name="inputPt">The coordinate to locate.</param>
		/// <param name="minLocation">
		/// The minimum location for the point location.
		/// </param>
		/// <returns>The location of the nearest point.</returns>
		public double IndexOfAfter(Coordinate inputPt, double minIndex)
		{
			if (minIndex < 0.0)
				return IndexOf(inputPt);
			
			// sanity check for minIndex at or past end of line
			double endIndex = linearGeom.Length;
			if (endIndex < minIndex)
				return endIndex;
			
			double closestAfter = IndexOfFromStart(inputPt, minIndex);

            // Return the minDistanceLocation found.
			// This will not be null, since it was initialized to minLocation
			Debug.Assert(closestAfter > minIndex, 
                    "computed index is before specified minimum index");
			
            return closestAfter;
		}
		
		private double IndexOfFromStart(Coordinate inputPt, double minIndex)
		{
			double minDistance = Double.MaxValue;
			
			double ptMeasure           = minIndex;
			double segmentStartMeasure = 0.0;
			
            LineSegment seg   = new LineSegment(linearGeom.Factory);
			LinearIterator it = new LinearIterator(linearGeom);

			while (it.HasNext())
			{
				if (!it.EndOfLine)
				{
					seg.p0 = it.SegmentStart;
					seg.p1 = it.SegmentEnd;
					double segDistance = seg.Distance(inputPt);
					double segMeasureToPt = SegmentNearestMeasure(seg, 
                        inputPt, segmentStartMeasure);

					if (segDistance < minDistance && segMeasureToPt > minIndex)
					{
						ptMeasure   = segMeasureToPt;
						minDistance = segDistance;
					}

					segmentStartMeasure += seg.Length;
				}

				it.Next();
			}

			return ptMeasure;
		}
		
		private double SegmentNearestMeasure(LineSegment seg, 
            Coordinate inputPt, double segmentStartMeasure)
		{
			// found new minimum, so compute location distance of point
			double projFactor = seg.ProjectionFactor(inputPt);

			if (projFactor <= 0.0)
				return segmentStartMeasure;
			
            if (projFactor <= 1.0)
				return segmentStartMeasure + projFactor * seg.Length;
			
            // projFactor > 1.0
			return segmentStartMeasure + seg.Length;
		}
	}
}