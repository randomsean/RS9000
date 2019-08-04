using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

namespace RS9000
{
    internal class Radar
    {
        public const float MaxSpeed = 999f;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value == isEnabled)
                {
                    return;
                }

                foreach (Antenna antenna in Antennas.Values)
                {
                    antenna.IsEnabled = value;
                }

                Script.SendMessage(MessageType.RadarPower, value);

                isEnabled = value;
            }
        }
        private bool isEnabled;

        public bool IsDisplayed
        {
            get => isDisplayed;
            set
            {
                if (value == isDisplayed)
                {
                    return;
                }

                Script.SendMessage(MessageType.DisplayRadar, value);

                isDisplayed = value;
            }
        }
        private bool isDisplayed;

        public bool ShouldBeep
        {
            get => shouldBeep;
            set
            {
                if (value == shouldBeep)
                {
                    return;
                }

                Script.SendMessage(MessageType.RadarBeep, value);

                shouldBeep = value;
            }
        }
        private bool shouldBeep;

        public IReadOnlyDictionary<string, Antenna> Antennas { get; }

        public Vehicle Vehicle { get; set; }

        public float FastLimit { get; set; }

        private readonly Script script;

        public Radar(Script script)
        {
            this.script = script;

            Antennas = new Dictionary<string, Antenna>()
            {
                { "front", new Antenna(this, "front", 0) },
                { "rear", new Antenna(this, "rear", 180) },
            };

            foreach (Antenna antenna in Antennas.Values)
            {
                antenna.FastLocked += OnFastLocked;
            }
        }

        public void Update()
        {
            if (!IsEnabled)
            {
                return;
            }

            Vehicle = Script.GetVehicleDriving(Game.PlayerPed);
            if (Vehicle == null)
            {
                return;
            }

            foreach (Antenna antenna in Antennas.Values)
            {
                if (!antenna.IsEnabled)
                {
                    continue;
                }

                antenna.Poll();
            }

            Script.SendMessage(MessageType.Heartbeat, new
            {
                speed = (uint)ConvertMetersToSpeed(script.Config.Units, Vehicle.Speed),
                antennas =
                    from a in Antennas.Values
                    where a.IsEnabled && a.Target != null
                    select new
                    {
                        name = a.Name,
                        speed = (uint)ConvertMetersToSpeed(script.Config.Units, a.Speed),
                        fast = (uint)ConvertMetersToSpeed(script.Config.Units, a.FastSpeed),
                        dir = a.TargetDirection,
                    }
            });
        }

        public void ResetFast()
        {
            foreach (Antenna antenna in Antennas.Values)
            {
                antenna.ResetFast();
            }
        }

        public static float ConvertMetersToSpeed(string units, float speed)
        {
            switch (units)
            {
                case "mph":
                    speed *= 2.237f;
                    break;
                case "km/h":
                    speed *= 3.6f;
                    break;
                default:
                    throw new NotSupportedException("units not supported");
            }
            return speed;
        }

        public static float ConvertSpeedToMeters(string units, float speed)
        {
            switch (units)
            {
                case "mph":
                    speed /= 2.237f;
                    break;
                case "km/h":
                    speed /= 3.6f;
                    break;
                default:
                    throw new NotSupportedException("units not supported");
            }
            return speed;
        }

        private void OnFastLocked(object sender, FastLockedEventArgs e)
        {
            if (ShouldBeep)
            {
                Audio.PlaySoundFrontend("Beep_Red", "DLC_HEIST_HACKING_SNAKE_SOUNDS");
            }
        }
    }
}
