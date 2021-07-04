using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntRunner.Utility;
using AntRunner.Entity;
using AntRunner.Cameras;
using AntRunner.Menu;
using AntRunner.Particles;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework;

using PriorityQueueLib;

namespace AntRunner.Utility
{
    /// <summary>
    /// Based on the aigamedev.com article on Theta* pathfinding
    /// </summary>
    public class ThetaStarPath : Pathfinding
    {
        #region Members
        // Heap containers for the open and closed list.
        private PriorityQueue<PathNode> m_OpenList = new PriorityQueue<PathNode>();
        private PriorityQueue<PathNode> m_ClosedList = new PriorityQueue<PathNode>();

        private List<PathNode> m_Path = new List<PathNode>();
        private Level m_Level = null;

        private PathNode[,] m_NodeGrid = null;

        private PathNode m_Start = null;
        private PathNode m_Goal = null;
        private PathNode m_Current = null;
        private PathNode m_Last = null;
        private PathNode m_Neighbour = null;
        #endregion

        #region Properties
        public List<PathNode> Path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }
        #endregion

        #region Constructors
        public ThetaStarPath(Level pLevel, Entity.Entity pOwner) : base(pLevel, pOwner)
        {
            m_Level = pLevel;

            this.m_PathfindingName = "Theta*";

            // Used for storing neighbour information
            m_NodeGrid = new PathNode[m_Level.TMXLevel.Width,m_Level.TMXLevel.Height];

            for (int x = 0; x < m_NodeGrid.GetLength(0); x++)
            {
                for (int y = 0; y < m_NodeGrid.GetLength(1); y++)
                {
                    m_NodeGrid[x, y] = new PathNode()
                    {
                        f = 0,
                        g = 0,
                        h = 0,
                        position = new Point(x, y),
                        visited = false,
                        isOpenList = false,
                        isClosedList = false,
                        parent = null
                    };
                }
            }

            m_DiagCost = 2;
            m_StraightCost = 1;
        }
        #endregion

        public override void UpdateGoal(Point pGoal)
        {
            m_Goal = new PathNode()
            {
                position = pGoal,
                g = 0,
                h = 0,
                f = 0
            };
        }

        public override void UpdateStart(Point pStart)
        {
            m_Start = new PathNode()
            {
                position = pStart,
                g = 0,
                f = 0,
                h = 0
            };
        }

        /// <summary>
        /// Set up the pathfinding object and make sure that there is a start and an end to the path
        /// </summary>
        /// <param name="pStart">The start poitn</param>
        /// <param name="pEnd">The end point</param>
        public void Initialize(Point pStart, Point pEnd)
        {
            // Set the costs that are to be used
            m_DiagCost = 2;
            m_StraightCost = 1;

            this.m_Configuration = PathConfiguration.Euclidean;

            // Set the new positions.
            m_Start = new PathNode() { position = pStart, f = 0, g = 0, h= 0 };
            m_Start.parent = m_Start;

            m_Goal = new PathNode() { position = pEnd, h = 0, f = 0, g = 0};
            m_Goal.parent = m_Goal;
            
            // Return the euclidean distance (true distance) from the starting node to the goal node.
            m_Start.h = EuclideanDistance(m_Start);
            m_Start.f = m_Start.h;
            m_Start.Recalculate();
        }

        /// <summary>
        /// Temporary fix for now -- while I sort out the priority queues.
        /// </summary>
        /// <returns>Returns the path node with the lowest F score.</returns>
        public PathNode GetLowestF()
        {
            PathNode _lowest = null;
            int _index = 0;

            // Loop through the list, if the item is null, then make it the first one
            // otherwise carry on as normal.
            for (int i = 0; i < m_OpenList.Data.Count; i++)
            {
                if (_lowest == null)
                {
                    _lowest = m_OpenList.Data[i];
                    _index = i;
                }
                else if (_lowest.f > m_OpenList.Data[i].f)
                {
                    _lowest = m_OpenList.Data[i];
                    _index = i;
                }
            }

            m_OpenList.Data.RemoveAt(_index);

            return _lowest;
        }

        /// <summary>
        /// Remove the particular node from the open list
        /// </summary>
        /// <param name="pOther">The node that we are aiming to remove.</param>
        public void RemoveFromOpenList(PathNode pOther)
        {
            int _index = 0;
            for (int i = 0; i < m_OpenList.Data.Count; i++)
            {
                if (m_OpenList.Data[i].position.X == pOther.position.X &&
                    m_OpenList.Data[i].position.Y == pOther.position.Y)
                {
                    _index = i;
                    break;
                }
            }

            // Remove the item at the specified index.
            m_OpenList.Data.RemoveAt(_index);
        }

        /// <summary>
        /// Return the G cost between one node to another
        /// </summary>
        /// <param name="pA">Point from</param>
        /// <param name="pB">Point to where we are heading</param>
        /// <returns>Returns the appropriate G cost based on the position of either nodes.</returns>
        public override int DistanceBetween(PathNode pA, PathNode pB)
        {
            if ((pA.position.X != pB.position.X) ||
                (pA.position.Y != pB.position.Y))
            {
                return m_DiagCost;
            }
            else
            {
                return m_StraightCost;
            }
        }

        /// <summary>
        /// Regenerate the path based on the start and goal values that are stored in the object.
        /// </summary>
        public override bool Replan()
        {

            this.m_ReplanTimeBegin = Environment.TickCount;
            // Make sure that the appropriate values have been set first.
            if (m_Start != null && 
                m_Goal != null)
            {
                // Reset the open and closed lists.
                m_ClosedList.Clear();
                m_OpenList.Clear();

                double g = 0, h = 0, f = 0;

                m_Current = m_Start;

                // Keep looping until we have met the end of the goal
                while (m_Current.position != m_Goal.position)
                {
                    /// Loop through the neighbouring 8 nodes determine the shortest path using ray traces.
                    for (int x = (m_Current.position.X - 1); x < (m_Current.position.X + 2); x++)
                    {
                        for (int y = (m_Current.position.Y - 1); y < (m_Current.position.Y + 2); y++)
                        {
                            /// Skip if the current node is not considered to be clear
                            /// or if it's the current node we are looking at
                            if (!IsClear(x, y) || 
                                (x == m_Current.position.X && y == m_Current.position.Y))
                            {
                                continue;
                            }

                            // Store a copy of the last node that was used.
                            m_Last = m_Current;

                            #region Refactored Code
                            //// Determine first that the node that we are focusing on isn't in the node 
                            //if (!IsInOpenList(m_NodeGrid[x, y]) && !IsInClosedList(m_NodeGrid[x, y]))
                            //{
                            //    // Check to see if there is any kind of collision in between
                            //    if (RayTrace(m_Current.parent.position, new Point(x, y)))
                            //    {
                            //        m_NodeGrid[x, y].parent = m_Current.parent;
                            //        m_NodeGrid[x, y].h = EuclideanDistance(m_NodeGrid[x, y]);
                            //        m_NodeGrid[x, y].g = m_Current.parent.g + EuclideanDistance(m_Current.parent, m_NodeGrid[x, y]);
                            //        m_NodeGrid[x, y].Recalculate();
                            //    }
                            //    else
                            //    {
                            //        // Generate the temporary cost
                            //        int _tempCost = (m_Current.position.X != m_NodeGrid[x, y].position.X || m_Current.position.Y != m_NodeGrid[x, y].position.Y) ? m_DiagCost : m_StraightCost;
                            //        m_NodeGrid[x, y].h = EuclideanDistance(m_NodeGrid[x, y]);
                            //        m_NodeGrid[x, y].g = m_Current.g + _tempCost;
                            //        m_NodeGrid[x, y].Recalculate();
                            //        m_NodeGrid[x, y].parent = m_Current;
                            //    }

                            //    // Once we've modified the values, add it to the open list.
                            //    m_OpenList.Add(m_NodeGrid[x, y]);
                            //}
                            //else // Re-adjust the parent of the node anyway just in case the current G values are larger than coming from this parent.
                            //{
                            //    // Determine if there is a hit between the two points
                            //    if (RayTrace(m_Current.parent.position, new Point(x, y)))
                            //    {
                            //        double _cost = (m_Current.parent.g + EuclideanDistance(m_Current.parent, m_NodeGrid[x, y]) + EuclideanDistance(m_NodeGrid[x, y]));

                            //        if (_cost < m_NodeGrid[x, y].f)
                            //        {
                            //            m_NodeGrid[x, y].g = m_Current.parent.g + EuclideanDistance(m_Current.parent);
                            //            m_NodeGrid[x, y].h = EuclideanDistance(m_NodeGrid[x, y]);
                            //            m_NodeGrid[x, y].parent = m_Current.parent;
                            //            m_NodeGrid[x, y].Recalculate();
                            //        }
                            //    }
                            //    else
                            //    {
                            //        double _cost = (m_Current.g + DistanceBetween(m_Current, m_NodeGrid[x, y]) + EuclideanDistance(m_NodeGrid[x, y]));
                                    
                            //        // Alter the parent for a shorter path if this is the case.
                            //        if (_cost < m_NodeGrid[x, y].f)
                            //        {
                            //            m_NodeGrid[x, y].g = DistanceBetween(m_Current, m_NodeGrid[x, y]) + m_Current.g;
                            //            m_NodeGrid[x, y].parent = m_Current;
                            //            m_NodeGrid[x, y].h = EuclideanDistance(m_NodeGrid[x, y]);
                            //            m_NodeGrid[x, y].Recalculate(); // regenerate the f score here.
                            //        }
                            //    }

                            //}
                            #endregion

                            #region Current Code
                            /// Return whether or not there is a blockage between the neighbouring node and the parent
                            /// of the current node we are looking at
                            if (RayTrace(m_Current.parent.position, new Point(x, y)))
                            {
                                g = m_Current.parent.g + EuclideanDistance(m_Current.parent, m_NodeGrid[x, y]);
                                h = EuclideanDistance(m_NodeGrid[x, y]);
                                f = g + h;

                                m_Last = m_Current.parent;
                            }
                            else
                            {
                                g = m_Current.g + DistanceBetween(m_Current, m_NodeGrid[x, y]);
                                h = EuclideanDistance(m_NodeGrid[x, y]);
                                f = g + h;
                            }

                            /// If it's not in the open or closed list, then we'll add it to the open list.
                            if (!IsInOpenList(m_NodeGrid[x, y]) && !IsInClosedList(m_NodeGrid[x, y]))
                            {
                                m_NodeGrid[x, y].g = g;
                                m_NodeGrid[x, y].h = h;
                                m_NodeGrid[x, y].Recalculate();

                                m_NodeGrid[x, y].parent = m_Last;

                                // Add this node to the open list
                                m_OpenList.Add(m_NodeGrid[x, y]);
                            }
                            else
                            {
                                // If the cumulative G value between the current node we are observing and the neighbouring node
                                // then use that instead and apply the appropriate parent also.
                                if (m_NodeGrid[x, y].f > f)
                                {
                                    m_NodeGrid[x, y].g = g;
                                    m_NodeGrid[x, y].h = h;
                                    m_NodeGrid[x, y].Recalculate();
                                    m_NodeGrid[x, y].parent = m_Last;
                                }

                            }
                            #endregion

                            #region Old Code
                            // // Make sure first that the neighbour we are looking at is in neither list
                           //if (!IsInOpenList(m_NodeGrid[x, y]) && !IsInClosedList(m_NodeGrid[x, y]))
                           // {
                           //     // Determine whether or not there is a collision from one point to another
                           //     if (RayTrace(m_Current.parent.position, new Point(x, y)))
                           //     {
                           //         m_NodeGrid[x, y].g = m_Current.parent.g + (int)EuclideanDistance(m_Current.parent, m_NodeGrid[x, y]);
                           //         m_NodeGrid[x, y].h = (int)EuclideanDistance(m_NodeGrid[x, y]);
                           //         m_NodeGrid[x, y].Recalculate(); // generate F values.
                           //         m_NodeGrid[x,y].parent = m_Current.parent;
                           //         m_Last = m_Current.parent;
                           //     }
                           //     else
                           //     {
                           //         // Add the values of the g together again
                           //         m_NodeGrid[x, y].g = m_Current.g + DistanceBetween(m_Current, m_NodeGrid[x, y]);
                           //         m_NodeGrid[x, y].h = (int)EuclideanDistance(m_NodeGrid[x, y]);
                           //         m_NodeGrid[x, y].Recalculate();
                           //         m_NodeGrid[x, y].parent = m_Current;
                           //     }

                           //     // Add the item to the open list
                           //     m_OpenList.Add(m_NodeGrid[x, y]);
                           // }
                           // else
                           // {
                           //     // Discover if it is shorter for this node to go from the current node we are looking at.
                           //     if (m_NodeGrid[x, y].g > m_NodeGrid[x, y].g + DistanceBetween(m_Current, m_NodeGrid[x,y]))
                           //     {
                           //         m_NodeGrid[x, y].g = DistanceBetween(m_Current, m_NodeGrid[x, y]);
                           //         m_NodeGrid[x, y].h = (int)EuclideanDistance(m_NodeGrid[x, y]);
                           //         m_NodeGrid[x, y].parent = m_Current;
                           //         m_NodeGrid[x, y].Recalculate();
                           //     }
                            // }

                            #endregion
                        }   
                    }

                    // Add this node to the closed list and consider it explored.
                    m_ClosedList.Add(m_Current);
                    m_Current = GetLowestF();
                }

                ConstructPath(m_Current);
            }

            m_ReplanTimeTaken = Environment.TickCount - m_ReplanTimeBegin;

            return true;
        }

        /// <summary>
        /// Return whether or not the provided no is in the open list
        /// </summary>
        /// <param name="pOther">The node that we are checking for</param>
        /// <returns>Whether or not it is open.</returns>
        public bool IsOpen(PathNode pOther)
        {
            return m_OpenList.Data.Contains(pOther);
        }

        /// <summary>
        /// Alternate entry point to the method in question
        /// </summary>
        /// <param name="pOther">The point that we are verifying in being in the open list.</param>
        /// <returns></returns>
        public bool IsInOpenList(Point pOther)
        {
            foreach (var item in m_OpenList.Data)
            {
                if (item.position.X == pOther.X &&
                    item.position.Y == pOther.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsInClosedList(Point pOther)
        {
            // Loop through all the nodes and determine if that is the right node.
            foreach (var item in m_ClosedList.Data)
            {
                if (item.position.X == pOther.X && 
                    item.position.Y == pOther.Y)
                {
                    return true;
                }
            }

            return false;
        }

        /** Again another hacky fix. **/
        public bool IsInOpenList(PathNode pOther)
        {
            return IsInOpenList(pOther.position);
        }

        /** Again another hacky fix for figuring out whether or not the provided node is in the closed list **/
        public bool IsInClosedList(PathNode pOther)
        {
            return IsInClosedList(pOther.position);
        }

        /// <summary>
        /// This should use IEquatable to determine whether or not the node is contained in the list
        /// </summary>
        /// <param name="pOther">The node that we are checking to see whether or not it is in the list</param>
        /// <returns>Whether or not it is in the list.</returns>
        public bool IsClosed(PathNode pOther)
        {
            return m_ClosedList.Data.Contains(pOther);
        }

        /// <summary>
        /// Generate a path recursively from one path node and go from there.
        /// </summary>
        /// <returns>Return a list of path nodes that is considered as the path.</returns>
        public List<PathNode> ConstructPath()
        {
            List<PathNode> _returnList = new List<PathNode>();

            // The node that we are looking at as we recurisvely go back
            PathNode _current = m_Goal;
            _returnList.Add(_current);
            
            while (_current != m_Start)
            {
                _current = _current.parent;
                _returnList.Insert(0, _current);
            }

            m_Path = _returnList;
            return _returnList;
        }

        /// <summary>
        /// Recursively generate a new path
        /// </summary>
        /// <param name="pOther">The node that we are starting the recursion from</param>
        public void ConstructPath(PathNode pOther)
        {
            // Need to prevent the stack overflow that is occurring here.
            if (pOther.parent != null)
            {
                // Insert the path at the front rather than just adding it to the end.
                m_Path.Insert(0,pOther);
                
                // Prevent the infinite loop that is going on here.
                if (pOther.position != m_Start.position)
                {
                    ConstructPath(pOther.parent);
                }
            }
            else
            {
                return;
            }
        }

        /// <summary>
        /// Brasenhams Line Drawing Algorithm
        /// 
        /// Refer to for pseudo code found at: http://rosettacode.org/wiki/Bitmap/Bresenham's_line_algorithm
        /// </summary>
        /// <param name="pA">The first point</param>
        /// <param name="pB">Second point</param>
        /// <returns>Returns whether or not there was a hit at a given point.</returns>
        public override bool RayTrace(Point pA, Point pB)
        {
            return base.RayTrace(pA, pB);
        }

        /// <summary>
        /// Return whether or not there was a hit from point A to point B
        /// </summary>
        /// <param name="pA">The starting node</param>
        /// <param name="pB">The next node.</param>
        /// <returns>Whether or not there was a hit on the ray trace</returns>
        public bool RayTrace(PathNode pA, PathNode pB)
        {
            return RayTrace(pA.position, pB.position);
        }

        /// <summary>
        /// Detect if there is some kind of collision between the two path nodes.
        /// 
        /// i.e. there is a shorter path in between.
        /// 
        /// This part is in accordance to what is presented by http://aigamedev.com/open/tutorials/theta-star-any-angle-paths/#Ferg:06
        /// 
        /// I believe that this is otherwised considered as Ray-Tracing.
        /// </summary>
        /// <param name="pA">The starting point</param>
        /// <param name="pB">The ending point that we are ray-tracing against</param>
        /// <returns>Returns whether or not there was a hit of sorts.</returns>
        public bool LineOfSight(Point pA, Point pB)
        {
            if (m_Level == null)
                return false;

            int _xIncremental = 0;
            int _yIncremental = 0;

            int _startX = pA.X; 
            int _startY = pA.Y;

            int _endX = pB.X;
            int _endY = pB.Y;

            int _f = 0;

            // Determine the distance between the two points.
            int _distanceX = _endX - _startX;

            int _distanceY = _endY - _startY;

            if (_distanceY < 0)
            {
                _distanceY = -_distanceY;
                _yIncremental = -1;
            }
            else
            {
                _yIncremental = 1;
            }

            if (_distanceX < 0)
            {
                _distanceX = -_distanceX;
                _xIncremental = -1;
            }
            else
            {
                _xIncremental = 1;
            }

            if (_distanceX >= _distanceY)
            {
                while (_startX != _endX)
                {
                    _f += _distanceY;
                    
                    if (_f >= _distanceX)
                    {
                        /// Returns whether or not the given coordinates are considered to be clear at all.
                        if (!m_Level.IsClear(
                            _startX + ((_xIncremental - 1) / 2),
                            _startY + ((_yIncremental - 1) / 2)))
                        {
                            return false;
                        }

                        _startY += _yIncremental;
                        _f -= _distanceX;
                    }

                    if (_f != 0 && 
                        !m_Level.IsClear(
                            _startX + ((_xIncremental - 1) / 2), 
                            _startY + ((_xIncremental - 1) / 2)))
                    {
                        return false;
                    }

                    if (_distanceY == 0 &&
                        !m_Level.IsClear(_startX + ((_xIncremental - 1) / 2), 
                                         _startY) &&
                        !m_Level.IsClear(_startY + ((_yIncremental - 1) / 2), 
                                         _startY - 1))
                    {
                        return false;
                    }

                    _startX += _xIncremental;
                }
            }
            else
            {
                while(_startY != _endY)
                {
                    _f += _distanceX;
                    
                    // Determine if the f score is bigger than the distance on the Y axis.
                    if (_f >= _distanceY)
                    {
                        if (!IsClear(_startX + ((_xIncremental - 1) / 2), 
                                     _startY + ((_yIncremental - 1 / 2))))
                        {
                            return false;
                        }

                        _startX += _xIncremental;
                        _f -= _distanceY;

                        if (_f != 0 && !IsClear(_startX + ((_xIncremental - 1) / 2), 
                                                _startY + ((_yIncremental - 1) / 2)))
                        {
                            return false;
                        }

                        if (_distanceX == 0 && !IsClear(_startX, 
                                                        _startY + ((_yIncremental - 1) / 2)) && 
                                               !IsClear(_startX - 1, 
                                                        _startY + ((_yIncremental - 1) / 2)))
                        {
                            return false;
                        }

                        _startY += _yIncremental;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Return the manhattan distance, although nothing new really has to be done here.
        /// </summary>
        /// <param name="pA">The starting node.</param>
        /// <param name="pB">The ending node.</param>
        /// <returns>Returns the retilinear distance.</returns>
        protected override double ManhattanDistance(PathNode pA, PathNode pB)
        {
            return base.ManhattanDistance(pA, pB);
        }

        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            // Draw lines to all of the nodes so that we can see better the path that 
            // has been generated.
            for (int i = 0; i < m_Path.Count - 1; i++)
            {
                Utility.DebugDraw.DrawDebugLine(new Vector2(m_Path[i].position.X * 64,
                                                            m_Path[i].position.Y * 64),
                                                new Vector2(m_Path[i + 1].position.X * 64,
                                                            m_Path[i + 1].position.Y * 64),
                                                            pSpriteBatch, Color.Red, 1f, false);
            }

            // Loop through the path nodes and render the values explaining them more
            foreach (var item in m_Path)
            {
                // Render the grid at the provided position.
                pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(item.position.X * 64, item.position.Y * 64), Color.White);
                pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"], string.Format("G: {0}", item.g.ToString()), new Vector2(item.position.X * 64, item.position.Y * 64), Color.Blue);
                pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"], string.Format("F: {0}", item.f.ToString()), new Vector2((item.position.X * 64) + 64, item.position.Y * 64), Color.Blue);
                pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"], string.Format("H: {0}", item.h.ToString()), new Vector2((item.position.X * 64) + 64, (item.position.Y * 64) + 64), Color.Blue);
            }
            base.DrawPathfinding(pSpriteBatch);
        }

        /// <summary>
        /// Wrapper function for the other definition in which we use the goal path node to determine
        /// </summary>
        /// <param name="pOther">The other node that we are using to get the euclidean distance</param>
        /// <returns>Returns the distance in the form of a double</returns>
        public double EuclideanDistance(PathNode pOther)
        {
            return EuclideanDistance(pOther, m_Goal);
        }

        protected override double EuclideanDistance(PathNode pA, PathNode pB)
        {
            return base.EuclideanDistance(pA, pB);
        }
        
    }
}
