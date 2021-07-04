using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;
using AntRunner.States;
using AntRunner.Tower;
using AntRunner.Utility;

namespace AntRunner.Entity
{
    public class Bullet : Entity
    {
        #region Members
        private int m_Damange;
        private RigidBody m_Body;
        private Player m_Owner;
        protected Weapon m_WeaponOrigin;
        #endregion

        #region Properties
        public int Damange
        {
            get { return m_Damange; }
            set { m_Damange = value; }
        }

        public Weapon WeaponOrigin
        {
            get { return m_WeaponOrigin; }
            set { m_WeaponOrigin = value; }
        }
        #endregion

        #region Constructors
        public Bullet(float pBulletSpeed, Vector2 pVelocity, float pRotation, Texture2D pBulletTexture, Vector2 pPosition, Weapon pWeaponOrigin) : base()
        {
            // Set the value that is going to be used for teh velocity.
            this.Velocity = pVelocity;
            this.m_SpriteSheet = pBulletTexture;
            this.Rotation = pRotation;
            this.m_Position = pPosition;
            this.Scale = 1.0f;
            this.m_Size = new Point(m_SpriteSheet.Width, m_SpriteSheet.Height);
            this.m_Origin = new Vector2(this.m_SpriteSheet.Width / 2, this.m_SpriteSheet.Height / 2);
            this.WorldOrigin = m_Position + m_Origin;
            this.BoundingBox = new Rectangle((int)m_Position.X, (int)m_Position.Y, m_SpriteSheet.Width, m_SpriteSheet.Height);
            this.WeaponOrigin = pWeaponOrigin;
            this.Mass = 15.0f;

            this.Initialize();
        }
        #endregion

        public override void Initialize()
        {
            // Set up the rigid body that is going to be used.
            this.m_Body = new RigidBody(this);           
            
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            // Increase it accordingly.
            Vector2 _currentPosition = Position;

            _currentPosition += Velocity;

            this.m_CollisionBox = new BoundingBox(new Vector3(Position, 0), 
                                                  new Vector3(Position.X + m_Size.X, 
                                                              Position.Y + m_Size.Y, 0));
            
            this.BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, m_SpriteSheet.Width, m_SpriteSheet.Height);

            // Determine if 
            if (pLevel.CheckCollision(this.m_Position, new Rectangle((int)m_Position.X - (int)this.m_Origin.X, (int)m_Position.Y - (int)this.m_Origin.Y, m_SpriteSheet.Width, m_SpriteSheet.Height)))
            {
                this.m_Dead = true;
            }

            // Check for collisions with boxes within the environment.
            foreach (var item in Entities)
            {
                // Determine that we are dealing with a box here
                if (item is Box && item.CollisionBox.Contains(new BoundingBox(
                    new Vector3(_currentPosition,0),
                    new Vector3(_currentPosition.X + m_Size.X, _currentPosition.Y + m_Size.Y, 0f)
                    )) != ContainmentType.Disjoint && !Dead)
                {
                    ((Box)item).ElasticCollision(this);
                    ((Box)item).TakeDamage(5);
                    this.Dead = true;
                }
            }

            // Determine if the bullet has gone out of the player area.
            // If it has, then get rid of it.
            if ((this.m_Position.X + m_SpriteSheet.Width) > (pLevel.TMXLevel.Width * pLevel.TMXLevel.TileWidth) ||
                (this.m_Position.Y + m_SpriteSheet.Width) > (pLevel.TMXLevel.Height * pLevel.TMXLevel.TileHeight) ||
                (this.m_Position.X < 0) || (this.m_Position.Y < 0))
            {
                this.Dead = true;
            }

            this.Position += Velocity;

            // Go through the other update method and od the other updates
            // required there
            Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Draw the given bullet at the target that is specified.
            pSpriteBatch.Draw(m_SpriteSheet, m_Position,null,Color.White,m_Rotation,m_Origin,Scale,SpriteEffects.None,0);

            base.Draw(pSpriteBatch);
        }

    }
}
