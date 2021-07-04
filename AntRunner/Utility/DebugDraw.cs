using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Utility
{
    public class DebugDraw
    {
        /// <summary>
        /// Draw a line from one point to another by using sprite batch
        /// </summary>
        /// <param name="pFrom">The point that we are drawing from</param>
        /// <param name="pTo">The point that we are drawing to</param>
        /// <param name="pSpriteBatch">The sprite batch object that is used for rendering stuff to the screen</param>
        /// <param name="pOpenSpriteBatch">Do we bother doing the begin and end sprite batch calls that we're meant to call?</param>
        public static void DrawDebugLine(Vector2 pFrom, Vector2 pTo, SpriteBatch pSpriteBatch, Color pLineColor, float pLineThickness, bool pOpenSpritebatch)
        {
            if (pOpenSpritebatch)
            {
                pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            }

            Vector2 _direction = pFrom - pTo;

            

            // Render the line to the screen
            //pSpriteBatch.Draw(MainGame.Instance.Textures["debug_dot"], 
            //                  new Rectangle((int)pFrom.X,(int)pFrom.Y,(int)pLineThickness,
            //                      (int)Utility.MathUtilities.DistanceBetweenVectors(pFrom,pTo)),
            //                      null, 
            //                      pLineColor,
            //                      (float)Math.Atan2((double)pTo.Y - pFrom.Y, pTo.X - pFrom.X),
            //                      Vector2.Zero,
            //                      SpriteEffects.None,
            //                      0f);

            pSpriteBatch.Draw(MainGame.Instance.Textures["debug_dot"], pFrom, null, pLineColor,
                         (float)Math.Atan2(pTo.Y - pFrom.Y, pTo.X - pFrom.X),
                         new Vector2(0f, (float)MainGame.Instance.Textures["debug_dot"].Height / 2),
                         new Vector2(Vector2.Distance(pFrom, pTo), 1f),
                         SpriteEffects.None, 0f);
            

            if (pOpenSpritebatch)
            {
                pSpriteBatch.End();
            }
        }

        /// <summary>
        /// Draws the box somewhere on the screen
        /// </summary>
        /// <param name="pOpenSpriteBatch">Do we make the open call?</param>
        /// <param name="pWhere">Where on the world we want to draw</param>
        /// <param name="pSpriteBatch">The spritebatch object that is used for rendering</param>
        /// <param name="pColor">The color that we want to draw the box as.</param>
        public static void DebugDrawBox(bool pOpenSpriteBatch, BoundingBox pBoundingBox, SpriteBatch pSpriteBatch, Color pColor)
        {
            if (pOpenSpriteBatch)
            {
                pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            }

            // Generate
            Vector2 _size = new Vector2(Math.Abs(pBoundingBox.Max.X - pBoundingBox.Min.X),Math.Abs(pBoundingBox.Max.Y - pBoundingBox.Min.Y));

            // Draw the line from one point to another
            DrawDebugLine(
                new Vector2(pBoundingBox.Min.X, pBoundingBox.Min.Y),
                new Vector2(pBoundingBox.Max.X, pBoundingBox.Min.Y),
                pSpriteBatch, 
                pColor, 1f, false);

            DrawDebugLine(
                new Vector2(pBoundingBox.Max.X, pBoundingBox.Min.Y),
                new Vector2(pBoundingBox.Max.X, pBoundingBox.Max.Y),
                pSpriteBatch, pColor, 1f, false);


            DrawDebugLine(
                new Vector2(pBoundingBox.Max.X, pBoundingBox.Max.Y),
                new Vector2(pBoundingBox.Min.X, pBoundingBox.Max.Y),
                pSpriteBatch, pColor, 1f, false);

            DrawDebugLine(
                new Vector2(pBoundingBox.Min.X, pBoundingBox.Max.Y),
                new Vector2(pBoundingBox.Min.X, pBoundingBox.Min.Y),
                pSpriteBatch, pColor, 1f, false);

            //DrawDebugLine(
            //    new Vector2(pBoundingBox.Min.X),
            //    new Vector2(pBoundingBox.Max.X


            if (pOpenSpriteBatch)
            {
                pSpriteBatch.End();
            }
        }

    }
}
