using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace iGeospatial.Texts
{
	/// <summary>
	/// Summary description for BackBinaryReader.
	/// </summary>
    /// <summary>
    /// Provides functionality to read and unread from a Stream.
    /// </summary>
    internal class BackBinaryReader : BinaryReader
    {
        private byte[] buffer;
        private int position = 1;

        /// <summary>
        /// Creates a BackBinaryReader with the specified stream and size for the buffer.
        /// </summary>
        /// <param name="streamReader">The stream to use.</param>
        /// <param name="size">The specific size of the buffer.</param>
        public BackBinaryReader(Stream streamReader, Int32 size) 
            : base(streamReader)
        {
            this.buffer = new byte[size];
            this.position = size;
        }

        /// <summary>
        /// Creates a BackBinaryReader with the specified stream.
        /// </summary>
        /// <param name="streamReader">The stream to use.</param>
        public BackBinaryReader(Stream streamReader) 
            : base(streamReader)
        {
            this.buffer = new byte[this.position];
        }

        /// <summary>
        /// Checks if this stream support mark and reset methods.
        /// </summary>
        /// <returns>Always false, these methods aren't supported.</returns>
        public bool MarkSupported()
        {	
            return false;
        }

        /// <summary>
        /// Reads the next bytes in the stream.
        /// </summary>
        /// <returns>The next byte readed</returns>
        public override int Read()
        {
            if (position >= 0 && position < buffer.Length)
                return (int) this.buffer[position++];

            return base.Read();
        }

        /// <summary>
        /// Reads the amount of bytes specified from the stream.
        /// </summary>
        /// <param name="array">The buffer to read data into.</param>
        /// <param name="index">The beginning point to read.</param>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The number of characters read into buffer.</returns>
        public virtual int Read(sbyte[] array, int index, int count)
        {
            int byteCount = 0;
            int readLimit = count + index;
            byte[] aux = ToByteArray(array);

            for (byteCount = 0; position < buffer.Length && index < readLimit; byteCount++)
                aux[index++] = buffer[position++];

            if (index < readLimit)
                byteCount += base.Read(aux, index, readLimit - index);

            for(int i = 0; i < aux.Length;i++)
                array[i] = (sbyte)aux[i];

            return byteCount;
        }

        /// <summary>
        /// Unreads a byte from the stream.
        /// </summary>
        /// <param name="element">The value to be unread.</param>
        public void UnRead(int element)
        {
            this.position--;
            if (position >= 0)
                this.buffer[this.position] = (byte) element;
        }

        /// <summary>
        /// Unreads an amount of bytes from the stream.
        /// </summary>
        /// <param name="array">The byte array to be unread.</param>
        /// <param name="index">The beginning index to unread.</param>
        /// <param name="count">The number of bytes to be unread.</param>
        public void UnRead(byte[] array, int index, int count)
        {
            this.Move(array, index, count);
        }

        /// <summary>
        /// Unreads an array of bytes from the stream.
        /// </summary>
        /// <param name="array">The byte array to be unread.</param>
        public void UnRead(byte[] array)
        {
            this.Move(array, 0, array.Length - 1);
        }

        /// <summary>
        /// Skips the specified number of bytes from the underlying stream.
        /// </summary>
        /// <param name="numberOfBytes">The number of bytes to be skipped.</param>
        /// <returns>The number of bytes actually skipped</returns>
        public long Skip(long numberOfBytes)
        {
            return this.BaseStream.Seek(numberOfBytes, SeekOrigin.Current) - this.BaseStream.Position;
        }

        /// <summary>
        /// Moves data from the array to the buffer field.
        /// </summary>
        /// <param name="array">The array of bytes to be unread.</param>
        /// <param name="index">The beginning index to unread.</param>
        /// <param name="count">The amount of bytes to be unread.</param>
        private void Move(byte[] array, int index, int count)
        {
            for (int arrayPosition = index + count;  arrayPosition >= index; arrayPosition--)
                this.UnRead(array[arrayPosition]);
        }

        /// <summary>
        /// Converts an array of sbytes to an array of bytes
        /// </summary>
        /// <param name="sbyteArray">The array of sbytes to be converted</param>
        /// <returns>The new array of bytes</returns>
        public static byte[] ToByteArray(sbyte[] sbyteArray)
        {
            byte[] byteArray = null;

            if (sbyteArray != null)
            {
                byteArray = new byte[sbyteArray.Length];
                for(int index=0; index < sbyteArray.Length; index++)
                {
                    byteArray[index] = (byte) sbyteArray[index];
                }
            }

            return byteArray;
        }

        /// <summary>
        /// Converts a string to an array of bytes
        /// </summary>
        /// <param name="sourceString">The string to be converted</param>
        /// <returns>The new array of bytes</returns>
        public static byte[] ToByteArray(string sourceString)
        {
            return UTF8Encoding.UTF8.GetBytes(sourceString);
        }
    }
}
