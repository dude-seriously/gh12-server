using System;
using Server.Game;
using Server.Packets;
using Server.Game.Objects;

namespace Server.Sockets.Client {
	public abstract partial class SocketClient {
		protected User user;

		public User User {
			set {
				if(value == null && this.user != null) {
					this.user.States["removing"] = true;
				}

				this.user = value;

				if(this.user != null) {
					this.user.States["new"] = true;
				}

				DataPacket packet = PacketFactory.Make("userOwn");
				if(packet != null) {
					if(user != null) {
						packet["id"] = user.ID;
					} else {
						packet["id"] = 0;
					}
					this.Send(packet);
				}
			}
			get { return this.user; }
		}
	}
}