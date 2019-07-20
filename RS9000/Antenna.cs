using System;

using CitizenFX.Core;

namespace RS9000
{
    internal class Antenna
    {
        public const float MaxSpeed = 999f;

        public bool Enabled { get; set; } = true;

        public AntennaMode Mode { get; set; }

        public float Speed { get; private set; }

        public float FastSpeed { get; private set; }

        private float fastLimit;
        public float FastLimit
        {
            get => fastLimit;
            set => fastLimit = Clamp(value, 0, MaxSpeed);
        }

        public bool FastLocked { get; set; }

        public Entity Source { get; set; }

        private Vector3 Direction { get; }

        public string Name { get; }

        private static readonly Vector3[] angles = new Vector3[]
        {
            new Vector3(0, 50, 0), // AntennaMode.Same
            new Vector3(-10, 50, 0), // AntennaMode.Opposite
        };

        public Antenna(string name, float heading)
        {
            Name = name;
            Direction = GameMath.HeadingToDirection(heading) + Vector3.UnitX;
        }

        private float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public void Reset()
        {
            Speed = 0;
            ResetFast();
        }

        public void ResetFast()
        {
            FastSpeed = 0;
            FastLocked = false;
        }

        public bool Poll()
        {
            if (!Enabled || Source == null)
            {
                return false;
            }

            Vector3 offset = angles[(int)Mode] * Direction;
            Vector3 max = Source.GetOffsetPosition(offset);

            RaycastResult result = World.RaycastCapsule(Source.Position, max, 2, (IntersectOptions)10, Source);

            Entity target = result.HitEntity;
            if (target == null || !target.Exists())
            {
                return false;
            }

            Speed = target.Velocity.Length();

            if (Speed > FastLimit && !FastLocked)
            {
                FastSpeed = Speed;
                FastLocked = true;
            }

            return true;
        }
    }
}
