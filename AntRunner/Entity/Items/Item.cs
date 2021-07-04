using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Entity.Items
{
    public class Item : Entity
    {
        #region Events
        // The item handler that is used for interaction with the item
        public delegate void ItemEventHandler(object sender, EventArgs e, Player pPlayer);
        protected ItemEventHandler OnTake = delegate(object sender, EventArgs e, Player pPlayer) { };
        #endregion

        #region Members
        protected bool m_Disappears = false;
        protected Point m_FrameSize;
        protected Point m_FrameIndex;
        protected int m_Amount; // The amount that is going to replenish the player with
        private bool m_IsTakeable; // Are we allowed to take it ?
        protected bool m_PhysicsSlide; // Will it slide to a halt a long the ground?
        protected float m_AccelerationDirectionDelta = 0f;
        protected float m_AccelerationDirection = 0f;
        protected Vector2 m_Direction; // Direction vector -- NOT NORMALISED.
        protected SoundEffect m_CollectSound;

        protected float m_Power; // This will amplify the speed at which it moves in a certain direction
        #endregion

        #region Mutators
        public bool IsTakeable
        {
            get { return m_IsTakeable; }
            set { m_IsTakeable = value; }
        }

        public Point FrameSize
        {
            get { return m_FrameSize; }
            set { m_FrameSize = value; }
        }

        public Point FrameIndex
        {
            get { return m_FrameIndex; }
            set { m_FrameIndex = value; }
        }

        public int Amount
        {
            get { return m_Amount; }
            set { m_Amount = value; }
        }
        #endregion

        #region Constructors
        public Item(
            Vector2 pPosition, 
            float pScale, 
            float pRotation,
            Vector2 pDirection,
            bool pIsTakeable,
            int pAmount) : base(
            pScale,
            pPosition,
            MainGame.Instance.Textures["collect_items"],
            pRotation)
        {
            this.FrameSize = new Point(34, 34);
            this.Origin = new Vector2(17, 17);
            this.Size = FrameSize;
            this.m_IsTakeable = pIsTakeable;
            this.Amount = pAmount;

            // Set the direction vector
            m_Direction = pDirection;

            m_CollectSound = MainGame.Instance.Sounds["pickup2"];
            
            // Set up the item so that when it enters the world, it does something
            Setup(pDirection, 50, pPosition, false);
            this.Initialize();
        }

        public Item()
        {

        }

        /// <summary>
        /// The entry constructor that is used for defining the amount that is to be collected
        /// from the item
        /// </summary>
        /// <param name="pAmount">The amount that the player is going to get from collecting this</param>
        /// <param name="pPosition">The location of the item in world space.</param>
        public Item(int pAmount, Vector2 pPosition, Point pFrameIndex)
        {
            this.Scale = 1f;
            this.Rotation = 0f;
            this.m_FrameIndex = pFrameIndex;
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            this.m_FrameSize = new Point(34, 34);
        }


        /// <summary>
        /// Set up the item with various properties such as the starting location
        /// and which direction that it moves in
        /// </summary>
        /// <param name="pDirection">The direction that the item is going to be floating in</param>
        /// <param name="pItemAmount">The amount that the item is going to benefit the player with</param>
        /// <param name="pPosition">The position that the item is going to start in</param>
        /// <param name="pDisappears">Is it going to disppear over time if not picked up?</param>
        public virtual void Setup(Vector2 pDirection, 
                                  int pItemAmount,
                                  Vector2 pPosition,
                                  bool pDisappears)
        {
            // Set the position and whether or not it is takeable at the moment
            this.Position = pPosition;
            this.m_IsTakeable = true;

            this.m_Disappears = pDisappears;

            // The amount that the item is worth
            this.m_Amount = 0;

            // Set a random force of power that is going to influence the speed that it goes in
            this.m_Power = m_Random.Next(5,10);

            // Set the direction and speed that it will move in
            this.m_Velocity = pDirection * m_Power;
        }

        /// <summary>
        /// The typical update loop with wonderous things.
        /// </summary>
        /// <param name="pGameTime">Delta time value that is generated from the game framework</param>
        /// <param name="pInputHandler">The object that deals with the input from the compooper.</param>
        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            Vector2 _currentPosition = Position;

            base.Update(pGameTime, pInputHandler);
        }

        /// <summary>
        /// A version of the update method that takes in the level as an argument
        /// </summary>
        /// <param name="pGameTime">Delta object</param>
        /// <param name="pInputHandler">The input handler that deals with inputs from the computer</param>
        /// <param name="pLevel">The level that the item will interact with</param>
        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            // Use this to temporarily determine collisions in the environment
            Vector2 _currentPosition = Position;

            this.WorldOrigin = Position + Origin;

            // Increase the position by using the velocity.
            // this.Position += m_Velocity;

            // Reduce the speed at which it is going
            this.m_Velocity *= 0.97f;

            _currentPosition += m_Velocity;

            #region Collision Box Updating
            // Update the collision boxes that we are going to be using.
            this.CollisionBox = new BoundingBox(new Vector3(Position, 0), 
                                                new Vector3(Position.X + Size.X, 
                                                            Position.Y + Size.Y, 0));
            this.BoundingBox = new Rectangle((int)Position.X, 
                                             (int)Position.Y, 
                                             Size.X, 
                                             Size.Y);
            #endregion

            //// See if there are some adjacent players that can collect this
            //if (m_IsTakeable)
            //{
            //    double _fixdistance = 100;
            //    int _absorbdistance = 16;

            //    // Loop through the players and find one that is of relative distnace
            //    foreach (var item in MainGame.Instance.GameState.PlayerManager.Players)
            //    {
            //        // Grab the direction delta for determining distance
            //        Vector2 _directionDelta = item.Position - this.Position;
                    
            //        // Get the distance value in question
            //        double _distance = Math.Sqrt(_directionDelta.X * _directionDelta.X + 
            //                                     _directionDelta.Y * _directionDelta.Y);

            //        float _suckpower = Player.SUCK_DISTANCE;
            //        float _suckdistance = 0f;
            //        _suckdistance = (float) (_fixdistance - 40) * _suckpower + 40;

            //        // Within the range to modify the velocity values.
            //        if (_distance < _suckdistance)
            //        {
            //            _directionDelta /= (float)_distance;
            //        }
            //    }
            //}

            // Perform any kind of velocity stuff here.

            #region Tiled map collision
            // If there is a collision on the X axis, then do something about it 
            if (!pLevel.CheckCollision(_currentPosition, new Rectangle((int)_currentPosition.X,
                                                                       (int)Position.Y, Size.X, Size.Y)))
            {
                this.Position = new Vector2(_currentPosition.X, Position.Y);
            }
            else
            {
                this.Velocity = new Vector2(-Velocity.X, Velocity.Y);
            }

            // Check a collision on the Y axis of the new position that we are dealing with.
            if (!pLevel.CheckCollision(_currentPosition, new Rectangle((int)Position.X,
                                                                       (int)_currentPosition.Y,
                                                                       Size.X,
                                                                       Size.Y)))
            {
                this.Position = new Vector2(Position.X, _currentPosition.Y);
            }
            else
            {
                // Change the direction that it's going in now if there was some kind of collision
                this.Velocity = new Vector2(Velocity.X, -Velocity.Y);
            }
            #endregion

            #region Entity Handling

            // Loop through the players and determine if there is someone that
            // can pick up the item in question
            foreach (var item in MainGame.Instance.GameState.PlayerManager.Players)
            {
                // Determine that the player has interacted with the item in some way
                if (item.CollisionBox.Contains(this.CollisionBox) != ContainmentType.Disjoint && !Dead)
                {
                    OnTake(this, null,item);

                    // Determine whether or not the content has been loaded appropriately.
                    if (m_CollectSound != null)
                    {
                        m_CollectSound.Play();
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Do something when rewarding the player in the end
        /// </summary>
        /// <param name="pPlayer">The player that we are focusing on</param>
        public virtual void Reward(Player pPlayer)
        {

        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Render the item to the screen at some given point
            pSpriteBatch.Draw(m_SpriteSheet, Position + Origin, new Rectangle(m_FrameIndex.X * m_FrameSize.X,
                                                                   m_FrameIndex.Y * m_FrameSize.Y,
                                                                   m_FrameSize.X,
                                                                   m_FrameSize.Y), 
                                                                   Color.White,
                                                                   0f,
                                                                   new Vector2(17,17),
                                                                   1.5f,SpriteEffects.None,0f);
            base.Draw(pSpriteBatch);
        }
        #endregion
    }
}
