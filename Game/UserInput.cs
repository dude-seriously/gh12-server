using System;
using System.Collections.Generic;

namespace Server.Game.Input {
	public class UserInput {
		private string name;
		private Dictionary<string, object> data;

		public UserInput(string name) {
			this.name = name;
			this.data = new Dictionary<string, object>();
		}

		public string Name {
			get { return this.name; }
		}

		public object this[string key] {
			set { this.data.Add(key, value); }
			get {
				object data = null;
				if(this.data.TryGetValue(key, out data)) {
					return data;
				} else {
					return null;
				}
			}
		}
	}
}