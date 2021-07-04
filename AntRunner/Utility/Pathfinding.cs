using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using System.Runtime.InteropServices;

using AntRunner.Entity;
using AntRunner.Cameras;
using AntRunner.Menu;
using AntRunner.Particles;

namespace AntRunner.Utility
{
    public class Pathfinding
    {
        #region Members
        protected Level m_Level = null;
        protected PathNode m_Start = null;
        protected PathNode m_Goal = null;

        protected PathConfiguration m_Configuration;

        protected string m_PathfindingName = "";

        protected Texture2D m_DebugTitle = null;
        protected Texture2D m_DebugContent = null;

        protected int m_ReplanTimeTaken = 0;
        protected int m_ReplanTimeBegin = 0;

        // Hold a reference to the owner of this pathfinding object in question
        private Entity.Entity m_Owner = null;

        protected TestStats m_TestStats = new TestStats();

        protected int m_StraightCost = 0;
        protected int m_DiagCost = 0;
        #endregion

        #region Constructors
        public Pathfinding()
        {
            m_Level = MainGame.Instance.GameState.Level;
        }

        public Pathfinding(Level pLevel, Entity.Entity pOwner)
        {
            m_Level = pLevel;
            m_Owner = pOwner;

            m_DebugTitle = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice,250,30, Color.Black);
            m_DebugContent = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice,250,65,Color.Black);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Determine whether the coordinates provided are considered clear.
        /// 
        /// This checked whether there is a box object that is in the way 
        /// or whether or not it is within the bounds of the level.
        /// </summary>
        /// <param name="pX">X coordinate</param>
        /// <param name="pY">Y coordinate</param>
        /// <returns>Whether or not the provided position is walkable.</returns>
        public virtual bool IsClear(int pX, int pY)
        {
            // Determine that the level is not null first.
            if (m_Level != null)
            {
                return (m_Level.IsClear(pX, pY) && 
                        !m_Level.IsObjectAt(pX,pY,typeof(NewBox)) &&
                        m_Level.WithinBounds(pX,pY));
            }

            return false;
        }

        // Return the size of the object for diagnostics.
        //public static int GetSizeOfObject(object obj)
        //{
        //    object _object = null;
        //    int size = 0;
        //    Type type = obj.GetType();
         
        //    PropertyInfo[] info = type.GetAllProperties();
            
        //    foreach (PropertyInfo property in info)
        //    {
        //        _object = property.GetValue(obj, null);

        //        Type _type = property.GetType();

        //        size += sizeof( );

        //    }

        //    return size;
        //}

        /// <summary>
        /// Return the size of the object in question through the usage of reflection.
        /// </summary>
        /// <param name="pObject"></param>
        /// <returns>Returns the size in bytes.</returns>
        public virtual int GetMemorySize(object pObject)
        {
            return 0;
        }

        /// <summary>
        /// For detecting a collision
        /// </summary>
        /// <param name="pA">The point that we are starting from</param>
        /// <param name="pB">The point that we are ending</param>
        /// <param name="pDistance">The distance in which the collision has to be recognized.</param>
        /// <returns>Returns whether or not there was a collision</returns>
        public virtual bool RayTraceDistance(Point pA, Point pB, float pDistance)
        {
            int _x0 = pA.X, _y0 = pA.Y;
            int _x1 = pB.X, _y1 = pB.Y;

            // Grab the distance values.
            int _distanceX = Math.Abs(_x1 - _x0);
            int _distanceY = Math.Abs(_y1 - _y0);

            // Generate the incremental values that we will use 
            // to detect a collision
            int _x = _x0, _y = _y0;

            #region Multiplied by tile width and height
            // Put them into real-world co-ordinates, and do the increments with these
            // get a more precise output perhaps?
            int _x0multiplied = _x0 * m_Level.TMXLevel.TileWidth;
            int _x1multiplied = _x1 * m_Level.TMXLevel.TileWidth;

            int _y0mutliplied = _y0 * m_Level.TMXLevel.TileHeight;
            int _y1multiplied = _y1 * m_Level.TMXLevel.TileHeight;

            // Return the distances from using the multiplied values
            int _distanceXmultiplied = Math.Abs(_x1multiplied - _x0multiplied);
            int _distanceYmultiplied = Math.Abs(_y1multiplied - _y1multiplied);

            int _xIncrementalMultiplied = (_x1multiplied > _x0multiplied) ? 1 : -1;

            int _distanceMultipliedTotal = _distanceXmultiplied - _distanceYmultiplied;
            #endregion

            // These will incremetna the starting points on every loop iteration
            int _xIncremental = (_x1 > _x0) ? 1 : -1;
            int _yIncremental = (_y1 > _y0) ? 1 : -1;

            int _distanceTotal = _distanceX - _distanceY;

            // Keep looping until we either reach the end of detect a blocking tile
            for (int n = _distanceX + _distanceY; n > 0; --n)
            {
                // Determine whether it's considered clear at the given co-ordinate on the map.
                if (!IsClear(_x, _y))
                    return false;

                if (_distanceTotal > 0)
                {
                    _x += _xIncremental;
                    _distanceTotal -= _distanceY;
                }
                else
                {
                    _y += _yIncremental;
                    _distanceTotal += _distanceX;
                }
            }

            return true;
        }

        public virtual bool RayTrace(Point pA, Point pB)
        {
            int _x0 = pA.X, _y0 = pA.Y;
            int _x1 = pB.X, _y1 = pB.Y;

            // Grab the distance values.
            int _distanceX = Math.Abs(_x1 - _x0);
            int _distanceY = Math.Abs(_y1 - _y0);

            // Generate the incremental values that we will use 
            // to detect a collision
            int _x = _x0, _y = _y0;

            #region Multiplied by tile width and height
            // Put them into real-world co-ordinates, and do the increments with these
            // get a more precise output perhaps?
            int _x0multiplied = _x0 * m_Level.TMXLevel.TileWidth;
            int _x1multiplied = _x1 * m_Level.TMXLevel.TileWidth;

            int _y0mutliplied = _y0 * m_Level.TMXLevel.TileHeight;
            int _y1multiplied = _y1 * m_Level.TMXLevel.TileHeight;

            // Return the distances from using the multiplied values
            int _distanceXmultiplied = Math.Abs(_x1multiplied - _x0multiplied);
            int _distanceYmultiplied = Math.Abs(_y1multiplied - _y1multiplied);

            int _xIncrementalMultiplied = (_x1multiplied > _x0multiplied) ? 1 : -1;

            int _distanceMultipliedTotal = _distanceXmultiplied - _distanceYmultiplied;
            #endregion

            // These will incremetna the starting points on every loop iteration
            int _xIncremental = (_x1 > _x0) ? 1 : -1;
            int _yIncremental = (_y1 > _y0) ? 1 : -1;

            int _distanceTotal = _distanceX - _distanceY;

            // Keep looping until we either reach the end of detect a blocking tile
            for (int n = _distanceX + _distanceY; n > 0; --n)
            {
                // Determine whether it's considered clear at the given co-ordinate on the map.
                if (!IsClear(_x, _y))
                    return false;

                if (_distanceTotal > 0)
                {
                    _x += _xIncremental;
                    _distanceTotal -= _distanceY;
                }
                else
                {
                    _y += _yIncremental;
                    _distanceTotal += _distanceX;
                }
            }

            return true;
        }

        public virtual void Initialize()
        {
            return;
        }

        /// <summary>
        /// Return whether or not
        /// </summary>
        /// <param name="pX">X coordinate from the world space that we are checking against</param>
        /// <param name="pY">Y coordinate from the world space that we are checking against</param>
        /// <returns></returns>
        public virtual bool IsClearWorld(int pX, int pY)
        {
            if (m_Level != null)
            {
                int _pointX = pX / m_Level.TMXLevel.TileWidth;
                int _pointY = pY / m_Level.TMXLevel.TileHeight;

                return (m_Level.IsClearWorld(pX, pY));
            }

            return false;
        }

        /// <summary>
        /// Return whether or not the provided coordinates are within the bounds
        /// </summary>
        /// <param name="pX">X coordinate</param>
        /// <param name="pY">Y coordinate</param>
        /// <returns></returns>
        public bool WithinBounds(int pX, int pY)
        {
            if (m_Level != null)
            {
                return (pX < m_Level.TMXLevel.Width &&
                    pY < m_Level.TMXLevel.Height &&
                    pX >= 0 && pY >= 0);
                
            }
            return false;
        }

        /// <summary>
        /// Nothing should happen within this function, rather the replan should be overrided and something else
        /// should be done inside.
        /// 
        /// If we fail to find a path then this should be called.
        /// </summary>
        public virtual bool Replan()
        {
            return false;
        }

        /** Mostly used for the likes of D* Lite when updating affected edges
        in the map. **/
        public virtual void UpdateVertex(int pX, int pY)
        {
            return;
        }

        public virtual void UpdateVertex(PathNode pA, PathNode pB)
        {
            return;
        }

        public virtual void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            if (Global.PATH_DEBUG)
            {
                pSpriteBatch.Draw(m_DebugTitle, new Vector2(this.m_Owner.Position.X, this.m_Owner.Position.Y + this.m_Owner.Size.Y), Color.White);
                pSpriteBatch.Draw(m_DebugContent,new Vector2(this.m_Owner.Position.X,this.m_Owner.Position.Y + this.m_Owner.Size.Y + 30), Color.White * 0.60f);

                Vector2 _textvector = new Vector2(this.m_Owner.Position.X, this.m_Owner.Position.Y + this.m_Owner.Size.Y + 30);

                // Output the pathfinding information in question
                ShadowText.Draw("STATS", pSpriteBatch, new Vector2(this.m_Owner.Position.X, this.m_Owner.Position.Y + this.m_Owner.Size.Y) + new Vector2(5,2));
                ShadowText.Draw(string.Format("Replan Time: {0} ms", m_ReplanTimeTaken), pSpriteBatch, _textvector);
                _textvector += new Vector2(0, 20);
                ShadowText.Draw(string.Format("Method: {0}", m_PathfindingName), pSpriteBatch, _textvector);
            }
        }

        public virtual void UpdateStart(Point pStart)
        {
            m_Start = new PathNode() { position = pStart, g = 0, h = 0, f = 0, isOpenList = false, isClosedList = false };
        }

        public virtual void UpdateGoal(Point pGoal)
        {
            m_Goal = new PathNode() { position = pGoal, g = 0, h = 0, f = 0, isOpenList = false, isClosedList = false };
        }
        #endregion

        /// <summary>
        /// Grab the diagonal cost between the two points
        /// </summary>
        /// <param name="pA">From</param>
        /// <param name="pB">To</param>
        /// <returns>The cost as a double</returns>
        protected virtual double Diagonal(PathNode pA, PathNode pB)
        {
            int _distanceX = Math.Abs(pA.position.X - pA.position.X), _distanceY = Math.Abs(pA.position.Y - pA.position.Y);
            float _diagonal = Math.Min(_distanceX, _distanceY);
            float _straight = _distanceX + _distanceY;

            return m_DiagCost * _diagonal + m_StraightCost * (_straight - 2 * _diagonal);
        }

        /// <summary>
        /// Return the Manhattan distance between the two nodes
        /// </summary>
        /// <param name="pA">Typically the current node that we are looking at </param>
        /// <param name="pB">Typically the goal node.</param>
        /// <returns>The manhattan distance between the two nodes.</returns>
        protected virtual double ManhattanDistance(PathNode pA, PathNode pB)
        {
        //    return Math.Abs(pA.position.X - pB.position.X) * m_StraightCost + Math.Abs(pA.position.Y - pB.position.Y) * m_StraightCost;
              return Math.Abs(pA.position.X - pB.position.X) + Math.Abs(pA.position.Y - pB.position.Y) * m_StraightCost;

        }
        
        /// <summary>
        /// Return the G path cost based on the two nodes that we are using
        /// </summary>
        /// <param name="pA">The starting point</param>
        /// <param name="pB">The end point</param>
        /// <returns>Returns the appropriate cost to apply</returns>
        public virtual int DistanceBetween(PathNode pA, PathNode pB)
        {
            return DistanceBetween(pA.position, pB.position);
        }

        /// <summary>
        /// Return whether or not to return diagonal or straight cost
        /// </summary>
        /// <param name="a">The first state that we are getting distance from</param>
        /// <param name="b">Second state that we are getting the distance from</param>
        /// <returns>Returns the G cost traversal from one node to another.</returns>
        public virtual int DistanceBetween(Point a, Point b)
        {
            // Applying the Manhattan distance here
            if ((Math.Abs(a.X - b.X) > 0) &&
                (Math.Abs(a.Y - b.Y) > 0))
                return m_DiagCost;
            else
                return m_StraightCost;
        }

        /// <summary>
        /// Return the Euclidean 
        /// </summary>
        /// <param name="pA">To</param>
        /// <param name="pB">From</param>
        /// <returns>Returns the euclidean distance between the two points</returns>
        protected virtual double EuclideanDistance(PathNode pA, PathNode pB)
        {
            var _distanceX = pA.position.X - pB.position.X;
            var _distanceY = pA.position.Y - pB.position.Y;

            return Math.Sqrt(_distanceX * _distanceX + _distanceY * _distanceY) * m_StraightCost;
        }
    }
}
