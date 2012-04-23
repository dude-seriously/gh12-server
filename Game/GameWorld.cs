using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Server.Packets;

namespace Server.Game {
	public class GameWorld {
		private ConcurrentDictionary<int, User> users;
		private ConcurrentDictionary<int, GameObject> objects;
		private List<DataPacket> packets;

		public GameWorld() {
			this.users = new ConcurrentDictionary<int, User>();
			this.objects = new ConcurrentDictionary<int, GameObject>();
			this.packets = new List<DataPacket>();
		}

		public User AddUser(string name) {
			User user = new User(name, this);
			bool result = this.users.TryAdd(user.ID, user);
			return user;
		}

		public bool RemoveUser(User user) {
			return this.users.TryRemove(user.ID, out user);
		}

		public ICollection<User> Users {
			get { return this.users.Values; }
		}

		public bool AddObject(GameObject obj) {
			return this.objects.TryAdd(obj.ID, obj);
		}

		public bool RemoveObject(GameObject obj) {
			return this.objects.TryRemove(obj.ID, out obj);
		}

		public ICollection<GameObject> Objects {
			get { return this.objects.Values; }
		}

		public void AddPacket(DataPacket packet) {
			this.packets.Add(packet);
		}

		public DataPacket[] GrabPackets() {
			DataPacket[] packets = new DataPacket[this.packets.Count];
			this.packets.CopyTo(packets);
			this.packets.Clear();
			return packets;
		}
		/*public void ClearPackets() {
			this.packets.Clear();
		}
		public ICollection<DataPacket> Packets {
			get { return this.packets; }
		}*/
	}
}