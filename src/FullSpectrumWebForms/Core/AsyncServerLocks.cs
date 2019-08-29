using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FSW.Core.AsyncLocks
{
    public interface IRequireAsyncReadOnlyLock
    {
        Nito.AsyncEx.AwaitableDisposable<IDisposable> EnterReadOnlyLock(CancellationToken token = default);
    }
    public interface IRequireAsyncLock : IRequireAsyncReadOnlyLock
    {
        Nito.AsyncEx.AwaitableDisposable<IAsyncLock> EnterLock(CancellationToken token = default);
    }
    public interface IRequireAnyLock
    {
        Nito.AsyncEx.AwaitableDisposable<IDisposable> EnterAnyLock(CancellationToken token = default);
    }
    public interface IUnlockedAsyncServer : IRequireAsyncReadOnlyLock, IRequireAsyncLock, IRequireAnyLock
    {
        IRequireAnyLock AsReadOnlyLock();
        IRequireAnyLock AsLock();

        Task Synchronize();
    }
    public interface IAsyncLock : IDisposable
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

            private async Task<IDisposable> EnterAnyLock_(CancellationToken token)
            {
                if (ReadOnly)
                    return await UnlockedAsyncServer.EnterReadOnlyLock(token);
                else
                    return await UnlockedAsyncServer.EnterLock(token);
            }
            public Nito.AsyncEx.AwaitableDisposable<IDisposable> EnterAnyLock(CancellationToken token = default)
            {
                return new Nito.AsyncEx.AwaitableDisposable<IDisposable>(EnterAnyLock_(token));
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

        private async Task<IDisposable> EnterNonExclusiveReadOnlyLock_(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(true, token, true);
            return pageLock;
        }
        public Nito.AsyncEx.AwaitableDisposable<IDisposable> EnterNonExclusiveReadOnlyLock(CancellationToken token = default)
        {
            return new Nito.AsyncEx.AwaitableDisposable<IDisposable>(EnterNonExclusiveReadOnlyLock(token));
        }

        private async Task<IDisposable> EnterReadOnlyLock_(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(true, token);
            return pageLock;
        }
        public Nito.AsyncEx.AwaitableDisposable<IDisposable> EnterReadOnlyLock(CancellationToken token = default)
        {
            return new Nito.AsyncEx.AwaitableDisposable<IDisposable>(EnterReadOnlyLock_(token));
        }

        public async Task<IAsyncLock> EnterLock_(CancellationToken token = default)
        {
            var pageLock = new FSWPage.PageLock(Page);
            await pageLock.AsyncAcquireLock(false, token);

            GotAnyLocked = false;

            return new AsyncServerLock(pageLock);
        }
        public Nito.AsyncEx.AwaitableDisposable<IAsyncLock> EnterLock(CancellationToken token = default)
        {
            return new Nito.AsyncEx.AwaitableDisposable<IAsyncLock>(EnterLock_(token));
        }

        public Nito.AsyncEx.AwaitableDisposable<IDisposable> EnterAnyLock(CancellationToken token = default)
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
