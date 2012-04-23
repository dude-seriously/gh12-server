using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Server.Packets;

namespace Server.Protocols.Data {
	[ADataProtocol("json")]
	sealed class DataProtocolJSON : IDataProtocol {
		static private IDataProtocol instance = new DataProtocolJSON();
		private string name;

		private DataProtocolJSON() {
		}

		public DataPacket Read(byte[] data) {
			string dataJSON = Encoding.UTF8.GetString(data).Replace(@"\""", "&quot;").Replace(@"<", "&lt;").Replace(@"<", "&gt;");

			string packetName = this.GetName(dataJSON);

			if(!string.IsNullOrEmpty(packetName)) {
				PacketType packet = PacketFactory.Get(packetName);

				if(packet != null) {
					DataPacket dataPacket = new DataPacket(packet);

					foreach(PacketField field in packet.Fields) {
						string value = string.Empty;
						if(field.DataType != null) {
							IPacketDataTypeParser parser = field.DataType.GetParser(this.name);
							if(parser != null) {
								switch(field.DataType.Name) {
									case "bool":
										value = this.GetValueBool(dataJSON, field.Name);
										break;
									case "byte":
									case "sbyte":
									case "short":
									case "ushort":
									case "int":
									case "uint":
									case "long":
									case "ulong":
									case "float":
									case "double":
									case "timespan":
										value = this.GetValueNumber(dataJSON, field.Name);
										break;
									case "string":
										value = this.GetValueString(dataJSON, field.Name);
										break;
									default:
										Log.Add("unknown data type: " + field.DataType.Name);
										break;
								}
								if(!string.IsNullOrEmpty(value)) {
									dataPacket[field] = field.DataType.GetParser(this.name).Read(value);
								}
							} else {
								Log.Add("no data type (" + field.DataType.Name + ") parser for " + this.name + " protocol");
							}
						} else {
							Log.Add("unknown data type, field: " + field.Name);
						}
					}
					return dataPacket;
				} else {
					Log.Add("unknown packet: " + packetName);
					return null;
				}
			} else {
				Log.Add("wrong packet (no type)");
				return null;
			}
		}

		private string GetName(string dataJSON) {
			Match result = Regex.Match(dataJSON, @"^{\s?""type""\s?:\s?""(?<value>[a-zA-Z_]+)""\s?", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

			if(result.Success && result.Groups["value"] != null) {
				return result.Groups["value"].Value;
			} else {
				return string.Empty;
			}
		}

		private string GetValueBool(string dataJSON, string fieldName) {
			Match result = Regex.Match(dataJSON, @"""" + fieldName + @"""\s?:\s?(?<value>(true|false))", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

			if(result.Success && result.Groups["value"] != null) {
				return result.Groups["value"].Value;
			} else {
				return string.Empty;
			}
		}

		private string GetValueNumber(string dataJSON, string fieldName) {
			Match result = Regex.Match(dataJSON, @"""" + fieldName + @"""\s?:\s?(?<value>(-|\+)?[\d.]+(e\+[\d.]+)?|null)?", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

			if(result.Success && result.Groups["value"] != null) {
				return result.Groups["value"].Value;
			} else {
				return string.Empty;
			}
		}

		private string GetValueString(string dataJSON, string fieldName) {
			Match result = Regex.Match(dataJSON, @"""" + fieldName + @"""\s?:\s?""(?<value>[^""]*)?""", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

			if(result.Success && result.Groups["value"] != null) {
				return result.Groups["value"].Value;
			} else {
				return string.Empty;
			}
		}

		public byte[] Write(DataPacket data) {
			StringBuilder dataJSON = new StringBuilder(@"{""type"":""");

			dataJSON.Append(data.Name);

			dataJSON.Append(@"""");

			foreach(KeyValuePair<PacketField, FieldData> pair in data.Enumerator) {
				if(pair.Key.DataType != null) {
					IPacketDataTypeParser parser = pair.Key.DataType.GetParser(this.name);
					if(parser != null) {
						if(pair.Value.Value != null) {
							dataJSON.Append(@",""");
							dataJSON.Append(pair.Key.Name);
							dataJSON.Append(@""":");
							dataJSON.Append(parser.Write(pair.Value.Value));
						}
					} else {
						Log.Add("no data type (" + pair.Key.DataType.Name + ") parser for " + this.name + " protocol");
					}
				} else {
					Log.Add("unknown data type, field " + pair.Key.Name);
				}
			}

			dataJSON.Append(@"}");

			return Encoding.UTF8.GetBytes(dataJSON.ToString());
		}

		public string Name {
			set {
				if(string.IsNullOrEmpty(this.name)) {
					this.name = value;
				}
			}
		}
	}
}