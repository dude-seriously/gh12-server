using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Server.Sockets.Client;

namespace Server.Packets {
	public sealed class FieldData {
		private object data;

		public FieldData() {
		}

		public FieldData(object data) {
			this.data = data;
		}

		public object Value {
			set { this.data = value; }
			get { return this.data; }
		}

		public bool IsNull {
			get {
				if(this.data == null) {
					return true;
				} else {
					return false;
				}
			}
		}

		public override int GetHashCode() {
			if(this.data != null) {
				return this.data.GetHashCode();
			} else {
				return 0;
			}
		}

		public override bool Equals(object obj) {
			if(this.data != null) {
				return this.data.Equals(obj);
			} else {
				return obj == null;
			}
		}

		public override string ToString() {
			if(this.data != null) {
				return this.data.ToString();
			} else {
				return string.Empty;
			}
		}
	}

	public sealed class DataPacket {
		private PacketType packet;
		private Dictionary<PacketField, FieldData> container;

		public DataPacket(PacketType packet) {
			this.packet = packet;

			this.container = new Dictionary<PacketField, FieldData>();

			foreach(PacketField field in this.packet.Fields) {
				this.container.Add(field, new FieldData(field.Default));
			}
		}

		public object this[string fieldName] {
			set {
				PacketField field = packet.GetField(fieldName);
				if(field != null && this.container.ContainsKey(field)) {
					this.container[field].Value = value;
				}
			}
			get {
				PacketField field = packet.GetField(fieldName);
				if(field != null && this.container.ContainsKey(field)) {
					return this.container[field];
				} else {
					return null;
				}
			}
		}

		public object this[PacketField field] {
			set {
				if(this.container.ContainsKey(field)) {
					this.container[field].Value = value;
				}
			}
			get {
				if(this.container.ContainsKey(field)) {
					return this.container[field];
				} else {
					return null;
				}
			}
		}

		public string Name {
			get { return this.packet.Name; }
		}

		public Dictionary<PacketField, FieldData> Enumerator {
			get { return this.container; }
		}

		public void TriggerEvents(SocketClient client) {
			this.packet.Trigger(client, this);
		}
	}
}