using System;
using System.Collections.Concurrent;
using Server.Packets;
using Server.Game.Input;
using Server.Game.Objects;
using Server.Sockets.Client;

namespace Server.Game {
	public class User {
		private static int counter;
		private int id;
		private GameWorld world;
		private StateCollection states;
		private ConcurrentQueue<UserInput> input;
		private Character character;
		private bool updated;
		private DString name;
		private DInt score;
		private DInt deaths;
		private DShort latency;
		public long lastGrenade = 0;
		public static int grenadeDelay = 3000;
		public long lastShoot = 0;
		public static int shootDelay = 200;
		public long lastEmotion = 0;
		public static int emotionDelay = 2000;
		private SocketClient client;
		//private ConcurrentDictionary<int, GameObjectControlled> objects;

		public User(string name, GameWorld world) {
			this.id = ++counter;
			this.world = world;
			this.states = new StateCollection();
			this.input = new ConcurrentQueue<UserInput>();
			this.character = new Character();
			this.character.Owner = this;

			//this.updated = true;
			this.name = new DString(name);
			this.score = new DInt();
			this.deaths = new DInt();
			this.latency = new DShort(-1);
			//this.objects = new ConcurrentDictionary<int, GameObjectControlled>();
		}

		public int ID {
			get { return this.id; }
		}

		public SocketClient Client {
			set { this.client = value; }
			get { return this.client; }
		}

		public string Name {
			set {
				if(!this.name.Value.Equals(value)) {
					this.updated = true;
					this.name.Value = value;
				}
			}
			get { return this.name.Value; }
		}

		public int Score {
			set {
				if(!this.score.Value.Equals(value)) {
					this.updated = true;
					this.score.Value = value;
				}
			}
			get { return this.score.Value; }
		}

		public int Deaths {
			set {
				if(!this.deaths.Value.Equals(value)) {
					this.updated = true;
					this.deaths.Value = value;
				}
			}
			get { return this.deaths.Value; }
		}

		public short Latency {
			set {
				if(!this.latency.Value.Equals(value)) {
					this.updated = true;
					this.latency.Value = value;
				}
			}
			get { return this.latency.Value; }
		}

		public DataPacket PacketUpdate {
			get {
				if(this.updated) {
					this.updated = false;
	
					DataPacket packet = PacketFactory.Make("userUpdate");

					if(packet != null) {
						packet["id"] = this.id;

						if(this.name.Updated) {
							this.name.Updated = false;
							packet["n"] = this.name.Value;
						}

						if(this.score.Updated) {
							this.score.Updated = false;
							packet["k"] = this.score.Value;
						}

						if(this.deaths.Updated) {
							this.deaths.Updated = false;
							packet["d"] = this.deaths.Value;
						}

						if(this.latency.Updated) {
							this.latency.Updated = false;
							packet["l"] = this.latency.Value;
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

		public GameWorld World {
			get { return this.world; }
		}

		public void Remove() {
			this.world.RemoveUser(this);
			this.world = null;
			this.states = null;
			this.input = null;
			//this.objects = null;
		}

		public StateCollection States {
			get { return this.states; }
		}

		public void AddInput(UserInput input) {
			this.input.Enqueue(input);
		}

		public UserInput GrabInput() {
			UserInput input = null;
			this.input.TryDequeue(out input);
			return input;
		}

		public Character Char {
			get { return this.character; }
		}

		/*public bool AddOwnage(GameObjectControlled obj) {
			return this.objects.TryAdd(obj.ID, obj);
		}
		public bool RemoveOwnage(GameObjectControlled obj) {
			return this.objects.TryRemove(obj.ID, out obj);
		}*/
	}
}