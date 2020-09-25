using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace iGeospatial.Texts
{
	/// <summary>
	/// This class provides auxiliar functionality to read and unread characters from a string into a buffer.
	/// </summary>
	internal class BackStringReader : StringReader
	{
		private char[] buffer;
		private int position = 1;

		/// <summary>
		/// Constructor. Calls the base constructor.
		/// </summary>
		/// <param name="stringReader">The buffer from which chars will be read.</param>
		/// <param name="size">The size of the Back buffer.</param>
		public BackStringReader(String s) : base (s)
		{
			this.buffer = new char[position];
		}


		/// <summary>
		/// Reads a character.
		/// </summary>
		/// <returns>The character read.</returns>
		public override int Read()
		{
			if (this.position >= 0 && this.position < this.buffer.Length)
				return (int) this.buffer[this.position++];
			return base.Read();
		}

		/// <summary>
		/// Reads an amount of characters from the buffer and copies the values to the array passed.
		/// </summary>
		/// <param name="array">Array where the characters will be stored.</param>
		/// <param name="index">The beginning index to read.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The number of characters read.</returns>
		public override int Read(char[] array, int index, int count)
		{
			int readLimit = this.buffer.Length - this.position;

			if (count <= 0)
				return 0;

			if (readLimit > 0)
			{
				if (count < readLimit)
					readLimit = count;
				System.Array.Copy(this.buffer, this.position, array, index, readLimit);
				count -= readLimit;
				index += readLimit;
				this.position += readLimit;
			}

			if (count > 0)
			{
				count = base.Read(array, index, count);
				if (count == -1)
				{
					if (readLimit == 0)
						return -1;
					return readLimit;
				}
				return readLimit + count;
			}
			return readLimit;
		}

		/// <summary>
		/// Unreads a character.
		/// </summary>
		/// <param name="unReadChar">The character to be unread.</param>
		public void UnRead(int unReadChar)
		{
			this.position--;
			this.buffer[this.position] = (char) unReadChar;
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		/// <param name="index">The beginning index to unread.</param>
		/// <param name="count">The number of characters to unread.</param>
		public void UnRead(char[] array, int index, int count)
		{
			this.Move(array, index, count);
		}

		/// <summary>
		/// Unreads an amount of characters by moving these to the buffer.
		/// </summary>
		/// <param name="array">The character array to be unread.</param>
		public void UnRead(char[] array)
		{
			this.Move(array, 0, array.Length - 1);
		}

		/// <summary>
		/// Moves the array of characters to the buffer.
		/// </summary>
		/// <param name="array">Array of characters to move.</param>
		/// <param name="index">Offset of the beginning.</param>
		/// <param name="count">Amount of characters to move.</param>
		private void Move(char[] array, int index, int count)
		{
			for (int arrayPosition = index + count; arrayPosition >= index; arrayPosition--)
				this.UnRead(array[arrayPosition]);
		}
	}

}
