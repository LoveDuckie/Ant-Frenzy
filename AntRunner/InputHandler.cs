using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AntRunner
{
    /// <summary>
    /// Main input handler class that is used.
    /// </summary>
    public class InputHandler
    {
        #region Members
        // The states that are to be used.
        private KeyboardState m_CurrentKeyboardState;
        private KeyboardState m_PreviousKeyboardState;

        private Dictionary<string, Buttons> m_Mappings;

        private MouseState m_CurrentMouseState;
        private const int MAX_PLAYERS = 4;

        private MouseState m_PreviousMouseState;

        private GamePadState[] m_PreviousGamePadStates;
        private GamePadState[] m_CurrentGamePadStates;
        #endregion

        #region Properties
        public MouseState CurrentMouseState
        {
            get { return m_CurrentMouseState; }
            set { m_CurrentMouseState = value; }
        }
        
        public MouseState PreviousMouseState
        {
            get { return m_PreviousMouseState; }
            set { m_PreviousMouseState = value; }
        }
        #endregion

        public InputHandler()
        {
            // Initiate the lists.
            m_PreviousGamePadStates = new GamePadState[MAX_PLAYERS];
            m_CurrentGamePadStates = new GamePadState[MAX_PLAYERS];

            // Used for mapping the controller buttons to a string literal input.
            m_Mappings = new Dictionary<string, Buttons>();

        }

        // Return the state of the game pad if it's more up to date.
        public GamePadState GetInput(PlayerIndex pPlayerIndex)
        {
            return GamePad.GetState(pPlayerIndex);
        }

        public Vector2 GetMouseToWorld(Matrix pWorld)
        {
            Matrix _inverse = Matrix.Invert(pWorld);
            Vector2 _mouseCoords = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            _mouseCoords = Vector2.Transform(_mouseCoords, _inverse);
            
            return _mouseCoords;
        }

        public bool IsKeyDownOnce(Keys pKey)
        {
            return (m_PreviousKeyboardState.IsKeyUp(pKey) &&
                    m_CurrentKeyboardState.IsKeyDown(pKey));
        }

        /// <summary>
        /// Has the left mouse button been pushed down once?
        /// </summary>
        /// <returns>Return if the left button was pressed down once.</returns>
        public bool IsLeftMouseButtonDownOnce()
        {
            return (m_PreviousMouseState.LeftButton == ButtonState.Released &&
                m_CurrentMouseState.LeftButton == ButtonState.Pressed);
        }

        /// <summary>
        /// Has the right mouse button been pushed down once?
        /// </summary>
        /// <returns></returns>
        public bool IsRightMouseButtonDownOnce()
        {
            return (m_PreviousMouseState.RightButton == ButtonState.Released &&
                m_CurrentMouseState.RightButton == ButtonState.Pressed);
        }
        
        public bool IsButtonDown(Buttons pButton, PlayerIndex pPlayer)
        {
            
            return GamePad.GetState(pPlayer).IsButtonDown(pButton);
        }

        public Vector2 GetMouse()
        {
            return new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        

        // Update the current states of the input
        public void UpdateCurrentState()
        {
            // Go through the list and update them accordingly.
            for (int i = 0; i < m_CurrentGamePadStates.Length; i++)
            {
                m_CurrentGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }

            m_CurrentKeyboardState = Keyboard.GetState();
            m_CurrentMouseState = Mouse.GetState();
        }

        public bool KeyboardButtonPressed(Keys pKeys)
        {
            // Determine whether or not the correct keys are down
            return (m_PreviousKeyboardState.IsKeyUp(pKeys) &&
                   m_CurrentKeyboardState.IsKeyDown(pKeys));
        }

        /// <summary>
        /// For determining a single button press.
        /// </summary>
        /// <param name="pIndex">The player index that we want to receive input from</param>
        /// <param name="pButtons">Button that we expect to be pressed down</param>
        /// <returns></returns>
        public bool KeyButtonDownOnce(PlayerIndex pIndex, Buttons pButtons)
        {
            // Determine a single button press.
            return (m_PreviousGamePadStates[(int)pIndex].IsButtonUp(pButtons) &&
                 m_CurrentGamePadStates[(int)pIndex].IsButtonDown(pButtons));
        }

        /// <summary>
        /// Return the vector2 value of the mouse within world space.
        /// </summary>
        /// <param name="pScreenPosition">Where on the screen is it being pointed at the minute?</param>
        /// <param name="pCamera">The camera that is transforming the appearance of the world.</param>
        /// <returns>Returns the transformed vertex.</returns>
        public static Vector2 TransformMouse(Vector2 pScreenPosition, Cameras.Camera pCamera)
        {
            Vector2 currentPosition = pScreenPosition;
            // For some reason I have to invert the view matrix before transforming
            // the position of the mouse
            Matrix inverseMatrix = Matrix.Invert(pCamera.GetMatrix());
            Vector2 mousePosition = Vector2.Transform(pScreenPosition, inverseMatrix);

            return mousePosition;
        }

        // Update the previous states of the inputs
        public void UpdatePreviousState()
        {
            // Update the game pad states
            for (int i = 0; i < m_PreviousGamePadStates.Length; i++)
            {
                m_PreviousGamePadStates[i] = GamePad.GetState((PlayerIndex)i);
            }

            m_PreviousKeyboardState = Keyboard.GetState();
            m_PreviousMouseState = Mouse.GetState();
        }

        public bool IsKeyDown(Keys pKey)
        {
            return Keyboard.GetState().IsKeyDown(pKey);
        }

        public void Update(GameTime pGameTime)
        {

        }

    }
}
