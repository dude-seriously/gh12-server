using System;
using Server.Packets;
using Server.Sockets.Client;
using Server.Sockets.Server;

namespace Server.Game.Packets {
	[APacket("userOwn")]
	public class PacketUserOwn : PacketType {
		static private PacketType instance = new PacketUserOwn();

		private PacketUserOwn() {
			this.AddField("id", "int");
		}
	}
    
     [APacket("userStart")]
     public class PacketUserStart : PacketType {
         static private PacketType instance = new PacketUserStart();
    
         private PacketUserStart() { }
     }

	[APacket("userAdd")]
	public class PacketUserAdd : PacketType {
		static private PacketType instance = new PacketUserAdd();

		private PacketUserAdd() {
			this.AddField("id", "int");
			this.AddField("n", "string");
			this.AddField("l", "short");
		}
	}

	[APacket("userRemove")]
	public class PacketUserRemove : PacketType {
		static private PacketType instance = new PacketUserRemove();

		private PacketUserRemove() {
			this.AddField("id", "int");
		}
	}

	[APacket("userSetName")]
	public class PacketUserSetName : PacketType {
		static private PacketType instance = new PacketUserSetName();

		private PacketUserSetName() {
			this.AddField("id", "int");
			this.AddField("n", "string");
		}
	}

	[APacketEvent("userSetName")]
	public class PacketUserSetNameEvent : IPacketEvent {
		static private IPacketEvent instance = new PacketUserSetNameEvent();

		private PacketUserSetNameEvent() {
		}

		public void Process(SocketClient client, DataPacket packet) {
			if(client.User != null) {
				client.User.Name = packet["n"].ToString();
				Log.Add(packet["n"].ToString());
			}
		}
	}

	[APacket("userUpdate")]
	public class PacketUserUpdate : PacketType {
		static private PacketType instance = new PacketUserUpdate();

		private PacketUserUpdate() {
			this.AddField("id", "int");
            this.AddField("n", "string");
            this.AddField("l", "short");
		}
	}
}