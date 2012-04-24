using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Server.Protocols.Transport {
    sealed class WSHandshakeParser {
        private string data;
        private uint version;
        private HandshakeData info;

        public WSHandshakeParser(byte[] buffer) {
            this.version = WSHandhsakeVersions.GetVersion(ref buffer);

            IWSHandshake parser = WSHandhsakeVersions.Get(this.version);
            if(parser != null) {
                this.info = parser.Parse(ref buffer);
            } else {
                Log.Add("handshake data: " + this.data);
            }
        }

        public uint Version {
            get { return this.version; }
        }

        public HandshakeData Info {
            get { return this.info; }
        }
    }

    sealed class WSHandhsakeVersions {
        static private volatile WSHandhsakeVersions instance = new WSHandhsakeVersions();
        private Dictionary<uint, IWSHandshake> container;

        private WSHandhsakeVersions() {
            this.container = new Dictionary<uint, IWSHandshake>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = assembly.GetTypes().Where(c => c.GetInterface(typeof(IWSHandshake).Name) != null && c.GetCustomAttributes(typeof(AWSHandshake), false).Length == 1);
            foreach(Type type in types) {
                AWSHandshake attribute = (AWSHandshake)(type.GetCustomAttributes(typeof(AWSHandshake), false)[0]);
                foreach(uint version in attribute.Versions) {
                    FieldInfo instanceField = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
                    if(instanceField != null) {
                        container.Add(version, (IWSHandshake)instanceField.GetValue(null));
                    }
                }
            }
        }

        static public uint GetVersion(ref byte[] data) {
            string dataString = Encoding.UTF8.GetString(data);
            Match result = Regex.Match(dataString, @"^Sec-WebSocket-Version: (?<version>\d+)\s?$", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            if(result.Success && result.Groups["version"] != null) {
                uint version = 0;
                uint.TryParse(result.Groups["version"].Value, out version);

                return version;
            } else {
                return 0;
            }
        }

        static public string GetValue(ref string data, string name) {
            Match result = Regex.Match(data, @"^" + name + @": (?<value>[\da-zA-Z :/\-+=*();.,_'<]+)\s?$", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            if(result.Success && result.Groups["value"] != null) {
                return result.Groups["value"].Value;
            } else {
                return string.Empty;
            }
        }

        static public string GetKeyValue(ref string data, string name) {
            Match result = Regex.Match(data, @"^" + name + @": (?<value>.+)\r$", RegexOptions.ExplicitCapture | RegexOptions.Multiline);

            if(result.Success && result.Groups["value"] != null) {
                return result.Groups["value"].Value;
            } else {
                return string.Empty;
            }
        }

        static public IWSHandshake Get(uint version) {
            if(instance.container.ContainsKey(version)) {
                return instance.container[version];
            } else {
                return null;
            }
        }
    }

    sealed class AWSHandshake : Attribute {
        private uint[] versions;

        public AWSHandshake(uint version) {
            this.versions = new uint[] { version };
        }

        public AWSHandshake(params uint[] versions) {
            this.versions = versions;
        }

        public uint[] Versions {
            get { return this.versions; }
        }
    }

    interface IWSHandshake {
        HandshakeData Parse(ref byte[] data);
    }

    sealed class HandshakeData {
        private Dictionary<string, string> container;
        private string response;

        public HandshakeData() {
            this.container = new Dictionary<string, string>();
        }

        public void Add(string name, string value) {
            this.container.Add(name, value);
        }

        public string Get(string name) {
            if(this.container.ContainsKey(name)) {
                return this.container[name];
            } else {
                return string.Empty;
            }
        }

        public string Response {
            set { this.response = value; }
            get { return this.response; }
        }
    }
}