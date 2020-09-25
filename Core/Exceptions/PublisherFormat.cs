using System;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// Enum containing the format options for the publisher tag.
    /// </summary>
    [Serializable]
    public enum PublisherFormat 
    {
        /// <summary>
        /// The ExceptionManager should call the IExceptionPublisher 
        /// interface of the publisher. This is the default.
        /// </summary>
        Exception,

        /// <summary>
        /// The ExceptionManager should call the 
        /// IExceptionXmlPublisher interface of the publisher.
        /// </summary>
        Xml
    }
}
