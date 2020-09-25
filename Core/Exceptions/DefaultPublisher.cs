using System;
using System.Text;
using System.Security;
using System.Resources;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// Component used as the default publishing component if one is not specified in the config file.
    /// </summary>
    public sealed class DefaultPublisher : IExceptionPublisher
    {
        #region Private Fields

        private static ResourceManager resourceManager = new ResourceManager(typeof(ExceptionManager).Namespace + ".ExceptionManagerText",
            Assembly.GetAssembly(typeof(ExceptionManager)));
		
        // Member variable declarations
        private string logName = "Application";
        private string applicationName = resourceManager.GetString("RES_EXCEPTIONMANAGER_PUBLISHED_EXCEPTIONS");
        private const string TEXT_SEPARATOR = "*********************************************";
        
        #endregion
		
        #region Constructors and Destructor
        
        /// <summary>
        /// Default Constructor.
        /// </summary>
        public DefaultPublisher()
        {
        }

        /// <summary>
        /// Constructor allowing the log name and application names to be set.
        /// </summary>
        /// <param name="logName">The name of the log for the DefaultPublisher to use.</param>
        /// <param name="applicationName">The name of the application.  This is used as the Source name in the event log.</param>
        public DefaultPublisher(string logName, string applicationName)
        {
            this.logName = logName;
            this.applicationName = applicationName;
        }
        
        #endregion
		
        #region IExceptionPublisher Members

        /// <summary>
        /// Method used to publish exception information and additional information.
        /// </summary>
        /// <param name="exception">The exception object whose information should be published.</param>
        /// <param name="additionalInfo">A collection of additional data that should be published along with the exception information.</param>
        /// <param name="configSettings">A collection of any additional attributes provided in the config settings for the custom publisher.</param>
        public void Publish(Exception exception, NameValueCollection additionalInfo, NameValueCollection configSettings)
        {
            // Load Config values if they are provided.
            if (configSettings != null)
            {
                if (configSettings["applicationName"] != null && configSettings["applicationName"].Length > 0) applicationName = configSettings["applicationName"];
                if (configSettings["logName"] != null && configSettings["logName"].Length > 0)  logName = configSettings["logName"];
            }

            // Verify that the Source exists before gathering exception information.
            VerifyValidSource();

            // Create StringBuilder to maintain publishing information.
            StringBuilder strInfo = new StringBuilder();

            #region Record the contents of the AdditionalInfo collection
            // Record the contents of the AdditionalInfo collection.
            if (additionalInfo != null)
            {
                // Record General information.
                strInfo.AppendFormat("{0}General Information {0}{1}{0}Additional Info:", Environment.NewLine, TEXT_SEPARATOR);

                foreach (string i in additionalInfo)
                {
                    strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, i, additionalInfo.Get(i));
                }
            }
            #endregion

            if (exception == null)
            {
                strInfo.AppendFormat("{0}{0}No Exception object has been provided.{0}", Environment.NewLine);
            }
            else
            {
                #region Loop through each exception class in the chain of exception objects
                // Loop through each exception class in the chain of exception objects.
                Exception currentException = exception;	// Temp variable to hold InnerException object during the loop.
                int intExceptionCount = 1;				// Count variable to track the number of exceptions in the chain.
                do
                {
                    // Write title information for the exception object.
                    strInfo.AppendFormat("{0}{0}{1}) Exception Information{0}{2}", Environment.NewLine, intExceptionCount.ToString(), TEXT_SEPARATOR);
                    strInfo.AppendFormat("{0}Exception Type: {1}", Environment.NewLine, currentException.GetType().FullName);
				
                    #region Loop through the public properties of the exception object and record their value
                    // Loop through the public properties of the exception object and record their value.
                    PropertyInfo[] aryPublicProperties = currentException.GetType().GetProperties();
                    NameValueCollection currentAdditionalInfo;
                    foreach (PropertyInfo p in aryPublicProperties)
                    {
                        // Do not log information for the InnerException or StackTrace. This information is 
                        // captured later in the process.
                        if (p.Name != "InnerException" && p.Name != "StackTrace")
                        {
                            if (p.GetValue(currentException,null) == null)
                            {
                                strInfo.AppendFormat("{0}{1}: NULL", Environment.NewLine, p.Name);
                            }
                            else
                            {
                                // Loop through the collection of AdditionalInformation if the exception type is a BaseException.
                                if (p.Name == "AdditionalInformation" && currentException is BaseException)
                                {
                                    // Verify the collection is not null.
                                    if (p.GetValue(currentException,null) != null)
                                    {
                                        // Cast the collection into a local variable.
                                        currentAdditionalInfo = (NameValueCollection)p.GetValue(currentException,null);

                                        // Check if the collection contains values.
                                        if (currentAdditionalInfo.Count > 0)
                                        {
                                            strInfo.AppendFormat("{0}AdditionalInformation:", Environment.NewLine);

                                            // Loop through the collection adding the information to the string builder.
                                            for (int i = 0; i < currentAdditionalInfo.Count; i++)
                                            {
                                                strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, currentAdditionalInfo.GetKey(i), currentAdditionalInfo[i]);
                                            }
                                        }
                                    }
                                }
                                    // Otherwise just write the ToString() value of the property.
                                else
                                {
                                    strInfo.AppendFormat("{0}{1}: {2}", Environment.NewLine, p.Name, p.GetValue(currentException,null));
                                }
                            }
                        }
                    }
                    #endregion
                    #region Record the Exception StackTrace
                    // Record the StackTrace with separate label.
                    if (currentException.StackTrace != null)
                    {
                        strInfo.AppendFormat("{0}{0}StackTrace Information{0}{1}", Environment.NewLine, TEXT_SEPARATOR);
                        strInfo.AppendFormat("{0}{1}", Environment.NewLine, currentException.StackTrace);
                    }
                    #endregion

                    // Reset the temp exception object and iterate the counter.
                    currentException = currentException.InnerException;
                    intExceptionCount++;
                } while (currentException != null);
                #endregion
            }

            // Write the entry to the event log.   
            WriteToLog(strInfo.ToString(), EventLogEntryType.Error);
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Helper function to write an entry to the Event Log.
        /// </summary>
        /// <param name="entry">The entry to enter into the Event Log.</param>
        /// <param name="type">The EventLogEntryType to be used when the entry is logged to the Event Log.</param>
        private void WriteToLog(string entry, EventLogEntryType type)
        {
            try
            {
                // Write the entry to the Event Log.
                EventLog.WriteEntry(applicationName,entry,type);
            }
            catch(SecurityException e)
            {				
                throw new SecurityException(String.Format(resourceManager.GetString("RES_DEFAULTPUBLISHER_EVENTLOG_DENIED"), applicationName),e);
            }
        }

        private void VerifyValidSource()
        {
            try
            {
                if (!EventLog.SourceExists(applicationName))
                {
                    EventLog.CreateEventSource(applicationName, logName);
                }
            }
            catch(SecurityException e)
            {
                throw new SecurityException(String.Format(resourceManager.GetString("RES_DEFAULTPUBLISHER_EVENTLOG_DENIED"), applicationName),e);
            }
        }
        
        #endregion
    }
}
