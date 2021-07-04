using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Utility;
using AntRunner.States;
using AntRunner.Cameras;

using AntRunner.Entity;

namespace AntRunner.Entity.Items
{
    /// <summary>
    /// A typical item class that is used for providing health to the player in the game
    /// </summary>
    public class HealthPotion : Item
    {
        // Sprites to be randomly selected when spawned.
        private readonly Point[] POTION_SPRITES = { new Point(0, 2), 
                                                    new Point(7, 2), 
                                                    new Point(7, 3) };

        #region Constructors
        public HealthPotion(
            int pAmount, 
            Vector2 pPosition, 
            Vector2 pDirection,
            bool pIsTakeable)
            : base(pPosition,1f,0f,pDirection,pIsTakeable,pAmount)
        {
            this.m_FrameIndex = POTION_SPRITES[m_Random.Next(0,POTION_SPRITES.Length)];

            this.OnTake = TakeHealth;
        }
        #endregion

        #region Methods
        public void TakeHealth(object sender, EventArgs e, Player pPlayer)
        {
            if (pPlayer != null)
            {
                pPlayer.AddHealth(m_Amount);
            }

            this.Dead = true;
            // Let the user know that a health potion was collected!
            NotificationText.Entities.Add(new NotificationText(true,"COLLECTED HEALTH POTION!",Position,true,Color.White,true));
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Reward(Player pPlayer)
        {
            pPlayer.Health += 100;
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Draw(m_SpriteSheet, Position, new Rectangle(m_FrameIndex.X * m_FrameSize.X,
                                                                     m_FrameIndex.Y * m_FrameSize.Y,
                                                                     Size.X,
                                                                     Size.Y), Color.White);

            base.Draw(pSpriteBatch);
        }
        #endregion
    }
}
