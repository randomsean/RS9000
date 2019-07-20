using System;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RS9000
{
    internal class Script : BaseScript
    {
        private readonly Radar Radar = new Radar();

        public Script()
        {
            Tick += Update;
            Tick += CheckInputs;
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
            if (Radar.Enabled && !InEmergencyVehicle)
            {
                Radar.Enabled = false;
            }
            else if (InEmergencyVehicle && !Radar.Enabled)
            {
                Radar.Enabled = true;
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
