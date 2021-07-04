using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace AntRunner.Utility
{
    // For influence mapping within the environment.
    public struct DangerPoint
    {
        public float m_Weighting;
    }

    public class DangerMap
    {
        private DangerPoint[,] m_DangerPoints;
        private Level m_Level;
        private Texture2D m_DebugTexture;

        #region Constructors
        public DangerMap()
        {
            
        }

        /// <summary>
        /// Constructor for the danger map. Takes in to consideration the
        /// </summary>
        /// <param name="pLevelWidth">The width of the level that we are concerned about.</param>
        /// <param name="pLevelHeight">The height of the level.</param>
        public DangerMap(Level pLevel)
        {
            m_DangerPoints = new DangerPoint[pLevel.TMXLevel.Width, pLevel.TMXLevel.Width];
            m_Level = pLevel;
            m_DebugTexture = ColourTexture.Create(MainGame.Instance.GraphicsDevice, pLevel.TMXLevel.TileWidth,pLevel.TMXLevel.TileHeight,Color.White);
        }
        #endregion

        /// <summary>
        /// Insert a point of concern onto the heat map interpolate the radius
        /// </summary>
        /// <param name="pX">The x coordinate on the danger map</param>
        /// <param name="pY">The y coordinate on the danger map</param>
        /// <param name="pRadius"></param>
        public void InsertDangerPoint(int pX, int pY, int pRadius)
        {
            // Determine that the point is within the bounds of the level.
            if (pX > 0 && pY > 0 &&
                pX < m_Level.TMXLevel.Width &&
                pY < m_Level.TMXLevel.Height)
            {
                m_DangerPoints[pX, pY].m_Weighting = 1;

                // Loop through adjacent nodes and interpolate
                for (int x = (pX - 1); x < (pX + 2); x++)
                {
                    for (int y = (pY - 1); y < (pY + 2); y++)
                    {
                        // Make sure that we're not applying values on this here.
                        if (pX > 0 &&
                            pY > 0 &&
                            pX < m_Level.TMXLevel.Width &&
                            pY < m_Level.TMXLevel.Height)
                        {
                            // Increase the heat map value but make sure that it
                            m_DangerPoints[x, y].m_Weighting = MathHelper.Clamp(m_DangerPoints[x, y].m_Weighting + 0.25f,0,1);
                        }
                    }
                }
                
            }    
        }

        /// <summary>
        /// Determines that the coordinates provided are in fact within the bounds of the map
        /// </summary>
        /// <param name="pX">X coordinate</param>
        /// <param name="pY">Y coordinate</param>
        /// <returns>Returns whether the coordinates are valid</returns>
        public bool CheckValid(int pX, int pY)
        {
            return (pX >= 0 && pY >= 0
                    && pX < m_Level.TMXLevel.Width
                    && pY < m_Level.TMXLevel.Height);
        }

        /// <summary>
        /// Insert a danger point depicting the points to avoid in the map when generating paths
        /// </summary>
        /// <param name="pPosition">The point within the map</param>
        /// <param name="pRadius">The radius of effect.</param>
        public void InsertDangerPoint(Point pPosition, int pRadius)
        {
            // Insert the new values appropriately.
            InsertDangerPoint(pPosition.X, pPosition.Y, pRadius);
        }
        
        /// <summary>
        /// Grab the value at the given coordinate
        /// </summary>
        /// <param name="pX">The x coordinate on the grid that we are going to retrieve</param>
        /// <param name="pY">The y coordinate on the grid that we are going to retrieve</param>
        /// <returns></returns>
        public DangerPoint GetDangerWeighting(int pX, int pY)
        {
            // Determine that the point in question is not out of bounds
            if (pX < m_DangerPoints.GetLength(0) &&
                pY < m_DangerPoints.GetLength(1))
            {
                return m_DangerPoints[pX, pY];
            }

            return new DangerPoint() { m_Weighting = 0 };
        }

        public bool IsDangerous(int pX, int pY)
        {
            return (m_DangerPoints[pX, pY].m_Weighting == 1);
        }

        // Return whether or not the given point is in fact dangerous
        public bool IsDangerous(Point pPoint)
        {
            return IsDangerous(pPoint.X, pPoint.Y);
        }

        /// <summary>
        /// Render the output data to the screen
        /// </summary>
        /// <param name="pSpriteBatch">Used for rendering the sprites</param>
        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Create a new 2d array instance of the color map for usage.
            Color[,] _colormap = new Color[m_Level.TMXLevel.Width, m_Level.TMXLevel.Height];

            // Loop through the danger map and render it out appropriately.
            for (int i = 0; i < m_DangerPoints.GetLength(0); i++)
            {
                for (int j = 0; j < m_DangerPoints.GetLength(1); j++)
                {
                    // Render a danger point based on whether or not there has been a weighting set.
                    if (m_DangerPoints[i,j].m_Weighting != 0)
                    {
                        // Draw the influence map based on the weighting that is given to the node
                        pSpriteBatch.Draw(m_DebugTexture, new Vector2(i * m_Level.TMXLevel.TileWidth,
                                                                      j * m_Level.TMXLevel.TileHeight),
                                                                      Color.Red * m_DangerPoints[i,j].m_Weighting);
                    }                                            
                }
            }
        }
    }
}
