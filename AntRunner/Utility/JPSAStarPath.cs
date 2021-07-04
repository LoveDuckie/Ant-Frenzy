using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using PriorityQueueLib;

namespace AntRunner.Utility
{
    public class JPSAStarPath : Pathfinding
    {
        #region Members
        private PriorityQueue<PathNode> m_OpenList = new PriorityQueue<PathNode>();
        private PriorityQueue<PathNode> m_ClosedList = new PriorityQueue<PathNode>();

        private List<PathNode> m_Pathlist = new List<PathNode>();

        private PathConfiguration m_Configuration = PathConfiguration.Euclidean;

        private PathNode m_Current = null;
        private PathNode[,] m_NodeGrid;
        #endregion

        #region Properties
        public List<PathNode> Pathlist
        {
            get { return m_Pathlist; }
            set { m_Pathlist = value; }
        }
        #endregion

        #region Constructors
        public JPSAStarPath() : base()
        {
            m_StraightCost = 1;
            m_DiagCost = 2;
        }

        public JPSAStarPath(Level pLevel, Entity.Entity pOwner)
            : base(pLevel, pOwner)
        {
            this.m_PathfindingName = "JPS A*";

            this.m_NodeGrid = new PathNode[pLevel.TMXLevel.Width, pLevel.TMXLevel.Height];

            // For determining what kind of heuristic that we want to use
            m_StraightCost = 1;
            m_DiagCost = 2;
            m_Configuration = PathConfiguration.Euclidean;

            // Initialize the variables for the node grid that we are going to be using
            for (int x = 0; x < m_NodeGrid.GetLength(0); x++)
            {
                for (int y = 0; y < m_NodeGrid.GetLength(1); y++)
                {
                    m_NodeGrid[x, y] = new PathNode()
                    {
                        g = 0,
                        h = 0,
                        f = 0,
                        position = new Point(x, y),
                        isOpenList = false,
                        isClosedList = false
                    };
                }
            }
            
        }
        #endregion

        #region Update Goal and Start
        public override void UpdateGoal(Point pGoal)
        {
            base.UpdateGoal(pGoal);
        }

        public override void UpdateStart(Point pStart)
        {

            base.UpdateStart(pStart);
        }
        #endregion

        /// <summary>
        /// For replanning the path based on the start and end goals that are defined
        /// </summary>
        /// <returns>Returns whether or not the replan was successful.</returns>
        public override bool Replan()
        {
            // Clear out the previously used lists if we are doing a replan.
            m_OpenList.Clear();
            m_ClosedList.Clear();
            m_Pathlist.Clear();

            // Return whether or not the null is considered null.
            if (m_Start == null || m_Goal == null)
            {
                return false;
            }

            // For benchmarking
            m_ReplanTimeBegin = Environment.TickCount;

            // Set the default cost values to 0
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].g = 0;
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].h = 0;
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].f = 0;

            // Register that it is a part of the open list
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].isOpenList = true;
            
            m_OpenList.Enqueue(m_Start);

            // Keep looping until we have exhausted all the items in the open list
            while (!m_OpenList.IsEmpty())
            {
                m_Current = m_OpenList.Pop();
                
                // Immediately place into the closed list
                m_NodeGrid[m_Current.position.X, m_Current.position.Y].isClosedList = true;
                m_NodeGrid[m_Current.position.X, m_Current.position.Y].isOpenList = false;
                m_ClosedList.Add(m_NodeGrid[m_Current.position.X, m_Current.position.Y]);

                if (m_Current.position == m_Goal.position)
                {
                    ConstructPath(m_Current);
                    break;
                }

                // Determine the neighbouring nodes based on the pruning method.
                // This method should keep looping as this method will place nodes into the open list.
                IdentifySuccessors(m_Current);
            }

            m_ReplanTimeTaken = Environment.TickCount - m_ReplanTimeBegin;
            return base.Replan();
        }

        #region Function Helpers
        public double GenerateHeuristic(PathNode pA)
        {
            return GenerateHeuristic(pA, m_Goal);
        }

        // Return the heuristic based on the configuration that is set.
        public double GenerateHeuristic(PathNode pA, PathNode pB)
        {
            switch (m_Configuration)
            {
                case PathConfiguration.Euclidean:
                    return EuclideanDistance(pA, pB);

                case PathConfiguration.Manhattan:
                    return ManhattanDistance(pA, pB);
            }

            return 0;
        }
        #endregion

        /// <summary>
        /// Get the neighboring nodes that are relevant.
        /// </summary>
        /// <param name="pCurrent">The node that we are observing</param>
        public void IdentifySuccessors(PathNode pCurrent)
        {
            PathNode _neighbour = null;
            PathNode _jumpnode = null;

            // Fetch the neighbours that we are after.
            var _neighbours = GetNeighbours(pCurrent);

            // Loop through the neighbours determining the heuristics of the nodes.
            for (int i = 0; i < _neighbours.Count; i++)
            {
                // Used for determining whether the pCurrent node is a better parent with a better G score.
                double _tentativeGScore = 0;

                _neighbour = _neighbours[i];
                _jumpnode = Jump(_neighbour.position, pCurrent.position);
                
                if (_jumpnode != null)
                {
                    // If it's already stored in the closed list then forget about it.
                    if (IsClosed(_jumpnode))
                    {
                        continue;
                    }

                    // Combine the G and the EuclideanDistance between the returned jump node and the current node.
                    _tentativeGScore = pCurrent.g + GenerateHeuristic(_jumpnode, pCurrent);

                    // If it's not in the open list nor is the current score of the node
                    // lower than the tentative g score...
                    if (!IsOpen(_jumpnode) || (_tentativeGScore < _jumpnode.g))
                    {
                        _jumpnode.g = _tentativeGScore;

                        // Replace the heuristic of the node if there has been none set for that given node.
                        _jumpnode.h = _jumpnode.h == 0 ? GenerateHeuristic(_jumpnode,pCurrent) : _jumpnode.h;
                        _jumpnode.Recalculate();
                        
                        // Set the current node that we are observing as the parent.
                        _jumpnode.parent = pCurrent;

                        if (!IsOpen(_jumpnode))
                        {
                            m_OpenList.Add(_jumpnode);
                            m_NodeGrid[_jumpnode.position.X,_jumpnode.position.Y].isOpenList = true;
                        }
                    }    
                }

            }

            return;
        }

        /// <summary>
        /// Don't think that this function is necessary, but just for safety I suppose
        /// </summary>
        /// <param name="pOther">The other node that we are focusing on</param>
        public void UpdateNode(PathNode pOther)
        {
            // Loop through the nodes in the open list
            for (int i = 0; i < m_OpenList.Data.Count; i++)
            {
                if (m_OpenList.Data[i].position.X == pOther.position.X &&
                    m_OpenList.Data[i].position.Y == pOther.position.Y)
                {
                    m_OpenList.Data[i].h = pOther.h;
                    m_OpenList.Data[i].f = pOther.f;
                    m_OpenList.Data[i].g = pOther.g;
                    break;
                }
            }
            return;
        }

        /// <summary>
        /// Return all the neighbours for the current node
        /// based on whether or not it has a parent.
        /// </summary>
        /// <param name="pCurrent">The node that we are focusing on</param>
        /// <returns>Returns a list of the nodes.</returns>
        public List<PathNode> GetNeighbours(PathNode pCurrent)
        {
            // List of neighbouring nodes that we are going to bother returning
            List<PathNode> _neighbours = new List<PathNode>();
            int cx = pCurrent.position.X, cy = pCurrent.position.Y;

            // Return all neighbours if the parent of the current node is considered null.
            if (pCurrent.parent == null)
            {
                // Loop through neighbouring nodes to the one that we are looking at.
                for (int x = (pCurrent.position.X - 1); x < (pCurrent.position.X + 2); x++)
                {
                    for (int y = (pCurrent.position.Y - 1); y < (pCurrent.position.Y + 2); y++)
                    {
                        if (pCurrent.position.X == x && pCurrent.position.Y == y)
                            continue;

                        if (IsClear(x, y) && WithinBounds(x, y))
                        {
                            _neighbours.Add(m_NodeGrid[x, y]);
                        }
                    }
                }

                return _neighbours;
            }
            else
            {
                int px = pCurrent.parent.position.X, 
                    py = pCurrent.parent.position.Y;

                /**  
                 * Determine the jump direction that we are heading in based on the
                 * position between the parent and the current node that we are observing
                 **/

               // int directionX = Math.Min(Math.Max(-1,pCurrent.position.X - pCurrent.position.X),1);
                //int directionY = Math.Min(Math.Max(-1,pCurrent.position.Y - pCurrent.position.Y),1);

                // Grab the normalized direction that we are after.
                int dx = (pCurrent.position.X - pCurrent.parent.position.X) / Math.Max(Math.Abs(cx - px),1),
                    dy = (pCurrent.position.Y - pCurrent.parent.position.Y) / Math.Max(Math.Abs(cy - py),1);

                /** Diagonal Search **/
                /** There's a difference on both the Y and X coordinate **/
                if (dx != 0 && dy != 0)
                {
                    if (IsClear(cx, cy + dy))
                    {
                        _neighbours.Add(m_NodeGrid[cx, cy + dy]);
                    }

                    if (IsClear(cx + dx, cy))
                    {
                        _neighbours.Add(m_NodeGrid[cx + dx, cy]);
                    }

                    if (IsClear(cx, cy + dy) || IsClear(cx + dx, cy))
                    {
                        _neighbours.Add(m_NodeGrid[cx + dx, cy + dy]);
                    }

                    if (!IsClear(cx - dx, cy) && IsClear(cx, cy + dy))
                    {
                        _neighbours.Add(m_NodeGrid[cx - dx, cy + dy]);
                    }

                    if (!IsClear(cx, cy - dy) && IsClear(cx + dx, cy))
                    {
                        _neighbours.Add(m_NodeGrid[cx + dx, cy - dy]);
                    }
                }
                else  /** Horizontal Search **/
                {
                    if (dx == 0)
                    {
                        if (IsClear(cx, cy + dy))
                        {
                            if (IsClear(cx, cy + dy))
                            {
                                _neighbours.Add(m_NodeGrid[cx, cy + dy]);
                            }

                            // Check these two directions if the one above is no good.
                            if (!IsClear(cx + 1, cy))
                            {
                                _neighbours.Add(m_NodeGrid[cx + 1, cy + dy]);
                            }

                            if (!IsClear(cx - 1, cy))
                            {
                                _neighbours.Add(m_NodeGrid[cx - 1, cy + dy]);
                            }
                        }
                    }
                    else /** Vertical Search **/
                    {
                        if (IsClear(cx + dx, cy))
                        {
                            if (IsClear(cx + dx, dy))
                            {
                                _neighbours.Add(m_NodeGrid[cx + dx, dy]);
                            }

                            // Check these two directions if the one above is no good.
                            if (!IsClear(cx, cy + 1))
                            {
                                _neighbours.Add(m_NodeGrid[cx + dx, cy + 1]);
                            }

                            if (!IsClear(cx, cy - 1))
                            {
                                _neighbours.Add(m_NodeGrid[cx + dx, cy - 1]);
                            }
                        }
                    }
                }

            }
           
            return _neighbours;
        }


        // Construct the back by recursively going through the parents that were assigned.
        public void ConstructPath(PathNode pOther)
        {
            if (pOther != null)
            {
                m_Pathlist.Insert(0, pOther);

                if (pOther.position != m_Start.position)
                {
                    ConstructPath(pOther.parent);
                }
            }
        }

        #region Helper Functions
        // Output the debugging information regarding the pathfinding
        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            if (Global.PATH_DEBUG)
            {
                foreach (var item in m_OpenList.Data)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(item.position.X * m_Level.TMXLevel.TileWidth, item.position.Y * m_Level.TMXLevel.TileHeight), Color.Red);
                }

                foreach (var item in m_ClosedList.Data)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(item.position.X * m_Level.TMXLevel.TileWidth, item.position.Y * m_Level.TMXLevel.TileHeight), Color.LightBlue);    
                }

                // Loop through the items in the path list and then display them accordingly.
                foreach (var item in m_Pathlist)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(item.position.X * m_Level.TMXLevel.TileWidth, 
                                                                                            item.position.Y * m_Level.TMXLevel.TileHeight), Color.LightGreen);
                }
            }
            
            base.DrawPathfinding(pSpriteBatch);
        }

        public bool IsOpen(PathNode pOther)
        {
            return m_OpenList.Data.Contains(pOther);
        }

        public bool IsClosed(PathNode pOther)
        {
            return m_ClosedList.Data.Contains(pOther);
        }
        #endregion

        public void Initialize(Point pStart, Point pEnd)
        {
            // Generate the new start and goal nodes.
            m_Start = new PathNode()
            {
                isClosedList = false,
                isOpenList = false,
                position = pStart,
                g = 0,
                f = 0,
                h = 0,
                isBezier = false
            };

            m_Goal = new PathNode()
            {
                isClosedList = false,
                isOpenList = false,
                position = pEnd,
                g = 0,
                f = 0,
                h = 0,
                isBezier = false
            };
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        
        /// <summary>
        /// Recursively jump in a given direction until we have discovered a jump point
        /// </summary>
        /// <param name="pStart">The start point for the jump detection</param>
        /// <param name="pEnd">The end point</param>
        /// <param name="pParent">The direction that we are checking to see if it's possible to jump in</param>
        private PathNode Jump(Point pStart, Point pParent)
        {
            // Return the base distance between the two nodes
            int distanceX = pStart.X - pParent.X;
            int distanceY = pStart.Y - pParent.Y;

            int x = pStart.X;
            int y = pStart.Y;

            PathNode _jumpX = null, _jumpY = null;

            // If the starting node for the jump is considered as blocked
            // then return null.
            if (!IsClear(pStart.X, pStart.Y))
            {
                return null;
            }
            
            if ((pStart.X == m_Goal.position.X &&
                 pStart.Y == m_Goal.position.Y))
            {
                return m_NodeGrid[pStart.X, pStart.Y];
            }

            /** Checking for forced neighbours diagonally. **/
            if (distanceX != 0 && distanceY != 0)
            {
                if ((IsClear(x - distanceX, y + distanceY) && !IsClear(x - distanceX, y)) ||
                    (IsClear(x + distanceX, y - distanceY) && !IsClear(x, y - distanceY)))
                {
                    return m_NodeGrid[x, y];
                }
            }
            else
            {
                /** Check for forced neighbours across the horizontal and vertical axis **/
                if (distanceX != 0)
                {
                    if ((IsClear(x + distanceX, y + 1) && !IsClear(x, y + 1)) ||
                        (IsClear(x + distanceX, y - 1) && !IsClear(x, y - 1)))
                    {
                        return m_NodeGrid[x, y];
                    }
                }
                else
                {
                    if ((IsClear(x + 1, y + distanceY) && !IsClear(x + 1, y)) ||
                        (IsClear(x - 1, y + distanceY) && !IsClear(x - 1, y)))
                    {
                        return m_NodeGrid[x, y];
                    }
                }
                
            }
            
            /// If no forced neighbours have to be made then do some jumping...
            if (distanceX != 0 && distanceY != 0)
            {
                _jumpX = Jump(new Point(x + distanceX, y), new Point(x, y));
                _jumpY = Jump(new Point(x, y + distanceY), new Point(x, y));

                // Determine whether either of the jumps returned back null.
                if (_jumpX != null || _jumpY != null)
                {
                    return m_NodeGrid[x,y];
                }
            }

            // And now for the diagonal jump ...
            if (IsClear(x + distanceX, y) || IsClear(x, y + distanceY))
            {
                return Jump(new Point(x + distanceX, y + distanceY), new Point(x, y));
            }
            else
            {
                return null;
            }
        }

        protected override double Diagonal(PathNode pA, PathNode pB)
        {
            return base.Diagonal(pA, pB);
        }
    }
}
