using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Controls
{
    public class CommonInformations : Core.ControlBase
    {
        public bool? IsMobile
        {
            get => GetProperty<bool?>(PropertyName());
            set => SetProperty(PropertyName(), value);
        }

        public override void InitializeProperties()
        {
            IsMobile = null;
        }

    }
}
