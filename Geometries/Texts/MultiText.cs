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

namespace iGeospatial.Geometries.Texts
{
	/// <summary>
	/// Summary description for MultiText.
	/// </summary>
	[Serializable]
    public class MultiText : GeometryCollection
	{
        #region Private Fields

        private float      m_fRotation = Single.NaN;

        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiText"/> geometry.
        /// </summary>
        /// <param name="factory">
        /// The <see cref="GeometryFactory">geometry factory</see>, which 
        /// created this curve instance.
        /// </param>
        public MultiText(GeometryFactory factory) : base(factory)
        {
        }
		
        public MultiText(Text[] texts, GeometryFactory factory)
            : base(texts, factory)
        {
        }
		
        public MultiText(Text[] texts, float rotation, GeometryFactory factory)
            : base(texts, factory)
        {
            m_fRotation = rotation;
        }

        #endregion
		
        #region Public Properties

        public override DimensionType Dimension
        {
            get 
            {
                return DimensionType.Point;
            }
        }
		
        public override DimensionType BoundaryDimension
        {
            get 
            {
                return DimensionType.Empty;
            }
        }
		
        public override GeometryType GeometryType
        {
            get 
            {
                return GeometryType.MultiText;
            }
        }
		
        public override string Name
        {
            get 
            {
                return "MultiText";
            }
        }
		
        public override bool IsSimple
        {
            get 
            {
                return true;
            }
        }
		
        public override Geometry Boundary
        {
            get 
            {
                return null;
            }
        }

        public float Rotation
        {
            get
            {
                return m_fRotation;
            }

            set
            {
                m_fRotation = value;
            }
        }
		
        public override bool EqualsExact(Geometry other, double tolerance)
        {
            if (!IsEquivalentType(other))
            {
                return false;
            }

            return base.EqualsExact(other, tolerance);
        }
        
        #endregion
    }
}
