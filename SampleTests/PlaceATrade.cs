using BusterWood.Goodies;
using BusterWood.Logging;
using sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace SampleTests
{

    class Basket : IBasket
    {
        public int Id { get; set; }

        public IUser DecisionMaker => throw new NotImplementedException();

        public string HasOneOrMoreOrders => throw new NotImplementedException();
    }

    class PlaceATrade : PlaceTrade
    {
        Basket basket;
        TransactionScope transaction;
        DateTime start;

        public List<Step> finished = new List<Step>();

        public override bool Given(IBasket item)
        {
            basket = item as Basket;
            return basket != null;
        }

        protected override void OnCheckRestrictions()
        {
        }

        protected override void OnGroupOrdersByRestrictionsIntoTickets()
        {
        }

        protected override void OnLocateStockWhenSellingShort()
        {
        }

        protected override void OnPlaceTicketsInEms()
        {
        }

        public Exception ReservePositionException;

        protected override void OnReservePosition()
        {
            if (ReservePositionException != null)
                throw ReservePositionException;
        }

        /// <summary>
        /// new transaction per step
        /// </summary>
        protected override void OnStart(Step step)
        {
            Log.Info("Starting", new { step });
            start = DateTime.UtcNow;

            transaction = new TransactionScope(TransactionScopeOption.Required);
        }

        /// <summary>
        /// commit each step
        /// </summary>
        protected override void OnEnd(Step step)
        {
            transaction.Complete();
            transaction.Dispose();

            var elapsed = DateTime.UtcNow - start;
            Log.Info($"Finished in {elapsed.ToHuman()}", new { step });

            finished.Add(step);
        }

        /// <summary>
        /// Rollback transaction on step failure
        /// </summary>
        protected override void OnFailure(Step step, Exception e)
        {
            transaction.Dispose();

            var elapsed = DateTime.UtcNow - start;
            Log.Error($"Failed in {elapsed.ToHuman()}, transaction rolled back", new { step, error=e });
        }
    }
}
