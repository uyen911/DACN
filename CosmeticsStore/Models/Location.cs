﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CosmeticsStore.Models
{
    public class Location
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Location(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}