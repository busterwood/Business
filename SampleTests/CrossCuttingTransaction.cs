using BusterWood.Goodies;
using BusterWood.Logging;
using System;
using System.Transactions;

namespace SampleTests
{

    class CrossCuttingTransaction<T> : ICrossCutting<T> where T : struct, IConvertible // enum 
    {
        DateTime start;

        /// <summary>create a new transaction per step</summary>
        public void OnStart(T step)
        {
            Log.Debug("Starting", new { step });
            start = DateTime.UtcNow;
        }

        /// <summary>commit each step</summary>
        public void OnEnd(T step)
        {
            var elapsed = DateTime.UtcNow - start;
            Log.Info($"Finished in {elapsed.ToHuman()}", new { step });
        }

        /// <summary>Rollback transaction on step failure</summary>
        public void OnFailure(T step, Exception e)
        {
            var elapsed = DateTime.UtcNow - start;
            Log.Error($"Failed in {elapsed.ToHuman()}, transaction rolled back", new { step, error = e });
        }

    }
}
