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

namespace iGeospatial.Geometries.PlanarGraphs
{
	/// <summary> 
	/// The base class for all graph component classes.
	/// </summary>
	/// <remarks>
	/// The <see cref="PlanarGraphObject"/> maintains flags of use in generic graph algorithms.
	/// Provides two flags:
	/// <list type="number">
	/// <item>
	/// <description>
	/// marked - typically this is used to indicate a state that persists
	/// for the course of the graph's lifetime.  For instance, it can be
	/// used to indicate that a component has been logically deleted from the graph.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// visited - this is used to indicate that a component has been processed
	/// or visited by an single graph algorithm.  For instance, a breadth-first 
	/// traversal of the graph might use this to indicate that a node has already 
	/// been traversed.
	/// The visited flag may be set and cleared many times during the lifetime of a graph.
	/// </description>
	/// </item>
	/// </list>
    /// Graph components support storing user context data.  This will typically be
    /// used by client algorithms which use planar graphs.
    /// </remarks>
	internal abstract class PlanarGraphObject
	{
        #region Private Fields

        internal bool   m_bIsMarked;
        internal bool   m_bIsVisited;
        internal object m_objData;

        #endregion
		
        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanarGraphObject"/> class.
        /// </summary>
        protected PlanarGraphObject()
        {
        }

        #endregion
		
        #region Public Properties

		/// <summary> 
		/// Gets or sets a value that indicates whether a component 
		/// has been visited during the course of a graph algorithm.
		/// </summary>
		/// <value>true if the component has been visited</value>
		public bool Visited
		{
			get
			{
				return m_bIsVisited;
			}
			
			set
			{
				this.m_bIsVisited = value;
			}
		}

		/// <summary> 
		/// Gets or sets a value that indicates whether if a component has 
		/// been marked at some point during the processing involving this graph.
		/// </summary>
		/// <value>true if the component has been marked.</value>
		public bool Marked
		{
			get
			{
				return m_bIsMarked;
			}
			
			set
			{
				this.m_bIsMarked = value;
			}
		}

        /// <summary> 
        /// Gets or sets the user-defined data for this component.
        /// </summary>
        /// <value> the user-defined data
        /// </value>
        public virtual object Context
        {
            get
            {
                return m_objData;
            }
			
            set
            {
                m_objData = value;
            }
        }

        /// <summary> 
        /// Gets or sets the user-defined data for this component.
        /// </summary>
        /// <value> the user-defined data
        /// </value>
        public virtual object Data
        {
            get
            {
                return m_objData;
            }
			
            set
            {
                m_objData = value;
            }
        }
			
        /// <summary> 
        /// Tests whether this component has been removed from its 
        /// containing graph
        /// </summary>
        /// <value> 
        /// <see langword="true"/> if this component is removed.
        /// </value>
        public abstract bool IsRemoved
        {
            get;
        }
        
        #endregion

        #region Public Static Methods

        /// <summary> 
        /// Sets the Visited state for all <see cref="PlanarGraphObject"/>s 
        /// in an <see cref="IEnumerator"/>.
        /// 
        /// </summary>
        /// <param name="i">
        /// The <see cref="IEnumerator"/> to scan
        /// </param>
        /// <param name="visited">
        /// The state to set the visited flag to.
        /// </param>
        public static void SetVisited(IEnumerator i, bool visited)
        {
            while (i.MoveNext())
            {
                PlanarGraphObject comp = (PlanarGraphObject) i.Current;
                comp.Visited        = visited;
            }
        }
		
        /// <summary> 
        /// Sets the Marked state for all <see cref="PlanarGraphObject"/>s 
        /// in an <see cref="IEnumerator"/>.
        /// </summary>
        /// <param name="i">the Iterator to scan
        /// </param>
        /// <param name="marked">
        /// The state to set the Marked flag to.
        /// </param>
        public static void SetMarked(IEnumerator i, bool marked)
        {
            while (i.MoveNext())
            {
                PlanarGraphObject comp = (PlanarGraphObject) i.Current;
                comp.Marked         = marked;
            }
        }
		
        /// <summary> 
        /// Finds the first <see cref="PlanarGraphObject"/> in a 
        /// <see cref="IEnumerator"/> set which has the specified 
        /// visited state.
        /// </summary>
        /// <param name="i">
        /// An <see cref="IEnumerator"/> of GraphComponents
        /// </param>
        /// <param name="visitedState">
        /// The visited state to test.
        /// </param>
        /// <returns> 
        /// The first component found, or <code>null</code> if none found
        /// </returns>
        public static PlanarGraphObject GetComponentWithVisitedState(
            IEnumerator i, bool visitedState)
        {
            while (i.MoveNext())
            {
                PlanarGraphObject comp = (PlanarGraphObject) i.Current;
                if (comp.Visited == visitedState)
                    return comp;
            }

            return null;
        }
		
        #endregion
    }
}