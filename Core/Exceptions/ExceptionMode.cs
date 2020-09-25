using System;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// Enum containing the mode options for the exceptionManagement tag.
    /// </summary>
    [Serializable]
    public enum ExceptionMode 
    {
        /// <summary>
        /// The ExceptionManager should not process exceptions.
        /// </summary>
        Off,

        /// <summary>
        /// The ExceptionManager should process exceptions. This is the 
        /// default.
        /// </summary>
        On
    }
}
