using System;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// Enum containing the mode options for the publisher tag.
    /// </summary>
    [Serializable]
    public enum PublisherMode
    {
        /// <summary>
        /// The ExceptionManager should not call the publisher.
        /// </summary>
        Off,

        /// <summary>
        /// The ExceptionManager should call the publisher. This 
        /// is the default.
        /// </summary>
        On		
    }
}
