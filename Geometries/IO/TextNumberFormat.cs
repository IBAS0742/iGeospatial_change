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
using System.Diagnostics;
using System.Globalization;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Implements number format functions
	/// </summary>
	internal sealed class TextNumberFormat
	{             
		//Enumeration of format types that can be used
		private enum FormatTypes 
		{ 
			General, 
			Number, 
			Currency, 
			Percent 
		};
		
		//Current localization number format infomation
		private NumberFormatInfo numberFormat;
		//Current format type used in the instance
		private FormatTypes numberFormatType;
		//Indicates if grouping is being used
		private bool groupingActivated;
		//Current separator used
		private string separator;
		//Number of maximun digits in the integer portion of the number to represent the number
		private int maxIntDigits;
		//Number of minimum digits in the integer portion of the number to represent the number
		private int minIntDigits;
		//Number of maximun digits in the fraction portion of the number to represent the number
		private int maxFractionDigits;
		//Number of minimum digits in the integer portion of the number to represent the number
		private int minFractionDigits;

		private bool directFormat;

		/// <summary>
		/// Initializes a new instance of the object class with the default values
		/// </summary>
		public TextNumberFormat()
		{
			this.numberFormat      = new NumberFormatInfo();
			this.numberFormatType  = TextNumberFormat.FormatTypes.General;
			this.groupingActivated = true;
			this.separator         = this.GetSeparator(TextNumberFormat.FormatTypes.General );
			this.maxIntDigits      = 127;
			this.minIntDigits      = 1;
			this.maxFractionDigits = 3;
		}

		/// <summary>
		/// Initializes a new instance of the class with the specified number format
		/// and the amount of fractional digits to use
		/// </summary>
		/// <param name="theType">Number format</param>
		/// <param name="digits">Number of fractional digits to use</param>
		private TextNumberFormat(TextNumberFormat.FormatTypes theType, 
            int digits)
		{
			this.numberFormat      = NumberFormatInfo.CurrentInfo;
			this.numberFormatType  = theType;
			this.groupingActivated = true;
			this.separator         = this.GetSeparator(theType);
			this.maxIntDigits      = 127;
			this.minIntDigits      = 1;
			this.maxFractionDigits = 3;
		}

		/// <summary>
		/// Initializes a new instance of the class with the specified number format,
		/// uses the system's culture information,
		/// and assigns the amount of fractional digits to use
		/// </summary>
		/// <param name="theType">Number format</param>
		/// <param name="cultureNumberFormat">Represents information about a specific culture including the number formatting</param>
		/// <param name="digits">Number of fractional digits to use</param>
		private TextNumberFormat(TextNumberFormat.FormatTypes theType, 
            CultureInfo cultureNumberFormat, int digits)
		{
			this.numberFormat      = cultureNumberFormat.NumberFormat;
			this.numberFormatType  = theType;
			this.groupingActivated = true;
			this.separator         = this.GetSeparator(theType);
			this.maxIntDigits      = 127;
			this.minIntDigits      = 1;
			this.maxFractionDigits = 3;
		}

		/// <summary>
		/// Initializes a new instance of the class with the specified number format,
		/// uses the system's culture information,
		/// and assigns the amount of fractional digits to use
		/// </summary>
		/// <param name="theType">Number format</param>
		/// <param name="cultureNumberFormat">Represents information about a specific culture including the number formatting</param>
		/// <param name="digits">Number of fractional digits to use</param>
		private TextNumberFormat(TextNumberFormat.FormatTypes theType, 
            NumberFormatInfo numberFormat, int digits)
		{
			this.numberFormat      = numberFormat;
			this.numberFormatType  = theType;
			this.groupingActivated = true;
			this.separator         = this.GetSeparator(theType);
			this.maxIntDigits      = 127;
			this.minIntDigits      = 1;
			this.maxFractionDigits = 3;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using number representation.
		/// </summary>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberInstance()
		{
			TextNumberFormat instance = new TextNumberFormat(
                TextNumberFormat.FormatTypes.Number, 3);

			return instance;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using currency representation.
		/// </summary>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberCurrencyInstance()
		{
			TextNumberFormat instance = new TextNumberFormat(
                TextNumberFormat.FormatTypes.Currency, 3);

			return instance;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using percent representation.
		/// </summary>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberPercentInstance()
		{
			TextNumberFormat instance = new TextNumberFormat(
                TextNumberFormat.FormatTypes.Percent, 3);

			return instance;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using number representation, it uses the culture format information provided.
		/// </summary>
		/// <param name="culture">Represents information about a specific culture</param>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberInstance(CultureInfo culture)
		{
			TextNumberFormat instance = new TextNumberFormat(
                TextNumberFormat.FormatTypes.Number, culture, 3);

			return instance;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using number representation, it uses the culture format information provided.
		/// </summary>
		/// <param name="culture">Represents information about a specific culture</param>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberInstance(NumberFormatInfo numberinfo)
		{
			TextNumberFormat instance = new TextNumberFormat(
                TextNumberFormat.FormatTypes.Number, numberinfo, 3);

			return instance;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using currency representation, it uses the culture format information provided.
		/// </summary>
		/// <param name="culture">Represents information about a specific culture</param>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberCurrencyInstance(CultureInfo culture)
		{
			TextNumberFormat instance = new TextNumberFormat(TextNumberFormat.FormatTypes.Currency, culture, 3);
			return instance;
		}

		/// <summary>
		/// Returns an initialized instance of the TextNumberFormat object
		/// using percent representation, it uses the culture format information provided.
		/// </summary>
		/// <param name="culture">Represents information about a specific culture</param>
		/// <returns>The object instance</returns>
		public static TextNumberFormat GetTextNumberPercentInstance(CultureInfo culture)
		{
			TextNumberFormat instance = new TextNumberFormat(TextNumberFormat.FormatTypes.Percent, culture, 3);
			return instance;
		}

		/// <summary>
		/// Clones the object instance
		/// </summary>
		/// <returns>The cloned object instance</returns>
		public object Clone()
		{
			return (object)this;
		}

		/// <summary>
		/// Determines if the received object is equal to the
		/// current object instance
		/// </summary>
		/// <param name="textNumberObject">TextNumber instance to Compare</param>
		/// <returns>True or false depending if the two instances are equal</returns>
		public override bool Equals(object textNumberObject)
		{
			return object.Equals((object)this, textNumberObject);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Formats a number with the current formatting parameters
		/// </summary>
		/// <param name="number">Source number to format</param>
		/// <returns>The formatted number string</returns>
		public string FormatDouble(double number)
		{
			if (this.directFormat && this.numberFormat != null)
			{
				return number.ToString(this.numberFormat);
			}

			if (this.groupingActivated)
			{
				return number.ToString(this.GetCurrentFormatString() + this.maxFractionDigits, this.numberFormat);
			}
			else
			{
				return (number.ToString(this.GetCurrentFormatString() + this.maxFractionDigits, this.numberFormat)).Replace(this.separator,"");
			}
		}

		/// <summary>
		/// Formats a number with the current formatting parameters
		/// </summary>
		/// <param name="number">Source number to format</param>
		/// <returns>The formatted number string</returns>
		public string FormatLong(long number)
		{
			if (this.directFormat && this.numberFormat != null)
			{
				return number.ToString(this.numberFormat);
			}

			if (this.groupingActivated)
			{
				return number.ToString(this.GetCurrentFormatString() + this.maxFractionDigits , this.numberFormat);
			}
			else
			{
				return (number.ToString(this.GetCurrentFormatString() + this.maxFractionDigits , this.numberFormat)).Replace(this.separator,"");
			}
		}

		/// <summary>
		/// Gets the list of all supported cultures
		/// </summary>
		/// <returns>An array of type CultureInfo that represents the supported cultures</returns>
		public static CultureInfo[] GetAvailableCultures()
		{
			return CultureInfo.GetCultures(CultureTypes.AllCultures);
		}

		/// <summary>
		/// Obtains the current format representation used
		/// </summary>
		/// <returns>A character representing the string format used</returns>
		private string GetCurrentFormatString()
		{
			string currentFormatString = "n";  //Default value
			switch (this.numberFormatType)
			{
				case TextNumberFormat.FormatTypes.Currency:
					currentFormatString = "c";
					break;

				case TextNumberFormat.FormatTypes.General:
					currentFormatString = "n" + this.numberFormat.NumberDecimalDigits;
					break;

				case TextNumberFormat.FormatTypes.Number:
					currentFormatString = "n" + this.numberFormat.NumberDecimalDigits;
					break;

				case TextNumberFormat.FormatTypes.Percent:
					currentFormatString = "p";
					break;
			}
			return currentFormatString;
		}

		/// <summary>
		/// Retrieves the separator used, depending on the format type specified
		/// </summary>
		/// <param name="numberFormatType">formatType enumarator value to inquire</param>
		/// <returns>The values of character separator used </returns>
		private string GetSeparator(FormatTypes numberFormatType)
		{
			string separatorItem = " ";  //Default Separator

			switch (numberFormatType)
			{
				case TextNumberFormat.FormatTypes.Currency:
					separatorItem = this.numberFormat.CurrencyGroupSeparator;
					break;

				case TextNumberFormat.FormatTypes.General:
					separatorItem = this.numberFormat.NumberGroupSeparator;
					break;

				case TextNumberFormat.FormatTypes.Number:
					separatorItem = this.numberFormat.NumberGroupSeparator;
					break;

				case TextNumberFormat.FormatTypes.Percent:
					separatorItem = this.numberFormat.PercentGroupSeparator;
					break;
			}
			return separatorItem;
		}

		/// <summary>
		/// Boolean value stating if IFormatProvider should be used directly
		/// </summary>
		public bool DirectFormat
		{
			get
			{
				return (this.directFormat);
			}
			set
			{
				this.directFormat = value;
			}
		}

		/// <summary>
		/// Boolean value stating if grouping is used or not
		/// </summary>
		public bool GroupingUsed
		{
			get
			{
				return (this.groupingActivated);
			}
			set
			{
				this.groupingActivated = value;
			}
		}

		/// <summary>
		/// Minimum number of integer digits to use in the number format
		/// </summary>
		public int MinIntDigits
		{
			get
			{
				return this.minIntDigits;
			}
			set
			{
				this.minIntDigits = value;
			}
		}

		/// <summary>
		/// Maximum number of integer digits to use in the number format
		/// </summary>
		public int MaxIntDigits
		{
			get
			{
				return this.maxIntDigits;
			}
			set
			{
				this.maxIntDigits = value;
			}
		}

		/// <summary>
		/// Minimum number of fraction digits to use in the number format
		/// </summary>
		public int MinFractionDigits
		{
			get
			{
				return this.minFractionDigits;
			}
			set
			{
				this.minFractionDigits = value;
			}
		}

		/// <summary>
		/// Maximum number of fraction digits to use in the number format
		/// </summary>
		public int MaxFractionDigits
		{
			get
			{
				return this.maxFractionDigits;
			}
			set
			{
				this.maxFractionDigits = value;
			}
		}
	}

}
