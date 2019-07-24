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

        private readonly Script script;
        private readonly Radar radar;

        public Controller(Script script, Radar radar)
        {
            script.RegisterNUICallback("close", new Action<IDictionary<string, object>, CallbackDelegate>((body, result) => { Visible = false;  }));

            script.RegisterNUICallback("radarPower", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleRadarPower));
            script.RegisterNUICallback("radarDisplay", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleRadarDisplay));
            script.RegisterNUICallback("antennaPower", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleAntennaPower));
            script.RegisterNUICallback("antennaMode", new Action<IDictionary<string, object>, CallbackDelegate>(SwitchAntennaMode));
            script.RegisterNUICallback("toggleBeep", new Action<IDictionary<string, object>, CallbackDelegate>(ToggleBeep));
            script.RegisterNUICallback("resetFast", new Action<IDictionary<string, object>, CallbackDelegate>(ResetFast));
            script.RegisterNUICallback("setFastLimit", new Action<IDictionary<string, object>, CallbackDelegate>(SetFastLimit));

            script.RegisterEventHandler("rs9000:_keyboardResult", new Action<string>(KeyboardResult));

            this.script = script;
            this.radar = radar; 
        }

        private void ToggleRadarPower(IDictionary<string, object> body, CallbackDelegate result)
        {
            radar.IsEnabled = !radar.IsEnabled;
        }

        private void ToggleRadarDisplay(IDictionary<string, object> body, CallbackDelegate result)
        {
            radar.IsDisplayed = !radar.IsDisplayed;
        }

        private void ToggleAntennaPower(IDictionary<string, object> body, CallbackDelegate result)
        {
            string name = (string)body["data"];
            if (!radar.Antennas.TryGetValue(name, out Antenna antenna))
            {
                return;
            }
            antenna.IsEnabled = !antenna.IsEnabled;
        }

        private void SwitchAntennaMode(IDictionary<string, object> body, CallbackDelegate result)
        {
            string name = (string)body["data"];
            if (!radar.Antennas.TryGetValue(name, out Antenna antenna))
            {
                return;
            }
            if (!antenna.IsEnabled)
            {
                return;
            }
            antenna.Mode = (antenna.Mode == AntennaMode.Same ? AntennaMode.Opposite : AntennaMode.Same);
        }

        private void ToggleBeep(IDictionary<string, object> body, CallbackDelegate result)
        {
            radar.ShouldBeep = !radar.ShouldBeep;
        }

        private void ResetFast(IDictionary<string, object> body, CallbackDelegate result)
        {
            foreach (Antenna antenna in radar.Antennas.Values)
            {
                antenna.ResetFast();
            }
        }

        private void SetFastLimit(IDictionary<string, object> body, CallbackDelegate result)
        {
            Visible = false;
            script.ShowKeyboard(Antenna.MaxSpeed.ToString().Length);
        }

        private void KeyboardResult(string result)
        {
            if (!uint.TryParse(result, out uint n) || n > Antenna.MaxSpeed)
            {
                return;
            }

            foreach (Antenna antenna in radar.Antennas.Values)
            {
                antenna.FastLimit = Script.ConvertSpeedToMeters(n);
                antenna.ResetFast();
            }
        }
    }
}
