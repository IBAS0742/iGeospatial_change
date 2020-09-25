using System;

namespace iGeospatial
{
	/// <summary>
	/// Summary description for MathUtil.
	/// </summary>
	internal sealed class MathUtil
	{
		private MathUtil()
		{
		}

        public static double Round(double value)
        {
            return Math.Round(value);
        }
	}
}
