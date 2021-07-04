using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AntRunner.Entity
{
    public class Switch : Entity
    {
        protected List<IToggable> m_Children = new List<IToggable>();
        protected Entity m_LastTouched = null;
        protected bool m_IsOn = false;
        private Point m_FrameIndex = Point.Zero;

        #region Constructors
        public Switch(Vector2 pPosition, float pScale, float pRotation, string pName)
            : base(pScale, pPosition, MainGame.Instance.Textures["terrain_tiles"],pRotation)
        {
            // Store this as the size of the frame of the image that is going to be rendered.
            this.Size = new Point(64, 64);
            this.Name = pName;
        }
            
        public Switch() : base()
        {

        }
        #endregion

        #region Methods
        public void Toggle(Entity pOther)
        {
            this.m_IsOn = m_IsOn == true ? false : true;
            m_LastTouched = pOther;

            foreach (var item in m_Children)
            {
                item.Toggle(pOther);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Provide the appropriate spritesheet index based on the state of the switch
            Point m_DrawIndex = m_IsOn == true ? new Point(3, 8) : new Point(3, 9);

            // Render the sprite of the switch based on the status of it.
            pSpriteBatch.Draw(m_SpriteSheet, Position, new Rectangle(m_DrawIndex.X * 64, 
                                                                     m_DrawIndex.Y * 64,
                                                                     Size.X, Size.Y), Color.White);

            base.Draw(pSpriteBatch);
        }
        #endregion

    }
}
