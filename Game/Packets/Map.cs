using System;
using Server.Packets;
using Server.Sockets.Client;

namespace Server.Game.Packets {
    [APacket("mapData")]
    public class PacketMapData : PacketType {
        static private PacketType instance = new PacketMapData();

        private PacketMapData() {
            this.AddField("w", "int"); // width
            this.AddField("h", "int"); // height
            this.AddField("d", "string"); // data as string, numbers devided by comma
        }
    }
    
    [APacket("soulUpdate")]
    public class PacketSoulUpdate : PacketType {
        static private PacketType instance = new PacketSoulUpdate();

        private PacketSoulUpdate() {
            this.AddField("id", "int");
            this.AddField("v", "int");
            this.AddField("x", "int");
            this.AddField("y", "int");
        }
    }
}