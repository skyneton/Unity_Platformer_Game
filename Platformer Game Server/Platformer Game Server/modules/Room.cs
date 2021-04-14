using System;
using System.Collections;
using System.Collections.Generic;

namespace Platformer_Game_Server.modules {
    class Room {
        private ArrayList worker = new ArrayList();
        private Dictionary<string, EntityMonster> monsters = new Dictionary<string, EntityMonster>();
        public bool isPlaying = false;
        public bool isGameStartTimer = false;
        public static int WAITING_TIME = 7;
        public static int START_USER_NUM = 2;
        public static int PLUS_MONSTER_NUM = 2;
        public static int START_MONSTER_NUM = 3;

        public static float START_MONSTER_HEALTH = 10f;
        public static float PLUS_MONSTER_HEALTH = 4f;

        private int stage;

        private long waitingTimer;

        public string ROOM_ID;

        public void AddPlayer(ClientWorker client) {
            CleanPlayers();

            foreach (ClientWorker clients in worker) {
                clients.SendMessage("JoinRoomPlayer");
            }
            worker.Add(client);

            client.SendMessage(new JsonSetting().Add("type", "RoomInfo")
                                .Add("PlayerNumbers", GetPlayerNumbers())
                                .Add("id", client.GetUserID()).ToString());

            if (GetPlayerNumbers() >= START_USER_NUM && !isPlaying && !isGameStartTimer) {
                isGameStartTimer = true;
                if (GetPlayerNumbers() < 4) {
                    string packet = new JsonSetting().Add("timer", WAITING_TIME).Add("type", "GameStartTimer").ToString();
                    waitingTimer = TimeUtils.CurrentTimeInMillis();
                    new DelayManager().Do(10, () => {
                        BroadcastMessage(packet);
                    });
                    new DelayManager().Do(WAITING_TIME * 1000, () => {
                        StartGame();
                    });
                }
                else {
                    new DelayManager().Do(10, () => {
                        StartGame();
                    });
                }
            }
            else if (isGameStartTimer) {
                new DelayManager().Do(10, () => {
                    long now = TimeUtils.CurrentTimeInMillis();
                    client.SendMessage(new JsonSetting().Add("timer", WAITING_TIME - (now - waitingTimer) / 1000).Add("type", "GameStartTimer").ToString());
                });
            }
        }

        public void RemovePlayer(ClientWorker client) {
            try {
                JsonSetting json = new JsonSetting();
                json.Add("id", client.GetUserID()).Add("type", "LeaveRoomPlayer");
                worker.Remove(client);
                BroadcastMessage(json.ToString());

                if (GetPlayerNumbers() < START_USER_NUM && !isPlaying) {
                    isGameStartTimer = false;
                    new DelayManager().Do(10, () => {
                        BroadcastMessage("GameStartTimerCancel");
                    });
                }
                else if (isPlaying && IsAllPlayersDead()) {
                    GameEnd();
                }
            }catch { }
        }

        private void StartGame() {
            if(!isPlaying) {
                if (isGameStartTimer && GetPlayerNumbers() >= START_USER_NUM) {
                    isPlaying = true;
                    isGameStartTimer = false;
                    BroadcastMessage("GameStart");
                    stage = 1;
                }else if(GetPlayerNumbers() >= START_USER_NUM) {
                    isGameStartTimer = true;
                    JsonSetting json = new JsonSetting();
                    BroadcastMessage(json.Add("timer", WAITING_TIME).Add("type", "GameStartTimer").ToString());
                    new DelayManager().Do(WAITING_TIME * 1000, () => {
                        StartGame();
                    });
                }
            }
        }

        public void LoadPlayer(ClientWorker client) {
            if (!isPlaying) return;
            foreach (ClientWorker clients in worker) {
                JsonSetting json = new JsonSetting();
                client.SendMessage(json.Add("id", clients.GetUserID()).Add("type", "EntityPlayerSpawn").ToString());
            }

            client.player = new EntityPlayer(client.GetUserID());
            client.player.SetHealthPoint(client.player.MAX_HEALTH);
            client.targets.Clear();

            if(AllPlayerPlayingCheck()) {
                MonsterSpawnRandomTargeting(START_MONSTER_NUM + PLUS_MONSTER_NUM * (stage - 1));
            }
        }

        public void CleanPlayers() {
            foreach (ClientWorker clients in (ArrayList) worker.Clone()) {
                if (!clients.IsConnected()) {
                    RemovePlayer(clients);
                }
            }
        }

        public bool AllPlayerPlayingCheck() {
            foreach (ClientWorker clients in worker) {
                if (!clients.IsPlaying()) return false;
            }

            return true;
        }

        public void BroadcastMessage(String msg) {
            foreach (ClientWorker clients in (ArrayList) worker.Clone()) {
                clients.SendMessage(msg);
            }
        }

        public void BroadcastMessageNotMe(String eid, String msg) {
            foreach (ClientWorker clients in worker) {
                if (clients.GetUserID().Equals(eid)) continue;
                clients.SendMessage(msg);
            }
        }

        public int GetPlayerNumbers() {
            return worker.Count;
        }

        public void MonsterSpawnRandomTargeting(int num) {
            for (int i = 0; i < num; i++) {
                EntityMonster monster = new EntityMonster();
                monster.SetHealthPoint(START_MONSTER_HEALTH + GetPlayerNumbers() * PLUS_MONSTER_HEALTH + (stage - 1) * PLUS_MONSTER_HEALTH / 2);
                monsters.Add(monster.GetEntityID(), monster);
                ClientWorker player = GetRandomUser();
                if (player == null) break;
                player.targets.Add(monster);
                player.SendMessage(new JsonSetting().Add("id", monster.GetEntityID()).Add("type", "MonsterTarget").ToString());
            }
        }

        public ClientWorker GetRandomUser() {
            Random random = new Random();
            try {
                return (ClientWorker)worker[random.Next(0, GetPlayerNumbers())];
            }catch {
                try {
                    return (ClientWorker)worker[0];
                }catch {
                    return null;
                }
            }
        }

        public void DamagedPlayer(ClientWorker client, string entityId) {
            if (entityId == null || client.player.GetDie()) return;
            client.player.Damaged((float) (new Random().NextDouble() * 2 + 1));
            if(client.player.GetHealthPoint() <= 0) {
                client.player.dies++;
                client.player.SetDie(true);
                BroadcastMessage(new JsonSetting().Add("id", client.GetUserID()).Add("type", "PlayerDied").ToString());
                if (IsAllPlayersDead()) {
                    GameEnd();
                }
            }
            else {
                BroadcastMessage(new JsonSetting().Add("id", client.GetUserID()).Add("hp", client.player.GetHealthPoint()/client.player.MAX_HEALTH).Add("type", "PlayerDamaged").ToString());
            }
        }

        public void DamagedEntity(ClientWorker client, string entityId) {
            if (entityId == null) return;
            EntityMonster monster;
            if(monsters.TryGetValue(entityId, out monster)) {
                monster.SubHealthPoint((float)(new Random().NextDouble() * 3 + 2));
                if(monster.GetHealthPoint() <= 0) {
                    client.targets.Remove(monster);
                    monsters.Remove(entityId);
                    client.player.kills++;
                    BroadcastMessage(new JsonSetting().Add("id", entityId).Add("type", "EntityDied").ToString());

                    if(monsters.Count <= 0) {
                        stage++;
                        PlayerHealAndRespawn();
                        new DelayManager().Do(10000, () => {
                            MonsterSpawnRandomTargeting(START_MONSTER_NUM + PLUS_MONSTER_NUM * (stage - 1));
                        });
                    }
                }else {
                    BroadcastMessage(new JsonSetting().Add("id", entityId).Add("type", "EntityDamaged").ToString());
                }
            }
        }

        public void EntityLocationUpdate(ClientWorker client, string entityId, float x, float y, float rY) {
            if (entityId == null) return;
            EntityMonster monster;
            if (monsters.TryGetValue(entityId, out monster)) {
                monster.GetLocation().Set(x, y, rY);
                BroadcastMessageNotMe(client.GetUserID(), new JsonSetting().Add("id", entityId).Add("x", x).Add("y", y).Add("rY", rY).Add("type", "MonsterLocation").ToString());
            }
        }

        public void PlayerHealAndRespawn() {
            foreach(ClientWorker client in worker) {
                if (client.player.GetDie()) {
                    BroadcastMessage(new JsonSetting().Add("id", client.GetUserID()).Add("type", "Respawn").ToString());
                }
                else {
                    BroadcastMessage(new JsonSetting().Add("id", client.GetUserID()).Add("type", "Recarvery").ToString());
                }
                client.player.Respawn();
            }
        }

        private void GameEnd() {
            isPlaying = false;
            JsonSetting json = new JsonSetting().Add("type", "GameEnd").Add("stage", stage);
            bool isFirst = true;
            foreach (ClientWorker client in worker) {
                if (isFirst) {
                    json.Add("kills", client.player.kills).Add("death", client.player.dies);
                }
                else {
                    json.Replace("kills", client.player.kills).Replace("death", client.player.dies);
                }
                DelaySender(client, json.ToString(), 500, true);
                new DelayManager().Do(1000, () => { client.RoomQuit(); });
                isFirst = false;
            }

            worker.Clear();
            monsters.Clear();
        }

        public bool IsAllPlayersDead() {
            foreach(ClientWorker client in worker) {
                if (!client.player.GetDie()) return false;
            }
            return true;
        }

        public static void DelaySender(ClientWorker client, string str, int time, bool finish = false) {
            new DelayManager().Do(time, () => {
                client.SendMessage(str);
            });
        }
    }
}
