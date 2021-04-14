using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class EntityPlayer : Entity {
        public float MAX_HEALTH = 30f;
        public int kills = 0;
        public int dies = 0;
        public long HEALING_TIME = 1000;
        private long healingTime = 0;

        public EntityPlayer(Room room, string id) : base(room) {
            SetEntityID(id);
        }

        public void Respawn() {
            SetDie(false);
            SetHealthPoint(MAX_HEALTH);
        }

        public bool Healing() {
            long now = TimeUtils.CurrentTimeInMillis();
            if (now - healingTime >= HEALING_TIME) {
                AddHealthPoint((float)(new Random().NextDouble() + 0.1));
                healingTime = now;
                return true;
            }
            return false;
        }
    }
}
