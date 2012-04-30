/*
Server.Sockets.Client.ClientWS : SocketClient

Implementation of socket to work with WebSockets protocol.
This is TCP layer socket implementation with WebSockets layer on top.
This class handles Handshake operation using TransportProtocol based on clients protocol version.
As well it handles WebSockets messages framing operations based on DataProtocol. For now data
protocol is used statically JSON, but Data Protocol layer is implemented that way, that client
or server will be able to define which Data Protocol it prefers to use and it will be applied
accordingly.
*/

using System;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Protocols.Transport;
using Server.Packets;
using Server.Protocols.Data;

namespace Server.Sockets.Client {
    sealed class ClientWS : SocketClient {
        static private ulong counter = 0;

        // Constructor
        public ClientWS(Socket socket) {
            this.id = ++counter;

            this.socket = socket;
            this.socket.NoDelay = true;
            this.socket.ReceiveTimeout = 1024;

            Log.Add("client[" + this.id + "] connected (" + this.socket.RemoteEndPoint.ToString() + ")");
        }

        // Start Client
        // Tryes to go through Handshake, if success
        // sends welcome message and start receiving messages.
        public override bool Start() {
            bool success = this.Handshake();
            if(success) {
                this.SendAsync(PacketFactory.Make("welcome", "message", "Welcome to GameHack!"));

                this.buffer = new byte[2];
                this.StartReceive();

                return true;
            } else {
                Log.Add("client[" + this.id + "] handshake failed");
                return false;
            }
        }

        // Stop Client
        public override void Stop() {
            if(!this.stopped) {
                this.stopped = true;

                lock(this.locker) {
                    if(this.socket != null) {
                        if(this.socket.Connected) {
                            this.socket.Shutdown(SocketShutdown.Both);
                        }
                        this.socket.BeginDisconnect(false, OnDisconnect, null);
                    } else {
                        this.OnStop();
                    }
                }
            }
        }

        // Event handler that triggers when client is stopped
        public override event Handler Stopped;

        private void OnStop() {
            if(Stopped != null) {
                Stopped(this);
                Stopped = null;
            }
        }

        private void OnDisconnect(IAsyncResult result) {
            lock(this.locker) {
                this.OnStop();

                if(this.socket != null) {
                    this.socket.EndDisconnect(result);
                    this.socket.Close();
                    this.socket.Dispose();
                    this.socket = null;

                    Log.Add("client[" + this.id + "] disconnected");
                }
            }
        }

        // Operation method to handle handshake process
        // It uses Handshake Parser to get protocol version
        // and use it to generate handshake response message
        private bool Handshake() {
            bool result = false;

            try {
                SocketError error = SocketError.Fault;

                int read = 0;
                byte[] buffer = new byte[1024];

                lock(this.locker) {
                    if(this.socket != null && this.socket.Connected) {
                        read = this.socket.Receive(buffer, 0, 1024, SocketFlags.None, out error);
                    }
                }

                if(error == SocketError.Success) {
                    WSHandshakeParser handshake = new WSHandshakeParser(buffer.Take(read).ToArray());

                    if(handshake.Info != null && !string.IsNullOrEmpty(handshake.Info.Response)) {
                        byte[] responseBytes = Encoding.UTF8.GetBytes(handshake.Info.Response);

                        error = SocketError.Fault;

                        bool stop = false;
                        lock(this.locker) {
                            if(this.socket != null && this.socket.Connected) {
                                this.socket.Send(responseBytes, 0, responseBytes.Length, SocketFlags.None, out error);
                                result = true;
                            } else {
                                stop = true;
                            }
                        }
                        if(stop) {
                            this.Stop();
                        }

                        if(result) {
                            this.transportProtocol = TransportProtocol.Get("websocket", handshake.Version);
                            if(this.transportProtocol == null) {
                                result = false;
                                Log.Add("client[" + this.id + "] Handshake error: unsuported version");
                            }
                        }
                    }
                } else {
                    Log.Add("client[" + this.id + "] Handshake error: " + error.ToString());
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] Handshake exception: " + exception.ToString());
                result = false;
            }

            return result;
        }

        // Start asynchronous receiving binary data
        private void StartReceive() {
            Log.Add("client[" + this.id + "] started");
            try {
                SocketError error = SocketError.Fault;

                bool stop = false;
                lock(this.locker) {
                    if(this.socket != null && this.socket.Connected) {
                        this.socket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, out error, OnReceive, null);
                    } else {
                        stop = true;
                    }
                }
                if(stop) {
                    this.Stop();
                }

                if(error != SocketError.Success) {
                    Log.Add("client[" + this.id + "] StartReceive error: " + error.ToString());
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] StartReceive exception: " + exception.ToString());
            }
        }

        // Force blocking receive of specific amount of bytes
        public override byte[] Receive(int length) {
            try {
                SocketError error = SocketError.Fault;
                int read = 0;
                byte[] buffer = new byte[length];

                bool stop = false;
                lock(this.locker) {
                    if(this.socket != null && this.socket.Connected) {
                        read = this.socket.Receive(buffer, 0, length, SocketFlags.None, out error);
                    } else {
                        stop = true;
                    }
                }

                if(!stop && error == SocketError.Success /*&& read == length*/) {
                    return buffer;
                } else {
                    this.Stop();
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] Receive exception: " + exception.ToString());
            }
            return null;
        }

        // When data received parse data using extra transport protocol
        // in this case it is always WebSockets so it handles rest of
        // functionality like pinging and closing packet
        private void OnReceive(IAsyncResult result) {
            IPacket packet = null;

            try {
                bool stop = false;

                lock(this.locker) {
                    SocketError error = SocketError.Fault;
                    int read = 0;

                    if(this.socket != null && this.socket.Connected) {
                        read = this.socket.EndReceive(result, out error);
                    } else {
                        stop = true;
                    }

                    if(error == SocketError.Success && read == this.buffer.Length) {
                        packet = this.transportProtocol.Read(this);

                        if(packet != null) {
                            switch(((WSPacket)packet).OpCode) {
                                case OpCode.Text:
                                    this.ProcessPacket(packet);
                                    break;
                                case OpCode.Pong:
                                    this.pong = true;
                                    this.latency = (short)(Time.Now - this.pingSent);
                                    if(this.user != null) {
                                        this.user.Latency = this.latency;
                                    }
                                    break;
                                case OpCode.Close:
                                    Log.Add("client[" + this.id + "] closing code: " + ((WSPacket)packet).CloseCode.ToString());
                                    stop = true;
                                    break;
                                default:
                                    stop = true;
                                    break;
                            }
                        }
                    } else {
                        stop = true;
                    }
                }
                if(stop) {
                    this.Stop();
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] OnReceive exception: " + exception.ToString());
            } finally {
                if(!stopped) {
                    SocketError error = SocketError.Fault;

                    bool stop = false;
                    lock(this.locker) {
                        if(this.socket != null && this.socket.Connected) {
                            this.socket.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, out error, OnReceive, null);
                        } else {
                            stop = true;
                        }
                    }
                    if(stop) {
                        this.Stop();
                    }

                    if(error != SocketError.Success) {
                        Log.Add("client[" + this.id + "] OnReceive error: " + error.ToString());
                    }
                }
            }
        }

        // Process packet will deserialize data from binary to DataPacket
        // based on selected Data Protocol
        // And will trigger subscribed event methods to process packets logic
        private void ProcessPacket(IPacket packet) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Worker), packet);
        }

        private void Worker(object packet) {
            DataPacket dataPacket = DataProtocol.Get("json").Read(((IPacket)packet).Data);
            if(dataPacket != null) {
                dataPacket.TriggerEvents(this);
            }
        }
        
        // Send data using thread from pool
        public override void SendAsync(DataPacket packet) {
            if (packet != null) {
                ThreadPool.QueueUserWorkItem(new WaitCallback(SendAsync), packet);
            }
        }
        
        // Callback for sending in thread from pool
        private void SendAsync(object data) {
            if (data != null) {
                this.Send((DataPacket)data);
            }
        }
        
        // Blocking sending of data
        // It serializes packet into binary based on selected Data Protocol
        public override void Send(DataPacket packet) {
            try {
                if (packet != null) {
                    byte[] data = DataProtocol.Get("json").Write(packet);
    
                    byte[] binary = this.transportProtocol.Send(data);
    
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
                    if(stop || error != SocketError.Success /*|| send != binary.Length*/) {
                        this.Stop();
                    }
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] Send exception: " + exception.ToString());
            }
        }
    }
}