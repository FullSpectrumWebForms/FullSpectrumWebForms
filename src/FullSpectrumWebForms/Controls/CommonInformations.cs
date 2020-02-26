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

        public void SetCookie(string name, string value)
        {
            CallCustomClientEvent("setCookie", new
            {
                name,
                value
            });
        }

        public Task<string> QueryCookie(string name)
        {
            return CallCustomClientEvent<string>("queryCookie", name);
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

        public override Task InitializeProperties()
        {
            IsMobile = null;

            return Task.CompletedTask;
        }

    }
}
