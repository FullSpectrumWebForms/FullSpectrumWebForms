using FSW.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Controls
{
    public class LoadingScreen : ControlBase
    {
        public LoadingScreen(FSWPage page = null) : base(page)
        {
        }
        public override void InitializeProperties()
        {
        }
        int NbLoadingScreenShowned = 0;
        string LastMessage;
        public void Show(string message = null)
        {
            ++NbLoadingScreenShowned;
            if (NbLoadingScreenShowned == 1 || LastMessage != message)
            {
                LastMessage = message;
                CallCustomClientEvent("showLoadingScreen", message);
            }
        }
        public void Hide()
        {
            --NbLoadingScreenShowned;
            if (NbLoadingScreenShowned == 0)
                CallCustomClientEvent("hideLoadingScreen");
            else if (NbLoadingScreenShowned < 0)
                NbLoadingScreenShowned = 0;
        }
    }
}
