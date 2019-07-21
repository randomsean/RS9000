using System;
using System.Collections.Generic;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace RS9000
{
    internal class Controller
    {
        private bool visible;
        public bool Visible
        {
            get => visible;
            set
            {
                if (value == visible)
                {
                    return;
                }
                visible = value;
                Script.SendMessage(MessageType.DisplayControl, visible);
                API.SetNuiFocus(value, value);
            }
        }

        private readonly Radar radar;

        public Controller(Script script, Radar radar)
        {
            script.RegisterNUICallback("close", new Action<IDictionary<string, object>, CallbackDelegate>((body, result) => { Visible = false;  }));

            script.RegisterNUICallback("radarPower", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleRadarPower));
            script.RegisterNUICallback("radarDisplay", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleRadarDisplay));
            script.RegisterNUICallback("antennaPower", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleAntennaPower));
            script.RegisterNUICallback("antennaMode", new Action<IDictionary<string, object>, CallbackDelegate>(SwitchAntennaMode));
            script.RegisterNUICallback("resetFast", new Action<IDictionary<string, object>, CallbackDelegate>(ResetFast));

            this.radar = radar; 
        }

        private void ToggleRadarPower(IDictionary<string, object> body, CallbackDelegate result)
        {
            radar.Enabled = !radar.Enabled;
        }

        private void ToggleRadarDisplay(IDictionary<string, object> body, CallbackDelegate result)
        {
            radar.Displayed = !radar.Displayed;
        }

        private void ToggleAntennaPower(IDictionary<string, object> body, CallbackDelegate result)
        {
            string name = (string)body["data"];
            if (!radar.Antennas.TryGetValue(name, out Antenna antenna))
            {
                return;
            }
            antenna.Enabled = !antenna.Enabled;
        }

        private void SwitchAntennaMode(IDictionary<string, object> body, CallbackDelegate result)
        {
            string name = (string)body["data"];
            if (!radar.Antennas.TryGetValue(name, out Antenna antenna))
            {
                return;
            }
            if (!antenna.Enabled)
            {
                return;
            }
            antenna.Mode = (antenna.Mode == AntennaMode.Same ? AntennaMode.Opposite : AntennaMode.Same);
        }

        private void ResetFast(IDictionary<string, object> body, CallbackDelegate result)
        {
            foreach (Antenna antenna in radar.Antennas.Values)
            {
                antenna.FastLocked = false;
            }
        }
    }
}
