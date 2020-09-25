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
using System.Collections;

namespace iGeospatial.Geometries.Indexers.SweepLine
{
	/// <summary> 
	/// A sweepline implements a sorted index on a set of intervals.
	/// </summary>
	/// <remarks>
	/// It is used to compute all Overlaps between the interval 
	/// in the index.
	/// </remarks>
	[Serializable]
    internal class SweepLineIndex
	{
        #region Private Fields

		private ArrayList events;
		private bool indexBuilt;
		// statistics information
		private int nOverlaps;
        
        #endregion
		
        #region Constructors and Destructor
		
        public SweepLineIndex()
		{
            events = new ArrayList();
		}
        
        #endregion
		
        #region Public Methods

		public void Add(SweepLineInterval sweepInt)
		{
			SweepLineEvent insertEvent = new SweepLineEvent(sweepInt.Min, null, sweepInt);

            events.Add(insertEvent);

            events.Add(new SweepLineEvent(sweepInt.Max, insertEvent, sweepInt));
		}
		
        public void ComputeOverlaps(ISweepLineOverlapAction action)
        {
            nOverlaps = 0;
            BuildIndex();
			
            for (int i = 0; i < events.Count; i++)
            {
                SweepLineEvent ev = (SweepLineEvent) events[i];
                if (ev.IsInsert)
                {
                    ProcessOverlaps(i, ev.DeleteEventIndex, ev.Interval, action);
                }
            }
        }
        
        #endregion
		
        #region Private Methods

		/// <summary> Because Delete Events have a link to their corresponding Insert event,
		/// it is possible to compute exactly the range of events which must be
		/// compared to a given Insert event object.
		/// </summary>
		private void BuildIndex()
		{
			if (indexBuilt)
				return ;

            events.Sort();
			for (int i = 0; i < events.Count; i++)
			{
				SweepLineEvent ev = (SweepLineEvent) events[i];
				if (ev.IsDelete)
				{
					ev.InsertEvent.DeleteEventIndex = i;
				}
			}
			indexBuilt = true;
		}
		
		private void ProcessOverlaps(int start, int end, 
            SweepLineInterval s0, ISweepLineOverlapAction action)
		{
			/// <summary> Since we might need to test for self-intersections,
			/// include current insert event object in list of event objects to test.
			/// Last index can be skipped, because it must be a Delete event.
			/// </summary>
			for (int i = start; i < end; i++)
			{
				SweepLineEvent ev = (SweepLineEvent) events[i];
				if (ev.IsInsert)
				{
					SweepLineInterval s1 = ev.Interval;
					action.Overlap(s0, s1);
					nOverlaps++;
				}
			}
		}
        
        #endregion
	}
}