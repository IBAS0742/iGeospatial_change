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
// The Polygon Triangulation is based on codes developed by
//          Frank Shen                                    
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;
using System.Runtime.Serialization;

namespace iGeospatial.Geometries.Triangulations
{
	/// <summary>
	/// Summary description for NoValidReturnException.
	/// </summary>
    [Serializable]
    public class NonValidReturnException : GeometryException
	{
		public NonValidReturnException() : base()
		{
		}

		public NonValidReturnException(string msg) : base(msg)
		{
			string errMsg="\nThere is no valid return value available!";
			throw new NonValidReturnException(errMsg);
		}

		public NonValidReturnException(string msg, Exception inner) 
            : base(msg, inner)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="NonValidReturnException"/> 
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
        protected NonValidReturnException(SerializationInfo info, 
            StreamingContext context) : base(info, context)
        {
        }
	}

    [Serializable]
    public class InvalidInputGeometryDataException : GeometryException
	{
		public InvalidInputGeometryDataException() : base()
		{
		}
		
		public InvalidInputGeometryDataException(string msg) : base(msg)
		{
		}

		public InvalidInputGeometryDataException(string msg, Exception inner) 
            : base(msg, inner)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputGeometryDataException"/> 
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
        protected InvalidInputGeometryDataException(SerializationInfo info, 
            StreamingContext context) : base(info, context)
        {
        }
	}
}
