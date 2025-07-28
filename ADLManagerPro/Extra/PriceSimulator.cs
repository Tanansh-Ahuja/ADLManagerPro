using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ADLManagerPro
{

    public static class PriceSimulator
    {
        private static CancellationTokenSource _cts;
        private static readonly ConcurrentDictionary<string, double> _latest = new ConcurrentDictionary<string, double>();

        // (contractId, price)
        public static event Action<string, double> PriceChanged;

        public static void Start(List<string> feedNames, double min = 67.6, double max = 69.7, int intervalMs = 10)
        {
            _cts = new CancellationTokenSource();

            foreach (var feedName in feedNames)
            {
                _ = Task.Run(() => Produce(feedName, min, max, intervalMs, _cts.Token));
            }
        }

        public static void Stop() => _cts?.Cancel();

        public static double GetLatest(string feedName) =>
            _latest.TryGetValue(feedName, out var v) ? v : double.NaN;

        private static async Task Produce(string feedName, double min, double max, int intervalMs, CancellationToken token)
        {
            var rnd = new Random();
            double range = max - min;

            while (!token.IsCancellationRequested)
            {
                double v = min + rnd.NextDouble() * range;
                _latest[feedName] = v;
                PriceChanged?.Invoke(feedName, v);
                await Task.Delay(intervalMs, token);
            }
        }
    }
}
