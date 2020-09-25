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
	/// Summary description for GeometryGml3Writer.
	/// </summary>
	public class GeometryGml3Writer : GeometryTextWriter
	{
        #region Private Fields

        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer() : base()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer(PrecisionModel customPrecision)
            : base(customPrecision)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer(PrecisionModel customPrecision, int indent)
            : base(customPrecision, indent)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer(CultureInfo culture) : base(culture)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer(CultureInfo culture, int indent)
            : base(culture, indent)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer(CultureInfo culture, 
            PrecisionModel customPrecision) : base(culture, customPrecision)
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryGml3Writer"/> class.
        /// </summary>
        public GeometryGml3Writer(CultureInfo culture, 
            PrecisionModel customPrecision, int indent) 
            : base(culture, customPrecision, indent)
        {
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
        public override string Write(Geometry geometry)
        {
            return null;
        }
		
        /// <summary>  
        /// Converts a Geometry to its well-known text representation.
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>
        /// A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification)
        /// </returns>
        public override void Write(Geometry geometry, TextWriter writer)
        {
        }
		
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
        public override string WriteFormatted(Geometry geometry)
        {
            return null;
        }

        /// <summary>  Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// 
        /// </summary>
        /// <param name="geometry"> a Geometry to process
        /// </param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public override void WriteFormatted(Geometry geometry, TextWriter writer)
        {
        }

        /// <summary>  
        /// Same as write, but with newlines and spaces to make the
        /// well-known text more readable.
        /// 
        /// </summary>
        /// <param name="geometry">A Geometry to process.</param>
        /// <returns>A "Geometry Tagged Text" string (see the OpenGIS Simple
        /// Features Specification), with newlines and spaces
        /// </returns>
        public override void WriteFormatted(Geometry geometry, 
            TextWriter writer, int indentation)
        {
        }
        
        #endregion
    }
}
