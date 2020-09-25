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
	/// Summary description for GeometryTextWriter.
	/// </summary>
	public abstract class GeometryTextWriter : MarshalByRefObject
	{
        #region Internal Members

        /// <summary>
        /// The text indentation size.
        /// </summary>
        internal static int       DefaultIndent = 2;
		
        internal int              m_nIndent;
        internal bool             m_bIsFormatted;
        internal bool             m_bIncludeSRID;
        internal bool             m_bApplyPrecision;
        internal bool             m_bIsMeasured;
        internal CultureInfo      m_objCulture;
        internal PrecisionModel   m_objCurPrecision;
        internal PrecisionModel   m_objUserPrecision;
        internal IFormatProvider  m_objProvider;
        internal TextNumberFormat m_objFormatter;
		
        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter()
        {
            m_nIndent      = DefaultIndent;
            m_bIncludeSRID = true;
            m_bIsFormatted = true;   
            m_bIsMeasured  = false;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter(PrecisionModel customPrecision)
        {
            m_nIndent          = DefaultIndent;
            m_objUserPrecision = customPrecision;
            m_bIncludeSRID     = true;
            m_bApplyPrecision  = true;
            m_bIsFormatted     = true;
            m_bIsMeasured      = false;

            if (m_objUserPrecision != null)
            {
                m_objFormatter = CreateFormatter(m_objUserPrecision);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter(PrecisionModel customPrecision, int indent)
        {
            m_nIndent          = indent;
            m_objUserPrecision = customPrecision;
            m_bIncludeSRID     = true;
            m_bApplyPrecision  = true;
            m_bIsFormatted     = true;
            m_bIsMeasured      = false;

            if (m_objUserPrecision != null)
            {
                m_objFormatter = CreateFormatter(m_objUserPrecision);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter(CultureInfo culture)
        {
            m_nIndent      = DefaultIndent;
            m_objCulture   = culture;
            m_bIncludeSRID = true;
            m_bIsFormatted = true;
            m_bIsMeasured  = false;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter(CultureInfo culture, int indent)
        {
            m_nIndent      = indent;
            m_objCulture   = culture;
            m_bIncludeSRID = true;
            m_bIsFormatted = true;
            m_bIsMeasured  = false;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter(CultureInfo culture, 
            PrecisionModel customPrecision)
        {
            m_nIndent          = DefaultIndent;
            m_objCulture       = culture;
            m_bIncludeSRID     = true;
            m_objUserPrecision = customPrecision;
            m_bIsFormatted     = true;
            m_bIsMeasured      = false;

            if (m_objUserPrecision != null)
            {
                m_objFormatter = CreateFormatter(m_objUserPrecision);
            }
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextWriter"/> class.
        /// </summary>
        protected GeometryTextWriter(CultureInfo culture, 
            PrecisionModel customPrecision, int indent)
        {
            m_nIndent          = indent;
            m_objCulture       = culture;
            m_bIncludeSRID     = true;
            m_objUserPrecision = customPrecision;
            m_bIsFormatted     = true;
            m_bIsMeasured      = false;

            if (m_objUserPrecision != null)
            {
                m_objFormatter = CreateFormatter(m_objUserPrecision);
            }
        }

        #endregion

        #region Public Properties

        public int Indentation
        {
            get
            {
                return m_nIndent;
            }

            set
            {
                m_nIndent = value;
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return m_objCulture;
            }
        }

        public PrecisionModel CustomPrecision
        {
            get
            {
                return m_objUserPrecision;
            }

            set
            {
                m_objUserPrecision = value;
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

        public bool IncludeSRID
        {
            get
            {
                return m_bIncludeSRID;
            }

            set
            {
                m_bIncludeSRID = value;
            }
        }

        #endregion
		
        #region Public Methods

        /// <summary>  
        /// Converts a Geometry to its well-known text representation.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification)
        /// </returns>
        public abstract string Write(Geometry geometry);
		
        /// <summary>  
        /// Converts a Geometry to its well-known text representation.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification)
        /// </returns>
        public abstract void Write(Geometry geometry, TextWriter writer);
		
        /// <summary>  
        /// Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public abstract string WriteFormatted(Geometry geometry);

        /// <summary>  Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// 
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public abstract void WriteFormatted(Geometry geometry, TextWriter writer);

        /// <summary>  
        /// Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// 
        /// </summary>
        /// <param name="geometry">A Geometry to process.</param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public abstract void WriteFormatted(Geometry geometry, 
            TextWriter writer, int indentation);
        
        #endregion

        #region Internal Methods
		
        /// <summary>  
        /// Converts a double to a string, not in scientific notation.
        /// </summary>
        /// <param name="d">The double to convert.</param>
        /// <returns>
        /// The double as a String, not in scientific notation.
        /// </returns>
        internal virtual string WriteNumber(double d)
        {                           
            return m_objFormatter.FormatDouble(d);
        }
		
        internal virtual void Indent(int level, TextWriter writer)
        {
            if (!m_bIsFormatted || level <= 0)
                return;

            writer.Write("\n");
            writer.Write(new string(' ', m_nIndent * level));
        }

        internal virtual void UpdateFormatter(PrecisionModel precision)
        {
            if (m_objUserPrecision == null)
            {
                if ((m_objFormatter == null || m_objCurPrecision == null) ||
                    !m_objCurPrecision.Equals(precision))
                {
                    m_objCurPrecision = precision;
                    m_objFormatter    = CreateFormatter(m_objCurPrecision);
                }
            }
            else
            {
                if (m_objFormatter == null)
                {
                    m_objFormatter = CreateFormatter(m_objUserPrecision);
                }
            }
        }

        /// <summary>
        /// Creates the DecimalFormat used to write doubles with a sufficient 
        /// number of decimal places.
        /// </summary>
        /// <param name="precisionModel">
        /// The PrecisionModel used to determine the number of decimal places to 
        /// write.
        /// </param>
        /// <returns>
        /// A DecimalFormat that write double without scientific notation.
        /// </returns>
        internal virtual TextNumberFormat CreateFormatter(PrecisionModel precisionModel)
        {
            if (m_objUserPrecision != null)
            {
                precisionModel = m_objUserPrecision;
            }

            // the default number of decimal places is 16, which is sufficient
            // to accomodate the maximum precision of a double.
            int decimalPlaces = precisionModel.MaximumSignificantDigits;
            // specify decimal separator explicitly to avoid problems in other locales

            NumberFormatInfo symbols = null;
            if (m_objCulture != null)
            {
                symbols = m_objCulture.NumberFormat;
            }

            if (symbols == null)
            {
                symbols = new NumberFormatInfo();
                if (decimalPlaces > 0)
                    symbols.NumberDecimalSeparator = '.'.ToString();
            }

            m_objProvider = symbols;

            TextNumberFormat numberFormatter  = TextNumberFormat.GetTextNumberInstance(symbols); 
            numberFormatter.MaxFractionDigits = decimalPlaces;
            numberFormatter.DirectFormat      = true;

            return numberFormatter;
        }

        #endregion
    }
}
