using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;
using AntRunner.Cameras;
using AntRunner.Menu;
using AntRunner.States;

namespace AntRunner.Cameras
{
    public class CameraManager : IEntity
    {
        private int m_CameraIndex;
        private List<Camera> m_Cameras;

        public List<Camera> Cameras
        {
            get { return m_Cameras; }
            set { m_Cameras = value; }
        }

        public CameraManager()
        {
            m_Cameras = new List<Camera>();
            m_CameraIndex = 0;
        }

        public void Initialize()
        {

        }

        public void RemoveCamera(Camera pCamera)
        {
            if (m_Cameras != null)
            {
                m_Cameras.Remove(pCamera);
            }
        }

        public void AddCamera(Camera pCamera)
        {
            if (m_Cameras != null)
            {
                m_Cameras.Add(pCamera);
            }
        }

        #region Camera Selection
        public void NextCamera()
        {
            if ((m_CameraIndex + 1) < m_Cameras.Count)
            {
                m_CameraIndex++;
            }
            else
            {
                m_CameraIndex = 0;
            }
        }

        public void PreviousCamera()
        {
            if ((m_CameraIndex - 1) >= 0)
            {
                m_CameraIndex = m_Cameras.Count - 1;
            }
            else
            {
                m_CameraIndex = m_Cameras.Count;
            }
        }
        #endregion

        public Camera ActiveCamera()
        {
            // Return the camera that is currently active.
            return this.m_Cameras[m_CameraIndex];
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {            // Make sure that there is some cameras available.
            if (m_Cameras.Count > 0)
            {
                for (int i = 0; i < m_Cameras.Count; i++)
                {
                    m_Cameras[i].Update(pGameTime, pInputHandler);
                }
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Make sure that there is some cameras available.
            if (m_Cameras.Count > 0)
            {
                for (int i = 0; i < m_Cameras.Count; i++)
                {
                    m_Cameras[i].Draw(pSpriteBatch);
                }
            }
        }



        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            if (Cameras.Count > 0)
            {
                foreach (var item in Cameras)
                {
                    item.Update(pGameTime, pInputHandler, pLevel);
                }
            }
        }
    }
}
