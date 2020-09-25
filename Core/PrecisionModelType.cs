using System;

namespace iGeospatial
{
    /// <summary>
    /// The types of numerical precision models supported by a coordinate or 
    /// a coordinate list.
    /// </summary>
    [Serializable]
    public enum PrecisionModelType : short
    {
        /// <summary> 
        /// Fixed Precision indicates that coordinates have a fixed number of decimal places.
        /// The number of decimal places is determined by the log10 of the scale factor.
        /// </summary>
        Fixed          = 1,

        /// <summary> 
        /// Floating precision corresponds to the standard .NET double-precision 
        /// floating-point representation, which is based on the IEEE-754 standard
        /// </summary>
        Floating       = 2,
		
        /// <summary> Floating single precision corresponds to the standard .NET
        /// single-precision floating-point representation, which is based on 
        /// the IEEE-754 standard.
        /// </summary>
        FloatingSingle = 3,

        /// <summary>
        /// Indicates a custom precision model.
        /// </summary>
        Custom         = 4
    }
	
}
