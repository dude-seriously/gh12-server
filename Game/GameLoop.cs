using System;
using System.Threading;
using System.Collections.Generic;
using Server.Game.Input;
using Server.Game.Objects;
using Server.Packets;
using System.Diagnostics;

namespace Server.Game {
	public class GameLoop {
		public delegate void HandlerPackets(ICollection<DataPacket> packets);

		private volatile bool running;
		private Thread thread;
		private GameWorld world;

		public GameLoop() {

		}

		public GameWorld World {
			set {
				if(!this.running) {
					this.world = value;
				}
			}
			get { return this.world; }
		}

		public void Start() {
			if(!this.running) {
				this.running = true;

				if(this.thread == null) {
					this.thread = new Thread(this.Loop);
				}
				this.thread.Start();
			}
		}

		public void Stop() {
			if(this.running == true) {
				this.running = false;
			}
		}

		public void Loop() {
			int iteration = 0;

			//Stopwatch watch = new Stopwatch();
			//watch.Start();

			long lastTick = Time.Now;

			while(this.running) {
				if(Time.Now - lastTick > 30) {
					lastTick = Time.Now;
					//Log.Add("c " + iteration);
					GameObject.Time = lastTick;
	
					ICollection<User> users = this.world.Users;
					ICollection<GameObject> objects = this.world.Objects;
	
					foreach(User user in users) {
						UserInput input = null;

						if(user.States["removing"] != null && user.States["removing"].Equals(true)) {
							{
								DataPacket packet = PacketFactory.Make("characterRemove");
								if(packet != null) {
									packet["id"] = user.Char.ID;
									this.world.AddPacket(packet);
								}
							}

							{
								DataPacket packet = PacketFactory.Make("userRemove");
								if(packet != null) {
									packet["id"] = user.ID;
									this.world.AddPacket(packet);
								}
							}
	
							user.Remove();
	
						} else {
							if(user.States["new"] != null && user.States["new"].Equals(true)) {
								user.States["new"] = null;

								{
									DataPacket packet = PacketFactory.Make("userAdd");
									if(packet != null) {
										packet["id"] = user.ID;
										packet["n"] = user.Name;
										packet["k"] = user.Score;
										packet["d"] = user.Deaths;
										packet["l"] = user.Latency;
										this.world.AddPacket(packet);
									}
								}

								user.Char.X = (float)Rand.Next(-320, 320);
								user.Char.Y = (float)Rand.Next(-240, 240);

								if(GameObject.Time - user.lastEmotion > User.emotionDelay) {
									user.lastEmotion = GameObject.Time;
									{
										if(Rand.Next(0, 2) == 0) {
											DataPacket packet = PacketFactory.Make("emotionAdd");
											if(packet != null) {
												packet["id"] = user.Char.ID;
												packet["t"] = 1;
												this.world.AddPacket(packet);
											}
										}
									}
								}

								foreach(User iUser in users) {
									if(iUser.ID != user.ID) {
										{
											DataPacket packet = PacketFactory.Make("userAdd");
											if(packet != null) {
												packet["id"] = iUser.ID;
												packet["k"] = iUser.Score;
												packet["d"] = iUser.Deaths;
												packet["n"] = iUser.Name;
												packet["l"] = iUser.Latency;
												if(user.Client != null) {
													user.Client.Send(packet);
												}
											}
										}
										{
											DataPacket packet = PacketFactory.Make("characterAdd");
											if(packet != null) {
												packet["id"] = iUser.Char.ID;
												packet["size"] = iUser.Char.Size;
												packet["x"] = iUser.Char.X;
												packet["y"] = iUser.Char.Y;
												if(user.Client != null) {
													user.Client.Send(packet);
												}
											}
										}
										{
											DataPacket packet = PacketFactory.Make("characterSet");
											if(packet != null) {
												packet["id"] = iUser.Char.ID;
												packet["userId"] = iUser.ID;
												if(user.Client != null) {
													user.Client.Send(packet);
												}
											}
										}
									}
								}
	
								this.world.AddObject(user.Char);
		
								{
									DataPacket packet = PacketFactory.Make("characterAdd");
									if(packet != null) {
										packet["id"] = user.Char.ID;
										packet["size"] = user.Char.Size;
										packet["x"] = user.Char.X;
										packet["y"] = user.Char.Y;
										this.world.AddPacket(packet);
									}
								}
	
								{
									DataPacket packet = PacketFactory.Make("characterSet");
									if(packet != null) {
										packet["id"] = user.Char.ID;
										packet["userId"] = user.ID;
										this.world.AddPacket(packet);
									}
								}
							}
	
							int limit = 10;
							while((input = user.GrabInput()) != null && limit > 0) {
								--limit;
								switch(input.Name) {
									case "shot":
										if(!user.Char.Zombie) {
											if(GameObject.Time - user.lastShoot > User.shootDelay) {
												user.lastShoot = GameObject.Time;

												float nX, nY;
												if(float.TryParse(input["nX"].ToString(), out nX)) {
													if(float.TryParse(input["nY"].ToString(), out nY)) {
														Bullet bullet = new Bullet(user, user.Char.X, user.Char.Y, nX, nY);
														BulletContainer.Add(bullet);
		
														{
															DataPacket packet = PacketFactory.Make("bulletAdd");
															if(packet != null) {
																packet["id"] = bullet.ID;
																packet["x"] = bullet.x;
																packet["y"] = bullet.y;
																packet["nX"] = bullet.nX;
																packet["nY"] = bullet.nY;
																packet["life"] = bullet.Life;
																packet["speed"] = bullet.speed;
																this.world.AddPacket(packet);
															}
														}
													}
												}
											}
										} else {
											if(GameObject.Time - user.lastShoot > User.shootDelay / 2) {
												user.lastShoot = GameObject.Time;

												foreach(GameObject obj in objects) {
													if(obj.type == 1) {
														Character character = (Character)obj;

														if(character.ID != user.Char.ID) {
															float x = character.x - user.Char.X;
															float y = character.y - user.Char.Y;
															if(Math.Sqrt(x * x + y * y) < (character.Size) + 6) {
																if(user.Char.Health > 50) {
																	character.Health -= 20;
																	user.Char.Health += 10;
																	user.Score += 2;
																} else if(user.Char.Health > 30) {
																		character.Health -= 50;
																		user.Char.Health += 20;
																		user.Score += 5;

																		if(GameObject.Time - user.lastEmotion > User.emotionDelay) {
																			user.lastEmotion = GameObject.Time;
																			{
																				if(Rand.Next(0, 2) == 0) {
																					DataPacket packet = PacketFactory.Make("emotionAdd");
																					if(packet != null) {
																						packet["id"] = user.Char.ID;
																						packet["t"] = Rand.Next(1, 4);
																						this.world.AddPacket(packet);
																					}
																				}
																			}
																		}
																	} else {
																		character.Health -= 80;
																		user.Char.Health += 30;
																		user.Score += 8;

																		if(GameObject.Time - user.lastEmotion > User.emotionDelay) {
																			user.lastEmotion = GameObject.Time;
																			{
																				if(Rand.Next(0, 2) == 0) {
																					DataPacket packet = PacketFactory.Make("emotionAdd");
																					if(packet != null) {
																						packet["id"] = user.Char.ID;
																						packet["t"] = Rand.Next(1, 4);
																						this.world.AddPacket(packet);
																					}
																				}
																			}
																		}
																	}
																if(user.Char.Health > 100) {
																	user.Char.Health = 100;
																}

																if(character.Health <= 0) {
																	++character.Owner.Deaths;

																	if(GameObject.Time - user.lastEmotion > User.emotionDelay) {
																		user.lastEmotion = GameObject.Time;
																		{
																			if(Rand.Next(0, 2) == 0) {
																				DataPacket packet = PacketFactory.Make("emotionAdd");
																				if(packet != null) {
																					packet["id"] = user.Char.ID;
																					packet["t"] = Rand.Next(1, 4);
																					this.world.AddPacket(packet);
																				}
																			}
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
										break;
									case "grenade":
										if(!user.Char.Zombie) {
											if(GameObject.Time - user.lastGrenade > User.grenadeDelay) {
												user.lastGrenade = GameObject.Time;

												float tX, tY;
												if(float.TryParse(input["tX"].ToString(), out tX)) {
													if(float.TryParse(input["tY"].ToString(), out tY)) {

														Grenade grenade = new Grenade(user, user.Char.X, user.Char.Y, tX, tY);
														GrenadeContainer.Add(grenade);

														{
															DataPacket packet = PacketFactory.Make("grenadeAdd");
															if(packet != null) {
																packet["id"] = grenade.ID;
																packet["x"] = grenade.x;
																packet["y"] = grenade.y;
																packet["tX"] = grenade.tX;
																packet["tY"] = grenade.tY;
																packet["speed"] = grenade.speed;
																this.world.AddPacket(packet);
															}
														}
													}
												}
											}
										}
										break;
								}
							}
						}
					}
	
					foreach(Bullet bullet in BulletContainer.instance.container) {
						foreach(GameObject obj in objects) {
							if(obj.type == 1) {
								Character character = (Character)obj;
								if(bullet.owner.ID != character.Owner.ID) {
									float x = bullet.x - character.X;
									float y = bullet.y - character.Y;
									if(Math.Sqrt(x * x + y * y) < (character.Size / 2) + 1) {
										bullet.Dead = true;
										character.Health -= 10;
										bullet.owner.Score += 1;

										if(character.Health <= 0) {
											++character.Owner.Deaths;

											if(GameObject.Time - bullet.owner.lastEmotion > User.emotionDelay) {
												bullet.owner.lastEmotion = GameObject.Time;
												{
													if(Rand.Next(0, 2) == 0) {
														DataPacket packet = PacketFactory.Make("emotionAdd");
														if(packet != null) {
															packet["id"] = bullet.owner.Char.ID;
															packet["t"] = Rand.Next(1, 4);
															this.world.AddPacket(packet);
														}
													}
												}
											}
										}
	
										{
											DataPacket packet = PacketFactory.Make("bulletRemove");
											if(packet != null) {
												packet["id"] = bullet.ID;
												this.world.AddPacket(packet);
											}
										}
									}
								}
							}
						}
					}
	
					foreach(GameObject obj in objects) {
						obj.Update();
					}

					BulletContainer.Update();

					GrenadeContainer.Update();

					for(int i = 0; i < GrenadeContainer.instance.container.Count; ++i) {
						if(GrenadeContainer.instance.container[i].Dead) {
							Grenade grenade = GrenadeContainer.instance.container[i];

							{
								DataPacket packet = PacketFactory.Make("grenadeRemove");
								if(packet != null) {
									packet["id"] = grenade.ID;
									this.world.AddPacket(packet);
								}
							}

							int count = Rand.Next(5, 9);

							for(int b = 0; b < count; ++b) {
								float nX = Rand.NextFloat(-1, 1);
								float nY = Rand.NextFloat(-1, 1);
								float dist = (float)Math.Sqrt(nX * nX + nY * nY);
								nX /= dist;
								nY /= dist;

								Bullet bullet = new Bullet(grenade.owner, grenade.x, grenade.y, nX, nY);
								bullet.speed = 3f + Rand.NextFloat(2);
								bullet.Life = Rand.Next(400, 800);
								BulletContainer.Add(bullet);
	
								{
									DataPacket packet = PacketFactory.Make("bulletAdd");
									if(packet != null) {
										packet["id"] = bullet.ID;
										packet["x"] = bullet.x;
										packet["y"] = bullet.y;
										packet["nX"] = bullet.nX;
										packet["nY"] = bullet.nY;
										packet["life"] = bullet.Life;
										packet["speed"] = bullet.speed;
										this.world.AddPacket(packet);
									}
								}
							}

							GrenadeContainer.instance.container.Remove(grenade);
						}
					}

					foreach(User user in users) {
						DataPacket userUpdate = user.PacketUpdate;
						if(userUpdate != null) {
							this.world.AddPacket(userUpdate);
						}

						if(user.Char != null) {
							{
								DataPacket packet = PacketFactory.Make("characterUpdate");
								if(packet != null) {
									packet["id"] = user.Char.ID;
									packet["size"] = user.Char.Size;
									packet["health"] = user.Char.Health;
									packet["x"] = user.Char.X;
									packet["y"] = user.Char.Y;
									packet["z"] = user.Char.Zombie;
									this.world.AddPacket(packet);
								}
							}
						}
					}

					++iteration;
	
					this.OnCycleEnd();
				}
			}
		}

		public event HandlerPackets CycleEnd;

		private void OnCycleEnd() {
			if(CycleEnd != null) {
				ICollection<DataPacket> packets = this.world.GrabPackets();
				CycleEnd(packets);
			}
		}
	}
}