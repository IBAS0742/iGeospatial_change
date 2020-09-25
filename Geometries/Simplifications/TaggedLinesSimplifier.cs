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

using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Simplifications
{
	/// <summary> 
	/// Simplifies a collection of TaggedLineStrings, preserving topology
	/// (in the sense that no new intersections are introduced).
	/// </summary>
	[Serializable]
    public class TaggedLinesSimplifier
	{
        #region Private Fields

        private LineSegmentIndex inputIndex;
        private LineSegmentIndex outputIndex;
        private double distanceTolerance;
        
        #endregion
		
        #region Constructors and Destructor

        public TaggedLinesSimplifier()
        {
            inputIndex   = new LineSegmentIndex();
            outputIndex  = new LineSegmentIndex();
        }
		
        public TaggedLinesSimplifier(double tolerance)
        {
            if (tolerance < 0.0)
                throw new ArgumentException("Tolerance must be non-negative");
				
            inputIndex   = new LineSegmentIndex();
            outputIndex  = new LineSegmentIndex();
            distanceTolerance = tolerance;
        }
        
        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Gets or sets the distance tolerance for the simplification.
		/// All vertices in the simplified geometry will be within this
		/// distance of the original geometry.
		/// </summary>
		/// <value>
		/// A number specifying the approximation tolerance to use.
		/// </value>
		public double DistanceTolerance
		{
            get
            {
                return this.distanceTolerance;
            }

			set
			{
                if (value < 0.0)
                    throw new ArgumentException("Tolerance must be non-negative");
				
                this.distanceTolerance = value;
			}
		}
        
        #endregion

        #region Public Methods

		/// <summary> 
		/// Simplify a collection of <see cref="TaggedLineString"/>s.
		/// </summary>
		/// <param name="taggedLines">the collection of lines to simplify
		/// </param>
		public void Simplify(ICollection taggedLines)
		{
            if (taggedLines == null)
            {
                throw new ArgumentNullException("taggedLines");
            }

            for (IEnumerator i = taggedLines.GetEnumerator(); i.MoveNext(); )
			{
				inputIndex.Add((TaggedLineString) i.Current);
			}

			for (IEnumerator i = taggedLines.GetEnumerator(); i.MoveNext(); )
			{
				TaggedLineStringSimplifier tlss = 
                    new TaggedLineStringSimplifier(inputIndex, outputIndex);
				tlss.DistanceTolerance = distanceTolerance;

                tlss.Simplify((TaggedLineString) i.Current);
			}
		}
        
        #endregion
	}
}