using System;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using CitizenFX.Core;
using System.Runtime.Serialization;

namespace RS9000
{
    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class Config
    {
        [JsonProperty]
        public string Units { get; set; }

        [JsonProperty]
        public ControlListConfig Controls { get; set; }

        private static readonly string[] validUnits = new[] { "mph", "km/h" };

        public void Validate()
        {
            if (!validUnits.Contains(Units))
            {
                throw new ArgumentException($"units: '{Units}' is not a valid unit type");
            }

            if (Controls.OpenControlPanel.Control == null)
            {
                throw new ArgumentException("controls.openControlPanel: not a valid input type");
            }
        }

        public static readonly Config Base = new Config()
        {
            Units = "mph",
            Controls = new ControlListConfig()
            {
                OpenControlPanel = new ControlConfig(-1, (int)Control.VehicleDuck),
            },
        };
    }

    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class ControlListConfig
    {
        [JsonProperty]
        public ControlConfig OpenControlPanel { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    internal class ControlConfig
    {
        [JsonProperty("modifier")]
        public int ModifierIndex { get; set; } = -1;

        [JsonProperty("control")]
        public int ControlIndex { get; set; } = -1;

        public Control? Modifier { get; private set; }

        public Control? Control { get; private set;  }

        public ControlConfig()
        {
        }

        public ControlConfig(int modifier, int control)
        {
            ModifierIndex = modifier;
            ControlIndex = control;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            Modifier = ControlFromIndex(ModifierIndex);
            Control = ControlFromIndex(ControlIndex);
        }

        private static Control? ControlFromIndex(int index)
        {
            if (!Enum.IsDefined(typeof(Control), index))
            {
                return null;
            }
            return (Control)index;
        }
    }
}
