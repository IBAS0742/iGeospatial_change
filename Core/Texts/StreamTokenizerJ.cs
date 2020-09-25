using System;
using System.IO;
using System.Text;
using System.Diagnostics;

namespace iGeospatial.Texts
{
    /// <summary>
    /// This class takes an input stream and parses it into "tokens".
    /// The stream tokenizer can recognize identifiers, numbers, quoted strings, and various comment styles. 
    /// </summary>
    public class StreamTokenizerJ
    {
        #region Public Fields

        /// <summary>
        /// Indicates that the end of the stream has been read.
        /// </summary>
        public const int TT_EOF		= - 1;

        /// <summary>
        /// Indicates that the end of the line has been read.
        /// </summary>
        public const int TT_EOL		= '\n';

        /// <summary>
        /// Indicates that a number token has been read.
        /// </summary>
        public const int TT_NUMBER	= - 2;

        /// <summary>
        /// Indicates that a word token has been read.
        /// </summary>
        public const int TT_WORD	= - 3;

        /// <summary>
        /// If the current token is a number, this field contains the value of that number.
        /// </summary>
        public double nval;

        /// <summary>
        /// If the current token is a word token, this field contains a string giving the characters of the word 
        /// token.
        /// </summary>
        public string sval;

        /// <summary>
        /// After a call to the nextToken method, this field contains the type of the token just read.
        /// </summary>
        public int ttype;
        
        #endregion
		
        #region Private Fields

        /// <summary>
        /// Internal constants and fields
        /// </summary>
        private const string TOKEN	 = "Token[";
        private const string NOTHING = "NOTHING";
        private const string NUMBER	 = "number=";
        private const string EOF	 = "EOF";
        private const string EOL	 = "EOL";
        private const string QUOTED	 = "quoted string=";
        private const string LINE	 = "], Line ";
        private const string DASH	 = "-.";
        private const string DOT	 = ".";
		
        private const int TT_NOTHING		= - 4;
		
        private const sbyte ORDINARYCHAR	= 0x00;
        private const sbyte WORDCHAR		= 0x01;
        private const sbyte WHITESPACECHAR	= 0x02;
        private const sbyte COMMENTCHAR		= 0x04;
        private const sbyte QUOTECHAR		= 0x08;
        private const sbyte NUMBERCHAR		= 0x10;
		
        private const int STATE_NEUTRAL		= 0;
        private const int STATE_WORD		= 1;
        private const int STATE_NUMBER1		= 2;
        private const int STATE_NUMBER2		= 3;
        private const int STATE_NUMBER3		= 4;
        private const int STATE_NUMBER4		= 5;
        private const int STATE_STRING		= 6;
        private const int STATE_LINECOMMENT	= 7;
        private const int STATE_DONE_ON_EOL = 8;

        private const int STATE_PROCEED_ON_EOL			= 9;
        private const int STATE_POSSIBLEC_COMMENT		= 10;
        private const int STATE_POSSIBLEC_COMMENT_END	= 11;
        private const int STATE_C_COMMENT				= 12;
        private const int STATE_STRING_ESCAPE_SEQ		= 13;
        private const int STATE_STRING_ESCAPE_SEQ_OCTAL	= 14;
		
        private const int STATE_DONE		= 100;
		
        private sbyte[] attribute			= new sbyte[256];
        private bool eolIsSignificant		= false;
        private bool slashStarComments	= false;
        private bool slashSlashComments	= false;
        private bool lowerCaseMode		= false;
        private bool pushedback			= false;
        private int lineno				= 1;

        private BackStreamReader inReader;
        private BackStringReader inStringReader;
        private BackBinaryReader inStream;
        private StringBuilder buf;
        
        #endregion

        #region Constructors and Destructor
        
        /// <summary>
        /// Creates a StreamToknizerSupport that parses the given string.
        /// </summary>
        /// <param name="reader">The System.IO.StringReader that contains the String to be parsed.</param>
        public StreamTokenizerJ(StringReader reader)
        {
            string s = "";
            for (int i = reader.Read(); i != -1 ; i = reader.Read())
            {
                s += (char) i;
            }
            reader.Close();
            this.inStringReader = new BackStringReader(s);
            this.Initialize();
        }		

        /// <summary>
        /// Creates a StreamTokenizerJ that parses the given stream.
        /// </summary>
        /// <param name="reader">Reader to be parsed.</param>
        public StreamTokenizerJ(StreamReader reader)
        {
            this.inReader = new BackStreamReader(new StreamReader(reader.BaseStream, 
                reader.CurrentEncoding).BaseStream, 2, 
                reader.CurrentEncoding);

            this.Initialize();
        }
		
        /// <summary>
        /// Creates a StreamTokenizerJ that parses the given stream.
        /// </summary>
        /// <param name="stream">Stream to be parsed.</param>
        public StreamTokenizerJ(Stream stream)
        {
            this.inStream = new BackBinaryReader(new BufferedStream(stream), 2);
            
            this.Initialize();
        }
        
        #endregion
		
        #region Public Methods

        /// <summary>
        /// Specified that the character argument starts a single-line comment.
        /// </summary>
        /// <param name="ch">The character.</param>
        public virtual void CommentChar(int ch)
        {
            if (ch >= 0 && ch <= 255)
                this.attribute[ch] = StreamTokenizerJ.COMMENTCHAR;
        }
		
        /// <summary>
        /// Determines whether or not ends of line are treated as tokens.
        /// </summary>
        /// <param name="flag">True indicates that end-of-line characters are separate tokens; False indicates 
        /// that end-of-line characters are white space.</param>
        public virtual void EOLIsSignificant(bool flag)
        {
            this.eolIsSignificant = flag;
        }

        /// <summary>
        /// Return the current line number.
        /// </summary>
        /// <returns>Current line number</returns>
        public virtual int Lineno()
        {
            return this.lineno;
        }
		
        /// <summary>
        /// Determines whether or not word token are automatically lowercased.
        /// </summary>
        /// <param name="flag">True indicates that all word tokens should be lowercased.</param>
        public virtual void LowerCaseMode(bool flag)
        {
            this.lowerCaseMode = flag;
        }
		
        /// <summary>
        /// Parses the next token from the input stream of this tokenizer.
        /// </summary>
        /// <returns>The value of the ttype field.</returns>
        public virtual int NextToken()
        {
            char prevChar = (char) (0);
            char ch = (char) (0);
            char qChar = (char) (0);
            int octalNumber = 0;
            int state;

            if (this.pushedback)
            {
                this.pushedback = false;
                return this.ttype;
            }
			
            this.ttype = StreamTokenizerJ.TT_NOTHING;
            state = StreamTokenizerJ.STATE_NEUTRAL;
            this.nval = 0.0;
            this.sval = null;
            this.buf.Length = 0;

            do 
            {
                int data = this.Read();
                prevChar = ch;
                ch = (char) data;
				
                switch (state)
                {
                    case StreamTokenizerJ.STATE_NEUTRAL:
                    {
                        if (data == - 1)
                        {
                            this.ttype = TT_EOF;
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        else if (ch > 255)
                        {
                            this.buf.Append(ch);
                            this.ttype = StreamTokenizerJ.TT_WORD;
                            state = StreamTokenizerJ.STATE_WORD;
                        }
                        else if (this.attribute[ch] == StreamTokenizerJ.COMMENTCHAR)
                        {
                            state = StreamTokenizerJ.STATE_LINECOMMENT;
                        }
                        else if (this.attribute[ch] == StreamTokenizerJ.WORDCHAR)
                        {
                            this.buf.Append(ch);
                            this.ttype = StreamTokenizerJ.TT_WORD;
                            state = StreamTokenizerJ.STATE_WORD;
                        }
                        else if (this.attribute[ch] == StreamTokenizerJ.NUMBERCHAR)
                        {
                            this.ttype = StreamTokenizerJ.TT_NUMBER;
                            this.buf.Append(ch);
                            if (ch == '-')
                                state = StreamTokenizerJ.STATE_NUMBER1;
                            else if (ch == '.')
                                state = StreamTokenizerJ.STATE_NUMBER3;
                            else
                                state = StreamTokenizerJ.STATE_NUMBER2;
                        }
                        else if (this.attribute[ch] == StreamTokenizerJ.QUOTECHAR)
                        {
                            qChar = ch;
                            this.ttype = ch;
                            state = StreamTokenizerJ.STATE_STRING;
                        }
                        else if ((this.slashSlashComments || this.slashStarComments) && ch == '/')
                            state = StreamTokenizerJ.STATE_POSSIBLEC_COMMENT;
                        else if (this.attribute[ch] == StreamTokenizerJ.ORDINARYCHAR)
                        {
                            this.ttype = ch;
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        else if (ch == '\n' || ch == '\r')
                        {
                            this.lineno++;
                            if (this.eolIsSignificant)
                            {
                                this.ttype = StreamTokenizerJ.TT_EOL;
                                if (ch == '\n')
                                    state = StreamTokenizerJ.STATE_DONE;
                                else if (ch == '\r')
                                    state = StreamTokenizerJ.STATE_DONE_ON_EOL;
                            }
                            else if (ch == '\r')
                                state = StreamTokenizerJ.STATE_PROCEED_ON_EOL;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_WORD: 
                    {
                        if (this.IsWordChar(data))
                            this.buf.Append(ch);
                        else
                        {
                            if (data != - 1)
                                this.Unread(ch);
                            this.sval = this.buf.ToString();
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_NUMBER1: 
                    {
                        if (data == - 1 || this.attribute[ch] != StreamTokenizerJ.NUMBERCHAR || ch == '-')
                        {
                            if ( this.attribute[ch] == StreamTokenizerJ.COMMENTCHAR && System.Char.IsNumber(ch) )
                            {
                                this.buf.Append(ch);
                                state = StreamTokenizerJ.STATE_NUMBER2;
                            }
                            else
                            {
                                if (data != - 1)
                                    this.Unread(ch);
                                this.ttype = '-';
                                state = StreamTokenizerJ.STATE_DONE;
                            }
                        }
                        else
                        {
                            this.buf.Append(ch);
                            if (ch == '.')
                                state = StreamTokenizerJ.STATE_NUMBER3;
                            else
                                state = StreamTokenizerJ.STATE_NUMBER2;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_NUMBER2: 
                    {
                        if (data == - 1 || this.attribute[ch] != StreamTokenizerJ.NUMBERCHAR || ch == '-')
                        {
                            if (System.Char.IsNumber(ch) && this.attribute[ch] == StreamTokenizerJ.WORDCHAR)
                            {
                                this.buf.Append(ch);
                            }
                            else if (ch == '.' && this.attribute[ch] == StreamTokenizerJ.WHITESPACECHAR)
                            {
                                this.buf.Append(ch);
                            }

                            else if ( (data != -1) && (this.attribute[ch] == StreamTokenizerJ.COMMENTCHAR && System.Char.IsNumber(ch) ))
                            {
                                this.buf.Append(ch);
                            }
                            else
                            {
                                if (data != - 1)
                                    this.Unread(ch);
                                try
                                {
                                    this.nval = System.Double.Parse(this.buf.ToString());
                                }
                                catch (System.FormatException) {}
                                state = StreamTokenizerJ.STATE_DONE;
                            }
                        }
                        else
                        {
                            this.buf.Append(ch);
                            if (ch == '.')
                                state = StreamTokenizerJ.STATE_NUMBER3;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_NUMBER3: 
                    {
                        if (data == - 1 || this.attribute[ch] != StreamTokenizerJ.NUMBERCHAR || ch == '-' || ch == '.')
                        {
                            if ( this.attribute[ch] == StreamTokenizerJ.COMMENTCHAR && System.Char.IsNumber(ch))
                            {
                                this.buf.Append(ch);
                            }
                            else 
                            {
                                if (data != - 1)
                                    this.Unread(ch);
                                string str = this.buf.ToString();
                                if (str.Equals(StreamTokenizerJ.DASH))
                                {
                                    this.Unread('.');
                                    this.ttype = '-';
                                }
                                else if (str.Equals(StreamTokenizerJ.DOT) && !(StreamTokenizerJ.WORDCHAR != this.attribute[prevChar]))
                                    this.ttype = '.';
                                else
                                {
                                    try
                                    {
                                        this.nval = System.Double.Parse(str);
                                    }
                                    catch (System.FormatException){}
                                }
                                state = StreamTokenizerJ.STATE_DONE;
                            }
                        }
                        else
                        {
                            this.buf.Append(ch);
                            state = StreamTokenizerJ.STATE_NUMBER4;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_NUMBER4: 
                    {
                        if (data == - 1 || this.attribute[ch] != StreamTokenizerJ.NUMBERCHAR || ch == '-' || ch == '.')
                        {
                            if (data != - 1)
                                this.Unread(ch);
                            try
                            {
                                this.nval = System.Double.Parse(this.buf.ToString());
                            }
                            catch (System.FormatException) {}
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        else
                            this.buf.Append(ch);
                        break;
                    }
                    case StreamTokenizerJ.STATE_LINECOMMENT: 
                    {
                        if (data == - 1)
                        {
                            this.ttype = StreamTokenizerJ.TT_EOF;
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        else if (ch == '\n' || ch == '\r')
                        {
                            this.Unread(ch);
                            state = StreamTokenizerJ.STATE_NEUTRAL;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_DONE_ON_EOL: 
                    {
                        if (ch != '\n' && data != - 1)
                            this.Unread(ch);
                        state = StreamTokenizerJ.STATE_DONE;
                        break;
                    }
                    case StreamTokenizerJ.STATE_PROCEED_ON_EOL: 
                    {
                        if (ch != '\n' && data != - 1)
                            this.Unread(ch);
                        state = StreamTokenizerJ.STATE_NEUTRAL;
                        break;
                    }
                    case StreamTokenizerJ.STATE_STRING: 
                    {
                        if (data == - 1 || ch == qChar || ch == '\r' || ch == '\n')
                        {
                            this.sval = this.buf.ToString();
                            if (ch == '\r' || ch == '\n')
                                this.Unread(ch);
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        else if (ch == '\\')
                            state = StreamTokenizerJ.STATE_STRING_ESCAPE_SEQ;
                        else
                            this.buf.Append(ch);
                        break;
                    }
                    case StreamTokenizerJ.STATE_STRING_ESCAPE_SEQ: 
                    {
                        if (data == - 1)
                        {
                            this.sval = this.buf.ToString();
                            state = StreamTokenizerJ.STATE_DONE;
                            break;
                        }
						
                        state = StreamTokenizerJ.STATE_STRING;
                        if (ch == 'a')
                            this.buf.Append(0x7);
                        else if (ch == 'b')
                            this.buf.Append('\b');
                        else if (ch == 'f')
                            this.buf.Append(0xC);
                        else if (ch == 'n')
                            this.buf.Append('\n');
                        else if (ch == 'r')
                            this.buf.Append('\r');
                        else if (ch == 't')
                            this.buf.Append('\t');
                        else if (ch == 'v')
                            this.buf.Append(0xB);
                        else if (ch >= '0' && ch <= '7')
                        {
                            octalNumber = ch - '0';
                            state = StreamTokenizerJ.STATE_STRING_ESCAPE_SEQ_OCTAL;
                        }
                        else
                            this.buf.Append(ch);
                        break;
                    }
                    case StreamTokenizerJ.STATE_STRING_ESCAPE_SEQ_OCTAL: 
                    {
                        if (data == - 1 || ch < '0' || ch > '7')
                        {
                            this.buf.Append((char) octalNumber);
                            if (data == - 1)
                            {
                                this.sval = buf.ToString();
                                state = StreamTokenizerJ.STATE_DONE;
                            }
                            else
                            {
                                this.Unread(ch);
                                state = StreamTokenizerJ.STATE_STRING;
                            }
                        }
                        else
                        {
                            int temp = octalNumber * 8 + (ch - '0');
                            if (temp < 256)
                                octalNumber = temp;
                            else
                            {
                                buf.Append((char) octalNumber);
                                buf.Append(ch);
                                state = StreamTokenizerJ.STATE_STRING;
                            }
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_POSSIBLEC_COMMENT: 
                    {
                        if (ch == '*')
                            state = StreamTokenizerJ.STATE_C_COMMENT;
                        else if (ch == '/')
                            state = StreamTokenizerJ.STATE_LINECOMMENT;
                        else
                        {
                            if (data != - 1)
                                this.Unread(ch);
                            this.ttype = '/';
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_C_COMMENT: 
                    {
                        if (ch == '*')
                            state = StreamTokenizerJ.STATE_POSSIBLEC_COMMENT_END;
                        if (ch == '\n')
                            this.lineno++;
                        else if (data == - 1)
                        {
                            this.ttype = StreamTokenizerJ.TT_EOF;
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        break;
                    }
                    case StreamTokenizerJ.STATE_POSSIBLEC_COMMENT_END: 
                    {
                        if (data == - 1)
                        {
                            this.ttype = StreamTokenizerJ.TT_EOF;
                            state = StreamTokenizerJ.STATE_DONE;
                        }
                        else if (ch == '/')
                            state = StreamTokenizerJ.STATE_NEUTRAL;
                        else if (ch != '*')
                            state = StreamTokenizerJ.STATE_C_COMMENT;
                        break;
                    }
                }
            }
            while (state != StreamTokenizerJ.STATE_DONE);
			
            if (this.ttype == StreamTokenizerJ.TT_WORD && this.lowerCaseMode)
                this.sval = this.sval.ToLower();

            return this.ttype;
        }

        /// <summary>
        /// Specifies that the character argument is "ordinary" in this tokenizer.
        /// </summary>
        /// <param name="ch">The character.</param>
        public virtual void OrdinaryChar(int ch)
        {
            if (ch >= 0 && ch <= 255)
                this.attribute[ch] = StreamTokenizerJ.ORDINARYCHAR;
        }
		
        /// <summary>
        /// Specifies that all characters c in the range low less-equal c less-equal high are "ordinary" in this 
        /// tokenizer.
        /// </summary>
        /// <param name="low">Low end of the range.</param>
        /// <param name="hi">High end of the range.</param>
        public virtual void OrdinaryChars(int low, int hi)
        {
            this.SetAttributes(low, hi, StreamTokenizerJ.ORDINARYCHAR);
        }
		
        /// <summary>
        /// Specifies that numbers should be parsed by this tokenizer.
        /// </summary>
        public virtual void ParseNumbers()
        {
            for (int i = '0'; i <= '9'; i++)
                this.attribute[i] = StreamTokenizerJ.NUMBERCHAR;
            this.attribute['.'] = StreamTokenizerJ.NUMBERCHAR;
            this.attribute['-'] = StreamTokenizerJ.NUMBERCHAR;
        }
		
        /// <summary>
        /// Causes the next call to the nextToken method of this tokenizer to return the current value in the 
        /// ttype field, and not to modify the value in the nval or sval field.
        /// </summary>
        public virtual void PushBack()
        {
            if (this.ttype != StreamTokenizerJ.TT_NOTHING)
                this.pushedback = true;
        }

        /// <summary>
        /// Specifies that matching pairs of this character delimit string constants in this tokenizer.
        /// </summary>
        /// <param name="ch">The character.</param>
        public virtual void QuoteChar(int ch)
        {
            if (ch >= 0 && ch <= 255)
                this.attribute[ch] = QUOTECHAR;
        }
		
        /// <summary>
        /// Resets this tokenizer's syntax table so that all characters are "ordinary." See the ordinaryChar 
        /// method for more information on a character being ordinary.
        /// </summary>
        public virtual void ResetSyntax()
        {
            this.OrdinaryChars(0x00, 0xff);
        }
		
        /// <summary>
        /// Determines whether or not the tokenizer recognizes C++-style comments.
        /// </summary>
        /// <param name="flag">True indicates to recognize and ignore C++-style comments.</param>
        public virtual void SlashSlashComments(bool flag)
        {
            this.slashSlashComments = flag;
        }
		
        /// <summary>
        /// Determines whether or not the tokenizer recognizes C-style comments.
        /// </summary>
        /// <param name="flag">True indicates to recognize and ignore C-style comments.</param>
        public virtual void SlashStarComments(bool flag)
        {
            this.slashStarComments = flag;
        }
		
        /// <summary>
        /// Returns the string representation of the current stream token.
        /// </summary>
        /// <returns>A String representation of the current stream token.</returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder(StreamTokenizerJ.TOKEN);
			
            switch (this.ttype)
            {
                case StreamTokenizerJ.TT_NOTHING:
                {
                    buffer.Append(StreamTokenizerJ.NOTHING);
                    break;
                }
                case StreamTokenizerJ.TT_WORD: 
                {
                    buffer.Append(this.sval);
                    break;
                }
                case StreamTokenizerJ.TT_NUMBER: 
                {
                    buffer.Append(StreamTokenizerJ.NUMBER);
                    buffer.Append(this.nval);
                    break;
                }
                case StreamTokenizerJ.TT_EOF: 
                {
                    buffer.Append(StreamTokenizerJ.EOF);
                    break;
                }
                case StreamTokenizerJ.TT_EOL: 
                {
                    buffer.Append(StreamTokenizerJ.EOL);
                    break;
                }
            }

            if (this.ttype > 0)
            {
                if (this.attribute[this.ttype] == StreamTokenizerJ.QUOTECHAR)
                {
                    buffer.Append(StreamTokenizerJ.QUOTED);
                    buffer.Append(this.sval);
                }
                else
                {
                    buffer.Append('\'');
                    buffer.Append((char) this.ttype);
                    buffer.Append('\'');
                }
            }

            buffer.Append(StreamTokenizerJ.LINE);
            buffer.Append(this.lineno);
            return buffer.ToString();
        }

        /// <summary>
        /// Specifies that all characters c in the range low less-equal c less-equal high are white space 
        /// characters.
        /// </summary>
        /// <param name="low">The low end of the range.</param>
        /// <param name="hi">The high end of the range.</param>
        public virtual void WhitespaceChars(int low, int hi)
        {
            this.SetAttributes(low, hi, StreamTokenizerJ.WHITESPACECHAR);
        }
		
        /// <summary>
        /// Specifies that all characters c in the range low less-equal c less-equal high are word constituents.
        /// </summary>
        /// <param name="low">The low end of the range.</param>
        /// <param name="hi">The high end of the range.</param>
        public virtual void WordChars(int low, int hi)
        {
            this.SetAttributes(low, hi, StreamTokenizerJ.WORDCHAR);
        }
        
        #endregion
             
        #region Private Methods

        /// <summary>
        /// Internal methods
        /// </summary>
        private int Read()
        {
            if (this.inReader != null)
                return this.inReader.Read();
            else if (this.inStream != null)
                return this.inStream.Read();
            else
                return this.inStringReader.Read();
        }
		
        private void Unread(int ch)
        {
            if (this.inReader != null) 
                this.inReader.UnRead(ch);
            else if (this.inStream != null)
                this.inStream.UnRead(ch);
            else
                this.inStringReader.UnRead(ch);
        }

        private void Initialize()
        {
            this.buf = new StringBuilder();
            this.ttype = StreamTokenizerJ.TT_NOTHING;
			
            this.WordChars('A', 'Z');
            this.WordChars('a', 'z');
            this.WordChars(160, 255);
            this.WhitespaceChars(0x00, 0x20);
            this.CommentChar('/');
            this.QuoteChar('\'');
            this.QuoteChar('\"');
            this.ParseNumbers();
        }

        private void SetAttributes(int low, int hi, sbyte attrib)
        {
            int l = System.Math.Max(0, low);
            int h = System.Math.Min(255, hi);
            for (int i = l; i <= h; i++)
                this.attribute[i] = attrib;
        }
		
        private bool IsWordChar(int data)
        {
            char ch = (char) data;
            return (data != - 1 && (ch > 255 || this.attribute[ch] == StreamTokenizerJ.WORDCHAR || this.attribute[ch] == StreamTokenizerJ.NUMBERCHAR));
        }
        
        #endregion
    }
}
