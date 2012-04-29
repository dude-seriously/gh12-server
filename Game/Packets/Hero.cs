using System;
using Server.Packets;
using Server.Sockets.Client;

namespace Server.Game.Packets {
    [APacket("heroUpdate")]
    public class PacketHeroUpdate : PacketType {
        static private PacketType instance = new PacketHeroUpdate();

        private PacketHeroUpdate() {
            this.AddField("id", "int"); // user id
            this.AddField("s", "int"); // speed
            this.AddField("x", "int"); // x of target cell
            this.AddField("y", "int"); // y of target cell
            this.AddField("h", "int"); // health
            this.AddField("e", "bool"); // evil ?
            this.AddField("t", "byte"); // type
            this.AddField("a", "bool"); // attacking ?
            this.AddField("as", "byte"); // attack speed
            this.AddField("ad", "byte"); // attack damage
        }
    }
    
    [APacket("heroRemove")]
    public class PacketHeroRemove : PacketType {
        static private PacketType instance = new PacketHeroRemove();

        private PacketHeroRemove() {
            this.AddField("id", "int"); // user id
        }
    }
}