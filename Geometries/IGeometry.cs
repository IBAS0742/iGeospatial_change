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

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries
{
	/// <summary>  
	/// A geometry is a set of points or coordinates in a particular 
	/// coordinate reference system. 
	/// </summary>
	/// <remarks>
	/// The spatial relationship predicates (like disjoint, touch etc) of
	/// the geometries are based on the Dimensionally Extended 
	/// Nine-Intersection Model (DE-9IM).
	/// <para>
	/// For a description of the DE-9IM, see the 
	/// <see href="http://www.opengis.org/techno/specs.htm">OpenGIS Simple Features
	/// Specification for SQL</see>.
	/// </para>
	/// This interface also defines a Precision Model (<see cref="PrecisionModel"/>)
	/// object is a member of every Geometry object. This enables computational
	/// geometry algorithms to control the numerical precision of the coordinates.
	/// <para>
	/// The Simple Feature Specification specifies that objects of each 
	/// Geometry subclass may be empty. It is sometimes necessary to 
	/// construct a generic empty object of class Geometry (e.g. if the 
	/// exact type of the IGeometry to be returned is not known). The
	/// Simple Feature Specification does not define a specific class 
	/// or object to represent a generic empty Geometry. 
	/// </para>
	/// <para>
	/// This implementation, however, defines an empty geometry, which can be
	/// accessed in the <see cref="Geometry"/> class, which is an 
	/// <see langword="abstract"/> implementation of this interface, as
	/// <see cref="Geometry.Empty"/>.
	/// </para>
	/// 
	/// The binary predicates of geometries can be completely specified in 
	/// terms of an <see cref="IntersectionMatrix"/> pattern.
	/// In fact, their implementation is simply a call to 
	/// <see cref="IGeometry.Relate"/> with the appropriate pattern. The
	/// <see cref="IntersectionMatrix"/> is class, which defines interfaces
    /// for manipulating and accessing the Dimensionally Extended 
    /// Nine-Intersection Model (DE-9IM). 
	/// <para>
	/// It is important to note that binary predicates are topological operations
	/// rather than pointwise operations. Even for apparently straightforward
	/// predicates such as <see cref="IGeometry.Equals"/>, it is easy to 
	/// find cases where a pointwise comparison does not produce the same result 
	/// as a topological comparison. 
	/// </para>
	/// For instance, assume <c>multiPointA</c> and <c>multiPointB</c> are 
	/// <see cref="MultiPoint"/>s with the same point repeated different 
	/// numbers of times; <c>multiPointA</c> is a <see cref="LineString"/> 
	/// with two collinear line segments and <c>multiPointB</c> is a
	/// single line segment with the same start and endpoints. The algorithm 
	/// used for the <see cref="IGeometry.Relate"/> method is a topology-based 
	/// algorithm which produces a topologically correct result. 
	/// <para>
	/// As in the Simple Feature Specification, the term 
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <term>P</term>
	/// <description>
	/// is used to refer to 0-dimensional geometries (<see cref="Point"/> and 
	/// <see cref="MultiPoint"/>),
	/// </description>
	/// </item>
	/// <item>
	/// <term>L</term>
	/// <description>
	/// is used to refer to 1-dimensional geometries (<see cref="LineString"/> 
	/// and <see cref="MultiLineString"/>), and
	/// </description>
	/// </item>
	/// <item>
	/// <term>A</term>
	/// <description>
	/// is used to refer to 2-dimensional geometries (<see cref="Polygon"/>
	/// and <see cref="MultiPolygon"/>).
	/// </description>
	/// </item>
	/// </list>
    /// The dimension of a collection, <see cref="IGeometryCollection"/>,
	/// is equal to the maximum dimension of its components. 
	/// <para>
	/// In the Simple Feature Specification some binary predicates are stated 
	/// to be undefined for some combinations of dimensions (for example, 
	/// <see cref="IGeometry.Touches"/> is undefined for <c>P</c> / <c>P</c>).
	/// In the interests of simplifying the API, combinations of argument 
	/// geometries that are not in the domain of a predicate will return 
	/// false (e.g. <c>IPoint.Touches(IPoint) => false</c>). 
	/// </para> 
	/// If the argument to a predicate is an empty Geometry then the
	/// predicate will return false. 
	/// <para>
	/// For certain inputs, the <see cref="IGeometry.Difference"/> and 
	/// <see cref="IGeometry.SymmetricDifference"/> methods may compute 
	/// non-closed sets. This can happen when the arguments overlap and 
	/// have different dimensions. Since Geometry objects in this 
	/// implementation can represent only closed sets, the spatial 
	/// analysis methods are specified to return the closure of the 
	/// point-set-theoretic result.
	/// </para>
	/// </remarks>
	public interface IGeometry : ICloneable, IComparable
	{
		/// <summary>  
		/// Gets the name of this <see cref="IGeometry"/> object.
		/// </summary>
		/// <value>
		/// A string containing the specific name of this 
		/// <see cref="IGeometry"/> object instance.
		/// </value>
		string Name
		{
			get;
		}

		/// <summary>  
		/// Gets the type of this <see cref="IGeometry"/> instance.
		/// </summary>
		/// <value>
		/// A <see cref="GeometryType"/> enumeration specifying the type
		/// of this <see cref="IGeometry"/>.
		/// </value>
		GeometryType GeometryType
		{
			get;
		}

		/// <summary>  
		/// Gets the precision model used by the <see cref="IGeometry"/>.
		/// </summary>
		/// <value>
		/// A <see cref="PrecisionModel"/>, which defines the numeric
		/// precision model of this geometry.
		/// </value>
		PrecisionModel PrecisionModel
		{
			get;
		}

        /// <summary>
        /// Gets the special or user-defined properties of this 
        /// <see cref="IGeometry"/>.
        /// </summary>
        /// <value>
        /// A <see cref="GeometryProperties"/>, which is a custom
        /// dictionary or hashtable of key/value.
        /// </value>
        /// <remarks>
        /// This can be used to store such information as the
        /// coordinate reference system objects of this geometry
        /// instance, and other extension properties without having
        /// to modify the sources of the geometry implementations.
        /// <para>
        /// This is meant to be use in a way similar to properties
        /// implementations in the Java language. 
        /// </para>
        /// By default, a geometry does not contain any instance of this
        /// object until it is created by the user.
        /// </remarks>
        IGeometryProperties Properties
        {
            get;
        }

		/// <summary>  
		/// Gets the minimum bounding box, which encloses this 
		/// <see cref="IGeometry"/>.
		/// </summary>
		/// <value>
		/// An <see cref="Envelope"/> of the bounding box, or if the 
		/// <see cref="IGeometry"/> is empty, a <see langword="null"/>
		/// Envelope.
		/// </value>
        /// <seealso cref="IGeometry.Envelope"/>
        Envelope Bounds
		{
			get;
		}

		/// <summary>  
		/// Gets this <see cref="IGeometry"/>s minimum bounding box, 
		/// returned as a geometry. 
		/// </summary>
		/// <value>
		/// An empty <see cref="IGeometry"/>, which might be implementation
		/// specific (<see cref="Geometry.Empty"/>), if this 
		/// <see cref="IGeometry"/> is empty, or a <see cref="Polygon"/>
		/// defining the minimum bounding box.
		/// </value>
		/// <remarks>
        /// The returned polygon for a non-empty geometry is defined by 
        /// the corner points of the bounding box ((minX, minY), 
        /// (maxX, minY), (maxX, maxY), (minX, maxY), (minX, minY)).
		/// </remarks>
        /// <seealso cref="IGeometry.Bounds"/>
		IGeometry Envelope
		{
			get;
		}

		/// <summary>  
		/// Gets a value that indicates whether the set of points in this 
		/// <see cref="IGeometry"/> is empty.
		/// </summary>
		/// <value>    
		/// true if this <see cref="IGeometry"/> represents the empty
		/// point set for the coordinate space.
		/// </value>
		bool IsEmpty
		{
			get;
		}

		/// <summary>  
        /// Gets a value that indicates whether the geometry has no anomalous 
        /// geometric points, such as self intersection or self tangency.
		/// </summary>
		/// <value>    
		/// true if this <see cref="IGeometry"/> has no self-tangency, 
		/// self-intersection or other anomalous points.
		/// </value>
        /// <remarks>
		/// <para>
		/// Subinterfaces can refine this definition of "simple" in their 
		/// comments.  
		/// </para>
		/// In general, the SFS specifications of simplicity seem to follow the
		/// following rule:
		/// <list type="number">
		/// <item>
		/// <description>
		/// A <see cref="IGeometry"/> is simple if the only self-intersections 
		/// are at boundary points.
		/// </description>
		/// </item>
        /// <item>
        /// <description>
        /// There is no self-tangency.
        /// </description>
        /// </item>
        /// </list>
		/// For all empty <see cref="IGeometry"/>s, this property always 
		/// returns true.
        /// </remarks>
		bool IsSimple
		{
			get;
		}

		/// <summary>  
		/// Gets the the closure of the combinatorial boundary of the 
		/// geometry.
		/// </summary>
		/// <value>
		/// A <see cref="IGeometry"/> defining the closure of the 
		/// combinatorial boundary of this <see cref="IGeometry"/>,
		/// or <see langword="null"/> if the geometry is empty.
		/// </value>
		IGeometry Boundary
		{
			get;
		}

		/// <summary>  
		/// Gets the inherent dimension of this <see cref="IGeometry"/>.
		/// </summary>
		/// <value>
		/// The dimension of this <see cref="IGeometry"/> instance, whether
		/// or not this geometry is the empty.
		/// </value>
		DimensionType Dimension
		{
			get;
		}
		
		/// <summary>  
        /// Determines whether this geometry is spatially equal to
        /// another geometry (the specified argument), in which the DE-9IM 
        /// intersection matrix for the two <see cref="IGeometry"/>s 
        /// is T*F**FFF*.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// true, if the two <see cref="IGeometry"/>s are spatially equal.
		/// </returns>
		bool Equals(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry is spatially disjoint to
        /// another geometry (the specified argument), in which the DE-9IM 
        /// intersection matrix for the two <see cref="IGeometry"/>s 
        /// is FF*FF****.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// true if the two <see cref="IGeometry"/>s are spatially disjoint.
		/// </returns>
		bool Disjoint(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially intersects
        /// another geometry (the specified argument), in which the 
        /// two are not spatially disjoint.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// true if the two <see cref="IGeometry"/>s intersect.
		/// </returns>
		bool Intersects(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially touches
        /// another geometry (the specified argument), in which the DE-9IM 
        /// intersection matrix for the two <see cref="IGeometry"/>s 
        /// is FT*******, F**T***** or F***T****.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns> 
		/// true if the two <see cref="IGeometry"/>s touch, false if both 
		/// <see cref="IGeometry"/>s are points.
		/// </returns>
		bool Touches(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially crosses
        /// another geometry (the specified argument), in which the DE-9IM 
        /// intersection matrix for the two <see cref="IGeometry"/>s is
        /// <list type="number">
        /// <item>
        /// <term>T*T******</term>
        /// <description>
        /// for a point and a curve, a point and an area or a line
		/// and an area,
        /// </description>
        /// </item>
        /// <item>
        /// <term>0********</term>
        /// <description>
        /// for two curves.
        /// </description>
        /// </item>
        /// </list>
		/// </summary>
		/// <param name="otherGeometry"> the <see cref="IGeometry"/> with which to compare this <see cref="Geometry"/>
		/// </param>
		/// <returns> 
		/// true if the two <see cref="IGeometry"/>s cross.
		/// </returns>
		/// <remarks>
		/// For this function to return true, the <see cref="IGeometry"/>s
		/// must be a point and a curve; a point and a surface; two curves; 
		/// or a curve and a surface.
		/// </remarks>
        bool Crosses(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially within another 
        /// geometry (the specified argument), in which the DE-9IM 
        /// intersection matrix for the two <see cref="IGeometry"/>s 
        /// is T*F**F***.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// true if this <see cref="IGeometry"/> is within the other
		/// geometry.
		/// </returns>
		bool Within(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially contains another 
        /// geometry (the specified argument), in which the 
        /// <see cref="IGeometry.Within"/> returns true.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// true if this <see cref="IGeometry"/> contains other geometry.
		/// </returns>
		bool Contains(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially overlaps
        /// another geometry (the specified argument), in which the DE-9IM 
        /// intersection matrix for the two <see cref="IGeometry"/>s is
        /// <list type="number">
        /// <item>
        /// <term>T*T***T**</term>
        /// <description>
        /// for two points or two surfaces,
        /// </description>
        /// </item>
        /// <item>
        /// <term>1*T***T**</term>
        /// <description>
        /// for two curves.
        /// </description>
        /// </item>
        /// </list>
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// true if the two <see cref="IGeometry"/>s overlap.
		/// </returns>
		/// <remarks>
		/// For this method to return true, the <see cref="IGeometry"/>s
		/// must be two points, two curves or two surfaces.
		/// </remarks>
		bool Overlaps(IGeometry otherGeometry);
		
		/// <summary>  
        /// Determines whether this geometry spatially relates to
        /// another geometry (the specified argument), by the specified
        /// DE-9IM intersection matrix pattern.
		/// </summary>
		/// <param name="otherGeometry">               the <see cref="Geometry"/> with which to compare
		/// this <see cref="Geometry"/>
		/// </param>
		/// <param name="intersectionPattern"> the pattern against which to check the
		/// intersection matrix for the two <see cref="Geometry"/>s
		/// </param>
		/// <returns>                      true if the DE-9IM intersection
		/// matrix for the two <see cref="Geometry"/>s match <code>intersectionPattern</code>
		/// </returns>
        /// <remarks>
        /// This returns true if the elements in the DE-9IM intersection
		/// matrix for the two <see cref="IGeometry"/>s match the elements in 
		/// the specified pattern, which may be:
		/// <list type="bullet">
		/// <item>
		/// <description>0</description>
		/// </item>
		/// <item>
		/// <description>1</description>
		/// </item>
		/// <item>
		/// <description>2</description>
		/// </item>
		/// <item>
		/// <description>T ( = 0, 1 or 2)</description>
		/// </item>
		/// <item>
		/// <description>F ( = -1)</description>
		/// </item>
		/// <item>
		/// <description>* ( = -1, 0, 1 or 2)</description>
		/// </item>
		/// </list>
		/// For more information on the DE-9IM, see the OpenGIS Simple Features
		/// Specification.
        /// </remarks>
		bool Relate(IGeometry otherGeometry, string intersectionPattern);
		
		/// <summary>  
		/// Determines the DE-9IM intersection matrix for the two 
		/// <see cref="IGeometry"/>s.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compare this 
		/// <see cref="IGeometry"/>.
		/// </param>
		/// <returns> 
		/// A matrix, <see cref="IntersectionMatrix"/> describing the 
		/// intersections of the interiors, boundaries and exteriors 
		/// of the two <see cref="IGeometry"/>s
		/// </returns>
		IntersectionMatrix Relate(IGeometry otherGeometry);
		
		/// <summary>  
		/// Determines the buffer region around this <see cref="IGeometry"/> 
		/// having the given width.
		/// </summary>
		/// <param name="distance"> 
		/// The width of the buffer, interpreted according to the
		/// <see cref="PrecisionModel"/> of the <see cref="IGeometry"/>.
		/// </param>
		/// <returns>
		/// All points whose distance from this <see cref="IGeometry"/>
		/// are less than or equal to distance.
		/// </returns>
		IGeometry Buffer(double distance);
		
		/// <summary>  
		/// Determines the smallest convex polygon that contains all the
		/// points in this <see cref="IGeometry"/>. 
		/// </summary>
		/// <returns>    
		/// The minimum-area convex polygon containing this 
		/// <see cref="IGeometry"/>'s points.
		/// </returns>
		/// <remarks>
        /// This obviously applies only to geometries which contain 3 or more points; 
        /// the results for degenerate cases are specified as follows:
        /// <list type="table">
        /// <listheader>
        /// <term>
        /// Number of Points in argument Geometry
        /// </term>
        /// <description>
        /// Geometry class of result
        /// </description>
        /// </listheader>
        /// <item>
        /// <term>0</term>
        /// <description>Empty <see cref="GeometryCollection"/></description>
        /// </item>
        /// <item>
        /// <term>1</term>
        /// <description><see cref="Point"/></description>
        /// </item>
        /// <item>
        /// <term>2</term>
        /// <description><see cref="LineString"/></description>
        /// </item>
        /// <item>
        /// <term>3 or more</term>
        /// <description><see cref="Polygon"/></description>
        /// </item>
        /// </list>
        /// </remarks>
		IGeometry ConvexHull();
		
		/// <summary>  
		/// Determines a <see cref="IGeometry"/> representing the points 
		/// shared by this and specified <see cref="IGeometry"/>s.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compute the intersection.
		/// </param>
		/// <returns>
		/// The points common to the two <see cref="IGeometry"/>s.
		/// </returns>
		IGeometry Intersection(IGeometry otherGeometry);
		
		/// <summary>  
		/// Determines a <see cref="IGeometry"/> representing all the 
		/// points in this and the other specified <see cref="IGeometry"/>s.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compute the union.
		/// </param>
		/// <returns>
		/// A set combining the points of this <see cref="IGeometry"/> and
		/// the points of other specified.
		/// </returns>
		IGeometry Union(IGeometry otherGeometry);
		
		/// <summary>  
		/// Determines a <see cref="IGeometry"/> representing the points 
		/// making up this <see cref="IGeometry"/> that do not make up 
		/// other specified <see cref="IGeometry"/>. This method returns 
		/// the closure of the resultant. 
		/// <see cref="Geometry"/>.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compute the difference.
		/// </param>
		/// <returns>        
		/// The point set difference of this <see cref="IGeometry"/> with
		/// other specified <see cref="IGeometry"/>.
		/// </returns>
		IGeometry Difference(IGeometry otherGeometry);
		
		/// <summary>  
		/// Determines a set combining the points in this <see cref="IGeometry"/> 
		/// not in the other specified <see cref="IGeometry"/>, and the points 
		/// in other <see cref="IGeometry"/> not in this <see cref="IGeometry"/>. 
		/// This method returns the closure of the resultant 
		/// <see cref="IGeometry"/>.
		/// </summary>
		/// <param name="otherGeometry"> 
		/// The <see cref="IGeometry"/> with which to compute the symmetric
		/// difference.
		/// </param>
		/// <returns>
		/// The point set symmetric difference of this <see cref="IGeometry"/>
		/// with the other specified <see cref="IGeometry"/>.
		/// </returns>
		IGeometry SymmetricDifference(IGeometry otherGeometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        bool IsWithinDistance(IGeometry geom, double distance);

        new IGeometry Clone();
    }
}