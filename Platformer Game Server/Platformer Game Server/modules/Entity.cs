using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class Entity {
        private Location loc = new Location();
        private Room room;
        private float hp = 10f;
        private string ENTITY_ID;
        private bool isDie = false;

        private long beforeDamagedTime = 0;
        public Entity(Room room) {
            this.room = room;
        }
        public Entity(Room room, float hp) {
            this.room = room;
            this.hp = hp;
        }

        public float GetHealthPoint() {
            return hp;
        }

        public Location GetLocation() {
            return loc;
        }

        public void SetHealthPoint(float hp) {
            this.hp = hp;
        }

        public void SubHealthPoint(float hp) {
            this.hp -= hp;
        }

        public void AddHealthPoint(float hp) {
            this.hp += hp;
        }

        public void SetEntityID(string id) {
            ENTITY_ID = id;
        }

        public string GetEntityID() {
            return ENTITY_ID;
        }

        public void SetDie(bool die) {
            this.isDie = die;
        }

        public bool GetDie() {
            return isDie;
        }

        public void Damaged(float damage) {
            long now = TimeUtils.CurrentTimeInMillis();
            if(now - beforeDamagedTime >= 10) {
                SubHealthPoint(damage);
                beforeDamagedTime = now;
            }
        }

        public Room GetRoom() {
            return room;
        }
    }
}
