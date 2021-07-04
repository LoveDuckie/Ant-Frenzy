using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

using AntRunner.Entity;

namespace AntRunner.Utility
{
    public class AABB : IEntity
    {
        #region Members
        // Modifiable properties
        public Vector2 Min { get; set; }
        public Vector2 Max { get; set; }
        private Entity.Entity Parent;
        public bool IsColliding { get; set; }

        private int Height { get; set; }
        private int Width { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// The main constructor for the item in question
        /// </summary>
        /// <param name="pMin">The minimum part of the bounding box</param>
        /// <param name="pMax">The max vector of the bounding box</param>
        /// <param name="pParent">The entity that this AABB belongs to</param>
        public AABB(Vector2 pMin, Vector2 pMax, Entity.Entity pParent)
        {
            this.Min = pMin;
            this.Max = pMax;
            this.Parent = pParent;
        }
        #endregion
        
        /// <summary>
        /// For determining whether or not there is an intersection within the environment.
        /// </summary>
        /// <param name="pOther">The other Axis Aligned Bounding Box</param>
        /// <returns>There is an intersection</returns>
        public bool Intersects(AABB pOther)
        {
            if (pOther != null)
            {
                // Return whether or not there is an intersection
                return (this.Min.X <= pOther.Min.X + pOther.Max.X &&
                        pOther.Min.X <= this.Min.X + this.Max.X &&
                        this.Min.Y <= pOther.Min.Y + pOther.Max.Y &&
                        pOther.Min.Y <= this.Min.Y + this.Max.Y);
            }

            return false;
        }

        // Return what type of collision is being made
        public CollisionType Collides(AABB pOther)
        {
            return CollisionType.Intersected;
        }

        /// <summary>
        /// Determine whether or not this AABB contains the other one in question
        /// </summary>
        /// <param name="pOther">The other AABB that we're checking against</param>
        /// <returns></returns>
        public bool Contains(AABB pOther)
        {
            return false;
        }

        // Called when the object is created for the first time
        public void Initialize()
        {

        }

        /// <summary>
        /// Return a list starting from 0,0 of all the axes that represent the AABB
        /// </summary>
        /// <returns>Returns a list of axes for the bounding box</returns>
        public List<Vector2> GetAxes()
        {
            List<Vector2> _returnlist = new List<Vector2>();

            _returnlist.Add(Min);
            _returnlist.Add(new Vector2(Min.X + Max.X, Min.Y));
            _returnlist.Add(Min + Max);
            _returnlist.Add(new Vector2(Min.X, Min.Y + Max.Y));

            return _returnlist;
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Grab a list of axes that have been rotated and translated.
            var _axes = Parent.GetAxes(false,false);

            // The values that are to be used
            float _minX , _minY, _maxX, _maxY;

            // Set the values that we are going to be comparing against.
            _minX = _maxX = _axes[0].X;
            _minY = _maxY = _axes[0].Y;

            // Loop through the axes and determine what it is that you're after.
            for (int i = 1; i < _axes.Count; i++)
            {
                // Assign the new X and Y values for the bounding box
                if (_axes[i].X < _minX) _minX = _axes[i].X;
                if (_axes[i].X > _maxX) _maxX = _axes[i].X;
                if (_axes[i].Y < _minY) _minY = _axes[i].Y;
                if (_axes[i].Y > _maxY) _maxY = _axes[i].Y;
            }

            this.Min = Parent.Position - Parent.Origin;
            this.Max = new Vector2((Parent.Position.X - Parent.Origin.X) + Parent.Size.X,
                                   (Parent.Position.Y - Parent.Origin.Y) + Parent.Size.Y);

            // Generate the width and height from the min and max.
            this.Width = (int)_maxX - (int)_minX;
            this.Height = (int)_maxY - (int)_minY;

            // Generate the new vectors out from the calculations
            this.Min = new Vector2(_minX, _minY);
            this.Max = new Vector2(_maxX, _maxY);

            //// Loop through the axes and determine whether the points are bigger or smaller.
            //for (int i = 0; i < _axes.Count; i++)
            //{
            //    // Do comparison checks and determine that the AABB has the smallest
            //    // and larges coordinates for it's box.
            //    if (Min.X > _axes[i].X)
            //        Min = new Vector2(_axes[i].X, Min.X);
            //    if (Min.Y > _axes[i].Y)
            //        Min = new Vector2(Min.Y, _axes[i].Y);

            //    if (Max.X < _axes[i].X)
            //        Max = new Vector2(_axes[i].X, Max.X);
            //    if (Max.Y < _axes[i].Y)
            //        Max = new Vector2(Max.Y, _axes[i].Y);
            //}
        }

        /// <summary>
        /// Render the lines that depict where the bounding areas of the box are.
        /// </summary>
        /// <param name="pSpriteBatch">SpriteBatch object used for rendering</param>
        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Only draw the wireframe of the AABB if the debug mode is enabled.
            if (Global.DEBUG)
            {
                // Draw the outline of the boxes appropriately.
                Utility.DebugDraw.DrawDebugLine(Min, new Vector2(Max.X,Min.Y), pSpriteBatch, Color.Purple, 1f, false);
                Utility.DebugDraw.DrawDebugLine(new Vector2(Max.X, Min.Y), Max, pSpriteBatch, Color.Purple, 1f, false);
                Utility.DebugDraw.DrawDebugLine(Max, new Vector2(Min.X, Max.Y), pSpriteBatch, Color.Purple, 1f, false);
                Utility.DebugDraw.DrawDebugLine(new Vector2(Min.X, Max.Y), Min, pSpriteBatch, Color.Purple, 1f, false);
            }
        }


        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            this.Update(pGameTime, pInputHandler);
        }
    }
}
