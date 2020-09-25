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

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Dissolves a noded collection of <see cref="SegmentString"/>s to produce
	/// a set of merged linework with unique segments.
	/// A custom merging strategy can be applied when two identical (up to orientation)
	/// strings are dissolved together.
	/// The default merging strategy is simply to discard the merged string.
	/// <p>
	/// A common use for this class is to merge noded edges
	/// while preserving topological labelling.
	/// 
	/// </summary>
	/// <seealso cref="SegmentStringMerger"/>
	internal class SegmentStringDissolver
	{
		public interface ISegmentStringMerger
		{
			/// <summary> Updates the context data of a SegmentString
			/// when an identical (up to orientation) one is found during dissolving.
			/// 
			/// </summary>
			/// <param name="mergeTarget">the segment string to update
			/// </param>
			/// <param name="ssToMerge">the segment string being dissolved
			/// </param>
			/// <param name="isSameOrientation"><see langword="true"/> if the strings are in the same direction,
			/// <see langword="false"/> if they are opposite
			/// </param>
			void Merge(SegmentString mergeTarget, SegmentString ssToMerge, bool isSameOrientation);
		}
		
		private SegmentStringDissolver.ISegmentStringMerger merger;

		private IDictionary ocaMap = new SortedList();
		
		/// <summary> Creates a dissolver with a user-defined merge strategy.
		/// 
		/// </summary>
		/// <param name="merger">the merging strategy to use
		/// </param>
		public SegmentStringDissolver(
            SegmentStringDissolver.ISegmentStringMerger merger)
		{
			this.merger = merger;
		}
		
		/// <summary> Creates a dissolver with the default merging strategy.</summary>
		public SegmentStringDissolver() : this(null)
		{
		}
		
		/// <summary> Gets the collection of dissolved (i.e. unique) <see cref="SegmentString"/>s
		/// 
		/// </summary>
		/// <returns> the unique <see cref="SegmentString"/>s
		/// </returns>
		public virtual ICollection Dissolved
		{
			get
			{
				return ocaMap.Values;
			}
		}
			
		/// <summary> Dissolve all <see cref="SegmentString"/>s in the input {@link Collection}</summary>
		/// <param name="segStrings">
		/// </param>
		public virtual void Dissolve(IList segStrings)
		{
			for (IEnumerator i = segStrings.GetEnumerator(); i.MoveNext(); )
			{
				Dissolve((SegmentString)i.Current);
			}
		}
		
		private void Add(OrientedCoordinateArray oca, 
            SegmentString segString)
		{
			ocaMap[oca] = segString;
		}
		
		/// <summary> Dissolve the given <see cref="SegmentString"/>.
		/// 
		/// </summary>
		/// <param name="segString">the string to dissolve
		/// </param>
		public void Dissolve(SegmentString segString)
		{
			OrientedCoordinateArray oca = new OrientedCoordinateArray(
                segString.Coordinates);
			SegmentString existing      = FindMatching(oca, segString);
			if (existing == null)
			{
				Add(oca, segString);
			}
			else
			{
				if (merger != null)
				{
					bool isSameOrientation = Equals(existing.Coordinates, 
                        segString.Coordinates);

					merger.Merge(existing, segString, isSameOrientation);
				}
			}
		}
		
		
        /// <summary> 
        /// Returns true if the two arrays are identical, both null, or pointwise
        /// equal (as compared using <see cref="Coordinate.Equals"/>).
        /// </summary>
        /// <seealso cref="Coordinate.Equals">Coordinate.Equals</seealso>
        private static bool Equals(ICoordinateList coord1, 
            ICoordinateList coord2)
        {
            if (coord1 == coord2)
                return true;
			
            if (coord1 == null || coord2 == null)
                return false;
			
            if (coord1.Count != coord2.Count)
                return false;

            for (int i = 0; i < coord1.Count; i++)
            {
                if (!coord1[i].Equals(coord2[i]))
                    return false;
            }

            return true;
        }

		private SegmentString FindMatching(OrientedCoordinateArray oca, 
            SegmentString segString)
		{
			SegmentString matchSS = (SegmentString) ocaMap[oca];
			return matchSS;
		}
	}
}