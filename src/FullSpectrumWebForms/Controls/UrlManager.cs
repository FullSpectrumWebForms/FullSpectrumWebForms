﻿using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls
{
    public class UrlManager : ControlBase
    {
        public string Url { get; private set; }
        public IReadOnlyDictionary<string, string>? Parameters { get; private set; }


        internal UrlManager(string url, Dictionary<string, string> parameters)
        {
            Url = url;
            Parameters = parameters ?? new Dictionary<string, string>();
        }

        public override Task InitializeProperties()
        {
            return Task.CompletedTask;
        }

        public void UpdateParameter(string parameterName, string value, bool clearEverythingElse = false)
        {
            UpdateParameters(new Dictionary<string, string>
            {
                [parameterName] = value
            }, !clearEverythingElse);
        }

        public void UpdateParameters(Dictionary<string, string> parameters, bool mergeWithPreviousParameters = true)
        {
            var copyOfOldParameters = mergeWithPreviousParameters ? Parameters.ToDictionary(x => x.Key, x => x.Value) : new Dictionary<string, string>();
            foreach (var parameter in parameters)
                copyOfOldParameters[parameter.Key] = parameter.Value;
            UpdateUrlWithoutReloading(Url, copyOfOldParameters);
        }

        public void UpdateUrlWithoutReloading(string url, Dictionary<string, string>? parameters = null)
        {
            Url = url;
            Parameters = parameters;

            CallCustomClientEvent("updateUrlWithoutReloading", new
            {
                url,
                parameters
            });
        }

        public void UpdateUrlAndReload(string url, Dictionary<string, string>? parameters = null)
        {
            CallCustomClientEvent("redirect", new
            {
                url,
                parameters
            });
        }

        public void OpenNewTab(string url, Dictionary<string, string>? parameters = null)
        {
            CallCustomClientEvent("openNewTab", new
            {
                url,
                parameters
            });
        }
    }
}
