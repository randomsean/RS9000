using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

namespace RS9000
{
    internal class Radar
    {
        private bool isEnabled;
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value != isEnabled)
                {
                    isEnabled = value;
                    Script.SendMessage(MessageType.RadarPower, value);
                    foreach (Antenna antenna in Antennas.Values)
                    {
                        antenna.IsEnabled = value;
                    }
                }
            }
        }

        private bool isDisplayed;
        public bool IsDisplayed
        {
            get => isDisplayed;
            set
            {
                if (value != isDisplayed)
                {
                    isDisplayed = value;
                    Script.SendMessage(MessageType.DisplayRadar, isDisplayed);
                }
            }
        }

        private bool shouldBeep;
        public bool ShouldBeep
        {
            get => shouldBeep;
            set
            {
                if (value != shouldBeep)
                {
                    shouldBeep = value;
                    Script.SendMessage(MessageType.RadarBeep, value);
                }
            }
        }

        public readonly IReadOnlyDictionary<string, Antenna> Antennas = new Dictionary<string, Antenna>()
        {
            { "front", new Antenna("front", 0) },
            { "rear", new Antenna("rear", 180) },
        };

        private readonly Script script;

        public Radar(Script script)
        {
            this.script = script;

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

            Vehicle v = Script.GetVehicleDriving(Game.PlayerPed);
            if (v == null)
            {
                return;
            }

            foreach (Antenna antenna in Antennas.Values)
            {
                if (!antenna.IsEnabled)
                {
                    continue;
                }

                antenna.Source = v;

                bool updated = antenna.Poll();
                if (!updated)
                {
                    continue;
                }
            }

            Script.SendMessage(MessageType.Heartbeat, new
            {
                speed = (uint)Script.ConvertMetersToSpeed(script.Config.Units, v.Speed),
                antennas =
                    from a in Antennas.Values
                    where a.IsEnabled && a.Target != null
                    select new
                    {
                        name = a.Name,
                        speed = (uint)Script.ConvertMetersToSpeed(script.Config.Units, a.Speed),
                        fast = (uint)Script.ConvertMetersToSpeed(script.Config.Units, a.FastSpeed),
                        dir = a.TargetDirection,
                    }
            });
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
