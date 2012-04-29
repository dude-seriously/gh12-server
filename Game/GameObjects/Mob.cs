using System;
using Server.Packets;

namespace Server.Game.Objects {
    public class Mob : GameObjectMovable {
        private DInt health;
        
        public Mob() {
            this.Speed = 10;
            this.health = new DInt(5);
        }
  
        public int Health {
            set { this.health.Value = value; }
            get { return this.health.Value; }
        }
        
        public override DataPacket PacketFull {
            get {
                DataPacket packet = PacketFactory.Make("mobUpdate");

                if(packet != null) {
                    packet["id"] = this.ID;

                    packet["s"] = this.speed.Value;
                    packet["x"] = this.x.Value;
                    packet["y"] = this.y.Value;
                    packet["h"] = this.health.Value;
         
                    return packet;
                } else {
                    return null;
                }
            }
        }
        
        public override DataPacket PacketUpdate {
            get {
                if(this.updated) {
                    this.updated = false;

                    DataPacket packet = PacketFactory.Make("mobUpdate");

                    if(packet != null) {
                        packet["id"] = this.ID;

                        if(this.speed.Updated) {
                            this.speed.Updated = false;
                            packet["s"] = this.speed.Value;
                        }
               
                        if(this.x.Updated) {
                            this.x.Updated = false;
                            packet["x"] = this.x.Value;
                        }
                
                        if(this.y.Updated) {
                            this.y.Updated = false;
                            packet["y"] = this.y.Value;
                        }
                        
                        if(this.health.Updated) {
                            this.health.Updated = false;
                            packet["h"] = this.health.Value;
                        }
             
                        return packet;
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            }
        }
        
        public override DataPacket PacketRemove {
            get { return PacketFactory.Make("mobRemove", "id", this.ID); }
        }
        
        public override void Update() {
            if (!this.deleted) {
                if (this.health.Value > 0) {
                    if (!this.IsMoving && Rand.Next()) {
                        if (Rand.Next()) {
                            if (Rand.Next()) {
                                this.Target = Map.inst.Cell(this.Cell.X + 1, this.Cell.Y);
                            } else {
                                this.Target = Map.inst.Cell(this.Cell.X - 1, this.Cell.Y);
                            }
                        } else {
                            if (Rand.Next()) {
                                this.Target = Map.inst.Cell(this.Cell.X, this.Cell.Y + 1);
                            } else {
                                this.Target = Map.inst.Cell(this.Cell.X, this.Cell.Y - 1);
                            }
                        }
                    }
                } else {
                    this.deleted = true;
                }
            }
            
            base.Update();
        }
    }
}