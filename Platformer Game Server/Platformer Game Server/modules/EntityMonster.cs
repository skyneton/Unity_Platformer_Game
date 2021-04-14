using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class EntityMonster : Entity {
        public EntityMonster() {
            SetEntityID(Guid.NewGuid().ToString());
        }
    }
}
