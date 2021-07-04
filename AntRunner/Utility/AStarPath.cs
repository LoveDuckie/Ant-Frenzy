using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using AntRunner.Entity;

namespace AntRunner.Utility
{
    /** Put the two separate list of nodes together for further comparison **/
    public struct PathPacker
    {
        public List<PathNode> m_BezierNodes;
        public List<PathNode> m_PathNodes;
    }

    /** The path node to be returned back to the user in a list **/
    public class PathNode : IComparable<PathNode>, IComparable
    {
        public Point position;
        public Vector2 bezierPosition;
        public double f, g, h;
        public PathNode parent { get; set; }
        public TileType type { get; set; }

        public bool isClosedList { get; set; }
        public bool isOpenList { get; set; }

        public bool visited { get; set; }
        public bool isBezier { get; set; }

        public PathNode()
        {
            isBezier = false;
        }

        /// <summary>
        /// For regenerating the F cost.
        /// </summary>
        public void Recalculate()
        {
            f = g + h;
        }


        /// <summary>
        /// Return which one is greater than the other
        /// </summary>
        /// <param name="other">The other pathnodes f score that we are comparing against</param>
        /// <returns>Returns which is greater or not</returns>
        public int CompareTo(PathNode other)
        {
            if (this.f < other.f)
                return -1;

            if (this.f == other.f)
                return 0;

            if (this.f > other.f)
                return 1;
            
            return 0;
        }

        public int CompareTo(object obj)
        {
            PathNode other = (PathNode)obj;
            
            // Determine first that the other object is in fact a path node
            if (other != null)
            {
                if (other.f > this.f)
                    return -1;

                if (other.f < this.f)
                    return 1;

                if (other.f == this.f)
                    return 0;
            }
            else
            {
                return -1;
            }

            return -1;
        }
    }

    /** Method of determining the path distance **/
    public enum PathConfiguration
    {
        Euclidean = 1,
        Manhattan,
        Diagonal
    }

    // Calculate the path from one point to another
    public class AStarPath : Pathfinding
    {
        #region Properties

        public List<PathNode> PathList
        {
            get { return m_PathList; }
            set { m_PathList = value; }
        }
        public List<PathNode> BezierPaths1
        {
            get { return m_BezierPaths; }
            set { m_BezierPaths = value; }
        }
        public List<PathNode> InterpolatedPath
        {
            get { return m_InterpolatedPath; }
            set { m_InterpolatedPath = value; }
        }
        #endregion

        #region Members
        // The path that is to be returned to the entity
        private List<PathNode> m_PathList = new List<PathNode>();

        /** **/
        private PathConfiguration m_Configuration = PathConfiguration.Manhattan;

        // This will be a reference to the pathnode during iteration that will have the lowest cost.
        private PathNode m_LowestCostNode = new PathNode();

        // The paths that have been modified for the likes of smoothing and using beziers instead.
        private List<PathNode> m_BezierPaths = new List<PathNode>();

        // First part of the process for ensuring that they are OK.
        private List<PathNode> m_InterpolatedPath = new List<PathNode>();

        private List<Vector2> m_ControlPoints = new List<Vector2>();

        // The list that is to be examined
        public List<PathNode> m_ClosedList = new List<PathNode>();
        public List<PathNode> m_OpenList = new List<PathNode>();

        public Dictionary<Point, PathNode> m_OpenListHash = new Dictionary<Point, PathNode>();
        public Dictionary<Point, PathNode> m_ClosedListHash = new Dictionary<Point, PathNode>();

        public List<PathNode> m_InterpolatedPoints = new List<PathNode>();

        #region Constants
        public const float BEZIER_SMOOTHING = 0.1f;
        public const int SEGMENTS_PER_CURVE = 10;
        #endregion

        // The environment that we'll check for.
        public PathNode[,] m_SearchNodes;
        
        // Reference to the level so that other methods can access it
        private Level m_Level;

        /// <summary>
        /// For applying the Theta* methodology.
        /// </summary>
        private bool m_ApplyTheta = false;

        // Used for interpolating the points
        public const float MINIMUM_SQR_DISTANCE = 0.01f;

        private Point m_Start = Point.Zero;
        private Point m_Goal = Point.Zero;

        public int m_CurveCount;
        #endregion

        public AStarPath(Level pLevel)
        {
            m_Level = pLevel;

            // Create the space we'll use
            m_SearchNodes = new PathNode[m_Level.TMXLevel.Width, m_Level.TMXLevel.Height];

            // Loop through the 2D array
            for (int i = 0; i < m_SearchNodes.GetLength(0); i++)
            {
                for (int j = 0; j < m_SearchNodes.GetLength(1); j++)
                {
                    m_SearchNodes[i, j] = new PathNode()
                    {
                        h = 0,
                        g = 0,
                        f = 0,
                        position = new Point(i, j),
                        visited = false,
                        isOpenList = false,
                        isClosedList = false,
                        parent = null,
                        type = TileType.Grass
                    };
                                                             
                }
            }
        }

        ~AStarPath()
        {
            // Clear up the search nodes that we were using
            for (int i = 0; i < m_SearchNodes.GetLength(0); i++)
            {
                for (int j = 0; j < m_SearchNodes.GetLength(1); j++)
                {
                    m_SearchNodes[i, j] = null;
                }
            }

        }

        
        /// <summary>
        /// Detect a diagonal point and determine if there is any kind of interpolation that is required.
        /// </summary>
        /// <returns>List of nodes that have been interpolated</returns>
        public List<PathNode> ProcessBezier()
        {
            // The list that is going to be returned.
            List<PathNode> _returnlist = new List<PathNode>();


            // Loop through the nodes and determine what has to be interpolated
            for (int i = 0; i < m_BezierPaths.Count; i++)
            {
                // Make sure that it's not the first one we are dealing with
                if (i != 0)
                {

                    // Keep going through the loop if we're not dealing with a diagonal joint
                    if (m_BezierPaths[i].position.X == m_BezierPaths[i - 1].position.X ||
                        m_BezierPaths[i].position.Y == m_BezierPaths[i - 1].position.Y)
                    {
                        // Add the pathnode to the list
                        _returnlist.Add(m_BezierPaths[i]);
                        continue;
                    }

                        List<PathNode> _temporaryInterp = new List<PathNode>();

                        _temporaryInterp.Add(m_BezierPaths[i - 1]);
                        _temporaryInterp.Add(m_BezierPaths[i]);

                        // Inteporplate the points and return them
                        _temporaryInterp = InterpolateBezier(_temporaryInterp, 5.0f);

                        _returnlist.Add(_temporaryInterp[0]);
                        _returnlist.Add(_temporaryInterp[1]);

                }
            }

            return _returnlist;
        }

        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            base.DrawPathfinding(pSpriteBatch);
        }

        protected override double ManhattanDistance(PathNode pA, PathNode pB)
        {
            return base.ManhattanDistance(pA, pB);
        }

        public override void UpdateGoal(Point pGoal)
        {
            base.UpdateGoal(pGoal);
        }

        public override void UpdateStart(Point pStart)
        {
            base.UpdateStart(pStart);
        }

        /// <summary>
        /// Translate the path points into Vector2 realworld positions.
        /// </summary>
        public void GenerateBezierPositions()
        {
            // Loop through the generated path
            for (int i = 0; i < m_PathList.Count; i++)
            {
                m_BezierPaths.Add(new PathNode()
                {
                    isBezier = true, // Important for the ants when they go pathfinding
                    position = m_PathList[i].position,

                    // Offset them slightly so that they are in the center of the path node in question
                    bezierPosition = new Vector2((m_PathList[i].position.X * m_Level.TMXLevel.TileWidth) + m_Level.TMXLevel.TileWidth / 2,
                                                 (m_PathList[i].position.Y * m_Level.TMXLevel.TileHeight) + m_Level.TMXLevel.TileHeight / 2),
                });

                if (i != 0) // If this is not the first in the list...
                {
                    // Set the parent if this is not the first in the list that we are dealing with
                    m_BezierPaths[i].parent = m_BezierPaths[i - 1];
                }
                else
                {
                    m_BezierPaths[i].parent = null;
                }
            }
        }

        // Entry point for the pathfinding. Do something neat here!
        public static List<PathNode> ComputePath(Point pTo, Point pFrom, Level pLevel)
        {
            return new AStarPath(pLevel).GeneratePath(pTo, pFrom, pLevel);
        }

        /// <summary>
        /// Return the A star object when you generate a new path from two given poitns
        /// </summary>
        /// <param name="pTo">To where we want to go</param>
        /// <param name="pFrom">From where we are coming from</param>
        /// <param name="pLevel">The level that we are interacting with.</param>
        /// <returns></returns>
        public static AStarPath ComputePathObject(Point pTo, Point pFrom, Level pLevel)
        {
            AStarPath _path = new AStarPath(pLevel);
            _path.GeneratePath(pTo, pFrom, pLevel);

            return _path;
        }

        /// <summary>
        /// Interpolate the original pathnode points before doing something else with them
        /// </summary>
        /// <param name="pSegmentPoints">The points that we are dealing with</param>
        /// <param name="pScale">The smoothness that we are after.</param>
        /// <returns>List of newly interpolated path nodes that are to be processed through bezier</returns>
        public List<PathNode> InterpolateBezierRaw(float pScale)
        {
            List<PathNode> m_ControlPoints = new List<PathNode>();
            // Return with nothing if the input is less than two.
            if (m_BezierPaths.Count < 2)
            {
                return m_BezierPaths;
            }

            // Loop through the generated BezierPaths
            for (int i = 0; i < m_BezierPaths.Count; i++)
            {
                if (i == 0) // First
                {
                    // Get the points from the bezier path list
                    Vector2 _pointOne = m_BezierPaths[i].bezierPosition;
                    Vector2 _pointTwo = m_BezierPaths[i + 1].bezierPosition;

                    // Generate the tangent that is required.
                    Vector2 _tangent = _pointTwo - _pointOne;
                    Vector2 _q1 = _pointOne + pScale * _tangent;

                    // Add the paths to the list.
                    m_ControlPoints.Add(new PathNode() { 
                        bezierPosition =  _pointOne,
                        isBezier = true
                    });

                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        bezierPosition = _q1
                    });
                }
                else if (i == m_BezierPaths.Count - 1) // Last index.
                {
                    Vector2 _pointZero = m_BezierPaths[i - 1].bezierPosition;
                    Vector2 _pointOne = m_BezierPaths[i].bezierPosition;

                    Vector2 _tangent = (_pointOne - _pointZero);
                    Vector2 _q0 = _pointOne - pScale * _tangent;

                    // Add the interpolated point to the list
                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        bezierPosition = _q0
                    });

                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        bezierPosition = _pointOne
                    });
                }
                else // Somewhere in between
                {
                    // Grab the vectors that we are going to be dealing with
                    Vector2 _pointZero = m_BezierPaths[i - 1].bezierPosition;
                    Vector2 _pointOne = m_BezierPaths[i].bezierPosition;
                    Vector2 _pointTwo = m_BezierPaths[i + 1].bezierPosition;

                    Vector2 _tangent = (_pointTwo - _pointZero);
                    _tangent.Normalize();

                    Vector2 _q0 = _pointOne - pScale * _tangent * (_pointOne - _pointZero).Length();
                    Vector2 _q1 = _pointOne + pScale * _tangent * (_pointTwo - _pointOne).Length();

                    // Add the new points bad onto it
                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        bezierPosition = _q0
                    });

                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        bezierPosition = _pointOne
                    });

                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        bezierPosition = _q1
                    });
                }
            }

            return m_ControlPoints;
        }


        /// <summary>
        /// A simple function that is meant to smooth out the joints between bezier
        /// curves.
        /// </summary>
        /// <param name="pSegmentPoints">The points that we want to focus on</param>
        /// <param name="pScale">To which degree that we want the curves to appear</param>
        public List<PathNode> InterpolateBezier(List<PathNode> pSegmentPoints, float pScale)
        {
            List<PathNode> m_ControlPoints = new List<PathNode>();

            // Only want to deal with these points if it's more than 3.
            if (pSegmentPoints.Count < 2)
            {
                return null;
            }

            // Loop through the segment points that we require
            for (int i = 0; i < pSegmentPoints.Count; i++)
            {
                if (i == 0) // first one
                {
                    Vector2 _pointOne = pSegmentPoints[i].bezierPosition;
                    Vector2 _pointTwo = pSegmentPoints[i + 1].bezierPosition;
                    Vector2 _tangent = _pointOne - _pointTwo;
                    Vector2 _q1 = _pointOne + pScale * _tangent;

                    // Store the new interpolated points some where
                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        isOpenList = false,
                        isClosedList = false,
                        bezierPosition = _pointOne
                    });

                    m_ControlPoints.Add(new PathNode()
                    {
                        isClosedList = false,
                        isOpenList = false,
                        isBezier = true,
                        bezierPosition = _q1,
                    });
                }
                else if (i == pSegmentPoints.Count - 1) // Last, zero-indexed.
                {
                    Vector2 _pointOne = pSegmentPoints[i - 1].bezierPosition;
                    Vector2 _pointTwo = pSegmentPoints[i].bezierPosition;
                    Vector2 _tangent = (_pointOne - _pointTwo);
                    Vector2 _q0 = _pointOne - pScale * _tangent;

                    // Add the pathnode to the interpolated list.
                    m_ControlPoints.Add(new PathNode()
                    {  
                        isBezier = true,
                        bezierPosition = _q0,
                        isClosedList = false,
                        isOpenList = false
                    });

                    m_ControlPoints.Add(new PathNode()
                    {
                        isBezier = true,
                        isOpenList = false,
                        isClosedList = false,
                        bezierPosition = _pointOne
                    });
                }
                else // Anywhere in between front and back
                {
                    // Grab the vectors that we are going to perform operations on.
                    Vector2 _p0 = pSegmentPoints[i - 1].bezierPosition;
                    Vector2 _p1 = pSegmentPoints[i].bezierPosition;
                    Vector2 _p2 = pSegmentPoints[i + 1].bezierPosition;

                    // Grab the tangent and the normalize it
                    Vector2 _tangent = (_p2 - _p0);
                    _tangent.Normalize();
                    Vector2 _q0 = _p1 - pScale * _tangent * (_p1 - _p0).Length();
                    Vector2 _q1 = _p1 + pScale * _tangent * (_p2 - _p1).Length();

                    // Create the new pathnode that we are working with
                    m_ControlPoints.Add(new PathNode()
                    { 
                        bezierPosition = _q0,
                        isOpenList = false,
                        isBezier = true,
                        isClosedList = false
                    });
                    m_ControlPoints.Add(new PathNode()
                    {
                        bezierPosition = _p1,
                        isOpenList = false,
                        isBezier = true,
                        isClosedList = false
                    });
                    m_ControlPoints.Add(new PathNode()
                    {
                        isOpenList = false,
                        isBezier = true,
                        isClosedList = false,
                        bezierPosition = _q1
                    });
                }
            }

            return m_ControlPoints;
        }

        // Recursively construct the path.
        public void ConstructPath(PathNode pNode)
        {
            if (pNode != null)
            {
                m_PathList.Add(pNode);
                ConstructPath(pNode.parent);
            }
        }

        /// <summary>
        /// Simple function for returning the path node that has the lowest cost.
        /// 
        /// This could get expensive considering the O(n) nature of the call.
        /// </summary>
        /// <returns>Returns the pathnode with the lowest scoring F node.</returns>
        public PathNode GetLowestF()
        {
            int _lowestF = int.MaxValue;
            PathNode _result = null;

            // Loop through the list and find the one with the lowest cost
            foreach (var item in m_OpenList)
            {
                if (item.f < _lowestF)
                {
                    _lowestF = (int)item.f;
                    _result = item;
                }
            }

            return _result;
        }

        #region A Star Functions
        // Return whether or not the parameters are in the open list.
        public bool IsInOpenList(int x, int y)
        {
            // If we have found the item in the closed list then return it
            foreach (var item in m_ClosedList)
            {
                if (item.position.X == x &&
                    item.position.Y == y)
                    return true;
            }

            return false;
        }

        // Getting rid of the O(n) problem that we are suffering from at the moment.
        public bool IsInOpenListHash(Point pPoint)
        {
            return m_OpenListHash.ContainsKey(pPoint);
        }
     
        // Determine if the point is in the open list
        public bool IsInOpenList(Point pNode)
        {
            return IsInOpenList(pNode.X, pNode.Y);
        }

        /// <summary>
        /// Using the likes of the hash table, I wanted there to be a way to immediately check
        /// whether or not there was a certain node in the open or closed list
        /// </summary>
        /// <param name="pPoint">The point that we are checking</param>
        /// <returns>Returns whether or not the point placed as a parameter is in the closed list</returns>
        public bool IsInClosedListHash(Point pPoint)
        {
            return m_ClosedListHash.ContainsKey(pPoint);
        }

        // Determine if the point is in the closed list
        public bool IsInCLosedList(Point pPosition)
        {
            return IsInClosedList(pPosition.X, pPosition.Y);
        }

        // Determine whether or not it's in the closed list
        public bool IsInClosedList(int x, int y)
        {
            foreach (var item in m_ClosedList)
            {
                 // Determine if that is the item we are after
                 // If so, then return true
                if (item.position.X == x &&
                    item.position.Y == y)
                    return true;
            }

            return false;
        }
        #endregion

        /// <summary>
        /// Search in a linear fashion to determine if the node is within the closed list.
        /// </summary>
        /// <param name="pNode">The node that we aim to check is within the list in question</param>
        /// <returns>Return whether or not the provided item is discovered to be in the closed list.</returns>
        public bool IsInClosedList(PathNode pNode)
        {
            foreach (var item in m_ClosedList)
            {
                // Determine if that is the item we are after
                // If so then return true
                if (item.position.X == pNode.position.X &&
                    item.position.Y == pNode.position.Y)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// For the Theta based optimisations.
        /// </summary>
        /// <param name="pStart">The starting point that we are going to be looking from</param>
        /// <param name="pAim">To where we aim to look at </param>
        /// <returns>Return whether or not something is within sight that we are after.</returns>
        public bool LineOfSight(Point pStart, Point pAim)
        {
            Point _vectorOne = new Point(pStart.X, pStart.Y);
            Point _vectorTwo = new Point(pAim.X, pAim.Y);
            
            // Grab the distance
            Point _distanceVector = new Point(_vectorTwo.X - _vectorOne.X, _vectorTwo.Y - _vectorOne.Y);

            if (_distanceVector.Y < 0)
            {

            }

            return false;
        }

        /// <summary>
        /// Reconfigure the path that we're going to be using
        /// </summary>
        /// <param name="pActiveList">The list of paths that are to be reconstructed</param>
        /// <param name="pToAdd">The path node that we aim to add.</param>
        /// <returns></returns>
        public List<PathNode> ReconstructPath(List<PathNode> pActiveList, PathNode pToAdd)
        {
            if (pToAdd.parent != null)
            {
                return ReconstructPath(pActiveList,pToAdd);
            }

            return pActiveList;
        }

        #region Bezier Curve Algorithms
        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">The weighting that is added to the points</param>
        /// <param name="pPoint0">The first point of the quadratic curve</param>
        /// <param name="pPoint1">The point</param>
        /// <param name="pPoint2">The second point of the quadratic curve</param>
        /// <returns></returns>
        public static Vector2 CalculateQuadraticBezierPoint(float t, Vector2 pPoint0, Vector2 pPoint1, Vector2 pPoint2)
        {
            // That looks like the right algorithm!? o__O
            Vector2 _newpoint = ((float) Math.Pow(1 - t, 2) * pPoint0) + // First term 
                                (2 * (1 - t) * t * pPoint1) + // Second term 
                                ((float) Math.Pow(t, 2) * pPoint2); // Third Term;

            return _newpoint;
        }

        /// <summary>
        /// Take in for different points for the polygon that is used for generating the curvature
        /// </summary>
        /// <param name="t">The point of the interpolation that we want</param>
        /// <param name="pPoint0">The first point of the curvature</param>
        /// <param name="pPoint1">Second point</param>
        /// <param name="pPoint2">Third point</param>
        /// <param name="pPoint3">Last and fourth point</param>
        /// <returns>The point on the curvature that we want -- this is dependant on the t value that is used</returns>
        public static Vector2 CalculateCubicBezierPoint(float t, Vector2 pPoint0, Vector2 pPoint1, Vector2 pPoint2, Vector2 pPoint3)
        {
            // First, second, third and fourth term.
            Vector2 _newpoint = ((float) Math.Pow((1 - t),3) * pPoint0) + // First term
                                (3 * (float) Math.Pow((1 - t),2) * t * pPoint1) +  // Second term
                                (3 * (1 - t) * (float) Math.Pow(t,2) * pPoint2) + // Third term
                                ((float) Math.Pow(t,3) * pPoint3); // Fourth term
            return _newpoint;
        }
        #endregion


        /// <summary>
        /// Generate smoother bezier paths from the generated paths to begin with
        /// </summary>
        private void BezierPaths()
        {
            // Make sure that the pathlist has been generated first
            if (m_PathList != null && m_PathList.Count > 0)
            {
                // Loop through the existing paths and do some wizardry
                for (int i = 0; i < m_PathList.Count - 3; i += 3)
                {
                    // Want to center the points to the center of the tiles that we are using
                    Vector2 p0 = new Vector2((m_PathList[i].position.X * m_Level.TMXLevel.TileWidth) + m_Level.TMXLevel.TileWidth / 2,
                                             (m_PathList[i].position.Y * m_Level.TMXLevel.TileHeight) + m_Level.TMXLevel.TileHeight / 2);
                    
                    Vector2 p1 = new Vector2((m_PathList[i + 1].position.X * m_Level.TMXLevel.TileWidth) + m_Level.TMXLevel.TileWidth / 2, 
                                             (m_PathList[i + 1].position.Y * m_Level.TMXLevel.TileHeight) + m_Level.TMXLevel.TileHeight / 2);
                    
                    Vector2 p2 = new Vector2((m_PathList[i + 2].position.X * m_Level.TMXLevel.TileWidth) + m_Level.TMXLevel.TileWidth / 2, 
                                             (m_PathList[i + 2].position.Y * m_Level.TMXLevel.TileHeight) + m_Level.TMXLevel.TileHeight / 2);
                    
                    Vector2 p3 = new Vector2((m_PathList[i + 3].position.X * m_Level.TMXLevel.TileWidth) + m_Level.TMXLevel.TileWidth / 2, 
                                             (m_PathList[i + 3].position.Y * m_Level.TMXLevel.TileHeight) + m_Level.TMXLevel.TileHeight / 2);

                    if (i == 0)
                    {
                        PathNode _tempbezier = new PathNode()
                        {
                            parent = null,
                            position = new Point(0, 0),
                            bezierPosition = new Vector2(0f, 0f),
                            isBezier = true,
                            visited = false,
                            isClosedList = true,
                            isOpenList = false
                        };

                        _tempbezier.bezierPosition = CalculateCubicBezierPoint(0f, p0, p1, p2, p3);

                        m_BezierPaths.Add(_tempbezier);
                    }

                    // Loop through 10 times for the granularity of the points that we are after.
                    for (int j = 0; j < SEGMENTS_PER_CURVE; j++)
                    {
                        float _tvalue = j / (float)SEGMENTS_PER_CURVE;

                        // The new bezier node that we are going to create.
                        PathNode _newbezier = new PathNode()
                        {
                            parent = null,
                            position = new Point(0, 0),
                            bezierPosition = new Vector2(0f, 0f),
                            isBezier = true,
                            visited = false,
                            isClosedList = true,
                            isOpenList = false
                        };

                        // Generate the bezier position at the given point
                        _newbezier.bezierPosition = CalculateCubicBezierPoint(_tvalue, p0, p1, p2, p3);
                        m_BezierPaths.Add(_newbezier);
                    }
                }
            }

           
        }

        /// <summary>
        /// Called when we want to generate a new path based on the objects in the maze.
        /// </summary>
        public void Replan()
        {
            m_PathList.Clear();
            m_OpenList.Clear();
            m_ClosedList.Clear();
        
        }

        // List of paths that are to have the curves generated on
        private List<PathNode> BezierPaths(List<PathNode> pPaths)
        {
            List<PathNode> m_CurvedPaths = new List<PathNode>();

            if (pPaths.Count != 0)
            {
                // Loop through the path that is sent through the function
                for (int i = 0; i < pPaths.Count - 3; i += 3)
                {
                    Vector2 p0 = pPaths[i].bezierPosition;
                    Vector2 p1 = pPaths[i + 1].bezierPosition;
                    Vector2 p2 = pPaths[i + 2].bezierPosition;
                    Vector2 p3 = pPaths[i + 3].bezierPosition;

                    // If we are dealing with the first point, then do something wonderous
                    if (i == 0)
                    {
                        PathNode _tempbezier = new PathNode()
                        {
                            parent = null,
                            position = new Point(0, 0),
                            bezierPosition = new Vector2(0f, 0f),
                            isBezier = true,
                            visited = false,
                            isClosedList = true,
                            isOpenList = false
                        };

                        _tempbezier.bezierPosition = CalculateCubicBezierPoint(0f, p0, p1, p2, p3);
                        m_CurvedPaths.Add(_tempbezier);
                    }

                    // Loop through 10 times for the granularity of the points that we are after.
                    for (int j = 0; j < SEGMENTS_PER_CURVE; j++)
                    {
                        float _tvalue = j / (float)SEGMENTS_PER_CURVE;

                        // The new bezier node that we are going to create.
                        PathNode _newbezier = new PathNode()
                        {
                            parent = null,
                            position = new Point(0, 0),
                            bezierPosition = new Vector2(0f, 0f),
                            isBezier = true,
                            visited = false,
                            isClosedList = true,
                            isOpenList = false
                        };

                        // Generate the bezier position at the given point
                        _newbezier.bezierPosition = CalculateCubicBezierPoint(_tvalue, p0, p1, p2, p3);
                        m_CurvedPaths.Add(_newbezier);
                    }
                }
            }

            // Return the list of paths in question
            return m_CurvedPaths;
        }

        /// <summary>
        /// Return the cost of the path from one area to another
        /// using the Manhattan distance
        /// </summary>
        /// <param name="pFrom">The point that we are moving from</param>
        /// <param name="pTo">The point to where we are going to </param>
        /// <returns></returns>
        public int GenerateHeuristic(Point pFrom, Point pTo)
        {
            // Considered as the Manhattan Distance
            return (Math.Abs(pFrom.X - pTo.X) + (Math.Abs(pFrom.Y - pTo.Y)) * 10);
        }

        // Determine the distance between two points based
        // on whether they are diagonal or forward
        public override int DistanceBetween(Point a, Point b)
        {
            // Applying the Manhattan distance here
            if ((Math.Abs(a.X - b.X) > 0) &&
                (Math.Abs(a.Y - b.Y) > 0))
                return 14;
            else
                return 10;

            // Need to consider how I am going to apply this function

            //if (a.X != b.X ||
            //    a.Y != b.Y)
            //{
            //    return 14;
            //}
            //else
            //{
            //    return 10;
            //}
        }

        /// <summary>
        /// Using the A* pathfinding method, we want to get from one pont to another.
        /// 
        /// Return a list of pathnodes that are going to do just that.
        /// </summary>
        /// <param name="pTo">The point that we are going to</param>
        /// <param name="pFrom">The point that we are coming from</param>
        /// <param name="pLevel">The level that we are interacting with</param>
        /// <returns></returns>
        public List<PathNode> GeneratePath(Point pTo, Point pFrom, Level pLevel)
        {
            m_ClosedList.Clear();
            m_OpenList.Clear();

            // Add the starting node to the list
            m_SearchNodes[pFrom.X,pFrom.Y].g = 0;
            m_SearchNodes[pFrom.X,pFrom.Y].h = GenerateHeuristic(pFrom,pTo); // Manhattan distance
            m_SearchNodes[pFrom.X,pFrom.Y].f = m_SearchNodes[pFrom.X,pFrom.Y].g + m_SearchNodes[pFrom.X,pFrom.Y].h;
            m_OpenList.Add(m_SearchNodes[pFrom.X,pFrom.Y]);

            // Keep looping until there are no open nodes in the list
            while (OpenNodes())
            {
                // Get the next least expensive node to deal with
                PathNode _currentNode = GetLowestF();

                // If we have reached the end, then break out of this and construct the path
                // that is going to be sent back to the ant
                if (_currentNode.position == pTo)
                {
                    ConstructPath(_currentNode);
                    break;
                }

                RemoveFromOpenList(_currentNode.position.X, _currentNode.position.Y);
                m_ClosedList.Add(_currentNode);

                // Loop through the neighbours
                // No idea why I am using g and f for the indexes of the for loops
                for (int g = (_currentNode.position.X - 1); g < (_currentNode.position.X + 2); g++)
                {
                    for (int f = (_currentNode.position.Y - 1); f < (_currentNode.position.Y + 2); f++)
                    {
                        // Make sure that it's within the boundaries and valid.
                        if (CheckClear(new Point(g, f))
                            && !m_Level.IsObjectAt(g, f,typeof(BlackHole))
                            && !m_Level.IsObjectAt(g, f,typeof(Box))
                            && !m_Level.IsObjectAt(g, f,typeof(NewBox))
                            && DistanceBetween(new Point(g, f), _currentNode.position) != 14) // Prevent diagonal moves.
                        {
                            // Determine first that it's not in the closed list.
                            if (!IsInClosedList(g, f))
                            {
                                int _tentativeGScore = (int)m_SearchNodes[g,f].g + DistanceBetween(_currentNode.position,new Point(g,f));
                                bool _tentativeIsBetter = false;
     
                                // Determine whether the co-ordinate is in the open list
                                if (!IsInOpenList(m_SearchNodes[g,f]))
                                {
                                    m_SearchNodes[g, f].h = GenerateHeuristic(m_SearchNodes[g, f].position, pTo) + 
                                                            ((int)pLevel.DangerMap.GetDangerWeighting(g,f).m_Weighting * 100);
                                    _tentativeIsBetter = true;
                                    m_OpenList.Add(m_SearchNodes[g, f]);
                                }
                                else if (_tentativeGScore < m_SearchNodes[g,f].g) // Item is already in the open list, check to see if the path could be shorter.
                                {
                                    _tentativeIsBetter = true;
                                }

                                /// Determine if this is a shorter path if we go this way
                                if (_tentativeIsBetter)
                                {
                                    m_SearchNodes[g, f].parent = m_SearchNodes[_currentNode.position.X, _currentNode.position.Y];
                                    m_SearchNodes[g, f].g = _tentativeGScore;
                                    m_SearchNodes[g, f].f = m_SearchNodes[g, f].g + m_SearchNodes[g, f].h; // Combine the scores together.
                                }
                            }
                        }
                    }
                }
            }

            // The curve count used for generating the points
            m_CurveCount = (m_PathList.Count - 1) / 3;

            // Add the raw pathnodes into bezier format and into the appropriate list.
           GenerateBezierPositions();

            var _path = InterpolateBezierRaw(0.25f);
            var _adjustedpaths = BezierPaths(_path);

            // Generate the bezier path list
           
            //BezierPaths();

            // Set the control points that we wish to curve.
            //m_BezierPath.SetControlPoints(m_BezierPaths);

            // Sample the path for now
           // m_BezierPath.SamplePoints(m_BezierPaths, 10, 1000, 0.33f);
            //m_BezierPath.GetDrawingPointsV2();

           // m_BezierPath.Interpolate(m_BezierPaths,0.25f); // Important for determining these things

          //  m_BezierPaths.AddRange(m_BezierPath.ControlPoints);

            //List<PathNode> _drawingPoints = m_BezierPath.GetDrawingPointsV2();

            // Return the default path list
            if (m_PathList.Count < 2)
            {
                return m_PathList;
            }
            else
            {
                //return _path;
               return _adjustedpaths;
               //return m_BezierPaths;

               // return m_BezierPaths;
                //return _newpath;
            }
        }

        // Remove the specified node from the closed list
        public void RemoveFromClosedList(PathNode pNode)
        {
            PathNode _toRemove = null;

            for (int i = 0; i < m_ClosedList.Count; i++)
            {
                // Determine that is the node that we want to deal with
                if (m_ClosedList[i].position == pNode.position)
                {
                    _toRemove = m_ClosedList[i];
                }
            }

            if (_toRemove != null)
            {
                m_ClosedList.Remove(_toRemove);
            }
        }

        public void RemoveFromClosedList(int x, int y)
        {
            PathNode _toRemove = null;

            for (int i = 0; i < m_ClosedList.Count; i++)
            {
                if (m_ClosedList[i].position == new Point(x, y))
                {
                    _toRemove = m_ClosedList[i];
                }
            }

            // Determine that something was found before removing
            if (_toRemove != null)
            {
                m_ClosedList.Remove(_toRemove);
            }
        }

        public void RemoveFromOpenList(PathNode pNode)
        {
            m_OpenList.Remove(pNode);
        }

        public void RemoveFromOpenList(int x, int y)
        {
            PathNode _toRemove = null;

            for (int i = 0; i < m_OpenList.Count; i++)
            {
                // Determine if the positioning is right
                if (m_OpenList[i].position == new Point(x, y))
                {
                    _toRemove = m_OpenList[i];
                }
            }

            if (_toRemove != null)
            {
                m_OpenList.Remove(_toRemove);
            }
        }

        // Grab the node that we want
        public PathNode GetNodeFromOpenHash(Point pIndex)
        {
            // Determine whether or not it has the key that we want
            if (m_OpenListHash.ContainsKey(pIndex))
            {
                return m_OpenListHash[pIndex];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the node from the dictionary presuming that it contains the key that we are 
        /// after
        /// </summary>
        /// <param name="pIndex">The index in question</param>
        /// <returns>The node, presuming that it exists.</returns>
        public PathNode GetNodeFromClosedHash(Point pIndex)
        {
            if (m_ClosedListHash.ContainsKey(pIndex))
            {
                return m_ClosedListHash[pIndex];
            }
            else
            {
                return null;
            }            
        }

        public PathNode GetNodeFromOpen(int x, int y)
        {
            for (int i = 0; i < m_OpenList.Count; i++)
            {
                if (m_OpenList[i].position == new Point(x, y))
                {
                    return m_OpenList[i];
                }
            }

            return null;
        }

        public PathNode GetNodeFromClosed(int x, int y)
        {
            for (int i = 0; i < m_ClosedList.Count; i++)
            {
                
                if (m_ClosedList[i].position == new Point(x, y))
                {
                    return m_ClosedList[i];
                }
            }

            return null;
        }

        #region Helper Functions
        public bool CheckClear(Point pArea)
        {
            // Return whether or not the co-ordinates is within the level
            // and whether we're walking on grass here.
            if (pArea.X > 0 && pArea.X < m_Level.TMXLevel.Grid.GetLength(0) &&
                pArea.Y > 0 && pArea.Y < m_Level.TMXLevel.Grid.GetLength(1))
            {
                return (m_Level.TMXLevel.Grid[pArea.X, pArea.Y,0].ID == 146);
            }

            return false;
        }

        public bool OpenNodes()
        {
            return m_OpenList.Count > 0;
        }

        public bool IsInOpenList(PathNode pNode)
        {
            return m_OpenList.Contains(pNode);
        }


        #endregion
    }
}
