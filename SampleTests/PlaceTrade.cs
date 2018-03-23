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

        public IEnumerable<IOrder> Orders => throw new NotImplementedException();
    }


    class PlaceTrade : PlaceTradeProcess
    {
        Basket basket;
        CrossCuttingTransaction<Step> crossCutting = new CrossCuttingTransaction<Step>();

        public List<Step> finished = new List<Step>();

        public Context GetContext() => _context;

        protected override bool GivenABasket(IBasket item)
        {
            basket = item as Basket;
            return basket != null;
        }

        /// <summary>create a new transaction per step</summary>
        protected override void OnStart(Step step) => crossCutting.OnStart(step);

        /// <summary>commit each step</summary>
        protected override void OnEnd(Step step) => crossCutting.OnEnd(step);

        /// <summary>Rollback transaction on step failure</summary>
        protected override void OnFailure(Step step, Exception e) => crossCutting.OnFailure(step, e);


        protected override Context OnStarting()
        {
            return _context;
        }

        protected override Context OnCheckRestrictions()
        {
            return _context;
        }

        public Exception ReservePositionException;

        protected override Context OnReservePosition()
        {
            if (ReservePositionException != null)
                throw ReservePositionException;
            return _context;
        }

        protected override Context OnLocateStockWhenSellingShort()
        {
            return _context;
        }

        protected override Context OnGroupOrdersByRestrictionsIntoTickets()
        {
            return _context;
        }

        protected override Context OnPlaceTicketsInEms()
        {
            return _context;
        }

        protected override Context OnEnding()
        {
            return _context;
        }


    }


}
