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

using AntRunner.Entity;

namespace AntRunner.Utility
{
    public class Narrator : IEntity
    {
        private Texture2D m_BackgroundTexture;
        private Texture2D m_FaceSprite;
        private Vector2 m_Position; // Where in world space is it going to start rendering?
        private Point m_FrameSize;
        private int m_AnimationIndex;
        private float m_AnimationTime;

        private float m_ScrollSpeed;
        private float m_ScrollCounter;

        private bool m_Display = false;

        private int m_CharacterIndex;
        private string m_Message;
        private int m_MaxCharactersPerLine = 0;

        private bool m_CanClose = false;

        #region Properties
        public bool Display
        {
            get { return m_Display; }
            set { m_Display = value; }
        }
        #endregion

        public Narrator(Vector2 pPosition, bool pCanClose)
        {
            this.m_CanClose = pCanClose;
            this.m_Position = pPosition;
            this.m_AnimationIndex = 0;
            this.m_CharacterIndex = 0;
            

            // Make sure that the required texture is available.
            if (MainGame.Instance.Textures["narrator"] != null)
            {
                this.m_FaceSprite = MainGame.Instance.Textures["narrator"];
            }

            this.m_ScrollCounter = 0f;
            this.m_ScrollSpeed = 250f;

            this.m_AnimationTime = 750f;
        }

        /// <summary>
        /// Set the new text to be narrated and set the index back to 0
        /// </summary>
        /// <param name="pMessage">The message that we want to be displayed</param>
        public void SetAndReset(string pMessage)
        {
            m_CharacterIndex = 0;
            m_Message = pMessage;
        }

        public void Initialize()
        {
            this.m_BackgroundTexture = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice, 350, 191, Color.Black);
        }

        /// <summary>
        /// Called on every tick for when the display is appearaing
        /// </summary>
        /// <param name="pGameTime">The delta time object that is used</param>
        /// <param name="pInputHandler">The input handler object.</param>
        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Make sure that we've hit the scroll counter.
            if (Environment.TickCount > m_ScrollCounter)
            {
                m_ScrollCounter = Environment.TickCount + m_ScrollSpeed;
                if (m_CharacterIndex < m_Message.Length) 
                    m_CharacterIndex++;
            }

            // If the enter key has been hit, then finish the narration and get back to normal
            if (m_CanClose && pInputHandler.IsKeyDownOnce(Keys.Enter))
            {
                if (m_CharacterIndex < m_Message.Length)
                    m_CharacterIndex = m_Message.Length - 1;
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Display the narrator if it's been enabled.
            if (Display)
            {
                pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                   pSpriteBatch.Draw(m_BackgroundTexture, m_Position, Color.White * 0.5f);
                pSpriteBatch.End();
            }    
        }


        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
    }
}
