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
using System.Diagnostics;

using iGeospatial.Coordinates;
using iGeospatial.Collections;
using iGeospatial.Collections.Sets;
using iGeospatial.Geometries.Visitors;
using iGeospatial.Geometries.PlanarGraphs;

using iGeospatial.Geometries.Operations.LineMerge;

namespace iGeospatial.Geometries.Operations
{
	/// <summary> Builds a sequence from a set of LineStrings so that
	/// they are ordered end to end.
	/// A sequence is a complete non-repeating list of the linear
	/// components of the input.  Each linestring is oriented
	/// so that identical endpoints are adjacent in the list.
	/// 
	/// The input linestrings may form one or more connected sets.
	/// The input linestrings should be correctly noded, or the results may
	/// not be what is expected.
	/// The output of this method is a single MultiLineString containing the ordered
	/// linestrings in the sequence.
	/// <p>
	/// The sequencing employs the classic <b>Eulerian path</b> graph algorithm.
	/// Since Eulerian paths are not uniquely determined,
	/// further rules are used to
	/// make the computed sequence preserve as much as possible of the input
	/// ordering.
	/// Within a connected subset of lines, the ordering rules are:
	/// <ul>
	/// <li>If there is degree-1 node which is the start
	/// node of an linestring, use that node as the start of the sequence
	/// <li>If there is a degree-1 node which is the end
	/// node of an linestring, use that node as the end of the sequence
	/// <li>If the sequence has no degree-1 nodes, use any node as the start
	/// </ul>
	/// 
	/// <p>
	/// Not all arrangements of lines can be sequenced.
	/// For a connected set of edges in a graph,
	/// Euler's Theorem states that there is a sequence containing each edge once
	/// if and only if there are no more than 2 nodes of odd degree.
	/// If it is not possible to find a sequence, the {@link #isSequenceable} method
	/// will return <see langword="false"/>.
	/// 
	/// </summary>
	public sealed class LineSequencer
	{
        #region Private Fields

        private LineMergeGraph graph;
        private GeometryFactory factory;
        private int lineCount;
		
        private bool isRun;
        private Geometry sequencedGeometry;
        private bool m_bIsSequenceable;
        
        #endregion

        #region Constructors and Destructor

        public LineSequencer()
        {
            graph = new LineMergeGraph();
            // initialize with default, in case no lines are input
            factory = GeometryFactory.GetInstance();
            lineCount = 0;
		
            isRun = false;
            sequencedGeometry = null;
            m_bIsSequenceable = false;
        }
        
        #endregion
		
        #region Public Properties

        /// <summary> 
		/// Tests whether the arrangement of linestrings has a valid
		/// sequence.
		/// </summary>
		/// <returns> <see langword="true"/> if a valid sequence exists.
		/// </returns>
		public bool IsSequenceable
		{
			get
			{
				ComputeSequence();

				return m_bIsSequenceable;
			}
		}
			
		/// <summary> Returns the <see cref="LineString"/> or <see cref="MultiLineString"/>
		/// built by the sequencing process, if one exists.
		/// 
		/// </summary>
		/// <returns> the sequenced linestrings,
		/// or <code>null</code> if a valid sequence does not exist
		/// </returns>
		public Geometry SequencedLineStrings
		{
			get
			{
				ComputeSequence();

				return sequencedGeometry;
			}    			
		}
        
        #endregion

        #region Public Methods

		/// <summary> Tests whether a <see cref="Geometry"/> is sequenced correctly.
		/// <see cref="LineString"/>s are trivially sequenced.
		/// <see cref="MultiLineString"/>s are checked for correct sequencing.
		/// Otherwise, <code>isSequenced</code> is defined
		/// to be <see langword="true"/> for geometries that are not lineal.
		///         
		/// </summary>
		/// <param name="geom">the geometry to test
		/// </param>
		/// <returns> <see langword="true"/> if the geometry is sequenced or is not lineal
		/// </returns>
		public static bool IsSequenced(Geometry geom)
		{
			if (!(geom is MultiLineString))
			{
				return true;
			}
			
			MultiLineString mls = (MultiLineString) geom;
			// the nodes in all subgraphs which have been completely scanned
			//TODO--PAUL 'java.util.TreeSet' was converted to ListSet
            ListSet prevSubgraphNodes = new ListSet();
			
			Coordinate lastNode = null;
			IList currNodes = new ArrayList();
			for (int i = 0; i < mls.NumGeometries; i++)
			{
				LineString line = (LineString) mls.GetGeometry(i);
				Coordinate startNode = line.GetCoordinate(0);
				Coordinate endNode = line.GetCoordinate(line.NumPoints - 1);
				
				// If this linestring is connected to a previous subgraph, 
                // geom is not sequenced
				if (prevSubgraphNodes.Contains(startNode))
					return false;
				if (prevSubgraphNodes.Contains(endNode))
					return false;
				
				if (lastNode != null)
				{
					if (!startNode.Equals(lastNode))
					{
						// start new connected sequence
						prevSubgraphNodes.AddAll(currNodes);
						currNodes.Clear();
					}
				}

				currNodes.Add(startNode);
				currNodes.Add(endNode);
				lastNode = endNode;
			}

			return true;
		}
		
		/// <summary> 
		/// Adds a {@link Collection} of <see cref="Geometry"/>s to be sequenced.
		/// May be called multiple times.
		/// Any dimension of Geometry may be added; the constituent linework will be
		/// extracted.
		/// 
		/// </summary>
		/// <param name="geometries">
		/// A Collection of geometries to add
		/// </param>
		public void Add(ICollection geometries)
		{
			for (IEnumerator i = geometries.GetEnumerator(); i.MoveNext(); )
			{
				Geometry geometry = (Geometry) i.Current;
				Add(geometry);
			}
		}
		/// <summary> Adds a <see cref="Geometry"/> to be sequenced.
		/// May be called multiple times.
		/// Any dimension of Geometry may be added; the constituent linework will be
		/// extracted.
		/// 
		/// </summary>
		/// <param name="geometry">the geometry to add
		/// </param>
		public void Add(Geometry geometry)
		{
			geometry.Apply(new GeometryComponentFilter(this));
		}
        
        #endregion
		
        #region Private Methods

		private void AddLine(LineString lineString)
		{
			if (factory == null)
			{
				this.factory = lineString.Factory;
			}
			graph.AddEdge(lineString);
			lineCount++;
		}
		
		private void ComputeSequence()
		{
			if (isRun)
			{
				return;
			}
			isRun = true;
			
			IList sequences = FindSequences();
			if (sequences == null)
				return ;
			
			sequencedGeometry = BuildSequencedGeometry(sequences);
			m_bIsSequenceable = true;
			
//			int finalLineCount = sequencedGeometry.NumGeometries;

			Debug.Assert(lineCount == sequencedGeometry.NumGeometries, 
                "Lines were missing from result");
			Debug.Assert(sequencedGeometry is LineString || 
                sequencedGeometry is MultiLineString, "Result is not lineal");
		}
		
		private IList FindSequences()
		{
			IList sequences = new ArrayList();
			ConnectedSubgraphFinder csFinder = new ConnectedSubgraphFinder(graph);
			IList subgraphs = csFinder.ConnectedSubgraphs;

            for (IEnumerator i = subgraphs.GetEnumerator(); i.MoveNext(); )
			{
				Subgraph subgraph = (Subgraph) i.Current;
				if (HasSequence(subgraph))
				{
					IList seq = FindSequence(subgraph);
					sequences.Add(seq);
				}
				else
				{
					// if any subgraph cannot be sequenced, abort
					return null;
				}
			}

			return sequences;
		}
		
		/// <summary> 
		/// Tests whether a complete unique path exists in a graph
		/// using Euler's Theorem.
		/// </summary>
		/// <param name="graph">
		/// The subgraph containing the edges.
		/// </param>
		/// <returns> 
		/// <see langword="true"/> if a sequence exists
		/// </returns>
		private bool HasSequence(Subgraph graph)
		{
			int oddDegreeCount = 0;

            for (IEnumerator i = graph.NodeIterator(); i.MoveNext(); )
			{
				Node node = (Node)i.Current;
				if (node.Degree % 2 == 1)
					oddDegreeCount++;
			}

			return (oddDegreeCount <= 2);
		}
		
		private IList FindSequence(Subgraph graph)
		{
			PlanarGraphObject.SetVisited(graph.EdgeIterator(), false);
			
			Node startNode = FindLowestDegreeNode(graph);

            DirectedEdge startDE    = 
                (DirectedEdge)startNode.OutEdges.Iterator().Current;
			DirectedEdge startDESym = startDE.Sym;
			
			IList seq = new ArrayList();
			AddReverseSubpath(startDESym, seq, false);
//			IEnumerator lit = seq.GetEnumerator();
			IEnumerator lit = new BackListEnumerator(seq);
			//TODO--PAUL: Method 'java.util.ListIterator.hasPrevious' was not converted.
			while (lit.MoveNext())
			{
				//TODO--PAUL: Method 'java.util.ListIterator.previous' was not converted.
				DirectedEdge prev = (DirectedEdge) lit.Current;
				DirectedEdge unvisitedOutDE = 
                    FindUnvisitedBestOrientedDE(prev.FromNode);

				if (unvisitedOutDE != null)
					AddReverseSubpath(unvisitedOutDE.Sym, seq, true);
			}
			
			// At this point, we have a valid sequence of graph DirectedEdges, 
            // but it is not necessarily appropriately oriented relative to 
            // the underlying geometry.
			IList orientedSeq = Orient(seq);

			return orientedSeq;
		}
		
		/// <summary> Finds an {@link DirectedEdge} for an unvisited edge (if any),
		/// choosing the dirEdge which preserves orientation, if possible.
		/// 
		/// </summary>
		/// <param name="node">the node to examine
		/// </param>
		/// <returns> the dirEdge found, or <code>null</code> if none were unvisited
		/// </returns>
		private static DirectedEdge FindUnvisitedBestOrientedDE(Node node)
		{
			DirectedEdge wellOrientedDE = null;
			DirectedEdge unvisitedDE = null;

            for (IEnumerator i = node.OutEdges.Iterator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				if (!de.Edge.Visited)
				{
					unvisitedDE = de;
					if (de.EdgeDirection)
						wellOrientedDE = de;
				}
			}
			
            if (wellOrientedDE != null)
				return wellOrientedDE;

			return unvisitedDE;
		}
		
		private void AddReverseSubpath(DirectedEdge de, 
            IList lit, bool expectedClosed)
		{
			// trace an unvisited path *backwards* from this de
			Node endNode = de.ToNode;
			
			Node fromNode = null;
			while (true)
			{
				lit.Add(de.Sym);
				de.Edge.Visited = true;
				fromNode = de.FromNode;
				DirectedEdge unvisitedOutDE = 
                    FindUnvisitedBestOrientedDE(fromNode);
				// this must terminate, since we are continually marking edges as visited
				if (unvisitedOutDE == null)
					break;

				de = unvisitedOutDE.Sym;
			}
			if (expectedClosed)
			{
				// the path should end at the toNode of this de, otherwise we have an error
				Debug.Assert(fromNode == endNode, "path not contiguous");
			}
		}
		
		private static Node FindLowestDegreeNode(Subgraph graph)
		{
			int minDegree      = Int32.MaxValue;
			Node minDegreeNode = null;

            for (IEnumerator i = graph.NodeIterator(); i.MoveNext(); )
			{
				Node node = (Node) i.Current;
				if (minDegreeNode == null || node.Degree < minDegree)
				{
					minDegree = node.Degree;
					minDegreeNode = node;
				}
			}

			return minDegreeNode;
		}
		
		/// <summary> Computes a version of the sequence which is optimally
		/// oriented relative to the underlying geometry.
		/// <p>
		/// Heuristics used are:
		/// <ul>
		/// <li>If the path has a degree-1 node which is the start
		/// node of an linestring, use that node as the start of the sequence
		/// <li>If the path has a degree-1 node which is the end
		/// node of an linestring, use that node as the end of the sequence
		/// <li>If the sequence has no degree-1 nodes, use any node as the start
		/// (NOTE: in this case could orient the sequence according to the majority of the
		/// linestring orientations)
		/// </ul>
		/// 
		/// </summary>
		/// <param name="seq">a List of DirectedEdges
		/// </param>
		/// <returns> a List of DirectedEdges oriented appropriately
		/// </returns>
		private IList Orient(IList seq)
		{
			DirectedEdge startEdge = (DirectedEdge) seq[0];
			DirectedEdge endEdge   = (DirectedEdge) seq[seq.Count - 1];
			Node startNode = startEdge.FromNode;
			Node endNode   = endEdge.ToNode;
			
			bool flipSeq = false;
			bool hasDegree1Node = startNode.Degree == 1 || 
                endNode.Degree == 1;
			
			if (hasDegree1Node)
			{
				bool hasObviousStartNode = false;
				
				// test end edge before start edge, to make result stable
				// (ie. if both are good starts, pick the actual start
				if (endEdge.ToNode.Degree == 1 && 
                    endEdge.EdgeDirection == false)
				{
					hasObviousStartNode = true;
					flipSeq = true;
				}
				if (startEdge.FromNode.Degree == 1 && 
                    startEdge.EdgeDirection == true)
				{
					hasObviousStartNode = true;
					flipSeq = false;
				}
				
				// since there is no obvious start node, use any node of degree 1
				if (!hasObviousStartNode)
				{
					// check if the start node should actually be the end node
					if (startEdge.FromNode.Degree == 1)
						flipSeq = true;
					// if the end node is of degree 1, it is properly the end node
				}
			}
			
			
			// if there is no degree 1 node, just use the sequence as is
			// (Could insert heuristic of taking direction of majority of lines as overall direction)
			
			if (flipSeq)
				return Reverse(seq);

			return seq;
		}
		
		/// <summary> Reverse the sequence.
		/// This requires reversing the order of the dirEdges, and flipping
		/// each dirEdge as well
		/// 
		/// </summary>
		/// <param name="seq">a List of DirectedEdges, in sequential order
		/// </param>
		/// <returns> the reversed sequence
		/// </returns>
		private IList Reverse(IList seq)
		{
			ArrayList newSeq = new ArrayList();

            for (IEnumerator i = seq.GetEnumerator(); i.MoveNext(); )
			{
				DirectedEdge de = (DirectedEdge) i.Current;
				newSeq.Insert(0, de.Sym);
			}

			return newSeq;
		}
		
		/// <summary> 
		/// Builds a geometry (<see cref="LineString"/> or <see cref="MultiLineString"/> )
		/// representing the sequence.
		/// 
		/// </summary>
		/// <param name="sequences">a List of Lists of DirectedEdges with
		/// LineMergeEdges as their parent edges.
		/// </param>
		/// <returns> the sequenced geometry, or <code>null</code> if no sequence exists
		/// </returns>
		private Geometry BuildSequencedGeometry(IList sequences)
		{
			GeometryList lines = new GeometryList();
			
			for (IEnumerator i1 = sequences.GetEnumerator(); i1.MoveNext(); )
			{
				IList seq = (IList) i1.Current;

                for (IEnumerator i2 = seq.GetEnumerator(); i2.MoveNext(); )
				{
					DirectedEdge de = (DirectedEdge) i2.Current;
					LineMergeEdge e = (LineMergeEdge) de.Edge;
					LineString line = e.Line;
					
					LineString lineToAdd = line;
					if (!de.EdgeDirection && !line.IsClosed)
						lineToAdd = Reverse(line);
					
					lines.Add(lineToAdd);
				}
			}

			if (lines.Count == 0)
				return factory.CreateMultiLineString(new LineString[0]);

			return factory.BuildGeometry(lines);
		}
		
		private static LineString Reverse(LineString line)
		{
			ICoordinateList pts = line.Coordinates;
            int len = pts.Count;
			Coordinate[] revPts = new Coordinate[len];
			for (int i = 0; i < len; i++)
			{
				revPts[len - 1 - i] = new Coordinate(pts[i]);
			}

			return line.Factory.CreateLineString(revPts);
		}
        
        #endregion

        #region GeometryComponentFilter Class

        private class GeometryComponentFilter : IGeometryComponentVisitor
        {
            #region Private Fields

            private LineSequencer m_objSequencer;
            
            #endregion
			
            #region Constructors and Destructor

            public GeometryComponentFilter(LineSequencer sequencer)
            {
                m_objSequencer = sequencer;
            }

            #endregion

            public LineSequencer Sequencer
            {
                get
                {
                    return m_objSequencer;
                }
            }

            #region IGeometryComponentVisitor Members

            public void Visit(Geometry component)
            {
                GeometryType geomType = component.GeometryType;

                if (geomType == GeometryType.LineString ||
                    geomType == GeometryType.LinearRing)
                {
                    m_objSequencer.AddLine((LineString) component);
                }
            }
            
            #endregion
        }
        
        #endregion

        #region ReverseIteratorEnumerator Class

        internal class ReverseIteratorEnumerator : IEnumerator
        {
            private ArrayList _list;
            private int       _index;

            internal ReverseIteratorEnumerator( IEnumerator enumerator )
            {
                _list = new ArrayList();
                while ( enumerator.MoveNext() )
                {
                    _list.Add( enumerator.Current );
                }
                _index = _list.Count;
            }

            public void Reset()
            {
                _index = _list.Count;
            }

            public object Current
            {
                get
                {
                    if ( ( _index < 0 ) || ( _index == _list.Count ) )
                        throw new InvalidOperationException();

                    return _list[_index];
                }
            }

            public bool MoveNext()
            {
                if ( _index >= 0 )
                    --_index;

                return _index >= 0;
            }
        }

        internal class BackListEnumerator : IEnumerator 
        { 
            IList _baseList; 
            int _currentIndex; 

            // we can choose to start iteration from a particular index 
            // instead of the last 
            public BackListEnumerator(IList baseList, int startAt) 
            { 
                _baseList = baseList; 
                _currentIndex = startAt; 
            } 

            // by default start at the last item 
            public BackListEnumerator(IList baseList) 
                : this(baseList, baseList.Count) 
            { 
            } 

            // this returns the object at the present position                
            public object Current 
            { 
                get 
                { 
                    return _baseList[_currentIndex]; 
                } 
            } 

            public bool MoveNext() 
            { 
                // if we are at the beginning 
                // then return false, since we can't move back anymore, 
                // else decrement the index and return true 
                if (_currentIndex == 0) 
                { 
                    return false; 
                } 
                else 
                {                         
                    _currentIndex--; 

                    return true; 
                } 
            } 

            public void Reset() 
            {          
                _currentIndex = _baseList.Count; 
            } 
        } 

        internal class BackListEnumerable : IEnumerable 
        { 
            IList _list; 

            public BackListEnumerable(IList list) 
            { 
                _list = list; 
            }                      

            public IEnumerator GetEnumerator()
            {                                     
                return new BackListEnumerator(_list); 
            } 
        } 

        #endregion
    }
}