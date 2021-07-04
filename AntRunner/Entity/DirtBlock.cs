using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

namespace AntRunner.Entity
{
    /// <summary>
    /// This will be a destroyable entity within the game world.
    /// </summary>
    public class DirtBlock : Entity, IDamageable
    {
        #region Members
        private int m_Health = 150;
        Point m_FrameIndex = Point.Zero;
        #endregion

        #region Constructors
        public DirtBlock() 
            : base()
        {

        }

        public DirtBlock(Vector2 pPosition, float pRotation, float pScale)
            : base(pScale,pPosition,MainGame.Instance.Textures["terrain_tiles"])
        {
            this.Rotation = pRotation;
            this.Scale = pScale;
            this.Position = pPosition;
            this.m_FrameIndex = new Point(6, 0);
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
            this.Size = new Point(64, 64);
        }
        #endregion

        #region Methods
        public void Destroy()
        {
            // Declare the entity as dead and then be done with it
            this.Dead = true;
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Output the image of the dirt block to the screen
            pSpriteBatch.Draw(m_SpriteSheet,
                              Position,
                              new Rectangle(
                                  (int)m_FrameIndex.X * 64,
                                  (int)m_FrameIndex.Y * 64,
                                  64,
                                  64),Color.White);

            base.Draw(pSpriteBatch);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }
        #endregion

        public void Damage(int pAmount)
        {
            this.m_Health = Math.Min(0, m_Health -= pAmount);
        }
    }
}
