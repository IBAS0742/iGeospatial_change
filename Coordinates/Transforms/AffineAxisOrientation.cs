using System;

namespace iGeospatial.Coordinates.Transforms
{
	/// <summary>
	/// Specifies the orientation or direction a coordinate system axis.
	/// </summary>
	/// <remarks>
	/// This is restricted to the 2-dimensional Cartesian coordinates
	/// system.
	/// </remarks>
	[Serializable]
    public enum AffineAxisOrientation : short
	{
        /// <summary>
        /// Unknown or unspecified orientation.
        /// </summary>
        None  = 0,

        /// <summary>
        /// Pointing or directed upwards.
        /// </summary>
        Up    = 1,

        /// <summary>
        /// Pointing or directed downwards.
        /// </summary>
        Down  = 2,

        /// <summary>
        /// Pointing or directed to the left.
        /// </summary>
        Left  = 3,

        /// <summary>
        /// Pointing or directed to the right.
        /// </summary>
        Right = 4
	}
}
