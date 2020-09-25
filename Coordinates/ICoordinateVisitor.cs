using System;

namespace iGeospatial.Coordinates
{
	/// <summary>  Geometry classes support the concept of applying a
	/// coordinate filter to every coordinate in the Geometry. A
	/// coordinate filter can either record information about each coordinate or
	/// change the coordinate in some way. Coordinate filters implement the
	/// interface ICoordinateVisitor. (ICoordinateVisitor is
	/// an example of the Gang-of-Four Visitor pattern). Coordinate filters can be
	/// used to implement such things as coordinate transformations, centroid and
	/// envelope computation, and many other functions.
	/// </summary>
	public interface ICoordinateVisitor
	{
		/// <summary>  Performs an operation with or on coord.
		/// 
		/// </summary>
		/// <param name="coord"> a Coordinate to which the filter is applied.
		/// </param>
		void  Visit(Coordinate coord);
	}
}