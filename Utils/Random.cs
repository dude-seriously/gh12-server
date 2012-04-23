using System;

namespace Server {
	public class Rand {
		private static Rand instance = new Rand();

		private Random random;

		private Rand() {
			this.random = new Random(Environment.TickCount);
		}

		public static bool Next() {
			return instance.random.Next(2) == 0;
		}

		public static int Next(int max) {
			return instance.random.Next(max);
		}

		public static int Next(int min, int max) {
			return instance.random.Next(min, max);
		}

		public static float NextFloat() {
			return (float)instance.random.NextDouble();
		}

		public static float NextFloat(float max) {
			return (float)instance.random.NextDouble() * max;
		}

		public static float NextFloat(float min, float max) {
			return (float)instance.random.NextDouble() * (max - min) + min;
		}

		public static double NextDouble() {
			return instance.random.NextDouble();
		}

		public static double NextDouble(double max) {
			return instance.random.NextDouble() * max;
		}

		public static double NextDouble(double min, double max) {
			return instance.random.NextDouble() * (max - min) + min;
		}
	}
}

