using Newtonsoft.Json;

namespace FitbitSNHP.Models
{
    public class LifetimeTotals
    {
        [JsonProperty("caloriesOut")]
        public int CaloriesOut { get; set; }

        [JsonProperty("distance")]
        public double Distance { get; set; }

        [JsonProperty("floors")]
        public int Floors { get; set; }

        [JsonProperty("steps")]
        public int Steps { get; set; }
    }
}