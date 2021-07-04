using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;

using AntRunner.Entity;
using AntRunner.States;
using AntRunner.Menu;

namespace AntRunner
{
    public class NetworkLayer : IEntity
    {
        public NetworkLayer()
        {
            Initialize();
        }

        public void Initialize()
        {
            // Wait for an incoming co
            Task.Run(async () =>
            {
                StreamSocketListener _listener = new StreamSocketListener();

                // Find the port on to the IP address
                await _listener.BindEndpointAsync(new HostName("localhost"), "6060");
            });

        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            throw new NotImplementedException();
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        }
    }
}
