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
            var taskCompletion = new TaskCompletionSource<GeoCoordinate>();
            Page.RegisterHostedService(() =>
            {
                Task<GeoCoordinate> task;
                using (Page.ServerSideLock)
                    task = CallCustomClientEvent<GeoCoordinate>("queryGeoCoordinate");

                task.Wait();
                taskCompletion.SetResult(task.Result);

            }, Core.FSWPage.HostedServicePriority.NewThread);
            return taskCompletion.Task;
        }

        public void QueryGeoCoordinate(Action<GeoCoordinate> callback)
        {
            var task = QueryGeoCoordinate();

            task.ContinueWith((geo) =>
            {
                callback(geo.Result);
            });
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
            var taskCompletion = new TaskCompletionSource<string>();
            Page.RegisterHostedService(() =>
            {
                Task<string> task;
                using (Page.ServerSideLock)
                    task = CallCustomClientEvent<string>("queryCookie", name);

                task.Wait();
                taskCompletion.SetResult(task.Result);
            });
            return taskCompletion.Task;
        }

        public void QueryCookie(string name, Action<string> callback)
        {
            var task = QueryCookie(name);
            task.ContinueWith(x =>
            {
                callback(x.Result);
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
