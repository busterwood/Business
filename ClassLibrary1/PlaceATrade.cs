using BusterWood.Goodies;
using BusterWood.Logging;
using sample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ClassLibrary1
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

        public override bool Given(IBasket item)
        {
            basket = item as Basket;
            return basket != null;
        }

        protected override void OnCheckRestrictions()
        {
            throw new NotImplementedException();
        }

        protected override void OnGroupOrdersByRestrictionsIntoTickets()
        {
            throw new NotImplementedException();
        }

        protected override void OnLocateStockWhenSellingShort()
        {
            throw new NotImplementedException();
        }

        protected override void OnPlaceTicketsInEms()
        {
            throw new NotImplementedException();
        }

        protected override void OnReservePosition()
        {
            throw new NotImplementedException();
        }

        protected override void OnStart(Step step)
        {
            transaction = new TransactionScope(TransactionScopeOption.Required);
            Log.Info("Starting", new { step });
            start = DateTime.UtcNow;
        }

        protected override void OnEnd(Step step)
        {
            transaction.Complete();
            transaction.Dispose();

            var elapsed = DateTime.UtcNow - start;
            Log.Info($"Finished in {elapsed.ToHuman()}", new { step });
        }

        protected override void OnFailure(Step step, Exception e)
        {
            transaction.Dispose();

            var elapsed = DateTime.UtcNow - start;
            Log.Error($"Failed in {elapsed.ToHuman()}, transaction rolled back", new { step, error=e });
        }
    }
}
