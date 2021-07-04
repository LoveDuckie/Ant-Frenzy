using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;


namespace AntRunner.Utility
{
    public enum FontSize
    {
        Small,
        Medium,
        Large,
        ExtraLarge
    }
    // Utility class for rendering some text to the screen
    public class ShadowText
    {
        // Display the text to the screen
        public static void Draw(string pText, SpriteBatch pSpriteBatch, Vector2 pPosition, FontSize pFontSize = FontSize.Small)
        {
            Draw(pText, pSpriteBatch, pPosition, Color.White, FontSize.Small);
        }
        // Display the text to the screen
        public static void Draw(string pText, SpriteBatch pSpriteBatch, Vector2 pPosition, Color pFontColor, FontSize pFontSize = FontSize.Small)
        {
            SpriteFont _font = null;

            // Determine that the sprite batch object.
            if (pSpriteBatch != null)
            {
                switch (pFontSize)
                {
                    case FontSize.Small:
                        _font = MainGame.Instance.Fonts["debug_font"];
                    break;

                    case FontSize.Large:
                        _font = MainGame.Instance.Fonts["debug_font"];
                    break;

                    case FontSize.Medium:
                        _font = MainGame.Instance.Fonts["debug_font"];
                    break;

                    case FontSize.ExtraLarge:
                        _font = MainGame.Instance.Fonts["debug_font"];
                    break;
                }

                // Render to the screen once we have the font that we are after.
                pSpriteBatch.DrawString(_font,
                                        pText,
                                        new Vector2(pPosition.X,pPosition.Y + 2f),
                                        Color.Black);

                pSpriteBatch.DrawString(_font, 
                                        pText, 
                                        pPosition, 
                                        pFontColor);
            }
        }
    }
}
