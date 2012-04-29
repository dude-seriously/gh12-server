using System;
using Server.Packets;
using Server.Game.Objects;

namespace Server.Game {
	public class GameObject {
		private static long time;
		private static int counter;
        
		public static long Time {
			set { time = value; }
			get { return time; }
		}

		private int id;
        protected bool deleted;
        protected bool updated;
        protected int type;

		protected GameObject() {
			this.id = ++counter;
		}

		public int ID {
			get { return this.id; }
		}
  
        public bool Deleted {
            get { return this.deleted; }
        }
        
        public int Type {
            set { this.type = value; }
            get { return this.type; }
        }
        
        public virtual void Delete() {
            
        }
        
        public virtual DataPacket PacketFull {
            get { return null; }
        }
        
        public virtual DataPacket PacketUpdate {
            get { return null; }
        }
        
        public virtual DataPacket PacketRemove {
            get { return null; }
        }
        
		public virtual void Update() {
		}
	}

	public class GameObjectMovable : GameObject {
        private Cell cell;
        private Cell target;
        
        protected int move;
        
        protected DInt speed;
        protected DInt x;
        protected DInt y;

		protected GameObjectMovable() {
            this.speed = new DInt();
            this.x = new DInt();
            this.y = new DInt();
        }
  
        public override void Delete() {
            base.Delete();
            
            this.cell = null;
            this.target = null;
        }
        
        public bool IsMoving {
            get {
                return this.target != null;
            }
        }
        
		public Cell Cell {
            set {
                if (this.cell != value && value != null) {
                    this.move = 0;
                    this.cell = value;
                    this.updated = true;
                    this.x.Value = this.cell.X;
                    this.y.Value = this.cell.Y;
                }
            }
            get { return this.cell; }
        }
        
        public Cell Target {
            set {
                if (this.target != value && value != null) {
                    this.move = 1;
                    this.target = value;
                    this.updated = true;
                    this.x.Value = this.target.X;
                    this.y.Value = this.target.Y;
                }
            }
            get { return this.target; }
        }
        
        public int Speed {
            set {
                if (!this.speed.Value.Equals(value)) {
                    this.updated = true;
                    this.speed.Value = value;
                }
            }
            get { return this.speed.Value; }
        }
  
        public int X {
            get { return this.x.Value; }
        }
        
        public int Y {
            get { return this.y.Value; }
        }
        
		public override void Update() {
			base.Update();

            if (this.target != null) {
                if (this.move <= (int)this.speed.Value) {
                    ++this.move;
                } else {
                    this.cell = this.target;
                    this.target = null;
                }
            }
		}
	}
}