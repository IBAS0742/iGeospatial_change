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

using iGeospatial.Geometries.IO;

namespace iGeospatial.Geometries.Exports
{
	/// <summary>
	/// Providers a common interface for <c>Geometry</c> export.
	/// </summary>
    public interface IGeometryExporter : IDisposable
    {
        /// <summary>
        /// Gets or sets the byte order to be applied to the exported object.
        /// </summary>
        /// <value><c>ByteOrder</c> specifying the by order.</value>
        /// <remarks>
        /// <para>
        /// The byte order is only applicable to binary export. Text exporters
        /// will ignore it.
        /// </para>
        /// The byte order can be one of the following:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Little-endian, in which the bytes of a multibyte value are ordered 
        /// from the least significant to most significant.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Big-endian, in which the bytes of a multibyte value are ordered from 
        /// the most significant to the least significant.
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        BytesOrder ByteOrder {get; set;}

        /// <summary>
        /// Gets the format or export type supported by this exporter instance.
        /// </summary>
        /// <value>
        /// A <c>GeometryExportType</c> specifying the export format.
        /// </value>
        GeometryExportType ExportType {get;}

        /// <summary>
        /// Exports the specified geometry, normally a <see cref="Geometry"/>, 
        /// to the format supported by this instance.
        /// </summary>
        /// <param name="geometryObject">
        /// The Geometry object instance to be exported.
        /// </param>
        /// <returns>
        /// Returns an object in the exported format, depending on the exporter,
        /// or null reference (Nothing in Visual Basic) if not successful.
        /// </returns>
        object Export(Geometry geometryObject);
    }
}
