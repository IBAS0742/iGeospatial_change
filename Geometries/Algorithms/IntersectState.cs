using System;

namespace iGeospatial.Geometries.Algorithms
{
    /// <summary>
    /// Specifies the intersection state of two line segments.  
    /// </summary>
    [Serializable]
    public enum IntersectState
    {
        /// <summary>
        /// Indicates lines do not intersect.
        /// </summary>
        DonotIntersect = 0,

        /// <summary>
        /// Indicates the lines do intersect.
        /// </summary>
        DoIntersect    = 1,

        /// <summary>
        /// Indicates the lines are passing through or lying on the same 
        /// straight path. The lines are coaxial.
        /// </summary>
        Collinear      = 2
    }
}
