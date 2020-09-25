using System;

using iGeospatial.Coordinates;

namespace iGeospatial.Geometries.Visitors
{
	/// <summary> 
	/// A visitor to <see cref="Geometry"/> elements which can be 
	/// short-circuited by a given condition
	/// </summary>
	[Serializable]
    public abstract class ShortCircuitedGeometryVisitor : IGeometryVisitor
	{
        private bool m_bIsDone;

		protected ShortCircuitedGeometryVisitor()
		{
		}
		
        public abstract bool IsDone
        {
            get;
        }
		
		public virtual void ApplyTo(Geometry geometry)
		{
            if (geometry == null)
            {
                throw new ArgumentNullException("geometry");
            }

            for (int i = 0; i < geometry.NumGeometries && !m_bIsDone; i++)
			{
				Geometry element = geometry.GetGeometry(i);
				if (!element.IsCollection)
				{
					this.Visit(element);
					if (this.IsDone)
					{
						m_bIsDone = true;
						return ;
					}
				}
				else
                {
                    ApplyTo(element);
                }
			}
		}
		
        public abstract void Visit(Geometry element);
    }
}