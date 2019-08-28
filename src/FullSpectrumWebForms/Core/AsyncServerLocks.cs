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
        Task<IAsyncLock> EnterLock(CancellationToken token = default);
    }
    public interface IRequireAnyLock
    {
        Task<IDisposable> EnterAnyLock(CancellationToken token = default);
    }
    public interface IUnlockedAsyncServer : IRequireAsyncReadOnlyLock, IRequireAsyncLock, IRequireAnyLock
    {
        IRequireAnyLock AsReadOnlyLock();
        IRequireAnyLock AsLock();

        Task Synchronize();
    }
    public interface IAsyncLock: IDisposable
    {
        void DemoteToReadOnlyLock();
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

            public async Task<IDisposable> EnterAnyLock(CancellationToken token = default)
            {
                if (ReadOnly)
                    return await UnlockedAsyncServer.EnterReadOnlyLock(token);
                else
                    return await UnlockedAsyncServer.EnterLock(token);
            }
        }
        private class AsyncServerLock : IAsyncLock
        {
            private readonly FSWPage.PageLock PageLock;

            public AsyncServerLock(FSWPage.PageLock pageLock)
            {
                PageLock = pageLock;
            }

            public void DemoteToReadOnlyLock()
            {
                PageLock.IsReadOnly = true;
            }

            public void Dispose()
            {
                PageLock.Dispose();
            }
        }
        private readonly FSWPage Page;

        internal bool GotAnyLocked { get; private set; } = false;

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


        public async Task<IDisposable> EnterReadOnlyLock(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(true, token);
            return pageLock;
        }

        public async Task<IAsyncLock> EnterLock(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(false, token);

            GotAnyLocked = false;

            return new AsyncServerLock(pageLock);
        }

        public Task<IDisposable> EnterAnyLock(CancellationToken token = default)
        {
            GotAnyLocked = true;

            return EnterReadOnlyLock(token);
        }

        public async Task Synchronize()
        {
            using (await EnterLock()) // this will send the infos to the client
            { }
        }
    }

}
