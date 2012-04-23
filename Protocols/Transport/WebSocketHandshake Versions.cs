using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;

namespace Server.Protocols.Transport {
	[AWSHandshake(0)]
    sealed class WSHandshakeV0:IWSHandshake {
		static private IWSHandshake instance = new WSHandshakeV0();

		private WSHandshakeV0() {
		}

		public HandshakeData Parse(ref byte[] data) {
			HandshakeData info = new HandshakeData();

			try {
				string dataString = Encoding.UTF8.GetString(data);

				StringBuilder response = new StringBuilder("HTTP/1.1 101 Switching Protocols\r\n");

				string upgrade = WSHandhsakeVersions.GetValue(ref dataString, "Upgrade");
				response.Append("Upgrade: ");
				response.Append(upgrade);
				response.Append("\r\n");
				info.Add("Upgrade", upgrade);

				string connection = WSHandhsakeVersions.GetValue(ref dataString, "Connection");
				response.Append("Connection: Upgrade");
				response.Append("\r\n");
				info.Add("Connection", connection);

				string origin = WSHandhsakeVersions.GetValue(ref dataString, "Origin");
				info.Add("Origin", origin);
				response.Append("Sec-WebSocket-Origin: ");
				response.Append(origin);
				response.Append("\r\n");

				info.Add("Host", WSHandhsakeVersions.GetValue(ref dataString, "Host"));

				response.Append("Sec-WebSocket-Location: ");
				response.Append("ws://86.171.240.1:8080/");
				response.Append("\r\n");

				string key1 = WSHandhsakeVersions.GetKeyValue(ref dataString, "Sec-WebSocket-Key1");
				string key2 = WSHandhsakeVersions.GetKeyValue(ref dataString, "Sec-WebSocket-Key2");

				if(!string.IsNullOrEmpty(key1) && !string.IsNullOrEmpty(key2)) {
					string key1Numbers = Regex.Replace(key1, @"[^\d]", "", RegexOptions.Singleline);
					string key2Numbers = Regex.Replace(key2, @"[^\d]", "", RegexOptions.Singleline);

					long key1Number, key2Number;
					if(long.TryParse(key1Numbers, out key1Number)) {
						if(long.TryParse(key2Numbers, out key2Number)) {

							int devide1 = Regex.Matches(key1, @"\s", RegexOptions.Singleline).Count;
							int devide2 = Regex.Matches(key2, @"\s", RegexOptions.Singleline).Count;

							key1Number = key1Number / devide1;
							key2Number = key2Number / devide2;

							MD5 md5 = MD5CryptoServiceProvider.Create();

							byte[] key1Bytes = BitConverter.GetBytes((int)key1Number);
							byte[] key2Bytes = BitConverter.GetBytes((int)key2Number);
							if(BitConverter.IsLittleEndian) {
								Array.Reverse(key1Bytes);
								Array.Reverse(key2Bytes);
							}
							byte[] rest = data.Skip(data.Length - 8).ToArray();

							byte[] resp = key1Bytes.Concat(key2Bytes).Concat(rest).ToArray();

							byte[] hashBytes = md5.ComputeHash(resp);

							response.Append(Encoding.ASCII.GetString(hashBytes));
						}
					}
				}
                
				info.Response = response.ToString();
			} catch(Exception ex) {
				Log.Add("websocket handshake version 8 Parse exception: " + ex.ToString());
			}

			return info;
		}
	}
    
	[AWSHandshake(8, 13)]
	sealed class WSHandshakeV8 : IWSHandshake {
		static private IWSHandshake instance = new WSHandshakeV8();
		static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		private WSHandshakeV8() {
		}

		public HandshakeData Parse(ref byte[] data) {
			HandshakeData info = new HandshakeData();

			try {
				string dataString = Encoding.UTF8.GetString(data);

				StringBuilder response = new StringBuilder("HTTP/1.1 101 Switching Protocols\r\n");

				info.Add("Host", WSHandhsakeVersions.GetValue(ref dataString, "Host"));
				info.Add("User-Agent", WSHandhsakeVersions.GetValue(ref dataString, "User-Agent"));
				info.Add("Sec-WebSocket-Origin", WSHandhsakeVersions.GetValue(ref dataString, "Sec-WebSocket-Origin"));

				string upgrade = WSHandhsakeVersions.GetValue(ref dataString, "Upgrade");
				response.Append("Upgrade: ");
				response.Append("websocket"/*upgrade*/);
				response.Append("\r\n");
				info.Add("Upgrade", "websocket"/*upgrade*/);

				string connection = WSHandhsakeVersions.GetValue(ref dataString, "Connection");
				response.Append("Connection: Upgrade");
				response.Append("\r\n");
				info.Add("Connection", connection);

				string key = WSHandhsakeVersions.GetValue(ref dataString, "Sec-WebSocket-Key");
				response.Append("Sec-WebSocket-Accept: ");
				response.Append(this.AcceptKey(ref key));
				response.Append("\r\n");
				info.Add("Sec-WebSocket-Key", key);
	
				response.Append("\r\n");

				info.Response = response.ToString();
			} catch(Exception ex) {
				Log.Add("websocket handshake version 8 Parse exception: " + ex.ToString());
			}

			return info;
		}

		private string AcceptKey(ref string key) {
			string longKey = key + guid;
			SHA1 sha1 = SHA1CryptoServiceProvider.Create();
			byte[] hashBytes = sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(longKey));
			return Convert.ToBase64String(hashBytes);
		}
	}

	/*[AWSHandshake(13)]
	sealed class WSHandshakeV13 : IWSHandshake {
		static private IWSHandshake instance = new WSHandshakeV13();
		static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		private WSHandshakeV13() { }

        public HandshakeData Parse(ref byte[] data) {
			HandshakeData info = new HandshakeData();

			try {
                string dataString = Encoding.UTF8.GetString(data);

				StringBuilder response = new StringBuilder("HTTP/1.1 101 Switching Protocols\r\n");

                info.Add("Host", WSHandhsakeVersions.GetValue(ref dataString, "Host"));
                info.Add("User-Agent", WSHandhsakeVersions.GetValue(ref dataString, "User-Agent"));
                info.Add("Origin", WSHandhsakeVersions.GetValue(ref dataString, "Origin"));

                string upgrade = WSHandhsakeVersions.GetValue(ref dataString, "Upgrade");
				response.Append("Upgrade: ");
				response.Append(upgrade);
				response.Append("\r\n");
				info.Add("Upgrade", upgrade);

                string connection = WSHandhsakeVersions.GetValue(ref dataString, "Connection");
				response.Append("Connection: Upgrade");
				response.Append("\r\n");
				info.Add("Connection", connection);

                string key = WSHandhsakeVersions.GetValue(ref dataString, "Sec-WebSocket-Key");
				response.Append("Sec-WebSocket-Accept: ");
				response.Append(this.AcceptKey(ref key));
				response.Append("\r\n");
				info.Add("Sec-WebSocket-Key", key);
	
				response.Append("\r\n");
	
				info.Response = response.ToString();
			} catch(Exception ex) {
				Log.Add("websocket handshake version 13 Parse exception: " + ex.ToString());
			}

			return info;
		}

		private string AcceptKey(ref string key) {
			string longKey = key + guid;
			SHA1 sha1 = SHA1CryptoServiceProvider.Create();
			byte[] hashBytes = sha1.ComputeHash(System.Text.Encoding.ASCII.GetBytes(longKey));
			return Convert.ToBase64String(hashBytes);
		}
	}*/
}