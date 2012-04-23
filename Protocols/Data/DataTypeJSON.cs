using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Protocols.Data {
	[APacketDataTypeParser("bool", "json")]
	public sealed class PacketDataTypeParserBoolJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserBoolJSON();

		private PacketDataTypeParserBoolJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				if(((string)data[0]) == "true") {
					return @"true";
				}
			}
			return @"false";
		}

		public object Write(object data) {
			return ((bool)data) ? @"true" : @"false";
		}
	}

	[APacketDataTypeParser("sbyte", "json")]
	public sealed class PacketDataTypeParserSByteJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserSByteJSON();

		private PacketDataTypeParserSByteJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				sbyte result = 0;

				if(sbyte.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((sbyte)data).ToString();
		}
	}

	[APacketDataTypeParser("byte", "json")]
	public sealed class PacketDataTypeParserByteJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserByteJSON();

		private PacketDataTypeParserByteJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				byte result = 0;

				if(byte.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((byte)data).ToString();
		}
	}

	[APacketDataTypeParser("short", "json")]
	public sealed class PacketDataTypeParserShortJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserShortJSON();

		private PacketDataTypeParserShortJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				short result = 0;

				if(short.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((short)data).ToString();
		}
	}

	[APacketDataTypeParser("ushort", "json")]
	public sealed class PacketDataTypeParserUShortJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserUShortJSON();

		private PacketDataTypeParserUShortJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				ushort result = 0;

				if(ushort.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((ushort)data).ToString();
		}
	}

	[APacketDataTypeParser("int", "json")]
	public sealed class PacketDataTypeParserIntJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserIntJSON();

		private PacketDataTypeParserIntJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				int result = 0;

				if(int.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((int)data).ToString();
		}
	}

	[APacketDataTypeParser("uint", "json")]
	public sealed class PacketDataTypeParserUIntJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserUIntJSON();

		private PacketDataTypeParserUIntJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				uint result = 0;

				if(uint.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((uint)data).ToString();
		}
	}

	[APacketDataTypeParser("long", "json")]
	public sealed class PacketDataTypeParserLongJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserLongJSON();

		private PacketDataTypeParserLongJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				long result = 0;

				if(long.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((long)data).ToString();
		}
	}

	[APacketDataTypeParser("ulong", "json")]
	public sealed class PacketDataTypeParserULongJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserULongJSON();

		private PacketDataTypeParserULongJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				ulong result = 0;

				if(ulong.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((ulong)data).ToString();
		}
	}

	[APacketDataTypeParser("float", "json")]
	public sealed class PacketDataTypeParserFloatJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserFloatJSON();

		private PacketDataTypeParserFloatJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				float result = 0;

				if(float.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((float)data).ToString();
		}
	}

	[APacketDataTypeParser("double", "json")]
	public sealed class PacketDataTypeParserDoubleJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserDoubleJSON();

		private PacketDataTypeParserDoubleJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				double result = 0;

				if(double.TryParse((string)data[0], out result)) {
					return result;
				}
			}
			return @"0";
		}

		public object Write(object data) {
			return ((double)data).ToString();
		}
	}

	[APacketDataTypeParser("string", "json")]
	public sealed class PacketDataTypeParserStringJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserStringJSON();

		private PacketDataTypeParserStringJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				return (string)data[0];
			}
			return @"""""";
		}

		public object Write(object data) {
			return @"""" + (string)data + @"""";
		}
	}

	[APacketDataTypeParser("timespan", "json")]
	public sealed class PacketDataTypeParserTimeSpanJSON : IPacketDataTypeParser {
		static private IPacketDataTypeParser instance = new PacketDataTypeParserTimeSpanJSON();

		private PacketDataTypeParserTimeSpanJSON() {
		}

		public object Read(params object[] data) {
			if(data.Length == 1 && data[0] != null) {
				long ticks;

				if(long.TryParse((string)data[0], out ticks)) {
					return new TimeSpan(ticks);
				}
			}
			return null;
		}

		public object Write(object data) {
			return ((TimeSpan)data).Ticks.ToString();
		}
	}
}