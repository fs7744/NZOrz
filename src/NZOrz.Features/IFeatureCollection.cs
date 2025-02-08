using System.Runtime.CompilerServices;

namespace NZOrz.Features
{
    public interface IFeatureCollection
    {
        private const int DefaultInitialCapacity = 16;
        private static volatile int typeLastIndex = -1;

        private static class TypeSlot<T>
        {
            internal static readonly int Index = Interlocked.Increment(ref typeLastIndex);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int GetIndex<TKey>() => TypeSlot<TKey>.Index;

        internal static int RecommendedCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var capacity = typeLastIndex + 1;

                if (capacity < DefaultInitialCapacity)
                {
                    capacity = DefaultInitialCapacity;
                }
                else
                {
                    capacity <<= 1;
                    if ((uint)capacity > (uint)Array.MaxLength)
                        capacity = Array.MaxLength;
                }

                return capacity;
            }
        }

        object? this[Type key] { get; set; }

        TFeature? Get<TFeature>();

        void Set<TFeature>(TFeature? instance);
    }

    public class FeatureCollection : IFeatureCollection
    {
        public object? this[Type key] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TFeature? Get<TFeature>()
        {
            throw new NotImplementedException();
        }

        public void Set<TFeature>(TFeature? instance)
        {
            throw new NotImplementedException();
        }
    }
}