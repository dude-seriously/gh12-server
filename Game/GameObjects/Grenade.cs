using System;
using System.Collections.Generic;

namespace Server.Game.Objects {
	public class GrenadeContainer {
		public static GrenadeContainer instance = new GrenadeContainer();
		public List<Grenade> container;

		private GrenadeContainer() {
			this.container = new List<Grenade>();
		}

		public static void Add(Grenade grenade) {
			instance.container.Add(grenade);
		}

		public static void Update() {
			foreach(Grenade grenade in instance.container) {
				grenade.Update();
			}
		}
	}

	public class Grenade : GameObject {
		public User owner;
		public float x, y;
		public float tX, tY;
		public float speed = 10f;
		private bool dead;

		public Grenade(User owner, float x, float y, float tX, float tY) {
			this.owner = owner;
			this.x = x;
			this.y = y;
			this.tX = tX;
			this.tY = tY;
		}

		public override void Update() {
			base.Update();

			if(!this.dead) {
				float nX = this.tX - this.x;
				float nY = this.tY - this.y;
				float dist = (float)Math.Sqrt(nX * nX + nY * nY);

				if(dist > this.speed * 1.5f) {
					nX /= dist;
					nY /= dist;

					this.x += nX * this.speed;
					this.y += nY * this.speed;
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