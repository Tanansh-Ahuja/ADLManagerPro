using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ADLManagerPro
{
    public static class PriceSimulator
    {
        public static CancellationTokenSource _cts;
        public static double _latestValue; // holds the latest generated value
        public static readonly object _lock = new object();

        //Neon feed connected
        public static void StartPriceGenerator()
        {
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                var rnd = new Random();
                const double min = 67.6, max = 69.7;
                double range = max - min;

                while (!_cts.Token.IsCancellationRequested)
                {
                    double val = min + rnd.NextDouble() * range;
                    lock (_lock)
                    {
                        _latestValue = val;
                    }
                    await Task.Delay(10, _cts.Token); // Generate every 10ms
                }
            }, _cts.Token);
        }

        public static void StopPriceGenerator()
        {
            _cts?.Cancel();
        }

        public static double GetLatestValue()
        {
            lock (_lock)
            {
                return _latestValue;
            }
        }
    }
}
