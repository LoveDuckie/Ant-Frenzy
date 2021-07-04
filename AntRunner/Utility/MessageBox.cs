using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Utility;
using AntRunner.Entity;

namespace AntRunner.Utility
{
    // Used for demonstrating a scrolling message onto the screen.
    public class MessageBox : IEntity 
    {
        private string m_Message;
        private Texture2D m_BackgroundTexture;
        private Vector2 m_Position;
        private int m_ScrollIndex;
        private float m_ScrollSpeed;
        private int m_CharactersPerLine = 0;

        #region Properties
        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }
        #endregion

        // What to do when there is no more text to display.
        private EventHandler m_OnComplete = delegate(object sender, EventArgs e) { };

        #region Properties
        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }
        #endregion

        public MessageBox(string pMessage, float pScrollSpeed)
        {
            m_ScrollIndex = 0;
            m_ScrollSpeed = pScrollSpeed;
            m_BackgroundTexture = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice, 600, 200, Color.Black);
        }

        public void Initialize()
        {
        
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            string[] _messageSplit = m_Message.Split(' ');

            

            pSpriteBatch.End();
        
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        }
    }

}
