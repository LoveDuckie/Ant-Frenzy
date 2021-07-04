using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.States;
using AntRunner.Utility;

using PriorityQueueLib;

namespace AntRunner.Utility
{
    

    public class AStarPathOpt : Pathfinding
    {
        #region Members
        // The open and closed list that will be used also.
        private PriorityQueue<PathNode> m_ClosedList = new PriorityQueue<PathNode>();
        private PriorityQueue<PathNode> m_OpenList = new PriorityQueue<PathNode>();

        // The final path that is to be used.
        private List<PathNode> m_Pathlist = new List<PathNode>();

        private PathNode[,] m_NodeGrid = null;

        private PathNode m_Current = null;

        // Start and end goals that will determine the path.
        private PathNode m_Start = null;
        private PathNode m_Goal = null;
        #endregion

        #region Properties
        public List<PathNode> Pathlist
        {
            get { return m_Pathlist; }
            set { m_Pathlist = value; }
        }
        #endregion

        #region Constructors
        public AStarPathOpt(Level pLevel, Entity.Entity pOwner) : base(pLevel,pOwner)
        {
            // Values required for determining the Manhattan cost during path generation
            //m_StraightCost = 14;
            //m_DiagCost = 10;
            this.m_Configuration = PathConfiguration.Manhattan;

            this.m_StraightCost = 10;
            this.m_DiagCost = 14;

            // Assign the name of the pathfinding method
            this.m_PathfindingName = "A* Optimized";

            this.m_NodeGrid = new PathNode[pLevel.TMXLevel.Width, pLevel.TMXLevel.Height];
            for (int x = 0; x < m_NodeGrid.GetLength(0); x++)
            {
                for (int y = 0; y < m_NodeGrid.GetLength(1); y++)
                {
                    // Create a new pathnode
                    m_NodeGrid[x, y] = new PathNode()
                    {
                        f = 0,
                        g = 0,
                        h = 0,
                        position = new Point(x,y),
                        isBezier = false,
                        isClosedList = false,
                        isOpenList = false,
                        visited = false
                    };
                }
            }
        }
        #endregion

        /// <summary>
        /// Set up the pathfinding object so we know how to generate.
        /// </summary>
        /// <param name="pFrom">The point from</param>
        /// <param name="pTo">The point to</param>
        public void Initialize(Point pFrom, Point pTo)
        {
            // Initialize the start and end points
            m_Start = new PathNode() { position = pFrom, g = 0, f = 0, h = 0, isBezier = false, isOpenList = false, isClosedList = false };
            m_Goal = new PathNode() { position = pTo, g = 0, f = 0, h = 0, isBezier = false, isClosedList = false, isOpenList = false };
        }


        /// <summary>
        /// Return the G cost between two points -- presumes that both points are only one square apart.
        /// </summary>
        /// <param name="pA">The first point </param>
        /// <param name="pB">Second point</param>
        /// <returns>Returns the G cost between points</returns>
        public override int DistanceBetween(PathNode pA, PathNode pB)
        {
            if ((pA.position.X != pB.position.X) &&
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
        /// Called for whether we want to regenerate the path for whatever reason
        /// </summary>
        public override bool Replan()
        {
            m_ReplanTimeBegin = Environment.TickCount;

            // Clear out the lists that we require
            m_OpenList.Clear();
            m_ClosedList.Clear();
            m_Pathlist.Clear();

            m_NodeGrid[m_Start.position.X, m_Start.position.Y].g = 0;
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].h = GenerateHeuristic(m_Start, m_Goal);
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].Recalculate();
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].parent = m_NodeGrid[m_Start.position.X, m_Start.position.Y];

            // Add the starting node to the open list to begin with
            m_NodeGrid[m_Start.position.X, m_Start.position.Y].isOpenList = true;
            m_OpenList.Add(m_NodeGrid[m_Start.position.X, m_Start.position.Y]);

            // Loop until we have completely dealt with it
            while (m_OpenList.Data.Count > 0)
            {
                // Retrieve the path node that has the lowest F score by popping from the top of the list
                PathNode _current = m_OpenList.Pop();
                m_NodeGrid[_current.position.X, _current.position.Y].isOpenList = false;

                if (_current.position == m_Goal.position)
                {
                    ConstructPath(_current);
                    break;
                }

                // Loop through the neighbouring nodes of the one that we are observing right now.
                for (int x = _current.position.X - 1; x < _current.position.X + 2; x++)
                {
                    for (int y = _current.position.Y - 1; y < _current.position.Y + 2; y++)
                    {
                        // Skip if the current item on the map is not clear and is the same as the node that we are observing
                        if (!IsClear(x, y) || (x == _current.position.X && y == _current.position.Y))
                            continue;

                        #region Object Collision Code
                        // Determine whether there is an object in the way
                        if (m_Level.IsObjectAt(x, y, typeof(BlackHole)) ||
                            m_Level.IsObjectAt(x, y, typeof(NewBox)) ||
                            m_Level.IsObjectAt(x, y, typeof(Box)))
                            continue;

                        #endregion

                        // Determine whether or not the item is in the closed list first.
                        if (!IsClosed(m_NodeGrid[x, y]) && !IsOpen(m_NodeGrid[x, y]))
                        {
                            m_NodeGrid[x, y].g = _current.g + DistanceBetween(_current, m_NodeGrid[x, y]);
                            m_NodeGrid[x, y].h = GenerateHeuristic(m_NodeGrid[x, y]);
                            m_NodeGrid[x, y].Recalculate();

                            m_NodeGrid[x, y].parent = _current;

                            m_OpenList.Add(m_NodeGrid[x, y]);
                        }
                        else
                        {
                            // Determine if the current node is a better parent or not.
                            if (m_NodeGrid[x,y].f > ((_current.g + DistanceBetween(_current,m_NodeGrid[x,y])) + GenerateHeuristic(m_NodeGrid[x,y])))
                            {
                                m_NodeGrid[x, y].g = _current.g + DistanceBetween(_current, m_NodeGrid[x, y]);
                                m_NodeGrid[x, y].h = GenerateHeuristic(m_NodeGrid[x,y]);
                                m_NodeGrid[x, y].Recalculate();
                                m_NodeGrid[x, y].parent = _current;
                            }
                        }
                    }
                }

                m_NodeGrid[_current.position.X, _current.position.Y].isClosedList = true;
                m_ClosedList.Add(_current);

            }

            // Return how long it took to generate the path.
            m_ReplanTimeTaken = Environment.TickCount - m_ReplanTimeBegin;
            return true;
        }

        // Recursively generate the path in question
        public void ConstructPath(PathNode pOther)
        {
            if (pOther != null)
            {
                m_Pathlist.Insert(0,pOther);

                if (pOther.position != m_Start.position)
                {
                    ConstructPath(pOther.parent);
                }
            }
        }

        #region Generate Heuristic
        public double GenerateHeuristic(PathNode pA)
        {
            switch (m_Configuration)
            {
                case PathConfiguration.Euclidean:
                    return EuclideanDistance(pA, m_Goal);

                case PathConfiguration.Manhattan:
                    return ManhattanDistance(pA, m_Goal);
            }

            return double.MinValue;
        
        }

        // Return the heuristic based on the configuration that has been applied to this
        public double GenerateHeuristic(PathNode pA, PathNode pB)
        {
            switch (m_Configuration)
            {
                case PathConfiguration.Euclidean:
                    return EuclideanDistance(pA, pB);

                case PathConfiguration.Manhattan:
                    return ManhattanDistance(pA, pB);
            }

            return double.MinValue;
        }
        #endregion

        /// <summary>
        /// Return the manhattan distance between the point in question to the end mark
        /// </summary>
        /// <param name="pOther">The node that we are returning the manhattan distance from</param>
        /// <returns>Manhattan distance between this node to the end goal</returns>
        protected double ManhattanDistance(PathNode pOther)
        {
            if (m_Goal != null)
            {
                return (Math.Abs(pOther.position.X - m_Goal.position.X) + (Math.Abs(pOther.position.Y - m_Goal.position.Y)) * m_StraightCost);
            }
            else
            {
                return -1;
            }
        }

        public override void UpdateGoal(Point pGoal)
        {
            m_Goal = new PathNode() { 
                position = pGoal,
                g = 0,
                h = 0,
                f = 0,
                isClosedList = false,
                isOpenList = false
            };
        }

        public override void UpdateStart(Point pStart)
        {
            m_Start = new PathNode()
            {
                position = pStart,
                h = 0,
                g = 0,
                f = 0,
                isClosedList = false,
                isOpenList = false
            };
        }

        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            if (Global.DEBUG)
            {
                // Loop through the open list data and display accordingly.
                //foreach (var item in m_OpenList.Data)
                //{
                //    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"],
                //                        new Vector2(item.position.X * m_Level.TMXLevel.TileWidth,
                //                                    item.position.Y * m_Level.TMXLevel.TileHeight),
                //                                    Color.AliceBlue);

                //    pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"], item.g.ToString(),
                //                            new Vector2(item.position.X * m_Level.TMXLevel.TileWidth,
                //                                        item.position.Y * m_Level.TMXLevel.TileHeight), Color.White);
                //}

                //// Draw out the closed list
                //foreach (var item in m_ClosedList.Data)
                //{
                //    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"],
                //                        new Vector2(item.position.X * m_Level.TMXLevel.TileWidth,
                //                                    item.position.Y * m_Level.TMXLevel.TileHeight), Color.Blue);
                //}

                foreach (var item in m_Pathlist)
                {

                    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"],
                                      new Vector2(item.position.X * m_Level.TMXLevel.TileWidth,
                                                  item.position.Y * m_Level.TMXLevel.TileHeight),
                                                  item.visited == true ? Color.Blue : Color.Red);
                }

            }
            
            base.DrawPathfinding(pSpriteBatch);
        }

        /// <summary>
        /// Return the G cost between the two points
        /// </summary>
        /// <param name="a">THe first node we are comparing against</param>
        /// <param name="b">The second node we are comparing against</param>
        /// <returns>The G cost.</returns>
        public override int DistanceBetween(Point a, Point b)
        {
            if (a.X != b.X &&
                a.Y != b.Y)
            {
                return m_DiagCost;
            }
            else
            {
                return m_StraightCost;
            }
        }

        #region Basic Methods
        public bool IsOpen(PathNode pOther)
        {
            //return m_NodeGrid[pOther.position.X, pOther.position.Y].isOpenList;

            return m_OpenList.Data.Contains(pOther);
        }

        public bool IsClosed(PathNode pOther)
        {
        //    return m_NodeGrid[pOther.position.X, pOther.position.Y].isClosedList;

            return m_ClosedList.Data.Contains(pOther);
        }
        #endregion
    }
}
