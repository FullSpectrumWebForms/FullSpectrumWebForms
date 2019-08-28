using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSW.Core.AsyncLocks
{
    public interface IRequireAsyncReadOnlyLock
    {
        Task<IDisposable> EnterReadOnlyLock(CancellationToken token = default);
    }
    public interface IRequireAsyncLock : IRequireAsyncReadOnlyLock
    {
        Task<IDisposable> EnterLock(CancellationToken token = default);
    }
    public interface IRequireAnyLock
    {
        Task<IDisposable> EnterLock(CancellationToken token = default);
    }
    public interface IUnlockedAsyncServer : IRequireAsyncReadOnlyLock, IRequireAsyncLock, IRequireAnyLock
    {
        IRequireAnyLock AsReadOnlyLock();
        IRequireAnyLock AsLock();
    }

    internal class UnlockedAsyncServer : IUnlockedAsyncServer
    {
        private class ForcedAsyncLock : IRequireAnyLock
        {
            private readonly UnlockedAsyncServer UnlockedAsyncServer;
            private readonly bool ReadOnly;
            internal ForcedAsyncLock(UnlockedAsyncServer unlockedAsyncServer, bool readOnly)
            {
                UnlockedAsyncServer = unlockedAsyncServer;
                ReadOnly = readOnly;
            }

            public Task<IDisposable> EnterLock(CancellationToken token = default)
            {
                if (ReadOnly)
                    return UnlockedAsyncServer.EnterReadOnlyLock(token);
                else
                    return UnlockedAsyncServer.EnterLock(token);
            }
        }
        private readonly FSWPage Page;

        internal bool GotAsyncLocked { get; private set; } = false;

        internal UnlockedAsyncServer(FSWPage page)
        {
            Page = page;
        }

        public IRequireAnyLock AsLock()
        {
            return new ForcedAsyncLock(this, false);
        }

        public IRequireAnyLock AsReadOnlyLock()
        {
            return new ForcedAsyncLock(this, true);
        }

        public async Task<IDisposable> EnterLock(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(false, token);

            GotAsyncLocked = true;

            return pageLock;
        }

        public async Task<IDisposable> EnterReadOnlyLock(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(true, token);
            return pageLock;
        }
    }

}
