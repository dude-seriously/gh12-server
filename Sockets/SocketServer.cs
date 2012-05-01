/*
Server.Sockets.Server.SocketServer

Server socket implementation using TCP layer.
It handles Client Accepting and creating operations as well timeout checks.
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Server.Packets;
using Server.Sockets.Client;
using System.Collections.Generic;

namespace Server.Sockets.Server {
    public sealed partial class SocketServer {
        public static SocketServer inst;
        private object locker = new object();
        private Publisher publisher;
        private Socket socket;
        private IPAddress address = IPAddress.Any;
        private int port = 8080;
        
        public SocketServer() {
            Log.Add("server created");
            this.publisher = new Publisher();
            inst = this;
        }
        
        // Start Server
        // It creates Socket, Binds it and Listens
        // As well run Accepting for connections
        public void Start() {
            try {
                lock(locker) {
                    if(this.socket == null) {
                        this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                        
                        this.socket.Bind(new IPEndPoint(this.address, this.port));
                        
                        this.socket.Listen(128);
                        
                        this.socket.BeginAccept(null, 0, OnAccept, null);
                        
                        this.RunCheckTimedOutConnections();
                        
                        Log.Add("server listening");
                    }
                }
            } catch(SocketException exception) {
                Log.Add("server Start exception: " + exception.ToString());
            }
        }
        
        // Event triggers when connection is comming
        // It creates socket, and for now statically creates it as WebSockets client
        // As well it creates User instance in World for GameLoop
        private void OnAccept(IAsyncResult result) {
            try {
                SocketClient client = null;
                lock(this.locker) {
                    if(this.socket != null && this.socket.IsBound) {
                        //Log.Add("server accepting");
                        client = new ClientWS(this.socket.EndAccept(result));
                    }
                }
                if(client != null) {
                    if(client.Start()) {
                        this.publisher.Subscribe(client);
                        
                        client.User = this.instance.Game.World.AddUser("guest" + client.ID);
                        client.Stopped += (obj) => obj.User = null;
                        client.User.Client = client;
                        /*client.Stopped += delegate(SocketClient obj) {
                        obj.User.World.RemoveUser(client.User);
                        };*/
                        /*DataPacket packet = this.StatsPacket;
                        if (packet != null) {
                        client.Send(packet);
                        }*/
                    } else {
                        client.Stop();
                    }
                }
            } catch(SocketException exception) {
                Log.Add("server OnAccept exception: " + exception.ToString());
            } finally {
                lock(this.locker) {
                    if(this.socket != null && this.socket.IsBound) {
                        socket.BeginAccept(null, 0, OnAccept, null);
                    }
                }
            }
        }
        
        // Stop server
        public void Stop() {
            try {
                lock(this.locker) {
                    if(this.socket != null) {
                        this.socket.Close();
                        this.socket.Dispose();
                        this.socket = null;
                        Log.Add("server stopped");
                    }
                }
            } catch(SocketException exception) {
                Log.Add("server Stop exception: " + exception.ToString());
            }
        }
        
        public IPAddress Address {
            set { this.address = value; }
            get { return this.address; }
        }
        
        /*private DataPacket StatsPacket {
            get {
                DataPacket packet = PacketFactory.Make("statsTotal");
                
                if (packet != null) {
                    packet["users"] = this.publisher.Count;
                    packet["rooms"] = ChatRoomManager.Count;
                }
                
                return packet;
            }
        }*/
        
        // Run pinging routine
        // It publishes ping message every second
        private void RunCheckTimedOutConnections() {
            ThreadPool.QueueUserWorkItem(new WaitCallback(WorkerTimedOut));
        }

        private void WorkerTimedOut(object data) {
            while(true) {
                long started = Time.Now;
                foreach(SocketClient client in this.publisher.Subscribers) {
                    client.Ping();
                }
                long elapsed = Time.Now - started;
                if(elapsed < 1000) {
                    Thread.Sleep(1000 - (int)elapsed);
                }
            }
        }
        
        // Asynchronous publishing of packet using Thread from Pool
        public void PublishAsync(DataPacket packets) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(PublishCallback), packets);
        }
        
        // Asynchronous publishing list of packets using Thread from Pool
        public void PublishAsync(ICollection<DataPacket> packets) {
            ThreadPool.QueueUserWorkItem(new WaitCallback(PublishManyCallback), packets);
        }
        
        // Publish callback
        private void PublishCallback(object data) {
            this.publisher.Publish((DataPacket)data);
        }
        
        // Publish many callback
        private void PublishManyCallback(object data) {
            this.publisher.Publish((ICollection<DataPacket>)data);
        }
        
        // Blocking publishing of packet
        public void Publish(DataPacket packet) {
            this.publisher.Publish(packet);
        }
        
        // Blocking publishing list of packets
        public void Publish(ICollection<DataPacket> packets) {
            this.publisher.Publish(packets);
        }
        /*private void RunPublishStats() {
            ThreadPool.QueueUserWorkItem(new WaitCallback(WorkerStats));
        }
        
        private void WorkerStats(object data) {
            DataPacket packet = this.StatsPacket;
            
            if (packet != null) {
                this.publisher.Publish(packet);
            }
            
            Thread.Sleep(5000);
            this.RunPublishStats();
        }*/
    }
}