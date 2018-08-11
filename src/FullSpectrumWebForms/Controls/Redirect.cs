using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls
{
    public class Redirect : Core.ControlBase
    {

        public override void InitializeProperties()
        {
        }

        public void RedirectToUrl(string url)
        {
            CallCustomClientEvent("redirect", url);
        }
    }
}
