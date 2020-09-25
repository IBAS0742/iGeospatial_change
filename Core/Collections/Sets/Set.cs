/* Copyright © 2002-2004 by Aidant Systems, Inc., and by Jason Smith. */ 
using System;
using System.Collections;
using System.Reflection;
                  
namespace iGeospatial.Collections.Sets
{
	/// <summary><p>A collection that contains no duplicate elements.  This class models the mathematical
	/// <c>Set</c> abstraction, and is the base class for all other <c>Set</c> implementations.  
	/// The order of elements in a set is dependant on (a)the data-structure implementation, and 
	/// (b)the implementation of the various <c>Set</c> methods, and thus is not guaranteed.</p>
	///  
	/// <p><c>Set</c> overrides the <c>Equals()</c> method to test for "equivalency": whether the 
	/// two sets contain the same elements.  The "==" and "!=" operators are not overridden by 
	/// design, since it is often desirable to compare object references for equality.</p>
	/// 
	/// <p>Also, the <c>GetHashCode()</c> method is not implemented on any of the set implementations, since none
	/// of them are truely immutable.  This is by design, and it is the way almost all collections in 
	/// the .NET framework function.  So as a general rule, don't store collection objects inside <c>Set</c>
	/// instances.  You would typically want to use a keyed <c>IDictionary</c> instead.</p>
	/// 
	/// <p>None of the <c>Set</c> implementations in this library are guranteed to be thread-safe
	/// in any way unless wrapped in a <c>SynchronizedSet</c>.</p>
	/// 
	/// <p>The following table summarizes the binary operators that are supported by the <c>Set</c> class.</p>
	/// <list type="table">
	///		<listheader>
	///			<term>Operation</term>
	///			<term>Description</term>
	///			<term>Method</term>
	///			<term>Operator</term>
	///		</listheader>
	///		<item>
	///			<term>Union (OR)</term>
	///			<term>Element included in result if it exists in either <c>A</c> OR <c>B</c>.</term>
	///			<term><c>Union()</c></term>
	///			<term><c>|</c></term>
	///		</item>
	///		<item>
	///			<term>Intersection (AND)</term>
	///			<term>Element included in result if it exists in both <c>A</c> AND <c>B</c>.</term>
	///			<term><c>InterSect()</c></term>
	///			<term><c>&amp;</c></term>
	///		</item>
	///		<item>
	///			<term>Exclusive Or (XOR)</term>
	///			<term>Element included in result if it exists in one, but not both, of <c>A</c> and <c>B</c>.</term>
	///			<term><c>ExclusiveOr()</c></term>
	///			<term><c>^</c></term>
	///		</item>
	///		<item>
	///			<term>Minus (n/a)</term>
	///			<term>Take all the elements in <c>A</c>.  Now, if any of them exist in <c>B</c>, remove
	///			them.  Note that unlike the other operators, <c>A - B</c> is not the same as <c>B - A</c>.</term>
	///			<term><c>Minus()</c></term>
	///			<term><c>-</c></term>
	///		</item>
	/// </list>
	/// </summary>
    public abstract class Set : ISet
	{
        #region Public Properties

        /// <summary>
        /// Returns <c>true</c> if this set contains no elements.
        /// </summary>
        public abstract bool IsEmpty{get;}

        /// <summary>
        /// The number of elements currently contained in this collection.
        /// </summary>
        public abstract int Count{get;}

        /// <summary>
        /// Returns <c>true</c> if the <c>Set</c> is synchronized across threads.  Note that
        /// enumeration is inherently not thread-safe.  Use the <c>SyncRoot</c> to lock the
        /// object during enumeration.
        /// </summary>
        public abstract bool IsSynchronized{get;}

        /// <summary>
        /// An object that can be used to synchronize this collection to make it thread-safe.
        /// When implementing this, if your object uses a base object, like an <c>IDictionary</c>,
        /// or anything that has a <c>SyncRoot</c>, return that object instead of "<c>this</c>".
        /// </summary>
        public abstract object SyncRoot{get;}

        #endregion

        #region Public Methods

		/// <summary>
		/// Performs a "union" of the two sets, where all the elements
		/// in both sets are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
		/// Neither this set nor the input set are modified during the operation.  The return value
		/// is a <c>Clone()</c> of this set with the extra elements added in.
		/// </summary>
		/// <param name="a">A collection of elements.</param>
		/// <returns>A new <c>Set</c> containing the union of this <c>Set</c> with the specified collection.
		/// Neither of the input objects is modified by the union.</returns>
		public virtual ISet Union(ISet a)
		{
			ISet resultSet = (ISet)this.Clone();
			if(a != null)
				resultSet.AddAll(a);
			return resultSet;
		}
		
		/// <summary>
		/// Performs a "union" of two sets, where all the elements
		/// in both are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
		/// The return value is a <c>Clone()</c> of one of the sets (<c>a</c> if it is not <c>null</c>) with elements of the other set
		/// added in.  Neither of the input sets is modified by the operation.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>A set containing the union of the input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
		public static ISet Union(ISet a, ISet b)
		{
			if (a == null && b == null)
				return null;
			else if(a == null)
				return (ISet)b.Clone();
			else if(b == null)
				return (ISet)a.Clone();
			else
				return a.Union(b);
		}

		/// <summary>
		/// Performs a "union" of two sets, where all the elements
		/// in both are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
		/// The return value is a <c>Clone()</c> of one of the sets (<c>a</c> if it is not <c>null</c>) with elements of the other set
		/// added in.  Neither of the input sets is modified by the operation.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>A set containing the union of the input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
		public static Set operator | (Set a, Set b)
		{
			return (Set)Union(a, b);
		}

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.  That is, the element is included if it exists in
		/// both sets.  The <c>Intersect()</c> operation does not modify the input sets.  It returns
		/// a <c>Clone()</c> of this set with the appropriate elements removed.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <returns>The intersection of this set with <c>a</c>.</returns>
		public virtual ISet Intersect(ISet a)
		{
			ISet resultSet = (ISet)this.Clone();
			if(a != null)
				resultSet.RetainAll(a);
			else
				resultSet.Clear();
			return resultSet;
		}

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.  That is, the element is included only if it exists in
		/// both <c>a</c> and <c>b</c>.  Neither input object is modified by the operation.
		/// The result object is a <c>Clone()</c> of one of the input objects (<c>a</c> if it is not <c>null</c>) containing the
		/// elements from the intersect operation. 
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>The intersection of the two input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
		public static ISet Intersect(ISet a, ISet b)
		{
			if(a == null && b == null)
				return null;
			else if(a == null)
			{
				return b.Intersect(a);
			}
			else
				return a.Intersect(b);
		}

		/// <summary>
		/// Performs an "intersection" of the two sets, where only the elements
		/// that are present in both sets remain.  That is, the element is included only if it exists in
		/// both <c>a</c> and <c>b</c>.  Neither input object is modified by the operation.
		/// The result object is a <c>Clone()</c> of one of the input objects (<c>a</c> if it is not <c>null</c>) containing the
		/// elements from the intersect operation. 
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>The intersection of the two input sets.  <c>null</c> if both sets are <c>null</c>.</returns>
		public static Set operator & (Set a, Set b)
		{
			return (Set)Intersect(a, b);
		}

		/// <summary>
		/// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
		/// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
		/// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
		/// of this <c>Set</c> containing the elements from the operation.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <returns>A set containing the elements from this set with the elements in <c>a</c> removed.</returns>
		public virtual ISet Minus(ISet a)
		{
			ISet resultSet = (ISet)this.Clone();
			if(a != null)
				resultSet.RemoveAll(a);
			return resultSet;
		}

		/// <summary>
		/// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
		/// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
		/// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
		/// of set <c>a</c> containing the elements from the operation. 
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>A set containing <c>A - B</c> elements.  <c>null</c> if <c>a</c> is <c>null</c>.</returns>
		public static ISet Minus(ISet a, ISet b)
		{
			if(a == null)
				return null;
			else
				return a.Minus(b);
		}

		/// <summary>
		/// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
		/// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
		/// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
		/// of set <c>a</c> containing the elements from the operation. 
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>A set containing <c>A - B</c> elements.  <c>null</c> if <c>a</c> is <c>null</c>.</returns>
		public static Set operator - (Set a, Set b)
		{
			return (Set)Minus(a, b);
		}


		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only the elements that
		/// are in one of the sets, but not in both.  The original sets are not modified
		/// during this operation.  The result set is a <c>Clone()</c> of this set containing
		/// the elements from the exclusive-or operation.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <returns>A set containing the result of <c>a ^ b</c>.</returns>
		public virtual ISet ExclusiveOr(ISet a)
		{
			ISet resultSet = (ISet)this.Clone();
			foreach(object element in a)
			{
				if(resultSet.Contains(element))
					resultSet.Remove(element);
				else
					resultSet.Add(element);
			}
			return resultSet;
		}

		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only the elements that
		/// are in one of the sets, but not in both.  The original sets are not modified
		/// during this operation.  The result set is a <c>Clone()</c> of one of the sets
		/// (<c>a</c> if it is not <c>null</c>) containing
		/// the elements from the exclusive-or operation.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>A set containing the result of <c>a ^ b</c>.  <c>null</c> if both sets are <c>null</c>.</returns>
		public static ISet ExclusiveOr(ISet a, ISet b)
		{
			if(a == null && b == null)
				return null;
			else if(a == null)
				return (Set)b.Clone();
			else if(b == null)
				return (Set)a.Clone();
			else
				return a.ExclusiveOr(b);
		}

		/// <summary>
		/// Performs an "exclusive-or" of the two sets, keeping only the elements that
		/// are in one of the sets, but not in both.  The original sets are not modified
		/// during this operation.  The result set is a <c>Clone()</c> of one of the sets
		/// (<c>a</c> if it is not <c>null</c>) containing
		/// the elements from the exclusive-or operation.
		/// </summary>
		/// <param name="a">A set of elements.</param>
		/// <param name="b">A set of elements.</param>
		/// <returns>A set containing the result of <c>a ^ b</c>.  <c>null</c> if both sets are <c>null</c>.</returns>
		public static Set operator ^ (Set a, Set b)
		{
			return (Set)ExclusiveOr(a, b);
		}

		/// <summary>
		/// Adds the specified element to this set if it is not already present.
		/// </summary>
		/// <param name="o">The object to add to the set.</param>
		/// <returns><c>true</c> is the object was added, <c>false</c> if it was already present.</returns>
		public abstract bool Add(object o);

		/// <summary>
		/// Adds all the elements in the specified collection to the set if they are not already present.
		/// </summary>
		/// <param name="c">A collection of objects to add to the set.</param>
		/// <returns><c>true</c> is the set changed as a result of this operation, <c>false</c> if not.</returns>
		public abstract bool AddAll(ICollection c);

		/// <summary>
		/// Removes all objects from the set.
		/// </summary>
		public abstract void Clear();

		/// <summary>
		/// Returns <c>true</c> if this set contains the specified element.
		/// </summary>
		/// <param name="o">The element to look for.</param>
		/// <returns><c>true</c> if this set contains the specified element, <c>false</c> otherwise.</returns>
		public abstract bool Contains(object o);

		/// <summary>
		/// Returns <c>true</c> if the set contains all the elements in the specified collection.
		/// </summary>
		/// <param name="c">A collection of objects.</param>
		/// <returns><c>true</c> if the set contains all the elements in the specified collection, <c>false</c> otherwise.</returns>
		public abstract bool ContainsAll(ICollection c);

		/// <summary>
		/// Removes the specified element from the set.
		/// </summary>
		/// <param name="o">The element to be removed.</param>
		/// <returns><c>true</c> if the set contained the specified element, <c>false</c> otherwise.</returns>
		public abstract bool Remove(object o);

		/// <summary>
		/// Remove all the specified elements from this set, if they exist in this set.
		/// </summary>
		/// <param name="c">A collection of elements to remove.</param>
		/// <returns><c>true</c> if the set was modified as a result of this operation.</returns>
		public abstract bool RemoveAll(ICollection c);


		/// <summary>
		/// Retains only the elements in this set that are contained in the specified collection.
		/// </summary>
		/// <param name="c">Collection that defines the set of elements to be retained.</param>
		/// <returns><c>true</c> if this set changed as a result of this operation.</returns>
		public abstract bool RetainAll(ICollection c);

		/// <summary>
		/// Copies the elements in the <c>Set</c> to an array.  The type of array needs
		/// to be compatible with the objects in the <c>Set</c>, obviously.
		/// </summary>
		/// <param name="array">An array that will be the target of the copy operation.</param>
		/// <param name="index">The zero-based index where copying will start.</param>
		public abstract void CopyTo(Array array, int index);
		
		/// <summary>
		/// Gets an enumerator for the elements in the <c>Set</c>.
		/// </summary>
		/// <returns>An <c>IEnumerator</c> over the elements in the <c>Set</c>.</returns>
		public abstract IEnumerator GetEnumerator();
		
		/// <summary>
		/// This method will test the <c>Set</c> against another <c>Set</c> for "equality".
		/// In this case, "equality" means that the two sets contain the same elements.
		/// The "==" and "!=" operators are not overridden by design.  If you wish to check
		/// for "equivalent" <c>Set</c> instances, use <c>Equals()</c>.  If you wish to check
		/// to see if two references are actually the same object, use "==" and "!=".  
		/// </summary>
		/// <param name="o">A <c>Set</c> object to compare to.</param>
		/// <returns></returns>
		public override bool Equals(object o)
		{
			if(o == null || !(o is Set) || ((Set)o).Count != this.Count)
				return false;
			else 
			{
				foreach(object elt in ((Set)o))
				{
					if(!this.Contains(elt))
						return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Gets the hashcode for the object.  Not implemented.
		/// </summary>
		/// <returns>An exception.</returns>
		/// <exception cref="NotImplementedException">This feature is not implemented.</exception>
		public override int GetHashCode()
		{
			throw new NotImplementedException();
		}
        
        #endregion

        #region Public Static Methods

        /// <overloads>
        /// Returns a set wrapper that is read-only.
        /// </overloads>
        /// <summary>
        /// Returns a read-only <see cref="Set"/> wrapper.
        /// </summary>
        /// <param name="inputSet">The <see cref="Set"/> to wrap.</param>
        /// <returns>A read-only <see cref="Set"/> wrapper around set.</returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="inputSet"/> is <see langword="null"/>.
        /// </exception>
        public static Set ReadOnly(Set inputSet)
        {
            if (inputSet == null)
            {
                throw new ArgumentNullException("inputSet");
            }

            return new ReadOnlySet(inputSet);
        }

        /// <summary>
        /// Returns a read-only <see cref="ISet"/> wrapper.
        /// </summary>
        /// <param name="inputSet">The <see cref="ISet"/> to wrap.</param>
        /// <returns>A read-only <see cref="ISet"/> wrapper around set.</returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="inputSet"/> is <see langword="null"/>.
        /// </exception>
        public static ISet ReadOnly(ISet inputSet)
        {
            if (inputSet == null)
            {
                throw new ArgumentNullException("inputSet");
            }

            return new ReadOnlySet(inputSet);
        }

        /// <overloads>
        /// Returns a set wrapper that is synchronized (thread-safe).
        /// </overloads>
        /// <summary>
        /// Returns an <see cref="Set"/> wrapper that is synchronized (thread-safe).
        /// </summary>
        /// <param name="inputSet">The <see cref="Set"/> to synchronize.</param>
        /// <returns>
        /// An <see cref="Set"/> wrapper that is synchronized (thread-safe).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="inputSet"/> is <see langword="null"/>.
        /// </exception>
        public static Set Synchronized(Set inputSet)
        {
            if (inputSet == null)
            {
                throw new ArgumentNullException("inputSet");
            }

            return new SynchronizedSet(inputSet);
        }

        /// <summary>
        /// Returns an <see cref="ISet"/> wrapper that is synchronized (thread-safe).
        /// </summary>
        /// <param name="inputSet">The <see cref="ISet"/> to synchronize.</param>
        /// <returns>
        /// An <see cref="ISet"/> wrapper that is synchronized (thread-safe).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="inputSet"/> is <see langword="null"/>.
        /// </exception>
        public static ISet Synchronized(ISet inputSet)
        {
            if (inputSet == null)
            {
                throw new ArgumentNullException("inputSet");
            }

            return new SynchronizedSet(inputSet);
        }

        #endregion

        #region ICloneable Members

        /// <summary>
        /// Returns a clone of the <c>Set</c> instance.  This will work for derived <c>Set</c>
        /// classes if the derived class implements a constructor that takes no arguments.
        /// </summary>
        /// <returns>A clone of this object.</returns>
        public virtual object Clone()
        {
            Set newSet = (Set)Activator.CreateInstance(this.GetType());
            newSet.AddAll(this);
            return newSet;
        }
        
        #endregion

        #region ReadOnlySet Class

        /// <summary>
        /// <p>Implements an immutable (read-only) <c>Set</c> wrapper.</p>
        /// <p>Although this is advertised as immutable, it really isn't.  Anyone with access to the
        /// <c>basisSet</c> can still change the data-set.  So <c>GetHashCode()</c> is not implemented
        /// for this <c>Set</c>, as is the case for all <c>Set</c> implementations in this library.
        /// This design decision was based on the efficiency of not having to <c>Clone()</c> the 
        /// <c>basisSet</c> every time you wrap a mutable <c>Set</c>.</p>
        /// </summary>
        private sealed class ReadOnlySet : Set
        {
            private const string ERROR_MESSAGE = "Object is immutable.";
            private ISet mBasisSet;

            internal ISet BasisSet
            {
                get
                {
                    return mBasisSet;
                }
            }

            /// <summary>
            /// Constructs an immutable (read-only) <c>Set</c> wrapper.
            /// </summary>
            /// <param name="basisSet">The <c>Set</c> that is wrapped.</param>
            public ReadOnlySet(ISet basisSet)
            {
                mBasisSet = basisSet;
            }

            /// <summary>
            /// Adds the specified element to this set if it is not already present.
            /// </summary>
            /// <param name="o">The object to add to the set.</param>
            /// <returns><c>true</c> is the object was added, <c>false</c> if it was already present.</returns>
            public sealed override bool Add(object o)
            {
                throw new NotSupportedException(ERROR_MESSAGE);
            }

            /// <summary>
            /// Adds all the elements in the specified collection to the set if they are not already present.
            /// </summary>
            /// <param name="c">A collection of objects to add to the set.</param>
            /// <returns><c>true</c> is the set changed as a result of this operation, <c>false</c> if not.</returns>
            public sealed override bool AddAll(ICollection c)
            {
                throw new NotSupportedException(ERROR_MESSAGE);
            }

            /// <summary>
            /// Removes all objects from the set.
            /// </summary>
            public sealed override void Clear()
            {
                throw new NotSupportedException(ERROR_MESSAGE);
            }

            /// <summary>
            /// Returns <c>true</c> if this set contains the specified element.
            /// </summary>
            /// <param name="o">The element to look for.</param>
            /// <returns><c>true</c> if this set contains the specified element, <c>false</c> otherwise.</returns>
            public sealed override bool Contains(object o)
            {
                return mBasisSet.Contains(o);
            }

            /// <summary>
            /// Returns <c>true</c> if the set contains all the elements in the specified collection.
            /// </summary>
            /// <param name="c">A collection of objects.</param>
            /// <returns><c>true</c> if the set contains all the elements in the specified collection, <c>false</c> otherwise.</returns>
            public sealed override bool ContainsAll(ICollection c)
            {
                return mBasisSet.ContainsAll(c);
            }

            /// <summary>
            /// Returns <c>true</c> if this set contains no elements.
            /// </summary>
            public sealed override bool IsEmpty
            {
                get{return mBasisSet.IsEmpty;}
            }


            /// <summary>
            /// Removes the specified element from the set.
            /// </summary>
            /// <param name="o">The element to be removed.</param>
            /// <returns><c>true</c> if the set contained the specified element, <c>false</c> otherwise.</returns>
            public sealed override bool Remove(object o)
            {
                throw new NotSupportedException(ERROR_MESSAGE);
            }

            /// <summary>
            /// Remove all the specified elements from this set, if they exist in this set.
            /// </summary>
            /// <param name="c">A collection of elements to remove.</param>
            /// <returns><c>true</c> if the set was modified as a result of this operation.</returns>
            public sealed override bool RemoveAll(ICollection c)
            {
                throw new NotSupportedException(ERROR_MESSAGE);
            }

            /// <summary>
            /// Retains only the elements in this set that are contained in the specified collection.
            /// </summary>
            /// <param name="c">Collection that defines the set of elements to be retained.</param>
            /// <returns><c>true</c> if this set changed as a result of this operation.</returns>
            public sealed override bool RetainAll(ICollection c)
            {
                throw new NotSupportedException(ERROR_MESSAGE);
            }

            /// <summary>
            /// Copies the elements in the <c>Set</c> to an array.  The type of array needs
            /// to be compatible with the objects in the <c>Set</c>, obviously.
            /// </summary>
            /// <param name="array">An array that will be the target of the copy operation.</param>
            /// <param name="index">The zero-based index where copying will start.</param>
            public sealed override void CopyTo(Array array, int index)
            {
                mBasisSet.CopyTo(array, index);
            }

            /// <summary>
            /// The number of elements contained in this collection.
            /// </summary>
            public sealed override int Count
            {
                get{return mBasisSet.Count;}
            }

            /// <summary>
            /// Returns an object that can be used to synchronize use of the <c>Set</c> across threads.
            /// </summary>
            public sealed override bool IsSynchronized
            {
                get{return mBasisSet.IsSynchronized;}
            }

            /// <summary>
            /// Returns an object that can be used to synchronize the <c>Set</c> between threads.
            /// </summary>
            public sealed override object SyncRoot
            {
                get{return mBasisSet.SyncRoot;}
            }

            /// <summary>
            /// Gets an enumerator for the elements in the <c>Set</c>.
            /// </summary>
            /// <returns>An <c>IEnumerator</c> over the elements in the <c>Set</c>.</returns>
            public sealed override IEnumerator GetEnumerator()
            {
                return mBasisSet.GetEnumerator();
            }

            /// <summary>
            /// Returns a clone of the <c>Set</c> instance.  
            /// </summary>
            /// <returns>A clone of this object.</returns>
            public sealed override object Clone()
            {
                return new ReadOnlySet(mBasisSet);
            }

            /// <summary>
            /// Performs a "union" of the two sets, where all the elements
            /// in both sets are present.  That is, the element is included if it is in either <c>a</c> or <c>b</c>.
            /// Neither this set nor the input set are modified during the operation.  The return value
            /// is a <c>Clone()</c> of this set with the extra elements added in.
            /// </summary>
            /// <param name="a">A collection of elements.</param>
            /// <returns>A new <c>Set</c> containing the union of this <c>Set</c> with the specified collection.
            /// Neither of the input objects is modified by the union.</returns>
            public sealed override ISet Union(ISet a)
            {
                ISet m = this;
                while(m is ReadOnlySet)
                    m = ((ReadOnlySet)m).BasisSet;
                return new ReadOnlySet(m.Union(a));
            }

            /// <summary>
            /// Performs an "intersection" of the two sets, where only the elements
            /// that are present in both sets remain.  That is, the element is included if it exists in
            /// both sets.  The <c>Intersect()</c> operation does not modify the input sets.  It returns
            /// a <c>Clone()</c> of this set with the appropriate elements removed.
            /// </summary>
            /// <param name="a">A set of elements.</param>
            /// <returns>The intersection of this set with <c>a</c>.</returns>
            public sealed override ISet Intersect(ISet a)
            {
                ISet m = this;
                while(m is ReadOnlySet)
                    m = ((ReadOnlySet)m).BasisSet;
                return new ReadOnlySet(m.Intersect(a));
            }

            /// <summary>
            /// Performs a "minus" of set <c>b</c> from set <c>a</c>.  This returns a set of all
            /// the elements in set <c>a</c>, removing the elements that are also in set <c>b</c>.
            /// The original sets are not modified during this operation.  The result set is a <c>Clone()</c>
            /// of this <c>Set</c> containing the elements from the operation.
            /// </summary>
            /// <param name="a">A set of elements.</param>
            /// <returns>A set containing the elements from this set with the elements in <c>a</c> removed.</returns>
            public sealed override ISet Minus(ISet a)
            {
                ISet m = this;
                while(m is ReadOnlySet)
                    m = ((ReadOnlySet)m).BasisSet;
                return new ReadOnlySet(m.Minus(a));
            }

            /// <summary>
            /// Performs an "exclusive-or" of the two sets, keeping only the elements that
            /// are in one of the sets, but not in both.  The original sets are not modified
            /// during this operation.  The result set is a <c>Clone()</c> of this set containing
            /// the elements from the exclusive-or operation.
            /// </summary>
            /// <param name="a">A set of elements.</param>
            /// <returns>A set containing the result of <c>a ^ b</c>.</returns>
            public sealed override ISet ExclusiveOr(ISet a)
            {
                ISet m = this;
                while(m is ReadOnlySet)
                    m = ((ReadOnlySet)m).BasisSet;
                return new ReadOnlySet(m.ExclusiveOr(a));
            }
        }
    
        #endregion

        #region SynchronizedSet Class

        /// <summary>
        /// <p>Implements a thread-safe <c>Set</c> wrapper.  The implementation is extremely conservative, 
        /// serializing critical sections to prevent possible deadlocks, and locking on everything.
        /// The one exception is for enumeration, which is inherently not thread-safe.  For this, you
        /// have to <c>lock</c> the <c>SyncRoot</c> object for the duration of the enumeration.</p>
        /// </summary>
        private sealed class SynchronizedSet : Set
        {
            private ISet   mBasisSet;
            private object mSyncRoot;

            /// <summary>
            /// Constructs a thread-safe <c>Set</c> wrapper.
            /// </summary>
            /// <param name="basisSet">The <c>Set</c> object that this object will wrap.</param>
            public SynchronizedSet(ISet basisSet)
            {
                mBasisSet = basisSet;
                mSyncRoot = basisSet.SyncRoot;
                if(mSyncRoot == null)
                    throw new NullReferenceException("The Set you specified returned a null SyncRoot.");
            }

            /// <summary>
            /// Adds the specified element to this set if it is not already present.
            /// </summary>
            /// <param name="o">The object to add to the set.</param>
            /// <returns><c>true</c> is the object was added, <c>false</c> if it was already present.</returns>
            public sealed override bool Add(object o)
            {
                lock(mSyncRoot)
                {
                    return mBasisSet.Add(o);
                }
            }

            /// <summary>
            /// Adds all the elements in the specified collection to the set if they are not already present.
            /// </summary>
            /// <param name="c">A collection of objects to add to the set.</param>
            /// <returns><c>true</c> is the set changed as a result of this operation, <c>false</c> if not.</returns>
            public sealed override bool AddAll(ICollection c)
            {
                Set temp;
                lock(c.SyncRoot)
                {
                    temp = new HybridSet(c);
                }

                lock(mSyncRoot)
                {
                    return mBasisSet.AddAll(temp);
                }
            }

            /// <summary>
            /// Removes all objects from the set.
            /// </summary>
            public sealed override void Clear()
            {
                lock(mSyncRoot)
                {
                    mBasisSet.Clear();
                }
            }

            /// <summary>
            /// Returns <c>true</c> if this set contains the specified element.
            /// </summary>
            /// <param name="o">The element to look for.</param>
            /// <returns><c>true</c> if this set contains the specified element, <c>false</c> otherwise.</returns>
            public sealed override bool Contains(object o)
            {
                lock(mSyncRoot)
                {
                    return mBasisSet.Contains(o);
                }
            }

            /// <summary>
            /// Returns <c>true</c> if the set contains all the elements in the specified collection.
            /// </summary>
            /// <param name="c">A collection of objects.</param>
            /// <returns><c>true</c> if the set contains all the elements in the specified collection, <c>false</c> otherwise.</returns>
            public sealed override bool ContainsAll(ICollection c)
            {
                Set temp;
                lock(c.SyncRoot)
                {
                    temp = new HybridSet(c);
                }
                lock(mSyncRoot)
                {
                    return mBasisSet.ContainsAll(temp);
                }
            }

            /// <summary>
            /// Returns <c>true</c> if this set contains no elements.
            /// </summary>
            public sealed override bool IsEmpty
            {
                get
                {
                    lock(mSyncRoot)
                    {
                        return mBasisSet.IsEmpty;
                    }
                }
            }


            /// <summary>
            /// Removes the specified element from the set.
            /// </summary>
            /// <param name="o">The element to be removed.</param>
            /// <returns><c>true</c> if the set contained the specified element, <c>false</c> otherwise.</returns>
            public sealed override bool Remove(object o)
            {
                lock(mSyncRoot)
                {
                    return mBasisSet.Remove(o);
                }
            }

            /// <summary>
            /// Remove all the specified elements from this set, if they exist in this set.
            /// </summary>
            /// <param name="c">A collection of elements to remove.</param>
            /// <returns><c>true</c> if the set was modified as a result of this operation.</returns>
            public sealed override bool RemoveAll(ICollection c)
            {
                Set temp;
                lock(c.SyncRoot)
                {
                    temp = new HybridSet(c);
                }
                lock(mSyncRoot)
                {
                    return mBasisSet.RemoveAll(temp);
                }
            }

            /// <summary>
            /// Retains only the elements in this set that are contained in the specified collection.
            /// </summary>
            /// <param name="c">Collection that defines the set of elements to be retained.</param>
            /// <returns><c>true</c> if this set changed as a result of this operation.</returns>
            public sealed override bool RetainAll(ICollection c)
            {
                Set temp;
                lock(c.SyncRoot)
                {
                    temp = new HybridSet(c);
                }
                lock(mSyncRoot)
                {
                    return mBasisSet.RetainAll(temp);
                }
            }

            /// <summary>
            /// Copies the elements in the <c>Set</c> to an array.  The type of array needs
            /// to be compatible with the objects in the <c>Set</c>, obviously.
            /// </summary>
            /// <param name="array">An array that will be the target of the copy operation.</param>
            /// <param name="index">The zero-based index where copying will start.</param>
            public sealed override void CopyTo(Array array, int index)
            {
                lock(mSyncRoot)
                {
                    mBasisSet.CopyTo(array, index);
                }
            }

            /// <summary>
            /// The number of elements contained in this collection.
            /// </summary>
            public sealed override int Count
            {
                get
                {
                    lock(mSyncRoot)
                    {
                        return mBasisSet.Count;
                    }
                }
            }

            /// <summary>
            /// Returns <c>true</c>, indicating that this object is thread-safe.  The exception to this
            /// is enumeration, which is inherently not thread-safe.  Use the <c>SyncRoot</c> object to
            /// lock this object for the entire duration of the enumeration.
            /// </summary>
            public sealed override bool IsSynchronized
            {
                get{return true;}
            }

            /// <summary>
            /// Returns an object that can be used to synchronize the <c>Set</c> between threads.
            /// </summary>
            public sealed override object SyncRoot
            {
                get{return mSyncRoot;}
            }

            /// <summary>
            /// Enumeration is, by definition, not thread-safe.  Use a <c>lock</c> on the <c>SyncRoot</c> 
            /// to synchronize the entire enumeration process.
            /// </summary>
            /// <returns></returns>
            public sealed override IEnumerator GetEnumerator()
            {
                return mBasisSet.GetEnumerator();
            }

            /// <summary>
            /// Returns a clone of the <c>Set</c> instance.  
            /// </summary>
            /// <returns>A clone of this object.</returns>
            public override object Clone()
            {
                return new SynchronizedSet((ISet)mBasisSet.Clone());
            }

        }
    
        #endregion
	}
}
