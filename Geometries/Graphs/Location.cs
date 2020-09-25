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
using System.Text;

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Graphs
{
	/// <summary> 
	/// A location is the labelling of a GraphComponent's topological 
	/// relationship to a single Geometry.
	/// </summary>
	/// <remarks>
	/// If the parent component is an area edge, each side and the edge itself
	/// have a topological location.  These locations are named
	/// <list type="number">
	/// <item>
	/// <description>
	/// ON: on the edge
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// LEFT: left-hand side of the edge
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// RIGHT: right-hand side
	/// </description>
	/// </item>
	/// </list>
	/// If the parent component is a line edge or node, there is a single 
	/// topological relationship attribute, ON.
	/// <para>
	/// The possible values of a topological location are
	/// {LocationType.None, LocationType.Exterior, LocationType.Boundary, LocationType.Interior}
	/// </para>
	/// The labelling is stored in an array location[j] where
	/// where j has the values ON, LEFT, RIGHT
	/// </remarks>
	[Serializable]
    internal sealed class Location
	{
        #region Private Fields

		internal int[] location;
        
        #endregion
		
        #region Constructors and Destructor

		public Location(int[] location)
		{
			Initialize(location.Length);
		}
		
        /// <summary> 
        /// Constructs a Location specifying how points on, to the left of, and to the
		/// right of some GraphComponent relate to some Geometry. Possible values for the
		/// parameters are LocationType.None, LocationType.Exterior, LocationType.Boundary, 
		/// and LocationType.Interior.
		/// </summary>
		/// <seealso cref="Location">
		/// </seealso>
		public Location(int on, int left, int right)
		{
			Initialize(3);
			location[Position.On] = on;
			location[Position.Left] = left;
			location[Position.Right] = right;
		}
		
		public Location(int on)
		{
			Initialize(1);
			location[Position.On] = on;
		}
		
        public Location(Location gl)
		{
			Initialize(gl.location.Length);
			if (gl != null)
			{
                int nLength = location.Length;
                for (int i = 0; i < nLength; i++)
				{
					location[i] = gl.location[i];
				}
			}
		}

        #endregion

        #region Public Properties

		/// <returns> 
		/// true if all locations are null
		/// </returns>
		public bool IsNull
		{
			get
			{
                int nLength = location.Length;
                for (int i = 0; i < nLength; i++)
				{
					if (location[i] != LocationType.None)
						return false;
				}

				return true;
			}
			
		}
		
        /// <returns> true if any locations are NULL
		/// </returns>
		public bool IsAnyNull
		{
			get
			{
                int nLength = location.Length;
                for (int i = 0; i < nLength; i++)
				{
					if (location[i] == LocationType.None)
						return true;
				}
				return false;
			}
		}
		
        public bool IsArea
		{
			get
			{
				return (location.Length > 1);
			}
		}
		
        public bool IsLine
		{
			get
			{
				return (location.Length == 1);
			}
		}
        
        #endregion
		
        #region Public Methods

		public bool IsEqualOnSide(Location le, int locIndex)
		{
			return location[locIndex] == le.location[locIndex];
		}
		
		public void Flip()
		{
			if (location.Length <= 1)
				return ;
			int temp = location[Position.Left];
			location[Position.Left] = location[Position.Right];
			location[Position.Right] = temp;
		}
		
        public int GetLocation(int posIndex)
        {
            if (posIndex < location.Length)
                return location[posIndex];
            return LocationType.None;
        }
		
		public void SetLocation(int locIndex, int locValue)
		{
			location[locIndex] = locValue;
		}
		
        public void SetLocation(int locValue)
		{
			SetLocation(Position.On, locValue);
		}

		public int[] GetLocations()
		{
			return location;
		}
		
        public void  SetLocations(int on, int left, int right)
		{
			location[Position.On] = on;
			location[Position.Left] = left;
			location[Position.Right] = right;
		}
		
        public void SetAllLocations(int value)
        {
            int nLength = location.Length;
            for (int i = 0; i < nLength; i++)
            {
                location[i] = value;
            }
        }
			
        public void SetAllLocationsIfNull(int value)
        {
            int nLength = location.Length;
            for (int i = 0; i < nLength; i++)
            {
                if (location[i] == LocationType.None)
                    location[i] = value;
            }
        }
		
        public bool IsAllPositionsEqual(int loc)
		{
            int nLength = location.Length;
            for (int i = 0; i < nLength; i++)
			{
				if (location[i] != loc)
					return false;
			}
			return true;
		}
		
		/// <summary> 
		/// Merge updates only the NULL attributes of this object
		/// with the attributes of another.
		/// </summary>
		public void Merge(Location gl)
		{
			// if the src is an Area label & and the dest is not, 
            // increase the dest to be an Area
			if (gl.location.Length > location.Length)
			{
				int[] newLoc = new int[3];
				newLoc[Position.On] = location[Position.On];
				newLoc[Position.Left] = LocationType.None;
				newLoc[Position.Right] = LocationType.None;
				location = newLoc;
			}

            int nLength = location.Length;
            for (int i = 0; i < nLength; i++)
			{
				if (location[i] == LocationType.None && i < gl.location.Length)
					location[i] = gl.location[i];
			}
		}
		
		public override string ToString()
		{
			StringBuilder buf = new StringBuilder();
			if (location.Length > 1)
				buf.Append(LocationType.ToLocationSymbol(location[Position.Left]));
			buf.Append(LocationType.ToLocationSymbol(location[Position.On]));
			if (location.Length > 1)
				buf.Append(LocationType.ToLocationSymbol(location[Position.Right]));

			return buf.ToString();
		}
        
        #endregion
		
        #region Private Methods

        private void Initialize(int size)
        {
            location = new int[size];
            SetAllLocations(LocationType.None);
        }
        
        #endregion
    }
}