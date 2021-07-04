using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// The required services for XNA to work
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

// Required includes
using AntRunner.States;
using AntRunner.Cameras;
using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.Tower;
using AntRunner.Utility;

namespace AntRunner.Entity
{
    /// <summary>
    /// This box will contain the proper rigid body that we want it to have.
    /// </summary>
    public class NewBox : Entity
    {
        #region Members
        private Point m_FrameIndex = new Point(0, 4);
        private Point m_FrameSize = new Point(64, 64);
        #endregion

        #region Const Variables
        private const float MIN_RANDOM_MOVEMENT_TIME = 2000f;
        private const float MAX_RANDOM_MOVEMENT_TIME = 5000f;

        private const float MIN_RANDOM_MOVEMENT_SPEED = 4.0f;
        private const float MAX_RANDOM_MOVEMENT_SPEED = 7.0f;
        #endregion

        private float m_RandomForceSpeed = 0;
        private float m_BounceTimer = 0f;

        #region Constructors
        public NewBox(Vector2 pPosition, float pScale, float pRotation)
        {
            m_Size = new Point(64, 64);
            m_Rotation = pRotation;
            m_Position = pPosition;
            m_Scale = pScale;
            m_Origin = new Vector2(Size.X / 2, Size.Y / 2);

            this.Mass = 25f;
            this.Velocity = Vector2.Zero;

            

            this.Position = new Vector2(Position.X + (Size.X / 2), Position.Y + (Size.Y / 2));

            // Generate the timer that is to be used.
            this.m_BounceTimer = GenerateRandomTime(MIN_RANDOM_MOVEMENT_TIME, MAX_RANDOM_MOVEMENT_TIME);

            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            base.Initialize();
        }

        public void ApplyRandomForce()
        {
            // Generate a random direction for us to move in
            float _randomDirection = MathHelper.ToRadians(m_Random.Next(0, 359));
            float _randomMovement = GenerateRandomForce(MIN_RANDOM_MOVEMENT_SPEED, MAX_RANDOM_MOVEMENT_SPEED);
            
            // Return the velocity that is to be used for the directional force.
            this.Velocity += new Vector2((float)Math.Cos(_randomDirection), (float)Math.Sin(_randomDirection)) * _randomMovement;
        }

        public float GenerateRandomTime(float pMin, float pMax)
        {
            // Return a random number that is amplified by the max number
            return Environment.TickCount + (pMin + (float)m_Random.NextDouble() * (pMax - pMin));
        }

        public float GenerateRandomForce(float pMin, float pMax)
        {
            return pMin + (float)m_Random.NextDouble() * (pMax - pMin);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {


            base.Update(pGameTime, pInputHandler);
        }

        // Return the surrounding points in question
        public Point[] SurroundingPoints()
        {
            // Grab the nodes that we are occupying, and then determine
            // if one node out whether or not they are there still.
            Point[] _points = OccupyingGridSpace();

            Point _min = new Point(_points[0].X - 1, _points[0].Y - 1);
            Point _max = new Point(_points[3].X + 1, _points[3].Y + 1);

            List<Point> _newpoints = new List<Point>();

            // Add the new list of points
            for (int x = _min.X; x <= _max.X; x++)
            {
                for (int y = _min.Y; y <= _max.Y; y++)
                {
                    _newpoints.Add(new Point(x, y));
                }
            }

            return _newpoints.ToArray();
        }

        public bool AdjacentAnts()
        {
            Level _level = MainGame.Instance.GameState.Level;

            // Grab the nodes that we are occupying, and then determine
            // if one node out whether or not they are there still.
            Point[] _points = OccupyingGridSpace();

            Point _min = new Point(_points[0].X - 1, _points[0].Y - 1);
            Point _max = new Point(_points[3].X + 1, _points[3].Y + 1);

            List<Point> _newpoints = new List<Point>();

            // Add the new list of points
            for (int x = _min.X; x <= _max.X; x++)
            {
                for (int y = _min.Y; y <= _max.Y; y++)
                {
                    _newpoints.Add(new Point(x, y));
                }
            }

            // Loop through the ants in the entities list.
            foreach (var item in Entity.Entities)
            {
                // Check to see if there is an ant there.
                if (item is Ant)
                {
                    if (_newpoints.ToArray().Contains(new Point((int)item.Position.X / _level.TMXLevel.TileWidth,
                                                                (int)item.Position.Y / _level.TMXLevel.TileHeight)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            Vector2 _currentPosition = Position;

            if (!AdjacentAnts())
            {
                if (m_BounceTimer < Environment.TickCount)
                {
                    // Generate a random time for this to next happen again
                    m_BounceTimer = GenerateRandomTime(MIN_RANDOM_MOVEMENT_TIME, MAX_RANDOM_MOVEMENT_TIME);
                    ApplyRandomForce();
                }


                _currentPosition += Velocity;

            }

            #region Collision Box Updating
            /// Update the collision boxes.
            this.CollisionBox = new BoundingBox(new Vector3(_currentPosition - Origin, 0),
                                                new Vector3(_currentPosition.X - Origin.X + m_Size.X,
                                                            _currentPosition.Y - Origin.Y + m_Size.Y,
                                                            0));
            this.BoundingBox = new Rectangle((int)_currentPosition.X - (Size.X / 2),
                                             (int)_currentPosition.Y - (Size.Y / 2),
                                             Size.X,
                                             Size.Y);
            #endregion

            if (pLevel.CheckCollision(_currentPosition,
                          new Rectangle((int)_currentPosition.X - (int)Origin.X,
                                        (int)Position.Y - (int)Origin.Y,
                                        Size.X,
                                        Size.Y)))
            {
                m_Velocity = new Vector2(-m_Velocity.X, m_Velocity.Y);
            }

            // Determine tile based collisions on the Y axis.
            if (pLevel.CheckCollision(_currentPosition,
                                       new Rectangle((int)Position.X - (int)Origin.X,
                                                     (int)_currentPosition.Y - (int)Origin.Y,
                                                     Size.X,
                                                     Size.Y)))
            {
                m_Velocity = new Vector2(m_Velocity.X, -m_Velocity.Y);
            }

            foreach (var item in Entities)
            {
                if (item is NewBox)
                {
                    // Determine if there is some kind of collision with the node.
                    if (item.CollisionBox.Contains(CollisionBox) != ContainmentType.Disjoint)
                    {
                        ElasticCollision(item);
                    }
                }

                if (item is Resource)
                {
                    // Check to see if there is some kind of collision that has occurred
                    if (item.CollisionBox.Contains(CollisionBox) != ContainmentType.Disjoint)
                    {
                        ElasticCollision(item);
                    }
                }
            }


            if (!AdjacentAnts())
            {
                // Apply the velocity onto the ants position.
                this.Position += Velocity;
                this.Velocity *= pLevel.TileFriction(this);
            }

            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            Level _level = MainGame.Instance.GameState.Level;

            Point[] _surroundingPoints = SurroundingPoints();


            if (Global.DEBUG)
            {

                Vector2 _drawdownVector = new Vector2(Position.X, Position.Y);

                // Draw the shadow text.
                ShadowText.Draw(m_RandomForceSpeed.ToString(), pSpriteBatch, new Vector2(Position.X, Position.Y + Size.Y));
                ShadowText.Draw(
                    m_BounceTimer.ToString(), 
                    pSpriteBatch, 
                    new Vector2(Position.X, Position.Y + Size.Y + 25));

                // Loop through the grid spaces and display what points in the graph that they are occupying.
                Point[] _gridspaces = OccupyingGridSpace();

                // Loop through the surrounding points
                foreach (var item in _surroundingPoints)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"],
                                      new Vector2(item.X * _level.TMXLevel.TileWidth, item.Y * _level.TMXLevel.TileHeight),
                                      Color.White);
                }

                // Loop through the grid spaces and draw them out appropriately.
                for (int i = 0; i < _gridspaces.Length; i++)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], 
                        new Vector2(_gridspaces[i].X * _level.TMXLevel.TileWidth, 
                                    _gridspaces[i].Y * _level.TMXLevel.TileHeight), 
                                    Color.HotPink);
                }

            }

            // Draw the box to the screen that we are going to be seeing
            pSpriteBatch.Draw(m_SpriteSheet,
                              new Vector2(m_Position.X, m_Position.Y),
                              new Rectangle(6 * 64, 12 * 64, 64, 64),
                              Color.White,
                              Rotation,
                              Origin,
                              Scale,
                              SpriteEffects.None,
                              0f);

            base.Draw(pSpriteBatch);
        }
        #endregion
    }
}
