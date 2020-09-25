using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// The types of coordinates modelled by this library.
	/// </summary>
	[Serializable]
    public enum CoordinateType
	{
        /// <summary>
        /// A normal coordinate, which contains no special information.
        /// </summary>
        Default   = 1,

        /// <summary>
        /// A measured coordinate, which contains measure information as use
        /// in Shapefile formats.
        /// </summary>
        Measured  = 2,

        /// <summary>
        /// A flagged coordinate, which contains a property indicating whether
        /// the point is visible or not.
        /// </summary>
        Flagged   = 3,

        /// <summary>
        /// A tagged coordinate, which contains a special tagged information.
        /// </summary>
        Tagged    = 4,

        /// <summary>
        /// A user defined or custom coordinate
        /// </summary>
        Custom    = 10
	}
}
