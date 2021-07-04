using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using AntRunner.Entity;
using AntRunner.Tower;

namespace AntRunner.Utility
{
    public class RigidBody : IEntity
    {
        #region Members
        private AABB m_BoundingBox;
        private Vector2 m_LinearVelocity;
        
        private float m_Rotation;
        private float m_Mass;
        
        private Vector2 m_CenterMass;
        private Vector2 m_Position;
        private float m_LinearDamping = 0.0f;
        private float m_AngularDamping = 0.0f;
        private float m_Inertia = 0f;
        private float m_InvInertia = 0f;

        private Entity.Entity m_Focus = null;

        private float m_Torque = 0f;
        private float m_AngularVelocity = 0f;

        private float m_InvMass = 0f;
        #endregion

        #region Properties
        // The entity that the body will be responsive for.
        public Entity.Entity Focus
        {
            get { return m_Focus; }
            set { m_Focus = value; }
        }

        public float AngularDamping
        {
            get { return m_AngularDamping; }
            set { m_AngularDamping = value; }
        }

        public float LinearDamping
        {
            get { return m_LinearDamping; }
            set { m_LinearDamping = value; }
        }

        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public Vector2 CenterMass
        {
            get { return m_CenterMass; }
            set { m_CenterMass = value; }
        }

        public float Mass
        {
            get { return m_Mass; }
            set { m_Mass = value; }
        }

        public float Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }
        #endregion

        public RigidBody(Entity.Entity pOther)
        {
            // Define the center of the mass
            this.m_CenterMass = new Vector2(pOther.Position.X + (pOther.Size.X / 2), 
                                            pOther.Position.Y + (pOther.Size.Y / 2));

            this.m_Mass = 5f;
            this.m_Position = pOther.Position;
            this.m_LinearVelocity = new Vector2(0, 2);
            this.m_BoundingBox = new AABB(this.Position, new Vector2(this.Position.X + pOther.Size.X,
                                                                    this.Position.Y + pOther.Size.Y), pOther);

            // What this rigid body is binding to.
            this.m_Focus = pOther;
        }

        public void AddForce(Vector2 pForce)
        {
            // Make sure that there is some kind of value in there.
            if (pForce != Vector2.Zero)
            {
                this.m_LinearVelocity += pForce;
            }
        }

        public void ApplyImpulse()
        {

        }

        /// <summary>
        /// Determine a collision between this rigid body and another
        /// </summary>
        /// <param name="pOther">The other rigid body that we're checking for collision against</param>
        /// <returns>Return the int value</returns>
        public int CollidesWith(RigidBody pOther)
        {
            List<Vector2> _axis = new List<Vector2>();

            return 0;
        }

        public void Initialize()
        {
 	        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGameTime"></param>
        /// <param name="pInputHandler"></param>
        /// <param name="pLevel">Use level for the likes of calculating </param>
        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            this.Position += (float)pGameTime.ElapsedGameTime.TotalMilliseconds * this.m_LinearVelocity;
            this.Rotation += (float)pGameTime.ElapsedGameTime.TotalMilliseconds * this.m_AngularVelocity;

            // Loop through the items and determine if there is a collision
            foreach (var item in Entity.Entity.Entities)
            {
                
            }

        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
 	        
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Only draw the output if we're in debug mode.
            if (Global.DEBUG)
            {
                
            } 	    
        }
    }
}
