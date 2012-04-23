using System;
using Server.Packets;
using Server.Sockets.Client;
using Server.Game.Input;

namespace Server.Game.Packets {
	[APacket("grenadeAdd")]
	public class PacketGrenadeAdd : PacketType {
		static private PacketType instance = new PacketGrenadeAdd();

		private PacketGrenadeAdd() {
			this.AddField("id", "int");
			this.AddField("x", "float");
			this.AddField("y", "float");
			this.AddField("tX", "float");
			this.AddField("tY", "float");
			this.AddField("speed", "float");
		}
	}

	[APacketEvent("grenadeAdd")]
	public class PacketGrenadeAddEvent : IPacketEvent {
		static private IPacketEvent instance = new PacketGrenadeAddEvent();

		private PacketGrenadeAddEvent() {
		}

		public void Process(SocketClient client, DataPacket packet) {
			if(client.User != null) {
				UserInput input = new UserInput("grenade");
				input["tX"] = packet["tX"];
				input["tY"] = packet["tY"];
				client.User.AddInput(input);
			}
		}
	}

	[APacket("grenadeRemove")]
	public class PacketGrenadeRemove: PacketType {
		static private PacketType instance = new PacketGrenadeRemove();

		private PacketGrenadeRemove() {
			this.AddField("id", "int");
		}
	}
}