using System;
using System.Runtime.Serialization;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for CoordinateException.
	/// </summary>
    [Serializable]
    public class CoordinateException : ApplicationException
    {
        #region Constructors and Destructor
		
        /// <summary>
        /// Constructor with no params.
        /// </summary>
        public CoordinateException() : base()
        {
        }
		
        /// <summary>
        /// Constructor allowing the Message property to be set.
        /// </summary>
        /// <param name="message">String setting the message of the exception.</param>
        public CoordinateException(string message) : base(message) 
        {
        }
		
        /// <summary>
        /// Constructor allowing the Message and InnerException property to be set.
        /// </summary>
        /// <param name="message">String setting the message of the exception.</param>
        /// <param name="inner">Sets a reference to the InnerException.</param>
        public CoordinateException(string message, Exception inner) : base(message, inner)
        {
        }
		
        /// <summary>
        /// Constructor allowing the Message property to be set through the exception ID.
        /// </summary>
        /// <param name="exceptionID">
        /// A number identifying the message of the exception in resource file.
        /// </param>
        public CoordinateException(int exceptionID) 
        {
        }
		
        /// <summary>
        /// Constructor allowing the Message resource ID and InnerException 
        /// property to be set.
        /// </summary>
        /// <param name="exceptionID">
        /// A number identifying the message of the exception in resource file.
        /// </param>
        /// <param name="inner">Sets a reference to the InnerException.</param>
        public CoordinateException(int exceptionID, Exception inner)
        {
        }   
		
        /// <summary>
        /// Constructor allowing the Message property to be set through the exception ID.
        /// </summary>
        /// <param name="exceptionID">
        /// A number identifying the message of the exception in resource file.
        /// </param>
        public CoordinateException(int exceptionID, params object[] args) 
        {
        }
		
        /// <summary>
        /// Constructor allowing the Message resource ID and InnerException property to be set
        /// </summary>
        /// <param name="exceptionID">
        /// A number identifying the message of the exception in resource file.
        /// </param>
        /// <param name="inner">Sets a reference to the InnerException.</param>
        public CoordinateException(int exceptionID, Exception inner, params object[] args)
        {
        }   
        /// <summary>
        /// Constructor used for deserialization of the exception class.
        /// </summary>
        /// <param name="info">
        /// Represents the SerializationInfo of the exception.
        /// </param>
        /// <param name="context">
        /// Represents the context information of the exception.
        /// </param>
        protected CoordinateException(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        #endregion
    }	
}
