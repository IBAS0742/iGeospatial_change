using System;

namespace iGeospatial
{
	/// <summary>
	/// Summary description for ProductInfo.
	/// </summary>
	[Serializable]
    public sealed class ProductInfo
	{
		private ProductInfo()
		{
		}

        /// <summary>
        /// Company name
        /// </summary>
        public const string Company   = "iGeospatial Team";

        /// <summary>
        /// Product name
        /// </summary>
        public const string Product   = "iGeospatial GIS Librarty";

        /// <summary>
        /// Copyright
        /// </summary>
        public const string Copyright = "Copyright (C) 2004-2005 iGeospatial Team";

        /// <summary>
        /// Trademark
        /// </summary>
        public const string Trademark = "";

        /// <summary>
        /// Culture - must be empty.
        /// </summary>
        public const string Culture   = "";

        /// <summary>
        /// General release version
        /// </summary>
        public const string Version   = "1.0.0.0";

        /// <summary>
        /// Key file location
        /// </summary>
        public const string KeyFile   = "..\\..\\..\\Open.snk";
    }
}
