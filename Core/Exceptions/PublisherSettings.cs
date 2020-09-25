using System;
using System.Collections;
using System.Collections.Specialized;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// Class that defines the publisher settings within the exception management settings in 
    /// the config file.
    /// </summary>
    public class PublisherSettings
    {
        private PublisherMode mode = PublisherMode.On;
        private PublisherFormat exceptionFormat = PublisherFormat.Exception;
        private string assemblyName;
        private string typeName;
        private TypeFilter includeTypes;
        private TypeFilter excludeTypes;
        private NameValueCollection otherAttributes = new NameValueCollection();
		
        /// <summary>
        /// Specifies the whether the exceptionManagement settings are "on" or "off".
        /// </summary>
        public PublisherMode Mode
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
        /// Specifies the whether the publisher supports the IExceptionXmlPublisher interface (value is set to "xml")
        /// or the publisher supports the IExceptionPublisher interface (value is either left off or set to "exception").
        /// </summary>
        public PublisherFormat ExceptionFormat
        {
            get
            {
                return exceptionFormat;
            }
            set
            {
                exceptionFormat = value;
            }
        }

        /// <summary>
        /// The assembly name of the publisher component that will be used to invoke the object.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return assemblyName;
            }
            set
            {
                assemblyName = value;
            }
        }

        /// <summary>
        /// The type name of the publisher component that will be used to invoke the object.
        /// </summary>
        public string TypeName
        {
            get
            {
                return typeName;
            }
            set
            {
                typeName = value;
            }
        }

        /// <summary>
        /// A semicolon delimited list of all exception types that the publisher will be invoked for. 
        /// A "*" can be used to specify all types and is the default value if this is left off.
        /// </summary>
        public TypeFilter IncludeTypes
        {
            get
            {
                return includeTypes;
            }
            set
            {
                includeTypes = value;
            }
        }

        /// <summary>
        /// A semicolon delimited list of all exception types that the publisher will not be invoked for. 
        /// A "*" can be used to specify all types. The default is to exclude no types.
        /// </summary>
        public TypeFilter ExcludeTypes
        {
            get
            {
                return excludeTypes;
            }
            set
            {
                excludeTypes = value;
            }
        }
				
        /// <summary>
        /// Determines whether the exception type is to be filtered out based on the includes and exclude
        /// types specified.
        /// </summary>
        /// <param name="exceptionType">The Type of the exception to check for filtering.</param>
        /// <returns>True is the exception type is to be filtered out, false if it is not filtered out.</returns>
        public bool IsExceptionFiltered(Type exceptionType)
        {
            // If no types are excluded then the exception type is not filtered.
            if (excludeTypes == null) return false;

            // If the Type is in the Exclude Filter
            if (MatchesFilter(exceptionType, excludeTypes))
            {
                // If the Type is in the Include Filter
                if (MatchesFilter(exceptionType, includeTypes))
                {
                    // The Type is not filtered out because it was explicitly Included.
                    return false;
                }
                    // If the Type is not in the Exclude Filter
                else
                {
                    // The Type is filtered because it was Excluded and did not match the Include Filter.
                    return true;
                }
            }
                // Otherwise it is not Filtered.
            else
            {
                // The Type is not filtered out because it did not match the Exclude Filter.
                return false;
            }
        }
		
        /// <summary>
        /// Determines if a type is contained the supplied filter. 
        /// </summary>
        /// <param name="type">The Type to look for</param> 
        /// <param name="typeFilter">The Filter to test against the Type</param>
        /// <returns>true or false</returns>
        private bool MatchesFilter(Type type, TypeFilter typeFilter)
        {
            TypeInfo typeInfo;

            // If no filter is provided type does not match the filter.
            if (typeFilter == null) return false;

            // If all types are accepted in the filter (using the "*") return true.
            if (typeFilter.AcceptAllTypes) return true;

            // Loop through the types specified in the filter.
            for (int i=0;i<typeFilter.Types.Count;i++)
            {
                typeInfo = (TypeInfo)typeFilter.Types[i];

                // If the Type matches this type in the Filter, then return true.
                if (typeInfo.ClassType.Equals(type)) return true;

                // If the filter type includes SubClasses of itself (it had a "+" before the type in the
                // configuration file) AND the Type is a SubClass of the filter type, then return true.
                if (typeInfo.IncludeSubClasses == true && typeInfo.ClassType.IsAssignableFrom(type)) return true;			
            }
            // If no matches are found return false.
            return false;
        }

        /// <summary>
        /// A collection of any other attributes included within the publisher tag in the config file. 
        /// </summary>
        public NameValueCollection OtherAttributes
        {
            get
            {
                return otherAttributes;
            }
        }

        /// <summary>
        /// Allows name/value pairs to be added to the Other Attributes collection.
        /// </summary>
        public void AddOtherAttributes(string name, string value)
        {
            otherAttributes.Add(name, value);
        }
    }
}
