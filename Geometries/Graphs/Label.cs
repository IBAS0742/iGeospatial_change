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
	/// A Label indicates the topological relationship of a component
	/// of a topology graph to a given Geometry.
	/// </summary>
	/// <remarks>
	/// This class supports labels for relationships to two <see cref="Geometry"/> instances,
	/// which is sufficient for algorithms for binary operations.
	/// <para>
	/// Topology graphs support the concept of labeling nodes and edges in the graph.
	/// The label of a node or edge specifies its topological relationship to one or
	/// more geometries.  (In fact, since OTS operations have only two arguments labels
	/// are required for only two geometries).  A label for a node or edge has one or
	/// two elements, depending on whether the node or edge occurs in one or both of the
	/// input <see cref="Geometry"/> instances.  Elements contain attributes which categorize the
	/// topological location of the node or edge relative to the parent
	/// Geometry; that is, whether the node or edge is in the interior,
	/// boundary or exterior of the Geometry.  Attributes have a value
	/// from the set {Interior, Boundary, Exterior}.  In a node each
	/// element has  a single attribute &lt;On&gt;.  For an edge each element has a
	/// triplet of attributes &lt;Left, On, Right&gt;.
	/// </para>
	/// It is up to the client code to associate the 0 and 1 locations
	/// with specific geometries.
	/// </remarks>
	[Serializable]
    internal class Label
	{
        #region Private Fields

        internal Location[] elt;
        
        #endregion
		
        #region Constructors and Destructor

		/// <summary> Construct a Label with a single location for both Geometries.
		/// Initialize the locations to Null
		/// </summary>
		public Label(int onLoc)
		{
            elt    = new Location[2];

            elt[0] = new Location(onLoc);
			elt[1] = new Location(onLoc);
		}

		/// <summary> Construct a Label with a single location for both Geometries.
		/// Initialize the location for the Geometry index.
		/// </summary>
		public Label(int geomIndex, int onLoc)
		{
            elt    = new Location[2];

            elt[0] = new Location(LocationType.None);
			elt[1] = new Location(LocationType.None);

			elt[geomIndex].SetLocation(onLoc);
		}

		/// <summary> Construct a Label with On, Left and Right locations for both Geometries.
		/// Initialize the locations for both Geometries to the given values.
		/// </summary>
		public Label(int onLoc, int leftLoc, int rightLoc)
		{
            elt = new Location[2];

            elt[0] = new Location(onLoc, leftLoc, rightLoc);
			elt[1] = new Location(onLoc, leftLoc, rightLoc);
		}

		/// <summary> Construct a Label with On, Left and Right locations for both Geometries.
		/// Initialize the locations for the given Geometry index.
		/// </summary>
		public Label(int geomIndex, int onLoc, int leftLoc, int rightLoc)
		{
            elt = new Location[2];

            elt[0] = new Location(LocationType.None, 
                LocationType.None, LocationType.None);
			elt[1] = new Location(LocationType.None, 
                LocationType.None, LocationType.None);

			elt[geomIndex].SetLocations(onLoc, leftLoc, rightLoc);
		}

		/// <summary> Construct a Label with the same values as the argument Label.</summary>
		public Label(Label lbl)
		{
            elt = new Location[2];

            elt[0] = new Location(lbl.elt[0]);
			elt[1] = new Location(lbl.elt[1]);
		}

        #endregion
		
        #region Public Properties

		public int GeometryCount
		{
			get
			{
				int count = 0;
				if (!elt[0].IsNull)
					count++;
				if (!elt[1].IsNull)
					count++;

				return count;
			}
		}
        
        #endregion
		
        #region Public Methods

		// converts a Label to a Line label (that is, one with no side Locations)
		public static Label ToLineLabel(Label label)
		{
			Label lineLabel = new Label(LocationType.None);
			for (int i = 0; i < 2; i++)
			{
				lineLabel.SetLocation(i, label.GetLocation(i));
			}

			return lineLabel;
		}
		
		public void Flip()
		{
			elt[0].Flip();
			elt[1].Flip();
		}
		
		public int GetLocation(int geomIndex, int posIndex)
		{
			return elt[geomIndex].GetLocation(posIndex);
		}

		public int GetLocation(int geomIndex)
		{
			return elt[geomIndex].GetLocation(Position.On);
		}

		public void SetLocation(int geomIndex, int posIndex, int location)
		{
			elt[geomIndex].SetLocation(posIndex, location);
		}

		public void SetLocation(int geomIndex, int location)
		{
			elt[geomIndex].SetLocation(Position.On, location);
		}

		public void SetAllLocations(int geomIndex, int location)
		{
			elt[geomIndex].SetAllLocations(location);
		}

		public void SetAllLocationsIfNull(int geomIndex, int location)
		{
			elt[geomIndex].SetAllLocationsIfNull(location);
		}

		public void SetAllLocationsIfNull(int location)
		{
			SetAllLocationsIfNull(0, location);
			SetAllLocationsIfNull(1, location);
		}

		/// <summary> Merge this label with another one.
		/// Merging updates any null attributes of this label with the attributes from lbl
		/// </summary>
		public void Merge(Label lbl)
		{
			for (int i = 0; i < 2; i++)
			{
				if (elt[i] == null && lbl.elt[i] != null)
				{
					elt[i] = new Location(lbl.elt[i]);
				}
				else
				{
					elt[i].Merge(lbl.elt[i]);
				}
			}
		}

		public bool IsNull(int geomIndex)
		{
			return elt[geomIndex].IsNull;
		}

		public bool IsAnyNull(int geomIndex)
		{
			return elt[geomIndex].IsAnyNull;
		}
		
		public bool IsArea()
		{
			return elt[0].IsArea || elt[1].IsArea;
		}

		public bool IsArea(int geomIndex)
		{
			return elt[geomIndex].IsArea;
		}

		public bool IsLine(int geomIndex)
		{
			return elt[geomIndex].IsLine;
		}
		
		public bool IsEqualOnSide(Label lbl, int side)
		{
			return this.elt[0].IsEqualOnSide(lbl.elt[0], side) && this.elt[1].IsEqualOnSide(lbl.elt[1], side);
		}

		public bool IsAllPositionsEqual(int geomIndex, int loc)
		{
			return elt[geomIndex].IsAllPositionsEqual(loc);
		}

		/// <summary> Converts one DistanceLocation to a Line location</summary>
		public void ToLine(int geomIndex)
		{
			if (elt[geomIndex].IsArea)
				elt[geomIndex] = new Location(elt[geomIndex].location[0]);
		}

		public override string ToString()
		{
			StringBuilder buf = new StringBuilder();
			if (elt[0] != null)
			{
				buf.Append("a:");

                buf.Append(elt[0].ToString());
			}

			if (elt[1] != null)
			{
				buf.Append(" b:");

                buf.Append(elt[1].ToString());
			}
			
            return buf.ToString();
		}
        
        #endregion
    }
}