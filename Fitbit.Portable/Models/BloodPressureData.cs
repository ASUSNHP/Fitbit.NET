﻿using System.Collections.Generic;

namespace FitbitSNHP.Models
{
    public class BloodPressureData
    {
        public BloodPressureAverage Average { get; set; }
        public List<BloodPressure> BP { get; set; }
    }
}
