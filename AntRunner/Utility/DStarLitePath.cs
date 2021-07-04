using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntRunner.Entity;
using AntRunner.Cameras;
using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.States;
using AntRunner.Tower;

// Custom library written pretty much just for this.
using PriorityQueueLib;

// XNA
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace AntRunner.Utility
{
    /// <summary>
    /// Basic pair class that is used for putting two items together.
    /// 
    /// Such a thing doesn't seem to exist within the .NET framework. 
    /// </summary>
    /// <typeparam name="K">First value that we are storing</typeparam>
    /// <typeparam name="V">The second value that we are storing</typeparam>
    public class Pair<K, V>
    {
        #region Constructors
        public Pair()
        {

        }

        public Pair(K pFirst, V pSecond)
        {
            First = pFirst;
            Second = pSecond;
        }
        #endregion

        #region Properties
        public K First { get; set; }
        public V Second { get; set; }
        #endregion
    }

    // Used for the pathfinding generation
    public class DStarLitePathNode : PathNode, IComparable<DStarLitePathNode>, IEquatable<DStarLitePathNode>
    {
        #region Members
        public Pair<double, double> k { get; set; }
        #endregion

        #region Constructors
        public DStarLitePathNode()
        {
            this.k = new Pair<double, double>();
        }

        public DStarLitePathNode(Point pPosition, Pair<double, double> pValues)
        {
            this.position = pPosition;
            this.k = pValues;
        }

        public DStarLitePathNode(int x, int y, Pair<double, double> pValues)
        {
            this.position = new Point(x, y);
            this.k = pValues;
        }

        public DStarLitePathNode(DStarLitePathNode pOther)
        {
            this.k = pOther.k;
            this.position = pOther.position;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Return the unique indentifier for the State
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.position.X + 34245 * position.Y;
        }

        #endregion


        /// <summary>
        /// Self-explanatory really. Return whether or not the two states are NOT EQUAL to each other
        /// </summary>
        /// <param name="a">Left handed value that we're comparing against</param>
        /// <param name="b">The right handed value that we're comparing against</param>
        /// <returns></returns>
        public static bool operator !=(DStarLitePathNode a, DStarLitePathNode b)
        {
            return (a.position.X != b.position.X ||
                    a.position.Y != b.position.Y);
        }

        public static bool operator ==(DStarLitePathNode a, DStarLitePathNode b)
        {
            return (a.position.X == b.position.X &&
                    a.position.Y == b.position.Y);
        }

        /// <summary>
        /// Determine which one is of greater value
        /// </summary>
        /// <param name="a">Left handed value</param>
        /// <param name="b">Right handed value</param>
        /// <returns>The bool resulting in which one is the largest</returns>
        public static bool operator >(DStarLitePathNode a, DStarLitePathNode b)
        {
            if (a.k.First - 0.00001 > b.k.First) return true;
            else if (a.k.First < b.k.First - 0.00001) return false;
            return a.k.Second > b.k.Second;
        }

        /// <summary>
        /// Return whether or not the right handed value is greater than this one
        /// </summary>
        /// <param name="a">The first value</param>
        /// <param name="b">The second value</param>
        /// <returns>Return whether or not the value is correct.</returns>
        public static bool operator <(DStarLitePathNode a, DStarLitePathNode b)
        {
            if (a.k.First + 0.00001 < b.k.First) return true;
            else if (a.k.First - 0.00001 > b.k.First) return false;
            return a.k.Second < b.k.Second;
        }

        // Less than or equal
        public static bool operator <=(DStarLitePathNode a, DStarLitePathNode b)
        {
            if (a.k.First < b.k.First) return true;
            else if (a.k.First > b.k.First) return false;
            return a.k.Second < b.k.Second + 0.00001;
        }

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        /// <param name="a">The left-handed value.</param>
        /// <param name="b">The one that we are comparing against</param>
        /// <returns>Returns whether or not the left-handed value is greater or equal to the right-handed value.</returns>
        public static bool operator >=(DStarLitePathNode a, DStarLitePathNode b)
        {
            if (a.k.First < b.k.First) return true;
            else if (a.k.First > b.k.First) return false;
            else return false;
        }

        public int CompareTo(DStarLitePathNode other)
        {
            if (k.First - 0.00001 > other.k.First) return 1;
            else if (k.First < other.k.First - 0.00001) return -1;

            if (k.Second > other.k.Second) return 1;
            else if (k.Second < other.k.Second) return -1;

            return 0;
        }

        public override bool Equals(object obj)
        {
            // Generate a temporary object to determine whether it can be casted as such.
            DStarLitePathNode _temporary = (DStarLitePathNode)obj;

            if (obj.GetType() != typeof(DStarLitePathNode))
                return false;

            if (_temporary != null)
            {
                return (_temporary.position.X == position.X &&
                        _temporary.position.Y == position.Y);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(DStarLitePathNode other)
        {
            return this.position.X == other.position.X &&
                   this.position.Y == other.position.Y;
        }

        bool IEquatable<DStarLitePathNode>.Equals(DStarLitePathNode other)
        {
            return this.position.X == other.position.X &&
                   this.position.Y == other.position.Y;
        }
    }

    // Information regarding a part of the level node
    public class CellInfo : IEquatable<CellInfo>
    {
        /// <summary>
        /// Cost -> The "F Score" if we were to compare to the A* alternative.
        /// G -> Concurrent cost from moving from the starting point to where we are now.
        /// RHS -> Right-Hand-Side Co-efficient.
        /// </summary>
        public double G, RHS, Cost;

        public bool Equals(CellInfo other)
        {
            return (G == other.G &&
                    RHS == other.RHS &&
                    Cost == other.Cost);
            
        }
    }

    /// <summary>
    /// The main pathfinding method for D* Lite.
    /// </summary>
    public class DStarLitePath : Pathfinding
    {
        #region Members
        private int m_MaxSteps; // How many steps within the loop are taken until we stop entirely.
        private int m_StepsTaken = 0;

        private bool m_AnalyzeMap = true;

        private double m_UnseenCellCost;
        private double k_m;

        private DStarLitePathNode m_Start = null;
        private DStarLitePathNode m_Goal = null;
        private DStarLitePathNode m_Last = null;

        // This provides information regarding path costs
        // Helps when the replanning is done after goal update.
        private Dictionary<DStarLitePathNode, CellInfo> m_CellHash = new Dictionary<DStarLitePathNode, CellInfo>();
        private Dictionary<DStarLitePathNode, float> m_OpenHash = new Dictionary<DStarLitePathNode, float>();
        
        // Custom implementation of the priority queue
        private PriorityQueue<DStarLitePathNode> m_OpenList = new PriorityQueue<DStarLitePathNode>();
        
        private List<DStarLitePathNode> m_Path = new List<DStarLitePathNode>();
        #endregion

        #region Constants
        private double M_SQRT2 = Math.Sqrt(2.0);
        #endregion

        #region Properties
        public List<DStarLitePathNode> Path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }
        #endregion

        private Level m_Level; // Reference to the level in question
       
        public DStarLitePath(Level pLevel, Entity.Entity pOwner) : base (pLevel,pOwner)
        {
            // How many steps are done before we give up entirely.
            m_MaxSteps = 50000;

            // Used for calculating the Euclidean Distance.
            this.m_StraightCost = 0;
            this.m_DiagCost = 0;

            this.m_PathfindingName = "D* Lite";

            m_AnalyzeMap = true;

            // Not going to require these for what we aim to do.
            m_StraightCost = 0;
            m_DiagCost = 0;

            m_UnseenCellCost = 1;
        }

        /// <summary>
        /// Return whether or not the node is occupied
        /// </summary>
        /// <param name="pOther">The other node that we are comparing against</param>
        /// <returns>Returns true or false based on the cell values and the TMX level values</returns>
        public bool IsOccupied(DStarLitePathNode pOther)
        {
            // Determine if there is something in the way within the level
            //if (!IsClear(pOther.position.X, pOther.position.Y))
            //    return true;

            if (!m_CellHash.ContainsKey(pOther))
                return false;

            return (m_CellHash[pOther].Cost < 0);
        }

        /// <summary>
        /// Same function, different argument
        /// </summary>
        /// <param name="pOther">The XNA point object that we are going to use to determine if that area is occupied</param>
        /// <returns>Return whether or not the point is considered clear.</returns>
        public bool IsClear(Point pOther)
        {
            return IsClear(pOther.X, pOther.Y);
        }

        /// <summary>
        /// Determine whether or not a point on the map is considered to be "clear"
        /// </summary>
        /// <param name="pX">The x coordinate</param>
        /// <param name="pY">The y coordinate</param>
        /// <returns></returns>
        public override bool IsClear(int pX, int pY)
        {
            // Make sure that we're within the bounds of the level
            if (pX < m_Level.TMXLevel.Width && pX >= 0 &&
                pY < m_Level.TMXLevel.Height && pY >= 0)
            {
                // Return whether or not the item at the given coordinate is being blocked or not.
                if (m_Level != null)
                {
                    // Return whether or not the current grid position is considered walkable.
                    if (m_Level.TMXLevel.Grid[pX, pY, 0].ID == 146)
                    {
                        return true;
                    }
                    else if (m_Level.IsObjectAt(pX, pY, typeof(NewBox)) || m_Level.IsObjectAt(pX,pY,typeof(DoorBlock))) // Return whether or not there is an object at the given coordinate.
                    {
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        /// <summary>
        /// Define the starting and ending point
        /// </summary>
        /// <param name="pStart">The starting point for the path</param>
        /// <param name="pGoal">The end point of the path that we want to get to</param>
        public void Initialize(Point pStart, Point pGoal, Level pLevel)
        {
            m_CellHash.Clear();
            m_Path.Clear();
            m_OpenHash.Clear();

            m_Level = pLevel;

            m_OpenList.Data.Clear();

            // Generate the start and goal nodes.
            m_Start = new DStarLitePathNode() { position = pStart };
            m_Goal = new DStarLitePathNode() { position = pGoal };

            k_m = 0;

            // Generate a new set of information regarding the starting and ending positions
            CellInfo _goalInfo = new CellInfo();
            
            _goalInfo.G = 0;
            _goalInfo.RHS = 0;
            _goalInfo.Cost = 0;

            m_CellHash.Add(m_Goal, _goalInfo);

            // Generate the cell info for the goa6l.
            CellInfo _startInfo = new CellInfo();

            _startInfo.G = _startInfo.RHS = Heuristic(m_Start, m_Goal);
            _startInfo.Cost = m_UnseenCellCost;

            m_CellHash.Add(m_Start, _startInfo);
            
            // Generate the key for the starting node within the map.
            m_Start = CalculateKey(m_Start); 
            
            m_Last = m_Start;

            /** Based on the provided parameter, determine whether or not
             * we want to inform the pathfinding about its**/
            if (m_AnalyzeMap)
            {
                //AnalyzeMap();
            }
        }

        // Loop through the map and place the appropriate hashes onto the list
        public void AnalyzeMap()
        {
            // Loop through the grid and updating the pathfinding appropriately
            for (int x = 0; x < m_Level.TMXLevel.Grid.GetLength(0); x++)
            {
                for (int y = 0; y < m_Level.TMXLevel.Grid.GetLength(1); y++)
                {
                    // Determine where on the map it's considered blocked.
                    if (m_Level.TMXLevel.Grid[x, y, 0].ID != 146)
                    {
                        // Inform the algorithm that it is in fact blocked.
                        UpdateCell(x, y, -1.0);
                    }       
                }
            }

        }

        public double GetRHS(DStarLitePathNode pOther)
        {
            if (pOther == m_Goal)
                return 0;

            // If the cell info hash map has no data on the particular node then return the heuristic.
            if (!m_CellHash.ContainsKey(pOther))
            {
                return Heuristic(pOther, m_Goal);
            }

            return m_CellHash[pOther].RHS;
        }

        /// <summary>
        /// Return whether or not the provided values equate to that of infinity values
        /// or are considered close to each other (i.e. is the true distance between them less than a ridiculously small amount)
        /// </summary>
        /// <param name="pX">X coordinate</param>
        /// <param name="pY">Y coordinate</param>
        /// <returns>Returns whether or not the two values are considered as close</returns>
        private bool Close(double pX, double pY)
        {
            if (pX == Double.PositiveInfinity &&
                pY == Double.PositiveInfinity)
            {
                return true;
            }

            // Checked to see when whether or not the value is less than that of something trivially small.
            return (Math.Abs(pX - pY) < 0.00001);
        }

        public void SetG(DStarLitePathNode pOther, double pG)
        {
            AddCell(pOther);

            if (m_CellHash.ContainsKey(pOther))
            {
                m_CellHash[pOther].G = pG;
            }
        }

        public void AddCell(DStarLitePathNode pOther)
        {
            if (m_CellHash.ContainsKey(pOther))
            {
                return;
            }

            CellInfo _temp = new CellInfo();
            
            // Generate the heuristic from the eight-way distance that is made.
            _temp.G = _temp.RHS = Heuristic(pOther, m_Goal);
            
            _temp.Cost = m_UnseenCellCost;

            if (m_CellHash.ContainsKey(pOther))
            {
                m_CellHash[pOther] = _temp;
            }
            else
            {
                m_CellHash.Add(pOther, _temp);
            }
            
        }

        /// <summary>
        /// As stated by [Koenig, 2002]
        /// 
        /// We generate a tuple value that is used for ordering values in the open list of
        /// locally inconsistent nodes that we are observing
        /// </summary>
        /// <param name="pOther">The node that we wish to generate the information about</param>
        /// <returns></returns>
        public DStarLitePathNode CalculateKey(DStarLitePathNode pOther)
        {
            double _value = Math.Min(GetRHS(pOther), GetG(pOther));

            pOther.k.First = _value + Heuristic(pOther, m_Start) + k_m;
            pOther.k.Second = _value;

            return pOther;
        }

        /// <summary>
        /// Returns the G cost in question for that particular goal.
        /// </summary>
        /// <param name="pOther">The state that we're using to process this</param>
        public double GetG(DStarLitePathNode pOther)
        {
            // There's no distance to worry ourselves about!
            if (pOther == m_Goal)
                return 0;

            // Determine first that the hash map doesn't contain the key
            // If it does, then we already have the information that we want. Return that.
            // If not then just generate the heuristic.
            if (!m_CellHash.ContainsKey(pOther))
            {
                return Heuristic(pOther, m_Goal);
            }
            else
            {
                return m_CellHash[pOther].G;
            }
        }

        /// <summary>
        /// Returns the eight way distance between point A and point B
        /// </summary>
        /// <param name="pA">The first node</param>
        /// <param name="pB">The second node</param>
        /// <returns>Returns the eight way distance</returns>
        public double EightCondist(DStarLitePathNode pA, DStarLitePathNode pB)
        {
            double _temp = 0f;

            double _min = Math.Abs(pA.position.X - pB.position.X);
            double _max = Math.Abs(pA.position.Y - pB.position.Y);

            // Compares the distance of the X and Y coordinate
            // Translates to the min and max values
            if (_min > _max)
            {
                _temp = _min;

                _min = _max;
                
                _max = _temp;
            }

            double _returnvalue = ((M_SQRT2 - 1.0) * _min + _max);
            return _returnvalue;
        }

        /// <summary>
        /// Return the heuristic distance that we are after.
        /// </summary>
        /// <param name="pA">First point</param>
        /// <param name="pB">Second point</param>
        /// <returns>The value of the euclidean distance.</returns>
        public double Heuristic(DStarLitePathNode pA, DStarLitePathNode pB)
        {
            return EightCondist(pA, pB) * m_UnseenCellCost;
        }

        /// <summary>
        /// Update the position of the goal within the map and then do something with it
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public void UpdateGoal(int x, int y)
        {
            List<Pair<Point, double>> _restore = new List<Pair<Point, double>>();

            // Store the cell hashes that we want to use
            foreach (var item in m_CellHash)
            {
                // Determine if the value is the same of an unseen cell cost
                if (!Close(item.Value.Cost, m_UnseenCellCost))
                {
                    var _tempPair = new Pair<Point, double>(new Point(item.Key.position.X, item.Key.position.Y),
                                                                      item.Value.Cost);
                    _restore.Add(_tempPair);
                }
            }

            // Clear out all the required containers
            m_CellHash.Clear();
            m_OpenHash.Clear();

            m_OpenList.Clear();

            //while (!m_OpenList.IsEmpty())
            //    m_OpenList.Poll();

            k_m = 0;

            m_Goal.position.X = x;
            m_Goal.position.Y = y;

            CellInfo _temporary = new CellInfo();

            _temporary.G = _temporary.RHS = 0;
            _temporary.Cost = m_UnseenCellCost;

            // Add to the cell hash of information the details the health of that given
            // node.
            m_CellHash.Add(m_Goal, _temporary);

            _temporary = new CellInfo();
            _temporary.G = _temporary.RHS = Heuristic(m_Start, m_Goal);
            _temporary.Cost = m_UnseenCellCost;


            if (!m_CellHash.ContainsKey(m_Start))
            {
                m_CellHash.Add(m_Start, _temporary);
            }
            else
            {
                m_CellHash[m_Start] = _temporary;
            }

            m_Start = CalculateKey(m_Start);

            m_Last = m_Start;

            // Go through the items that we've temporarily stored.
            // update the heuristics on them.
            foreach (var item in _restore)
            {
                UpdateCell(item.First.X, item.First.Y, item.Second);                
            }
        }

        /// <summary>
        /// Update the start of the path again
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public void UpdateStart(int x, int y)
        {
            m_Start.position.X = x;
            m_Start.position.Y = y;

            // Generate the heuristc with the new start and goal values;
            k_m += Heuristic(m_Last, m_Start);

            m_Start = CalculateKey(m_Start);
            m_Last = m_Start;
        }

        /// <summary>
        /// Typically used during the UpdateGoal and UpdateStart operations
        /// </summary>
        /// <param name="pX">The x coordinate</param>
        /// <param name="pY">The y coordinate</param>
        /// <param name="pValue">The value of the cost for that given cell</param>
        public void UpdateCell(int pX, int pY, double pValue)
        {
            DStarLitePathNode _temp = new DStarLitePathNode();

            _temp.position.X = pX;
            _temp.position.Y = pY;

            if (_temp == m_Start || _temp == m_Goal)
            {
                return;
            }

            AddCell(_temp);
            
            // Determine whether or not the cell hash contains the key first
            if (m_CellHash.ContainsKey(_temp))
            {
                m_CellHash[_temp].Cost = pValue;
            }

            UpdateVertex(_temp);  
        }

        /// <summary>
        /// Set the RHS (Right-Hand-Side) value and carry on as normal.
        /// </summary>
        /// <param name="pOtherOne">The node that we are setting the RHS value for</param>
        /// <param name="pRHS">Set the RHS value that is to be used</param>
        public void SetRHS(DStarLitePathNode pOtherOne, double pRHS)
        {
            AddCell(pOtherOne);

            if (m_CellHash.ContainsKey(pOtherOne))
            {
                m_CellHash[pOtherOne].RHS = pRHS;
            }
        }

        /// <summary>
        /// Returns the eight-way cost of moving from one state to another
        /// </summary>
        /// <param name="pA">The first state in question</param>
        /// <param name="pB">The second state in question</param>
        /// <returns>Returns the cost as a double value</returns>
        public double Cost(DStarLitePathNode pA, DStarLitePathNode pB)
        {
            int Xdistance = Math.Abs(pA.position.X - pB.position.X);
            int Ydistance = Math.Abs(pA.position.Y - pB.position.Y);

            // This seems completely irrelevant...
            double _scale = 0;

            // If we're making a diagonal move, then apply this scaling.
            if (Xdistance + Ydistance > 1)
            {
                _scale = Math.Sqrt(2.0); // 1.41 with extra decimal values
            }
            else
            {
                _scale = 1;
            }

            // Based on whether or not there is already information about this hash 
            // in the table, return the weighted cost of going here or return the standard scale.
            if (!m_CellHash.ContainsKey(pA))
            {
                return _scale * m_UnseenCellCost;
            }

            // Influence the cost of the node based on whether or not there is an item blocking the way
            //if (!IsClear(pA.position))
            //{
            //    return _scale * -1;
            //}

            // If the cell that we are observing is in fact a part of the cell hash
            // then we return the cost influenced by the scale.
            // Which is either 1.41 or 1 based on whether it's straight of diagonal
            return _scale * m_CellHash[pA].Cost;
        }

        /// <summary>
        /// Render the output for the pathfinding
        /// </summary>
        /// <param name="pSpriteBatch">The spritebatch object that is to be used.</param>
        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Make sure that the path is OK first.
            if (Global.DEBUG)
            {
                if (m_Path != null)
                {
                    if (m_Path.Count != 0)
                    {
                        // Loop through the open hash
                        //foreach (var item in m_OpenHash)
                        //{
                        //    // Draw the blank grid from the open hash
                        //    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"],
                        //        new Vector2(item.Key.position.X * 64,
                        //                    item.Key.position.Y * 64),
                        //                    Color.White);
                        //}



                        //// Render the cell hash items
                        foreach (var item in m_CellHash)
                        {
                            // Render the cell based on the item in the cell hash
                            pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"],
                                              new Vector2(item.Key.position.X * 64,
                                                          item.Key.position.Y * 64), Color.HotPink);
                            pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"], item.Value.Cost.ToString(),
                                                    new Vector2(item.Key.position.X * 64,
                                                                item.Key.position.Y * 64 + 50),
                                                    Color.White);
                        }

                        // Loop through the openlist in case there is any information there that can be shared.
                        //foreach (var item in m_OpenList.Data)
                        //{
                        //    // Draw the grid to the screen in its entirety.
                        //    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(item.position.X * 64,
                        //                                                                            item.position.Y * 64), Color.Green);
                        //}

                        // Render the final path that is going to be used.
                        foreach (var item in Path)
                        {
                            // Render the blank grid to the screen.
                            pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(item.position.X * 64,
                                                                                         item.position.Y * 64), item.visited == true ? Color.Blue : Color.Red);

                            // Output the k values that were stored.
                            pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"], string.Format("K: {0},{1}", item.k.First.ToString(), item.k.Second.ToString()), new Vector2(item.position.X * 64 + 10, item.position.Y * 64 + 10), Color.White);

                        }


                        //// Loop through the cell hash.
                        //foreach (var item in m_CellHash)
                        //{
                        //    pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"],
                        //        item.Value.G.ToString(),
                        //        new Vector2(item.Key.position.X * 64,
                        //                    item.Key.position.Y * 64), Color.White);
                        //}
                    }
                }
            }

            DrawPathfinding(pSpriteBatch);
        }

        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            base.DrawPathfinding(pSpriteBatch);
        }

        /// <summary>
        /// Update the value of a vertex within a given list
        /// 
        /// Based on the information provided by Sven Koenig, 2002.
        /// </summary>
        /// <param name="pOther">The node that we are going to update the vertex of</param>
        public void UpdateVertex(DStarLitePathNode pOther)
        {
            LinkedList<DStarLitePathNode> _successors = new LinkedList<DStarLitePathNode>();
            
            // If we haven't reached the goal node.
            if (pOther != m_Goal)
            {
                /** Grab the surrounding eight nodes to the one that we are focusing on]
                 * unless this node is considered as occupied **/
                _successors = Successors(pOther);

                double _temp = Double.PositiveInfinity;
                
                // For each of the successors in the list, do something
                foreach (var item in _successors)
                {
                    double _temporaryCost = GetG(item) + Cost(pOther, item);

                    if (_temporaryCost < _temp)
                    {
                        _temp = _temporaryCost;
                    }
                }

                // Determine that the two values are not similar to each other.
                if (!Close(GetRHS(pOther), _temp))
                {
                    SetRHS(pOther, _temp);
                }
            }

            // Store the values temporarily.
            double _tempG = GetG(pOther);
            double _tempRHS = GetRHS(pOther);

            // If the two items are no close then insert it into the hash map and priorityqueue.
            if (!Close(GetG(pOther), GetRHS(pOther)))
            {
                InsertHash(pOther);
            }
        }

        /// <summary>
        /// Regenerate the path in question by taking into consideration the current nodes that are in the list
        /// </summary>
        /// <returns>Return whether or not we were able to regenerate</returns>
        public override bool Replan()
        {
            // Used for when determining that there is no suitable path.
            m_StepsTaken = 0;

            m_ReplanTimeBegin = Environment.TickCount;

            // Clear out the nodes that have been placed in the list
            m_Path.Clear();

            /** Return whether or not it's possible to generate a path using the information
             *  that is available.
             * **/
            int _result = ComputeShortestPath();
            
            // Return that we are unable to plan a path.
            if (_result < 0)
            {
                return false;
            }

            // List of all the successors
            LinkedList<DStarLitePathNode> _successors = new LinkedList<DStarLitePathNode>();
            
            // The current node that we are observing
            DStarLitePathNode _current = m_Start;

            // Set the parent for usage with ray-tracing later on down the line/
            _current.parent = _current;

            /** There's no path to the goal. */
            if (GetG(m_Start) == double.PositiveInfinity)
            {
                return false;
            }

            // While the current path node is not the goal, keep going.
            while (_current != m_Goal)
            {
                m_Path.Add(_current);
                _successors = new LinkedList<DStarLitePathNode>();
                
                _successors = Successors(_current);

                if (_successors.Count == 0)
                {
                    return false;
                }

                double _cmin = Double.PositiveInfinity;
                double _tmin = 0;

                // Return the lowest scoring node 
                DStarLitePathNode _lowestScoring = new DStarLitePathNode();

                // Loop through the successor nodes for the one that we are observing.
                foreach (var item in _successors)
                {
                    // Determine first if the node that we are looking at is occupied
                    if (IsOccupied(item))
                        continue;

                    // Return the diagonal / straight cost from the current node to it's successor
                    double _value = Cost(_current, item);
                    _value += GetG(item);

                    double _valueTwo = EuclideanDistance(m_Start,item) + EuclideanDistance(item, m_Goal);

                    if (Close(_value, _cmin))
                    {
                        if (_tmin > _valueTwo)
                        {
                            _tmin = _valueTwo;
                            _cmin = _value;

                            _lowestScoring = item;
                        }
                    }
                    else if (_value < _cmin)
                    {
                        _tmin = _valueTwo;
                        _cmin = _value;

                        _lowestScoring = item;
                    }
                }

                /** Perform the Theta* based ray trace to determine if
                 we can use the same parent. **/

                //if (RayTrace(_current.parent.position, _lowestScoring.position))
                //{
                //    _lowestScoring.parent = _current.parent;
                //}
                //else
                //{
                //    _lowestScoring.parent = _current;
                //}

                // Clear the list and loop again.
                _successors.Clear();

                _current = new DStarLitePathNode(_lowestScoring);
                
            }

            m_ReplanTimeTaken = Environment.TickCount - m_ReplanTimeBegin;

            m_Path.Add(m_Goal);

            return true;
        }


        /// <summary>
        /// Incase we want to make use of ray-tracing for going back through the nodes in the list
        /// </summary>
        /// <param name="pOther">The starting node that we are focusing on</param>
        public void ReconstructPath(DStarLitePathNode pOther)
        {
            if (m_Path != null)
            {
                m_Path.Add(pOther);
                ReconstructPath(pOther);
            }

            return;
        }

        /// <summary>
        /// Returns the euclidean distance between two points
        /// </summary>
        /// <param name="pA">From the first point within the environment</param>
        /// <param name="pB">The second point within the environment</param>
        /// <returns>The euclidean distance</returns>
        public double EuclideanDistance(DStarLitePathNode pA, DStarLitePathNode pB)
        {
            return base.EuclideanDistance(pA, pB);
        }

        /// <summary>
        /// Determine whether or not the given index is within the level bounds
        /// </summary>
        /// <param name="pX">X coordinate</param>
        /// <param name="pY">Y coordinate</param>
        /// <returns></returns>
        public bool WithinLevelBounds(int pX, int pY)
        {
            if (pX >= 0 && pY >= 0 &&
                pX < m_Level.TMXLevel.Width &&
                pY < m_Level.TMXLevel.Height)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Is this coordinate the edge of the map?
        /// </summary>
        /// <param name="pX">X coordinate</param>
        /// <param name="pY">Y coordinate</param>
        /// <returns></returns>
        public bool EdgeBounds(int pX, int pY)
        {
            if (pX == 0 || pX == m_Level.TMXLevel.Width - 1 ||
                pY == 0 || pY == m_Level.TMXLevel.Height)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// As stated by Sven Koenig, 2002
        /// 
        /// Return the neighbouring nodes to the state that has been provided.
        /// 
        /// Unless the cell that we are looking at is considered as occupied, in which case we do not care.
        /// </summary>
        /// <param name="pOther">The state that we are generating neighbouring nodes for</param>
        /// <returns>The 8 neighbouring nodes of the provided state</returns>
        public LinkedList<DStarLitePathNode> Successors(DStarLitePathNode pOther)
        {
            LinkedList<DStarLitePathNode> nodes = new LinkedList<DStarLitePathNode>();
            
            DStarLitePathNode _pathNode = null;

            // If the provided node is in fact occupied, then forget about it.
            if (IsOccupied(pOther)) return nodes;

            // Loop through neighbouring nodes and add them to the list.
            for (int x = (pOther.position.X - 1); x < (pOther.position.X + 2); x++)
            {
                for (int y = (pOther.position.Y - 1); y < (pOther.position.Y + 2); y++)
                {
                    // Don't want the one that we are focusing on right now.
                    if (x == pOther.position.X && y == pOther.position.Y)
                        continue;

                    DStarLitePathNode _newState = new DStarLitePathNode()
                    {
                        position = new Point(x, y),
                        k = new Pair<double, double>(-1.0, -1.0)
                    };
                    nodes.AddFirst(_newState);
                }
            }

            return nodes;
        }

        /// <summary>
        /// Retrieve all surrounding nodes that are not occupied in one way or another
        /// 
        /// As per Sven Koenig, 2002.
        /// </summary>
        /// <param name="pOther">The node that we are getting the surrounding nodes from</param>
        /// <returns>Returns a list of surrounding nodes.</returns>
        public LinkedList<DStarLitePathNode> Predecessors(DStarLitePathNode pOther)
        {
            LinkedList<DStarLitePathNode> nodes = new LinkedList<DStarLitePathNode>();
            DStarLitePathNode _pathNode = null;

             // Loop through the neighbours of the provided node
            for (int x = (int)pOther.position.X - 1; x < (int)pOther.position.X + 2; x++)
            {
                for (int y = (int)pOther.position.Y - 1; y < (int)pOther.position.Y + 2; y++)
                {
                    if (x == pOther.position.X && y == pOther.position.Y)
                        continue;

                    // Generate the temporary pathnode that we are going to test against
                    DStarLitePathNode _neighbour = new DStarLitePathNode() { position = new Point(x,y), 
                                                                            k = new Pair<double,double>(-1.0,-1.0)};
                    // Determine first that there is no node in the way.
                    if (!IsOccupied(_neighbour))
                    {
                        nodes.AddFirst(_neighbour);
                    }
                }
            }

            return nodes;
        }


        /// <summary>
        /// Generate a new path after being called from replan
        /// </summary>
        public int ComputeShortestPath()
        {
            // Generate a new list of states to be used.
            LinkedList<DStarLitePathNode> states = new LinkedList<DStarLitePathNode>();

            /** For making sure that we stop if it's not possible to generate a path **/
            int k = 0;

            double _tempRHS = GetRHS(m_Start);
            double _tempG = GetG(m_Start);
            
            // Keep looping while the open list is considered not empty.
            while (!m_OpenList.IsEmpty() &&
                    (m_OpenList.Peek() < (m_Start = CalculateKey(m_Start))) ||
                    (GetRHS(m_Start) != GetG(m_Start)))
            {
                // Was unable to generate a valid path
                if (k++ > m_MaxSteps)
                {
                    return -1;
                }

                // The temporary node state that we are checking against
                DStarLitePathNode _current = new DStarLitePathNode();

                // Prune any of the nodes that are not considered valid.
                while (true)
                {
                    // No need to do any computation, there's nothing in the list.
                    // Generate the A* path as normal.
                    if (m_OpenList.IsEmpty())
                    {
                        return 1;
                    }

                    _current = m_OpenList.Poll();

                    if (!IsValid(_current))
                    {
                        continue;
                    }

                    if (!(_current < m_Start) && (!(GetRHS(m_Start) != GetG(m_Start))))
                    {
                        return 2;
                    }

                    break;                    
                }

                m_OpenHash.Remove(_current);

                DStarLitePathNode _old = new DStarLitePathNode(_current);

                if (_old < CalculateKey(_current)) // currently out of date requires updating
                {
                    InsertHash(_current);
                }
                else if (GetG(_current) > GetRHS(_current)) // Needs an update, 
                                                            // there was an "overconsistency" -> got better. As stated in Sven Koenig
                {
                    SetG(_current, GetRHS(_current));

                    states = Predecessors(_current);

                    // Loop through the items and change the vertex values.
                    foreach (var item in states)
                    {
                        UpdateVertex(item);
                    }
                }
                else // g <= RHS, state has got worse unfortunately.
                {
                    SetG(_current, Double.PositiveInfinity);

                    states = Predecessors(_current);

                    // Loop through the predecessors and update their vertices.
                    foreach (var item in states)
                    {
                        UpdateVertex(item);
                    }

                    UpdateVertex(_current);
                }
            }

            return 0;
        }

        /// <summary>
        /// Insert a new item in to the open hash list a long with
        /// </summary>
        /// <param name="pOther">The node that we are going to be inserting into the open list and open hash</param>
        public void InsertHash(DStarLitePathNode pOther)
        {
            float _csum;

            pOther = CalculateKey(pOther);

            _csum = KeyHashCode(pOther);

           // var _current = m_OpenHash[pOther];

            if (m_OpenHash.ContainsKey(pOther))
                m_OpenHash[pOther] = _csum;
            else
                m_OpenHash.Add(pOther, _csum);

            // Place into the open list for further consideration when it comes to ComputeShortestPath()
            m_OpenList.Add(pOther);
        }

        /// <summary>
        /// Returns true if the node in question is on the open list
        /// </summary>
        /// <param name="pOther">The node that we are determining whether or not is valid</param>
        /// <returns>Returns whether it is</returns>
        public bool IsValid(DStarLitePathNode pOther)
        {
            // Determine whether the open hash contains the node first
            if (!m_OpenHash.ContainsKey(pOther))
                return false;

            if (!Close(KeyHashCode(pOther), m_OpenHash[pOther])) 
                return false;

            return true;
        }

        /// <summary>
        /// Used to determine whether a given key hash has been updated or not
        /// </summary>
        /// <param name="pOther">The node that we are generating a key hash for</param>
        /// <returns>Returns the key hash code as a floating point value.</returns>
        public float KeyHashCode(DStarLitePathNode pOther)
        {
            return (float)(pOther.k.First + 1193 * pOther.k.Second);
        }
    }
}
