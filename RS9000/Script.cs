using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace RS9000
{
    internal class Script : BaseScript
    {
        private readonly Radar Radar;
        private readonly Controller controller;
        
        public Config Config { get; } 

        private bool IsDisplayingKeyboard { get; set; }

        public Script()
        {
            string configData = API.LoadResourceFile(API.GetCurrentResourceName(), "config.json");

            Config = Config.Base;
            JsonConvert.PopulateObject(configData, Config);
            Config.Validate();

            Radar = new Radar(this);
            controller = new Controller(this, Radar);

            Tick += Update;
            Tick += CheckInputs;
        }

        public void RegisterEventHandler(string eventName, Delegate callback)
        {
            EventHandlers[eventName] += callback;
        }

        public void RegisterNUICallback(string msg, Action<IDictionary<string, object>, CallbackDelegate> callback)
        {
            API.RegisterNuiCallbackType(msg);
            EventHandlers[$"__cfx_nui:{msg}"] += new Action<ExpandoObject, CallbackDelegate>((body, result) =>
            {
                callback?.Invoke(body, result);
            });
        }

        public static Vehicle GetVehicleDriving(Ped ped)
        {
            Vehicle v = ped.CurrentVehicle;
            bool driving = ped.SeatIndex == VehicleSeat.Driver;

            if (v == null || !driving || v.ClassType != VehicleClass.Emergency)
            {
                return null;
            }

            return v;
        }

        private bool InEmergencyVehicle => GetVehicleDriving(Game.PlayerPed) != null;

        private async Task CheckInputs()
        {
            if (API.IsPauseMenuActive())
            {
                if (Radar.IsDisplayed)
                {
                    Radar.IsDisplayed = false;
                }
                return;
            }

            if (Radar.IsDisplayed && !InEmergencyVehicle)
            {
                Radar.IsDisplayed = false;
            }
            else if (InEmergencyVehicle && !Radar.IsDisplayed && Radar.IsEnabled)
            {
                Radar.IsDisplayed = true;
            }

            bool modifierPressed = Config.Controls.OpenControlPanel.Modifier.HasValue &&
                Game.IsControlPressed(0, Config.Controls.OpenControlPanel.Modifier.Value);

            bool controlPressed = Game.IsControlJustPressed(0, Config.Controls.OpenControlPanel.Control.Value);

            if (modifierPressed && controlPressed && InEmergencyVehicle)
            {
                controller.Visible = !controller.Visible;
            }

            await Task.FromResult(0);
        }

        private async Task Update()
        {
            await Delay(10);

            Radar.Update();
        }

        public void ShowKeyboard(int limit, string text = "")
        {
            if (IsDisplayingKeyboard)
            {
                return;
            }

            API.DisplayOnscreenKeyboard(0, "", "", text, "", "", "", limit);

            Tick += KeyboardUpdate;
        }

        private Task KeyboardUpdate()
        {
            int status = API.UpdateOnscreenKeyboard();

            if (status == 1)
            {
                string result = API.GetOnscreenKeyboardResult();
                TriggerEvent("rs9000:_keyboardResult", result);
            }

            if (status == 1 || status == 2)
            {
                Tick -= KeyboardUpdate;
                IsDisplayingKeyboard = false;
            }

            return Task.FromResult(0);
        }

        public static void SendMessage(MessageType type, object data)
        {
            string json = JObject.FromObject(new { type, data }).ToString();
            API.SendNuiMessage(json);
        }

        
    }
}
