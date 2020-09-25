#region License
// <copyright>
//         iGeospatial Geometries Package
//       
// This is part of the Open Geospatial Library for .NET.
// 
// Package Description:
// This is a collection of C# classes that implement the fundamental 
// operations required to validate a given geo-spatial data set to 
// a known topological specification.
// It aims to provide a complete implementation of the Open Geospatial
// Consortium (www.opengeospatial.org) specifications for Simple 
// Feature Geometry.
// 
// Contact Information:
//     Paul Selormey (paulselormey@gmail.com or paul@toolscenter.org)
//     
// Credits:
// This library is based on the JTS Topology Suite, a Java library by
// 
//     Vivid Solutions Inc. (www.vividsolutions.com)  
//     
// License:
// See the license.txt file in the package directory.   
// </copyright>
#endregion

using System;
using System.Collections;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries
{
	/// <summary>  
	/// Iterates over all <see cref="Geometry"/>s in a 
	/// <see cref="GeometryCollection"/>. 
	/// </summary>
	/// <remarks>
	/// The GeometryIterator implements a pre-order depth-first traversal 
	/// of the GeometryCollection (which may be nested). The original GeometryCollection is
	/// returned as well (as the first object), as are all sub-collections. It is
	/// simple to ignore the GeometryCollection objects if they are not
	/// needed.
	/// </remarks>
	[Serializable]
    public class GeometryIterator : IGeometryEnumerator
	{
        #region Private Members

		/// <summary>  The GeometryCollection being iterated over.</summary>
		private Geometry parent;

		/// <summary>  
		/// Indicates whether or not the first element (the GeometryCollection) 
		/// has been returned.
		/// </summary>
		private bool atStart;

		/// <summary>  
		/// The number of <see cref="Geometry"/> instances in the the GeometryCollection.
		/// </summary>
		private int max;

		/// <summary>  
		/// The index of the Geometry that will be returned when next is called.
		/// </summary>
		private int index;

		/// <summary>  
		/// The iterator over a nested GeometryCollection, or null if this 
		/// GeometryIterator is not currently iterating over a nested GeometryCollection.
		/// </summary>
		private GeometryIterator subcollectionIterator;
		
        #endregion

        #region Constructors and Destructor
        
		/// <summary>  
		/// Constructs an iterator over the given <see cref="Geometry"/>.
		/// </summary>
		/// <param name="parent"> 
		/// The collection over which to iterate; also, the first element 
		/// returned by the iterator.
		/// </param>
		internal GeometryIterator(Geometry parent)
		{
			this.parent = parent;
			atStart     = true;
			max         = parent.NumGeometries;
		}
		
        #endregion

        #region Public Properties

		public Geometry Current
		{
			get
			{
				// the parent GeometryCollection is the first object returned
				if (atStart)
				{
					atStart = false;
					return parent;
				}

				if (subcollectionIterator != null)
				{
					if (subcollectionIterator.MoveNext())
					{
						return subcollectionIterator.Current;
					}
					else
					{
						subcollectionIterator = null;
					}
				}

				if (index >= max)                             
				{
                    return null;
				}

				Geometry obj = parent.GetGeometry(index++);
				if (obj.IsCollection)
				{
					subcollectionIterator = new GeometryIterator(obj);
					// there will always be at least one element in the sub-collection
					return subcollectionIterator.Current;
				}

				return obj;
			}
			
		}
		
        #endregion

        #region Public Methods

		public virtual bool MoveNext()
		{
			if (atStart)
			{
				return true;
			}
			
            if (subcollectionIterator != null)
			{
				if (subcollectionIterator.MoveNext())
				{
					return true;
				}
				subcollectionIterator = null;
			}
			
            if (index >= max)
			{
				return false;
			}

			return true;
		}

		public virtual void Reset()
		{
			index   = 0;
			atStart = true;
		}
		
		/// <summary>Not implemented.
		/// </summary>
		/// <exception cref="NotSupportedException">
		/// This method is not implemented.
		/// </exception>
		public void Remove()
		{
			throw new System.NotSupportedException();
		}

        #endregion
	}
}