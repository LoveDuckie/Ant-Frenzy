using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace AntRunner.Entity
{
    public class Gold : Resource
    {
        #region Constructors
        public Gold() : base()
        {

        }

        public Gold(Vector2 pPosition, float pScale, float pRotation, int pAmount)
            : base (pAmount, new Point(0,2))
        {
            this.Position = pPosition;
            this.Scale = pScale;
            this.Rotation = pRotation;
            this.m_Resources = pAmount;
            this.m_MaxResource = pAmount;
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
            this.Size = new Point(64, 64);
        }
        #endregion

        #region Methods
        public override void Consume(int pAmount, Entity pEntity)
        {
            base.Consume(pAmount, pEntity);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            #region Collision Box Updating
            this.CollisionBox = new BoundingBox(new Vector3(Position, 0),
                                    new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0));

            // Create a new bounding box using the rectangle object
            this.BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
            #endregion

            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            float _normalized = m_Resources / m_MaxResource;

            // Render the sprite to the screen
            pSpriteBatch.Draw(m_SpriteSheet, Position, new Rectangle(m_FrameIndex.X * 64,
                                                                     m_FrameIndex.Y * 64,
                                                                     64,
                                                                     64), 
                                                                     Color.White * _normalized);

            base.Draw(pSpriteBatch);
        }
        #endregion
    }
}
