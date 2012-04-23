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

			this.game.CycleEnd += (packets) => this.server.PublishAsync(packets);

			this.game.Start();
		}

		public GameLoop Game {
			get { return this.game; }
		}
	}
}

