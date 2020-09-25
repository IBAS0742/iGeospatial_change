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

namespace iGeospatial.Geometries.Algorithms
{
	/// <summary>
	/// An exception thrown by members classes of the iGeospatial.Geometries.Algorithms namespace.
	/// </summary>
	[Serializable]
	public class AlgorithmException : GeometryException
	{
        #region Constructor and Destructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmException"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor initializes the Message property of the new 
        /// instance to a system-supplied message that describes the error, 
        /// such as "An application error has occurred." 
        /// This message takes into account the current system culture.
        /// </remarks>
		public AlgorithmException() : base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmException"/> 
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        public AlgorithmException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmException"/> 
        /// class with a specified error message and a reference to the inner 
        /// exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The error message that explains the reason for the exception.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception. 
        /// If the innerException parameter is not a null reference, 
        /// the current exception is raised in a catch block that handles 
        /// the inner exception.
        /// </param>
        public AlgorithmException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AlgorithmException"/> 
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
        protected AlgorithmException(SerializationInfo info, 
            StreamingContext context) : base(info, context)
        {
        }

        #endregion
	}
}
