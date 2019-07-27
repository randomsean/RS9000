using System;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace RS9000
{
    internal class Antenna
    {
        /// <summary>
        /// Deviation angle from forward for the opposite mode
        /// </summary>
        private const float OppositeAngle = 10f; // degrees

        /// <summary>
        /// Maximum distance antenna can reach
        /// </summary>
        private const float BeamReach = 50f; // meters

        /// <summary>
        /// Beamwidth angle at 3db (half-power)
        /// </summary>
        private const float Beamwidth = 6f; // degrees

        /// <summary>
        /// Radius of the beamwidth measured at half-power
        /// </summary>
        private static readonly float BeamRadius = (BeamReach / 2) * (float)Math.Tan(Beamwidth * (Math.PI / 180d));

        /// <summary>
        /// Distance of deviation from center for opposite mode
        /// </summary>
        private static readonly float OppositeOffset = BeamReach * (float)Math.Tan(OppositeAngle * (Math.PI / 180d));

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value)
                {
                    return;
                }

                Script.SendMessage(MessageType.AntennaPower, new
                {
                    name = Name,
                    powered = value,
                    mode = Mode,
                });

                if (!value)
                {
                    Reset();
                }

                isEnabled = value;
            }
        }
        private bool isEnabled;

        public AntennaMode Mode
        {
            get => mode;
            set
            {
                if (value == mode)
                {
                    return;
                }

                Script.SendMessage(MessageType.SwitchMode, new
                {
                    name = Name,
                    mode = (int)value,
                });

                mode = value;
            }
        }
        private AntennaMode mode;

        public Entity LockedTarget
        {
            get => lockedTarget;
            set
            {
                if (value == lockedTarget)
                {
                    return;
                }

                bool locked = value != null;

                Script.SendMessage(MessageType.TargetLock, new
                {
                    name = Name,
                    locked,
                    plate = locked && value is Vehicle v ? v.Mods.LicensePlate : null,
                });

                lockedTarget = value;
            }
        }
        private Entity lockedTarget;

        public Entity Target { get; private set; }

        public TargetDirection TargetDirection { get; set; }

        public float Speed { get; private set; }

        public float FastSpeed { get; private set; }

        public float FastLimit { get; set; }

        public Entity Source { get; set; }

        public string Name { get; }

        public event EventHandler<FastLockedEventArgs> FastLocked;

        private readonly Vector3 direction;

        public Antenna(string name, float heading)
        {
            Name = name;
            direction = GameMath.HeadingToDirection(heading) + Vector3.UnitX;
        }

        public void Reset()
        {
            Speed = 0;
            ResetFast();
        }

        public void ResetFast()
        {
            FastSpeed = 0;
            LockedTarget = null;
        }

        public bool Poll()
        {
            if (!IsEnabled || Source == null)
            {
                Target = null;
                TargetDirection = TargetDirection.None;
                return false;
            }

            Vector3 offset = angles[(int)Mode] * direction;
            Vector3 max = Source.GetOffsetPosition(offset);

#if DEBUG
            DrawLine(Source.Position, max, 0xFF, 0x00, 0x00);
#endif

            RaycastResult result = World.RaycastCapsule(Source.Position, max, BeamRadius, (IntersectOptions)10, Source);

            Target = result.HitEntity;
            if (Target == null || !Target.Exists())
            {
                Target = null;
                TargetDirection = TargetDirection.None;
                return false;
            }

            if (Target is Vehicle)
            {
                Speed = ((Vehicle)Target).Speed;
            }
            else
            {
                Speed = Target.Velocity.Length();
            }

            TargetDirection = IsHeadingTowards(Source, Target) ? TargetDirection.Coming : TargetDirection.Going;

            if (Speed > FastLimit)
            {
                if (Speed > FastSpeed)
                {
                    FastSpeed = Speed;
                }
                if (LockedTarget == null)
                {
                    LockedTarget = Target;
                    FastLocked?.Invoke(this, new FastLockedEventArgs(FastSpeed));
                }
            }

            return true;
        }

        private static readonly Vector3[] angles = new Vector3[]
        {
            new Vector3(0, BeamReach, 0), // AntennaMode.Same
            new Vector3(-OppositeOffset, BeamReach, 0), // AntennaMode.Opposite
        };

        private static bool IsHeadingTowards(Entity source, Entity target)
        {
            float diff = (source.Heading - target.Heading + 180) % 360;
            if (diff < 0)
                diff += 360;
            diff -= 180;
            diff = Math.Abs(diff);
            return diff > 90;
        }

#if DEBUG
        private static void DrawLine(Vector3 from, Vector3 to, int r, int g, int b)
        {
            API.DrawLine(from.X, from.Y, from.Z, to.X, to.Y, to.Z, r, g, b, 0xFF);
        }
#endif
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
