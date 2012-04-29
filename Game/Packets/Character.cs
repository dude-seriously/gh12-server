using System;
using Server.Packets;
using Server.Sockets.Client;
using Server.Game.Input;

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
    [APacket("charUpdate")]
    public class PacketCharUpdate : PacketType {
        static private PacketType instance = new PacketCharUpdate();

        private PacketCharUpdate() {
            this.AddField("id", "int"); // user id
            this.AddField("e", "bool"); // enabled ?
            this.AddField("s", "int"); // speed
            this.AddField("x", "int"); // x of target cell
            this.AddField("y", "int"); // y of target cell
        }
    }
    
    [APacket("charMove")]
    public class PacketCharMove : PacketType {
        static private PacketType instance = new PacketCharMove();

        private PacketCharMove() {
            this.AddField("d", "byte");
        }
    }

    [APacketEvent("charMove")]
    public class PacketCharMoveEvent : IPacketEvent {
        static private IPacketEvent instance = new PacketCharMoveEvent();

        private PacketCharMoveEvent() {
        }

        public void Process(SocketClient client, DataPacket packet) {
            if (client.User != null) {
                UserInput input = new UserInput("charMove");
                input["d"] = ((FieldData)packet["d"]).Value;
                client.User.AddInput(input);
                //client.User.Char.Target = 
            }
            /*if(client.User != null) {
                client.User.Name = packet["n"].ToString();
            }*/
        }
    }
    
    [APacket("mobSpawn")]
    public class PacketMobSpawn : PacketType {
        static private PacketType instance = new PacketMobSpawn();

        private PacketMobSpawn() {
            this.AddField("x", "int");
            this.AddField("y", "int");
        }
    }

    [APacketEvent("mobSpawn")]
    public class PacketMobSpawnEvent : IPacketEvent {
        static private IPacketEvent instance = new PacketMobSpawnEvent();

        private PacketMobSpawnEvent() {
        }

        public void Process(SocketClient client, DataPacket packet) {
            if (client.User != null) {
                UserInput input = new UserInput("mobSpawn");
                input["x"] = ((FieldData)packet["x"]).Value;
                input["y"] = ((FieldData)packet["y"]).Value;
                //input["d"] = ((FieldData)packet["d"]).Value;
                client.User.AddInput(input);
                //client.User.Char.Target = 
            }
            /*if(client.User != null) {
                client.User.Name = packet["n"].ToString();
            }*/
        }
    }
}