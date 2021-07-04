using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#region MonoGame Libraries
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
#endregion

using AntRunner.Menu;

namespace AntRunner.States
{
    public class HelpState : State
    {
        MenuManager m_Manager = new MenuManager("HELP SCREEN");

        public HelpState()
        {
            // Add the required items for the HelpState
            m_Manager.AddMenuItem(new MenuItem()
            {
                Action = delegate(object sender, EventArgs e)
                {

                },
                Message = "PROJECT INFO"
            });

            m_Manager.AddMenuItem(new MenuItem()
            {
                Action = delegate(object sender, EventArgs e)
                {
                    // Change back to the main menu
                    MainGame.Instance.StateValue = StateValue.Menu;
                },
                Message = "RETURN"
            });
        }

        public override void Initialize()
        {
            m_Manager.Initialize();

            base.Initialize();
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                m_Manager.Draw(pSpriteBatch);
            pSpriteBatch.End();

            base.Draw(pSpriteBatch);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            m_Manager.Update(pGameTime, pInputHandler);

            base.Update(pGameTime, pInputHandler);
        }
        

    }
}
