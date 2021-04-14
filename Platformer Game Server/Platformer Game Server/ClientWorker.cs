using Newtonsoft.Json.Linq;
using Platformer_Game_Server.modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Platformer_Game_Server {
    class ClientWorker {
        public TcpClient listener { get; }
        private string ROOM_ID, USER_ID;
        private bool isReady = false;
        private bool isPlaying = false;
        private Location loc = new Location();
        public ArrayList targets = new ArrayList();

        public EntityPlayer player;
        public ClientWorker(TcpClient listener) {
            if(Program.debug) Console.WriteLine("클라이언트가 연결되었습니다.");
            this.listener = listener;
        }

        public void SendMessage(String str) {
            try {
                StreamWriter writer = new StreamWriter(listener.GetStream());
                writer.WriteLine(str);
                writer.Flush();
                if (Program.debug && Program.post) Console.WriteLine(str);
            } catch(Exception e) {
                if (Program.debug) Console.WriteLine("쓰기 오류");
            }
        }

        public string GetUserID() {
            return USER_ID;
        }

        private Room roomCreateOrJoin() {
            USER_ID = Guid.NewGuid().ToString();
            isReady = true;
            foreach (KeyValuePair<string, Room> item in Program.roomList) {
                if(!item.Value.isPlaying && item.Value.GetPlayerNumbers() < 4) {
                    item.Value.AddPlayer(this);
                    ROOM_ID = item.Key;
                    return item.Value;
                }
            }

            Room room = new Room();
            room.AddPlayer(this);
            ROOM_ID = Guid.NewGuid().ToString();
            Program.roomList.Add(ROOM_ID, room);
            return room;
        }

        public void RoomQuit() {
            if(isReady) {
                Room room = GetRoom();
                if(room != null) {
                    room.RemovePlayer(this);
                    if(room.GetPlayerNumbers() <= 0) {
                        Program.roomList.Remove(ROOM_ID);
                    }
                }
                isReady = false;
                isPlaying = false;
            }
        }

        public Room GetRoom() {
            Room room;
            Program.roomList.TryGetValue(ROOM_ID, out room);
            return room;
        }

        public void Exit() {
            if(isReady) {
                RoomQuit();
            }
            if(IsConnected()) {
                listener.Close();
            }
        }

        public bool IsConnected() {
            return Program.IsConnected(listener);
        }

        public bool IsPlaying() {
            return isPlaying;
        }

        public void ReceivePacketEvent() {
            try {
                NetworkStream s = listener.GetStream();
                if (s.DataAvailable) {
                    string data = new StreamReader(s, true).ReadLine();
                    if (data != null) {
                        DistinguishPacket(data);
                    }
                }
            } catch { }
        }

        public void DistinguishPacket(string packet) {
            if (Program.debug && Program.receive) Console.WriteLine(packet);
            switch (packet) {
                case "GameReady": {
                        if (!isReady) {
                            roomCreateOrJoin();
                        }
                        break;
                    }
                case "Quit": {
                        Exit();
                        break;
                    }
                case "InGameSceneLoaded": {
                        if (isReady) {
                            isPlaying = true;
                            Room room = GetRoom();
                            if (room != null && room.isPlaying) {
                                room.LoadPlayer(this);
                            }
                        }
                        break;
                    }
                case "AttackMotionStart": case "AttackMotionEnd": {
                        if (isPlaying) {
                            Room room = GetRoom();
                            if (room != null && room.isPlaying) {
                                room.BroadcastMessageNotMe(USER_ID, new JsonSetting().Add("id", USER_ID).Add("type", packet).ToString());
                            }
                        }
                        break;
                    }
                default: {
                        DistinguishJsonPacket(new JsonSetting().loadJsonString(packet));
                        break;
                    }
            }
        }

        public void DistinguishJsonPacket(JsonSetting packet) {
            if (packet.Get("type") == null) return;
            switch(packet.Get("type")) {
                case "Location": {
                    if (isReady && isPlaying) {
                        Room room = GetRoom();
                        if (room != null && room.isPlaying) {
                                float x = 0, y = 0, rY = 0;
                                float.TryParse(packet.Get("x"), out x);
                                float.TryParse(packet.Get("y"), out y);
                                float.TryParse(packet.Get("rY"), out rY);
                                loc.Set(x, y, rY);
                                SendLocation(room);
                            }
                    }
                    break;
                }
                case "PlayerDamagedByEntity": {
                    if(isPlaying) {
                        Room room = GetRoom();
                        if (room != null && room.isPlaying) {
                            room.DamagedPlayer(this, packet.Get("entity"));
                        }
                    }
                    break;
                }
                case "EntityDamagedByPlayer": {
                    if (isPlaying) {
                        Room room = GetRoom();
                        if (room != null && room.isPlaying) {
                            room.DamagedEntity(this, packet.Get("entity"));
                        }
                    }
                    break;
                }
                case "MonsterLocation": {
                    if (isPlaying) {
                        Room room = GetRoom();
                        if (room != null && room.isPlaying) {
                            float x = 0, y = 0, rY = 0;
                            float.TryParse(packet.Get("x"), out x);
                            float.TryParse(packet.Get("y"), out y);
                            float.TryParse(packet.Get("rY"), out rY);
                            room.EntityLocationUpdate(this, packet.Get("id"), x, y, rY);
                        }
                    }
                    break;
                }
            }
        }

        public void SendLocation(Room room) {
            JsonSetting json = new JsonSetting();
            room.BroadcastMessageNotMe(USER_ID, json.Add("x", loc.GetX()).Add("y", loc.GetY()).Add("rY", loc.GetRY()).Add("id", USER_ID).Add("type", "Location").ToString());
        }
    }
}
