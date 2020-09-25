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
using System.Security.Permissions;
using System.Runtime.Serialization;
                            
using iGeospatial.Exceptions;
using iGeospatial.Coordinates;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for GeometryException.
	/// </summary>
	[Serializable]
	public class GeometryException : BaseException
	{
        private Coordinate pt;
		
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryException"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor initializes the Message property of the new 
        /// instance to a system-supplied message that describes the error, 
        /// such as "An application error has occurred." 
        /// This message takes into account the current system culture.
        /// </remarks>
		public GeometryException() : base()
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryException"/> 
        /// class with a specified error message.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        public GeometryException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryException"/> 
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
        public GeometryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
		
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryException"/> 
        /// class with a specified error message and a coordinate at which the 
        /// error occurred.
        /// </summary>
        /// <param name="message">
        /// A message that describes the error.
        /// </param>
        /// <param name="pt">
        /// The coordinate of the point at which the error occurred.
        /// </param>
        public GeometryException(string message, Coordinate pt) 
            : base(Format(message, pt))
        {
            this.pt = new Coordinate(pt);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryException"/> 
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
        protected GeometryException(SerializationInfo info, 
            StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Coordinate", pt);
        }

        /// <summary>
        /// The coordinate of the point at which the error occurred, if any.
        /// </summary>
        /// <value>
        /// A <see cref="iGeospatial.Coordinates.Coordinate"/> instance of the point
        /// at which problem occurred.
        /// </value>
        public virtual Coordinate Coordinate
        {
            get
            {
                return pt;
            }
        }

        [
        SecurityPermissionAttribute(SecurityAction.Demand, 
            SerializationFormatter=true)
        ]
        public override void GetObjectData(SerializationInfo info, 
            StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);

            pt = (Coordinate)info.GetValue("Coordinate", typeof(Coordinate));
        }


        /// <summary>
        /// Formats a coordinate for output in the exception message text.
        /// </summary>
        /// <param name="msg">
        /// A text to which a formatted coordinate string is appended.
        /// </param>
        /// <param name="pt">
        /// The <see cref="iGeospatial.Coordinates.Coordinate"/> to be formated.
        /// </param>
        /// <returns>
        /// A formatted string of the coordinate and the message text, if the 
        /// coordinate is not null (Nothing in Visual Basic). Otherwise, the 
        /// message is returned an modified.
        /// </returns>
        private static string Format(string msg, Coordinate pt)
        {
            if (pt != null)
            {
                return msg + " [ " + pt + " ]";
            }
            return msg;
        }
    }
}
