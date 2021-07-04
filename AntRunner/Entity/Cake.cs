using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AntRunner.Entity
{
    public class Cake : Entity
    {
        #region Members
        // The amount of cake left that can be eaten
        private int m_RemainingCake;

        // Size of the cake that is going to be renderered
        private Point m_FrameSize;

        // For when an ant has taken a slice of the cake.
        private int m_EatenColour;
        #endregion

        #region Constructors
        public Cake()
        {
            this.Initialize();
        }

        public Cake(int pRemainingCake)
        {
            m_RemainingCake = pRemainingCake;
            this.Initialize();
        }

        public Cake(Vector2 pPosition)
        {
            this.m_Position = pPosition;
            this.Initialize();
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];

            this.m_Rotation = 0f;
            this.Scale = 1f;

            m_EatenColour = 255;

            // Populate the cake if the remaining amount is currently 0
            if (m_RemainingCake == 0)
            {
                m_RemainingCake = 100;
            }

            this.m_FrameSize = new Point(64, 64);

            // Going to be used for determining the center point of the image
            this.m_Origin = new Vector2(m_FrameSize.X / 2, 
                                        m_FrameSize.Y / 2);
            
            // Determine the size of the entity.
            this.Size = new Point(64, 64);

            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Update the collision box for usage
            this.CollisionBox = new BoundingBox(new Vector3(Position - Origin, 0), 
                                                new Vector3(Position.X - Origin.X + Size.X, 
                                                            Position.Y - Origin.Y + Size.Y, 0));

            // Generate a new bounding box that we are going to be using
            this.BoundingBox = new Rectangle((int)Position.X - (int)Origin.X, 
                                             (int)Position.Y - (int)Origin.Y,
                                             Size.X,
                                             Size.Y);

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            //12x8
            // Output the cake to the screen!
            pSpriteBatch.Draw(m_SpriteSheet, 
                new Vector2(Position.X + m_Origin.X,Position.Y + m_Origin.Y), 
                new Rectangle(12 * 64, 8 * 64, 64, 64), 
                Color.White, 
                m_Rotation, 
                m_Origin, 
                Scale, 
                SpriteEffects.None, 
                0f);

            // Output the amount of cake that is left
            Utility.ShadowText.Draw(this.m_RemainingCake.ToString(), 
                pSpriteBatch, 
                m_Position - new Vector2(0, 15));
            
            base.Draw(pSpriteBatch);
        }

        /// <summary>
        /// For when the ants acquire the cake that we are after.
        /// </summary>
        /// <param name="pAmount">The amount of the cake that is to be taken.</param>
        public void TakeCake(int pAmount)
        {
            if ((m_RemainingCake - pAmount) > 0)
            {
                m_RemainingCake -= pAmount;
            }
            else
            {
                this.Dead = true;
            }
        }
        #endregion
    }
}
