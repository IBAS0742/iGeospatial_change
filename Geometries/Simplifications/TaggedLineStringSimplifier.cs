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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Simplifications
{
	/// <summary> 
	/// Simplifies a TaggedLineString, preserving topology
	/// (in the sense that no new intersections are introduced).
	/// Uses the recursive Douglas-Peucker algorithm.
	/// </summary>
	[Serializable]
    public class TaggedLineStringSimplifier
	{
        #region Private Fields

        private static LineIntersector li = new RobustLineIntersector();
		
        private LineSegmentIndex inputIndex;
        private LineSegmentIndex outputIndex;
		
        private double distanceTolerance;
        private TaggedLineString line;
        private Coordinate[] linePts;
		
        /// <summary> 
        /// Index of section to be tested for flattening - reusable
        /// </summary>
        private int[] validSectionIndex = new int[2];
        
        #endregion
		
        #region Constructors and Destructor

        public TaggedLineStringSimplifier()
        {
            this.inputIndex  = new LineSegmentIndex();
            this.outputIndex = new LineSegmentIndex();
        }
		
        public TaggedLineStringSimplifier(LineSegmentIndex inputIndex, 
            LineSegmentIndex outputIndex)
        {
            this.inputIndex  = inputIndex;
            this.outputIndex = outputIndex;
        }
        
        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Gets or sets the distance tolerance for the simplification.
		/// </summary>
		/// <value>
		/// A number specifying the approximation tolerance to use.
		/// </value>
		/// <remarks>
		/// All vertices in the simplified geometry will be within this
		/// distance of the original geometry.
		/// </remarks>
		public double DistanceTolerance
		{
            get
            {
                return this.distanceTolerance;
            }

			set
			{
				this.distanceTolerance = value;
			}
		}
        
        #endregion
			
        #region Public Methods

		public void Simplify(TaggedLineString line)
		{
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }

			this.line = line;
			linePts   = line.ParentCoordinates.ToArray();
			
            SimplifySection(0, linePts.Length - 1, 0);
		}
        
        #endregion
		
        #region Private Methods

		private void SimplifySection(int i, int j, int depth)
		{
			depth += 1;
			int[] sectionIndex = new int[2];
			if ((i + 1) == j)
			{
				LineSegment newSeg = line.GetSegment(i);
				line.AddToResult(newSeg);
				
                // leave this segment in the input index, for efficiency
				return;
			}
			
			bool isValidToSimplify = true;
			
			// Following logic ensures that there is enough points in the output line.
			// If there is already more points than the minimum, there's nothing to check.
			// Otherwise, if in the worst case there wouldn't be enough points,
			// don't flatten this segment (which avoids the worst case scenario)
			if (line.ResultSize < line.MinimumSize)
			{
				int worstCaseSize = depth + 1;
				if (worstCaseSize < line.MinimumSize)
					isValidToSimplify = false;
			}
			
			double[] distance   = new double[1];
			int furthestPtIndex = FindFurthestPoint(linePts, i, j, distance);
			// flattening must be less than distanceTolerance
			if (distance[0] > distanceTolerance)
				isValidToSimplify = false;

			// test if flattened section would cause intersection
			LineSegment candidateSeg = new LineSegment((GeometryFactory)null);
			candidateSeg.p0 = linePts[i];
			candidateSeg.p1 = linePts[j];
			sectionIndex[0] = i;
			sectionIndex[1] = j;
			if (HasBadIntersection(line, sectionIndex, candidateSeg))
				isValidToSimplify = false;
			
			if (isValidToSimplify)
			{
				LineSegment newSeg = Flatten(i, j);
				line.AddToResult(newSeg);
				return ;
			}

			SimplifySection(i, furthestPtIndex, depth);
			SimplifySection(furthestPtIndex, j, depth);
		}
		
		private int FindFurthestPoint(Coordinate[] pts, int i, int j, double[] maxDistance)
		{
			LineSegment seg = new LineSegment((GeometryFactory)null);
			seg.p0 = pts[i];
			seg.p1 = pts[j];
			double maxDist = - 1.0;
			int maxIndex = i;
			for (int k = i + 1; k < j; k++)
			{
				Coordinate midPt = pts[k];
				double distance = seg.Distance(midPt);
				if (distance > maxDist)
				{
					maxDist = distance;
					maxIndex = k;
				}
			}
			maxDistance[0] = maxDist;
			return maxIndex;
		}
		
		private LineSegment Flatten(int start, int end)
		{
			// make a new segment for the simplified geometry
			Coordinate p0      = linePts[start];
			Coordinate p1      = linePts[end];
			LineSegment newSeg = new LineSegment(null, p0, p1);
			
            // update the indexes
			Remove(line, start, end);
			outputIndex.Add(newSeg);

			return newSeg;
		}
				
		private bool HasBadIntersection(TaggedLineString parentLine, 
            int[] sectionIndex, LineSegment candidateSeg)
		{
			if (HasBadOutputIntersection(candidateSeg))
				return true;
			
            if (HasBadInputIntersection(parentLine, 
                sectionIndex, candidateSeg))
				return true;

			return false;
		}
		
		private bool HasBadOutputIntersection(LineSegment candidateSeg)
		{
			IList querySegs = outputIndex.Query(candidateSeg);

            for (IEnumerator i = querySegs.GetEnumerator(); i.MoveNext(); )
			{
				LineSegment querySeg = (LineSegment) i.Current;
				if (HasInteriorIntersection(querySeg, candidateSeg))
				{
					return true;
				}
			}
			return false;
		}
		
		private bool HasBadInputIntersection(TaggedLineString parentLine, 
            int[] sectionIndex, LineSegment candidateSeg)
		{
			IList querySegs = inputIndex.Query(candidateSeg);
			
			for (IEnumerator i = querySegs.GetEnumerator(); i.MoveNext(); )
			{
				TaggedLineSegment querySeg = (TaggedLineSegment) i.Current;
				if (HasInteriorIntersection(querySeg, candidateSeg))
				{
					if (IsInLineSection(parentLine, sectionIndex, querySeg))
						continue;
					return true;
				}
			}
			return false;
		}
		
		private bool HasInteriorIntersection(LineSegment seg0, 
            LineSegment seg1)
		{
			li.ComputeIntersection(seg0.p0, seg0.p1, seg1.p0, seg1.p1);
			
            return li.IsInteriorIntersection();
		}
		
		/// <summary> Remove the segs in the section of the line</summary>
		/// <param name="line">
		/// </param>
		/// <param name="pts">
		/// </param>
		/// <param name="sectionStartIndex">
		/// </param>
		/// <param name="sectionEndIndex">
		/// </param>
		private void Remove(TaggedLineString line, int start, int end)
		{
			for (int i = start; i < end; i++)
			{
				TaggedLineSegment seg = line.GetSegment(i);

				inputIndex.Remove(seg);
			}
		}
		
		/// <summary> 
		/// Tests whether a segment is in a section of a TaggedLineString.
		/// </summary>
		/// <param name="line">
		/// </param>
		/// <param name="sectionIndex">
		/// </param>
		/// <param name="seg">
		/// </param>
		/// <returns>
		/// </returns>
		private static bool IsInLineSection(TaggedLineString line, 
            int[] sectionIndex, TaggedLineSegment seg)
		{
			// not in this line
			if (seg.Parent != line.Parent)
				return false;

			int segIndex = seg.Index;
			
            if (segIndex >= sectionIndex[0] && segIndex < sectionIndex[1])
				return true;
			
            return false;
		}
        
        #endregion
    }
}