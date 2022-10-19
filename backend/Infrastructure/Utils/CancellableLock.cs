using System;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Utils;

// https://www.rocksolidknowledge.com/articles/locking-and-asyncawait
public class CancellableLock
{
    private readonly SemaphoreSlim _guard;

    public CancellableLock() =>
        _guard = new SemaphoreSlim(1, 1);

    public async Task<LockReleaser> Lock(CancellationToken token)
    {
        await _guard.WaitAsync(token);
        return new LockReleaser(_guard);
    }

    public struct LockReleaser : IDisposable
    {
        private readonly SemaphoreSlim guard;

        public LockReleaser(SemaphoreSlim guard) =>
            this.guard = guard;

        public void Dispose() =>
            guard.Release();
    }
}