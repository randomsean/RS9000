using System;

using CitizenFX.Core;

namespace RS9000
{
    internal class Antenna
    {
        public const float MaxSpeed = 999f;

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (enabled != value)
                {
                    enabled = value;
                    Script.SendMessage(MessageType.AntennaPower, new
                    {
                        name = Name,
                        enabled,
                        mode = Mode,
                    });
                    if (!enabled)
                    {
                        Reset();
                    }
                }
            }
        }

        private AntennaMode mode;
        public AntennaMode Mode
        {
            get => mode;
            set
            {
                if (value != mode)
                {
                    mode = value;
                    Script.SendMessage(MessageType.SwitchMode, new
                    {
                        name = Name,
                        mode = (int)mode,
                    });
                }
            }
        }

        public float Speed { get; private set; }

        public float FastSpeed { get; private set; }

        private float fastLimit;
        public float FastLimit
        {
            get => fastLimit;
            set => fastLimit = Clamp(value, 0, MaxSpeed);
        }

        private bool fastLocked;
        public bool FastLocked
        {
            get => fastLocked;
            set
            {
                if (value == fastLocked)
                {
                    return;
                }
                fastLocked = value;
                if (!fastLocked)
                {
                    FastSpeed = 0;
                }
            }
        }

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
