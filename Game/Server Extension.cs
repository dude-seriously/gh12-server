using System;
using Server.Game;

namespace Server.Sockets.Server {
	public sealed partial class SocketServer {
		private ServerInstance instance;

		public ServerInstance Instance {
			set {
				if(this.instance == null) {
					this.instance = value;
				}
			}
			get { return this.instance; }
		}
	}
}

