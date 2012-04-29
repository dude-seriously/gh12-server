using System;
using Server.Sockets.Server;
using Server.Game.Objects;

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
            this.game.Map = new Map(40, 26);
            
            for(int y = 0; y < this.game.Map.Height; ++y) {
                for(int x = 0; x < this.game.Map.Width; ++x) {
                    int type = 0;
                    
                    /*if ((x < 24 && y < 24) ||
                        (x > 48 && y < 24) ||
                        (x < 24 && y > 48) ||
                        (x > 48 && y > 48)) {*/
                        
                    /*if ((x < 3 && y < 3) ||
                        (x > 5 && y < 3) ||
                        (x < 3 && y > 5) ||
                        (x > 5 && y > 5)) {
                        
                        type = 1;
                    }*/
                    
                    this.game.Map.Cell(x, y).Type = Rand.Next(0, 2);
                }
            }
            
            for(int i = 0; i < 2; ++i) {
                Hero hero = new Hero();
                hero.Cell = this.game.Map.Cell(Rand.Next(this.game.Map.Width), Rand.Next(this.game.Map.Height));
                this.game.World.AddObject(hero);
            }

			this.game.CycleEnd += (packets) => this.server.PublishAsync(packets);

			this.game.Start();
		}

		public GameLoop Game {
			get { return this.game; }
		}
	}
}