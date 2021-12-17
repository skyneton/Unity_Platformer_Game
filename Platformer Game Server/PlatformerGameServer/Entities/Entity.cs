using System;
using PlatformerGameServer.Utils;

namespace PlatformerGameServer.Entities
{
    public class Entity
    {
        public readonly Guid EntityID = Guid.NewGuid();
        public readonly Location Location = new Location();

        public double Health;
    }
}