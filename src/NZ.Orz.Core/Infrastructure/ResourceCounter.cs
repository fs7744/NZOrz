using System.Diagnostics;

namespace NZ.Orz.Infrastructure;

public abstract class ResourceCounter
{
    public abstract bool TryLockOne();

    public abstract void ReleaseOne();

    public static ResourceCounter Unlimited { get; } = new UnlimitedCounter();

    public static ResourceCounter Quota(long amount) => new FiniteCounter(amount);

    private sealed class UnlimitedCounter : ResourceCounter
    {
        public override bool TryLockOne() => true;

        public override void ReleaseOne()
        {
        }
    }

    internal sealed class FiniteCounter : ResourceCounter
    {
        private readonly long _max;
        private long _count;

        public FiniteCounter(long max)
        {
            if (max < 0)
            {
                throw new ArgumentOutOfRangeException("Value must be null or a non-negative number.");
            }

            _max = max;
        }

        public override bool TryLockOne()
        {
            var count = _count;

            while (count < _max && count != long.MaxValue)
            {
                var prev = Interlocked.CompareExchange(ref _count, count + 1, count);
                if (prev == count)
                {
                    return true;
                }

                count = prev;
            }

            return false;
        }

        public override void ReleaseOne()
        {
            Interlocked.Decrement(ref _count);

            Debug.Assert(_count >= 0, "Resource count is negative. More resources were released than were locked.");
        }

        internal long Count
        {
            get => _count;
            set => _count = value;
        }
    }
}