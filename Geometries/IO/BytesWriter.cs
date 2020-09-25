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
	/// Summary description for BytesWriter.
	/// </summary>
    [Serializable]
    public class BytesWriter
	{
        #region Private Fields

        private BytesBuffer m_objBuffer;
        private byte[]      m_objData;
        private BytesOrder  m_enumOrder;
        private bool        m_bIsLittle;

        #endregion

        #region Constructors and Destructor
		
        public BytesWriter()
		{
            m_objBuffer = new BytesBuffer();
            m_objData   = new byte[16];
            m_enumOrder = BytesOrder.LittleEndian;
            m_bIsLittle = (m_enumOrder == BytesOrder.LittleEndian);
        }
        
        #endregion

        #region Public Properties

        public int Length
        {
            get
            {
                if (m_objBuffer != null)
                {
                    return m_objBuffer.Length;
                }

                return 0;
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
            if (m_objBuffer != null)
            {
                return m_objBuffer.ToArray();
            }

            return null;
        }

        public void Initialize()
        {
            if (m_objBuffer == null)
            {
                m_objBuffer = new BytesBuffer();
            }

            m_objBuffer.Initialize(0);
        }

        public void Initialize(int capacity)
        {
            if (m_objBuffer == null)
            {
                m_objBuffer = new BytesBuffer();
            }

            m_objBuffer.Initialize(capacity);
        }

        public void Initialize(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException ("buffer");
            
            if (m_objBuffer == null)
            {
                m_objBuffer = new BytesBuffer();
            }

            m_objBuffer.Initialize(buffer, 0, buffer.Length, true);
        }

        public void Initialize(byte[] buffer, int offset)
        {
            if (buffer == null)
                throw new ArgumentNullException ("buffer");
            
            if (m_objBuffer == null)
            {
                m_objBuffer = new BytesBuffer();
            }

            m_objBuffer.Initialize(buffer, offset, buffer.Length, true);
        }

        public void Uninitialize()
        {
            if (m_objBuffer != null)
            {
                m_objBuffer.Close();
            }
        }

        /// <summary>
        /// Writes the given byte to the given buffer at the given location.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <returns>
        /// The number of bytes written, this will always be 1.
        /// </returns>
        public int WriteByte(byte value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = value;

                m_objBuffer.Write(m_objData, 0, 1);

                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Writes the given byte to the given buffer at the given location.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <returns>
        /// The number of bytes written, this will always be 1.
        /// </returns>
        public int WriteByte(int offset, byte value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = value;

                m_objBuffer.Write(m_objData, offset, 1);

                return 1;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given short to the given buffer at the given location
        /// in little or big endian format.
        /// </summary>
        /// <param name="value">The short to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt16(short value)
        {
            if (m_objBuffer != null)
            {
                if (m_bIsLittle)
                {
                    m_objData[0] = (byte)((value) & 0xff);
                    m_objData[1] = (byte)((value >> 8) & 0xff);
                }
                else
                {
                    m_objData[0] = (byte)((value >> 8) & 0xff);
                    m_objData[1] = (byte)((value) & 0xff); 
                }

                m_objBuffer.Write(m_objData, 0, 2);
			
                return 2;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given short to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="value">The short to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt16LE(short value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value) & 0xff);
                m_objData[1] = (byte)((value >> 8) & 0xff);

                m_objBuffer.Write(m_objData, 0, 2);
			
                return 2;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given short to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The short to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt16LE(int offset, short value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value) & 0xff);
                m_objData[1] = (byte)((value >> 8) & 0xff);

                m_objBuffer.Write(m_objData, offset, 2);
			
                return 2;
            }

            return 0;
        }
 		
        /// <summary>
        /// Writes the given short to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="value">The short to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt16BE(short value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value >> 8) & 0xff);
                m_objData[1] = (byte)((value) & 0xff); 

                m_objBuffer.Write(m_objData, 0, 2);
			
                return 2;
            }

            return 0;
        }
 		
        /// <summary>
        /// Writes the given short to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The short to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt16BE(int offset, short value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value >> 8) & 0xff);
                m_objData[1] = (byte)((value) & 0xff); 

                m_objBuffer.Write(m_objData, offset, 2);
			
                return 2;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given integer to the given buffer at the given location
        /// in little or big endian format.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt32(int value)
        {
            if (m_objBuffer != null)
            {
                if (m_bIsLittle)
                {
                    m_objData[0] = (byte)((value) & 0xff);
                    m_objData[1] = (byte)((value >> 8)  & 0xff);
                    m_objData[2] = (byte)((value >> 16) & 0xff);
                    m_objData[3] = (byte)((value >> 24) & 0xff);
                }
                else
                {
                    m_objData[0] = (byte)((value >> 24) & 0xff);
                    m_objData[1] = (byte)((value >> 16) & 0xff);
                    m_objData[2] = (byte)((value >> 8)  & 0xff);
                    m_objData[3] = (byte)((value) & 0xff);
                }

                m_objBuffer.Write(m_objData, 0, 4);
			
                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given integer to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt32LE(int value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value) & 0xff);
                m_objData[1] = (byte)((value >> 8)  & 0xff);
                m_objData[2] = (byte)((value >> 16) & 0xff);
                m_objData[3] = (byte)((value >> 24) & 0xff);

                m_objBuffer.Write(m_objData, 0, 4);
			
                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given integer to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt32LE(int offset, int value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value) & 0xff);
                m_objData[1] = (byte)((value >> 8)  & 0xff);
                m_objData[2] = (byte)((value >> 16) & 0xff);
                m_objData[3] = (byte)((value >> 24) & 0xff);

                m_objBuffer.Write(m_objData, offset, 4);
			
                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given integer to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt32BE(int value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value >> 24) & 0xff);
                m_objData[1] = (byte)((value >> 16) & 0xff);
                m_objData[2] = (byte)((value >> 8)  & 0xff);
                m_objData[3] = (byte)((value) & 0xff);

                m_objBuffer.Write(m_objData, 0, 4);
			
                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given integer to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The integer to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt32BE(int offset, int value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value >> 24) & 0xff);
                m_objData[1] = (byte)((value >> 16) & 0xff);
                m_objData[2] = (byte)((value >> 8)  & 0xff);
                m_objData[3] = (byte)((value) & 0xff);

                m_objBuffer.Write(m_objData, offset, 4);
			
                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given long to the given buffer at the given location
        /// in little or big endian format.
        /// </summary>
        /// <param name="value">The long to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt64(long value)
        {
            if (m_objBuffer != null)
            {
                if (m_bIsLittle)
                {
                    m_objData[0] = (byte)((value) & 0xff);
                    m_objData[1] = (byte)((value >> 8)  & 0xff);
                    m_objData[2] = (byte)((value >> 16) & 0xff);
                    m_objData[3] = (byte)((value >> 24) & 0xff);
                    m_objData[4] = (byte)((value >> 32) & 0xff);
                    m_objData[5] = (byte)((value >> 40) & 0xff);
                    m_objData[6] = (byte)((value >> 48) & 0xff);
                    m_objData[7] = (byte)((value >> 56) & 0xff);
                }
                else
                {
                    m_objData[0] = (byte)((value >> 56) & 0xff);
                    m_objData[1] = (byte)((value >> 48) & 0xff);
                    m_objData[2] = (byte)((value >> 40) & 0xff);
                    m_objData[3] = (byte)((value >> 32) & 0xff);
                    m_objData[4] = (byte)((value >> 24) & 0xff);
                    m_objData[5] = (byte)((value >> 16) & 0xff);
                    m_objData[6] = (byte)((value >> 8)  & 0xff);
                    m_objData[7] = (byte)((value) & 0xff);
                }

                m_objBuffer.Write(m_objData, 0, 8);
			
                return 8;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given long to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="value">The long to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt64LE(long value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value) & 0xff);
                m_objData[1] = (byte)((value >> 8)  & 0xff);
                m_objData[2] = (byte)((value >> 16) & 0xff);
                m_objData[3] = (byte)((value >> 24) & 0xff);
                m_objData[4] = (byte)((value >> 32) & 0xff);
                m_objData[5] = (byte)((value >> 40) & 0xff);
                m_objData[6] = (byte)((value >> 48) & 0xff);
                m_objData[7] = (byte)((value >> 56) & 0xff);

                m_objBuffer.Write(m_objData, 0, 8);
			
                return 8;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given long to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur
        /// </param>
        /// <param name="value">The long to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt64LE(int offset, long value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value) & 0xff);
                m_objData[1] = (byte)((value >> 8)  & 0xff);
                m_objData[2] = (byte)((value >> 16) & 0xff);
                m_objData[3] = (byte)((value >> 24) & 0xff);
                m_objData[4] = (byte)((value >> 32) & 0xff);
                m_objData[5] = (byte)((value >> 40) & 0xff);
                m_objData[6] = (byte)((value >> 48) & 0xff);
                m_objData[7] = (byte)((value >> 56) & 0xff);

                m_objBuffer.Write(m_objData, offset, 8);
			
                return 8;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given long to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="value">The long to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt64BE(long value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value >> 56) & 0xff);
                m_objData[1] = (byte)((value >> 48) & 0xff);
                m_objData[2] = (byte)((value >> 40) & 0xff);
                m_objData[3] = (byte)((value >> 32) & 0xff);
                m_objData[4] = (byte)((value >> 24) & 0xff);
                m_objData[5] = (byte)((value >> 16) & 0xff);
                m_objData[6] = (byte)((value >> 8)  & 0xff);
                m_objData[7] = (byte)((value) & 0xff);

                m_objBuffer.Write(m_objData, 0, 8);
			
                return 8;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given long to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The long to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteInt64BE(int offset, long value)
        {
            if (m_objBuffer != null)
            {
                m_objData[0] = (byte)((value >> 56) & 0xff);
                m_objData[1] = (byte)((value >> 48) & 0xff);
                m_objData[2] = (byte)((value >> 40) & 0xff);
                m_objData[3] = (byte)((value >> 32) & 0xff);
                m_objData[4] = (byte)((value >> 24) & 0xff);
                m_objData[5] = (byte)((value >> 16) & 0xff);
                m_objData[6] = (byte)((value >> 8)  & 0xff);
                m_objData[7] = (byte)((value) & 0xff);

                m_objBuffer.Write(m_objData, offset, 8);
			
                return 8;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in little or big endian format.
        /// </summary>
        /// <param name="value">The float to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteSingle(float value)
        {      
            if (m_objBuffer != null)
            {
                int bits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

                if (m_bIsLittle)
                {
                    m_objData[0] = (byte)bits;
                    m_objData[1] = (byte)(bits >> 8);
                    m_objData[2] = (byte)(bits >> 16);
                    m_objData[3] = (byte)(bits >> 24);
                }
                else
                {
                    m_objData[3] = (byte)bits;
                    m_objData[2] = (byte)(bits >> 8);
                    m_objData[1] = (byte)(bits >> 16);
                    m_objData[0] = (byte)(bits >> 24);
                }
 
                m_objBuffer.Write(m_objData, 0, 4);

                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="value">The float to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteSingleLE(float value)
        {      
            if (m_objBuffer != null)
            {
                int bits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

                m_objData[0] = (byte)bits;
                m_objData[1] = (byte)(bits >> 8);
                m_objData[2] = (byte)(bits >> 16);
                m_objData[3] = (byte)(bits >> 24);
 
                m_objBuffer.Write(m_objData, 0, 4);

                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The float to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteSingleLE(int offset, float value)
        {      
            if (m_objBuffer != null)
            {
                int bits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

                m_objData[0] = (byte)bits;
                m_objData[1] = (byte)(bits >> 8);
                m_objData[2] = (byte)(bits >> 16);
                m_objData[3] = (byte)(bits >> 24);
 
                m_objBuffer.Write(m_objData, offset, 4);

                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="value">The float to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteSingleBE(float value)
        {
            if (m_objBuffer != null)
            {
                int bits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

                m_objData[3] = (byte)bits;
                m_objData[2] = (byte)(bits >> 8);
                m_objData[1] = (byte)(bits >> 16);
                m_objData[0] = (byte)(bits >> 24);
 
                m_objBuffer.Write(m_objData, 0, 4);

                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.</param>
        /// <param name="value">The float to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteSingleBE(int offset, float value)
        {
            if (m_objBuffer != null)
            {
                int bits = BitConverter.ToInt32(BitConverter.GetBytes(value), 0);

                m_objData[3] = (byte)bits;
                m_objData[2] = (byte)(bits >> 8);
                m_objData[1] = (byte)(bits >> 16);
                m_objData[0] = (byte)(bits >> 24);
 
                m_objBuffer.Write(m_objData, offset, 4);

                return 4;
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in little or big endian format.
        /// </summary>
        /// <param name="value">The double to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteDouble(double value)
        {      
            if (m_objBuffer != null)
            {
                if (m_bIsLittle)
                {
                    return WriteInt64LE(BitConverter.DoubleToInt64Bits(value));
                }
                else
                {
                    return WriteInt64BE(BitConverter.DoubleToInt64Bits(value));
                }
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="value">The double to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteDoubleLE(double value)
        {      
            if (m_objBuffer != null)
            {
                return WriteInt64LE(BitConverter.DoubleToInt64Bits(value));
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in little endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.
        /// </param>
        /// <param name="value">The double to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteDoubleLE(int offset, double value)
        {      
            if (m_objBuffer != null)
            {
                return WriteInt64LE(offset, 
                    BitConverter.DoubleToInt64Bits(value));
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="value">The double to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteDoubleBE(double value)
        {
            if (m_objBuffer != null)
            {
                return WriteInt64BE(BitConverter.DoubleToInt64Bits(value));
            }

            return 0;
        }
		
        /// <summary>
        /// Writes the given double to the given buffer at the given location
        /// in big endian format.
        /// </summary>
        /// <param name="offset">
        /// The offset into the buffer where writing should occur.</param>
        /// <param name="value">The double to write.</param>
        /// <returns>The number of bytes written.</returns>
        public int WriteDoubleBE(int offset, double value)
        {
            if (m_objBuffer != null)
            {
                return WriteInt64BE(offset, 
                    BitConverter.DoubleToInt64Bits(value));
            }

            return 0;
        }

        #endregion
    }
}
