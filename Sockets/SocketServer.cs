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

		public void Start() {
			try {
				lock(locker) {
					if(this.socket == null) {
						this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

						this.socket.Bind(new IPEndPoint(this.address, this.port));

						this.socket.Listen(128);

						this.socket.BeginAccept(null, 0, OnAccept, null);

						this.RunCheckTimedOutConnections();
						//this.RunPublishStats();

						Log.Add("server listening");
					}
				}
			} catch(SocketException exception) {
				Log.Add("server Start exception: " + exception.ToString());
			}
		}

		private void OnAccept(IAsyncResult result) {
			try {
				SocketClient client = null;
				lock(this.locker) {
					if(this.socket != null && this.socket.IsBound) {
						Log.Add("server accepting");
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

		public void PublishAsync(DataPacket packets) {
			ThreadPool.QueueUserWorkItem(new WaitCallback(PublishCallback), packets);
		}

		public void PublishAsync(ICollection<DataPacket> packets) {
			ThreadPool.QueueUserWorkItem(new WaitCallback(PublishManyCallback), packets);
		}

		private void PublishCallback(object data) {
			this.publisher.Publish((DataPacket)data);
		}

		private void PublishManyCallback(object data) {
			this.publisher.Publish((ICollection<DataPacket>)data);
		}

		public void Publish(DataPacket packet) {
			this.publisher.Publish(packet);
		}

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