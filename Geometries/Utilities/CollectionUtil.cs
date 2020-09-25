using System;
using System.Collections;

namespace iGeospatial.Geometries
{
	/// <summary> 
	/// Executes a transformation function on each element of a collection
	/// and returns the results in a new List.
	/// </summary>
	internal class CollectionUtil
	{
		public interface Function
		{
			object Execute(object obj);
		}
		
		/// <summary> 
		/// Executes a function on each item in a {@link Collection}
		/// and returns the results in a new {@link List}
		/// </summary>
		/// <param name="coll">
		/// </param>
		/// <param name="func">the Function to execute
		/// </param>
		public static IList Transform(IList coll, 
            CollectionUtil.Function func)
		{
			IList result = new ArrayList();

			for (IEnumerator i = coll.GetEnumerator(); i.MoveNext(); )
			{
				result.Add(func.Execute(i.Current));
			}

			return result;
		}
		
		/// <summary> 
		/// Executes a function on each item in a Collection but does
		/// not accumulate the result
		/// </summary>
		/// <param name="coll">
		/// </param>
		/// <param name="func">the Function to execute
		/// </param>
		public static void Apply(ICollection coll, 
            CollectionUtil.Function func)
		{
			for (IEnumerator i = coll.GetEnumerator(); i.MoveNext(); )
			{
				func.Execute(i.Current);
			}
		}
		
		/// <summary> 
		/// Executes a function on each item in a Collection
		/// and collects all the entries for which the result
		/// of the function is equal to {@link Boolean}.TRUE.
		/// </summary>
		/// <param name="coll">
		/// </param>
		/// <param name="func">the Function to execute
		/// </param>
		public static IList Select(ICollection collection, 
            CollectionUtil.Function func)
		{
			IList result = new ArrayList();

			for (IEnumerator i = collection.GetEnumerator(); i.MoveNext(); )
			{
				System.Object item = i.Current;

				if (true.Equals(func.Execute(item)))
				{
					result.Add(item);
				}
			}

			return result;
		}
	}
}