using System;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// TypeInfo class contains information about each type within a TypeFilter
    /// </summary>
    public class TypeInfo
    {
        private Type classType;
        private bool includeSubClasses = false;
		
        /// <summary>
        /// Indicates if subclasses are to be included with the type specified in the Include and Exclude filters
        /// </summary>
        public bool IncludeSubClasses
        {
            get
            {
                return includeSubClasses;
            }
			
            set
            {
                includeSubClasses = value;
            }
        }
		
        /// <summary>
        /// The Type class representing the type specified in the Include and Exclude filters
        /// </summary>
        public Type ClassType
        {
            get
            {
                return classType;	
            }
			
            set
            {
                classType = value;
            }
        }
    }
}
