using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Newtonsoft.Json.Linq;

namespace RS9000
{
    internal class Script : BaseScript
    {
        private readonly Radar Radar = new Radar();
        private readonly Controller controller;

        public const string Units = "mph";

        public Script()
        {
            controller = new Controller(this, Radar);

            Tick += Update;
            Tick += CheckInputs;
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
            if (Radar.Displayed && !InEmergencyVehicle)
            {
                Radar.Displayed = false;
            }
            else if (InEmergencyVehicle && !Radar.Displayed && Radar.Enabled)
            {
                Radar.Displayed = true;
            }

            if (Game.IsControlJustPressed(0, Control.VehicleDuck) && InEmergencyVehicle)
            {
                controller.Visible = !controller.Visible;
            }

            await Delay(0);
        }

        private async Task Update()
        {
            await Delay(10);

            Radar.Update();
        }

        public static void SendMessage(MessageType type, object data)
        {
            string json = JObject.FromObject(new { type, data }).ToString();
            API.SendNuiMessage(json);
        }
    }
}
