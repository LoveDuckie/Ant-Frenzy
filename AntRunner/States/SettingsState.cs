using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.States;
using AntRunner.Entity;
using AntRunner.Menu;

// Required for the serialization
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;
using System.Runtime.Serialization;

namespace AntRunner.States
{
    [DataContract]
    public class SettingsObject
    {
        [DataMember]
        public bool AutoHostGame { get; set; }
    }

    public class SettingsState : State
    {
        public MenuManager m_MenuManager;

        public SettingsState()
        {
            m_MenuManager = new MenuManager("Settings", new Vector2(MainGame.Instance.Window.ClientBounds.Width / 2 - 125,
                                                                    MainGame.Instance.Window.ClientBounds.Height / 2));

            m_MenuManager.AddMenuItem(new MenuItem()
             {
                 Message = "GAME SETTINGS",
                 Action = delegate(object sender, EventArgs e)
                 {

                 }
             });

            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "PAD SETTINGS",
                Action = delegate(object sender, EventArgs e)
                {

                }
            });

            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "RETURN",
                Action = delegate(object sender, EventArgs e)
                {
                    MainGame.Instance.StateValue = StateValue.Menu;
                }
            });
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            m_MenuManager.Update(pGameTime, pInputHandler);
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Draw the menu manager accordingly.
            pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            int _renderGreenX = MainGame.Instance.Window.ClientBounds.Width / 64;
            int _renderGreenY = (MainGame.Instance.Window.ClientBounds.Height / 64) + 1;

            for (int x = 0; x < _renderGreenX; x++)
            {
                // Draw the green back ground just for funsies.
                for (int y = 0; y < _renderGreenY; y++)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["terrain_tiles"], new Vector2(x * 64, y * 64), new Rectangle(2 * 64, 9 * 64, 64, 64), Color.White);
                }
            }

            m_MenuManager.Draw(pSpriteBatch);

            pSpriteBatch.End();
            base.Draw(pSpriteBatch);
        }


    }
}
