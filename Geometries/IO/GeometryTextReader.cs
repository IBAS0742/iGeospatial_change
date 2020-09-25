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
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using iGeospatial.Texts;
using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryTextReader.
	/// </summary>
	public abstract class GeometryTextReader : MarshalByRefObject
	{
        #region Private Members

        internal bool             m_bApplyPrecision;
        internal bool             m_bMeasured;
        internal GeometryFactory  m_objFactory;
        internal PrecisionModel   m_objPrecision;
        internal NumberFormatInfo m_objNumberInfo;
        internal IFormatProvider  m_objProvider;
		
        #endregion

        #region Constructors and Destructor

        protected GeometryTextReader() : this(new GeometryFactory())
        {
        }

        protected GeometryTextReader(GeometryFactory geometryFactory)
		{
            if (geometryFactory == null)
            {
                throw new ArgumentNullException("geometryFactory");
            }

            m_objFactory      = geometryFactory;
            m_objPrecision    = m_objFactory.PrecisionModel;

            m_objNumberInfo   = CultureInfo.InvariantCulture.NumberFormat;
            m_objProvider     = m_objNumberInfo;
            m_bApplyPrecision = true;
        }

        #endregion
		
        #region Public Properties

        public GeometryFactory Factory
        {
            get
            {
                return m_objFactory;
            }
        }

        public PrecisionModel Precision
        {
            get
            {
                return m_objPrecision;
            }

            set
            {
                if (value != null)
                {
                    m_objPrecision = value;
                }
            }
        }

        public bool ApplyPrecision
        {
            get
            {
                return m_bApplyPrecision;
            }

            set
            {
                m_bApplyPrecision = value;
            }
        }

        public NumberFormatInfo NumberFormat
        {
            get
            {
                return m_objNumberInfo;
            }

            set
            {
                if (value != null)
                {
                    m_objNumberInfo = value;
                    m_objProvider   = m_objNumberInfo;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary> 
        /// Converts a Well-known Text representation to a Geometry.
        /// </summary>
        /// <param name="">wellKnownText
        /// one or more "Geometry Tagged Text" strings (see the OpenGIS
        /// Simple Features Specification) separated by whitespace
        /// </param>
        /// <returns> a Geometry specified by wellKnownText
        /// </returns>
        /// <exception cref="GeometryIOException">
        /// If a parsing problem occurs.
        /// </exception>
        public abstract Geometry Read(string wellKnownText);
		
        /// <summary>  
        /// Converts a Well-known Text representation to a Geometry.
        /// </summary>
        /// <param name="reader">A reader, which will return a "Geometry Tagged Text"
        /// string (see the OpenGIS Simple Features Specification)
        /// </param>
        /// <returns> A Geometry read from reader. </returns>  
        /// <exception cref="GeometryIOException">
        /// If a parsing problem occurs.
        /// </exception>
        public abstract Geometry Read(TextReader reader);
        
        #endregion
    }
}
