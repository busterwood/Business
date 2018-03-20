using BusterWood.Goodies;
using BusterWood.Logging;
using System;
using System.Transactions;

namespace SampleTests
{

    class CrossCuttingTransaction<T> : ICrossCutting<T> where T : struct, IConvertible // enum 
    {
        TransactionScope transaction;
        DateTime start;

        public T NextStep { get; private set; }

        /// <summary>create a new transaction per step</summary>
        public void OnStart(T step)
        {
            Log.Debug("Starting", new { step });
            start = DateTime.UtcNow;

            transaction = new TransactionScope(TransactionScopeOption.Required);
        }

        /// <summary>Store the next step until after the transaction commits in <see cref="OnEnd(Step)"/></summary>
        public void SetNextStep(T s)
        {
            NextStep = s;
        }

        /// <summary>commit each step</summary>
        public void OnEnd(T step)
        {
            transaction.Complete();
            transaction.Dispose();

            var elapsed = DateTime.UtcNow - start;
            Log.Info($"Finished in {elapsed.ToHuman()}", new { step });
        }

        /// <summary>Rollback transaction on step failure</summary>
        public void OnFailure(T step, Exception e)
        {
            transaction.Dispose();

            var elapsed = DateTime.UtcNow - start;
            Log.Error($"Failed in {elapsed.ToHuman()}, transaction rolled back", new { step, error = e });
        }

    }
}
