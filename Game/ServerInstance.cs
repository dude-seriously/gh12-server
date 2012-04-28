using System;
using Server.Sockets.Server;

namespace Server.Game {
	public class ServerInstance {
		private SocketServer server;
		private GameLoop game;

		public ServerInstance() {
			this.server = new SocketServer();
			this.server.Instance = this;
			this.server.Start();

			this.game = new GameLoop();
			this.game.World = new GameWorld();
            this.game.Map = new Map(64, 64);
            
            for(int y = 0; y < this.game.Map.Height; ++y) {
                for(int x = 0; x < this.game.Map.Width; ++x) {
                    this.game.Map.Cell(x, y).Type = Rand.Next(0, 2);
                }
            }

			this.game.CycleEnd += (packets) => this.server.PublishAsync(packets);

			this.game.Start();
		}

		public GameLoop Game {
			get { return this.game; }
		}
	}
}