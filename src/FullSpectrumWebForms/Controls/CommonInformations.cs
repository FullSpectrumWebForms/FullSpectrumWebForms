using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls
{
    public class CommonInformations : Core.ControlBase
    {
        public class GeoCoordinate
        {
            public double Latitude;
            public double Longitude;
            public double? Altitude;
            public double? Accuracy;
            public double? Speed;
            public double? Heading;
        }
        public bool? IsMobile
        {
            get => GetProperty<bool?>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }



        public Task<GeoCoordinate> QueryGeoCoordinate()
        {
            return CallCustomClientEvent<GeoCoordinate>("queryGeoCoordinate");
        }

        public Task PerformLifeCycle()
        {
            return CallCustomClientEvent<bool>("performLifeCycle");
        }

        public void ConsoleLog(string message)
        {
            CallCustomClientEvent("consoleLog", new
            {
                message
            });
        }

        public void ConsoleError(string message)
        {
            CallCustomClientEvent("ConsoleError", new
            {
                message
            });
        }

        public override void InitializeProperties()
        {
            IsMobile = null;

        }

    }
}
