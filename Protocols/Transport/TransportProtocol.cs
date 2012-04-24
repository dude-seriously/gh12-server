using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Server.Sockets.Client;

namespace Server.Protocols.Transport {
    sealed class TransportProtocol {
        static private volatile TransportProtocol instance = new TransportProtocol();
        private Dictionary<string, TransportProtocolContainer> container;

        private TransportProtocol() {
            this.container = new Dictionary<string, TransportProtocolContainer>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = assembly.GetTypes().Where(c => c.GetInterface(typeof(ITransportProtocol).Name) != null && c.GetCustomAttributes(typeof(ATransportProtocol), false).Length == 1);
            foreach(Type type in types) {
                ATransportProtocol attribute = (ATransportProtocol)(type.GetCustomAttributes(typeof(ATransportProtocol), false)[0]);
                FieldInfo instanceField = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
                if(instanceField != null) {
                    TransportProtocolContainer protocolContainer = null;
                    if(!this.container.ContainsKey(attribute.Name)) {
                        protocolContainer = new TransportProtocolContainer();
                        this.container.Add(attribute.Name, protocolContainer);
                    } else {
                        protocolContainer = this.container[attribute.Name];
                    }
                    ITransportProtocol protocol = (ITransportProtocol)instanceField.GetValue(null);
                    foreach(byte version in attribute.Versions) {
                        //Log.Add("ver: " + version);
                        protocolContainer.Add(version, protocol);
                    }
                }
            }
        }

        static public ITransportProtocol Get(string name, uint version) {
            if(instance.container.ContainsKey(name)) {
                return instance.container[name].Get(version);
            } else {
                return null;
            }
        }
    }

    sealed class ATransportProtocol : Attribute {
        private string name;
        private uint[] versions;

        public ATransportProtocol(string name, uint version) {
            this.name = name;
            this.versions = new uint[] { version };
        }

        public ATransportProtocol(string name, params uint[] versions) {
            this.name = name;
            this.versions = versions;
        }

        public string Name {
            get { return this.name; }
        }

        public uint[] Versions {
            get { return this.versions; }
        }
    }

    public sealed class TransportProtocolContainer {
        private Dictionary<uint, ITransportProtocol> container;

        public TransportProtocolContainer() {
            this.container = new Dictionary<uint, ITransportProtocol>();
        }

        public void Add(byte version, ITransportProtocol protocol) {
            this.container.Add(version, protocol);
        }

        public ITransportProtocol Get(uint version) {
            if(this.container.ContainsKey(version)) {
                return this.container[version];
            } else {
                return null;
            }
        }
    }

    public interface ITransportProtocol {
        IPacket Read(SocketClient socket);

        byte[] Send(byte[] data);

        byte[] Ping();
    }

    public interface IPacket {
        byte[] Data { get; }
    }
}