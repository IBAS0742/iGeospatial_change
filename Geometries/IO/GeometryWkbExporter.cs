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

using iGeospatial.Geometries.Exports;

namespace iGeospatial.Geometries.IO
{
	/// <summary>
	/// Summary description for GeometryWkbExporter.
	/// </summary>
	public class GeometryWkbExporter : MarshalByRefObject, IGeometryExporter
	{
        #region Private Members

        private BytesOrder m_enumByteOrder = BytesOrder.LittleEndian;

        #endregion

        #region Constructors and Destructor

        /// <summary>
        /// ?
        /// </summary>
		public GeometryWkbExporter()
		{
        }

		~GeometryWkbExporter()
		{
            Dispose(false);
        }

        #endregion

        #region Public Methods

        public byte[] Export(Geometry geometryObject)
        {
            // TODO:  Add GeometryWkbExporter.Export implementation
            return null;
        }

        #endregion

        #region IGeometryExporter Members

        public BytesOrder ByteOrder
        {
            get
            {
                return m_enumByteOrder;
            }

            set
            {
                m_enumByteOrder = value;
            }
        }

        public GeometryExportType ExportType
        {
            get
            {
                return GeometryExportType.Wkb;
            }
        }

        object IGeometryExporter.Export(Geometry geometryObject)
        {
            // TODO:  Add GeometryWkbExporter.Export implementation
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
