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

namespace iGeospatial.Geometries.IO
{
    [Serializable]
    public class BytesBuffer
    {
        #region Private Fields

        private byte[] m_objBuffer;
        private bool   m_bIsWritable;
        private int    m_nCapacity;
        private int    m_nLength;
        private int    m_nOrigin;
        private bool   m_bExpandable;
        private bool   m_bIsClosed;
        private int    m_nPosition;
        
        #endregion

        #region Constructors and Destructor

        public BytesBuffer() : this(0)
        {
        }

        public BytesBuffer(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("Capacity cannot be negative",
                    "capacity");

            m_bIsWritable = true;
            m_nCapacity   = capacity;
            m_objBuffer   = new byte[capacity];
            m_bExpandable = true;
        }

        public BytesBuffer(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException ("buffer");
            
            Initialize(buffer, 0, buffer.Length, true);                        
        }

        public BytesBuffer(byte[] buffer, bool writable)
        {
            if (buffer == null)
                throw new ArgumentNullException ("buffer");
            
            Initialize(buffer, 0, buffer.Length, writable);
        }

        public BytesBuffer(byte[] buffer, int index, int count)
        {
            Initialize(buffer, index, count, true);
        }

        public BytesBuffer(byte[] buffer, int index, int count, bool writable)
        {
            Initialize(buffer, index, count, writable);
        }
        
        #endregion
        
        #region Public Properties

        public bool CanRead 
        {
            get 
            { 
                return !m_bIsClosed; 
            }
        }

        public bool CanWrite 
        {
            get 
            { 
                return (!m_bIsClosed && m_bIsWritable); 
            }
        }

        public int Capacity 
        {
            get 
            {
                CheckValidity();

                return (m_nCapacity - m_nOrigin);
            }

            set 
            {
                CheckValidity();

                if (value == m_nCapacity)
                    return;

                if (!m_bExpandable)
                    throw new NotSupportedException("Cannot expand this BytesBuffer");

                if (value < 0 || value < m_nLength)
                {
                    throw new ArgumentException("The new capacity cannot be " 
                        + "negative or less than the current capacity " 
                        + m_nCapacity);
                }

                byte[] newBuffer = null;
                if (value != 0) 
                {
                    newBuffer = new byte[value];
                    Buffer.BlockCopy (m_objBuffer, 0, newBuffer, 0, m_nLength);
                }

                m_objBuffer = newBuffer;
                m_nCapacity = value;
            }
        }

        public int Length 
        {
            get 
            {
                CheckValidity();

                return m_nLength - m_nOrigin;
            }
        }

        public int Position 
        {
            get 
            {
                CheckValidity();

                return m_nPosition - m_nOrigin;
            }

            set 
            {
                CheckValidity();

                if (value < 0)
                    throw new ArgumentException("Position cannot be negative" );

                if (value == Int32.MaxValue)
                    throw new ArgumentException("Position must be less than 2^31 - 1");

                m_nPosition = m_nOrigin + value;
            }
        }
        
        #endregion

        #region Public Methods

        public void Initialize(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentException("The capacity cannot be negative", 
                    "capacity");

            Reset();

            m_bIsWritable = true;

            this.m_nCapacity = capacity;
            m_objBuffer      = new byte[capacity];

            m_bExpandable    = true;
            m_bIsClosed      = false;
        }

        public void Initialize(byte[] buffer, int index, int count, bool writable)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (index < 0)
                throw new ArgumentException("The index must be greater than zero.");

            if (count < 0)
                throw new ArgumentException("The count must be greater than zero.");

            if (buffer.Length - index < count)
                throw new ArgumentException("The size of the buffer must be " 
                    + "greater than index + count.");

            Reset();

            m_bIsWritable = writable;

            m_objBuffer   = buffer;
            m_nCapacity   = count + index;
            m_nLength     = m_nCapacity;
            m_nPosition   = index;
            m_nOrigin     = index;
            m_bIsClosed   = false;
            m_bExpandable = false;                
        }

        public void Close()
        {
            m_bIsClosed = true;
            m_bExpandable   = false;
        }

        public byte[] GetBuffer()
        {
            return m_objBuffer;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            CheckValidity();

            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (offset < 0)
                throw new ArgumentException("The offset must be greater than zero.");

            if (count < 0)
                throw new ArgumentException("The count must be greater than zero.");

            if ((buffer.Length - offset) < count)
                throw new ArgumentException("The size of the buffer must be " 
                    + "greater than offset + count.");

            if (m_nPosition >= m_nLength || count == 0)
                return 0;

            if (m_nPosition > (m_nLength - count))
                count = m_nLength - m_nPosition;

            Buffer.BlockCopy(m_objBuffer, m_nPosition, buffer, offset, count);

            m_nPosition += count;
            
            return count;
        }

        public int ReadByte()
        {
            CheckValidity ();
            if (m_nPosition >= m_nLength)
                return -1;

            return m_objBuffer[m_nPosition++];
        }

        public byte[] ToArray()
        {
            int count     = m_nLength - m_nOrigin;
            byte[] buffer = new byte[count];

            Buffer.BlockCopy(m_objBuffer, m_nOrigin, buffer, 0, count);

            return buffer; 
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            CheckValidity();

            if (!m_bIsWritable)
                throw new NotSupportedException("Cannot write to this buffer.");

            if (buffer == null)
                throw new ArgumentNullException("buffer");
            
            if (offset < 0)
                throw new ArgumentException("The offset must be greater than zero.");

            if (count < 0)
                throw new ArgumentException("The count must be greater than zero.");

            if ((buffer.Length - offset) < count)
                throw new ArgumentException("The size of the buffer must be " 
                    + "greater than offset + count.");

            if (m_nPosition > (m_nCapacity - count))
                EnsureCapacity(m_nPosition + count);

            Buffer.BlockCopy(buffer, offset, m_objBuffer, m_nPosition, count);
            
            m_nPosition += count;

            if (m_nPosition >= m_nLength)
            {
                m_nLength = m_nPosition;
            }
        }

        public void WriteByte(byte value)
        {
            CheckValidity();

            if (!m_bIsWritable)
                throw new NotSupportedException("Cannot write to this buffer.");

            if (m_nPosition >= m_nCapacity)
            {
                EnsureCapacity(m_nPosition + 1);
            }

            if (m_nPosition >= m_nLength)
            {
                m_nLength = m_nPosition + 1;
            }

            m_objBuffer[m_nPosition++] = value;
        }
        
        #endregion

        #region Private Methods
        
        private void CheckValidity()
        {
            if (m_bIsClosed)
            {
                throw new InvalidOperationException("BytesBuffer is not valid or is closed.");
            }
        }

        private void Reset()
        {
            m_objBuffer   = null;
            m_bIsWritable = false;
            m_nCapacity   = 0;
            m_nLength     = 0;
            m_nOrigin     = 0;
            m_bExpandable = false;
            m_bIsClosed   = false;
            m_nPosition   = 0;
        }

        private bool EnsureCapacity(int min)
        {
            if (min < 0)
            {
                throw new ArgumentException("The capacity cannot be negative", "min");
            }

            if (min > m_nCapacity)
            {
                if (min < 256)
                    min = 256;

                if (min < m_nCapacity * 2)
                    min = m_nCapacity * 2;

                this.Capacity = min;

                return true;
            }

            return false;
        }
        
        #endregion
    }               
}
