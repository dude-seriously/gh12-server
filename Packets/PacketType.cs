using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Server.Protocols.Data;
using Server.Sockets.Client;

namespace Server.Packets {
    sealed class PacketFactory {
        static private PacketFactory instance = new PacketFactory();
        private Dictionary<string, PacketType> container;

        private PacketFactory() {
            this.container = new Dictionary<string, PacketType>();

            Assembly assembly = Assembly.GetExecutingAssembly();

            IEnumerable<Type> types = assembly.GetTypes().Where(c => c.IsSubclassOf(typeof(PacketType)) && c.GetCustomAttributes(typeof(APacket), false).Length == 1).ToArray();
            foreach(Type type in types) {
                APacket attribute = (APacket)(type.GetCustomAttributes(typeof(APacket), false)[0]);
                if(!container.ContainsKey(attribute.Name)) {
                    FieldInfo instanceField = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
                    if(instanceField != null) {
                        PacketType instancePacket = (PacketType)instanceField.GetValue(null);

                        container.Add(attribute.Name, instancePacket);
                    }
                }
            }

            foreach(KeyValuePair<string, PacketType> pair in this.container) {
                pair.Value.Name = pair.Key;
                pair.Value.AssignEvents();
            }
        }

        static public PacketType Get(string name) {
            if(instance.container.ContainsKey(name)) {
                return instance.container[name];
            } else {
                return null;
            }
        }

        static public DataPacket Make(string name) {
            PacketType packetType = Get(name);
            if(packetType != null) {
                return new DataPacket(packetType);
            } else {
                return null;
            }
        }

        static public DataPacket Make(string name, params object[] list) {
            PacketType packetType = Get(name);
            if(packetType != null) {
                DataPacket packet = new DataPacket(packetType);
                if(list != null && list.Length > 0) {
                    for(int i = 0; i < list.Length / 2; ++i) {
                        packet[(string)list[i * 2]] = list[i * 2 + 1];
                    }
                }
                return packet;
            } else {
                return null;
            }
        }
    }

    sealed class APacket : Attribute {
        private string name;

        public APacket(string name) {
            this.name = name;
        }

        public string Name {
            get { return this.name; }
        }
    }

    public class PacketType {
        private string name;
        private Dictionary<string, PacketField> fields;
        private List<IPacketEvent> events;

        public PacketType() {
            this.fields = new Dictionary<string, PacketField>();
        }

        protected void AddField(string name, string dataType) {
            PacketField field = new PacketField(name, PacketDataTypes.Get(dataType));
            this.fields.Add(field.Name, field);
        }

        protected void AddField(string name, string dataType, object defaultValue) {
            PacketField field = new PacketField(name, PacketDataTypes.Get(dataType), defaultValue);
            this.fields.Add(field.Name, field);
        }

        public PacketField GetField(string name) {
            if(this.fields.ContainsKey(name)) {
                return this.fields[name];
            } else {
                return null;
            }
        }

        public IEnumerable<PacketField> Fields {
            get { return this.fields.Values; }
        }

        public void AssignEvents() {
            if(this.events == null) {
                this.events = new List<IPacketEvent>();
    
                Assembly assembly = Assembly.GetExecutingAssembly();
    
                IEnumerable<Type> types = assembly.GetTypes().Where(c => c.GetInterface(typeof(IPacketEvent).Name) != null && c.GetCustomAttributes(typeof(APacketEvent), false).Length == 1 && ((APacketEvent)c.GetCustomAttributes(typeof(APacketEvent), false)[0]).Name == this.name).ToArray();
                foreach(Type type in types) {
                    APacketEvent attribute = (APacketEvent)(type.GetCustomAttributes(typeof(APacketEvent), false)[0]);
                    FieldInfo instanceField = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
                    if(instanceField != null) {
                        this.events.Add((IPacketEvent)instanceField.GetValue(null));
                    }
                }
            }
        }

        public void Trigger(SocketClient client, DataPacket packet) {
            foreach(IPacketEvent evnt in this.events) {
                evnt.Process(client, packet);
            }
        }

        public string Name {
            set {
                if(string.IsNullOrEmpty(this.name)) {
                    this.name = value;
                }
            }
            get { return this.name; }
        }
    }

    public sealed class PacketField {
        private string name;
        private PacketDataType dataType;
        private object defaultValue;

        public PacketField(string name, PacketDataType dataType) {
            this.name = name;
            this.dataType = dataType;
        }

        public PacketField(string name, PacketDataType dataType, object defaultValue) {
            this.name = name;
            this.dataType = dataType;
            this.defaultValue = defaultValue;
        }

        public string Name {
            get { return this.name; }
        }

        public PacketDataType DataType {
            get { return this.dataType; }
        }

        public object Default {
            get { return this.defaultValue; }
        }
    }

    public sealed class APacketEvent : Attribute {
        private string name;

        public APacketEvent(string name) {
            this.name = name;
        }

        public string Name {
            get { return this.name; }
        }
    }

    public interface IPacketEvent {
        void Process(SocketClient client, DataPacket packet);
    }
}