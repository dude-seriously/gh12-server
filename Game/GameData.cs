namespace Server.Game {
	public abstract class GameData {
		protected bool updated;
		protected bool isNull;

		public bool Updated {
			set { this.updated = value; }
			get { return this.updated; }
		}

		public bool Null {
			set { this.isNull = value; }
			get { return this.isNull; }
		}
	}

	public class DBool : GameData {
		private bool data;

		public DBool() {
			this.isNull = true;
		}
		public DBool(bool data) {
			this.updated = true;
			this.data = data;
		}

		public bool Value {
			set {
				if (this.data != value) {
					this.updated = true;
					this.isNull = false;
					this.data = value;
				}
			}
			get { return this.data; }
		}
	}

	public class DByte : GameData {
		private byte data;

		public DByte() {
			this.isNull = true;
		}
		public DByte(byte data) {
			this.updated = true;
			this.data = data;
		}

		public byte Value {
			set {
				if (this.data != value) {
					this.updated = true;
					this.isNull = false;
					this.data = value;
				}
			}
			get { return this.data; }
		}
	}

	public class DShort : GameData {
		private short data;

		public DShort() {
			this.isNull = true;
		}
		public DShort(short data) {
			this.updated = true;
			this.data = data;
		}

		public short Value {
			set {
				if (this.data != value) {
					this.updated = true;
					this.isNull = false;
					this.data = value;
				}
			}
			get { return this.data; }
		}
	}

	public class DInt : GameData {
		private int data;

		public DInt() {
			this.isNull = true;
		}
		public DInt(int data) {
			this.updated = true;
			this.data = data;
		}

		public int Value {
			set {
				if (this.data != value) {
					this.updated = true;
					this.isNull = false;
					this.data = value;
				}
			}
			get { return this.data; }
		}
	}

	public class DFloat : GameData {
		private float data;

		public DFloat() {
			this.isNull = true;
		}
		public DFloat(float data) {
			this.updated = true;
			this.data = data;
		}

		public float Value {
			set {
				if (this.data != value) {
					this.updated = true;
					this.isNull = false;
					this.data = value;
				}
			}
			get { return this.data; }
		}
	}

	public class DString : GameData {
		private string data;

		public DString() {
			this.isNull = true;
		}
		public DString(string data) {
			this.updated = true;
			this.data = data;
		}

		public string Value {
			set {
				if (this.data != value) {
					this.updated = true;
					this.isNull = false;
					this.data = value;
				}
			}
			get { return this.data; }
		}
	}
}