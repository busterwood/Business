using BusterWood.Goodies;
using BusterWood.Logging;
using sample;
using System;
using System.Collections.Generic;
using System.Transactions;

namespace SampleTests
{

    class Basket : IBasket
    {
        public int Id { get; set; }

        public IUser DecisionMaker => throw new NotImplementedException();

        public string HasOneOrMoreOrders => throw new NotImplementedException();
    }


    class PlaceTrade : PlaceTradeProcess
    {
        Basket basket;
        CrossCuttingTransaction<Step> crossCutting = new CrossCuttingTransaction<Step>();

        public List<Step> finished = new List<Step>();

        protected override bool GivenABasket(IBasket item)
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

        /// <summary>create a new transaction per step</summary>
        protected override void OnStart(Step step) => crossCutting.OnStart(step);

        /// <summary>Store the next step until after the transaction commits in <see cref="OnEnd(Step)"/></summary>
        protected override void SetNextStep(Step s) => crossCutting.SetNextStep(s);

        /// <summary>commit each step</summary>
        protected override void OnEnd(Step step)
        {
            crossCutting.OnEnd(step);
            finished.Add(step);
            base.SetNextStep(crossCutting.NextStep);
        }

        /// <summary>Rollback transaction on step failure</summary>
        protected override void OnFailure(Step step, Exception e) => crossCutting.OnFailure(step, e);
    }

    
}
