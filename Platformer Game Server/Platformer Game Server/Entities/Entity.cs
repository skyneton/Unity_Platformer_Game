using System;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer.Entities
{
    public class Entity
    {
        public readonly Guid EntityId = Guid.NewGuid();
        public readonly Location Location = new();

        public double Health;
        
        public bool IsAlive { get; protected set; } = true;
        public double Damage { get; protected set; }

        public void Die()
        {
            IsAlive = false;
        }

        public void Respawn()
        {
            IsAlive = true;
        }
    }
}