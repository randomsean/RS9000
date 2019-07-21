using System;
using System.Collections.Generic;
using System.Linq;

using CitizenFX.Core;

namespace RS9000
{
    internal class Radar
    {
        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (value != enabled)
                {
                    enabled = value;
                    Script.SendMessage(MessageType.RadarPower, value);
                    foreach (Antenna antenna in Antennas.Values)
                    {
                        antenna.Enabled = value;
                    }
                }
            }
        }

        private bool displayed;
        public bool Displayed
        {
            get => displayed;
            set
            {
                if (value != displayed)
                {
                    displayed = value;
                    Script.SendMessage(MessageType.DisplayRadar, displayed);
                }
            }
        }

        public readonly IReadOnlyDictionary<string, Antenna> Antennas = new Dictionary<string, Antenna>()
        {
            { "front", new Antenna("front", 0) },
            { "rear", new Antenna("rear", 180) },
        };

        private static uint ConvertSpeed(float speed)
        {
            switch (Script.Units)
            {
                case "mph":
                    speed *= 2.237f;
                    break;
                case "km/h":
                    speed *= 3.6f;
                    break;
            }

            return (uint)Math.Floor(speed);
        }

        public void Update()
        {
            if (!Enabled)
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
                if (!antenna.Enabled)
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
                speed = ConvertSpeed(v.Speed),
                antennas =
                    from a in Antennas.Values
                    where a.Enabled
                    select new
                    {
                        name = a.Name,
                        speed = ConvertSpeed(a.Speed),
                        fast = ConvertSpeed(a.FastSpeed),
                    }
            });
        }
    }
}
