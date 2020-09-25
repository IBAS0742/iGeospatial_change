using System;
using System.Collections;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// Class that defines the exception management settings in the config file.
    /// </summary>
    public class ExceptionSettings
    {
        private ExceptionMode mode       = ExceptionMode.On;
        private ArrayList     publishers = new ArrayList();

        /// <summary>
        /// Specifies the whether the exceptionManagement settings are "on" or "off".
        /// </summary>
        public ExceptionMode Mode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
            }
        }

        /// <summary>
        /// An ArrayList containing all of the PublisherSettings listed in the config file.
        /// </summary>
        public ArrayList Publishers
        {
            get
            {
                return publishers;
            }
        }

        /// <summary>
        /// Adds a PublisherSettings to the arraylist of publishers.
        /// </summary>
        public void AddPublisher(PublisherSettings publisher)
        {
            publishers.Add(publisher);
        }
    }
}
