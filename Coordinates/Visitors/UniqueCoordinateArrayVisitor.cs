using System;
using System.Collections;

using iGeospatial.Collections;
using iGeospatial.Collections.Sets;

namespace iGeospatial.Coordinates.Visitors
{
	/// <summary>  
	/// A ICoordinateVisitor that builds a set of Coordinates.
	/// The set of coordinates Contains no duplicate points.
	/// </summary>
	public class UniqueCoordinateArrayVisitor : ICoordinateVisitor
	{
        private CoordinateCollection list;
        private ISet                 m_objSet;

        public UniqueCoordinateArrayVisitor()
        {
            list     = new CoordinateCollection();
            m_objSet = new HashedSet();
        }
		
		/// <summary>  
		/// Returns the gathered Coordinates.
		/// </summary>
		/// <returns> 
		/// Returns the Coordinates collected by this CoordinateArrayVisitor
		/// </returns>
		public virtual ICoordinateList Coordinates
		{
			get
			{
                return list;
			}
		}
		
		public virtual void Visit(Coordinate coord)
		{
			if (!m_objSet.Contains(coord))
			{
				m_objSet.Add(coord);
                list.Add(coord);
			}
		}
	}
}