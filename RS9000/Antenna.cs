using System;

using CitizenFX.Core;

namespace RS9000
{
    internal class Antenna
    {
        public const float MaxSpeed = 999f;

        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    Script.SendMessage(MessageType.AntennaPower, new
                    {
                        name = Name,
                        isEnabled,
                        mode = Mode,
                    });
                    if (!isEnabled)
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

        private bool isFastLocked;
        public bool IsFastLocked
        {
            get => isFastLocked;
            set
            {
                if (value == isFastLocked)
                {
                    return;
                }
                isFastLocked = value;
                if (!isFastLocked)
                {
                    FastSpeed = 0;
                }
            }
        }

        public Entity Source { get; set; }

        private Vector3 Direction { get; }

        public string Name { get; }

        public event EventHandler<FastLockedEventArgs> FastLocked;

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
            IsFastLocked = false;
        }

        public bool Poll()
        {
            if (!IsEnabled || Source == null)
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

            if (target is Vehicle)
            {
                Speed = ((Vehicle)target).Speed;
            }
            else
            {
                Speed = target.Velocity.Length();
            }

            if (Speed > FastLimit && !IsFastLocked)
            {
                FastSpeed = Speed;
                IsFastLocked = true;
                FastLocked?.Invoke(this, new FastLockedEventArgs(FastSpeed));
            }

            return true;
        }
    }

    internal class FastLockedEventArgs : EventArgs
    {
        public float Speed { get; }

        public FastLockedEventArgs(float speed)
        {
            Speed = speed;
        }
    }
}
