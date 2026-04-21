namespace DongUtility
{
    /// <summary>
    /// A thread-safe random class
    /// Copied from Stack Overflow
    /// </summary>
    public static class ThreadSafeRandom
    {
        private static int seed = Environment.TickCount;

        private static readonly ThreadLocal<Random> random =
            new(() => new Random(Interlocked.Increment(ref seed)));

        public static Random Random()
        {
            return random.Value!; // Null-forgiving operator to keep the compiler quiet
        }
    }

}
