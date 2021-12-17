using System;
using PlatformerGameServer.Network;
using PlatformerGameServer.Network.Packet;
using PlatformerGameServer.Utils;
using PlatformerGameServer.World;

namespace PlatformerGameServer.Entities
{
    public class EntityMonster : Entity
    {
        public const long LocationSendTerm = 40;
        public const int StartMonsterCount = 3;
        public const int PlusMonsterNum = 2;
        public const double Height = 0.44418;
        
        public const float StartMonsterHealth = 30f;
        public const float PlusMonsterHealth = 4f;

        public const double Gravity = -9.8;

        private static readonly double MoveSpeed = 4.3;

        private long beforeUpdateTime = TimeManager.CurrentTimeMillis;
        private Location beforeLocation;

        private long LastLocationSendTime = TimeManager.CurrentTimeMillis;

        private readonly Room room;

        private double velocityY = 0;

        private bool IsOnGround;

        public EntityMonster(Room room, double x, double y)
        {
            Location.Set(x, y);
            this.room = room;
            beforeLocation = Location.Clone();
        }

        public void Update()
        {
            var now = TimeManager.CurrentTimeMillis;
            var deltaTime = (now - beforeUpdateTime) * 0.001;
            
            Move(deltaTime);
            GravityUpdate(deltaTime);
            LocationSendUpdate(now);
            GroundCheck();
            
            beforeUpdateTime = now;
        }

        private void Move(double deltaTime)
        {
            if (HasForward(deltaTime) && !HasForwardDown(deltaTime))
                Forward(deltaTime);
            else
                Location.Direction *= -1;
        }

        private void GravityUpdate(double deltaTime)
        {
            if(velocityY > Gravity)
                velocityY += Gravity * deltaTime;
            
            var blockX = (int) Math.Floor(Location.X + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY);

            var updateY = Location.Y + velocityY * deltaTime;
            var checkY = (int) Math.Floor(updateY - Height + WorldData.DifY);

            var direction = velocityY > 0 ? 1 : -1;
            var height = WorldData.Map.GetLength(0);

            for (var i = blockY; i >= 0 && i < height && i != checkY; i += direction)
            {
                if (WorldData.Map[i, blockX] != 1) continue;
                updateY = i - WorldData.DifY - direction + Height;
                if (velocityY < 0) velocityY = 0;
                break;
            }
            
            checkY = (int) Math.Floor(updateY - Height + WorldData.DifY);
            if (checkY < 0 || checkY >= height || WorldData.Map[checkY, blockX] == 1)
            {
                updateY = checkY - WorldData.DifY - direction + Height;
                if (velocityY < 0) velocityY = 0;
            }
            Location.Y = updateY;
        }

        private void GroundCheck()
        {
            var blockX = (int) Math.Floor(Location.X + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY) - 1;
            
            IsOnGround = !((blockX < 0 || blockX >= WorldData.Map.GetLength(1)) ||
                           (blockY < 0 || WorldData.Map.GetLength(0) <= blockY) || WorldData.Map[blockY, blockX] == 1);
        }

        private bool HasForward(double deltaTime)
        {
            var blockX = (int) Math.Floor(Location.X + MoveSpeed * Location.Direction * deltaTime + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY);

            return !((blockX < 0 || blockX >= WorldData.Map.GetLength(1)) ||
                     (blockY < 0 || WorldData.Map.GetLength(0) <= blockY) || WorldData.Map[blockY, blockX] == 1);
        }

        private bool HasForwardDown(double deltaTime)
        {
            var blockX = (int) Math.Floor(Location.X + MoveSpeed * Location.Direction * deltaTime + WorldData.DifX) + Location.Direction;
            var blockY = (int) Math.Floor(Location.Y + WorldData.DifY) - 1;

            return !((blockX < 0 || blockX >= WorldData.Map.GetLength(1)) ||
                     (blockY < 0 || WorldData.Map.GetLength(0) <= blockY) || WorldData.Map[blockY, blockX] == 1);
        }

        private void Forward(double deltaTime)
        {
            Location.X += MoveSpeed * Location.Direction * deltaTime;
            // Console.WriteLine("A");
        }

        private void LocationSendUpdate(long now)
        {
            if (now - LastLocationSendTime < LocationSendTerm) return;
            if (Math.Abs(beforeLocation.X - Location.X) < 0.05 && Math.Abs(beforeLocation.Y - Location.Y) < 0.05 &&
                beforeLocation.Direction == Location.Direction)
                return;
            
            room.Broadcast(new PacketOutMonsterLocation(EntityID.ToByteArray(), Location.X, Location.Y, Location.Direction));

            beforeLocation = Location.Clone();
        }
    }
}