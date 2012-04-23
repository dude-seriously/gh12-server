using System;
using Server.Packets;
using Server.Sockets.Client;
using Server.Sockets.Server;

namespace Server.Game.Packets {
	[APacket("emotionAdd")]
	public class PacketEmotionAdd : PacketType {
		static private PacketType instance = new PacketEmotionAdd();

		private PacketEmotionAdd() {
			this.AddField("id", "int");
			this.AddField("t", "int");
		}
	}

	[APacketEvent("emotionAdd")]
	public class PacketEmotionAddEvent : IPacketEvent {
		static private IPacketEvent instance = new PacketEmotionAddEvent();

		private PacketEmotionAddEvent() {
		}

		public void Process(SocketClient client, DataPacket packet) {
			if(client.User != null && client.User.Char != null) {
				if(GameObject.Time - client.User.lastEmotion > User.emotionDelay) {
					client.User.lastEmotion = GameObject.Time;
					{
						DataPacket pckt = PacketFactory.Make("emotionAdd");
						if(pckt != null) {
							pckt["id"] = client.User.Char.ID;
							pckt["t"] = int.Parse(packet["t"].ToString());
							SocketServer.inst.Publish(pckt);
						}
					}
				}
			}
		}
	}
}