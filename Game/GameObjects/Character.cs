using System;

namespace Server.Game.Objects {
	public class Character : GameObjectMovable {
		private float size;
		private float health = 100;
		private bool zombie = false;

		public Character() {
			this.speed = 3f;
			this.size = 32f;
			this.type = 1;
		}

		public float Size {
			set { this.size = value; }
			get { return this.size; }
		}

		public float Health {
			set {
				this.health = value;
				if(!this.zombie && this.health < 70) {
					this.zombie = true;
				}
				if(this.zombie) {
					this.speed = 7f + ((100 - this.health) / 30f);
				} else {
					this.speed = 3f;
				}
			}
			get { return this.health; }
		}

		public bool Zombie {
			get { return this.zombie; }
		}

		public override void Update() {
			base.Update();

			if(this.health <= 0) {
				this.x = (float)Rand.Next(-320, 320);
				this.y = (float)Rand.Next(-240, 240);
				this.health = 100;
				this.speed = 3f;
				this.zombie = false;
			} else {
				if(this.zombie) {
					this.health = this.health - .5f;
				}
			}
		}
	}
}