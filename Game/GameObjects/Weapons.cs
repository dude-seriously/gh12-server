using System;
using System.Collections.Generic;

namespace Server.Game.Objects {
	public class BulletContainer {
		public static BulletContainer instance = new BulletContainer();
		public List<Bullet> container;

		private BulletContainer() {
			this.container = new List<Bullet>();
		}

		public static void Add(Bullet bullet) {
			instance.container.Add(bullet);
		}

		public static void Update() {
			foreach(Bullet bullet in instance.container) {
				bullet.Update();
			}

			for(int i = 0; i < instance.container.Count; ++i) {
				if(instance.container[i].Dead) {
					Bullet bullet = instance.container[i];
					instance.container.Remove(bullet);
				}
			}
		}
	}

	public class Bullet : GameObject {
		public User owner;
		public float x, y;
		public float nX, nY;
		public float speed = 8f;
		private long created;
		private int life = 1000;
		private bool dead;

		public Bullet(User owner, float x, float y, float nX, float nY) {
			this.owner = owner;
			this.x = x;
			this.y = y;
			this.nX = nX;
			this.nY = nY;
			this.created = Time;
		}

		public int Life {
			set { this.life = value; }
			get { return this.life; }
		}

		public override void Update() {
			base.Update();

			if(!this.dead) {
				if(Time - this.created < this.life) {
					this.x += this.nX * this.speed;
					this.y += this.nY * this.speed;
				} else {
					this.dead = true;
				}
			}
		}

		public bool Dead {
			set { this.dead = value; }
			get { return this.dead; }
		}
	}
}