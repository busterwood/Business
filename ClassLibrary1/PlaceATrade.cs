using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
    }
}
