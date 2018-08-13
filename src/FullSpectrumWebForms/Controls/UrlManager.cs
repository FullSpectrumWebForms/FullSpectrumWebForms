using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls
{
    public class UrlManager : ControlBase
    {
        public string Url { get; private set; }
        public IReadOnlyDictionary<string, string> Parameters { get; private set; }


        internal UrlManager(string url, Dictionary<string, string> parameters)
        {
            Url = url;
            Parameters = parameters ?? new Dictionary<string, string>();
        }

        public override void InitializeProperties()
        {

        }
        public void UpdateParameter(string parameterName, string value)
        {
            UpdateParameters(new Dictionary<string, string>
            {
                [parameterName] = value
            }, true);
        }
        public void UpdateParameters(Dictionary<string, string> parameters, bool mergeWithPreviousParameters)
        {
            var copyOfOldParameters = mergeWithPreviousParameters ? Parameters.ToDictionary(x => x.Key, x => x.Value) : new Dictionary<string, string>();
            foreach (var parameter in parameters)
                copyOfOldParameters[parameter.Key] = parameter.Value;
            UpdateUrlWithoutReloading(Url, copyOfOldParameters);
        }
        public void UpdateUrlWithoutReloading(string url, Dictionary<string, string> parameters = null)
        {
            Url = url;
            Parameters = parameters;

            CallCustomClientEvent("updateUrlWithoutReloading", new
            {
                url,
                parameters
            });
        }
        public void UpdateUrlAndReload(string url)
        {
            CallCustomClientEvent("redirect", url);
        }
    }
}
