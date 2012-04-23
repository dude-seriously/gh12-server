using System;
using System.Collections.Concurrent;

namespace Server.Game {
	public class StateCollection {
		private ConcurrentDictionary<string, object> container;

		public StateCollection() {
			this.container = new ConcurrentDictionary<string, object>();
		}

		public object this[string state] {
			set {
				if(value == null) {
					this.container.TryRemove(state, out value);
				} else {
					this.container.AddOrUpdate(state, value, (key, old) => {
						return value; });
				}
			}
			get {
				object result = null;
				if(this.container.TryGetValue(state, out result)) {
					return result;
				} else {
					return null;
				}
			}
		}
	}
}