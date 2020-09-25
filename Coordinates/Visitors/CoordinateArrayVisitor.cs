using System;

namespace iGeospatial.Coordinates.Visitors
{
	/// <summary>  
	/// A <see cref="ICoordinateVisitor"/> that creates an array containing every
	/// coordinate in a Geometry.
	/// </summary>
	public class CoordinateArrayVisitor : ICoordinateVisitor
	{
        #region Private Fields

        private Coordinate[] pts = null;
        private int n            = 0;
        
        #endregion
		
		/// <summary>  
		/// Constructs a new instance of <see cref="CoordinateArrayVisitor"/>.
		/// </summary>
		/// <param name="size"> 
		/// The number of points that the CoordinateArrayVisitor will collect.
		/// </param>
		public CoordinateArrayVisitor(int size)
		{
			pts = new Coordinate[size];
		}
		
        /// <summary>  
        /// Returns the gathered Coordinates.
		/// </summary>
		/// <returns>
		/// The Coordinates collected by this CoordinateArrayVisitor.
		/// </returns>
		public virtual Coordinate[] Coordinates
		{
			get
			{
				return pts;
			}
		}
		
		public virtual void Visit(Coordinate coord)
		{
			pts[n++] = coord;
		}
	}
}