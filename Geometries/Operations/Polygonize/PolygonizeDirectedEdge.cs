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
using iGeospatial.Geometries;
using iGeospatial.Geometries.PlanarGraphs;

namespace iGeospatial.Geometries.Operations.Polygonize
{
	/// <summary> A <see cref="DirectedEdge"/> of a <see cref="PolygonizeGraph"/>, 
	/// which represents an edge of a polygon formed by the graph.
	/// May be logically deleted from the graph by setting the marked flag.
	/// </summary>
	internal class PolygonizeDirectedEdge : DirectedEdge
	{
        #region Private Members
        
        private EdgeRing               edgeRing;
        private PolygonizeDirectedEdge next;
        private long                   label = -1;

        #endregion
		
        #region Constructors and Destructor
        
        /// <summary>
  		/// Constructs a directed edge connecting the from node to the to node.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="directionPt">
 		/// specifies this DirectedEdge's direction (given by an imaginary
		/// line from the from node to directionPt)
        /// </param>
        /// <param name="edgeDirection">
		/// whether this DirectedEdge's direction is the same as or
		/// opposite to that of the parent Edge (if any)
        /// </param>
        public PolygonizeDirectedEdge(Node from, Node to, Coordinate directionPt, 
            bool edgeDirection) : base(from, to, directionPt, edgeDirection)
		{
		}

        #endregion
		
        #region Public Properties

        /// <summary>
        /// Gets or sets the identifier attached to this directed edge.
        /// </summary>
        /// <value>
        /// The identifier attached to this edge.
        /// </value>
        public virtual long Label
		{
			get
			{
				return label;
			}
			
			set
			{
				this.label = value;
			}
			
		}

		/// <summary>
		/// Gets or sets the next directed edge in the EdgeRing that this directed 
		/// edge is a member of.
		/// </summary>
		/// <value>
		/// The next <see cref="PolygonizeDirectedEdge"/> edge in the EdgeRing.
		/// </value>
		public virtual PolygonizeDirectedEdge Next
		{
			get
			{
				return next;
			}
			
			set
			{
				this.next = value;
			}
			
		}

		/// <summary> Returns the ring of directed edges that this directed edge is
		/// a member of, or null if the ring has not been set.
		/// </summary>
		/// <seealso cref="PolygonizeDirectedEdge.Ring">
		/// </seealso>
		public virtual bool InRing
		{
			get
			{
				return edgeRing != null;
			}
			
		}

		/// <summary> Sets the ring of directed edges that this directed edge is
		/// a member of.
		/// </summary>
		public virtual EdgeRing Ring
		{
			set
			{
				this.edgeRing = value;
			}
		}

        #endregion
	}
}