using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for CoordinateListFactory.
	/// </summary>
	public class CoordinateListFactory
	{
		public CoordinateListFactory()
		{
		}

        [Serializable]
        private class SingleCoordinateList : ICoordinateList
        {
            #region Private Fields

            private Coordinate m_objCoordinate;
            
            #endregion

            #region Constructors and Destructor

            public SingleCoordinateList(Coordinate coordinate)
            {
                m_objCoordinate = coordinate;
            }
        
            #endregion

            #region ICoordinateList Members

            public bool IsFixedSize
            {
                get
                {
                    return true;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public bool IsPacked
            {
                get
                {
                    return false;
                }
            }

            public Coordinate this[int index]
            {
                get
                {
                    return m_objCoordinate;
                }

                set
                {
                    m_objCoordinate = value;
                }
            }

            public Coordinate this[int index, Coordinate inout]
            {
                get
                {
                    return m_objCoordinate;
                }

                set
                {
                    m_objCoordinate = value;
                }
            }

            public int Add(Coordinate value)
            {
                // TODO:  Add SingleCoordinateList.Add implementation
                return 0;
            }

            public void Clear()
            {
                // TODO:  Add SingleCoordinateList.Clear implementation
            }

            public bool Contains(Coordinate value)
            {
                // TODO:  Add SingleCoordinateList.Contains implementation
                return false;
            }

            public int IndexOf(Coordinate value)
            {
                // TODO:  Add SingleCoordinateList.IndexOf implementation
                return 0;
            }

            public void Insert(int index, Coordinate value)
            {
                // TODO:  Add SingleCoordinateList.Insert implementation
            }

            public void Remove(Coordinate value)
            {
                // TODO:  Add SingleCoordinateList.Remove implementation
            }

            public void RemoveAt(int index)
            {
                // TODO:  Add SingleCoordinateList.RemoveAt implementation
            }

            public Coordinate[] ToArray()
            {
                // TODO:  Add SingleCoordinateList.ToArray implementation
                return null;
            }

            public void Reverse()
            {
                // TODO:  Add SingleCoordinateList.Reverse implementation
            }

            public int AddRange(ICoordinateList x)
            {
                // TODO:  Add SingleCoordinateList.AddRange implementation
                return 0;
            }

            public int AddRange(Coordinate[] x)
            {
                // TODO:  Add SingleCoordinateList.AddRange implementation
                return 0;
            }

            public void CopyTo(Coordinate[] array)
            {
                // TODO:  Add SingleCoordinateList.CopyTo implementation
            }

            public bool Add(ICoordinateList coord, bool allowRepeated, bool direction)
            {
                // TODO:  Add SingleCoordinateList.Add implementation
                return false;
            }

            public bool Add(Coordinate[] coord, bool allowRepeated, bool direction)
            {
                // TODO:  Add SingleCoordinateList.Add implementation
                return false;
            }

            public bool Add(ICoordinateList coord, bool allowRepeated)
            {
                // TODO:  Add SingleCoordinateList.Add implementation
                return false;
            }

            public bool Add(Coordinate[] coord, bool allowRepeated)
            {
                // TODO:  Add SingleCoordinateList.Add implementation
                return false;
            }

            public void Add(Coordinate coord, bool allowRepeated)
            {
                // TODO:  Add SingleCoordinateList.Add implementation
            }

            public void MakePrecise(PrecisionModel precision)
            {
                if (precision != null && m_objCoordinate != null)
                {
                    m_objCoordinate.MakePrecise(precision);
                }
            }

            #endregion

            #region ICoordinateCollection Members

            public int Count
            {
                get
                {
                    return 1;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    // TODO:  Add SingleCoordinateList.IsSynchronized getter implementation
                    return false;
                }
            }

            public object SyncRoot
            {
                get
                {
                    // TODO:  Add SingleCoordinateList.SyncRoot getter implementation
                    return null;
                }
            }

            public void CopyTo(Coordinate[] array, int arrayIndex)
            {
                // TODO:  Add SingleCoordinateList.iGeospatial.Coordinates.ICoordinateCollection.CopyTo implementation
            }

            public ICoordinateEnumerator GetEnumerator()
            {
                // TODO:  Add SingleCoordinateList.GetEnumerator implementation
                return null;
            }

            #endregion

            #region ICloneable Members

            public ICoordinateList Clone()
            {
                // TODO:  Add SingleCoordinateList.Clone implementation
                return null;
            }

            object System.ICloneable.Clone()
            {
                // TODO:  Add SingleCoordinateList.System.ICloneable.Clone implementation
                return null;
            }

            #endregion
        }

        [Serializable]
        private class FixedCoordinateList : ICoordinateList
        {
            #region Constructors and Destructor

            public FixedCoordinateList()
            {
            } 

            #endregion
  
            #region ICoordinateList Members

            public bool IsFixedSize
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.IsFixedSize getter implementation
                    return false;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.IsReadOnly getter implementation
                    return false;
                }
            }

            public bool IsPacked
            {
                get
                {
                    return false;
                }
            }

            public Coordinate this[int index]
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.this getter implementation
                    return null;
                }
                set
                {
                    // TODO:  Add FixedCoordinateList.this setter implementation
                }
            }

            public Coordinate this[int index, Coordinate inout]
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.this getter implementation
                    return null;
                }
                set
                {
                    // TODO:  Add FixedCoordinateList.this setter implementation
                }
            }

            public int Add(Coordinate value)
            {
                // TODO:  Add FixedCoordinateList.Add implementation
                return 0;
            }

            public void Clear()
            {
                // TODO:  Add FixedCoordinateList.Clear implementation
            }

            public bool Contains(Coordinate value)
            {
                // TODO:  Add FixedCoordinateList.Contains implementation
                return false;
            }

            public int IndexOf(Coordinate value)
            {
                // TODO:  Add FixedCoordinateList.IndexOf implementation
                return 0;
            }

            public void Insert(int index, Coordinate value)
            {
                // TODO:  Add FixedCoordinateList.Insert implementation
            }

            public void Remove(Coordinate value)
            {
                // TODO:  Add FixedCoordinateList.Remove implementation
            }

            public void RemoveAt(int index)
            {
                // TODO:  Add FixedCoordinateList.RemoveAt implementation
            }

            public Coordinate[] ToArray()
            {
                // TODO:  Add FixedCoordinateList.ToArray implementation
                return null;
            }

            public void Reverse()
            {
                // TODO:  Add FixedCoordinateList.Reverse implementation
            }

            public int AddRange(ICoordinateList x)
            {
                // TODO:  Add FixedCoordinateList.AddRange implementation
                return 0;
            }

            public int AddRange(Coordinate[] x)
            {
                // TODO:  Add FixedCoordinateList.AddRange implementation
                return 0;
            }

            public void CopyTo(Coordinate[] array)
            {
                // TODO:  Add FixedCoordinateList.CopyTo implementation
            }

            public bool Add(ICoordinateList coord, bool allowRepeated, bool direction)
            {
                // TODO:  Add FixedCoordinateList.Add implementation
                return false;
            }

            public bool Add(Coordinate[] coord, bool allowRepeated, bool direction)
            {
                // TODO:  Add FixedCoordinateList.Add implementation
                return false;
            }

            public bool Add(ICoordinateList coord, bool allowRepeated)
            {
                // TODO:  Add FixedCoordinateList.Add implementation
                return false;
            }

            public bool Add(Coordinate[] coord, bool allowRepeated)
            {
                // TODO:  Add FixedCoordinateList.Add implementation
                return false;
            }

            public void Add(Coordinate coord, bool allowRepeated)
            {
                // TODO:  Add FixedCoordinateList.Add implementation
            }

            public void MakePrecise(PrecisionModel precision)
            {
                // TODO:  Add FixedCoordinateList.MakePrecise implementation
            }

            #endregion

            #region ICoordinateCollection Members

            public int Count
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.Count getter implementation
                    return 0;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.IsSynchronized getter implementation
                    return false;
                }
            }

            public object SyncRoot
            {
                get
                {
                    // TODO:  Add FixedCoordinateList.SyncRoot getter implementation
                    return null;
                }
            }

            public void CopyTo(Coordinate[] array, int arrayIndex)
            {
                // TODO:  Add FixedCoordinateList.iGeospatial.Coordinates.ICoordinateCollection.CopyTo implementation
            }

            public ICoordinateEnumerator GetEnumerator()
            {
                // TODO:  Add FixedCoordinateList.GetEnumerator implementation
                return null;
            }

            #endregion

            #region ICloneable Members

            public ICoordinateList Clone()
            {
                // TODO:  Add FixedCoordinateList.Clone implementation
                return null;
            }

            object System.ICloneable.Clone()
            {
                // TODO:  Add FixedCoordinateList.System.ICloneable.Clone implementation
                return null;
            }

            #endregion
        }   
	}
}
