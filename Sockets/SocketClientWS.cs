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

        public ClientWS(Socket socket) {
            this.id = ++counter;

            this.socket = socket;
            this.socket.NoDelay = true;
            this.socket.ReceiveTimeout = 1024;

            Log.Add("client[" + this.id + "] connected (" + this.socket.RemoteEndPoint.ToString() + ")");
        }

        public override bool Start() {
            bool success = this.Handshake();
            if(success) {
                DataPacket packet = PacketFactory.Make("launcherWelcome");
                if(packet != null) {
                    packet["message"] = "Welcome to MOLYJAM!";
                    this.Send(packet);
                }

                this.buffer = new byte[2];
                this.StartReceive();

                return true;
            } else {
                Log.Add("client[" + this.id + "] handshake failed");
                return false;
            }
        }

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

                if(!stop && error == SocketError.Success && read == length) {
                    return buffer;
                } else {
                    this.Stop();
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] Receive exception: " + exception.ToString());
            }
            return null;
        }

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

        private void ProcessPacket(IPacket packet) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(Worker), packet);
        }

        private void Worker(object packet) {
            DataPacket dataPacket = DataProtocol.Get("json").Read(((IPacket)packet).Data);
            if(dataPacket != null) {
                dataPacket.TriggerEvents(this);
            }
        }

        public override void Send(DataPacket packet) {
            try {
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
                if(stop || error != SocketError.Success || send != binary.Length) {
                    this.Stop();
                }
            } catch(SocketException exception) {
                Log.Add("client[" + this.id + "] Send exception: " + exception.ToString());
            }
        }
    }
}