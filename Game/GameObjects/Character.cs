using System;
using Server.Packets;
using System.Collections.Generic;

namespace Server.Game.Objects {
    public class Character : GameObjectMovable {
        private User owner;
        private DBool enabled;
        private byte direction;
        
        public Character(User user) {
            this.owner = user;
            this.enabled = new DBool();
            this.Speed = 3;
        }

        public User Owner {
            get { return this.owner; }
        }
        
        public bool Enabled {
            set {
                this.updated = true;
                this.enabled.Value = value;
            }
            get { return this.enabled.Value; }
        }
        
        public byte Direction {
            set { this.direction = value; }
            get { return this.direction; }
        }
        
        public override DataPacket PacketFull {
            get {
                if(this.owner != null) {
                    DataPacket packet = PacketFactory.Make("charUpdate");

                    if(packet != null) {
                        packet["id"] = this.owner.ID;
                        
                        packet["e"] = this.enabled.Value;
                        packet["s"] = this.speed.Value;
                        packet["x"] = this.x.Value;
                        packet["y"] = this.y.Value;
             
                        return packet;
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            }
        }
        
        public override DataPacket PacketUpdate {
            get {
                if(this.updated) {
                    this.updated = false;

                    if(this.owner != null) {
                        DataPacket packet = PacketFactory.Make("charUpdate");

                        if(packet != null) {
                            packet["id"] = this.owner.ID;

                            if(this.enabled.Updated) {
                                this.enabled.Updated = false;
                                packet["e"] = this.enabled.Value;
                            }
                    
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
                 
                            return packet;
                        } else {
                            return null;
                        }
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            }
        }
        
        public override void Update() {
            if (this.enabled.Value.Equals(true) && !this.IsMoving && this.direction != 0) {
                switch(this.direction) {
                    case 1:
                        this.Target = Map.inst.Cell(this.Cell.X, this.Cell.Y - 1);
                        break;
                    case 2:
                        this.Target = Map.inst.Cell(this.Cell.X + 1, this.Cell.Y);
                        break;
                    case 3:
                        this.Target = Map.inst.Cell(this.Cell.X, this.Cell.Y + 1);
                        break;
                    case 4:
                        this.Target = Map.inst.Cell(this.Cell.X - 1, this.Cell.Y);
                        break;
                }
                if (this.Target != null) {
                    if (this.Target.Soul > 0) {
                        this.owner.Score += this.Target.Soul * 2;
                        this.Target.Soul = 0;
                    }
                }
            }
            
            base.Update();
        }
    }
}