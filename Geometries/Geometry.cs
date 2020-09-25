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
using System.Diagnostics;

using iGeospatial.Exceptions;
using iGeospatial.Coordinates;
using iGeospatial.Geometries.IO;
using iGeospatial.Geometries.Exports;
using iGeospatial.Geometries.Visitors;
using iGeospatial.Geometries.Algorithms;
using iGeospatial.Geometries.Operations;

namespace iGeospatial.Geometries
{
	/// <summary>  
	/// This is the base class for all geometric objects. 
	/// </summary>
	/// <remarks>
    /// The <see cref="Geometry"/> is an <see langword="abstract"/> base class
    /// defining OpenGIS Simple Feature Specification (SFS) interfaces.
    /// <para>
	/// <see cref="Geometry"/> and its derivatives implement a deep copy of the object,
	/// for cloning of its instances <seealso cref="System.ICloneable"/>.
    /// </para> 
	/// Binary Predicates
	/// <para>
	/// Because it is not clear at this time what semantics for spatial
	/// analysis methods involving GeometryCollections would be useful,
	/// GeometryCollections are not supported as arguments to binary
	/// predicates (other than ConvexHull) or the relate method.
	/// </para>
	/// Set-Theoretic Methods
	/// <para>
	/// The spatial analysis methods will
	/// return the most specific class possible to represent the result. If the
	/// result is homogeneous, a Point, LineString, or
	/// Polygon will be returned if the result Contains a single
	/// element; otherwise, a MultiPoint, MultiLineString,
	/// or MultiPolygon will be returned. If the result is
	/// heterogeneous a GeometryCollection will be returned.
	/// </para>
	/// <para>
	/// Because it is not clear at this time what semantics for set-theoretic
	/// methods involving GeometryCollections would be useful,
	/// GeometryCollections
	/// are not supported as arguments to the set-theoretic methods.
	/// </para>
	/// Representation of Computed Geometries
	/// <para>
	/// The SFS states that the result of a set-theoretic method is the 
	/// "point-set" result of the usual set-theoretic definition of the 
	/// operation (SFS 3.2.21.1). However, there are
	/// sometimes many ways of representing a point set as a Geometry.
	/// </para>
	/// <para>
	/// The SFS does not specify an unambiguous representation of a given point set
	/// returned from a spatial analysis method. One goal of OTS is to make this
	/// specification precise and unambiguous. OTS will use a canonical form for
	/// <see cref="Geometry"/> instances returned from spatial analysis methods. The canonical
	/// form is a Geometry which is simple and noded:
	/// </para>
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// Simple means that the Geometry returned will be simple according to
	/// the OTS definition of <c>IsSimple</c>.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Noded applies only to overlays involving <see cref="LineString"/> objects. It
	/// means that all intersection points on <see cref="LineString"/> objects will be
	/// present as endpoints of <see cref="LineString"/> objects in the result.
	/// </description>
	/// </item>
	/// </list>
	/// <para>
	/// This definition implies that non-simple geometries which are arguments to
	/// spatial analysis methods must be subjected to a line-dissolve process to
	/// ensure that the results are simple.
	/// </para>
	/// Constructed Points And The Precision Model
	/// <para>
	/// The results computed by the set-theoretic methods may
	/// contain constructed points which are not present in the input Geometry
	/// s. These new points arise from intersections between line segments in the
	/// edges of the input <see cref="Geometry"/> instances. In the general case it is not
	/// possible to represent constructed points exactly. This is due to the fact
	/// that the coordinates of an intersection point may contain twice as many bits
	/// of precision as the coordinates of the input line segments. In order to
	/// represent these constructed points explicitly, OTS must truncate them to fit
	/// the PrecisionModel.
	/// </para>
	/// <para>
	/// Unfortunately, truncating coordinates moves them slightly. Line segments
	/// which would not be coincident in the exact result may become coincident in
	/// the truncated representation. This in turn leads to "topology collapses" --
	/// situations where a computed element has a lower dimension than it would in
	/// the exact result.
	/// </para>
	/// <para>
	/// When OTS detects topology collapses during the computation of spatial
	/// analysis methods, it will throw an exception. If possible the exception will
	/// report the location of the collapse.
	/// </para>
	/// 
	/// <see cref="Object.Equals"/> and <see cref="Object.GetHashCode"/> are not 
	/// overridden, so that when two topologically equal Geometries are added to 
	/// Hashtable and related collections, they remain distinct. 
	/// This behaviour is desired in many cases.
	/// </remarks>
	[Serializable]
	public abstract class Geometry : IGeometry
	{
        #region Private Fields

        private IGeometryProperties m_objProperties;
        private GeometryFactory     m_objFactory;
		
        /// <summary>  The bounding box of this Geometry.</summary>
        [NonSerialized] 
        internal Envelope           m_objEnvelope;
        
        #endregion

        #region Constructors and Destructor

        protected Geometry(GeometryFactory factory)
        {
            if (factory == null)
            {
                m_objFactory = GeometryFactory.GetInstance();
            }
            else
            {
                m_objFactory = factory;
            }
        }

        protected Geometry(GeometryFactory factory, IGeometryProperties properties)
        {
            if (factory == null)
            {
                m_objFactory = GeometryFactory.GetInstance();
            }
            else
            {
                m_objFactory = factory;
            }

            m_objProperties = properties;
        }
        
        #endregion
		
        #region Public Properties

		/// <summary>  
		/// Returns the name of this object's interface.
		/// </summary>
		/// <returns>    
		/// The name of this <see cref="Geometry"/> instances most specific iGeospatial.Geometries
		/// interface
		/// </returns>
		public abstract GeometryType GeometryType
        {
            get;
        }
		
		/// <summary>  
		/// Returns the name of this object's interface.
		/// </summary>
		/// <returns>    
		/// The name of this <see cref="Geometry"/> instances most specific iGeospatial.Geometries
		/// interface
		/// </returns>
		public abstract string Name
        {
            get;
        }

		/// <summary> Gets the factory which Contains the context in which this geometry was created.
		/// 
		/// </summary>
		/// <returns> the factory for this geometry
		/// </returns>
		public virtual GeometryFactory Factory
		{
			get
			{
				return m_objFactory;
			}
		}

		/// <summary>  
		/// Gets the PrecisionModel used by the Geometry.
		/// </summary>
		/// <value> 
		/// The specification of the grid of allowable points, for this
		/// Geometry and all other <see cref="Geometry"/> instances
		/// </value>
		public virtual PrecisionModel PrecisionModel
		{
			get
			{
				return m_objFactory.PrecisionModel;
			}
		}

        /// <summary>  
        /// Gets a vertex of this <see cref="Geometry"/>.
        /// </summary>
        /// <value> 
        /// A Coordinate which is a vertex of this Geometry. It will returns null 
        /// (Nothing in Visual Basic) if this Geometry is empty.
        /// </value>
        public abstract Coordinate Coordinate
        {
            get;
        }

		/// <summary>  
		/// Gets this <see cref="Geometry"/>'s vertices. If you modify the coordinates
		/// in this array, be sure to call <see cref="Geometry.GeometryChanged"/> 
		/// afterwards.
		/// The <see cref="Geometry"/> instances contained by composite 
		/// <see cref="Geometry"/> instances
		/// must be Geometry's; that is, they must implement getCoordinates.
		/// </summary>
		/// <value> Returns the vertices of this Geometry.</value>
		public abstract ICoordinateList Coordinates
        {
            get;
        }

		/// <summary>  
		/// Gets the count of this <see cref="Geometry"/> instances vertices. The Geometry
		/// s contained by composite <see cref="Geometry"/> instances must be
		/// Geometry's; that is, they must implement <c>NumPoints</c>
		/// </summary>
		/// <value> The number of vertices in this Geometry </value>
        public abstract int NumPoints
        {
            get;
        }

		/// <summary>  
		/// Determines whether a <see cref="Geometry"/> is simple according to the 
		/// OpenGIS Simple Feature Specifications (SFS).
		/// </summary>
		/// <value>
		/// This property returns <see langword="true"/> if this 
		/// <see cref=""Geometry/> has any points of self-tangency, 
		/// self-intersection or other anomalous points.
		/// </value>
		/// <remarks>
		/// <para>
		/// Subclasses provide their own definition of "simple". If
		/// this <see cref="Geometry"/> is empty, returns true.
		/// </para>
		/// In general, the SFS specifications of simplicity seem to follow the
		/// following rule:
		/// <para>
		/// A Geometry is simple iff the only self-intersections are at
		/// boundary points.
		/// </para>
        /// Simplicity is defined for each <see cref="Geometry"/> subclass as follows:
	    /// <list type="bullet">
	    /// <item>
	    /// <description>
        /// Valid polygonal geometries are simple by definition, so
        /// <c>IsSimple</c> trivially returns true.
	    /// </description>
	    /// </item>
        /// <item>
        /// <description>
        /// Linear geometries are simple iff they do not self-intersect at points
        /// other than boundary points.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Zero-dimensional geometries (points) are simple iff they have no
        /// repeated points.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Empty <see cref="Geometry"/>s are always simple
        /// </description>
        /// </item>
	    /// </list>
		/// </remarks>
	    /// <seealso cref="IsValid"/>
		public abstract bool IsSimple
        {
            get;
        }

		/// <summary>  
		/// Tests the validity of this <see cref="Geometry"/> instance.
		/// Subclasses provide their own definition of "valid".
		/// </summary>
		/// <value>
		/// This property returns <see langword="true"/> if this 
		/// <see cref="Geometry"/> is valid.
		/// </value>
		/// <seealso cref="IsValidOp">Overlap Operations</seealso>
		public virtual bool IsValid
		{
			get
			{
				IsValidOp isValidOp = new IsValidOp(this);

				return isValidOp.IsValid();
			}
		}

		/// <summary>  
		/// Determines whether or not the set of points in this 
		/// <see cref="Geometry"/> is empty.
		/// </summary>
		/// <value>
		/// This property returns <see langword="true"/> if this 
		/// <see cref="Geometry"/> equals the empty geometry.
		/// </value>
		public abstract bool IsEmpty
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsCollection
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsSurface
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsRectangle
        {
            get
            {
                return false;
            }
        }

		/// <summary>  
		/// Gets the area of this <see cref="Geometry"/> instance.
		/// </summary>
		/// <value>The area of the Geometry.</value>
		/// <remarks>
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// Area or <see cref="Surface"/> geometries have a non-zero area.
		/// These geometries override this function to compute the area.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// All other return 0.0.
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public virtual double Area
		{
			get
			{
				return 0.0;
			}
		}

		/// <summary>  
		/// Gets the length of this <see cref="Geometry"/>.
		/// </summary>
		/// <value>The length of the Geometry</value>
		/// <remarks>
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// Linear geometries, such as <see cref="LinearString"/>, return their length.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// Area or <see cref="Surface"/> geometries return their perimeter. These 
		/// geometries override this function to compute the perimeter.
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// All others geometries, such as <see cref="Point"/>, return 0.0.
		/// </description>
		/// </item> 
		/// </list>
		/// </remarks>
		public virtual double Length
		{
			get
			{
				return 0.0;
			}
		}

		/// <summary> 
		/// Gets the centroid of this Geometry.
		/// </summary>
		/// <value> A <see cref="Point"/> which is the centroid of this Geometry
		/// </value>
		/// <remarks>
		/// The centroid is equal to the centroid of the set of component geometries 
		/// of highest dimension (since the lower-dimension geometries contribute zero
		/// "weight" to the centroid)
		/// </remarks>
		public virtual Point Centroid
		{
			get
			{
				if (IsEmpty)
				{
					return null;
				}
				Coordinate centPt = null;
				DimensionType dim = this.Dimension;
				if (dim == DimensionType.Point)
				{
					CentroidPoint cent = new CentroidPoint();
					cent.Add(this);
					centPt = cent.Centroid;
				}
				else if (dim == DimensionType.Curve)
				{
					CentroidLine cent = new CentroidLine();
					cent.Add(this);
					centPt = cent.Centroid;
				}
				else
				{
					CentroidArea cent = new CentroidArea();
					cent.Add(this);
					centPt = cent.Centroid;
				}

                centPt.MakePrecise(this.PrecisionModel);

                return this.Factory.CreatePoint(centPt);
			}
		}

		/// <summary> 
		/// Computes an interior point of this <see cref="Geometry"/>.
		/// </summary>
		/// <value> 
		/// A <see cref="Point"/> which is in the interior of this Geometry.
		/// </value>
		/// <remarks>
		/// An interior point is guaranteed to lie in the interior of the Geometry,
		/// if it possible to calculate such a point exactly. Otherwise,
		/// the point may lie on the boundary of the geometry.
		/// </remarks>
		public virtual Point InteriorPoint
		{
			get
			{
				Coordinate interiorPt = null;
				DimensionType dim = this.Dimension;
				if (dim == DimensionType.Point)
				{
					InteriorPointPoint intPt = new InteriorPointPoint(this);
					interiorPt = intPt.InteriorPoint;
				}
				else if (dim == DimensionType.Curve)
				{
					InteriorPointLine intPt = new InteriorPointLine(this);
					interiorPt = intPt.InteriorPoint;
				}
				else
				{
					InteriorPointArea intPt = new InteriorPointArea(this);
					interiorPt = intPt.InteriorPoint;
				}

                interiorPt.MakePrecise(this.PrecisionModel);

                return this.Factory.CreatePoint(interiorPt);
			}
		}

		/// <summary>  
		/// Gets the dimension of this Geometry.
		/// </summary>
		/// <value>
		/// The dimension of the class implementing this interface, whether
		/// or not this object is the empty geometry.
		/// </value>
		public abstract DimensionType Dimension
        {
            get;
        }

		/// <summary>  
		/// Gets the boundary, or the empty geometry if this <see cref="Geometry"/>
		/// is empty. For a discussion of this function, see the OpenGIS Simple
		/// Features Specification (SFS). As stated in SFS Section 2.1.13.1, "the boundary
		/// of a Geometry is a set of Geometries of the next lower dimension."
		/// </summary>
		/// <value>
		/// The closure of the combinatorial boundary of this Geometry.
		/// </value>
		public abstract Geometry Boundary
        {
            get;
        }

		/// <summary>  
		/// Gets the dimension of this <see cref="Geometry"/> instances inherent boundary.
		/// </summary>
		/// <value> The dimension of the boundary of the class implementing this
		/// interface, whether or not this object is the empty geometry. Returns
		/// DimensionType.Empty if the boundary is the empty geometry.
		/// </value>
		public abstract DimensionType BoundaryDimension
        {
            get;
        }

		/// <summary>  
		/// Gets the bounding box of this <see cref="Geometry"/> instances. 
		/// If the Geometry is empty geometry, it returns an empty <see cref="Point"/>. 
		/// If the Geometry is a point, returns a non-empty Point. Otherwise, returns a
		/// Polygon whose points are (minx, miny), (maxx, miny), (maxx,
		/// maxy), (minx, maxy), (minx, miny).
		/// </summary>
		/// <value> An empty Point (for empty <see cref="Geometry"/> instances), a
		/// Point (for Points) or a Polygon (in all other cases)
		/// </value>
		public virtual Geometry Envelope
		{
			get
			{
				return Factory.ToGeometry(this.Bounds);
			}
		}

		/// <summary>  
		/// Gets the minimum and maximum x and y values in this <see cref="Geometry"/>, 
		/// or a null Envelope if this Geometry is empty.
		/// </summary>
		/// <value>The bounding box of this <see cref="Geometry"/> instance; 
		/// if the Geometry is empty, 
		/// <see cref="iGeospatial.Coordinates.Envelope.IsNull"/> will return true.
		/// </value>
		public virtual Envelope Bounds
		{
			get
			{
				if (m_objEnvelope == null)
				{
					m_objEnvelope = ComputeEnvelope();
				}

				return m_objEnvelope;
			}
		}

        public IGeometryProperties Properties
        {
            get 
            {
                return m_objProperties;
            }
        }

        public virtual int NumGeometries
        {
            get
            {
                return 1;
            }
        }

        public virtual int CoordinateDimension 
        { 
            get
            {
                if (this.Factory != null)
                {
                    return Factory.CoordinateDimension;
                }

                return 2;
            }
        }
        
        #endregion

        #region Private Properties

        private int SortIndex
        {
            get
            {
                return (int)this.GeometryType;
            }
        }
        
        #endregion

        #region Public Methods

        public void CreateProperties()
        {
            if (m_objProperties == null)
            {
                m_objProperties = new GeometryProperties();
            }                                              
        }

        public void CreateProperties(IComparer comparer)
        {
            if (m_objProperties == null)
            {
                m_objProperties = new GeometryProperties(comparer);
            }                                              
        }
		
		/// <summary>  
		/// Retrieves the minimum Distance between this Geometry and the Geometry g
		/// </summary>
		/// <param name="g"> the Geometry from which to compute the Distance
		/// </param>
		public virtual double Distance(Geometry g)
		{
			return DistanceOp.Distance(this, g);
		}
		
		/// <summary> 
		/// Tests whether the distance from this <see cref="Geometry"/> to another 
		/// is less than or equal to a specified value.
		/// </summary>
		/// <param name="geometry">The Geometry to check the distance to</param>
		/// <param name="distance">The distance value to compare.</param>
		/// <returns> true if the geometries are less than distance apart. </returns>
		public virtual bool IsWithinDistance(Geometry geometry, double distance)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            double envDist = Bounds.Distance(geometry.Bounds);
			if (envDist > distance)
				return false; 
            
            return DistanceOp.IsWithinDistance(this, geometry, distance);
		}
		
		/// <summary> 
		/// Notifies this <see cref="Geometry"/> that its coordinates have been 
		/// changed by an external party (using a 
		/// <see cref="iGeospatial.Coordinates.ICoordinateVisitor"/>, for example). 
		/// The Geometry will flush and/or update any information it has cached 
		/// (such as its <see cref="iGeospatial.Coordinates.Envelope"/>).
		/// </summary>
		public virtual void Changed()
		{
			Apply(GeometryComponentFilter.Instance);
		}
		
		/// <summary>  
		/// Determines whether or not this <see cref="Geometry"/> instance and
		/// the other Geometry passed in the parameter are disjoint.
		/// </summary>
		/// <param name="g">
		/// The Geometry with which to compare this Geometry.
		/// </param>
		/// <returns>true if the two <see cref="Geometry"/> instances are Disjoint
		/// </returns>
		/// <remarks>
		/// The two geometries are disjoint if the DE-9IM intersection matrix for the two 
		/// Geometry instances is FF*FF****.
		/// </remarks>
		public virtual bool Disjoint(Geometry g)
		{
			return !Intersects(g);
		}
		
		/// <summary>  
		/// Determines whether or not the current Geometry instance and the parameter touches.
		/// </summary>
		/// <param name="g">
		/// The Geometry with which to compare this Geometry.
		/// </param>
		/// <returns>
		/// true if the two <see cref="Geometry"/> instances touch; returns false if both <see cref="Geometry"/> instances are points.
		/// </returns>
		/// <remarks>
		/// The geometries touch if the DE-9IM intersection matrix 
		/// (<see cref="IntersectionMatrix"/> for the two <see cref="Geometry"/> 
		/// instances is FT*******, F**T***** or F***T****. 
		/// </remarks>
		public virtual bool Touches(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            // short-circuit test
            if (!this.Bounds.Intersects(g.Bounds))
                return false;

			return Relate(g).IsTouches(this.Dimension, g.Dimension);
		}
		
		/// <summary> 
		/// Determines whether or not the current <see cref="Geometry"/> instance and the
		/// parameter Geometry are disjoint. 
		/// </summary>
		/// <param name="g">The Geometry with which to compare this Geometry.</param>
		/// <returns> 
		/// true if the two <see cref="Geometry"/> instances intersect.
		/// </returns>
		public virtual bool Intersects(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            // short-circuit envelope test
            if (!this.Bounds.Intersects(g.Bounds))
                return false;

            // TODO: (MD) Add optimizations:
            //
            // - for P-A case:
            // If P is in env(A), test for point-in-poly
            //
            // - for A-A case:
            // If env(A1).overlaps(env(A2))
            // test for overlaps via point-in-poly first (both ways)
            // Possibly optimize selection of point to test by finding point of A1
            // closest to centre of env(A2).
            // (Is there a test where we shouldn't bother - e.g. if env A
            // is much smaller than env B, maybe there's no point in testing
            // pt(B) in env(A)?
			
            // optimizations for rectangle arguments
            if (this.IsRectangle) 
            {
                return RectangleIntersects.Intersects((Polygon)this, g);
            }

            if (g.IsRectangle) 
            {
                return RectangleIntersects.Intersects((Polygon)g, this);
            }

            // general case
			return Relate(g).Intersects;
		}
		
		/// <summary> 
		/// Determines whether or not the current <see cref="Geometry"/> instance and the
		/// parameter Geometry cross.
		/// </summary>
		/// <param name="g">
		/// The Geometry with which to compare this Geometry.
		/// </param>
		/// <returns> 
		/// true if the two <see cref="Geometry"/> instances cross. For this function 
		/// to return true, the geometries must be a point and a curve; a point and a 
		/// surface; two curves; or a curve and a surface.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The geometries cross if the DE-9IM intersection matrix 
		/// (<see cref="IntersectionMatrix"/>) for the two <see cref="Geometry"/> instances is
		/// </para>
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// T*T****** (for a point and a curve, a point and an area or a line
		/// and an area)
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// 0******** (for two curves).
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public virtual bool Crosses(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            // short-circuit test
            if (!this.Bounds.Intersects(g.Bounds))
                return false;
	
            return Relate(g).IsCrosses(Dimension, g.Dimension);
		}
		
		/// <summary>  
		/// Determines whether or not the current <see cref="Geometry"/> instance 
		/// is within the parameter Geometry.
		/// </summary>
		/// <param name="g">
		/// The Geometry with which to compare this Geometry.
		/// </param>
		/// <returns>true if this Geometry is within other.</returns>
		/// <remarks>
		/// <para>
		/// This will be true if the DE-9IM intersection matrix 
		/// (<see cref="IntersectionMatrix"/> for the two <see cref="Geometry"/> 
		/// instances is T*F**F***.
		/// </para>
		/// </remarks>
		public virtual bool Within(Geometry g)
		{
//			return Relate(g).Within;
                             
            return g.Contains(this);
		}
		
		/// <summary>  
		/// Determines whether or not this <see cref="Geometry"/> instance 
		/// contains the other Geometry passed in the parameter or the other Geometry
		/// is within this Geometry.
		/// </summary>
		/// <param name="g">The Geometry with which to compare this Geometry.</param>
		/// <returns>true if this Geometry contains other.</returns>
		/// <remarks>
		/// This test is similar to g.Within(this).
		/// </remarks>
		public virtual bool Contains(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            // short-circuit test
            if (!this.Bounds.Contains(g.Bounds))
                return false;

            // optimizations for rectangle arguments
            if (this.IsRectangle) 
            {
                return RectangleContains.Contains((Polygon)this, g);
            }

            // general case
            return Relate(g).Contains;
		}
		
		/// <summary>
		/// Determines whether or not the current <see cref="Geometry"/> instance 
		/// and other passed in the parameter overlap.
		/// </summary>
		/// <param name="g">The Geometry with which to compare this Geometry.</param>
		/// <returns>true if the two <see cref="Geometry"/> instances overlap. For 
		/// this function to return true, the geometries must be two points, 
		/// two curves or two surfaces.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The geometries overlap if the DE-9IM intersection matrix 
		/// (<see cref="IntersectionMatrix"/>) for the two <see cref="Geometry"/> instances is
		/// </para>
		/// <list type="bullet">
		/// <item>
		/// <description>
		/// T*T***T** (for two points or two surfaces)
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// 1*T***T** (for two curves).
		/// </description>
		/// </item>
		/// </list>
		/// </remarks>
		public virtual bool Overlaps(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            // short-circuit test
            if (!this.Bounds.Intersects(g.Bounds))
                return false;

			return Relate(g).IsOverlaps(this.Dimension, g.Dimension);
		}
		
        /// <summary> Returns <see langword="true"/> if this geometry covers the
        /// specified geometry.
        /// <p>
        /// The <code>covers</code> predicate has the following equivalent definitions:
        /// <ul>
        /// <li>Every point of the other geometry is a point of this geometry.
        /// <li>The DE-9IM Intersection Matrix for the two geometries is
        /// <code>T*****FF*</code>
        /// or <code>*T****FF*</code>
        /// or <code>***T**FF*</code>
        /// or <code>****T*FF*</code>
        /// <li><code>g.coveredBy(this)</code>
        /// (<code>covers</code> is the inverse of <code>coverdBy</code>)
        /// </ul>
        /// Note the difference between <code>covers</code> and <code>contains</code>
        /// - <code>covers</code> is a more inclusive relation.
        /// In particular, unlike <code>contains</code> it does not distinguish between
        /// points in the boundary and in the interior of geometries.
        /// For most situations, <code>covers</code> should be used in preference to <code>contains</code>.
        /// As an added benefit, <code>covers</code> is more amenable to optimization,
        /// and hence should be more performant.
        /// 
        /// </summary>
        /// <param name="g"> the <code>Geometry</code> with which to compare this <code>Geometry</code>
        /// </param>
        /// <returns>        <see langword="true"/> if this <code>Geometry</code> covers <code>g</code>
        /// 
        /// </returns>
        /// <seealso cref="Geometry.contains">
        /// </seealso>
        /// <seealso cref="Geometry.coveredBy">
        /// </seealso>
        public virtual bool Covers(Geometry g)
        {
            // short-circuit test
            if (!Bounds.Contains(g.Bounds))
                return false;
            // optimization for rectangle arguments
            if (this.IsRectangle)
            {
                return Bounds.Contains(g.Bounds);
            }

            return Relate(g).Covers;
        }
		
        /// <summary> Returns <see langword="true"/> if this geometry is covered by the
        /// specified geometry.
        /// <p>
        /// The <code>coveredBy</code> predicate has the following equivalent definitions:
        /// <ul>
        /// <li>Every point of this geometry is a point of the other geometry.
        /// <li>The DE-9IM Intersection Matrix for the two geometries is
        /// <code>T*F**F***</code>
        /// or <code>*TF**F***</code>
        /// or <code>**FT*F***</code>
        /// or <code>**F*TF***</code>
        /// <li><code>g.covers(this)</code>
        /// (<code>coveredBy</code> is the inverse of <code>covers</code>)
        /// </ul>
        /// Note the difference between <code>coveredBy</code> and <code>within</code>
        /// - <code>coveredBy</code> is a more inclusive relation.
        /// 
        /// </summary>
        /// <param name="g"> the <code>Geometry</code> with which to compare this <code>Geometry</code>
        /// </param>
        /// <returns>        <see langword="true"/> if this <code>Geometry</code> is covered by <code>g</code>
        /// 
        /// </returns>
        /// <seealso cref="Geometry.within">
        /// </seealso>
        /// <seealso cref="Geometry.covers">
        /// </seealso>
        public virtual bool IsCoveredBy(Geometry g)
        {
            return g.Covers(this);
        }
				
		/// <summary>  
		/// Determines whether or not the current <see cref="Geometry"/> instance
		/// and the other passed in the parameter relates as specified in the
		/// intersection matrix (<see cref="IntersectionMatrix"/>) pattern string.
		/// </summary>
		/// <param name="g">The Geometry with which to compare this Geometry.</param>
		/// <param name="intersectionPattern"> the pattern against which to check the
		/// intersection matrix for the two <see cref="Geometry"/> instances
		/// </param>
		/// <returns>
		/// true if the DE-9IM intersection matrix for the two Geometry instances 
		/// match intersectionPattern.
		/// </returns>
		/// <remarks>
		/// The operation returns true if the elements in the DE-9IM intersection matrix 
		/// (<see cref="IntersectionMatrix"/>) for the two <see cref="Geometry"/> 
		/// instances match the elements in intersectionPattern, which may be:
		/// <list type="number">
		/// <item><description>0</description></item>
		/// <item><description>1</description></item>
		/// <item><description>2</description></item>
		/// <item><description>T ( = 0, 1 or 2)</description></item>
		/// <item><description>F ( = -1)</description></item>
		/// <item><description>* ( = -1, 0, 1 or 2)</description></item>
		/// </list>
		/// For more information on the DE-9IM, see the OpenGIS Simple Features
		/// Specification.
		/// </remarks>
		public virtual bool Relate(Geometry g, string intersectionPattern)
		{
			return Relate(g).Matches(intersectionPattern);
		}
		
		/// <summary>  
		/// Computes the DE-9IM intersection matrix (<see cref="IntersectionMatrix"/>)
		/// for the two <see cref="Geometry"/> instances.
		/// </summary>
		/// <param name="g">The Geometry with which to compare this Geometry.</param>
		/// <returns>A matrix describing the intersections of the interiors,
		/// boundaries and exteriors of the two <see cref="Geometry"/> instances
		/// </returns>
		public virtual IntersectionMatrix Relate(Geometry g)
		{
			CheckNotGeometryCollection(this);
			CheckNotGeometryCollection(g);

			return RelateOp.Relate(this, g);
		}
		
		/// <summary>  
		/// Determines whether this <see cref="Geometry"/> instance and the other 
		/// passed in the parameter are equal.
		/// </summary>
		/// <param name="g">The Geometry with which to compare this Geometry.</param>
		/// <returns>true if the two Geometry instances are equal.</returns>
		/// <remarks>
		/// The two geometries are equal, according to the OpenGIS Simple Feature
		/// Specifications, if the DE-9IM intersection matrix (<see cref="IntersectionMatrix"/>
		/// for the two Geometry instances is T*F**FFF*.
		/// </remarks>
		public virtual bool Equals(Geometry g)
		{
            if (g == null)
            {
                throw new ArgumentNullException("g");
            }

            // short-circuit test
            if (!this.Bounds.Equals(g.Bounds))
                return false;

            return Relate(g).IsEquals(Dimension, g.Dimension);
		}
		
		public override string ToString()
		{
			string wkt = ToWKT();

            if (wkt == null || wkt.Length == 0)
            {
                return base.ToString();
            }

            return wkt;
		}
		
		/// <summary>  
		/// Retrieves the Well-known Text (WKT) representation of this Geometry.
		/// For a definition of the Well-known Text format, see the OpenGIS Simple
		/// Features Specification. 
		/// </summary>
		/// <returns> The Well-known Text representation of this Geometry
		/// </returns>
		/// <remarks>
		/// Note that this method requires the registration of the well-known 
		/// text format exporter.
		/// </remarks>
		public virtual string ToWKT()
		{
            try
            {
                int nExporters             = GeometryExporters.Exporters;
                IGeometryExporter exporter = null;

                if (nExporters > 0)
                {
                    exporter = GeometryExporters.GetExporter(
                        GeometryExportType.Wkt);
                }

                if (nExporters <= 0 || exporter == null)
                {
                    GeometryWktWriter writer = 
                        new GeometryWktWriter(this.PrecisionModel);

                    string strText = writer.Write(this);

                    return strText;
                }

                if (exporter != null && 
                    exporter.ExportType == GeometryExportType.Wkt)
                {
                    object objExport = exporter.Export(this);

                    if (objExport != null && 
                        objExport.GetType() == typeof(string))
                    {
                        return (string)objExport;
                    }
                }
           }
           catch (GeometryExportException ex)
           {
                ExceptionManager.Publish(ex);

                return null;
           }

           return null;
		}

        /// <summary>  
        /// Retrieves the Well-Known Binary (WKB) representation of this Geometry.
        /// For a definition of the Well-Known Binary format, see the OpenGIS Simple
        /// Features Specification. 
        /// </summary>
        /// <returns> 
        /// The Well-Known Binary representation of this Geometry as byte array.
        /// </returns>
        /// <remarks>
        /// Note that this method requires the registration of the well-known 
        /// binary format exporter.
        /// </remarks>
        public byte[] ToWKB()
        {
            try
            {
                int nExporters = GeometryExporters.Exporters;
                if (nExporters <= 0)
                {
                    GeometryWkbWriter writer = 
                        new GeometryWkbWriter();

                    byte[] arrBytes = writer.Write(this);

                    return arrBytes;
                }

                IGeometryExporter exporter = GeometryExporters.GetExporter(GeometryExportType.Wkb);

                if (exporter != null && exporter.ExportType == GeometryExportType.Wkb)
                {
                    object objExport = exporter.Export(this);

                    if (objExport != null && objExport.GetType() == typeof(byte[]))
                    {
                        return (byte[])objExport;
                    }
                }
            }
            catch (GeometryExportException ex)
            {
                ExceptionManager.Publish(ex);

                return null;
            }

            return null;
        }

        /// <summary>  
        /// Retrieves the Well-Known Binary (WKB) representation of this Geometry.
        /// For a definition of the Well-Known Binary format, see the OpenGIS Simple
        /// Features Specification. 
        /// </summary>
        /// <param name="order">A specified byte order of the exported binary.</param>
        /// <returns> 
        /// The Well-Known Binary representation of this Geometry as byte array.
        /// </returns>
        /// <remarks>
        /// Note that this method requires the registration of the well-known 
        /// binary format exporter.
        /// </remarks>
        public byte[] ToWKB(BytesOrder order)
        {
            try
            {
                int nExporters             = GeometryExporters.Exporters;
                IGeometryExporter exporter = null;

                if (nExporters > 0)
                {
                    exporter = GeometryExporters.GetExporter(GeometryExportType.Wkb);
                }

                if (nExporters <= 0 || exporter == null)
                {
                    GeometryWkbWriter writer = 
                        new GeometryWkbWriter();

                    byte[] arrBytes = writer.Write(this, order);

                    return arrBytes;
                }

                if (exporter != null && exporter.ExportType == GeometryExportType.Wkb)
                {
                    // set the byte order to the specified value.
                    exporter.ByteOrder = order;
                    object objExport = exporter.Export(this);

                    if (objExport != null && objExport.GetType() == typeof(byte[]))
                    {
                        return (byte[])objExport;
                    }
                }
            }
            catch (GeometryExportException ex)
            {
                ExceptionManager.Publish(ex);

                return null;
            }

            return null;
        }
		
        /// <summary>  
        /// Retrieves the Geography Markup Language (GML) representation of this Geometry.
        /// </summary>
        /// <returns> The Geography Markup Language representation of this Geometry.
        /// </returns>
        /// <remarks>
        /// Note that this method requires the registration of the GML format exporter.
        /// <para>
        /// The Geography Markup Language (GML) is an XML encoding for the 
        /// transport and storage of geographic information, including both the 
        /// geometry and properties of geographic features.
        /// <seealso href="http://www.opengis.org/">OpenGIS specification for 
        /// Geography Markup Language</seealso> for more information.
        /// </para>   
        /// </remarks>
        public string ToGML()
        {
            try
            {
                int nExporters             = GeometryExporters.Exporters;
                IGeometryExporter exporter = null;

                if (nExporters > 0)
                {
                    exporter = GeometryExporters.GetExporter(
                        GeometryExportType.Gml);
                }

                if (nExporters <= 0 || exporter == null)
                {
                    GeometryGml2Writer writer = 
                        new GeometryGml2Writer(this.PrecisionModel);

                    string strText = writer.Write(this);

                    return strText;
                }

                if (exporter != null && exporter.ExportType == GeometryExportType.Gml)
                {
                    object objExport = exporter.Export(this);

                    if (objExport != null && objExport.GetType() == typeof(string))
                    {
                        return (string)objExport;
                    }
                }
            }
            catch (GeometryExportException ex)
            {
                ExceptionManager.Publish(ex);

                return null;
            }

            return null;
        }
		
        /// <summary>  
        /// Retrieves the Geospatial-XML (G-XML) representation of this Geometry.
        /// </summary>
        /// <returns> The G-XML representation of this Geometry.
        /// </returns>
        /// <remarks>
        /// Note that this method requires the registration of the G-XML format exporter.
        /// <para>
        /// The Geospatial Extensible Markup Language (G-XML) is a Japanese XML 
        /// encoding for the transport and storage of geographic information, 
        /// including both the geometry and properties of geographic features.
        /// </para>
        /// </remarks>
        public string ToGXML()
        {
            try
            {
                IGeometryExporter exporter = GeometryExporters.GetExporter(GeometryExportType.GXml);

                if (exporter != null && exporter.ExportType == GeometryExportType.GXml)
                {
                    object objExport = exporter.Export(this);

                    if (objExport != null && objExport.GetType() == typeof(string))
                    {
                        return (string)objExport;
                    }
                }
            }
            catch (GeometryExportException ex)
            {
                ExceptionManager.Publish(ex);

                return null;
            }

            return null;
        }

		/// <overloads>
		/// Calculates a buffer region around the current geometry.
		/// </overloads>
		/// <summary>  
		/// Computes a buffer region around this <see cref="Geometry"/> having 
		/// the given width.
		/// </summary>
		/// <param name="distance"> 
		/// The width of the buffer, interpreted according to the 
		/// <see cref="iGeospatial.Coordinates.PrecisionModel"/> of the <see cref="Geometry"/>.
		/// </param>
		/// <returns> 
		/// All points whose distance from this <see cref="Geometry"/> are less than or 
		/// equal to distance.
		/// </returns>
		/// <remarks>
		/// The buffer of a <see cref="Geometry"/> is the Minkowski sum or 
		/// difference of the <see cref="Geometry"/> with a disc of radius 
		/// distance.
		/// </remarks>
		public virtual Geometry Buffer(double distance)
		{
			return BufferOp.Buffer(this, distance);
		}

        /// <summary>
        /// Calculates a buffer region around this <see cref="Geometry"/> 
        /// having the given width.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <see cref="PrecisionModel"/> of the <see cref="Geometry"/>.
        /// </param>
        /// <param name="capStyle">
        /// An enumeration, <see cref="BufferCapType"/>, specifying the cap style to use for compute buffer.
        /// </param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry"/>
        /// are less than or equal to distance.
        /// </returns>
        /// <remarks>
        /// The buffer of a <see cref="Geometry"/> is the Minkowski sum or 
        /// difference of the <see cref="Geometry"/> with a disc of radius 
        /// distance.
        /// </remarks>
        public virtual Geometry Buffer(double distance, BufferCapType capStyle)
        {
            return BufferOp.Buffer(this, distance, capStyle);
        }
		
		/// <summary>  
		/// Calculates a buffer region around this <see cref="Geometry"/> having 
		/// the given width and with a specified number of segments used to 
		/// approximate curves.
		/// </summary>
		/// <param name="distance">The width of the buffer, interpreted according to the
		/// <see cref="PrecisionModel"/> of the <see cref="Geometry"/>.
		/// </param>
		/// <param name="quadrantSegments">
		/// The number of segments to use to approximate a quadrant of a circle.
		/// </param>
		/// <returns> 
		/// Returns all points whose distance from this <see cref="Geometry"/> are less than or 
		/// equal to distance.
		/// </returns>
		/// <remarks>
		/// The buffer of a <see cref="Geometry"/> is the Minkowski sum of the Geometry with
		/// a disc of radius distance. Curves in the buffer polygon are
		/// approximated with line segments. This method allows specifying the
		/// accuracy of that approximation.
		/// </remarks>
		public virtual Geometry Buffer(double distance, int quadrantSegments)
		{
			return BufferOp.Buffer(this, distance, quadrantSegments);
		}

        /// <summary>
        /// Calculates a buffer region around this <see cref="Geometry"/> having 
        /// the given width and with a specified number of segments used to 
        /// approximate curves.
        /// </summary>
        /// <param name="distance">
        /// The width of the buffer, interpreted according to the
        /// <see cref="PrecisionModel"/> of the <see cref="Geometry"/>.
        /// </param>
        /// <param name="quadrantSegments">
        /// The number of segments to use to approximate a quadrant of a circle.
        /// </param>
        /// <param name="capStyle">
        /// The cap style to use for compute buffer.
        /// </param>
        /// <returns>
        /// All points whose distance from this <see cref="Geometry"/> are less 
        /// than or equal to specified distance.
        /// </returns>
        /// <remarks>
        /// The buffer of a <see cref="Geometry"/> is the Minkowski sum of the 
        /// <see cref="Geometry"/> with a disc of radius distance. Curves in the 
        /// buffer polygon are approximated with line segments. This method 
        /// allows specifying the accuracy of that approximation.
        /// </remarks>
        public virtual Geometry Buffer(double distance, int quadrantSegments, 
            BufferCapType capStyle)
        {
            return BufferOp.Buffer(this, distance, quadrantSegments, capStyle);
        } 
		
		/// <summary> 
		/// Computes the smallest convex <see cref="Polygon"/> that contains all the
		/// points in the <see cref="Geometry"/>. 
		/// </summary>
		/// <returns>
		/// The minimum-area convex polygon containing this Geometry's points.
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
		public virtual Geometry ConvexHull()
		{
			return (new ConvexHull(this)).ComputeConvexHull();
		}
		
		/// <summary>  
		/// Returns a Geometry representing the points shared by this
		/// Geometry and other.
		/// </summary>
		/// <param name="other"> the Geometry with which to compute the
		/// intersection
		/// </param>
		/// <returns>        the points common to the two <see cref="Geometry"/> instances
		/// </returns>
		public virtual Geometry Intersection(Geometry other)
		{
			CheckNotGeometryCollection(this);
			CheckNotGeometryCollection(other);

            Geometry geoTemp1 = this;
            Geometry geoTemp2 = other;
			return OverlayOp.Overlay(geoTemp1, geoTemp2, 
                OverlayType.Intersection);
		}
		
		/// <summary>  
		/// Computes a <see cref="Geometry"/> representing all the points in this 
		/// Geometry instance and other specified Geometry.
		/// </summary>
		/// <param name="other">
		/// The Geometry with which to compute the union.
		/// </param>
		/// <returns>
		/// A set combining the points of this Geometry and the points of other.
		/// </returns>
		public virtual Geometry Union(Geometry other)
		{
			CheckNotGeometryCollection(this);
			CheckNotGeometryCollection(other);

			return OverlayOp.Overlay(this, other, OverlayType.Union);
		}
		
		/// <summary>  
		/// Computes a <see cref="Geometry"/> representing the points making up this
		/// Geometry that do not make up other. This method returns the closure of 
		/// the resultant Geometry.
		/// </summary>
		/// <param name="other">
		/// The Geometry with which to compute the difference.
		/// </param>
		/// <returns>
		/// The point set difference of this Geometry with other.
		/// </returns>
		public virtual Geometry Difference(Geometry other)
		{
			CheckNotGeometryCollection(this);
			CheckNotGeometryCollection(other);

			return OverlayOp.Overlay(this, other, 
                OverlayType.Difference);
		}
		
		/// <summary>  
		/// Returns a set combining the points in this <see cref="Geometry"/> not in
		/// other, and the points in other not in this Geometry. This method returns 
		/// the closure of the resultant Geometry.
		/// </summary>
		/// <param name="other">
		/// The Geometry with which to compute the symmetric difference.
		/// </param>
		/// <returns>
		/// The point set symmetric difference of this Geometry with other.
		/// </returns>
		public virtual Geometry SymmetricDifference(Geometry other)
		{
			CheckNotGeometryCollection(this);
			CheckNotGeometryCollection(other);

			return OverlayOp.Overlay(this, other, 
                OverlayType.SymmetricDifference);
		}
		
		/// <summary>  
		/// Returns true if the two <see cref="Geometry"/> instances are exactly equal,
		/// up to a specified tolerance.
		/// </summary>
		/// <param name="other">The Geometry with which to compare this Geometry
		/// </param>
		/// <param name="tolerance">
		/// Distance at or below which two Coordinates will be considered equal.
		/// </param>
		/// <returns> true if this and the other Geometry
		/// are of the same class and have equal internal data.
		/// </returns>
		/// <remarks>
		/// Two geometries are exactly within a tolerance equal if the 
		/// following conditions are satisfied:
		/// <list type="number">
		/// <item>
		/// <description>
		/// they have the same class,
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// they have the same values of <see cref="iGeospatial.Coordinates.Coordinate"/>, 
		/// within the given tolerance distance, in their internal 
		/// <see cref="iGeospatial.Coordinates.Coordinate"/> lists, in exactly 
		/// the same order.
		/// </description>
		/// </item>
		/// </list>
		/// If this and the other <see cref="Geometry"/> instances are
		/// composites and any children are not <see cref="Geometry"/> instances, returns
		/// false.
		/// </remarks>
		public abstract bool EqualsExact(Geometry other, double tolerance);
		
		/// <summary>  
		/// Returns true if the two <see cref="Geometry"/> instances are exactly equal.
		/// </summary>
		/// <param name="other"> the Geometry with which to compare this Geometry
		/// </param>
		/// <returns>        true if this and the other Geometry
		/// are of the same class and have equal internal data.
		/// </returns>
		/// <remarks>
		/// Two Geometries are exactly equal if the following conditions are satisfied:
		/// <list type="number">
		/// <item>
		/// <description>
		/// they have the same class
		/// </description>
		/// </item>
		/// <item>
		/// <description>
		/// they have the same values of Coordinates in their internal
		/// </description>
		/// </item>
		/// Coordinate lists, in exactly the same order.
		/// </list>
		/// If this and the other <see cref="Geometry"/> instances are
		/// composites and any children are not <see cref="Geometry"/> instances, returns
		/// false.
		/// <para>
		/// This provides a stricter test of equality than equals.
		/// </para>
		/// </remarks>
		public virtual bool EqualsExact(Geometry other)
		{
			return EqualsExact(other, 0);
		}
		
		/// <summary>  
		/// Performs an operation with or on this <see cref="Geometry"/>'s 
		/// coordinates. 
		/// </summary>
		/// <param name="filter">
		/// The filter to pply to this Geometry's coordinates.
		/// </param>
		/// <remarks>
		/// If you are using this method to modify the geometry, be sure to call 
		/// <see cref="Geometry.Changed()"/> afterwards. Note that you cannot 
		/// use this method to modify this <see cref="Geometry"/> if its underlying 
		/// <see cref="CoordinateCollection.GetCoordinate"/> method
		/// returns a copy of the Coordinate, rather than the actual Coordinate stored
		/// (if it even stores Coordinates at all).
		/// </remarks>
		public abstract void Apply(ICoordinateVisitor filter);
		
		/// <summary>  
		/// Performs an operation with or on this <see cref="Geometry"/> and its 
		/// subelement <see cref="Geometry"/> instances (if any).
		/// </summary>
		/// <param name="filter"> 
		/// The filter to apply to this <see cref="Geometry"/> (and
		/// its children, if it is a <see cref="GeometryCollection"/>).
		/// </param>
		/// <remarks>
		/// Only geometry collections (<see cref="GeometryCollection"/>) and 
		/// subclasses have subelement geometries.
		/// </remarks>
		public abstract void  Apply(IGeometryVisitor filter);
		
		/// <summary>  
		/// Performs an operation with or on this <see cref="Geometry"/> and its
		/// component geometries.  
		/// </summary>
		/// <param name="filter"> 
		/// The filter to apply to this <see cref="Geometry"/>.
		/// </param>
		/// <remarks>
		/// Only geometry collections (<see cref="GeometryCollection"/>) and 
		/// polygons (<see cref="Polygon"/>) have component geometries; for 
		/// polygons they are the linear rings (<see cref="LinearRing"/>) of the 
		/// shell and holes.
		/// </remarks>
		public abstract void Apply(IGeometryComponentVisitor filter);
		
		/// <summary>  
		/// Converts this <see cref="Geometry"/> to normal form (or canonical 
		/// form). 
		/// </summary>
		/// <remarks>
		/// Normal form is a unique representation for <see cref="Geometry"/>. 
		/// It can be used to test whether two <see cref="Geometry"/> instances 
		/// are equal in a way that is independent of the ordering of the coordinates Within
		/// them. 
		/// <para>
		/// Normal form equality is a stronger condition than topological
		/// equality, but weaker than pointwise equality. The definitions for normal
		/// form use the standard lexicographical ordering for coordinates. "Sorted in
		/// order of coordinates" means the obvious extension of this ordering to
		/// sequences of coordinates.
		/// </para>
		/// </remarks>
		public abstract void Normalize();
		
		/// <summary>  
		/// Determines whether this <see cref="Geometry"/> is greater than, equal to,
		/// or less than another <see cref="Geometry"/>.
		/// </summary>
		/// <param name="o"> 
		/// A <see cref="Geometry"/> with which to compare this <see cref="Geometry"/>.
		/// </param>
		/// <returns>
		/// A positive number, 0, or a negative number, depending on whether
		/// this object is greater than, equal to, or less than o, as
		/// defined in "Normal Form For Geometry" in the Technical
		/// Specifications
		/// </returns>
		/// <remarks>
		/// If their classes are different, they are compared using the following
		/// ordering:
		/// <list type="number">
		/// <item><description>Point (lowest)</description></item>
		/// <item><description>MultiPoint</description></item>
		/// <item><description>LineString</description></item>
		/// <item><description>LinearRing</description></item>
		/// <item><description>MultiLineString</description></item>
		/// <item><description>Polygon</description></item>
		/// <item><description>MultiPolygon</description></item>
		/// <item><description>GeometryCollection (highest)</description></item>
		/// </list>
		/// If the two <see cref="Geometry"/> instances have the same class, their first
		/// elements are compared. If those are the same, the second elements are
		/// compared, etc.
		/// </remarks>
		public virtual int CompareTo(object o)
		{
			Geometry other = (Geometry) o;
			if (SortIndex != other.SortIndex)
			{
				return SortIndex - other.SortIndex;
			}
			
            if (IsEmpty && other.IsEmpty)
			{
				return 0;
			}
			
            if (IsEmpty)
			{
				return -1;
			}
			
            if (other.IsEmpty)
			{
				return 1;
			}

			return CompareToGeometry(other);
		}
		
        /// <summary>
        /// Retrieves the <see cref="Geometry"/> at the specified index.
        /// </summary>
        /// <param name="n">
        /// The zero-based index of the <see cref="Geometry"/> to retrieve.
        /// </param>
        /// <returns>
        /// This returns the <see cref="Geometry"/> at the specified index, if any
        /// or returns a reference to itself if this is not a collection or a
        /// container.
        /// </returns>
        public virtual Geometry GetGeometry(int n)
        {
            return this;
        }
        
        #endregion

        #region Protected Methods
		
		/// <summary> 
		/// Notifies this Geometry that its Coordinates have been changed by an external
		/// party. When <see cref="Geometry.GeometryChanged"/> is called, this method 
		/// will be called for this Geometry and its component Geometries.
		/// </summary>
		/// <seealso cref="Geometry.Apply(IGeometryComponentVisitor)">
		/// </seealso>
		protected virtual void OnChanged()
		{
			m_objEnvelope = null;
		}
		
		/// <summary>  
		/// Returns whether the two <see cref="Geometry"/> instances are equal, 
		/// from the point of view of the EqualsExact method. Called by 
		/// EqualsExact. In general, two Geometry classes are considered to be
		/// "equivalent" only if they are the same class. An exception is 
		/// LineString, which is considered to be equivalent to its subclasses.
		/// </summary>
		/// <param name="other">
		/// The <see cref="Geometry"/> with which to compare this Geometry for equality.
		/// </param>
		/// <returns> true if the classes of the two Geometry
		/// s are considered to be equal by the EqualsExact method.
		/// </returns>
		protected virtual bool IsEquivalentType(Geometry other)
		{
            return (this.GeometryType == other.GeometryType);
		}
		
		/// <summary>  
		/// Throws an exception if g's class is <see cref="GeometryCollection"/>. 
		/// (Its subclasses do not trigger an exception). 
		/// </summary>
		/// <param name="g">The <see cref="Geometry"/> to check or validate.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// If g is a GeometryCollection but not one of its subclasses
		/// </exception>
		protected virtual void CheckNotGeometryCollection(Geometry g)
		{
			//Don't use typeof because we want to allow subclasses
			if (g.GeometryType == GeometryType.GeometryCollection)
			{
				throw new System.ArgumentException("This method does not support GeometryCollection arguments");
			}
		}
		
		/// <summary>  
		/// Returns the minimum and maximum x and y values in this Geometry, 
		/// or a null Envelope if this Geometry is empty.
		/// Unlike <c>EnvelopeInternal</c>, this method calculates the Envelope
		/// each time it is called; <c>EnvelopeInternal</c> caches the result
		/// of this method.
		/// </summary>
		/// <returns>This <see cref="Geometry"/> instances bounding box; if the Geometry
		/// is empty, <see cref="iGeospatial.Coordinates.Envelope.IsNull"/> 
		/// will return true.
		/// </returns>
		protected abstract Envelope ComputeEnvelope();
		
		/// <summary>  
		/// Returns whether this <see cref="Geometry"/> is greater than, equal to, 
		/// or less than another Geometry having the same class.
		/// </summary>
		/// <param name="o"> 
		/// A Geometry instance having the same class as this Geometry.
		/// </param>
		/// <returns>    a positive number, 0, or a negative number, depending on whether
		/// this object is greater than, equal to, or less than o, as
		/// defined in "Normal Form For Geometry" in the OTS Technical
		/// Specifications
		/// </returns>
		protected abstract int CompareToGeometry(Geometry o);
        
        #endregion
		
        #region Internal Static Methods

		/// <summary>  
		/// Returns true if the array Contains any non-empty <see cref="Geometry"/> instances.
		/// </summary>
		/// <param name="geometries"> 
		/// An array of <see cref="Geometry"/> instances; no elements may be null
		/// </param>
		/// <returns> 
		/// true if any of the <see cref="Geometry"/> instances IsEmpty methods return false
		/// </returns>
		internal static bool HasNonEmptyElements(Geometry[] geometries)
		{
			for (int i = 0; i < geometries.Length; i++)
			{
				if (!geometries[i].IsEmpty)
				{
					return true;
				}
			}

			return false;
		}
		
		/// <summary>  
		/// Returns true if the array Contains any null elements.
		/// </summary>
		/// <param name="array">An array to validate</param>
		/// <returns> 
		/// true if any of arrays elements are null.
		/// </returns>
		internal static bool HasNullElements(Geometry[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					return true;
				}
			}

			return false;
		}
        
        #endregion

        #region IGeometry Members

        IGeometry IGeometry.Envelope
        {
            get
            {
                return this.Envelope;
            }
        }

        IGeometry IGeometry.Boundary
        {
            get
            {
                return this.Boundary;
            }
        }

        bool IGeometry.Equals(IGeometry otherGeometry)
        {
            return this.Equals((Geometry)otherGeometry);
        }

        bool IGeometry.Disjoint(IGeometry otherGeometry)
        {
            return this.Disjoint((Geometry)otherGeometry);
        }

        bool IGeometry.Intersects(IGeometry otherGeometry)
        {
            return this.Intersects((Geometry)otherGeometry);
        }

        bool IGeometry.Touches(IGeometry otherGeometry)
        {
            return this.Touches((Geometry)otherGeometry);
        }

        bool IGeometry.Crosses(IGeometry otherGeometry)
        {
            return this.Crosses((Geometry)otherGeometry);
        }

        bool IGeometry.Within(IGeometry otherGeometry)
        {
            return this.Within((Geometry)otherGeometry);
        }

        bool IGeometry.Contains(IGeometry otherGeometry)
        {
            return this.Contains((Geometry)otherGeometry);
        }

        bool IGeometry.Overlaps(IGeometry otherGeometry)
        {
            return this.Overlaps((Geometry)otherGeometry);
        }

        bool IGeometry.Relate(IGeometry otherGeometry, string intersectionPattern)
        {
            return this.Relate((Geometry)otherGeometry, intersectionPattern);
        }

        IntersectionMatrix IGeometry.Relate(IGeometry otherGeometry)
        {
            return this.Relate((Geometry)otherGeometry);
        }

        IGeometry IGeometry.Buffer(double distance)
        {
            return this.Buffer(distance);
        }

        IGeometry IGeometry.ConvexHull()
        {
            return this.ConvexHull();
        }

        IGeometry IGeometry.Intersection(IGeometry otherGeometry)
        {
            return this.Intersection((Geometry)otherGeometry);
        }

        IGeometry IGeometry.Union(IGeometry otherGeometry)
        {
            return this.Union((Geometry)otherGeometry);
        }

        IGeometry IGeometry.Difference(IGeometry otherGeometry)
        {
            return this.Difference((Geometry)otherGeometry);
        }

        IGeometry IGeometry.SymmetricDifference(IGeometry otherGeometry)
        {
            return this.SymmetricDifference((Geometry)otherGeometry);
        }

        bool IGeometry.IsWithinDistance(IGeometry geom, double distance)
        {
            return this.IsWithinDistance((Geometry)geom, distance);
        }

        #endregion

        #region ICloneable Members
		
        public virtual Geometry Clone()
        {
            Geometry cloned = (Geometry) base.MemberwiseClone();
            if (cloned.m_objEnvelope != null)
            {
                cloned.m_objEnvelope = new Envelope(cloned.m_objEnvelope);
            }

            return cloned;
        }

        IGeometry IGeometry.Clone()
        {
            return this.Clone();
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
		
        #region GeometryComponentFilter Class

        internal sealed class GeometryComponentFilter : IGeometryComponentVisitor
        {
            private static readonly IGeometryComponentVisitor 
                geometryChangedFilter = new GeometryComponentFilter();

            private GeometryComponentFilter()
            {
            }

            public void Visit(Geometry geom)
            {
                if (geom == null)
                {
                    throw new ArgumentNullException("geom");
                }

                geom.OnChanged();
            }

            public static IGeometryComponentVisitor Instance 
            {
                get
                {
                    return geometryChangedFilter;
                }
            }
        }
        
        #endregion
    }
}