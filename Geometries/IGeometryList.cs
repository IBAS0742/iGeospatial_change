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

namespace iGeospatial.Geometries
{
    /// <summary>
    /// Represents a strongly typed collection of <see cref="Geometry"/>
    /// objects that can be individually accessed by index.
    /// </summary>
    /// <remarks>
    /// <c>IGeometryList</c> provides an <see cref="IList"/>
    /// that is strongly typed for <see cref="Geometry"/> elements.
    /// </remarks>
    public interface IGeometryList : IGeometryCollection, ICloneable 
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the <see cref="IGeometryList"/> has a fixed size.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="IGeometryList"/> has a fixed size;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>Please refer to <see cref="IList.IsFixedSize"/> for details.</remarks>
        bool IsFixedSize { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IGeometryList"/> is read-only.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="IGeometryList"/> is read-only;
        /// otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>Please refer to <see cref="IList.IsReadOnly"/> for details.</remarks>
        bool IsReadOnly { get; }

        /// <summary>
        /// Gets or sets the <see cref="Geometry"/> element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the
        /// <see cref="Geometry"/> element to get or set.</param>
        /// <value>
        /// The <see cref="Geometry"/> element at the specified <paramref name="index"/>.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="IGeometryCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="IGeometryList"/> is read-only.</exception>
        /// <remarks>Please refer to <see cref="IList.this"/> for details.</remarks>
        Geometry this[int index] { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a <see cref="Geometry"/> to the end
        /// of the <see cref="IGeometryList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to be added to the end of the <see cref="IGeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>The <see cref="IGeometryList"/> index at which
        /// the <paramref name="value"/> has been added.</returns>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IGeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>IGeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Add"/> for details.</remarks>
        int Add(Geometry value);

        /// <summary>
        /// Removes all elements from the <see cref="IGeometryList"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IGeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>IGeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Clear"/> for details.</remarks>
        void Clear();

        /// <summary>
        /// Determines whether the <see cref="IGeometryList"/>
        /// contains the specified <see cref="Geometry"/> element.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to locate in the <see cref="IGeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is found in the
        /// <see cref="IGeometryList"/>; otherwise, <see langword="false"/>.</returns>
        /// <remarks>Please refer to <see cref="IList.Contains"/> for details.</remarks>
        bool Contains(Geometry value);

        /// <summary>
        /// Returns the zero-based index of the first occurrence of the specified
        /// <see cref="Geometry"/> in the <see cref="IGeometryList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to locate in the <see cref="IGeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <returns>
        /// The zero-based index of the first occurrence of <paramref name="value"/>
        /// in the <see cref="IGeometryList"/>, if found; otherwise, -1.
        /// </returns>
        /// <remarks>Please refer to <see cref="IList.IndexOf"/> for details.</remarks>
        int IndexOf(Geometry value);

        /// <summary>
        /// Inserts a <see cref="Geometry"/> element into the
        /// <see cref="IGeometryList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which
        /// <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to insert into the <see cref="IGeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is greater than
        /// <see cref="IGeometryCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IGeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>IGeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Insert"/> for details.</remarks>
        void Insert(int index, Geometry value);

        /// <summary>
        /// Removes the first occurrence of the specified <see cref="Geometry"/>
        /// from the <see cref="IGeometryList"/>.
        /// </summary>
        /// <param name="value">The <see cref="Geometry"/> object
        /// to remove from the <see cref="IGeometryList"/>.
        /// This argument can be a null reference.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IGeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>IGeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.Remove"/> for details.</remarks>
        void Remove(Geometry value);

        /// <summary>
        /// Removes the element at the specified index of the
        /// <see cref="IGeometryList"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para><paramref name="index"/> is less than zero.</para>
        /// <para>-or-</para>
        /// <para><paramref name="index"/> is equal to or greater than
        /// <see cref="IGeometryCollection.Count"/>.</para>
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// <para>The <see cref="IGeometryList"/> is read-only.</para>
        /// <para>-or-</para>
        /// <para>The <c>IGeometryList</c> has a fixed size.</para></exception>
        /// <remarks>Please refer to <see cref="IList.RemoveAt"/> for details.</remarks>
        void RemoveAt(int index);

        /// <summary>
        /// Copies the elements of the <see cref="GeometryList"/> to a new
        /// <see cref="Array"/> of <see cref="Geometry"/> elements.
        /// </summary>
        /// <returns>A one-dimensional <see cref="Array"/> of <see cref="Geometry"/>
        /// elements containing copies of the elements of the <see cref="GeometryList"/>.</returns>
        /// <remarks>Please refer to <see cref="ArrayList.ToArray"/> for details.</remarks>
        Geometry[] ToArray();

        #endregion
    }

}
