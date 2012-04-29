using System;
using System.Net.Sockets;
using Server.Protocols.Transport;
using Server.Packets;
using Server.Game;

namespace Server.Sockets.Client {
    public abstract partial class SocketClient {
        public delegate void Handler(SocketClient client);

        protected readonly object locker = new object();
        protected ulong id;

        public ulong ID { get { return this.id; } }

        protected Socket socket;
        protected byte[] buffer;

        public byte[] Buffer { get { return this.buffer; } }

        protected ITransportProtocol transportProtocol;

        public abstract bool Start();

        public abstract void Stop();

        public abstract event Handler Stopped;

        protected bool stopped;

        public abstract byte[] Receive(int length);
  
        public abstract void SendAsync(DataPacket packet);
        public abstract void Send(DataPacket packet);

        protected bool pong = false;
        protected long pingSent;
        protected short latency;

        public short Latency { get { return this.latency; } }

        public virtual void Ping() {
            if(this.socket == null || !this.socket.Connected || !this.socket.IsConnected()) {
                this.Stop();
            } else {
                if(!this.pong) {
                    this.latency = -1;
                }
                this.pong = false;
                this.pingSent = Time.Now;
    
                try {
                    byte[] binary = this.transportProtocol.Ping();
    
                    SocketError error = SocketError.Fault;
                    int send = 0;
    
                    bool stop = false;
                    lock(this.locker) {
                        if(this.socket != null && this.socket.Connected) {
                            send = this.socket.Send(binary, 0, binary.Length, SocketFlags.None, out error);
                        } else {
                            stop = true;
                        }
                    }
                    if(stop || error != SocketError.Success/* || send != binary.Length*/) {
                        this.Stop();
                    }
                } catch(SocketException exception) {
                    Log.Add("client[" + this.id + "] Ping exception: " + exception.ToString());
                }
            }
        }
    }
}