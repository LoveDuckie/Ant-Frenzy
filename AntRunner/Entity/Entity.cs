using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// Required libraries that are developed internally.
using AntRunner.Particles;
using AntRunner.Utility;

namespace AntRunner.Entity
{
    public class Entity : IEntity, IDisposable
    {
        #region Members
        public static Random        m_Random = new Random();

        private string m_Name = "";
        private bool m_Selected = false;    

        protected Vector2       m_Position;
        protected Texture2D     m_SpriteSheet;
        protected float m_Scale;
        private AABB m_AlignedBox;
        private RigidBody m_RigidBody;

        // Additional members for calculating torque on the given entity.
        protected float m_InvertedMass;

        protected bool          m_AABBCollision = false;
        protected Vector2       m_Origin;
        protected float         m_Rotation;
        protected Rectangle     m_BoundingBox;
        protected bool          m_Dead;

        protected float         m_Mass;
        protected Vector2       m_Acceleration;
        protected Vector2       m_Velocity;

        // Used for determining whether or not there is a collision with the likes of 
        // separating axis theorem
        protected bool m_SATcollision = false;

        private ParticleEmitter m_Emitter;
        protected float         m_Radius;
        protected Vector2       m_TempPosition;
        private Vector2         m_WorldOrigin;

        protected BoundingBox   m_CollisionBox;
        protected Point         m_Size;

        // All the forces that have to be added.
        private Dictionary<string, Vector2> m_Forces = new Dictionary<string, Vector2>();

        // The axes that are going to be used for the SAT collision
        protected bool m_AxesColliding = false;
        protected List<Vector2> m_Axes = new List<Vector2>();
        #endregion

        #region Mutators
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public AABB AlignedBox
        {
            get { return m_AlignedBox; }
            set { m_AlignedBox = value; }
        }

        public Dictionary<string, Vector2> Forces
        {
            get { return m_Forces; }
            set { m_Forces = value; }
        }

        protected ParticleEmitter Emitter
        {
            get { return m_Emitter; }
            set { m_Emitter = value; }
        }

        public float Mass
        {
            get { return m_Mass; }
            set { m_Mass = value; }
        }

        public float Radius
        {
            get { return m_Radius; }
            set { m_Radius = value; }
        }

        public List<Vector2> Axes
        {
            get { return m_Axes; }
            set { m_Axes = value; }
        }

        public Point Size
        {
            get { return m_Size; }
            set { m_Size = value; }
        }

        public BoundingBox CollisionBox
        {
            get { return m_CollisionBox; }
            set { m_CollisionBox = value; }
        }

        public Vector2 Velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }

        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public float Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        public Vector2 Origin
        {
            get { return m_Origin; }
            set { m_Origin = value; }
        }

        public bool Dead
        {
            get { return m_Dead; }
            set { m_Dead = value; }
        }

        protected float Scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        public Vector2 WorldOrigin
        {
            get { return m_WorldOrigin; }
            set { m_WorldOrigin = value; }
        }
        
        public Rectangle BoundingBox
        {
            get { return m_BoundingBox; }
            set { m_BoundingBox = value; }
        }
        #endregion

        // Generate a new list of entites that is going to be used globally.
        private static List<Entity> _entities = new List<Entity>();

        private static List<Items.Item> _items = new List<Items.Item>();

        public static List<Items.Item> GameItems
        {
            get { return _items; }
            set { _items = value; }
        }
        
        public static List<Entity> Entities
        {
            get { return _entities; }
            set { _entities = value; }
        }

        #region Constructors
        public Entity()
        {

        }

        public Entity(float pScale, Vector2 pPosition, Texture2D pSpriteSheet)
        {
            m_Scale = pScale;
            m_Position = pPosition;
            m_SpriteSheet = pSpriteSheet;

            m_AlignedBox = new AABB(Position, 
                                    new Vector2(m_Size.X + Position.X, 
                                                m_Size.Y + Position.Y),this); 

            this.m_Axes = new List<Vector2>();
            this.m_RigidBody = new RigidBody(this);

            this.Initialize();
        }
        #endregion

        /// <summary>
        /// Declaration of the entity class
        /// </summary>
        /// <param name="pScale">How big do we want the entity to appear?</param>
        /// <param name="pPosition">Where in the game world is the entity going to be placed</param>
        /// <param name="pSpriteSheet">The spritesheet that we are going to render from</param>
        /// <param name="pRotation">The rotation in radians for the entity in question</param>
        public Entity(float pScale, Vector2 pPosition, Texture2D pSpriteSheet, float pRotation)
        {
            this.m_Scale = pScale;
            this.m_Position = pPosition;
            this.m_SpriteSheet = pSpriteSheet;
            this.m_Rotation = pRotation;
        
            // Generate a new copy of the object.
            m_AlignedBox = new AABB(Position,
                            new Vector2(m_Size.X + Position.X,
                                        m_Size.Y + Position.Y), this); 

        }

        /// <summary>
        /// Returns whether or the entity is colliding with any of the other entities
        /// in the game world.
        /// </summary>
        /// <param name="pBoundingBox">The temporary bounding box that we're using to compare</param>
        /// <returns>Returns true if there is a collision</returns>
        public virtual bool CollideWithEntities(Rectangle pBoundingBox)
        {
            // Do a linear loop to determine if there is a collision with another entity.
            foreach (var item in Entities)
            {
                if (item.BoundingBox.Intersects(pBoundingBox) && !(item is Bullet) && !(item is Box))
                    return true;
            }

            return false;
        }

        public static int GenerateIntertiaX(List<Vector2> pAxes)
        {
            return 0;
        }

        public static int GenerateInertiaY(List<Vector2> pAxes)
        {
            return 0;
        }

        /// <summary>
        /// Apply "Elastic Collision" with the likes of another object.
        /// </summary>
        /// <param name="pOther">The other object that we are dealing with at the moment</param>
        public void ElasticCollision(Entity pOther)
        {
            // Find out if the entity we're dealing with here is an ant
            if (pOther is Ant)
            {
                pOther.Velocity = pOther.CalculateForces();
            }

            // Get the direction that we want to be dealing with here.
            Vector2 _direction = new Vector2(this.Position.X - pOther.Position.X,
                                             this.Position.Y - pOther.Position.Y);

            float _angle = (float)Math.Atan2(_direction.X, _direction.Y);

            float _magnitudeOne, _magnitudeTwo, _directionOne, _directionTwo;

            /// Grab the magnitudes (length!) of the vectors in question
            _magnitudeOne = (float)Math.Sqrt(this.Velocity.X * this.Velocity.X + this.Velocity.Y * this.Velocity.Y);
            _magnitudeTwo = (float)Math.Sqrt(pOther.Velocity.X * pOther.Velocity.X + pOther.Velocity.Y * pOther.Velocity.Y);

            // Generate the two angles in terms of radians.
            _directionOne = (float)Math.Atan2(this.Velocity.Y, this.Velocity.X);
            _directionTwo = (float)Math.Atan2(pOther.Velocity.Y, pOther.Velocity.X);

            /// Now generate the new speeds by offsetting the angles from each other
            /// and multiplying by the magitude.
            Vector2 _newSpeedOne, _newSpeedTwo, _finalSpeedOne, _finalSpeedTwo;
            _newSpeedOne = new Vector2(_magnitudeOne * (float)Math.Cos(_directionOne - _angle),
                                       _magnitudeOne * (float)Math.Sin(_directionOne - _angle));

            _newSpeedTwo = new Vector2(_magnitudeTwo * (float)Math.Cos(_directionTwo - _angle),
                                       _magnitudeTwo * (float)Math.Sin(_directionTwo - _angle));

            // Generate the final velocity before hand
            // by multiplying the end of mass between one to another
            _finalSpeedOne = new Vector2(((Mass - pOther.Mass) * _newSpeedOne.X + (pOther.Mass + pOther.Mass) * _newSpeedTwo.X) / (Mass + pOther.Mass),
                                          _newSpeedOne.Y);

            _finalSpeedTwo = new Vector2(((Mass + pOther.Mass) * _newSpeedOne.X + (pOther.Mass - Mass) * _newSpeedTwo.X) / (Mass + pOther.Mass),
                                        _newSpeedTwo.Y);

            // Set the newly calculated velocities
            Velocity = _finalSpeedOne;

            // SO MUCH HACKYNESS!
            // Make sure that the other entity is not an ant because
            // we don't wish to perform those operations on them
            if (!(pOther is Ant))
            {
                pOther.Velocity = _finalSpeedTwo;
            }
            else if (pOther is Resource)
            {
                return;
            }
            else
            {
                pOther.Forces["Collision"] += _finalSpeedTwo;
            }
        }

        /// <summary>
        /// Produce torque through using
        /// </summary>
        /// <param name="pOther">The other item that we are calculating torque against</param>
        /// <returns></returns>
        public Vector2 CalculateTorque(Entity pOther)
        {
            Vector2 r;
            Vector2 torque = Vector2.Zero;
            Vector3 torqueV3 = Vector3.Zero;

            r = pOther.Position - pOther.m_WorldOrigin;

            return torque;
        }

        /// <summary>
        /// Perform SAT collision checking by grabbing a list of vertices from either
        /// object and then projecting them onto a separate plane.
        /// </summary>
        /// <param name="pOther">The other list in question</param>
        public bool CheckPolygonCollision(List<Vector2> pOther)
        {
            Vector2 _vectorOffset = Vector2.Zero;
            List<Vector2> m_VerticesOne = GetAxes(false,false);
            
            // Grab the values from the axis projection to determine a collision
            float _minOne, _minTwo, _maxOne, _maxTwo;
            float _testOne = 0f, _testTwo = 0f, _testNum = 0f;

            Vector2 _axis = Vector2.Zero; // The normal axis for projection
            
            // Loop through the vertices and project
            for (int i = 0; i < m_VerticesOne.Count; i++)
            {
                // Find the face normal at the given index on the set of vertices
                _axis = FindNormalAxis(m_VerticesOne, i);

                _minOne = Vector2.Dot(_axis, m_VerticesOne[0]);
                _maxOne = _minOne; // Set max one to be the equal as min one at the moment
              
                // Loop through the other faces of the polygon to determine the lowest
                // and max values for compariso on this projection
                for (int j = 1; j < m_VerticesOne.Count; j++)
                {
                    // Apply the new min and max values where necessary
                    _testNum = Vector2.Dot(_axis, m_VerticesOne[j]);
                    if (_testNum < _minOne) _minOne = _testNum;
                    if (_testNum > _maxOne) _maxOne = _testNum;
                }
            }

            return false;
        }

        /// <summary>
        /// Find the normal between two points. This will be used for calculating
        /// the projection between the points
        /// </summary>
        /// <param name="pVertices">The vertices that we want to determine the axis normals from</param>
        /// <param name="pIndex">What side of the polygon do we want the axis norma lof</param>
        /// <returns>The axis normal between the two given sides</returns>
        private Vector2 FindNormalAxis(List<Vector2> pVertices, int pIndex)
        {
            Vector2 _vectorOne = pVertices[pIndex];
            Vector2 _vectorTwo = pIndex >= pVertices.Count - 1 ? pVertices[0] : pVertices[pIndex + 1];

            // Generate the right-normal axis
            Vector2 _normalAxis = new Vector2(-(_vectorTwo.Y - _vectorOne.Y), 
                                                _vectorTwo.X - _vectorOne.X); 
            
            return _normalAxis;
        }

        /// <summary>
        /// For the seeking movement key, we want to make sure that the speed is truncated
        /// </summary>
        /// <param name="pMaxSpeed">The speed that we want to truncate it by</param>
        public void TruncateMovementVelocity(float pMaxSpeed)
        {
            float _minValue = Math.Min(pMaxSpeed, Forces["Movement"].Length());
            float _newX = (float)Math.Cos(m_Rotation) * _minValue;
            float _newY = (float)Math.Sin(m_Rotation) * _minValue;

            // Set the new values varying on whether the condition is met.
            if (Math.Abs(_newX) < 0.00000001) _newX = 0;
            if (Math.Abs(_newY) < 0.00000001) _newY = 0;

            // Set the new values based on the max speed.
            this.Forces["Movement"] = new Vector2(_newX, _newY);
        }

        /// <summary>
        /// Simple helper function that rotates the entity to another one
        /// </summary>
        /// <param name="pOther">The entity to rotate to </param>
        /// <returns>The angle in radians that we want to rotate to</returns>
        public virtual float RotateTo(Entity pOther)
        {
            return RotateTo(pOther.Position);
        }

        public virtual float RotateTo(Vector2 pOther)
        {
            // That's the distance
            Vector2 _normalized = pOther - Position;

            //_normalized.Normalize();
            return (float)Math.Atan2((double)_normalized.Y, (double)_normalized.X);
        }

        /// <summary>
        /// Sort of redundant considering that we now have the method "GetAxes()"
        /// </summary>
        public virtual void PrepareBox()
        {
            // Make sure that we're dealing with something that is
            // instantiated.
            if (m_Axes != null)
            {
                m_Axes.Clear();

                // Add the center axis to the list
                m_Axes.Add(m_Position + m_Origin);

                // Add the axes to the list
                m_Axes.Add(m_Position);
                m_Axes.Add(new Vector2(m_Position.X + m_Size.X, m_Position.Y));
                m_Axes.Add(new Vector2(m_Position.X + m_Size.X, m_Position.Y + m_Size.Y));
                m_Axes.Add(new Vector2(m_Position.X, m_Position.Y + m_Size.Y));
            }
        }

        // Return the point on the grid in the world.
        public static Point WorldGridPosition(Vector2 pPosition, Point pTileSize)
        {
            return new Point((int)pPosition.X / pTileSize.X,
                             (int)pPosition.Y / pTileSize.Y);
        }

        /// <summary>
        /// Gets the min and max values that are used to compare on the axis
        /// 
        /// We want the furthest and least furthest vectors to use for collision determination
        /// </summary>
        /// <param name="_list">The list that we are going to perform the dot product on to determine
        /// the smallest values.
        /// </param>
        /// <param name="_axis">The vector of the axis that we are going to perform this on</param>
        /// <returns>Returns the axes on the list that are</returns>
        public MinMaxReturn GetMinMax(List<Vector2> _list, Vector2 _axis)
        {
            // Determine the angle bteween the first axis and the 
            var _minprojectionBox = Vector2.Dot(_list[0], _axis);
            var _maxprojectionbox = Vector2.Dot(_list[0], _axis);
            var _minDotBox = 0;
            var _maxDotBox = 0;

            // Don't want the middle one and we've already calculated
            // the dot product above of the first axis in the list
            for (int i = 1; i < _list.Count; i++)
            {
                var _currentprojection = Vector2.Dot(_list[i], _axis);

                // Assign the new values.
                if (_minprojectionBox > _currentprojection)
                {
                    _minprojectionBox = _currentprojection;
                    _minDotBox = i;
                }

                if (_maxprojectionbox < _currentprojection)
                {
                    _maxprojectionbox = _currentprojection;
                    _maxDotBox = i;
                }
            }

            // Return this value for now to get rid of the errors that are appearing
            return new MinMaxReturn() { 
                m_MinIndex = _minDotBox,
                m_MaxIndex = _maxDotBox,
                m_MaxProjection = _maxprojectionbox,
                m_MinProjection = _minprojectionBox
            };
        }

        /// <summary>
        /// Used for returning data from the min max return
        /// </summary>
        public struct MinMaxReturn
        {
            #region Members
            public float m_MinProjection;
            public float m_MaxProjection;
            public int m_MinIndex;
            public int m_MaxIndex;
            #endregion
        }

        /// <summary>
        /// Determine a rotated collision between the two entities
        /// </summary>
        /// <param name="pOther">The other entity that we are comparing against</param>
        /// <returns>Whether or not there was a collision</returns> 
        public virtual bool RotatedIntersection(Entity pOther)
        {
            List<Vector2> _axes = GetAxes(true,false);
            List<Vector2> _otheraxes = pOther.GetAxes(true, false);

            List<Vector2> _normalizedAxesOne = new List<Vector2>();
            
            _normalizedAxesOne.Add(_axes[1] - _axes[0]);
            _normalizedAxesOne.Add(_axes[1] - _axes[2]);

            _normalizedAxesOne.Add(_otheraxes[0] - _otheraxes[3]);
            _normalizedAxesOne.Add(_otheraxes[0] - _otheraxes[1]);

            // Loop through the axes
            foreach (var item in _normalizedAxesOne)
            {
                if (!IsAxisCollision(pOther, item))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Return the values that the object is occupying in the grid space.
        /// </summary>
        /// <returns>Returns whether or not it's occupying the grid spaces.</returns>
        public Point[] OccupyingGridSpace()
        {
            List<Point> CollisionAreas = new List<Point>();
            Level _level = MainGame.Instance.GameState.Level;

            // Add all the nodes that are considered to be occupied
            CollisionAreas.Add(new Point((int)(Position.X - Origin.X) / _level.TMXLevel.TileWidth,
                                         (int)(Position.Y - Origin.Y) / _level.TMXLevel.TileHeight));
            
            CollisionAreas.Add(new Point((int)(Position.X + Size.X - Origin.X) / _level.TMXLevel.TileWidth,
                                         (int)(Position.Y - Origin.Y) / _level.TMXLevel.TileHeight));

            CollisionAreas.Add(new Point((int)(Position.X - Origin.X) / _level.TMXLevel.TileWidth,
                                         (int)(Position.Y + Size.Y - Origin.Y) / _level.TMXLevel.TileHeight));

            CollisionAreas.Add(new Point((int)(Position.X + Size.X - Origin.X) / _level.TMXLevel.TileWidth,
                                         (int)(Position.Y + Size.Y - Origin.Y) / _level.TMXLevel.TileHeight));


            return CollisionAreas.ToArray();
        }

        /// <summary>
        /// Project the corner onto the axis
        /// </summary>
        /// <param name="pBoxCorner">The corner to project onto the provided axis</param>
        /// <param name="pAxis">The axis that we are projecting on</param>
        /// <returns>The value that will determine where on the axis that it is</returns>
        public int GenerateScalar(Vector2 pBoxCorner, Vector2 pAxis)
        {
            float _numerator = (pBoxCorner.X * pAxis.X) + (pBoxCorner.Y + pAxis.Y);
            float _denominator = Vector2.Dot(pAxis, pAxis);
            float _divisionresult = _numerator / _denominator;
            Vector2 _projected = new Vector2(_divisionresult * pAxis.X, _divisionresult * pAxis.Y);

            float _scalar = (pAxis.X * pBoxCorner.X) + (pAxis.Y * pBoxCorner.Y);
            return (int)_scalar;
        }

        // Determine if there is a collision on this provided axis.
        public bool IsAxisCollision(Entity pOther, Vector2 pAxis)
        {
            var _cornersOne = GetAxes(true,false);
            var _cornersTwo = GetAxes(true,false);

            List<int> _boxOneScalars = new List<int>();
           // _boxOneScalars.Add(GenerateScalar(
            return false;
        }

        /// <summary>
        /// Determine whether or not there is a collision even with the rotation
        /// or the sprite.
        /// 
        /// This method utilizies in essence the method of Separating Axis Theorem
        /// 
        /// Utilized the method displayed on http://gamedev.tutsplus.com/tutorials/implementation/collision-detection-with-the-separating-axis-theorem/
        /// 
        /// We assume that the "size" variable is going to be properly initialized.
        /// </summary>
        /// <param name="pOther">The entity that we are comparing against</param>
        /// <returns>Return whether or not there is a collision with the other entity</returns>
        public virtual bool IsRotatedCollision(Entity pOther)
        {
            bool _isSeparated = false;

            // Grab the poinst that we are going to be working with 
            List<Vector2> m_Axes = GetAxes(true,false);
            List<Vector2> m_OtherAxes = pOther.GetAxes(true,false);

            // Grab the outwards facing normals that we need for dot product projection
            List<Vector2> m_NormalsOne = GetAxesNormals();
            List<Vector2> m_NormalsTwo = pOther.GetAxesNormals();

            //m_NormalsOne.Min();

            // First axis that it's meant to be tested on
            var _axisP1 = GetMinMax(m_Axes, m_NormalsOne[1]);
            var _axisP2 = GetMinMax(m_OtherAxes, m_NormalsOne[1]);

            // Second axis that it's meant to be tested on
            var _axisQ1 = GetMinMax(m_Axes, m_NormalsOne[0]);
            var _axisQ2 = GetMinMax(m_OtherAxes,m_NormalsOne[0]);

            // Third axis that it's meant to be tested on
            var _axisR1 = GetMinMax(m_Axes, m_NormalsTwo[1]);
            var _axisR2 = GetMinMax(m_OtherAxes, m_NormalsTwo[1]);

            var _axisS1 = GetMinMax(m_Axes, m_NormalsTwo[0]);
            var _axisS2 = GetMinMax(m_OtherAxes, m_NormalsTwo[0]);

            // Test the conditions now to determine if there is a collision
            bool _separateP = _axisP1.m_MaxProjection < _axisP2.m_MinProjection || _axisP2.m_MaxProjection < _axisP1.m_MinProjection;
            bool _separateR = _axisR1.m_MaxProjection < _axisR2.m_MinProjection || _axisR2.m_MaxProjection < _axisR1.m_MinProjection;
            bool _separateQ = _axisQ1.m_MaxProjection < _axisQ2.m_MinProjection || _axisQ2.m_MaxProjection < _axisQ1.m_MinProjection;
            bool _separateS = _axisS1.m_MaxProjection < _axisS2.m_MinProjection || _axisS2.m_MaxProjection < _axisS1.m_MinProjection;

            _isSeparated = false;
            _isSeparated = _separateP || _separateQ || _separateR || _separateS;

            // Second axis to be tested on
            // var _axisQ1 = GetMinMax(,


            #region Old Code

            // Loop through the axes of the current shape
            //for (int i = 0; i < m_NormalsOne.Count; i++)
            //{
            //    var _minmaxOne = GetMinMax(m_Axes, m_NormalsOne[i]);
            //    var _minmaxTwo = GetMinMax(m_OtherAxes, m_NormalsOne[i]);

            //    // Set whether or not the points went past each other
            //    _isSeparated = _minmaxOne.m_MaxProjection < _minmaxTwo.m_MinProjection ||
            //                   _minmaxTwo.m_MaxProjection < _minmaxOne.m_MinProjection;



            //    if (_isSeparated)
            //    {
            //        break;
            //    }
            //}


            //if (!_isSeparated)
            //{
            //    DebugDraw.DebugDrawBox(true, m_Axes[0], MainGame.Instance.SpriteBatch, Color.Red);
            //}

            //// Only bother carrying on if there is a separation between the two
            //if (!_isSeparated)
            //{
            //    // Loop through the second set of normals to deal with
            //    for (int j = 0; j < m_NormalsTwo.Count; j++)
            //    {
            //        // Get the min and max properties.
            //        var _minmaxOne = GetMinMax(m_Axes, m_NormalsTwo[j]);
            //        var _minmaxTwo = GetMinMax(m_OtherAxes, m_NormalsTwo[j]);

            //        // Set the value as to whether the two boxes are separated or not
            //        _isSeparated = _minmaxOne.m_MaxProjection < _minmaxTwo.m_MinProjection ||
            //                       _minmaxTwo.m_MaxProjection < _minmaxOne.m_MinProjection;

            //        if (_isSeparated)
            //        {
            //            break;
            //        }
            //    }
            //}

            #endregion
            return _isSeparated;
        }

        public int GetIntertiaX()
        {
            return 1;
        }

        public int GetInertiaY()
        {
            return 1;
        }

        /// <summary>
        /// Returns all the axes of the shape with them rotated by the angle at which the entity is rotated to
        /// </summary>
        /// <returns></returns>
        public virtual List<Vector2> GetAxes(bool pApplyOrigin,bool pAddCenter)
        {
            List<Vector2> _templist = new List<Vector2>();

            Vector2 _realworldorigin = Position;

            _realworldorigin = pApplyOrigin == true ? _realworldorigin + Origin : _realworldorigin; 

            // Rotate the vector in question around a particular origin,
            // then place it onto the list.
            if (pAddCenter)
            {
                _templist.Add(this.WorldOrigin);
            }
            _templist.Add(MathUtilities.RotateVector(_realworldorigin, Rotation, Position - Origin));
            _templist.Add(MathUtilities.RotateVector(_realworldorigin, Rotation, new Vector2(Position.X + Size.X - Origin.X,Position.Y - Origin.Y)));
            _templist.Add(MathUtilities.RotateVector(_realworldorigin, Rotation, new Vector2(Position.X + Size.X - Origin.X,Position.Y + Size.Y - Origin.Y)));
            _templist.Add(MathUtilities.RotateVector(_realworldorigin, Rotation, new Vector2(Position.X - Origin.X,Position.Y + Size.Y - Origin.Y)));

            return _templist;
        }

        /// <summary>
        /// For applying friction to a certain vector value
        /// </summary>
        /// <param name="pValue">The vector value that we are going to apply</param>
        /// <param name="pAmount">The amount of friction that we are going to apply</param>
        /// <returns></returns>
        public Vector2 ApplyFriction(Vector2 pValue, float pAmount)
        {
            // Make sure that the length of the vector is a certain amount
            return pValue.Length() > 0.01f ? pValue *= pAmount : Vector2.Zero;
        }


        public float GenerateScalar(Vector2 pAxis)
        {
            return float.MaxValue;
        }

        /// <summary>
        /// Used for SAT collisions
        /// </summary>
        /// <returns>Returns a list of the axes normalised.</returns>
        private List<Vector2> GetAxesNormals()
        {
            List<Vector2> _normals = new List<Vector2>();

            // Retrieve all the points on the entity.
            var _axes = GetAxes(true,true);
            
            // Loop through the axes and generate the left sided normals
            for (int i = 1; i < _axes.Count - 1; i++)
            {
                // Generate the face normal for that face of the polygon by 
                // using those two points
                Vector2 _currentnormal = new Vector2(
                    _axes[i + 1].X - _axes[i].X,
                    _axes[i + 1].Y - _axes[i].Y);

                // Push that to the list.
                _normals.Add(_currentnormal);
            }
        
            // Add the remaining normal to the list
            _normals.Add(new Vector2(_axes[0].X - _axes[_axes.Count - 1].X,
                                                _axes[0].Y - _axes[_axes.Count - 1].Y));

            return _normals;
        }

        /// <summary>
        /// Return the value with the X coordinate sent into the negative
        /// </summary>
        /// <param name="pValue">The value that we are going to change</param>
        /// <returns>The value after it's been transformed</returns>
        public static Vector2 LeftNormal(Vector2 pValue)
        {
            return new Vector2(pValue.Y, -1 * pValue.X);
        }

        /// <summary>
        /// Change the Y coordinate so that the normal is right based.
        /// </summary>
        /// <param name="pValue">The value that we are going to modify</param>
        /// <returns>The normal vetor2 that we want to use</returns>
        public static Vector2 RightNormal(Vector2 pValue)
        {
            return new Vector2(pValue.Y * -1, pValue.X);
        }

        /// <summary>
        /// Apply Cos and Sin transformations using the provided angle on the given axes
        /// </summary>
        /// <param name="pAngle">The rotation that we are to apply</param>
        private void ApplyAngleToAxes(float pAngle)
        {
            // Loop through the axes aside from the first index
            // as that is the center of the bounding box.
            for (int i = 1; i < m_Axes.Count - 1; i++)
            {
                float x_length = 0, y_length = 0;

                // Subtract from the center of the bounding box to do wizardry
                x_length = m_Axes[i].X - m_Axes[0].X;
                y_length = m_Axes[i].Y - m_Axes[0].Y;

                
                m_Axes[i] = new Vector2(x_length * (float)Math.Cos(pAngle) - y_length * (float)Math.Sin(pAngle),
                                        x_length * (float)Math.Sin(pAngle) + y_length * (float)Math.Cos(pAngle));
            }
        }

        /// <summary>
        /// Determine whether or not the other entity is  within a certain radius of this entity.
        /// </summary>
        /// <param name="pOther">The other entity that we are checking against</param>
        /// <returns>Return whether or not the other entity is within the radius that is defined</returns>
        public virtual bool WithinRange(Entity pOther, float pCheckRadius)
        {
            return WithinRange(pOther.Position, pCheckRadius);
        }

        /// <summary>
        /// Check whether or not the provided vector is within the radius that is provided in the second argument
        /// </summary>
        /// <param name="pOther"></param>
        /// <param name="pCheckRadius">The squared radius in which the range will be checked.</param>
        /// <returns></returns>
        public virtual bool WithinRange(Vector2 pOther, float pCheckRadius)
        {
            float _squaredistance = (float)Math.Sqrt(Math.Pow(this.Position.X - pOther.X, 2) +
                                           Math.Pow(this.Position.Y - pOther.Y, 2));

            return _squaredistance <= Math.Pow(pCheckRadius, 2);
        }

        public virtual bool WithinRangeOrigin(Vector2 pOther, float pCheckRadius)
        {
            float _squaredistance = (float)Math.Sqrt(Math.Pow(this.WorldOrigin.X - pOther.X, 2) +
                               Math.Pow(this.WorldOrigin.Y - pOther.Y, 2));

            return _squaredistance <= Math.Pow(pCheckRadius, 2);
        }

        public virtual bool WithinRange(Point pOther, float pCheckRadius)
        {
            return WithinRange(new Vector2(pOther.X, pOther.Y), pCheckRadius);
        }

        public virtual void Initialize()
        {
            this.Dead = false;

        }

        /// <summary>
        /// Grab all the forces from the dictionary and then average them out and return
        /// </summary>
        /// <returns>All the forces that are being applied</returns>
        public virtual Vector2 CalculateForces()
        {
            Vector2 _average = Vector2.Zero;
            int _count = 0;

            foreach (var item in m_Forces)
            {
                // Only want to apply it to the average if it's an actual affecting force.
                if (item.Value != Vector2.Zero)
                {
                    _average += item.Value;
                    _count++;
                }
            }

            return _average / _count;
        }

        public virtual Vector2 MoveTo(Entity pOther, float pSpeed)
        {
            return MoveTo(pOther.Rotation, pSpeed);
        }

        public virtual Vector2 MoveTo(float pRotation, float pSpeed)
        {
            return new Vector2((float)Math.Cos(pRotation), (float)Math.Sin(pRotation)) * pSpeed;
        }
        
        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Continuously update the world origin
            this.m_WorldOrigin = m_Position + m_Origin;

            if (AlignedBox == null)
            {
                AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y), this);
            }

            // Update the aligned box so that we can measure collisions accordingly.
            this.AlignedBox.Update(pGameTime, pInputHandler);
        }

        /// <summary>
        /// The main update loop called on every tick
        /// </summary>
        /// <param name="pGameTime">The delta time object that is used for time based operations</param>
        /// <param name="pInputHandler">The input handler for dealing with input</param>
        /// <param name="pLevel">The level context we are operating in</param>
        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            // Loop through the entities and check for SAT based collisions
            foreach (var item in Entities)
            {
                if (!IsRotatedCollision(item) && 
                    item is Ant && item != this)
                {
                    m_SATcollision = true;
                    break;
                }
                else
                {
                    m_SATcollision = false;
                }

                // Make sure to update the aligned box
                if (AlignedBox != null)
                {
                    // Return whether or not there was a collision
                    if (AlignedBox.Intersects(item.AlignedBox))
                    {
                        m_AABBCollision = true;
                        break;
                    }
                    else
                    {
                        m_AABBCollision = false;
                    }
                }
            }

            

            Update(pGameTime, pInputHandler);
        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
#if DEBUG
            // Display red lines if there is a SAT based collision that has been done.
            Color _collisioncolor = Color.White;
            _collisioncolor = m_SATcollision == true ? Color.Red : Color.White;

            // Only render if we're in DEBUG mode at the moment.
            if (Global.DEBUG)
            {
                // Output the type name of the class
                ShadowText.Draw(this.GetType().ToString(), 
                                pSpriteBatch, 
                                new Vector2(this.Position.X, Position.Y - 25));

                // Draw the axes with the applied rotation on them
                List<Vector2> _allaxes = GetAxes(false,false);
                for (int i = 0; i < _allaxes.Count; i++)
                {
                    if (i == _allaxes.Count - 1)
                    {
                        // Draw the CollisionBox
                        Utility.DebugDraw.DrawDebugLine(new Vector2(_allaxes[i].X, _allaxes[i].Y),
                                                        new Vector2(_allaxes[0].X, _allaxes[0].Y),
                                                        pSpriteBatch, _collisioncolor, 5f, false);
                    }
                    else
                    {
                        Utility.DebugDraw.DrawDebugLine(new Vector2(_allaxes[i].X, _allaxes[i].Y),
                                                        new Vector2(_allaxes[i + 1].X, _allaxes[i + 1].Y),
                                                        pSpriteBatch, _collisioncolor, 5f, false);
                    }
                }

                // Draw the CollisionBox
                Utility.DebugDraw.DrawDebugLine(new Vector2(CollisionBox.Min.X, CollisionBox.Min.Y),
                                                new Vector2(CollisionBox.Max.X, CollisionBox.Min.Y),
                                                pSpriteBatch, Color.White, 5f, false);

                Utility.DebugDraw.DrawDebugLine(new Vector2(CollisionBox.Min.X, CollisionBox.Min.Y),
                                                new Vector2(CollisionBox.Min.X, CollisionBox.Max.Y),
                                                pSpriteBatch, Color.White, 5f, false);

                Utility.DebugDraw.DrawDebugLine(new Vector2(CollisionBox.Min.X, CollisionBox.Max.Y),
                                                new Vector2(CollisionBox.Max.X, CollisionBox.Max.Y),
                                                pSpriteBatch, Color.White, 5f, false);

                Utility.DebugDraw.DrawDebugLine(new Vector2(CollisionBox.Max.X, CollisionBox.Min.Y),
                                                new Vector2(CollisionBox.Max.X, CollisionBox.Max.Y),
                                                pSpriteBatch, Color.White, 5f, false);

                // Draw the BoundingBox
                Utility.DebugDraw.DrawDebugLine(new Vector2(BoundingBox.X, BoundingBox.Y),
                                                new Vector2(BoundingBox.X + Size.X, BoundingBox.Y),
                                                pSpriteBatch, Color.White, 5f, false);


                Utility.DebugDraw.DrawDebugLine(new Vector2(BoundingBox.X + Size.X, BoundingBox.Y),
                                                new Vector2(BoundingBox.X + Size.X, BoundingBox.Y + Size.Y),
                                                pSpriteBatch, Color.White, 5f, false);


                Utility.DebugDraw.DrawDebugLine(new Vector2(BoundingBox.X + Size.X, BoundingBox.Y + Size.Y),
                                                new Vector2(BoundingBox.X, BoundingBox.Y + Size.Y),
                                                pSpriteBatch, Color.White, 5f, false);

                Utility.DebugDraw.DrawDebugLine(new Vector2(BoundingBox.X, BoundingBox.Y + Size.Y),
                                    new Vector2(BoundingBox.X, BoundingBox.Y),
                                    pSpriteBatch, Color.White, 5f, false);

                pSpriteBatch.DrawString(MainGame.Instance.Fonts["astar_font"],
                                        Velocity.ToString(),
                                        Position - new Vector2(0, 15),
                                        Color.White);

                if (AlignedBox != null)
                {
                    AlignedBox.Draw(pSpriteBatch);
                }
            }
#endif
        }

        public void Dispose()
        {
            _entities.Remove(this);
        }
    }
}
