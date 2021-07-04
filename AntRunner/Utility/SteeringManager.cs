using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using AntRunner.Entity;
using AntRunner.Menu;

namespace AntRunner.Utility
{
    // For dealing with seaking behaviours within the environment.
    public class SteeringManager : IEntity
    {
        public Vector2 m_Desired;
        public Vector2 m_Steering;
        public Entity.Entity m_Parent;

        public const float MAX_VELOCITY = 5.0f;

        // Instantiate the class
        public SteeringManager(Entity.Entity pParent)
        {
            this.m_Parent = pParent;
        }

        // Return the desired vector based on 
        public void Seek(Vector2 pTarget)
        {
            Vector2 _force;

            m_Desired = pTarget - m_Parent.Position;
            m_Desired.Normalize();

            m_Desired *= MAX_VELOCITY;

        }

        public void Truncate(ref Vector2 pValue, float pMax)
        {
            var i = 0f;
            i = pMax / pValue.Length();
            i = i < 1.0f ? 1.0f : i;

            pValue *= i;
        }

        public void Initialize()
        {
        
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
         
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
        
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        }
    }
}
