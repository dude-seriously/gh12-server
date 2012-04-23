using System;
using Server.Packets;
using Server.Sockets.Client;

namespace Server.Game.Packets {
	[APacket("characterSet")]
	public class PacketCharacterSet : PacketType {
		static private PacketType instance = new PacketCharacterSet();

		private PacketCharacterSet() {
			this.AddField("id", "int");
			this.AddField("userId", "int");
		}
	}

	[APacket("characterAdd")]
	public class PacketCharacterAdd : PacketType {
		static private PacketType instance = new PacketCharacterAdd();

		private PacketCharacterAdd() {
			this.AddField("id", "int");
			this.AddField("size", "float");
			this.AddField("x", "float");
			this.AddField("y", "float");
		}
	}

	[APacket("characterRemove")]
	public class PacketCharacterRemove : PacketType {
		static private PacketType instance = new PacketCharacterRemove();

		private PacketCharacterRemove() {
			this.AddField("id", "int");
		}
	}

	[APacket("characterSetDirection")]
	public class PacketCharacterSetDirection : PacketType {
		static private PacketType instance = new PacketCharacterSetDirection();

		private PacketCharacterSetDirection() {
			this.AddField("id", "int");
			this.AddField("nX", "float");
			this.AddField("nY", "float");
		}
	}

	[APacketEvent("characterSetDirection")]
	public class PacketCharacterSetDirectionEvent : IPacketEvent {
		static private IPacketEvent instance = new PacketCharacterSetDirectionEvent();

		private PacketCharacterSetDirectionEvent() {
		}

		public void Process(SocketClient client, DataPacket packet) {
			if(client.User != null && client.User.Char != null && client.User.Char.ID == int.Parse(packet["id"].ToString())) {
				client.User.Char.SetDirection(float.Parse(packet["nX"].ToString()), float.Parse(packet["nY"].ToString()));
			}
		}
	}

	[APacket("characterUpdate")]
	public class PacketCharacterUpdate : PacketType {
		static private PacketType instance = new PacketCharacterUpdate();

		private PacketCharacterUpdate() {
			this.AddField("id", "int");
			this.AddField("size", "float");
			this.AddField("health", "float");
			this.AddField("x", "float");
			this.AddField("y", "float");
			this.AddField("z", "bool");
		}
	}
}