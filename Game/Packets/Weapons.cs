using System;
using Server.Packets;
using Server.Sockets.Client;
using Server.Game.Input;

namespace Server.Game.Packets {
	[APacket("bulletAdd")]
	public class PacketBulletAdd : PacketType {
		static private PacketType instance = new PacketBulletAdd();

		private PacketBulletAdd() {
			this.AddField("id", "int");
			this.AddField("x", "float");
			this.AddField("y", "float");
			this.AddField("nX", "float");
			this.AddField("nY", "float");
			this.AddField("life", "int");
			this.AddField("speed", "float");
		}
	}

	[APacketEvent("bulletAdd")]
	public class PacketBulletAddEvent : IPacketEvent {
		static private IPacketEvent instance = new PacketBulletAddEvent();

		private PacketBulletAddEvent() {
		}

		public void Process(SocketClient client, DataPacket packet) {
			if(client.User != null) {
				UserInput input = new UserInput("shot");
				input["nX"] = packet["nX"];
				input["nY"] = packet["nY"];
				client.User.AddInput(input);
			}
		}
	}

	[APacket("bulletRemove")]
	public class PacketBulletRemove: PacketType {
		static private PacketType instance = new PacketBulletRemove();

		private PacketBulletRemove() {
			this.AddField("id", "int");
		}
	}
}