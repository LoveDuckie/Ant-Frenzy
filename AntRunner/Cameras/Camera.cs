using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Required libs for input in certain places
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.States;
using AntRunner.Cameras;

namespace AntRunner.Cameras
{
    public class Camera : IEntity
    {
        private Vector2 m_Position;
        private float m_Rotation;
        private float m_Scale;

        #region Constants
        private const float ZOOM_INCREMENT_AMOUNT = 0.05f;
        private const float MOVEMENT_AMOUNT = 5.0f;
        #endregion

        #region Properties
        public Vector2 Position
        {
            get {
                    if (m_Focus != null)
                    {
                        return m_Focus.Position;
                    }
                    else
                    {
                        return m_Position; 
                    }
                }
            set { m_Position = value; }
        }

        public float Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        public float Scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        public Entity.Entity Focus
        {
            get { return m_Focus; }
            set { m_Focus = value; }
        }
        #endregion

        private Entity.Entity m_Focus; // Should the camnera be focusing on something

        // Set the default values
        #region Constructors
        public Camera()
        {
            m_Position = Vector2.Zero;
            m_Scale = 1.0f;
            m_Rotation = 0.0f;
        }

        public Camera(Vector2 pPosition, float pScale, float pRotation)
        {
            m_Rotation = pRotation;
            m_Position = pPosition;
            m_Scale = pScale;
        }

        /// <summary>
        /// Shall we focus on a particular entity instead within the game world?
        /// </summary>
        /// <param name="pFocus">The entity that we are going to look at instead.</param>
        /// <param name="pScale">The proposed size of the image</param>
        /// <param name="pRotation">At what angle will the camera be rotated to</param>
        public Camera(Entity.Entity pFocus, float pScale, float pRotation)
        {
            this.m_Focus = pFocus;
            this.m_Scale = pScale;
            this.m_Rotation = pRotation;
        }
        #endregion

        #region Methods
        public void Initialize()
        {

        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            if (m_Focus != null)
            {
                m_Position = m_Focus.Position;
            }
            else // If we have nothing that we're specified to look at, then enable the keyboard to intervene.
            {
                if (pInputHandler.IsKeyDownOnce(Keys.PageDown))
                    this.m_Scale -= ZOOM_INCREMENT_AMOUNT;

                if (pInputHandler.IsKeyDownOnce(Keys.PageUp))
                    this.m_Scale += ZOOM_INCREMENT_AMOUNT;

                if (pInputHandler.IsKeyDown(Keys.Up))
                    this.Position += new Vector2(0, -MOVEMENT_AMOUNT);

                if (pInputHandler.IsKeyDown(Keys.Down))
                    this.Position += new Vector2(0, MOVEMENT_AMOUNT);

                if (pInputHandler.IsKeyDown(Keys.Left))
                    this.Position += new Vector2(-MOVEMENT_AMOUNT, 0);

                if (pInputHandler.IsKeyDown(Keys.Right))
                    this.Position += new Vector2(MOVEMENT_AMOUNT, 0);
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {

        }

        public Matrix GetProjection()
        {
            GraphicsDevice _graphicsDevice = MainGame.Instance.GraphicsDevice;

            return Matrix.CreateTranslation(new Vector3(_graphicsDevice.Viewport.Width * 0.5f, _graphicsDevice.Viewport.Height * 0.5f, 0));
        }

        public Matrix GetWorld()
        {
            return Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0));
        }

        // Return the matrix given the current data of the camera.
        public Matrix GetMatrix()
        {
            // Grab a local variable of the graphics device.
            GraphicsDevice _graphicsDevice = MainGame.Instance.GraphicsDevice;

            Vector2 _returnposition;

            // Determine that there is nothing to focus on first.
            if (m_Focus != null)
            {
                _returnposition = m_Focus.Position;
            }
            else
            {
                _returnposition = this.m_Position;
            }

            return Matrix.CreateTranslation(new Vector3(-_returnposition.X, -_returnposition.Y, 0)) *
                   Matrix.CreateRotationZ(m_Rotation) *
                   Matrix.CreateScale(m_Scale) *
                   Matrix.CreateTranslation(new Vector3(_graphicsDevice.Viewport.Width * 0.5f, _graphicsDevice.Viewport.Height * 0.5f, 0));
        }
        #endregion


        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
    }
}
