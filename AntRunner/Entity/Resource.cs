using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input;

namespace AntRunner.Entity
{
    public class Resource : Entity
    {
        protected int m_Resources;
        protected int m_MaxResource;
        protected Point m_FrameIndex = Point.Zero;
        protected Entity m_LastConsumed = null;

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public Resource()
        {
            m_Resources = 100;
        }

        /// <summary>
        /// The main constructor
        /// </summary>
        /// <param name="pAmount">The amount that we wish the resource to have.</param>
        public Resource(int pAmount, Point pFrameIndex)
        {
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
            this.m_Resources = pAmount;
            this.m_FrameIndex = pFrameIndex;
        }

        #endregion

        /// <summary>
        /// Consume the resource in question by a fixed amount and 
        /// </summary>
        /// <param name="pAmount">Consume this amount of the resource</param>
        /// <param name="pEntity">The entity that is doing the consuming</param>
        public virtual void Consume(int pAmount, Entity pEntity)
        {
            m_Resources = Math.Max(0, m_Resources -= pAmount);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Have this loop cotinuously.
        /// </summary>
        /// <param name="pGameTime">Delta time object that is to be used</param>
        /// <param name="pInputHandler">Input handler object for usage</param>
        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            

            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Render the sprite out
            pSpriteBatch.Draw(m_SpriteSheet, Position, new Rectangle(m_FrameIndex.X * 64,
                                                                   m_FrameIndex.Y * 64,
                                                                   64, 64), Color.White);

            base.Draw(pSpriteBatch);
        }
    }
}
