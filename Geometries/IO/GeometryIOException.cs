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
using System.Runtime.Serialization;

namespace iGeospatial.Geometries.IO
{
	/// <summary>  
	/// Thrown by a reader when a parsing problem occurs. 
	/// </summary>
	[Serializable]
    public class GeometryIOException : GeometryException
	{
        public GeometryIOException()
        {
        }

		/// <summary>  Creates a GeometryIOException with the given detail message.
		/// 
		/// </summary>
		/// <param name="message"> a description of this GeometryIOException
		/// </param>
		public GeometryIOException(string message) : base(message)
		{
		}
		
		/// <summary>  Creates a GeometryIOException with es detail message.
		/// 
		/// </summary>
		/// <param name="e"> an exception that occurred while a WKTReader was
		/// parsing a Well-known Text string
		/// </param>
		public GeometryIOException(string message, System.Exception e) 
            : base(message, e)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryIOException"/> 
        /// class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The object that holds the serialized object data.
        /// </param>
        /// <param name="context">
        /// The contextual information about the source or destination.
        /// </param>
        /// <remarks>
        /// This constructor is called during deserialization to reconstitute 
        /// the exception object transmitted over a stream.
        /// </remarks>
        protected GeometryIOException(SerializationInfo info, 
            StreamingContext context) : base(info, context)
        {
        }
	}
}