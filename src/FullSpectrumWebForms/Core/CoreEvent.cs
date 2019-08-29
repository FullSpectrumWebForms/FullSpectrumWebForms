using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FSW.Core
{
    [Obsolete("Consider using AsyncCoreEventAttribute")]
    public class CoreEventAttribute : Attribute
    {
    }
    public class AsyncCoreEventAttribute : Attribute
    {
    }
}