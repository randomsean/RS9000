using System;
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
                    Script.SendMessage(MessageType.DisplayRadar, enabled);
                }
            }
        }

        public Antenna[] Antennas { get; } = new Antenna[]
        {
            new Antenna("front", 0),
            new Antenna("rear", 180),
        };

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

            float patrol = v.Velocity.Length();

            foreach (Antenna antenna in Antennas)
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
                patrol,
                antennas =
                    from a in Antennas
                    select new
                    {
                        name = a.Name,
                        speed = a.Speed,
                        fast = a.FastSpeed,
                    }
            });
        }
    }
}
