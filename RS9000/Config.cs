using System;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace RS9000
{
    [JsonObject(MemberSerialization.OptIn, NamingStrategyType = typeof(CamelCaseNamingStrategy))]
    internal class Config
    {
        [JsonProperty(Required = Required.Always)]
        public string Units { get; set; }

        private static readonly string[] validUnits = new[] { "mph", "km/h" };

        public void Validate()
        {
            if (!validUnits.Contains(Units))
            {
                throw new ArgumentException($"units: '{Units}' is not a valid unit type");
            }
        }
    }
}
