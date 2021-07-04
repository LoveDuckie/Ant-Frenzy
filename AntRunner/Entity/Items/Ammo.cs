using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;
using AntRunner.Utility;

namespace AntRunner.Entity.Items
{
    public class Ammo : Item
    {
        // A list of frame indexes that we can use for randomising the appearance of the drops
        public readonly Point[] AMMO_SPRITES = { new Point(5, 7), new Point(6, 7), new Point(7, 7) };

        public Ammo(int pAmount, Vector2 pDirection,bool pIsTakeable,Vector2 pPosition) 
            : base(pPosition,1f,0f,pDirection,pIsTakeable,pAmount)
        {
            
            this.m_FrameIndex = AMMO_SPRITES[m_Random.Next(0,AMMO_SPRITES.Length)];
            m_Amount = 15;
            // Set the event that is going to deal with ammo replenishment.
            this.OnTake = TakeAmmo;
        }
        
        public Ammo() : base()
        {

        }

        /// <summary>
        /// The this event should be called when the item has collided with the player
        /// </summary>
        /// <param name="sender">The object that this is being called from</param>
        /// <param name="e">Additional arguments for additional configuration</param>
        /// <param name="pPlayer">The player that we are going to replenish</param>
        public void TakeAmmo(object sender, EventArgs e, Player pPlayer)
        {
            if (pPlayer != null)
            {
                // Set the item to dead as it has now been collected and is no longer useful to us.
                if (!Dead)
                    Dead = true;

                pPlayer.AddAmmo(m_Amount);
            }

            // Add some floating text to let the user know that we just collected some ammo
            NotificationText.Entities.Add(new NotificationText(true,"COLLECTED AMMO!",Position,true,Color.White,true));
        }

        /// <summary>
        /// Some what of a redundant function right now
        /// </summary>
        /// <param name="pPlayer">The player object that we are going to be dealing with</param>
        public override void Reward(Player pPlayer)
        {
            base.Reward(pPlayer);
        }

        public override void Initialize()
        {
            
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            //// Draw the sprites to the screen
            //pSpriteBatch.Draw(m_SpriteSheet, 
            //                  Position, 
            //                  new Rectangle(m_FrameIndex.X * m_FrameSize.X,
            //                                m_FrameIndex.Y * m_FrameSize.Y,
            //                                Size.X,
            //                                Size.Y), 
            //                                Color.White, 
            //                                Rotation, 
            //                                Origin, 
            //                                Scale, 
            //                                SpriteEffects.None, 
            //                                0f);

            base.Draw(pSpriteBatch);
        }
    }
}
