using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Utility
{
    class ColourTexture
    {
        /// <summary>
        /// Creates a 1x1 pixel black texture.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice)
        {
            return Create(graphicsDevice, 1, 1, new Color());
        }
 
        /// <summary>
        /// Creates a 1x1 pixel texture of the specified color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="color">The color to set the texture to.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice, Color color)
        {
            return Create(graphicsDevice, 1, 1, color);
        }

        /// <summary>
        /// Generate a new texture from an area of another texture
        /// </summary>
        /// <param name="pFrameIndex"></param>
        /// <param name="pFrameSize"></param>
        /// <returns></returns>
        public Texture2D CreateFromTexture(Point pFrameIndex, Point pFrameSize, Texture2D pBaseTexture)
        {
            Texture2D _return = new Texture2D(MainGame.Instance.GraphicsDevice, pFrameSize.X, pFrameSize.Y,false,SurfaceFormat.Color);

            // Loop through the area and transfer the colour data onto the next sprite
            for (int x = 0; x < pFrameSize.X * pFrameIndex.X; x++)
            {
                for (int y = 0; y < pFrameSize.Y * pFrameIndex.Y; y++)
                {
                    
                }
            }

            return _return;
        }
 
        /// <summary>
        /// Creates a texture of the specified color.
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="color">The color to set the texture to.</param>
        /// <returns>The newly created texture.</returns>
        public static Texture2D Create(GraphicsDevice graphicsDevice, int width, int height, Color color)
        {
            // create the rectangle texture without colors
            Texture2D texture = new Texture2D(
                graphicsDevice,
                width,
                height,
                false,
                SurfaceFormat.Color);

            // Create a color array for the pixels
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = new Color(color.ToVector3());
            }

            // Set the color data for the texture
            texture.SetData(colors);

            return texture;
        }
    }
}
