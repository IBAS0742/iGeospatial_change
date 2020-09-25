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

namespace iGeospatial.Geometries.Simplifications
{
	/// <summary> 
	/// A <see cref="LineSegment"/> which is tagged with its location in a 
	/// <see cref="Geometry"/>.
	/// </summary>
	/// <remarks>
	/// Used to index the segments in a geometry and recover the segment 
	/// locations from the index. 
	/// </remarks>
	[Serializable]
	public class TaggedLineSegment : LineSegment
	{
        #region Private Fields

        private Geometry parent;
        private int index;
        
        #endregion
		
        #region Constructors and Destructor

        public TaggedLineSegment(Coordinate p0, Coordinate p1, 
            Geometry parent, int index) 
            : base((parent == null) ? null : parent.Factory, p0, p1)
        {
            this.parent = parent;
            this.index = index;
        }
		
        public TaggedLineSegment(Coordinate p0, Coordinate p1) 
            : this(p0, p1, null, -1)
        {
        }
        
        #endregion

        #region Public Properties

		public Geometry Parent
		{
			get
			{
				return parent;
			}
		}
			
		public int Index
		{
			get
			{
				return index;
			}
		}
        
        #endregion
	}
}