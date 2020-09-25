using System;

namespace iGeospatial.Geometries.Operations
{
	/// <summary>
	/// This is used in the <see cref="ValidationError"/> class to
	/// indicate the type of error, which occurred during the topological
	/// validation test with the <see cref="IsValidOp"/> class.
	/// </summary>
    [Serializable]
    public enum ValidationErrorType
    {
        /// <summary>
        /// Topology Validation Error.
        /// </summary>
        Error                = 0,

        /// <summary>
        /// Repeated Point.
        /// </summary>
        RepeatedPoint        = 1,

        /// <summary>
        /// Hole lies outside shell.
        /// </summary>
        HoleOutsideShell     = 2,

        /// <summary>
        /// Holes are nested.
        /// </summary>
        NestedHoles          = 3,

        /// <summary>
        /// Interior is disconnected.
        /// </summary>
        DisconnectedInterior = 4,

        /// <summary>
        /// Self-intersection.
        /// </summary>
        SelfIntersection     = 5,

        /// <summary>
        /// Ring Self-intersection.
        /// </summary>
        RingSelfIntersection = 6,

        /// <summary>
        /// Nested shells.
        /// </summary>
        NestedShells         = 7,

        /// <summary>
        /// Duplicate Rings.
        /// </summary>
        DuplicateRings       = 8,

        /// <summary>
        /// Too few points in geometry component.
        /// </summary>
        TooFewPoints         = 9,

        /// <summary>
        /// Invalid Coordinate.
        /// </summary>
        InvalidCoordinate    = 10,
        
        /// <summary>
        /// Ring is not closed
        /// </summary>
        RingNotClosed        = 11
    }
}
