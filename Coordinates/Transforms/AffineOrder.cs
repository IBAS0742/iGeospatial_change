using System;

namespace iGeospatial.Coordinates.Transforms
{
	/// <summary>
	/// Specifies the order for matrix transform operations.
	/// </summary>
	/// <remarks>
	/// Matrix transform operations are not necessarily commutative. 
	/// The order in which they are applied is important.
	/// </remarks>
	[Serializable]
    public enum AffineOrder : short
	{
        /// <summary>
        /// The new operation is applied after the old operation.
        /// </summary>
        Append  = 1,
                    
        /// <summary>
        /// The new operation is applied before the old operation.
        /// </summary>
        Prepend = 2
	}
}
