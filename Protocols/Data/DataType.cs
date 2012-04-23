using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Server.Protocols.Data {
	sealed class PacketDataTypes {
		static private volatile PacketDataTypes instance = new PacketDataTypes();
		private Dictionary<string, PacketDataType> container;

		private PacketDataTypes() {
			this.container = new Dictionary<string, PacketDataType>();

			Assembly assembly = Assembly.GetExecutingAssembly();
			IEnumerable<Type> types = assembly.GetTypes().Where(c => c.GetInterface(typeof(IPacketDataTypeParser).Name) != null && c.GetCustomAttributes(typeof(APacketDataTypeParser), false).Length == 1);
			foreach(Type type in types) {
				APacketDataTypeParser attribute = (APacketDataTypeParser)(type.GetCustomAttributes(typeof(APacketDataTypeParser), false)[0]);

				PacketDataType dataType = null;
				if(!container.ContainsKey(attribute.DataType)) {
					dataType = new PacketDataType(attribute.DataType);
					this.container.Add(attribute.DataType, dataType);
				} else {
					dataType = this.container[attribute.DataType];
				}

				FieldInfo instanceField = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
				if(instanceField != null) {
					dataType.AddParser(attribute.Protocol, (IPacketDataTypeParser)instanceField.GetValue(null));
				}
			}
		}

		static public PacketDataType Get(string name) {
			if(instance.container.ContainsKey(name)) {
				return instance.container[name];
			} else {
				return null;
			}
		}
	}

	public sealed class PacketDataType {
		private string name;
		private Dictionary<string, IPacketDataTypeParser> container;

		public PacketDataType(string name) {
			this.name = name;
			this.container = new Dictionary<string, IPacketDataTypeParser>();
		}

		public void AddParser(string protocol, IPacketDataTypeParser parser) {
			if(!this.container.ContainsKey(protocol)) {
				this.container.Add(protocol, parser);
			} else {

			}
		}

		public IPacketDataTypeParser GetParser(string protocol) {
			if(this.container.ContainsKey(protocol)) {
				return this.container[protocol];
			} else {
				return null;
			}
		}

		public string Name {
			get { return this.name; }
		}
	}

	public interface IPacketDataTypeParser {
		object Read(params object[] data);

		object Write(object data);
	}

	public sealed class APacketDataTypeParser : Attribute {
		private string dataType;
		private string protocol;

		public APacketDataTypeParser(string dataType, string protocol) {
			this.dataType = dataType;
			this.protocol = protocol;
		}

		public string DataType {
			get { return this.dataType; }
		}

		public string Protocol {
			get { return this.protocol; }
		}
	}
}