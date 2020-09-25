using System;

namespace iGeospatial.Geometries
{
	/// <summary>
	/// Summary description for IIntersectionMatrix.
	/// </summary>
	public interface IIntersectionMatrix : ICloneable
	{
        #region Properties

        bool Within 
        { 
            get; 
        }

        bool Contains 
        { 
            get; 
        }

        bool Disjoint 
        { 
            get; 
        }

        bool Intersects 
        { 
            get; 
        }
        
        #endregion

        #region Methods

        bool IsCrosses(DimensionType geometryA, DimensionType geometryB);
        bool IsEquals(DimensionType geometryA, DimensionType geometryB);
        bool IsOverlaps(DimensionType geometryA, DimensionType geometryB);
        bool IsTouches(DimensionType geometryA, DimensionType geometryB);
        bool Matches(string requiredDimensionSymbols);
        void SetAtLeast(int row, int column, int minimumDimensionValue);
        void SetAtLeast(string minimumDimensionSymbols);
        void SetAtLeastIfValid(int row, int column, int minimumDimensionValue);

        int GetValue(int row, int column);
        void SetValue(int row, int column, int dimensionValue);
        void SetValues(string dimensionSymbols);
        void SetAll(int value);
        
        IIntersectionMatrix Transpose();
        void Add(IIntersectionMatrix im);
        
        new IIntersectionMatrix Clone();
        
        #endregion
    }
}
