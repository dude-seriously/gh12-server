using System;
using System.Text;

namespace Server.Game {
	public class Map {
        private int width;
        private int height;
        private Cell[] cells;
        
        private bool cache;
        private string array;
        
		public Map(int width, int height) {
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
            return this.cells[y * this.width + x];
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
        
        private int type;
        
        public Cell(Map map, int x, int y, int type) {
            this.map = map;
            this.x = x;
            this.y = y;
            this.type = type;
        }
        
        public int Type {
            set { this.type = value; }
            get { return this.type; }
        }
    }
}