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
using System.Globalization;

using iGeospatial.Geometries.Exports;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryWktExporter.
	/// </summary>
	public class GeometryWktExporter : MarshalByRefObject, IGeometryExporter
	{
        #region Private Members

        private GeometryWktWriter m_objWktWriter;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryWktExporter"/> class.
        /// </summary>
		public GeometryWktExporter() : base()
		{
            m_objWktWriter = new GeometryWktWriter((CultureInfo)null);
        }

        /// <summary>
        /// Allows a <see cref="GeometryWktExporter"/> instance to attempt to 
        /// free resources and perform other cleanup operations before the 
        /// GeometryWktExporter is reclaimed by garbage collection.
        /// </summary>
        ~GeometryWktExporter()
        {
            Dispose(false);
        }
        
        #endregion

        #region Public Methods

        public string Export(Geometry geometryObject)
        {
            if (m_objWktWriter == null)
            {
                m_objWktWriter = new GeometryWktWriter((CultureInfo)null);
            }

            if (geometryObject != null)
            {
                return m_objWktWriter.Write(geometryObject);
            }
 
            return null;
        }

        #endregion

        #region IGeometryExporter Members

        public BytesOrder ByteOrder
        {
            get
            {
                return BytesOrder.LittleEndian;
            }

            set
            {
                // just ignore it!
            }
        }

        public GeometryExportType ExportType
        {
            get
            {
                return GeometryExportType.Wkt;
            }
        }

        object IGeometryExporter.Export(Geometry geometryObject)
        {
            if (m_objWktWriter == null)
            {
                m_objWktWriter = new GeometryWktWriter((CultureInfo)null);
            }

            if (geometryObject != null)
            {
                return m_objWktWriter.Write(geometryObject);
            }
 
            return null;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
