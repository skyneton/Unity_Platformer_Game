using System;
using PlatformerGameServer.Network;
using PlatformerGameServer.Network.Packet;
using PlatformerGameServer.Utils;
using PlatformerGameServer.World;

namespace PlatformerGameServer.Entities
{
    public class EntityMonster : Entity
    {
        private const long LocationSendTerm = 30;
        public const int StartMonsterCount = 3;
        public const int PlusMonsterNum = 2;
        private const double Height = 0.44418;
        private const double TargetDistance = 11.5 * 11.5;
        
        public const float StartMonsterHealth = 30f;
        public const float PlusMonsterHealth = 4f;

        private const double Gravity = -9.8;
        private const double JumpPower = 8.3;

        private static readonly double MoveSpeed = 4.5;

        private long beforeUpdateTime = TimeManager.CurrentTimeMillis;
        private Location beforeLocation;

        private long lastLocationSendTime = TimeManager.CurrentTimeMillis;

        private readonly Room room;

        private double velocityY = 0;

        private bool isOnGround;

        private Entity target;

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
            
            TargetUpdate();
            Move(deltaTime);
            GravityUpdate(deltaTime);
            LocationSendUpdate(now);
            GroundCheck();
            
            beforeUpdateTime = now;
        }

        private void Move(double deltaTime)
        {
            if (target != null)
            {
                GoToTarget(deltaTime);
                return;
            }

            if (!HasForward(deltaTime) && (HasForwardDown(deltaTime) || !isOnGround))
                Forward(deltaTime);
            else
                Location.Direction *= -1;
        }

        private void GoToTarget(double deltaTime)
        {
            var direction = target.Location.X - Location.X < 0 ? -1 : 1;
            switch (target.Location.Y - Location.Y)
            {
                case > 2 when isOnGround:
                {
                    if(direction == Location.Direction && HasJumpBarrier())
                        Jump();
                    else if (HasForward(deltaTime) || direction != Location.Direction && (HasBackJumpBarrier() || !HasForwardDown(deltaTime)))
                        Location.Direction *= -1;
                    else
                        Forward(deltaTime);
                    return;
                }
                case < -1.3 when isOnGround:
                {
                    if (HasForward(deltaTime))
                        Location.Direction *= -1;
                    else
                        Forward(deltaTime);
                    return;
                }
                default:
                {
                    if (Math.Abs(target.Location.Y - Location.Y) < .5)
                        Location.Direction = direction;
                    break;
                }
            }

            if (!HasForward(deltaTime))
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
                if (GetBlockCode(blockX, i) != 1) continue;
                updateY = i - WorldData.DifY - direction - Height * direction;
                if (velocityY < 0) velocityY = 0;
                break;
            }
            
            checkY = (int) Math.Floor(updateY - Height + WorldData.DifY);
            if (GetBlockCode(blockX, checkY) == 1)
            {
                updateY = checkY - WorldData.DifY - direction - Height * direction;
                if (velocityY < 0) velocityY = 0;
            }
            Location.Y = updateY;
        }

        private void Jump()
        {
            if (!isOnGround) return;
            velocityY += JumpPower;
            isOnGround = false;
        }

        private void GroundCheck()
        {
            var blockX = (int) Math.Floor(Location.X + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY) - 1;
            
            isOnGround = velocityY == 0 && GetBlockCode(blockX, blockY) == 1;
        }

        private void TargetUpdate()
        {
            if (target == null || target?.Location.DistancePow(Location) > TargetDistance)
            {
                target = room.NearPlayer(Location, TargetDistance);
            }
        }

        private bool HasForward(double deltaTime)
        {
            var blockX = (int) Math.Floor(Location.X + MoveSpeed * Location.Direction * deltaTime + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY);

            return GetBlockCode(blockX, blockY) == 1;
        }

        private bool HasForwardDown(double deltaTime)
        {
            var blockX = (int) Math.Floor(Location.X + MoveSpeed * Location.Direction * deltaTime + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY) - 1;

            return GetBlockCode(blockX, blockY) == 1;
        }

        private bool HasJumpBarrier()
        {
            var blockX = (int) Math.Floor(Location.X + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY);

            return GetBlockCode(blockX, blockY + 2) == 0 &&
                   GetBlockCode(blockX + Location.Direction, blockY + 2) == 0 && GetBlockCode(blockX + Location.Direction * 2, blockY + 2) == 0 &&
                   (GetBlockCode(blockX + Location.Direction * 3, blockY + 2) == 1 ||
                    GetBlockCode(blockX, blockY + 3) == 0 &&
                    GetBlockCode(blockX + Location.Direction * 2, blockY + 3) == 0 &&
                    GetBlockCode(blockX + Location.Direction * 3, blockY) == 1);
        }

        private bool HasBackJumpBarrier()
        {
            var blockX = (int) Math.Floor(Location.X + WorldData.DifX);
            var blockY = (int) Math.Floor(Location.Y - Height + WorldData.DifY);

            return GetBlockCode(blockX, blockY + 2) == 0 &&
                   GetBlockCode(blockX + -Location.Direction, blockY + 2) == 0 && GetBlockCode(blockX + -Location.Direction * 2, blockY + 2) == 0 &&
                   (GetBlockCode(blockX + -Location.Direction * 3, blockY + 2) == 1 ||
                    GetBlockCode(blockX, blockY + 3) == 0 &&
                    GetBlockCode(blockX + -Location.Direction * 2, blockY + 3) == 0 &&
                    GetBlockCode(blockX + -Location.Direction * 3, blockY) == 1);
        }

        private static int GetBlockCode(int blockX, int blockY)
        {
            return IsWidthIn(blockX, blockY) ? WorldData.Map[blockY, blockX] : 1;
        }

        private static bool IsWidthIn(int blockX, int blockY)
        {
            return blockX >= 0 && blockX < WorldData.Map.GetLength(1) && blockY >= 0 &&
                   blockY < WorldData.Map.GetLength(0);
        }

        private void Forward(double deltaTime)
        {
            Location.X += MoveSpeed * Location.Direction * deltaTime;
        }

        private void LocationSendUpdate(long now)
        {
            if (now - lastLocationSendTime < LocationSendTerm) return;
            if (Math.Abs(beforeLocation.X - Location.X) < 0.05 && Math.Abs(beforeLocation.Y - Location.Y) < 0.05 &&
                beforeLocation.Direction == Location.Direction)
                return;
            
            room.Broadcast(new PacketOutMonsterLocation(EntityID.ToByteArray(), Location.X, Location.Y, Location.Direction));

            beforeLocation = Location.Clone();
            lastLocationSendTime = now;
        }
    }
}