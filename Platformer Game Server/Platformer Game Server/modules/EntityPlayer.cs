using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class EntityPlayer : Entity {
        public float MAX_HEALTH = 30f;
        public int kills = 0;
        public int dies = 0;
        public EntityPlayer(string id) {
            SetEntityID(id);
        }

        public void Respawn() {
            SetDie(false);
            SetHealthPoint(MAX_HEALTH);
        }
    }
}
