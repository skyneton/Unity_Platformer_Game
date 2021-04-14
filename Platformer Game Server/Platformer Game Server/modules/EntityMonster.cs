using System;
using System.Collections.Generic;
using System.Text;

namespace Platformer_Game_Server.modules {
    class EntityMonster : Entity {
        public EntityMonster(Room room) : base(room) {
            SetEntityID(Guid.NewGuid().ToString());
        }
    }
}
