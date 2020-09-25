using System;
using System.Collections;

namespace iGeospatial.Exceptions
{
    /// <summary>
    /// TypeFilter class stores contents of the Include and Exclude filters provided in the
    /// configuration file
    /// </summary>
    public class TypeFilter
    {	
        private bool acceptAllTypes = false;
        private ArrayList types = new ArrayList();
		
        /// <summary>
        /// Indicates if all types should be accepted for a filter
        /// </summary>
        public bool AcceptAllTypes
        {
            get
            {
                return acceptAllTypes;
            }
			
            set
            {
                acceptAllTypes = value;
            }
        }
		
        /// <summary>
        /// Collection of types for the filter
        /// </summary>
        public ArrayList Types
        {
            get
            {
                return types;
            }
        }
    }
}
