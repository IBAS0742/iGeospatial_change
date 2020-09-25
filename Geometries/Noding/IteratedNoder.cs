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

using iGeospatial.Coordinates;
using iGeospatial.Geometries.Algorithms;

namespace iGeospatial.Geometries.Noding
{
	/// <summary> 
	/// Nodes a set of SegmentStrings completely.
	/// </summary>
	/// <remarks>
	/// The set of segmentStrings is fully noded; i.e. noding is repeated until 
	/// no further intersections are detected.
	/// <para>
	/// Iterated noding using a FLOATING precision model is not guaranteed to converge,
	/// due to roundoff error.   This problem is detected and an exception is thrown.
	/// Clients can choose to rerun the noding using a lower precision model.
	/// </para>
	/// </remarks>
	[Serializable]
    internal class IteratedNoder : INoder
	{
        #region Private Fields

        public const int MAX_ITER = 5;
		
        private IList nodedSegStrings;
        private int maxIter = MAX_ITER;
		
        private PrecisionModel pm;
		private LineIntersector li;
        
        #endregion
		
        #region Constructors and Destructor

		public IteratedNoder(PrecisionModel pm)
		{
			li      = new RobustLineIntersector();
			this.pm = pm;
			li.PrecisionModel = pm;
		}
        
        #endregion
		
        #region Public Properties
	
        /// <summary> 
        /// Gets or sets the maximum number of noding iterations performed 
        /// before the noding is aborted.
        /// </summary>
        /// <remarks>
        /// Experience suggests that this should rarely need to be changed
        /// from the default. The default is MAX_ITER.
        /// </remarks>
        /// <value>The maximum number of iterations to perform.</value>
        public int MaximumIterations
        {
            get
            {
                return this.maxIter;
            }
			
            set
            {
                this.maxIter = value;
            }
        }
        
        #endregion

        #region Private Methods

        /// <summary> Node the input segment strings once
        /// and create the split edges between the nodes
        /// </summary>
        private void Node(IList segStrings, int[] numInteriorIntersections)
        {
            IntersectionAdder si = new IntersectionAdder(li);
            MCIndexNoder noder   = new MCIndexNoder();
            noder.SegmentIntersector = si;
            
            noder.ComputeNodes(segStrings);

            nodedSegStrings             = noder.NodedSubstrings;
            numInteriorIntersections[0] = si.numInteriorIntersections;
        }

//		/// <summary> 
//		/// Fully nodes a list of <see cref="SegmentStrings"/>, i.e. peforms noding 
//		/// iteratively until no intersections are found between segments.
//		/// Maintains labelling of edges correctly through the noding.
//		/// </summary>
//		/// <param name="segStrings">A collection of SegmentStrings to be noded.</param>
//		/// <returns>A collection of the noded SegmentStrings.</returns>
//		/// <exception cref="GeometryException">
//		/// If the iterated noding fails to converge.
//		/// </exception>
//		private IList Node(IList segStrings)
//		{
//			int[] numInteriorIntersections = new int[1];
//			IList nodedEdges = segStrings;
//			int nodingIterationCount = 0;
//			int lastNodesCreated = - 1;
//			do 
//			{
//				nodedEdges = Node(nodedEdges, numInteriorIntersections);
//				nodingIterationCount++;
//				int nodesCreated = numInteriorIntersections[0];
//
//				if (lastNodesCreated > 0 && nodesCreated > lastNodesCreated)
//				{
//					throw new GeometryException("Iterated node-attributes failed to converge after " 
//                        + nodingIterationCount + " iterations");
//				}
//				
//				lastNodesCreated = nodesCreated;
//			}
//			while (lastNodesCreated > 0);
//
//            return nodedEdges;
//		}
//		
//		/// <summary> 
//		/// Node the input segment strings once and create the split edges 
//		/// between the nodes.
//		/// </summary>
//		private IList Node(IList segStrings, int[] numInteriorIntersections)
//		{
//			SegmentIntersector si = new SegmentIntersector(li);
//			MonotoneChainQuadtreeNoder noder = new MonotoneChainQuadtreeNoder();
//			noder.SegmentIntersector = si;
//			
//			// perform the noding
//			IList nodedSegStrings = noder.Node(segStrings);
//			numInteriorIntersections[0] = si.numInteriorIntersections;
//
//            return nodedSegStrings;
//        }
        
        #endregion
		
        #region INoder Members

        public IList NodedSubstrings
        {
            get
            {
                return nodedSegStrings;
            }
        }

        public void ComputeNodes(IList segStrings)
        {
            int[] numInteriorIntersections = new int[1];
            nodedSegStrings = segStrings;
            
            int nodingIterationCount = 0;
            
            int lastNodesCreated = - 1;
            do 
            {
                Node(nodedSegStrings, numInteriorIntersections);
                nodingIterationCount++;
                int nodesCreated = numInteriorIntersections[0];
				
                // Fail if the number of nodes created is not declining.
                // However, allow a few iterations at least before doing this
                if (lastNodesCreated > 0 && nodesCreated >= lastNodesCreated && 
                    nodingIterationCount > maxIter)
                {
                    throw new GeometryException("Iterated noding failed to converge after " + nodingIterationCount + " iterations");
                }
                lastNodesCreated = nodesCreated;
            }
            while (lastNodesCreated > 0);
        }

        #endregion
    }
}