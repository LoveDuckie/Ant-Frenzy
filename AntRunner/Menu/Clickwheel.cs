using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.States;
using AntRunner.Utility;


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;


namespace AntRunner.Menu
{
    public class ClickwheelItem : MenuItem
    {
        private Texture2D m_ClickImage = null;
        private EventHandler m_LeftClick = null;

        #region Properties
        public Texture2D ClickImage
        {
            get { return m_ClickImage; }
        }

        public EventHandler RightClick
        {
            get { return m_LeftClick; }
            set { m_LeftClick = value; }
        }
        #endregion

        #region Constructors
        public ClickwheelItem() : base()
        {

        }

        public ClickwheelItem(string pMessage)
            : base(pMessage)
        {

        }

        public ClickwheelItem(string pMessage, Texture2D pImage) : base(pMessage)
        {
            this.m_ClickImage = pImage;
        }
        #endregion
    }

    /// <summary>
    /// The click wheel effect that we want for ants to use.
    /// </summary>
    public class Clickwheel : MenuManager
    {
        private ClickwheelItem m_MouseHovering = null;
        private ClickwheelItem m_LastHovering = null;
        private Level m_Level = null;

        private Vector2 m_RenderingMoveout = Vector2.Zero;

        public Vector2 RenderingMoveout
        {
            get { return m_RenderingMoveout; }
            set { m_RenderingMoveout = value; }
        }

        public Vector2 m_TransformedMouse = Vector2.Zero;

        public const int MOVEOUT_MAX = 128;
        public const float MOVEOUT_SPEED = 8.5f;

        #region Constructors
        /// <summary>
        /// Default construct if we don't have anything to hand over to it.
        /// </summary>
        public Clickwheel()
        {
            this.Position = Vector2.Zero;
            m_Level = MainGame.Instance.GameState.Level;
            this.Initialize();
        }

        /// <summary>
        /// Main constructor for the level
        /// </summary>
        /// <param name="pPosition">The position that we want the clickwheel to appear at.</param>
        /// <param name="pLevel">The reference to the level that we are interacting with</param>
        public Clickwheel(Vector2 pPosition, Level pLevel)
        {
            this.Position = pPosition;
            this.Initialize();
            m_Level = pLevel;
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            this.Active = false;
            this.Items = new List<MenuItem>();
        }

        public bool IsActive()
        {
            bool _isActive = false;

            if (Active)
            {
                _isActive = true;
                return _isActive;
            }

            // Loop through the children in the list
            // Recursively, if one of the children is considered active
            // then continue on as normal.
            foreach (var child in Children)
            {
                if (((Clickwheel)child).IsActive())
                {
                    _isActive = true;
                    break;
                }
            }

            return _isActive;
        }

        /// <summary>
        /// The main update loop for the clickwheel that is to be used.
        /// </summary>
        /// <param name="pGameTime">The delta time object that is used</param>
        /// <param name="pInputHandler">The input object.</param>
        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            Matrix inverseMatrix = Matrix.Invert(MainGame.Instance.GameState.CameraManager.ActiveCamera().GetMatrix());
            Vector2 transformedMousePosition = Vector2.Transform(new Vector2(Mouse.GetState().X,Mouse.GetState().Y), inverseMatrix);
            TMXLevel _level = MainGame.Instance.GameState.Level.TMXLevel;

            // Return the world coordinates of the mouse from screen space.
            int _mouseX = (int)transformedMousePosition.X / _level.TileWidth;
            int _mouseY = (int)transformedMousePosition.Y / _level.TileHeight;

            #region Mouse Input Handling
            // Determine whether or not the right button has been pressed.
            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                // This method of implementation is a little bit hackish.
                if (!Active)
                {
                    this.Position = pInputHandler.GetMouse();
                }

                if (!this.DisplayActiveChild)
                {
                    this.Active = true;
                }
            }
            else if (Mouse.GetState().RightButton == ButtonState.Released)
            {

                DisableChild();
                this.DisplayActiveChild = false;
                
                this.Active = false;
                m_RenderingMoveout = Vector2.Zero;
                m_MouseHovering = null;

            }
            
            if (pInputHandler.IsLeftMouseButtonDownOnce())
            {
                /// Determine first that our mouse is actually above something
                if (m_MouseHovering != null)
                {
                    if (m_MouseHovering.Action != null)
                    {
                        m_MouseHovering.Action(this, null);
                    }
                }
            }
            #endregion

            // Only deal with them if this menu manager is considered as active.
            if (!DisplayActiveChild)
            {
                if (Active)
                {
                    for (int i = 0; i < Items.Count; i++)
                    {
                        Rectangle _temp = new Rectangle((int)pInputHandler.GetMouse().X,
                                                        (int)pInputHandler.GetMouse().Y, 32, 32);

                        Vector2 _temporaryPosition = new Vector2(Position.X - MOVEOUT_MAX, Position.Y);

                        Vector2 _renderVector = MathUtilities.RotateVector(Position, MathHelper.ToRadians(45 * i), _temporaryPosition);

                        // Generate the other collision box that we are going to use to compare
                        Rectangle _otherTemp = new Rectangle((int)_renderVector.X, (int)_renderVector.Y, 64, 64);

                        // Determine if there is a collision between the two.
                        if (_temp.Intersects(_otherTemp))
                        {
                            m_MouseHovering = (ClickwheelItem)Items[i];

                        }
                    }
                }
            }
            else
            {
                // Return 
                if (m_ActiveChild != -1)
                {
                    // Update the child instead.
                    Children[m_ActiveChild].Update(pGameTime, pInputHandler);

                    this.m_RenderingMoveout = Vector2.Zero;
                }
            }

            //base.Update(pGameTime, pInputHandler);
        }

        /// <summary>
        /// New Update function that offers some more functionality to us
        /// </summary>
        /// <param name="pGameTime">The game deltatime object</param>
        /// <param name="pInputHandler">The inputhandler for the task</param>
        /// <param name="pState">The state of the game</param>
        /// <param name="pLevel">The level that we are using</param>
        public override void Update(GameTime pGameTime, InputHandler pInputHandler, State pState, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pState, pLevel);
        }

        /// <summary>
        /// Display the child set of menus that are nested in the click wheel options
        /// </summary>
        /// <param name="pIndex">The item from the children that we are going for</param>
        public override void DisplayChild(int pIndex)
        {
            if (pIndex >= 0 && pIndex < m_Children.Count)
            {
                m_RenderingMoveout = Vector2.Zero;
                DisplayActiveChild = true;

                Active = false;
                m_ActiveChild = pIndex;
                m_Children[m_ActiveChild].Active = true;
                m_Children[m_ActiveChild].Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
        }

        /// <summary>
        /// Recursively go through the active children of the child and disable them
        /// </summary>
        public override void DisableChild()
        {
            if (m_ActiveChild >= 0 && m_ActiveChild < m_Children.Count)
            {
                m_Children[m_ActiveChild].DisableChild();
                m_Children[m_ActiveChild].Active = false;
                ((Clickwheel)m_Children[m_ActiveChild]).RenderingMoveout = Vector2.Zero;
            }
        }

        protected override void Reset()
        {
            
            base.Reset();
        }

        /// <summary>
        /// Output the clickwheel that we are after.
        /// </summary>
        /// <param name="pSpriteBatch">Spritebatch object responsible for rendering</param>
        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Display the menu if it is OK.
            
            if (Active)
            {
                if (m_RenderingMoveout.X < MOVEOUT_MAX)
                    m_RenderingMoveout.X += MOVEOUT_SPEED;

                // Generate the temporary position that we are going to use
                Vector2 _temporaryPosition = new Vector2(Position.X - m_RenderingMoveout.X, Position.Y);
                // Loop through the items and display them appropriately.
                for (int i = 0; i < Items.Count; i++)
                {
                    Vector2 _renderVector = MathUtilities.RotateVector(Position, MathHelper.ToRadians(45 * i), _temporaryPosition);

                    
                    float _enabledOpacity = Items[i].Enabled == false ? 0.5f : 1.0f;
                    Color _enabledColor = Color.White * _enabledOpacity;

                    // Render the click wheel to the screen.
                    pSpriteBatch.Draw(((ClickwheelItem)Items[i]).ClickImage, 
                                      _renderVector, 
                                      null, 
                                      Color.White * _enabledOpacity,
                                      0f, 
                                      Vector2.Zero, 
                                      m_MouseHovering == Items[i] ? 1.25f : 1.0f, 
                                      SpriteEffects.None, 
                                      0f);
                }



                // Determine whether or not a mouse was found hovering on top of an item.
                if (m_MouseHovering != null)
                {
                    // Display the message on the screen if this is the case.
                    ShadowText.Draw(m_MouseHovering.Message.ToString(), pSpriteBatch, new Vector2(Mouse.GetState().X, Mouse.GetState().Y - 25));
                }

            }
            else
            {
                if (DisplayActiveChild)
                {
                    m_Children[m_ActiveChild].Draw(pSpriteBatch);
                }
                
                m_RenderingMoveout = Vector2.Zero;
            }
        }
        #endregion
    }
}
