// StreamTokenizer.cs
// 
// Copyright (C) 2002-2004 Ryan Seghers
//
// This software is provided AS IS. No warranty is granted, 
// neither expressed nor implied. USE THIS SOFTWARE AT YOUR OWN RISK.
// NO REPRESENTATION OF MERCHANTABILITY or FITNESS FOR ANY 
// PURPOSE is given.
//
// License to use this software is limited by the following terms:
// 1) This code may be used in any program, including programs developed
//    for commercial purposes, provided that this notice is included verbatim.
//    
// Also, in return for using this code, please attempt to make your fixes and
// updates available in some way, such as by sending your updates to the
// author.
//
// To-do:
//		make type exclusivity explict, and enforce:
//			digit can be word
//			word can't be whitespace
//			etc.
//      large-integer handling is imprecise, fix it
//          (most 19 digit decimal numbers can be Int64's but I'm 
//           using float anyway)
//		add more mangled float test cases
//
// Later:
//		reconfigurable vs Unicode support
//		add NUnit wrap of built-in tests

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Diagnostics;

namespace iGeospatial.Texts
{
	#region Exceptions
	// ---------------------------------------------------------------------

	/// <summary>
	/// Exception class for unterminated tokens.
	/// </summary>
	public class UntermException : System.ApplicationException
	{
		/// <summary>
		/// Construct with a particular message.
		/// </summary>
		/// <param name="msg">The message to store in this object.</param>
		public UntermException(string msg) : base(msg) {}
	}

	/// <summary>
	/// Exception class for unterminated quotes.
	/// </summary>
	public class UntermQuoteException : UntermException
	{
		/// <summary>
		/// Construct with a particular message.
		/// </summary>
		/// <param name="msg">The message to store in this object.</param>
		public UntermQuoteException(string msg) : base(msg) {}
	}

	/// <summary>
	/// Exception class for unterminated block comments.
	/// </summary>
	public class UntermCommentException : UntermException
	{
		/// <summary>
		/// Construct with a particular message.
		/// </summary>
		/// <param name="msg">The message to store in this object.</param>
		public UntermCommentException(string msg) : base(msg) {}
	}

	#endregion

	/// <summary>
	/// A StreamTokenizer similar to Java's.  This breaks an input stream
	/// (coming from a <see cref="System.IO.TextReader"/>) into Tokens based 
	/// on various settings.  
	/// The settings are stored in the TokenizerSettings property, which is a
	/// StreamTokenizerSettings instance.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is configurable in that you can modify TokenizerSettings.CharTypes[] 
	/// array to specify which characters are which type, along with other 
	/// settings such as whether to look for comments or not.
	/// </para>
	/// <para>
	/// <list type="">
	/// There are two main ways to use this: 
	/// 1) Parse the entire stream at once and get an <see cref="System.Collections.ArrayList"/> 
	/// of Tokens (see the Tokenize* methods). 
	/// 2) Call NextToken() successively.
	/// </list>
	/// This reads from a TextReader, which you can set directly, and this
	/// also provides some convenient methods to parse files and strings.
	/// This returns an Eof token if the end of the input is reached.
	/// </para>
	/// <para>
	/// Here's an example of the NextToken() style of use:
	/// <code>
	/// StreamTokenizer tokenizer         = new StreamTokenizer();
	/// tokenizer.Settings.GrabWhitespace = true;
	/// tokenizer.TextReader              = File.OpenText(fileName);
	/// Token token;
	/// while (tokenizer.NextToken(out token)) 
	/// {
	///		Debug.Writeline("Token = '{0}'", token);
	/// }
	/// </code>
	/// </para>
	/// <para>
	/// Here's an example of the Tokenize... style of use:
	/// <code>
	/// StreamTokenizer tokenizer = new StreamTokenizer("some string");
	/// ArrayList tokens = new ArrayList();
	/// if (!tokenizer.Tokenize(tokens)) 
	/// { 
	///		// error handling
	/// }
	/// foreach (Token t in tokens) 
	/// {
	///		Debug.WriteLine("t = {0}", t);
	///	}
	/// </code>
	/// </para>
	/// <para>
	/// Comment delimiters are hardcoded (// and /*), not affected by char 
	/// type table. This is not internationalized, this only handles 
	/// the 7-bit ASCII character range, which is a subset of UTF-8.
	/// </para>
	/// <para>
	/// This sets line numbers in the tokens it produces.  These numbers 
	/// are normally the line on which the token starts.
	/// There is one known caveat, and that is that when GrabWhitespace 
	/// setting is true, and a whitespace token contains a newline, 
	/// that token's line number will be set to the following line 
	/// rather than the line on which the token started.
	/// </para>
	/// </remarks>
	public class StreamTokenizer
	{
        #region NextTokenState Enumeration

		/// <summary>
		/// The states of the state machine.
		/// </summary>
		private enum NextTokenState
		{
			Start,
			Whitespace,
			Word,
			Quote,
			EndQuote,
			MaybeNumber, // could be number or word
			MaybeComment, // after first slash, might be comment or not
			MaybeHex, // after 0, may be hex
			HexGot0x, // after 0x, may be hex
			HexNumber,
			LineComment,
			BlockComment,
			EndBlockComment,
			Char,
			Eol,
			Eof,
			Invalid
		}
        
        #endregion

		#region Constants

		/// <summary>
		/// This is the number of characters in the character table.
		/// </summary>
		public static readonly int  NCHARS = 128;
		private static readonly int EOF    = NCHARS;

		#endregion

		#region Private Fields

		// The TextReader we're reading from
		private TextReader textReader;

		// buffered wrap of reader
		//private BufferedTextReader bufferedReader; // was slower

		// keep track of current line number during parse
		private int lineNumber;  

		// used to back up in the stream
		private CharBuffer backString;  

		// used to collect characters of the current (next to be
		// emitted) token
		private CharBuffer nextTokenSb;

		// for speed, construct these once and re-use
		private CharBuffer tmpSb;
		private CharBuffer expSb;
			
		private bool _pushedBack = false;
		private Token _token     = null;

        private StreamTokenizerSettings settings;

		#endregion

		#region Properties

		/// <summary>
		/// This is the TextReader that this object will read from.
		/// Set this to set the input reader for the parse.
		/// </summary>
		public TextReader TextReader
		{
			get 
            { 
                return textReader; 
            }

			set 
            { 
                textReader = value; 
            }
		}

		/// <summary>
		/// The settings which govern the behavior of the tokenization.
		/// </summary>
		public StreamTokenizerSettings Settings 
        { 
            get 
            { 
                return settings; 
            } 
        }

		#endregion

        #region Constructors and Destructor

		/// <summary>
		/// Default constructor.
		/// </summary>
		public StreamTokenizer()
		{
			Initialize();
		}

		/// <summary>
		/// Construct and set this object's TextReader to the one specified.
		/// </summary>
		/// <param name="reader">The TextReader to read from.</param>
		public StreamTokenizer(TextReader reader)
		{
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

			Initialize();
			textReader = reader;
		}

		/// <summary>
		/// Construct and set a string to tokenize.
		/// </summary>
		/// <param name="text">The string to tokenize.</param>
		public StreamTokenizer(string text)
		{
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (text.Length == 0)
            {
                throw new ArgumentException("text");
            }

			Initialize();
			textReader = new StringReader(text);
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Display the state of this object.
		/// </summary>
		public void Display()
		{
			Display("");
		}

		/// <summary>
		/// Display the state of this object, with a per-line prefix.
		/// </summary>
		/// <param name="prefix">The pre-line prefix.</param>
		public void Display(string prefix)
		{
			if (settings != null) 
                settings.Display(prefix + "    ");
		}

        /// <summary>
        /// Construct and set this object's TextReader to the one specified.
        /// </summary>
        /// <param name="reader">The TextReader to read from.</param>
        public void Initialize(TextReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            Initialize();
            textReader = reader;
        }

        /// <summary>
        /// Construct and set a string to tokenize.
        /// </summary>
        /// <param name="text">The string to tokenize.</param>
        public void Initialize(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            if (text.Length == 0)
            {
                throw new ArgumentException("text");
            }

            Initialize();
            textReader = new StringReader(text);
        }

		/// <summary> 
		/// Causes the next call to the <c>nextToken</c> method of this
		/// tokenizer to return the current value in the <c>ttype</c>
		/// field, and not to modify the value in the <c>nval</c> or
		/// <c>sval</c> field. 
		/// </summary>
		public void PushBack()
		{
			if (_token.Type != TokenType.Eof)
				_pushedBack = true;
		}

        /// <summary>  
        /// Returns the next number in the stream.
        /// </summary>
        /// <param name="tokenizer"> 
        /// Tokenizer over a stream of text in Well-known Text
        /// format. The next token must be a number.
        /// </param>
        /// <returns>The next number in the stream.</returns>
        /// <exception cref="TokenizerException">
        /// If the next token is not a number.
        /// </exception>
        /// <exception cref="TokenizerException">
        /// If an I/O error occurs.
        /// </exception>
        public double GetNextNumber()
        {
            Token token = null;
            if (!NextToken(out token))
            {
                return double.NaN;
            }

            TokenType type = token.Type;

            switch (type)
            {
                case TokenType.Eof: 
                    throw new TokenizerException("Expected number but encountered end of stream");
				
                case TokenType.Eol: 
                    throw new TokenizerException("Expected number but encountered end of line");
				
                case TokenType.Float: 
                    return Convert.ToDouble(token.Object);

                case TokenType.Integer: 
                    return Convert.ToDouble(token.Object);
				
                case TokenType.Word: 
                    throw new TokenizerException("Expected number but encountered word: " + token.StringValue);
				
                default:
                {
                    string sVal = token.StringValue;
                    if (sVal == "(")
                        throw new TokenizerException("Expected number but encountered '('");
				
                    if (sVal == ")") 
                        throw new TokenizerException("Expected number but encountered ')'");
				
                    if (sVal == ",")
                        throw new TokenizerException("Expected number but encountered ','");
                }
                    break;
            }

            Debug.Assert(false, "Should never reach here: Encountered unexpected StreamTokenizer type: " + type);
            return 0;
        }
		
        /// <summary>  Returns the next word in the stream as uppercase text.
        /// 
        /// </summary>
        /// <param name="tokenizer"> tokenizer over a stream of text in Well-known Text
        /// format. The next token must be a word.
        /// </param>
        /// <returns>                  the next word in the stream as uppercase text
        /// </returns>
        /// <exception cref="TokenizerException">If the next token is not a word.</exception>
        /// <exception cref="TokenizerException">If an I/O error occurs.</exception>
        public System.String GetNextWord()
        {
            Token token = null;
            if (!NextToken(out token))
            {
                return null;
            }

            TokenType type = token.Type;

            switch (type)
            {
                case TokenType.Eof: 
                    throw new TokenizerException("Expected word but encountered end of stream");
				
                case TokenType.Eol: 
                    throw new TokenizerException("Expected word but encountered end of line");
				
                case TokenType.Float: 
                    throw new TokenizerException("Expected word but encountered number: " + token.StringValue);
				
                case TokenType.Integer: 
                    throw new TokenizerException("Expected word but encountered number: " + token.StringValue);
				
                case TokenType.Word: 
                    return token.StringValue.ToUpper();

                default:
                    {
                    string sVal = token.StringValue;
                    if (sVal == "(")
                        return "(";
				
                    if (sVal == ")") 
                        return ")";
				
                    if (sVal == ",")
                        return ",";
                    }
                    break;
            }   

            Debug.Assert(false, "Should never reach here: Encountered unexpected StreamTokenizer type: " + type);
            
            return null;
        }
						
		#endregion

		#region Public Methods (Tokenize wrapper methods)

		/// <summary>
		/// Parse the rest of the stream and put all the tokens
		/// in the input ArrayList. This resets the line number to 1.
		/// </summary>
		/// <param name="tokens">The ArrayList to append to.</param>
		/// <returns>bool - true for success</returns>
		public bool Tokenize(ArrayList tokens)
		{
			Token token;
			this.lineNumber = 1;

			while (NextToken(out token))
			{
				if (token == null) throw new NullReferenceException(
						"StreamTokenizer: Tokenize: Got a null token from NextToken.");
				tokens.Add(token);
			}

			// add the last token returned (EOF)
			tokens.Add(token);
			return(true);
		}

		/// <summary>
		/// Parse all tokens from the specified TextReader, put
		/// them into the input ArrayList.
		/// </summary>
		/// <param name="tr">The TextReader to read from.</param>
		/// <param name="tokens">The ArrayList to append to.</param>
		/// <returns>bool - true for success, false for failure.</returns>
		public bool TokenizeReader(TextReader tr, ArrayList tokens)
		{
			textReader = tr;

			return(Tokenize(tokens));
		}

		/// <summary>
		/// Parse all tokens from the specified file, put
		/// them into the input ArrayList.
		/// </summary>
		/// <param name="fileName">The file to read.</param>
		/// <param name="tokens">The ArrayList to put tokens in.</param>
		/// <returns>bool - true for success, false for failure.</returns>
		public bool TokenizeFile(string fileName, ArrayList tokens)
		{
			FileInfo fi = new FileInfo(fileName);
			FileStream fr = null;
			try 
			{
				fr = fi.Open(FileMode.Open, FileAccess.Read, FileShare.None);
				textReader = new StreamReader(fr);
			} 
			catch (DirectoryNotFoundException)
			{
			}
			try
			{
				if (!Tokenize(tokens))
				{
					textReader.Close();
					if (fr != null) 
                        fr.Close();

					return false;
				}
			}
			catch (UntermException e)
			{
				textReader.Close();
				if (fr != null) 
                    fr.Close();

				throw e;
			}

			if (textReader != null) 
                textReader.Close();

			if (fr != null) 
                fr.Close();

			return true;
		}

		/// <summary>
		/// Parse all tokens from the specified string, put
		/// them into the input ArrayList.
		/// </summary>
		/// <param name="str"></param>
		/// <param name="tokens">The ArrayList to put tokens in.</param>
		/// <returns>bool - true for success, false for failure.</returns>
		public bool TokenizeString(string str, ArrayList tokens)
		{
			textReader = new StringReader(str);

			return Tokenize(tokens);
		}

		/// <summary>
		/// Parse all tokens from the specified Stream, put
		/// them into the input ArrayList.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="tokens">The ArrayList to put tokens in.</param>
		/// <returns>bool - true for success, false for failure.</returns>
		public bool TokenizeStream(Stream s, ArrayList tokens)
		{
			textReader = new StreamReader(s);

			return Tokenize(tokens);
		}

		/// <summary>
		/// Tokenize a file completely and return the tokens in a Token[].
		/// </summary>
		/// <param name="fileName">The file to tokenize.</param>
		/// <returns>A Token[] with all tokens.</returns>
		public Token[] TokenizeFile(string fileName)
		{
			ArrayList list = new ArrayList();
			if (!TokenizeFile(fileName, list))
			{
				return null;
			}
			else
			{
				if (list.Count > 0)
				{
					return (Token[])list.ToArray(typeof(Token));
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

        #region Private Methods

        /// <summary>
        /// Utility function, things common to constructors.
        /// </summary>
        private void Initialize()
        {
            backString  = new CharBuffer(32);
            nextTokenSb = new CharBuffer(1024);

            InitializeStream();
            settings = new StreamTokenizerSettings();
            settings.SetDefaults();

            expSb = new CharBuffer();
            tmpSb = new CharBuffer();
        }

        /// <summary>
        /// Clear the stream settings.
        /// </summary>
        void InitializeStream()
        {
            lineNumber = 1; // base 1 line numbers
            textReader = null;
        }
        
        #endregion

        #region Private Methods (NextToken - the state machine)

		/// <summary>
		/// Pick the next state given just a single character.  This is used
		/// at the start of a new token.
		/// </summary>
		/// <param name="ctype">The type of the character.</param>
		/// <param name="c">The character.</param>
		/// <returns>The state.</returns>
		private NextTokenState PickNextState(byte ctype, int c)
		{
			return(PickNextState(ctype, c, NextTokenState.Start));
		}

		/// <summary>
		/// Pick the next state given just a single character.  This is used
		/// at the start of a new token.
		/// </summary>
		/// <param name="ctype">The type of the character.</param>
		/// <param name="c">The character.</param>
		/// <param name="excludeState">Exclude this state from the possible next state.</param>
		/// <returns>The state.</returns>
		private NextTokenState PickNextState(byte ctype, int c, NextTokenState excludeState)
		{
			if (c == '/') 
			{
				return(NextTokenState.MaybeComment); // overrides all other cats
			}
			else if ((excludeState != NextTokenState.MaybeHex) 
				&& settings.ParseHexNumbers && (c == '0')) 
			{
				return(NextTokenState.MaybeHex);
			}
			else if ((excludeState != NextTokenState.MaybeNumber) 
				&& settings.ParseNumbers 
				&& (settings.IsCharType(ctype, CharTypeBits.Digit) 
				|| (c == '-') || (c == '.'))) 
			{
				return(NextTokenState.MaybeNumber);
			}
			else if (settings.IsCharType(ctype, CharTypeBits.Word)) 
			{
				return(NextTokenState.Word);
			}
			else if (settings.GrabEol && (c == 10)) 
			{
				return(NextTokenState.Eol);
			}
			else if (settings.IsCharType(ctype, CharTypeBits.Whitespace)) 
			{
				return(NextTokenState.Whitespace);
			}
			else if (settings.IsCharType(ctype, CharTypeBits.Comment)) 
			{
				return(NextTokenState.LineComment);
			}
			else if (settings.IsCharType(ctype, CharTypeBits.Quote)) 
			{
				return(NextTokenState.Quote);
			}
			else if ((c == EOF) || 
				(settings.IsCharType(ctype, CharTypeBits.Eof))) 
			{
				return(NextTokenState.Eof);
			}

			return(NextTokenState.Char);
		}

		/// <summary>
		/// Read the next character from the stream, or from backString
		/// if we backed up.
		/// </summary>
		/// <returns>The next character.</returns>
		private int GetNextChar()
		{
			int c;

			// consume from backString if possible
			if (backString.Length > 0)
			{
				c = backString[0];
				backString.Remove(0, 1);
				return(c);
			}

			if (textReader == null) 
				return(EOF);

			try
			{
				while((c = textReader.Read()) == 13) {} // skip LF (13)
			}
			catch(Exception)
			{
				return(EOF);
			}

			if (c == 10) 
			{
				lineNumber++;
			}
			else if (c < 0) 
			{
				c = EOF;
			}

			return(c);
		}

		/// <summary>
		/// Get the next token.  The last token will be an EofToken unless
		/// there's an unterminated quote or unterminated block comment
		/// and Settings.DoUntermCheck is true, in which case this throws
		/// an exception of type UntermException or sub-class.
		/// </summary>
		/// <param name="token">The output token.</param>
		/// <returns>bool - true for success, false for failure.</returns>
		public bool NextToken(out Token token)
		{
			if (_pushedBack)
			{
				_pushedBack = false;
				token       = _token;
				return true;
			}

			token = null;
			int thisChar = 0; // current character
			byte ctype; // type of this character

			NextTokenState state = NextTokenState.Start;
			int prevChar = 0; // previous character
			byte prevCtype = (byte)CharTypeBits.Eof;

			// get previous char from nextTokenSb if there
			// (nextTokenSb is a StringBuilder containing the characters
			//  of the next token to be emitted)
			if (nextTokenSb.Length > 0) 
			{
				prevChar = nextTokenSb[nextTokenSb.Length - 1];
				prevCtype = settings.CharTypes[prevChar];
				state = PickNextState(prevCtype, prevChar);
			}

			// extra state for number parse
			int seenDot = 0; // how many .'s in the number
			int seenE = 0; // how many e's or E's have we seen in the number
			bool seenDigit = false; // seen any digits (numbers can start with -)

			// lineNumber can change with each GetNextChar()
			// tokenLineNumber is the line on which the token started
			int tokenLineNumber = lineNumber;

			// State Machine: Produces a single token.
			// Enter a state based on a single character.
			// Generally, being in a state means we're currently collecting chars 
			// in that type of token.
			// We do state machine until it builds a token (Eof is a token), then
			// return that token.
			thisChar = prevChar;  // for first iteration, since prevChar is set to this 
			bool done = false; // optimization
			while (!done)
			{
				prevChar = thisChar;
				thisChar = GetNextChar();
				ctype = settings.CharTypes[thisChar];

				// see if we need to change states, or emit a token
				switch(state)
				{
					case NextTokenState.Start:
						// RESET
						state = PickNextState(ctype, thisChar);
						tokenLineNumber = lineNumber;
						break;

					case NextTokenState.Char:
						token = new CharToken((char)prevChar, tokenLineNumber);
						done = true;
						nextTokenSb.Length = 0;
						break;

					case NextTokenState.Word:
						if ((!settings.IsCharType(ctype, CharTypeBits.Word))
							&& (!settings.IsCharType(ctype, CharTypeBits.Digit)))
						{
							// end of word, emit
							token = new WordToken(nextTokenSb.ToString(), tokenLineNumber);
							done = true;
							nextTokenSb.Length = 0;
						}
						break;

					case NextTokenState.Whitespace:
						if (!settings.IsCharType(ctype, CharTypeBits.Whitespace)
							|| (settings.GrabWhitespace && (thisChar == 10)))
						{
							// end of whitespace, emit
							if (settings.GrabWhitespace)
							{
								token = new WhitespaceToken(nextTokenSb.ToString(), tokenLineNumber);
								done = true;
								nextTokenSb.Length = 0;
							}
							else
							{
								// RESET
								nextTokenSb.Length = 0;
								tokenLineNumber = lineNumber;
								state = PickNextState(ctype, thisChar);
							}
						}
						break;

					case NextTokenState.EndQuote:
						// we're now 1 char after end of quote
						token = new QuoteToken(nextTokenSb.ToString(), tokenLineNumber);
						done = true;
						nextTokenSb.Length = 0;
						break;

					case NextTokenState.Quote:
						// looking for end quote matching char that started the quote
						if (thisChar == nextTokenSb[0])
						{
							// handle escaped backslashes: count the immediately prior backslashes 
							// - even (including 0) means it's not escaped 
							// - odd means it is escaped 
							int backSlashCount = 0; 
							for (int i = nextTokenSb.Length - 1; i >= 0; i--)
							{ 
								if (nextTokenSb[ i ] == '\\') backSlashCount++; 
								else break; 
							} 

							if ((backSlashCount % 2) == 0) 
							{ 
								state = NextTokenState.EndQuote;
							}
						}

						if ((state != NextTokenState.EndQuote) && (thisChar == EOF))
						{
							if (settings.DoUntermCheck) 
							{
								nextTokenSb.Length = 0;
								throw new UntermQuoteException("Unterminated quote");
							}

							token = new QuoteToken(nextTokenSb.ToString(), tokenLineNumber);
							done = true;
							nextTokenSb.Length = 0;
						}
						break;

					case NextTokenState.MaybeComment:
						if (thisChar == EOF)
						{
							token = new CharToken(nextTokenSb.ToString(), tokenLineNumber);
							done = true;
							nextTokenSb.Length = 0;
						}
						else
						{
							// if we get the right char, we're in a comment
							if (settings.SlashSlashComments && (thisChar == '/')) 
								state = NextTokenState.LineComment;
							else if (settings.SlashStarComments && (thisChar == '*')) 
								state = NextTokenState.BlockComment;
							else
							{
								token = new CharToken(nextTokenSb.ToString(), tokenLineNumber);
								done = true;
								nextTokenSb.Length = 0;
							}
						}
						break;

					case NextTokenState.LineComment:
						if (thisChar == EOF)
						{
							if (settings.GrabComments) 
							{
								token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
								done = true;
								nextTokenSb.Length = 0;
							}
							else
							{
								// RESET
								nextTokenSb.Length = 0;
								tokenLineNumber = lineNumber;
								state = PickNextState(ctype, thisChar);
							}
						}
						else
						{
							if (thisChar == '\n')
							{
								if (settings.GrabComments) 
								{
									token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
									done = true;
									nextTokenSb.Length = 0;
								}
								else
								{
									// RESET
									nextTokenSb.Length = 0;
									tokenLineNumber = lineNumber;
									state = PickNextState(ctype, thisChar);
								}
							}
						}
						break;

					case NextTokenState.BlockComment:
						if (thisChar == EOF)
						{
							if (settings.DoUntermCheck) 
							{
								nextTokenSb.Length = 0;
								throw new UntermCommentException("Unterminated comment.");
							}

							if (settings.GrabComments) 
							{
								token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
								done = true;
								nextTokenSb.Length = 0;
							}
							else
							{
								// RESET
								nextTokenSb.Length = 0;
								tokenLineNumber = lineNumber;
								state = PickNextState(ctype, thisChar);
							}
						}
						else
						{
							if ((thisChar == '/') && (prevChar == '*'))
							{
								state = NextTokenState.EndBlockComment;
							}
						}
						break;

					// special case for 2-character token termination
					case NextTokenState.EndBlockComment:
						if (settings.GrabComments) 
						{
							token = new CommentToken(nextTokenSb.ToString(), tokenLineNumber);
							done = true;
							nextTokenSb.Length = 0;
						}
						else
						{
							// RESET
							nextTokenSb.Length = 0;
							tokenLineNumber = lineNumber;
							state = PickNextState(ctype, thisChar);
						}
						break;

					case NextTokenState.MaybeHex:
						// previous char was 0
						if (thisChar != 'x')
						{
							// back up and try non-hex
							// back up to the 0
							nextTokenSb.Append((char)thisChar);
							backString.Append(nextTokenSb);
							nextTokenSb.Length = 0;

							// reset state and don't choose MaybeNumber state.
							// pull char from backString
							thisChar = backString[0];
							backString.Remove(0, 1);
							state = PickNextState(settings.CharTypes[thisChar], (int)thisChar, 
								NextTokenState.MaybeHex);
						}
						else state = NextTokenState.HexGot0x;
						break;

					case NextTokenState.HexGot0x:
						if (!settings.IsCharType(ctype, CharTypeBits.HexDigit))
						{
							// got 0x but now a non-hex char
							// back up to the 0
							nextTokenSb.Append((char)thisChar);
							backString.Append(nextTokenSb);
							nextTokenSb.Length = 0;

							// reset state and don't choose MaybeNumber state.
							// pull char from backString
							thisChar = backString[0];
							backString.Remove(0, 1);
							state = PickNextState(settings.CharTypes[thisChar], (int)thisChar, 
								NextTokenState.MaybeHex);
						}
						else state = NextTokenState.HexNumber;
						break;

					case NextTokenState.HexNumber:
						if (!settings.IsCharType(ctype, CharTypeBits.HexDigit))
						{
							// emit the hex number we've collected
							token = IntToken.ParseHex(nextTokenSb.ToString(), tokenLineNumber);
							done = true;
							nextTokenSb.Length = 0;
						}
						break;

					case NextTokenState.MaybeNumber:
						//
						// Determine whether or not to stop collecting characters for
						// the number parse.  We terminate when it's clear it's not
						// a number or no longer a number.
						//
						bool term = false;

						if (settings.IsCharType(ctype, CharTypeBits.Digit)  
							|| settings.IsCharType(prevChar, CharTypeBits.Digit)) seenDigit = true;

						// term conditions
						if (thisChar == '.') 
						{ 
							seenDot++; 
							if (seenDot > 1) term = true;  // more than one dot, it aint a number
						}
						else if (((thisChar == 'e') || (thisChar == 'E')))
						{
							seenE++;
							if (!seenDigit) term = true;  // e before any digits is bad
							else if (seenE > 1) term = true;  // more than 1 e is bad
							else
							{
								term = true; // done regardless

								// scan the exponent, put its characters into
								// nextTokenSb, if there are any
								char c;
								expSb.Clear();
								expSb.Append((char)thisChar);
								if (GrabInt(expSb, true, out c))
								{
									// we got a good exponent, tack it on
									nextTokenSb.Append(expSb);
									thisChar = c; // and continue after the exponent's characters
								}
							}
						}
						else if (thisChar == EOF) term = true;  
							// or a char that can't be in a number
						else if ((!settings.IsCharType(ctype, CharTypeBits.Digit) 
							&& (thisChar != 'e') && (thisChar != 'E') 
							&& (thisChar != '-') && (thisChar != '.')) 
							|| ((thisChar == '+') && (seenE == 0)))
						{
							// it's not a normal number character
							term = true;
						}
						// or a dash not after e
						else if ((thisChar == '-') && 
                            (!((prevChar == 'e') || (prevChar == 'E')))) 
                            term = true;

						if (term)
						{
							// we are terminating a number, or it wasn't a number
							if (seenDigit)
							{
								if ((nextTokenSb.IndexOf('.') >= 0)
									|| (nextTokenSb.IndexOf('e') >= 0)
									|| (nextTokenSb.IndexOf('E') >= 0)
									|| (nextTokenSb.Length >= 19) // probably too large for Int64, use float
									)
								{
									token = new FloatToken(nextTokenSb.ToString(), tokenLineNumber);
								}
								else 
								{
									token = new IntToken(nextTokenSb.ToString(), tokenLineNumber);
								}
								done = true;
								nextTokenSb.Length = 0;
							}
							else
							{
								// -whatever or -.whatever
								// didn't see any digits, must have gotten here by a leading -
								// and no digits after it
								// back up to -, pick next state excluding numbers
								nextTokenSb.Append((char)thisChar);
								backString.Append(nextTokenSb);
								nextTokenSb.Length = 0;

								// restart on the - and don't choose MaybeNumber state
								// pull char from backString
								thisChar = backString[0];
								backString.Remove(0, 1);
								state = PickNextState(settings.CharTypes[thisChar], (int)thisChar, 
									NextTokenState.MaybeNumber);
							}
						}
						break;

					case NextTokenState.Eol:
						// tokenLineNumber - 1 because the newline char is on the previous line
						token = new EolToken(tokenLineNumber - 1);
						done = true;
						nextTokenSb.Length = 0;
						_token = token;
						break;

					case NextTokenState.Eof:
						token = new EofToken(tokenLineNumber);
						done = true;
						nextTokenSb.Length = 0;
						_token = token;
						return false;

					case NextTokenState.Invalid:
					default:
						// not a good sign, some unrepresented state?
//						log.Error("NextToken: Hit unrepresented state {0}", state);
						_token = null;
						return false;
				}

				// use a StringBuilder to accumulate characters which are part of this token
				if (thisChar != EOF) 
					nextTokenSb.Append((char)thisChar);
			}

			_token = token;

			return true;
		}

		/// <summary>
		/// Starting from current stream location, scan forward
		/// over an int.  Determine whether it's an integer or not.  If so, 
		/// push the integer characters to the specified CharBuffer.  
		/// If not, put them in backString (essentially leave the
		/// stream as it was) and return false.
		/// <para>
		/// If it was an int, the stream is left 1 character after the
		/// end of the int, and that character is output in the thisChar parameter.
		/// </para>
		/// <para>The formats for integers are: 1, +1, and -1</para>
		/// The + and - signs are included in the output buffer.
		/// </summary>
		/// <param name="sb">The CharBuffer to append to.</param>
		/// <param name="allowPlus">Whether or not to consider + to be part
		/// of an integer.</param>
		/// <param name="thisChar">The last character read by this method.</param>
		/// <returns>true for parsed an int, false for not an int</returns>
		private bool GrabInt(CharBuffer sb, bool allowPlus, out char thisChar)
		{
			tmpSb.Clear(); // use tmp CharBuffer

			// first character can be -, maybe can be + depending on arg
			thisChar = (char)GetNextChar();

			if (thisChar == EOF)
			{
				return false;
			}
			else if (thisChar == '+')
			{
				if (allowPlus) 
				{
					tmpSb.Append(thisChar);
				}
				else
				{
					backString.Append(thisChar);
					return false;
				}
			}
			else if (thisChar == '-')
			{
				tmpSb.Append(thisChar);
			}
			else if (settings.IsCharType(thisChar, CharTypeBits.Digit))
			{
				// a digit, back this out so we can handle it in loop below
				backString.Append(thisChar);
			}
			else 
			{
				// not a number starter
				backString.Append(thisChar);
				
                return false;
			}

			// rest of chars have to be digits
			bool gotInt = false;
			while(((thisChar = (char)GetNextChar()) != EOF)
				&& (settings.IsCharType(thisChar, CharTypeBits.Digit)))
			{
				gotInt = true;
				tmpSb.Append(thisChar);
			}

			if (gotInt) 
			{
				sb.Append(tmpSb);
				
                return true;
			}
			else 
			{
				// didn't get any chars after first 
				backString.Append(tmpSb); // put + or - back on
				if (thisChar != EOF) 
                    backString.Append(thisChar);

				return false;
			}
		}

		#endregion
    }
}


