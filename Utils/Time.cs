using System.Diagnostics;

namespace Server {
	public class Time {
		private static Time instance = new Time();

		private Stopwatch watches;

		private Time() {
			this.watches = new Stopwatch();
			this.watches.Start();
		}

		public static long Now {
			get {
				return instance.watches.ElapsedMilliseconds;
			}
		}

		public static bool Elapsed(long from, long length) {
			return (instance.watches.ElapsedMilliseconds - from) > length;
		}
	}
}

