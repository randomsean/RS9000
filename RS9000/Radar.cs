using System;
using System.Collections.Generic;
using System.Text;

using CitizenFX.Core;
using CitizenFX.Core.Native;

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
                    string v = value ? "true" : "false";
                    API.SendNuiMessage($"{{\"enabled\": {v}}}");
                }
            }
        }

        public Antenna[] Antennae { get; } = new Antenna[]
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

            List<string> data = new List<string>();

            foreach (Antenna antenna in Antennae)
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

                data.Add(antenna.ToJson());
            }

            StringBuilder b = new StringBuilder("{");

            b.Append($"\"speed\": {v.Velocity.Length()}, \"antennae\": [");

            for (int i = 0; i < data.Count; i++)
            {
                b.Append(data[i]);
                if (i != data.Count - 1)
                {
                    b.Append(',');
                }
            }

            b.Append("]}");

            API.SendNuiMessage(b.ToString());
        }
    }
}
