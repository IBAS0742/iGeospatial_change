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
	/// <summary>
	/// Summary description for BytesReader.
	/// </summary>
	[Serializable]
    public class BytesReader
	{
        #region Private Fields

        private byte[]     m_objBuffer;
        private int        m_nOffset;
        private BytesOrder m_enumOrder;
        private bool       m_bIsLittle;

        #endregion

        #region Constructors and Destructor
		
        public BytesReader()
		{
            m_enumOrder = BytesOrder.LittleEndian;
            m_bIsLittle = (m_enumOrder == BytesOrder.LittleEndian);
		}
		
        public BytesReader(byte[] buffer)
        {
            m_objBuffer = buffer;
            m_enumOrder = BytesOrder.LittleEndian;
            m_bIsLittle = (m_enumOrder == BytesOrder.LittleEndian);
        }
		
        public BytesReader(byte[] buffer, int offset)
        {
            m_objBuffer = buffer;
            m_nOffset   = offset;
            m_enumOrder = BytesOrder.LittleEndian;
            m_bIsLittle = (m_enumOrder == BytesOrder.LittleEndian);
        }
        
        #endregion

        #region Public Properties

        public int Position
        {
            get
            {
                return m_nOffset;
            }

            set
            {
                if (value >= 0)
                {
                    m_nOffset = value;
                }
            }
        }

        public BytesOrder Order
        {
            get
            {
                return m_enumOrder;
            }

            set
            {
                m_enumOrder = value;
                m_bIsLittle = (m_enumOrder == BytesOrder.LittleEndian);
            }
        }

        #endregion

        #region Public Methods

        public byte[] GetBuffer()
        {
            return m_objBuffer;
        }

        public void Initialize(byte[] buffer)
        {
            m_objBuffer = buffer;
            m_nOffset   = 0;
        }

        public void Initialize(byte[] buffer, int offset)
        {
            m_objBuffer = buffer;
            m_nOffset   = offset;
        }

        public void Uninitialize()
        {
            m_objBuffer = null;
            m_nOffset   = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            int offset  = m_nOffset;
            m_nOffset  += 1;

            return m_objBuffer[offset];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte ReadByte(int offset)
        {
            return m_objBuffer[offset];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool ReadBoolean() 
        {
            return ReadByte() != 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public bool ReadBoolean(int offset) 
        {
            return ReadByte(offset) != 0;
        }
		
        /// <summary>
        /// Reads a little or big endian small integer.
        /// </summary>
        /// <returns> 
        /// The short read from the buffer at the offset location
        /// </returns>
        public short ReadInt16()
        {
            int offset  = m_nOffset;
            m_nOffset  += 2;

            if (m_bIsLittle)
            {
                return Convert.ToInt16((((m_objBuffer[offset + 1] & 0xff) << 8) | 
                    ((m_objBuffer[offset + 0] & 0xff))));
            }
            else
            {
                return Convert.ToInt16((((m_objBuffer[offset + 0] & 0xff) << 8) | 
                    ((m_objBuffer[offset + 1] & 0xff))));
            }
        }
		
        /// <summary>
        /// Reads a little endian small integer.
        /// </summary>
        /// <returns> 
        /// The short read from the buffer at the offset location
        /// </returns>
        public short ReadInt16LE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 2;

            return Convert.ToInt16((((m_objBuffer[offset + 1] & 0xff) << 8) | 
                ((m_objBuffer[offset + 0] & 0xff))));
        }
		
        /// <summary>
        /// Reads a little endian small integer.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the int resides.
        /// </param>
        /// <returns> 
        /// The short read from the buffer at the offset location.
        /// </returns>
        public short ReadInt16LE(int offset)
        {
            // just to keep FxCop happy!
            int nOffset = offset;

            return Convert.ToInt16((((m_objBuffer[nOffset + 1] & 0xff) << 8) | 
                ((m_objBuffer[nOffset + 0] & 0xff))));
        }

        /// <summary>
        /// Reads a big endian small integer.
        /// </summary>
        /// <returns> 
        /// The short read from the buffer at the offset location.
        /// </returns>
        public short ReadInt16BE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 2;

            return Convert.ToInt16((((m_objBuffer[offset + 0] & 0xff) << 8) | 
                ((m_objBuffer[offset + 1] & 0xff))));
        }

        /// <summary>
        /// Reads a big endian small integer.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the int resides.
        /// </param>
        /// <returns> 
        /// The int read from the buffer at the offset location.
        /// </returns>
        public short ReadInt16BE(int offset)
        {
            int nOffset = offset;

            return Convert.ToInt16((((m_objBuffer[nOffset + 0] & 0xff) << 8) | 
                ((m_objBuffer[nOffset + 1] & 0xff))));
        }
		
        /// <summary> 
        /// Reads a little or big endian integer.
        /// </summary>
        /// <returns> 
        /// The integer read from the buffer at the offset location.
        /// </returns>
        public int ReadInt32()
        {
            int offset  = m_nOffset;
            m_nOffset  += 4;

            if (m_bIsLittle)
            {
                return (((m_objBuffer[offset + 3] & 0xff) << 24) | 
                    ((m_objBuffer[offset + 2] & 0xff) << 16)     | 
                    ((m_objBuffer[offset + 1] & 0xff) << 8)      | 
                    ((m_objBuffer[offset + 0] & 0xff)));
            }
            else
            {
                return (((m_objBuffer[offset + 0] & 0xff) << 24) | 
                    ((m_objBuffer[offset + 1] & 0xff) << 16)     | 
                    ((m_objBuffer[offset + 2] & 0xff) << 8)      | 
                    ((m_objBuffer[offset + 3] & 0xff)));
            }
        }
		
        /// <summary> 
        /// Reads a little endian integer.
        /// </summary>
        /// <returns> 
        /// The integer read from the buffer at the offset location.
        /// </returns>
        public int ReadInt32LE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 4;

            return (((m_objBuffer[offset + 3] & 0xff) << 24) | 
                ((m_objBuffer[offset + 2] & 0xff) << 16)     | 
                ((m_objBuffer[offset + 1] & 0xff) << 8)      | 
                ((m_objBuffer[offset + 0] & 0xff)));
        }
		
        /// <summary> 
        /// Reads a little endian integer.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the int resides.
        /// </param>
        /// <returns> 
        /// The int read from the buffer at the offset location.
        /// </returns>
        public int ReadInt32LE(int offset)
        {
            int nOffset = offset;

            return (((m_objBuffer[nOffset + 3] & 0xff) << 24) | 
                ((m_objBuffer[nOffset + 2] & 0xff) << 16)     | 
                ((m_objBuffer[nOffset + 1] & 0xff) << 8)      | 
                ((m_objBuffer[nOffset + 0] & 0xff)));
        }
		
        /// <summary> 
        /// Reads a big endian integer.
        /// </summary>
        /// <returns> 
        /// The integer read from the buffer at the offset location.
        /// </returns>
        public int ReadInt32BE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 4;

            return (((m_objBuffer[offset + 0] & 0xff) << 24) | 
                ((m_objBuffer[offset + 1] & 0xff) << 16)     | 
                ((m_objBuffer[offset + 2] & 0xff) << 8)      | 
                ((m_objBuffer[offset + 3] & 0xff)));
        }
		
        /// <summary> 
        /// Reads a big endian integer.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the int resides.
        /// </param>
        /// <returns> 
        /// The int read from the buffer at the offset location.
        /// </returns>
        public int ReadInt32BE(int offset)
        {
            int nOffset = offset;

            return (((m_objBuffer[nOffset + 0] & 0xff) << 24) | 
                ((m_objBuffer[nOffset + 1] & 0xff) << 16)     | 
                ((m_objBuffer[nOffset + 2] & 0xff) << 8)      | 
                ((m_objBuffer[nOffset + 3] & 0xff)));
        }
		
        /// <summary>
        /// Reads a little or big endian 8 byte integer.
        /// </summary>
        /// <returns> 
        /// The long read from the buffer at the offset location.
        /// </returns>
        public long ReadInt64()
        {
            int offset  = m_nOffset;
            m_nOffset  += 8;

            if (m_bIsLittle)
            {
                return (((m_objBuffer[offset + 0] & 0xffL))   | 
                    ((m_objBuffer[offset + 1] & 0xffL) << 8)  | 
                    ((m_objBuffer[offset + 2] & 0xffL) << 16) | 
                    ((m_objBuffer[offset + 3] & 0xffL) << 24) | 
                    ((m_objBuffer[offset + 4] & 0xffL) << 32) | 
                    ((m_objBuffer[offset + 5] & 0xffL) << 40) | 
                    ((m_objBuffer[offset + 6] & 0xffL) << 48) | 
                    ((m_objBuffer[offset + 7] & 0xffL) << 56));
            }
            else
            {
                return (((m_objBuffer[offset + 7] & 0xffL))   | 
                    ((m_objBuffer[offset + 6] & 0xffL) << 8)  | 
                    ((m_objBuffer[offset + 5] & 0xffL) << 16) | 
                    ((m_objBuffer[offset + 4] & 0xffL) << 24) | 
                    ((m_objBuffer[offset + 3] & 0xffL) << 32) | 
                    ((m_objBuffer[offset + 2] & 0xffL) << 40) | 
                    ((m_objBuffer[offset + 1] & 0xffL) << 48) | 
                    ((m_objBuffer[offset + 0] & 0xffL) << 56));
            }
        }
		
        /// <summary>
        /// Reads a little endian 8 byte integer.
        /// </summary>
        /// <returns> 
        /// The long read from the buffer at the offset location.
        /// </returns>
        public long ReadInt64LE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 8;

            return (((m_objBuffer[offset + 0] & 0xffL))   | 
                ((m_objBuffer[offset + 1] & 0xffL) << 8)  | 
                ((m_objBuffer[offset + 2] & 0xffL) << 16) | 
                ((m_objBuffer[offset + 3] & 0xffL) << 24) | 
                ((m_objBuffer[offset + 4] & 0xffL) << 32) | 
                ((m_objBuffer[offset + 5] & 0xffL) << 40) | 
                ((m_objBuffer[offset + 6] & 0xffL) << 48) | 
                ((m_objBuffer[offset + 7] & 0xffL) << 56));
        }
		
        /// <summary>
        /// Reads a little endian 8 byte integer.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the long resides.
        /// </param>
        /// <returns> 
        /// The long read from the buffer at the offset location.
        /// </returns>
        public long ReadInt64LE(int offset)
        {
            int nOffset = offset;

            return (((m_objBuffer[nOffset + 0] & 0xffL))   | 
                ((m_objBuffer[nOffset + 1] & 0xffL) << 8)  | 
                ((m_objBuffer[nOffset + 2] & 0xffL) << 16) | 
                ((m_objBuffer[nOffset + 3] & 0xffL) << 24) | 
                ((m_objBuffer[nOffset + 4] & 0xffL) << 32) | 
                ((m_objBuffer[nOffset + 5] & 0xffL) << 40) | 
                ((m_objBuffer[nOffset + 6] & 0xffL) << 48) | 
                ((m_objBuffer[nOffset + 7] & 0xffL) << 56));
        }
		
        /// <summary>
        /// Reads a little endian 8 byte integer.
        /// </summary>
        /// <returns> 
        /// The long read from the buffer at the offset location.
        /// </returns>
        public long ReadInt64BE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 8;

            return (((m_objBuffer[offset + 7] & 0xffL))   | 
                ((m_objBuffer[offset + 6] & 0xffL) << 8)  | 
                ((m_objBuffer[offset + 5] & 0xffL) << 16) | 
                ((m_objBuffer[offset + 4] & 0xffL) << 24) | 
                ((m_objBuffer[offset + 3] & 0xffL) << 32) | 
                ((m_objBuffer[offset + 2] & 0xffL) << 40) | 
                ((m_objBuffer[offset + 1] & 0xffL) << 48) | 
                ((m_objBuffer[offset + 0] & 0xffL) << 56));
        }
		
        /// <summary>
        /// Reads a little endian 8 byte integer.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the long resides.
        /// </param>
        /// <returns> 
        /// The long read from the buffer at the offset location.
        /// </returns>
        public long ReadInt64BE(int offset)
        {
            int nOffset = offset;

            return (((m_objBuffer[nOffset + 7] & 0xffL))   | 
                ((m_objBuffer[nOffset + 6] & 0xffL) << 8)  | 
                ((m_objBuffer[nOffset + 5] & 0xffL) << 16) | 
                ((m_objBuffer[nOffset + 4] & 0xffL) << 24) | 
                ((m_objBuffer[nOffset + 3] & 0xffL) << 32) | 
                ((m_objBuffer[nOffset + 2] & 0xffL) << 40) | 
                ((m_objBuffer[nOffset + 1] & 0xffL) << 48) | 
                ((m_objBuffer[nOffset + 0] & 0xffL) << 56));
        }
		
        /// <summary> 
        /// Reads a little or big endian float.
        /// </summary>
        /// <returns> 
        /// The float read from the buffer at the offset location.
        /// </returns>
        public float ReadSingle()
        {
            int offset  = m_nOffset;
            m_nOffset  += 4;

            if (m_bIsLittle)
            {
                return BitConverter.ToSingle(BitConverter.GetBytes(
                    ReadInt32LE(offset)), 0);
            }
            else
            {
                return BitConverter.ToSingle(BitConverter.GetBytes(
                    ReadInt32BE(offset)), 0);
            }
        }
		
        /// <summary> 
        /// Reads a little endian float.
        /// </summary>
        /// <returns> 
        /// The float read from the buffer at the offset location.
        /// </returns>
        public float ReadSingleLE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 4;

            return BitConverter.ToSingle(BitConverter.GetBytes(
                ReadInt32LE(offset)), 0);
        }
		
        /// <summary> 
        /// Reads a little endian float.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the float resides.
        /// </param>
        /// <returns> 
        /// The float read from the buffer at the offset location.
        /// </returns>
        public float ReadSingleLE(int offset)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(
                ReadInt32LE(offset)), 0);
        }
		
        /// <summary> 
        /// Reads a big endian float.
        /// </summary>
        /// <returns> 
        /// The float read from the buffer at the offset location.
        /// </returns>
        public float ReadSingleBE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 4;

            return BitConverter.ToSingle(BitConverter.GetBytes(
                ReadInt32BE(offset)), 0);
        }
		
        /// <summary> 
        /// Reads a big endian float.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the float resides.
        /// </param>
        /// <returns> 
        /// The float read from the buffer at the offset location.
        /// </returns>
        public float ReadSingleBE(int offset)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(
                ReadInt32BE(offset)), 0);
        }
		
        /// <summary>
        /// Reads a little or big endian double.
        /// </summary>
        /// <returns> 
        /// The double read from the buffer at the offset location.
        /// </returns>
        public double ReadDouble()
        {
            int offset  = m_nOffset;
            m_nOffset  += 8;

            if (m_bIsLittle)
            {
                return BitConverter.Int64BitsToDouble(ReadInt64LE(offset));
            }
            else
            {
                return BitConverter.Int64BitsToDouble(ReadInt64BE(offset));
            }
        }
		
        /// <summary>
        /// Reads a little endian double.
        /// </summary>
        /// <returns> 
        /// The double read from the buffer at the offset location.
        /// </returns>
        public double ReadDoubleLE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 8;

            return BitConverter.Int64BitsToDouble(ReadInt64LE(offset));
        }
		
        /// <summary>
        /// Reads a little endian double.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the double resides.
        /// </param>
        /// <returns> 
        /// The double read from the buffer at the offset location.
        /// </returns>
        public double ReadDoubleLE(int offset)
        {
            return BitConverter.Int64BitsToDouble(ReadInt64LE(offset));
        }
		
        /// <summary>
        /// Reads a big endian double.
        /// </summary>
        /// <returns> 
        /// The double read from the buffer at the offset location.
        /// </returns>
        public double ReadDoubleBE()
        {
            int offset  = m_nOffset;
            m_nOffset  += 8;

            return BitConverter.Int64BitsToDouble(ReadInt64BE(offset));
        }
		
        /// <summary>
        /// Reads a big endian double.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where the double resides.
        /// </param>
        /// <returns> 
        /// The double read from the buffer at the offset location.
        /// </returns>
        public double ReadDoubleBE(int offset)
        {
            return BitConverter.Int64BitsToDouble(ReadInt64BE(offset));
        }
		
        #endregion
    }
}
