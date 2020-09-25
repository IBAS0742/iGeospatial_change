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
	/// An iterator over the components and coordinates of a linear geometry
	/// (<see cref="LineString"/>s and <see cref="MultiLineString"/>s.
	/// </summary>
	/// <example>
	/// The standard usage pattern for a LinearIterator is:
	/// <code lang="C#">
	/// for (LinearIterator it = new LinearIterator(...); it.HasNext(); it.Next()) 
	/// {
	/// ...
	/// int ci = it.ComponentIndex;   // for example
	/// int vi = it.VertexIndex;      // for example
	/// ...
	/// }
	/// </code>
	/// </example>
    [Serializable]
    public sealed class LinearIterator
	{
        #region Private Fields

        private Geometry linear;
        private int numLines;
		
        /// <summary> 
        /// Invariant: currentLine <> null if the iterator is pointing at a 
        /// valid coordinate
        /// </summary>
        private LineString currentLine;
        private int componentIndex = 0;
        private int vertexIndex = 0;
        
        #endregion
		
        #region Constructors and Destructor

        /// <summary> 
        /// Creates an iterator initialized to the start of a linear 
        /// <see cref="Geometry"/>.
        /// </summary>
        /// <param name="linear">
        /// The linear geometry to iterate over.
        /// </param>
        public LinearIterator(Geometry linear) : this(linear, 0, 0)
        {
        }
		
        /// <summary> 
        /// Creates an iterator starting at a <see cref="LinearLocation"/> on 
        /// a linear <see cref="Geometry"/>.
        /// </summary>
        /// <param name="linear">The linear geometry to iterate over.</param>
        /// <param name="start">The location to start at.</param>
        public LinearIterator(Geometry linear, LinearLocation start) 
            : this(linear, start.ComponentIndex, SegmentEndVertexIndex(start))
        {
        }
		
        /// <summary> 
        /// Creates an iterator starting at a component and vertex in 
        /// a linear <see cref="Geometry"/>.
        /// </summary>
        /// <param name="linear">The linear geometry to iterate over.</param>
        /// <param name="componentIndex">The component to start at.</param>
        /// <param name="vertexIndex">The vertex to start at.</param>
        public LinearIterator(Geometry linear, int componentIndex, 
            int vertexIndex)
        {
            this.linear         = linear;
            this.numLines       = linear.NumGeometries;
            this.componentIndex = componentIndex;
            this.vertexIndex    = vertexIndex;
            
            LoadCurrentLine();
        }
        
        #endregion
		
        #region Public Properties

        /// <summary> 
		/// Gets a value that checks whether the iterator cursor is pointing 
		/// to the endpoint of a linestring.
		/// </summary>
		/// <value> 
		/// This property returns <see langword="true"/> if the iterator is at 
		/// an endpoint; otherwise, <see langword="false"/>.
		/// </value>
		public bool EndOfLine
		{
			get
			{
				if (componentIndex >= numLines)
					return false;
				//LineString currentLine = (LineString) linear.getGeometryN(componentIndex);
				if (vertexIndex < currentLine.NumPoints - 1)
					return false;

				return true;
			}
		}
			
		/// <summary> 
		/// Gets the component index of the vertex the iterator is currently at.
		/// </summary>
		/// <value>A number specifying the current component index.</value>
		public int ComponentIndex
		{
			get
			{
				return componentIndex;
			}
		}
			
		/// <summary> 
		/// Gets the vertex index of the vertex the iterator is currently at.
		/// </summary>
		/// <value>A number specifying the current vertex index.</value>
		public int VertexIndex
		{
			get
			{
				return vertexIndex;
			}
		}
			
		/// <summary> 
		/// Gets the <see cref="LineString"/> component the iterator is 
		/// current at.
		/// </summary>
		/// <value> 
		/// A <see cref="LineString"/> object at the current position 
		/// of iteration.
		/// </value>
		public LineString Line
		{
			get
			{
				return currentLine;
			}
		}
			
		/// <summary> 
		/// Gets the first <see cref="Coordinate"/> of the current segment.
		/// (the coordinate of the current vertex).
		/// </summary>
		/// <value> 
		/// A <see cref="Coordinate"/> object specifying the first coordinate
		/// of the current segement.
		/// </value>
		public Coordinate SegmentStart
		{
			get
			{
				return currentLine.GetCoordinate(vertexIndex);
			}
		}
			
		/// <summary>  
		/// Gets the second <see cref="Coordinate"/> of the current segment (the 
		/// coordinate of the next vertex).
		/// </summary>
		/// <value> 
		/// A <see cref="Coordinate"/> object specifying second coordinate of the 
		/// current segment, or <see langword="null"/> if the iterator is at the 
		/// end of a line.
		/// </value>
		public Coordinate SegmentEnd
		{
			get
			{
				if (vertexIndex < Line.NumPoints - 1)
					return currentLine.GetCoordinate(vertexIndex + 1);

				return null;
			}
		}
        
        #endregion
			
        #region Private Methods

		private static int SegmentEndVertexIndex(LinearLocation loc)
		{
			if (loc.SegmentFraction > 0.0)
				return loc.SegmentIndex + 1;

			return loc.SegmentIndex;
		}
		
		private void LoadCurrentLine()
		{
			if (componentIndex >= numLines)
			{
				currentLine = null;
				
                return;
			}

			currentLine = (LineString)linear.GetGeometry(componentIndex);
		}
        
        #endregion
		
        #region Public Methods

		/// <summary> 
		/// Tests whether there are any vertices left to iterator over.</summary>
		/// <returns> 
		/// Returns <see langword="true"/> if there are more vertices to scan.
		/// </returns>
		public bool HasNext()
		{
			if (componentIndex >= numLines)
				return false;

			if (componentIndex == (numLines - 1) && 
                vertexIndex >= currentLine.NumPoints)
				return false;

			return true;
		}
		
		/// <summary> 
		/// Moves the iterator ahead to the next vertex and (possibly) 
		/// linear component.
		/// </summary>
		public void Next()
		{
			if (!HasNext())
				return ;
			
			vertexIndex++;
			if (vertexIndex >= currentLine.NumPoints)
			{
				componentIndex++;
				
                LoadCurrentLine();
				
                vertexIndex = 0;
			}
		}
        
        #endregion
	}
}