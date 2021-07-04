using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AntRunner.Entity
{
    public class Character : Entity
    {
        protected Point m_FrameIndex;
        protected Point m_FrameSize;
        protected int m_FrameSpeed;
        protected float m_FrameCounter;
        private bool m_IsDead;

        protected float m_MovementSpeed;

        private int m_Health;
        private int m_MaxHealth;

        #region Properties
        public int MaxHealth
        {
            get { return m_MaxHealth; }
            set { m_MaxHealth = value; }
        }

        public float MovementSpeed
        {
            get { return m_MovementSpeed; }
            set { m_MovementSpeed = value; }
        }

        protected bool IsDead
        {
            get { return m_IsDead; }
            set { m_IsDead = value; }
        }

        public int Health
        {
            get { return m_Health; }
            set { m_Health = value; }
        }
        #endregion

        #region Constructors
        public Character()
        {
            m_Health = 100;
        }

        public Character(Point pFrameSize, int pFrameSpeed)
        {
            m_FrameSize = pFrameSize;
            m_FrameSpeed = pFrameSpeed;
            m_FrameCounter = 0;
            m_FrameIndex = new Point(0, 0);
        }
        #endregion

        public override void Initialize()
        {
            this.m_BoundingBox = new Rectangle((int)m_Position.X, (int)m_Position.Y, m_FrameSize.X, m_FrameSize.Y);

           // base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            m_BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, m_FrameSize.X, m_FrameSize.Y);

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Output some wireframe rendering of the bounding box so that we can show 
            // Where the collisions should be taking place.
#if DEBUG
            VertexPositionColor[] _vertices = new VertexPositionColor[5];
            _vertices[0].Position = new Vector3(Position.X, Position.Y, 1f);
            _vertices[0].Color = Color.White;
            _vertices[1].Position = new Vector3((Position.X + m_FrameSize.Y), Position.Y, 1f);
            _vertices[1].Color = Color.White;
            _vertices[2].Position = new Vector3(0f, 0f, 1f);
            _vertices[2].Color = Color.White;
            _vertices[3].Position = Vector3.Zero;
            _vertices[3].Color = Color.White;

            // Set up the new indices -- for drawing the wireframe
            int[] _indices = new int[6];
            _indices[0] = 3;
            _indices[1] = 0;
#endif

            base.Draw(pSpriteBatch);
        }
    }
}
