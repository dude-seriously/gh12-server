using System.Collections.Generic;
using System.Collections.Concurrent;
using Server.Sockets.Client;

namespace Server.Packets {
    public class Publisher {
        public delegate void Handler(Publisher publisher);

        private ConcurrentDictionary<ulong, SocketClient> subscribers;
        private ulong count;

        public Publisher() {
            this.subscribers = new ConcurrentDictionary<ulong, SocketClient>();
        }

        public virtual bool Subscribe(SocketClient client) {
            client.Stopped += ClientStopped;
            if(this.subscribers.TryAdd(client.ID, client)) {
                ++count;
                return true;
            } else {
                return false;
            }
        }

        private void ClientStopped(SocketClient client) {
            this.Unsubscribe(client);
        }

        public virtual bool Unsubscribe(SocketClient client) {
            if(this.subscribers.TryRemove(client.ID, out client)) {
                --count;
                return true;
            } else {
                return false;
            }
        }

        public void Publish(DataPacket packet) {
            if(packet != null) {
                foreach(SocketClient client in this.subscribers.Values) {
                    client.Send(packet);
                }
            }
        }

        public void Publish(ICollection<DataPacket> packets) {
            if(packets != null) {
                foreach(SocketClient client in this.subscribers.Values) {
                    foreach(DataPacket packet in packets) {
                        client.Send(packet);
                    }
                }
            }
        }

        public void CrossSubscribe(Publisher publisher) {
            foreach(KeyValuePair<ulong, SocketClient> pair in this.subscribers) {
                publisher.Subscribe(pair.Value);
            }
        }

        public void CrossUnsubscribe(Publisher publisher) {
            foreach(KeyValuePair<ulong, SocketClient> pair in this.subscribers) {
                publisher.Unsubscribe(pair.Value);
            }
        }

        public bool HasSubscriber(ulong clientID) {
            return this.subscribers.ContainsKey(clientID);
        }

        public ulong Count {
            get { return this.count; }
        }

        public ICollection<SocketClient> Subscribers {
            get { return this.subscribers.Values; }
        }
    }
}