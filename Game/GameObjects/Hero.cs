using System;
using System.Linq;
using Server.Packets;

namespace Server.Game.Objects {
    public class Hero : GameObjectMovable {
        private DInt health;
        private DBool evil;
        private DByte kind;
        private DBool attacking;
        private DByte attackSpeed;
        private DByte attackDamage;
        
        private byte attack;
        private bool souled;
        
        public Hero() {
            this.Speed = 10;
            this.health = new DInt(Rand.Next(4,10));
            this.evil = new DBool();
            this.kind = new DByte();
            this.attacking = new DBool();
            this.attackSpeed = new DByte((byte)Rand.Next(5,12));
            this.attackDamage = new DByte((byte)Rand.Next(1,4));
            
            this.type = 10;
        }
  
        public override void Delete () {
            base.Delete ();
        }
        
        public int Health {
            set {
                this.updated = true;
                this.health.Value = value;
            }
            get { return this.health.Value; }
        }
        
        public bool Evil {
            set {
                this.updated = true;
                this.evil.Value = value;
            }
            get { return this.evil.Value; }
        }
        
        public byte Kind {
            set {
                this.updated = true;
                this.kind.Value = value;
            }
            get { return this.kind.Value; }
        }
        
        public bool Attacking {
            set {
                if (this.attacking.Value != value) {
                    this.updated = true;
                    this.attacking.Value = value;
                }
            }
            get { return this.attacking.Value; }
        }
        
        public byte AttackSpeed {
            set {
                this.updated = true;
                this.attackSpeed.Value = value;
            }
            get { return this.attackSpeed.Value; }
        }
        
        public byte AttackDamage {
            set {
                this.updated = true;
                this.attackDamage.Value = value;
            }
            get { return this.attackDamage.Value; }
        }
        
        public override DataPacket PacketFull {
            get {
                DataPacket packet = PacketFactory.Make("heroUpdate");

                if(packet != null) {
                    packet["id"] = this.ID;

                    packet["s"] = this.speed.Value;
                    packet["x"] = this.x.Value;
                    packet["y"] = this.y.Value;
                    packet["h"] = this.health.Value;
                    packet["e"] = this.evil.Value;
                    packet["t"] = this.kind.Value;
                    packet["a"] = this.attacking.Value;
                    packet["as"] = this.attackSpeed.Value;
                    packet["ad"] = this.attackDamage.Value;
         
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

                    DataPacket packet = PacketFactory.Make("heroUpdate");

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
                        
                        if(this.evil.Updated) {
                            this.evil.Updated = false;
                            packet["e"] = this.evil.Value;
                        }
                        
                        if(this.kind.Updated) {
                            this.kind.Updated = false;
                            packet["t"] = this.kind.Value;
                        }
                        
                        if(this.attacking.Updated) {
                            this.attacking.Updated = false;
                            packet["a"] = this.attacking.Value;
                        }
                        
                        if(this.attackSpeed.Updated) {
                            this.attackSpeed.Updated = false;
                            packet["as"] = this.attackSpeed.Value;
                        }
                        
                        if(this.attackDamage.Updated) {
                            this.attackDamage.Updated = false;
                            packet["ad"] = this.attackDamage.Value;
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
            get { return PacketFactory.Make("heroRemove", "id", this.ID); }
        }
        
        public override void Update() {
            if (!this.deleted) {
                if (this.health.Value > 0) {
                    
                    if (this.attack > 0) {
                        --this.attack;
                        this.Attacking = true;
                    } else {
                        this.Attacking = false;
                    }
                    
                    if (!this.IsMoving) {
                        Hero closestEnemy = null;
                        int dist = 0;
                        
                        for(int i = 0; i < GameWorld.inst.Objects.Count; ++i) {
                            if (GameWorld.inst.Objects.ElementAt(i).Type == 10) {
                                Hero mob = (Hero)GameWorld.inst.Objects.ElementAt(i);
                                
                                if (this.ID != mob.ID && mob.Evil != this.Evil && mob.Cell != null) {
                                    Cell a = this.Cell;
                                
                                    if (closestEnemy == null) {
                                        closestEnemy = mob;
                                        
                                        Cell b = closestEnemy.Cell;
                                        if (closestEnemy.Target != null) {
                                            b = closestEnemy.Target;
                                        }
                                        
                                        dist = Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
                                    } else {
                                        Cell b = mob.Cell;
                                        if (mob.Target != null) {
                                            b = mob.Target;
                                        }
                                        
                                        int possible = Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
                                        if (possible < dist) {
                                            closestEnemy = mob;
                                            dist = possible;
                                        }
                                    }
                                }
                            }
                        }
                        
                        if (closestEnemy != null) {
                            if (dist == 0 || dist == 1) {
                                if (this.attack == 0) {
                                    this.attack = this.attackSpeed.Value;
                                    
                                    closestEnemy.Health = closestEnemy.Health - this.AttackDamage;
                                }
                            } else if (!this.IsMoving) {
                                Cell a = this.Cell;
                                
                                Cell b = closestEnemy.Cell;
                                if (closestEnemy.Target != null) {
                                    b = closestEnemy.Target;
                                }
                            
                                int vert = a.Y - b.Y;
                                int horiz = a.X - b.X;
                                
                                if (Math.Abs(vert) > Math.Abs(horiz)) {
                                    if (vert > 0) {
                                        this.Target = Map.inst.Cell(a.X, a.Y - 1);
                                    } else {
                                        this.Target = Map.inst.Cell(a.X, a.Y + 1);
                                    }
                                } else {
                                    if (horiz > 0) {
                                        this.Target = Map.inst.Cell(a.X - 1, a.Y);
                                    } else {
                                        this.Target = Map.inst.Cell(a.X + 1, a.Y);
                                    }
                                }
                            }
                        } else {
                            if (Rand.Next()) {
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
                        }
                    }
                } else {
                    this.deleted = true;
                    if (!souled) {
                        souled = true;
                        
                        this.Cell.Soul += 1;
                    }
                }
            }
            
            base.Update();
        }
    }
}