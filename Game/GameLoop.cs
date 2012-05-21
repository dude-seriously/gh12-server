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
        private int ups;
        
        private GameWorld world;
        private Map map;

        private Timer timer;
        private Timer timerSend;
        private double averigeTimeLast;
        private double averigeTimeSendLast;

        public GameLoop() {
            this.ups = 100;
            this.timer = new Timer();
            this.timer.Average = 1000;

            this.timerSend = new Timer();
            this.timerSend.Average = 1000;
        }

        public GameWorld World {
            set {
                if(!this.running) {
                    this.world = value;
                }
            }
            get { return this.world; }
        }
        
        
        public Map Map {
            set {
                if(!this.running) {
                    this.map = value;
                }
            }
            get { return this.map; }
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

            long lastTick = Time.Now;

            while(this.running) {
                Thread.Sleep(1);
                if(Time.Now - lastTick > this.ups) {
/* TICK BEGIN */
                    lastTick = Time.Now;
                    GameObject.Time = lastTick;

                    this.timer.Start();
 
                    ICollection<User> users = this.world.Users;
                    ICollection<GameObject> objects = this.world.Objects;
 
/* UPDATE ALL USERS */
                    foreach(User user in users) {
/* PROCESS USER STATES */
        /* REMOVING */
                        if(user.States["removing"] != null && user.States["removing"].Equals(true)) {
                            this.world.AddPacket(PacketFactory.Make("userRemove", "id", user.ID));

                            user.Remove();
                        } else {
        /* NEW */
                            if(user.States["new"] != null && user.States["new"].Equals(true)) {
                                user.States["new"] = null;
                                
                                user.States["sync"] = true;

                                foreach(User iUser in users) {
                                    if(iUser.ID != user.ID && user.Client != null) {
                                        user.Client.SendAsync(PacketFactory.Make("userAdd", "id", iUser.ID, "n", iUser.Name, "l", iUser.Latency, "s", iUser.Score));
                                    }
                                }
                                
                                foreach(GameObject obj in objects) {
                                    try {
                                        user.Client.SendAsync(obj.PacketFull);
                                    } catch(Exception ex) {
                                        
                                    }
                                }
                                
                                for(int y = 0; y < map.Height; ++y) {
                                    for(int x = 0; x < map.Width; ++x) {
                                        user.Client.SendAsync(map.Cell(x, y).PacketFull);
                                    }
                                }
                                
                                this.world.AddPacket(PacketFactory.Make("userAdd", "id", user.ID, "n", user.Name, "l", user.Latency, "s", user.Score));
                            }
        /* SET NAME */
                            if(user.States["nick"] != null && user.States["nick"].Equals(true)) {
                                user.States["nick"] = null;
                                
                                user.Client.SendAsync(PacketFactory.Make("userStart"));
                                
                                user.Client.SendAsync(PacketFactory.Make("mapData", "w", this.map.Width, "h", this.map.Height, "d", this.map.MapArray));
                                
                                user.Char.Enabled = true;
                                user.Char.Cell = map.Cell(Rand.Next(0, map.Width), Rand.Next(0, map.Height));
                            }
                            
/* PROCESS INPUT */
                            UserInput input = null;
                            
                            while((input = user.GrabInput()) != null) {
                                switch(input.Name) {
                                    case "charMove":
                                        user.Char.Direction = (byte)input["d"];
                                        break;
                                    case "mobSpawn":
                                        if (Time.Now - user.lastSpawn > 1000) {
                                            user.lastSpawn = Time.Now;
                                            
                                            if (user.Score > 0) {
                                                --user.Score;
                                                
                                                Hero hero = new Hero();
                                                hero.Evil = (user.ID % 2) == 0;
                                                hero.Cell = this.map.Cell((int)input["x"], (int)input["y"]);
                                                this.world.AddObject(hero);
                                                this.world.AddPacket(hero.PacketFull);
                                            }
                                        }
                                        break;
                                }
                            }
                            
/* PUBLISH USER UPDATE PACKET */
                            this.world.AddPacket(user.PacketUpdate);
                            //this.world.AddPacket(user.Char.PacketUpdate);
                        }
                        
                        if (user.Client == null) {
                            user.States["removing"] = true;
                        }
                    }
 
/* UPDATE ALL OBJECTS */
                    foreach(GameObject obj in objects) {
                        obj.Update();
                        
                        if (obj.Deleted) {
                            obj.Delete();
                            this.world.AddPacket(obj.PacketRemove);
                            this.world.RemoveObject(obj);
                        } else {
                            this.world.AddPacket(obj.PacketUpdate);
                        }
                    }
                    
                    for(int y = 0; y < map.Height; ++y) {
                        for(int x = 0; x < map.Width; ++x) {
                            this.world.AddPacket(map.Cell(x, y).PacketUpdate);
                        }
                    }

                    timer.Stop();

                    if (averigeTimeLast != this.timer.ElapsedMilliseconds) {
                        averigeTimeLast = this.timer.ElapsedMilliseconds;
                        Log.Add("cycle: " + averigeTimeLast);
                    }

/* TICK END */
                    ++iteration;
 
                    this.timerSend.Start();
                    this.OnCycleEnd();
                    this.timerSend.Stop();

                    if (averigeTimeSendLast != this.timerSend.ElapsedMilliseconds) {
                        averigeTimeSendLast = this.timerSend.ElapsedMilliseconds;
                        Log.Add("cycle end: " + averigeTimeSendLast);
                    }
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