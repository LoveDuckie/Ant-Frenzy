using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

using AntRunner.Utility;
using AntRunner.Particles;
using AntRunner.Cameras;
using AntRunner.States;
using AntRunner.Tower;

namespace AntRunner.Entity
{
    public class Box : Entity
    {
        #region Members
        private int m_BoxHealth;
        private RigidBody m_Body;
        public const float TERMINAL_VELOCITY = 5f; // At what speed is the box really meant to be moving at?
        #endregion

        public Box()
            : base()
        {

            this.Initialize();
        }

        public Box(Vector2 pPosition)
        {
            this.m_Position = pPosition;
            this.m_BoxHealth = 100;

            this.Initialize();
        }

        /// <summary>
        /// Called to set all the base values that are required by the object in order
        /// to operate accordingly.
        /// </summary>
        public override void Initialize()
        {
            // Set the spritesheet and the size of the entity that we are going to be using
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
            this.m_Size = new Point(64, 64);
            this.m_Origin = new Vector2(m_Size.X / 2, m_Size.Y / 2);
            this.Scale = 1f;
            this.m_Rotation = 0f;
            this.Mass = 25f;
            this.m_Velocity = Vector2.Zero;
            this.m_BoundingBox = new Rectangle((int)m_Position.X + m_Size.X, 
                                               (int)m_Position.Y + m_Size.Y, 
                                               m_Size.X, 
                                               m_Size.Y);

            // To be utilized for determining the physical properties of the entity.
            this.m_Body = new RigidBody(this);

            base.Initialize();
        }

        public void TakeDamage(int pAmount)
        {
            // Make sure that when we substract, that the new amount is going to be higher than
            // 0!!
            if ((this.m_BoxHealth - pAmount) > 0)
            {
                m_BoxHealth -= pAmount;
            }
            else
            {
                this.Dead = true;
                // Generate new particle emitter here.
            }
        }

        public void ApplyImpulseAnt(Ant pOther)
        {
            Vector2 _antVelocity = pOther.CalculateForces();


        }

        // Extended version of the update function
        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            // Used for preventing collisions into the likes of the tiled map
            Vector2 _currentPosition = Position;
            Vector2 _collisionPosition = Vector2.Zero;

            _currentPosition += m_Velocity;


            #region Collision Box Updating
            // Update the bounding box.
            this.BoundingBox = new Rectangle((int)_currentPosition.X,
                                             (int)_currentPosition.Y,
                                             m_Size.X,
                                             m_Size.Y);

            // Update the vector based collision bounding box for usage later.
            this.CollisionBox = new BoundingBox(new Vector3(_currentPosition, 0),
                                                new Vector3(_currentPosition.X + m_Size.X,
                                                            _currentPosition.Y + m_Size.Y,
                                                            0));
            #endregion

            // Determine whether or not there are collisons on the X axis.
            if (pLevel.CheckCollision(_currentPosition,
                                      new Rectangle((int)_currentPosition.X,
                                                    (int)Position.Y,
                                                    Size.X,
                                                    Size.Y)))
            {
                m_Velocity = new Vector2(-m_Velocity.X, m_Velocity.Y);    
            }

            // Determine tile based collisions on the Y axis.
            if (pLevel.CheckCollision(_currentPosition,
                                       new Rectangle((int)Position.X,
                                                     (int)_currentPosition.Y,
                                                     Size.X,
                                                     Size.Y)))
            {
                m_Velocity = new Vector2(m_Velocity.X, -m_Velocity.Y);
            }


            #region Collision Detection with Entities
            // Loop through the entities and determine if a bullet has collided into it.
            foreach (var item in Entity.Entities)
            {
                // TODO: Figure out how to apply impulse physics to the likes of the ants.
                if (item is Box || item is Cake)
                {
                    // Apply some kind of bouncing effecton them
                    if (item.CollisionBox.Contains(CollisionBox) != ContainmentType.Disjoint)
                    {
                        ElasticCollision(item);
                    }
                }
            }
            #endregion

            #region Black Hole Collision Detection
            // Loop through the black holes and check for collisions
            foreach (var item in pLevel.BlackHoles)
            {
                // Determine that there is a collision
                if (new Rectangle((int)_currentPosition.X,
                   (int)Position.Y,
                   Size.X,
                   Size.Y).Intersects(item.BoundingBox))
                {
                    this.Velocity = new Vector2(-Velocity.X, Velocity.Y);
                    return;
                }

                if (new Rectangle((int)Position.X,
                                  (int)_currentPosition.Y,
                                  Size.X,
                                  Size.Y).Intersects(item.BoundingBox))
                {
                    this.Velocity = new Vector2(Velocity.X, -Velocity.Y);
                    return;
                }
            }
            #endregion


            #region Cake Collision Detection
            // Prevent the boxes from blocking the paths.
            foreach (var item in pLevel.Cakes)
            {
                // Determine collisions on the X axis
                if (new Rectangle((int)_currentPosition.X,
                                  (int)Position.Y,
                                  Size.X,
                                  Size.Y).Intersects(item.BoundingBox))
                {
                    this.Velocity = new Vector2(-Velocity.X, Velocity.Y);
                }

                // Determine collisions on the Y axis
                if (new Rectangle((int)Position.X,
                                  (int)_currentPosition.Y,
                                  Size.X,
                                  Size.Y).Intersects(item.BoundingBox))
                {
                    this.Velocity = new Vector2(Velocity.X, -Velocity.Y);
                }
            }

            #endregion

            // Loop through the spawn points and determine a collision
            foreach (var item in pLevel.SpawnPoints)
            {
                if (new Rectangle((int)_currentPosition.X,
                                  (int)Position.Y,
                                  Size.X,
                                  Size.Y).Intersects(item.BoundingBox))
                {
                    this.Velocity = new Vector2(-Velocity.X, Velocity.Y);
                }

                if (new Rectangle((int)Position.X,
                                  (int)_currentPosition.Y,
                                  Size.X,
                                  Size.Y).Intersects(item.BoundingBox))
                {
                    this.Velocity = new Vector2(Velocity.X, -Velocity.Y);
                }
            }


            #region Player Collision Detection
            // Loop through and have the boxes bounce off from the player.
            foreach (var item in MainGame.Instance.GameState.PlayerManager.Players)
            {
                // Reverse the velocity of the item that the box is using to move

                if (new Rectangle((int)_currentPosition.X, (int)Position.Y, Size.X, Size.Y).Intersects(item.BoundingBox))
                {
                    m_Velocity = new Vector2(-m_Velocity.X, m_Velocity.Y);
                }

                if (new Rectangle((int)Position.X, (int)_currentPosition.Y, Size.X, Size.Y).Intersects(item.BoundingBox))
                {
                    m_Velocity = new Vector2(m_Velocity.X, -m_Velocity.Y);
                }
            }
            #endregion

            this.Position += Velocity;
            // Apply the appropriate friction based on what tile that we are on
            this.m_Velocity *= pLevel.TileFriction(this);

            // Update the body so that it can do further mathematics.
            m_Body.Update(pGameTime, pInputHandler);
        }

        /// <summary>
        /// For now, overwrite this while I work out the other logistics.
        /// </summary>
        /// <returns>Returns a list of axes that are used for the SAT collisions</returns>
        public override List<Vector2> GetAxes(bool pApplyOrigin,bool pAddOrigin)
        {
            return base.GetAxes(true,false);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        /// <summary>
        /// Activate the particle emitter for the box and break it up into 4 pieces.
        /// </summary>
        public void Explosion()
        {
            // Generate a new particle emitter to depict the exploision of the box
            //ParticleEmitter.Emitters.Add(new SpriteSheetEmitter(4,
            //                                                    MainGame.Instance.Textures["terrain_tiles"],
            //                                                    new Point(64,64),
            //                                                    new Point(4,0));

        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            int _damagespriteX = 0;
            int _damagespriteY = 0;
            
            // Render the sprite to the screen by single out the wooden box that we are after.
            pSpriteBatch.Draw(
                m_SpriteSheet, 
                new Vector2(m_Position.X + 32, m_Position.Y + 32), 
                new Rectangle(4 * 64, 0, 64, 64), 
                Color.White,
                m_Rotation,
                m_Origin,
                Scale,
                SpriteEffects.None,
                0f);

            #region Box Damage Overlay
            // Draw the overlay for the kind of damage that has been inflicted on the box.
            if (m_BoxHealth > 80 && m_BoxHealth < 99)
            {
                _damagespriteX = 0;
                _damagespriteY = 15;
            }
            else if (m_BoxHealth < 80 && m_BoxHealth > 60)
            {
                _damagespriteX = 2;
                _damagespriteY = 15;
            }
            else if (m_BoxHealth <= 60 && m_BoxHealth > 40)
            {
                _damagespriteX = 4;
                _damagespriteY = 15;
            }
            else if (m_BoxHealth <= 40 && m_BoxHealth > 20)
            {
                _damagespriteX = 6;
                _damagespriteY = 15;
            }
            else if (m_BoxHealth <= 20 && m_BoxHealth > 0)
            {
                _damagespriteX = 8;
                _damagespriteY = 15;
            }
            #endregion

            #region Box Spritesheet Rendering
            // Draw the overlay that we are after to represent the damage.
            if (m_BoxHealth != 100)
            {

                pSpriteBatch.Draw(m_SpriteSheet,
                    new Vector2(m_Position.X + 32, m_Position.Y + 32),
                    new Rectangle(_damagespriteX * 64, 15 * 64, 64, 64),
                    Color.White,
                    m_Rotation,
                    m_Origin,
                    Scale,
                    SpriteEffects.None,
                    0f);

            }
            #endregion

#if DEBUG
            // Render stuff if we're in debug mode.
            if (Global.DEBUG)
            {

                // Render the grid squares that the box is currently occupying.


                
                //pSpriteBatch.Draw(MainGame.Instance.Textures["bezier_dot"], new Vector2(BoundingBox.X, BoundingBox.Y), Color.White);

                //pSpriteBatch.Draw(MainGame.Instance.Textures["bezier_dot"], new Vector2(BoundingBox.X + BoundingBox.Width, BoundingBox.Y + BoundingBox.Height), Color.White);


            }
                ShadowText.Draw(m_BoxHealth.ToString(), pSpriteBatch, new Vector2(m_Position.X, m_Position.Y - 20));
#endif

                        // Output the points on the axes list so that we can see if the 
            // collision is really happening
            #if DEBUG
            //for (int i = 0; i < m_Axes.Count; i++)
            //{
            //    pSpriteBatch.Draw(MainGame.Instance.Textures["bullet"], m_Axes[i], Color.White);
            //}
            #endif

            base.Draw(pSpriteBatch);
        }
    }
}
