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
    /// Defines size, enumerators, and synchronization methods for strongly
    /// typed collections of <see cref="Geometry"/> elements.
    /// </summary>
    /// <remarks>
    /// <c>IGeometryCollection</c> provides an <see cref="ICollection"/>
    /// that is strongly typed for <see cref="Geometry"/> elements.
    /// </remarks>
    public interface IGeometryCollection 
    {
        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the
        /// <see cref="IGeometryCollection"/>.
        /// </summary>
        /// <value>The number of elements contained in the
        /// <see cref="IGeometryCollection"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.Count"/> for details.</remarks>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether access to the
        /// <see cref="IGeometryCollection"/> is synchronized (thread-safe).
        /// </summary>
        /// <value><see langword="true"/> if access to the <see cref="IGeometryCollection"/> is
        /// synchronized (thread-safe); otherwise, <see langword="false"/>. The default is <see langword="false"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.IsSynchronized"/> for details.</remarks>
        bool IsSynchronized { get; }

        /// <summary>
        /// Gets an object that can be used to synchronize access
        /// to the <see cref="IGeometryCollection"/>.
        /// </summary>
        /// <value>An object that can be used to synchronize access
        /// to the <see cref="IGeometryCollection"/>.</value>
        /// <remarks>Please refer to <see cref="ICollection.SyncRoot"/> for details.</remarks>
        object SyncRoot { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Copies the entire <see cref="IGeometryCollection"/> to a one-dimensional <see cref="Array"/>
        /// of <see cref="Geometry"/> elements, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the
        /// <see cref="Geometry"/> elements copied from the <see cref="IGeometryCollection"/>.
        /// The <c>Array</c> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/>
        /// at which copying begins.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than zero.</exception>
        /// <exception cref="ArgumentException"><para>
        /// <paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.
        /// </para><para>-or-</para><para>
        /// The number of elements in the source <see cref="IGeometryCollection"/> is greater
        /// than the available space from <paramref name="arrayIndex"/> to the end of the destination
        /// <paramref name="array"/>.</para></exception>
        /// <remarks>Please refer to <see cref="ICollection.CopyTo"/> for details.</remarks>
        void CopyTo(Geometry[] array, int arrayIndex);

        /// <summary>
        /// Returns an <see cref="IGeometryEnumerator"/> that can
        /// iterate through the <see cref="IGeometryCollection"/>.
        /// </summary>
        /// <returns>An <see cref="IGeometryEnumerator"/>
        /// for the entire <see cref="IGeometryCollection"/>.</returns>
        /// <remarks>Please refer to <see cref="IEnumerable.GetEnumerator"/> for details.</remarks>
        IGeometryEnumerator GetEnumerator();

        #endregion
    }
}
