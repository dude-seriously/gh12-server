using System;
using System.Text;
using System.Collections.Generic;
using Server.Game.Objects;
using Server.Packets;

namespace Server.Game {
	public class Map {
        public static Map inst;
        
        private int width;
        private int height;
        private Cell[] cells;
        
        private bool cache;
        private string array;
        
		public Map(int width, int height) {
            inst = this;
            this.width = width;
            this.height = height;
            
			this.cells = new Cell[this.width * this.height];
            
            for(int y = 0; y < this.height; ++y) {
                for(int x = 0; x < this.width; ++x) {
                    this.cells[y * this.width + x] = new Cell(this, x, y, 0);
                }
            }
		}
        
        public Cell Cell(int x, int y) {
            if (x >= 0 && x < this.width &&
                y >= 0 && y < this.height) {
                
                return this.cells[y * this.width + x];
            }
            return null;
        }
        
        public int Width {
            get { return this.width; }
        }
        
        public int Height {
            get { return this.height; }
        }
        
        public string MapArray {
            get {
                if (!this.cache) {
                    this.cache = true;
                    
                    StringBuilder arr = new StringBuilder();
                    
                    for(int y = 0; y < this.height; ++y) {
                        for(int x = 0; x < this.width; ++x) {
                            if (!(x == 0 && y == 0)) {
                                arr.Append(",");
                            }
                            
                            arr.Append(this.cells[y * this.width + x].Type.ToString());
                        }
                    }
                    
                    this.array = arr.ToString();
                }
                
                return this.array;
            }
        }
	}
    
    public class Cell {
        private Map map;
        private int x;
        private int y;
        
        private DInt soul;
        private bool updated;
        
        private int type;
        
        public Cell(Map map, int x, int y, int type) {
            this.map = map;
            this.x = x;
            this.y = y;
            this.type = type;
            this.soul = new DInt();
            this.updated = false;
        }
        
        public int Soul {
            set {
                updated = true;
                this.soul.Value = value;
            }
            get { return this.soul.Value; }
        }
        
        public int Type {
            set { this.type = value; }
            get { return this.type; }
        }
        
        public int X {
            get { return this.x; }
        }
        
        public int Y {
            get { return this.y; }
        }
        
        public DataPacket PacketFull {
            get {
                if (this.soul.Value > 0) {
                    DataPacket packet = PacketFactory.Make("soulUpdate");
    
                    if(packet != null) {
                        packet["id"] = this.x * map.Height + this.y;
    
                        packet["v"] = this.soul.Value;
                        packet["x"] = this.x;
                        packet["y"] = this.y;
             
                        return packet;
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            }
        }
        
        public DataPacket PacketUpdate {
            get {
                if(this.updated) {
                    this.updated = false;

                    DataPacket packet = PacketFactory.Make("soulUpdate");

                    if(packet != null) {
                        packet["id"] = this.x * map.Height + this.y;

                        if(this.soul.Updated) {
                            this.soul.Updated = false;
                            packet["v"] = this.soul.Value;
                        }
                        
                        packet["x"] = this.x;
                        packet["y"] = this.y;
                        
                        return packet;
                    } else {
                        return null;
                    }
                } else {
                    return null;
                }
            }
        }
    }
}