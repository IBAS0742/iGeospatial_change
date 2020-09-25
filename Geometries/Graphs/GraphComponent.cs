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

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A GraphComponent is the parent class for the objects'
	/// that form a graph.  Each GraphComponent can carry a
	/// Label.
	/// </summary>
	[Serializable]
    internal abstract class GraphComponent
	{
        #region Private Fields

		internal Label m_objLabel;

		/// <summary> 
		/// isInResult indicates if this component has already been included in the result
		/// </summary>
		private bool m_bIsInResult;
		private bool m_bIsCovered;
		private bool m_bIsCoveredSet;
		private bool m_bIsVisited;
        
        #endregion
		
        #region Constructors and Destructor

        protected GraphComponent()
        {
        }
		
        protected GraphComponent(Label label)
        {
            m_objLabel = label;
        }
        
        #endregion
		
        #region Public Properties
		
		/// <summary> 
		/// An isolated component is one that does not intersect or touch any other
		/// component.  This is the case if the label has valid locations for
		/// only a single Geometry.
		/// 
		/// </summary>
		/// <value> 
		/// true if this component is isolated
		/// </value>
        public abstract bool Isolated 
        {
            get; 
            set;
        }

		public virtual bool InResult
		{
			get
			{
				return m_bIsInResult;
			}
			
			set
			{
				this.m_bIsInResult = value;
			}
		}
		
		public virtual bool Covered
		{
			get
			{
				return m_bIsCovered;
			}
			
			set
			{
				this.m_bIsCovered    = value;
				this.m_bIsCoveredSet = true;
			}
		}
		
		public virtual bool CoveredSet
		{
			get
			{
				return m_bIsCoveredSet;
			}
		}
		
		public virtual bool Visited
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
		
        public virtual Label Label
        {
            get
            {
                return this.m_objLabel;
            }

            set
            {
                this.m_objLabel = value;
            }
        }

		/// <returns> a coordinate in this component (or null, if there are none)
		/// </returns>
		public abstract Coordinate Coordinate
        {
            get;
        }
        
        #endregion
		
        #region Public Methods
		
		/// <summary> compute the contribution to an IM for this component</summary>
		public abstract void ComputeIM(IntersectionMatrix im);

		/// <summary> Update the IM with the contribution for this component.
		/// A component only contributes if it has a labelling for both parent geometries
		/// </summary>
		public virtual void UpdateIM(IntersectionMatrix im)
		{
			Debug.Assert(m_objLabel.GeometryCount >= 2, "found partial label");
			ComputeIM(im);
		}
        
        #endregion
	}
}