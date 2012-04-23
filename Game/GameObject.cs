using System;

namespace Server.Game {
	public class GameObject {
		private static long time;
		private static int counter;

		public static long Time {
			set { time = value; }
			get { return time; }
		}

		private int id;
		public int type;

		protected GameObject() {
			this.id = ++counter;
		}

		public int ID {
			get { return this.id; }
		}

		public virtual void Update() {
		}
	}

	public class GameObjectControlled : GameObject {
		private User owner;

		protected GameObjectControlled() {
		}

		public User Owner {
			set {
				this.owner = value;
				/*if (this.owner != value) {
					if (this.owner != null) {
						this.owner.RemoveOwnage(this);
					}

					this.owner = value;

					if (this.owner != null) {
						this.owner.AddOwnage(this);
					}
				}*/
			}
			get { return this.owner; }
		}
	}

	public class GameObjectMovable : GameObjectControlled {
		public float x;
		public float y;
		private float nX;
		private float nY;
		public float speed;

		protected GameObjectMovable() {
		}

		public float X {
			set { this.x = value; }
			get { return this.x; }
		}

		public float Y {
			set { this.y = value; }
			get { return this.y; }
		}

		public void SetDirection(float nX, float nY) {
			if(nX != 0 || nY != 0) {
				float length = (float)Math.Sqrt(nX * nX + nY * nY);
				this.nX = nX / length;
				this.nY = nY / length;
			} else {
				this.nX = 0;
				this.nY = 0;
			}
		}

		public override void Update() {
			base.Update();

			if(this.nX != 0 || this.nY != 0) {
				this.x += this.nX * speed;
				this.y += this.nY * speed;
			}
		}
	}
}