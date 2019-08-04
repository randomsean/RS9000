using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

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
            script.RegisterNUICallback("setFastLimit", new Func<IDictionary<string, object>, CallbackDelegate, Task>(SetFastLimit));

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
            if (!radar.IsEnabled)
            {
                return;
            }

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
            if (!radar.Antennas.TryGetValue(name, out Antenna antenna) || !antenna.IsEnabled)
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
            radar.ResetFast();
        }

        private async Task SetFastLimit(IDictionary<string, object> body, CallbackDelegate result)
        {
            Visible = false;
            string input = await Game.GetUserInput(Radar.MaxSpeed.ToString().Length);
            if (!uint.TryParse(input, out uint n) || n > Radar.MaxSpeed)
            {
                return;
            }
            radar.FastLimit = Radar.ConvertSpeedToMeters(script.Config.Units, n);
            radar.ResetFast();
            Screen.ShowSubtitle($"Fast limit set to ~y~{n} {script.Config.Units}");
        }
    }
}
