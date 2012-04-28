using System;
using Server.Packets;
using Server.Sockets.Client;

namespace Server.Game.Packets {
	/*[APacket("charSet")]
	public class PacketCharSet : PacketType {
		static private PacketType instance = new PacketCharSet();

		private PacketCharSet() {
			this.AddField("id", "int"); // user id
			this.AddField("x", "int"); // cell x
			this.AddField("y", "int"); // cell y
		}
	}*/

    [APacket("charEnable")]
    public class PacketCharEnable : PacketType {
        static private PacketType instance = new PacketCharEnable();

        private PacketCharEnable() {
            this.AddField("id", "int"); // user id
            this.AddField("s", "bool"); // state true / false
        }
    }
}