using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Server.Packets;

namespace Server.Protocols.Data {
    sealed class DataProtocol {
        static private volatile DataProtocol instance = new DataProtocol();
        private Dictionary<string, IDataProtocol> container;

        private DataProtocol() {
            this.container = new Dictionary<string, IDataProtocol>();

            Assembly assembly = Assembly.GetExecutingAssembly();
            IEnumerable<Type> types = assembly.GetTypes().Where(c => c.GetInterface(typeof(IDataProtocol).Name) != null && c.GetCustomAttributes(typeof(ADataProtocol), false).Length == 1);
            foreach(Type type in types) {
                ADataProtocol attribute = (ADataProtocol)(type.GetCustomAttributes(typeof(ADataProtocol), false)[0]);
                if(!container.ContainsKey(attribute.Name)) {
                    FieldInfo instanceField = type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
                    if(instanceField != null) {
                        container.Add(attribute.Name, (IDataProtocol)instanceField.GetValue(null));
                    }
                }
            }

            foreach(KeyValuePair<string, IDataProtocol> pair in this.container) {
                pair.Value.Name = pair.Key;
            }
        }

        static public IDataProtocol Get(string name) {
            if(instance.container.ContainsKey(name)) {
                return instance.container[name];
            } else {
                return null;
            }
        }
    }

    sealed class ADataProtocol : Attribute {
        private string name;

        public ADataProtocol(string name) {
            this.name = name;
        }

        public string Name {
            get { return this.name; }
        }
    }

    interface IDataProtocol {
        DataPacket Read(byte[] data);

        byte[] Write(DataPacket data);

        string Name { set; }
    }
}