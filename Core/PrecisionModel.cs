using System;

namespace iGeospatial
{
	/// <summary> 
	/// Specifies the precision model of the Coordinates in a Geometry.
	/// </summary>
	/// <remarks>
	/// In other words, the precision model specifies the grid of allowable
	/// points for all <see cref="Geometry"/> instances.
	/// <para>
	/// The <see cref="Precision.MakePrecise"/> method allows rounding a coordinate to
	/// a "precise" value; that is, one whose
	/// precision is known exactly.
	/// </para>
	/// Coordinates are assumed to be precise in geometries.
	/// That is, the coordinates are assumed to be rounded to the
	/// precision model given for the geometry.
	/// OTS input routines automatically round coordinates to the precision model
	/// before creating geometries.
	/// <para>
	/// All internal operations assume that coordinates are rounded to the precision model.
	/// Constructive methods (such as boolean operations) always round computed
	/// coordinates to the appropriate precision model.
	/// </para>
	/// Currently three types of precision model are supported:
	/// <list type="number">
	/// <item>
	/// <description>
	/// Floating - represents full double precision floating point.
	/// This is the default precision model used in OTS
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Floating-single - represents single precision floating point.
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// Fixed - represents a model with a fixed number of decimal places.
	/// A Fixed Precision Model is specified by a scale factor.
	/// </description>
	/// </item>
	/// </list>
	/// The scale factor specifies the grid which numbers are rounded to.
	/// Input coordinates are mapped to fixed coordinates according to the following
	/// equations:
	/// <list type="bullet">
	/// <item>
	/// <description>
	/// jtsPt.x = round( (inputPt.x * scale ) / scale
	/// </description>
	/// </item>
	/// <item>
	/// <description>
	/// jtsPt.y = round( (inputPt.y * scale ) / scale
	/// </description>
	/// </item>
	/// </list>
	/// Coordinates are represented internally as .NET double-precision values.
	/// Since .NET uses the IEEE-394 floating point standard, this
	/// provides 53 bits of precision. (Thus the maximum precisely representable
	/// integer is 9,007,199,254,740,992).
	/// OTS methods currently do not handle inputs with different precision models.
	/// </remarks>
	[Serializable]
	public class PrecisionModel : IComparable, ICloneable
	{
		/// <summary>  
		/// The maximum precise value representable in a double. Since IEE754
		/// double-precision numbers allow 53 bits of mantissa, the value is equal to
		/// 2^53 - 1.  This provides almost 16 decimal digits of precision.
		/// </summary>
		public const double maximumPreciseValue = 9007199254740992.0;
		
		/// <summary> The type of PrecisionModel this represents.</summary>
		private PrecisionModelType modelType;
		
        /// <summary> 
        /// The scale factor which determines the number of decimal places in fixed precision.
        /// </summary>
		private double scale;
		
		/// <summary> 
		/// Creates a PrecisionModel with a default precision of FLOATING.
		/// </summary>
		public PrecisionModel()
		{
			// default is floating precision
			modelType = PrecisionModelType.Floating;
		}
		
		/// <summary> Creates a PrecisionModel that specifies
		/// an explicit precision model type.
		/// If the model type is FIXED the scale factor will default to 1.
		/// 
		/// </summary>
		/// <param name="modelType">the type of the precision model
		/// </param>
		public PrecisionModel(PrecisionModelType modelType)
		{
			this.modelType = modelType;
			if (modelType == PrecisionModelType.Fixed)
			{
				this.Scale = 1.0;
			}
		}

		/// <summary>  
		/// Creates a PrecisionModel that specifies Fixed precision.
		/// Fixed-precision coordinates are represented as precise internal coordinates,
		/// which are rounded to the grid defined by the scale factor.
		/// </summary>
		/// <param name="scale">
		/// Amount by which to multiply a coordinate after subtracting the offset, 
		/// to obtain a precise coordinate.
		/// </param>
		public PrecisionModel(double scale)
		{
			modelType  = PrecisionModelType.Fixed;
			this.Scale = scale;
		}

		/// <summary>  
		/// Copy constructor to create a new PrecisionModel from an existing one.
		/// </summary>
		public PrecisionModel(PrecisionModel pm)
		{
			modelType = pm.modelType;
			scale = pm.scale;
		}
		
		/// <summary> Tests whether the precision model supports floating point</summary>
		/// <returns> true if the precision model supports floating point
		/// </returns>
		public virtual bool IsFloating
		{
			get
			{
				return modelType == PrecisionModelType.Floating || 
                    modelType == PrecisionModelType.FloatingSingle;
			}
		}

		/// <summary> Returns the maximum number of significant digits provided by this
		/// precision model.
		/// Intended for use by routines which need to Print out precise values.
		/// 
		/// </summary>
		/// <returns> the maximum number of decimal places provided by this precision model
		/// </returns>
		public virtual int MaximumSignificantDigits
		{
			get
			{
				int maxSigDigits = 16;
				if (modelType == PrecisionModelType.Floating)
				{
					maxSigDigits = 16;
				}
				else if (modelType == PrecisionModelType.FloatingSingle)
				{
					maxSigDigits = 6;
				}
				else if (modelType == PrecisionModelType.Fixed)
				{
					maxSigDigits = 1 + (int) Math.Ceiling(Math.Log(this.Scale) / Math.Log(10));
				}

				return maxSigDigits;
			}
		}
		
		/// <summary>  
		/// Returns the multiplying factor used to obtain a precise coordinate.
		/// This method is private because PrecisionModel is intended to
		/// be an immutable (value) type.
		/// 
		/// </summary>
		/// <value>
		/// The amount by which to multiply a coordinate after 
		/// subtracting the offset
		/// </value>
		public virtual double Scale
		{
            get 
            {
                return scale;
            }

            set 
            {
                scale = Math.Abs(value);
            }
		}
		
		/// <summary> Gets the type of this PrecisionModel</summary>
		/// <returns> the type of this PrecisionModel
		/// </returns>
		public virtual PrecisionModelType ModelType
		{
            get 
            {
                return modelType;
            }
		}
		
		/// <summary> Rounds a numeric value to the PrecisionModel grid.</summary>
		public virtual double MakePrecise(double val)
		{
			if (modelType == PrecisionModelType.FloatingSingle)
			{
                float floatSingleVal = (float) val;

                return (double) floatSingleVal;
			}
			
            if (modelType == PrecisionModelType.Fixed)
			{        
				return Math.Floor(val * scale + 0.5d) / scale;
//				return Math.Round(val * scale) / scale;
			}

			// modelType == FLOATING - no rounding necessary
			return val;
		}
		
		public override System.String ToString()
		{
			System.String description = "UNKNOWN";
			if (modelType == PrecisionModelType.Floating)
			{
				description = "Floating";
			}
			else if (modelType == PrecisionModelType.FloatingSingle)
			{
				description = "Floating-Single";
			}
			else if (modelType == PrecisionModelType.Fixed)
			{
				description = "Fixed (Scale=" + this.Scale + ")";
			}
			
            return description;
		}
		
		public  override bool Equals(object other)
		{
			if (!(other is PrecisionModel))
			{
				return false;
			}
			
            PrecisionModel otherPrecisionModel = (PrecisionModel) other;
			
            return modelType == otherPrecisionModel.modelType && scale == otherPrecisionModel.scale;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>  
		/// Compares this PrecisionModel object with the specified object for order.
		/// A PrecisionModel is greater than another if it provides greater precision.
		/// The comparison is based on the value returned by the
		/// {@link getMaximumSignificantDigits) method.
		/// This comparison is not strictly accurate when comparing floating precision models
		/// to fixed models; however, it is correct when both models are either floating or fixed.
		/// 
		/// </summary>
		/// <param name="o"> the PrecisionModel with which this PrecisionModel
		/// is being compared
		/// </param>
		/// <returns>    a negative integer, zero, or a positive integer as this PrecisionModel
		/// is less than, equal to, or greater than the specified PrecisionModel
		/// </returns>
		public virtual int CompareTo(object o)
		{
			PrecisionModel other = (PrecisionModel) o;
			
			int sigDigits = MaximumSignificantDigits;
			int otherSigDigits = other.MaximumSignificantDigits;

            return (sigDigits).CompareTo(otherSigDigits);
        }

        #region ICloneable Members

        public virtual object Clone()
        {
            return new PrecisionModel(this);
        }

        #endregion
    }
}