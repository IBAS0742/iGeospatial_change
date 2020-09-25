using System;

namespace iGeospatial.Coordinates
{
	/// <summary>
	/// Summary description for Envelope3D.
	/// </summary>
	[Serializable]
    public class Envelope3D : Envelope
	{
		public Envelope3D()
		{
		}

        #region ICloneable Members

        public override Envelope Clone()
        {
            return new Envelope(m_dMinX, m_dMinY, m_dMaxX, m_dMaxY);
        }

        #endregion
    }
}
