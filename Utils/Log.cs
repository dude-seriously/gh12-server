using System;
using System.Diagnostics;

namespace Server {
	static class Log {
		public static void Add(string message) {
			Console.WriteLine(message);
			Debug.WriteLine(message);
		}
	}
}

