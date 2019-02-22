﻿using System;
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

        public override void InitializeProperties()
        {
            IsMobile = null;
        }

    }
}
