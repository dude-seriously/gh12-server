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

        public GameLoop() {
            this.ups = 20;
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
                if(Time.Now - lastTick > this.ups) {
/* TICK BEGIN */
                    lastTick = Time.Now;
                    GameObject.Time = lastTick;
 
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

                                foreach(User iUser in users) {
                                    if(iUser.ID != user.ID && user.Client != null) {
                                        user.Client.SendAsync(PacketFactory.Make("userAdd", "id", iUser.ID, "n", iUser.Name, "l", iUser.Latency));
                                    }
                                }
                                
                                this.world.AddPacket(PacketFactory.Make("userAdd", "id", user.ID, "n", user.Name, "l", user.Latency));
                            }
                            
                            if(user.States["nick"] != null && user.States["nick"].Equals(true)) {
                                user.States["nick"] = null;
                                
                                user.Client.SendAsync(PacketFactory.Make("userStart"));
                                
                                user.Client.SendAsync(PacketFactory.Make("mapData", "w", this.map.Width, "h", this.map.Height, "d", this.map.MapArray));
                            }
                            
/* PROCESS INPUT */
                            UserInput input = null;
                            
                            while((input = user.GrabInput()) != null) {
                                switch(input.Name) {
                                    case "..":
                                        break;
                                }
                            }
                            
/* PUBLISH USER UPDATE PACKET */
                            this.world.AddPacket(user.PacketUpdate);
                        }
                    }
 
/* UPDATE ALL OBJECTS */
                    foreach(GameObject obj in objects) {
                        obj.Update();
                    }

/* TICK END */
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