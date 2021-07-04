using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.States;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Menu
{
    #region Extra Classes
    public class MenuItem
    {
        // The members that are to be stored as a way of displaying the menu manager
        #region Members
        private string m_Message;
        private EventHandler m_Action;
        private bool m_Enabled = false;
        #endregion

        #region Properties
        public bool Enabled
        {
            get { return m_Enabled; }
            set { m_Enabled = value; }
        }

        public string Message
        {
            get { return m_Message; }
            set { m_Message = value; }
        }

        public EventHandler Action
        {
            get { return m_Action; }
            set { m_Action = value; }
        }
        #endregion

        public MenuItem()
        {

        }

        public MenuItem(string pMessage)
        {
            this.Message = pMessage;
        }
    }

    public class SettingsItem
    {
        #region Members
        public string m_SettingsMessage { get; set; }
        public string m_SettingsStatusMessage { get; set; }
        public EventHandler m_Action { get; set; }
        public object m_SettingsValue { get; set; }
        #endregion
    }
    #endregion

    public class MenuManager : IEntity
    {
        #region Properties
        public bool DisplayActiveChild
        {
            get { return m_DisplayChild; }
            set { m_DisplayChild = value; }
        }
        
        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public List<MenuManager> Children
        {
            get { return m_Children; }
            set { m_Children = value; }
        }

        public bool Active
        {
            get { return m_Active; }
            set { m_Active = value; }
        }

        public List<MenuItem> Items
        {
            get { return m_Items; }
            set { m_Items = value; }
        }

        public MenuManager Parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }
        #endregion

        #region Members
        protected bool m_Active;
        protected List<MenuItem> m_Items = new List<MenuItem>();

        protected int m_SelectionIndex;
        protected string m_MenuMessage;
        protected Texture2D m_ArrowSelect;

        protected byte m_Alpha = 0;
        protected Color m_SelectionColor;
        protected Color m_ActiveColor;

        protected MenuManager m_Parent;

        protected Texture2D m_BackgroundTexture;
        protected Texture2D m_HeadingBackgroundTexture;

        private bool m_DisplayChild = false;
        protected int m_ActiveChild = 0;
        protected List<MenuManager> m_Children = new List<MenuManager>();

        private Vector2 m_Position;
        #endregion

        #region Constructors
        public MenuManager()
        {
            m_MenuMessage = "Default Menu Message";
            m_Active = true;

            Initialize();
        }

        public MenuManager(string pMenuMessage, Vector2 pPosition)
        {
            m_Position = pPosition;
            m_MenuMessage = pMenuMessage;
            m_Active = true;

            Initialize();
        }

        public MenuManager(Texture2D pArrowSelect, string pMenuMessage)
        {
            m_MenuMessage = pMenuMessage;
            m_Active = true;

            Initialize();
            
        }

        public MenuManager(string pMenuTitle)
        {
            m_MenuMessage = pMenuTitle;
            m_Active = true;

            Initialize();
        }
        #endregion

        #region Methods
        // Add a new item to the list.
        public virtual void AddMenuItem(MenuItem pMenuItem)
        {
            // Ensure that the list is legit!
            if (m_Items != null)
            {
                m_Items.Add(pMenuItem);
            }
        }

        public virtual void SelectNext()
        {
            // Play the selection sound
            //MainGame.Instance.Sounds["menu_select"].Play();

            // Up the index
            if ((m_SelectionIndex + 1) < m_Items.Count)
            {
                m_SelectionIndex++;
            }
            else
            {
                m_SelectionIndex = 0;
            }
        }

        // Choose the previous item in the list
        public virtual void SelectPrevious()
        {
            //MainGame.Instance.Sounds["menu_select"].Play();

            // Determine that the previous select is still above.
            if ((m_SelectionIndex - 1) >= 0)
            {
                m_SelectionIndex--;
            }
            else
            {
                m_SelectionIndex = m_Items.Count - 1;
            }
        }
        
        // Perform the action that is assigned to this item.
        public virtual void SelectItem()
        {
            m_Items[m_SelectionIndex].Action.Invoke(this, null);
        }

        /// <summary>
        /// This is called as soon as the object is made for the first time
        /// </summary>
        public virtual void Initialize()
        {
            this.m_SelectionColor = Color.White;
            this.m_ActiveColor = Color.White;
            m_ArrowSelect = MainGame.Instance.Textures["arrow_select"]; // Grab the item that we are after

            // Generate a texture that is to be used for the main menu
            m_BackgroundTexture = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice, 250, 300, new Color(0f,0f,0f,0.5f));
            m_HeadingBackgroundTexture = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice, 250, 50, new Color(0, 0, 0, 50));

            this.m_Items = new List<MenuItem>();
        }

        // Revert back to the parent if we're no longer needed
        // in the child menu manager.
        public virtual void DisableChild()
        {
            m_Children[m_ActiveChild].Active = false;
            m_ActiveChild = 0;
            m_DisplayChild = false;
        }

        public virtual void DisplayChild(int pIndex)
        {
            // Make sure that the desired index is within the boundaries of the list itself.
            if (pIndex < m_Children.Count &&
                pIndex >= 0)
            {
                m_DisplayChild = true;
                m_ActiveChild = pIndex;
                m_Children[m_ActiveChild].Active = true;
            }
        }

        protected virtual void Reset()
        {
            // Do nothing for now
        }

        /// <summary>
        /// Called when the input for the menu has to be dealt with
        /// </summary>
        /// <param name="pGameTime">Delta time objects required</param>
        /// <param name="pInputHandler">The input handler object.</param>
        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            if (!m_DisplayChild)
            {
                // Only do actions with the menu if there is appropriate
                // input.
                if (m_Active)
                {
                    // Determine if there is any input for it to go down
                    if (pInputHandler.KeyButtonDownOnce(PlayerIndex.One, Buttons.DPadDown) ||
                        pInputHandler.GetInput(PlayerIndex.One).ThumbSticks.Left.Y > 0.15f ||
                        pInputHandler.KeyboardButtonPressed(Keys.Down))
                    {
                        SelectNext();
                    }

                    // Receive input from one of the methods
                    if (pInputHandler.KeyButtonDownOnce(PlayerIndex.One, Buttons.DPadUp) ||
                        pInputHandler.KeyboardButtonPressed(Keys.Up) ||
                        pInputHandler.GetInput(PlayerIndex.One).ThumbSticks.Left.Y < -0.15f)
                    {
                        SelectPrevious();
                    }

                    // Do something if the enter key has been pressed.
                    if (pInputHandler.KeyboardButtonPressed(Keys.Enter) ||
                        pInputHandler.KeyButtonDownOnce(PlayerIndex.One, Buttons.A))
                    {
                        SelectItem();
                    }
                }
            }
            else
            {
                m_Children[m_ActiveChild].Update(pGameTime, pInputHandler);
            }
        }

        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler, State pState, Level pLevel)
        {
            this.Update(pGameTime, pInputHandler);
        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
            if (!m_DisplayChild)
            {
                pSpriteBatch.Draw(m_BackgroundTexture, new Vector2(m_Position.X - m_ArrowSelect.Width - 14, m_Position.Y - 32), new Rectangle(0, 0, m_BackgroundTexture.Width, 32), new Color(255, 255, 255, 175));
                pSpriteBatch.DrawString(MainGame.Instance.Fonts["debug_font"], m_MenuMessage, new Vector2(m_Position.X, m_Position.Y - 28), Color.White);
                pSpriteBatch.Draw(m_BackgroundTexture, new Vector2(m_Position.X - m_ArrowSelect.Width - 14, m_Position.Y), new Color(255, 255, 255, 50));

                // Render the arrow appropriately on which item has been selected.
                pSpriteBatch.Draw(m_ArrowSelect, new Vector2(m_Position.X - m_ArrowSelect.Width - 14, ((m_SelectionIndex * 26) + m_Position.Y)), Color.White);

                // Draw all the menu items that are to be used
                for (int i = 0; i < m_Items.Count; i++)
                {
                    // Draw the menu item text to the screen
                    pSpriteBatch.DrawString(MainGame.Instance.Fonts["debug_font"],
                                            m_Items[i].Message,
                                            new Vector2(m_Position.X, m_Position.Y + (i * 25)),
                                            Color.White);
                }

            }
            else
            {
                m_Children[m_ActiveChild].Draw(pSpriteBatch);
            }
        }
        #endregion


        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
    }
}
