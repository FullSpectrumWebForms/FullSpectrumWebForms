using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSW.Core
{
    public interface IUnlockedAsyncServer
    {
        Task<IDisposable> EnterLock();
    }
    public class AsyncServerSideLock
    {
        private readonly object _lock = new object();




        internal void Free()
        {

        }
    }
}
