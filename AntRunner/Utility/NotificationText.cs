using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Utility
{
    public class NotificationText : Entity.Entity
    {
        #region Containers
        private static List<NotificationText> m_Notifications = new List<NotificationText>();
        #endregion

        #region Members
        protected string m_TextToDisplay;
        protected int m_Opacity;
        protected SpriteFont m_Font;
        protected bool m_MoveUp;
        protected float m_MoveUpSpeed;
        protected Color m_TextColor;
        protected bool m_Shadow;
        protected bool m_BounceIn;
        protected float m_Scale;
        protected const float BOUNCE_IN_SCALE = 2f;
        #endregion

        #region Properties
        public static List<NotificationText> Notifications
        {
            get { return NotificationText.m_Notifications; }
            set { NotificationText.m_Notifications = value; }
        }
        #endregion

        #region Constructors
        public NotificationText(bool pBounceIn, string pText, Vector2 pPosition, bool pMoveUp, Color pTextColor, bool pShadow)
        {
            this.m_TextToDisplay = pText;
            this.m_Position = pPosition;
            this.m_MoveUp = pMoveUp;
            this.m_TextColor = pTextColor;
            this.m_Shadow = pShadow;
            this.m_Opacity = 255;
            this.m_MoveUpSpeed = 5.0f;
            this.m_BounceIn = pBounceIn;
            this.m_Scale = 1f;
        }
        #endregion

        public override void Initialize()
        {
            
            base.Initialize();
        }

        private bool m_BounceBack = false;
        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Make the text kind of bounce in when spawned into the game world.
            if (m_BounceIn)
            {
                // Determine whether or not we have breached the threshold.
                if (!m_BounceBack)
                {
                    // Increase the scale of the text until we meet a threshold
                    if (this.m_Scale < BOUNCE_IN_SCALE)
                        this.m_Scale += 0.25f;
                    else
                        m_BounceBack = true;
                }
                else
                {
                    if (this.m_Scale > 1f)
                        this.m_Scale -= 0.25f;
                }
            }

            if (m_MoveUp)
            {
                this.m_Position.Y -= m_MoveUpSpeed;
            }

            if (m_Opacity > 0)
                m_Opacity -= 3;
            else
                this.Dead = true;

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Make sure that the item is not dead before it is rendered.
            if (!Dead)
            {
                if (m_Shadow)
                {
                    pSpriteBatch.DrawString(MainGame.Instance.Fonts["debug_font"], m_TextToDisplay, new Vector2(m_Position.X, m_Position.Y + 3f), Color.Black * ((float)m_Opacity / (float)byte.MaxValue), 0f, Vector2.Zero, m_Scale, SpriteEffects.None, 0f);
                }

                // Display the string with the applied opacity.
                pSpriteBatch.DrawString(MainGame.Instance.Fonts["debug_font"], m_TextToDisplay, m_Position, Color.White * ((float)m_Opacity / (float)byte.MaxValue),0f,Vector2.Zero,m_Scale,SpriteEffects.None,0f);
            }

            base.Draw(pSpriteBatch);
        }


    }
}
