using System;

namespace iGeospatial.Coordinates.Visitors
{
	/// <summary>  
	/// A ICoordinateVisitor that counts the total number of coordinates in a Geometry.
	/// </summary>
	public class CoordinateCountVisitor : ICoordinateVisitor
	{
        private int n = 0;
		
        public CoordinateCountVisitor()
        {
        }
		
        /// <summary>  
        /// Returns the result of the filtering.
		/// </summary>
		/// <returns>    the number of points found by this CoordinateCountVisitor
		/// </returns>
		public virtual int Count
		{
			get
			{
				return n;
			}
		}
		
		public virtual void  Visit(Coordinate coord)
		{
			n++;
		}
	}
}