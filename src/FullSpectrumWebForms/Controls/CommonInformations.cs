using FSW.Core.AsyncLocks;
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



        public async Task<GeoCoordinate> QueryGeoCoordinate(IUnlockedAsyncServer unlockedAsyncServer)
        {
            Task<GeoCoordinate> task;
            using (await unlockedAsyncServer.EnterLock())
                task = CallCustomClientEvent<GeoCoordinate>("queryGeoCoordinate");
            return await task;
        }

        public void QueryGeoCoordinate(IUnlockedAsyncServer unlockedAsyncServer, Action<GeoCoordinate> callback)
        {
            var task = QueryGeoCoordinate(unlockedAsyncServer);

            task.ContinueWith((geo) =>
            {
                callback(geo.Result);
            });
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
