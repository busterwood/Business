using BusterWood.Testing;
using System;
using System.Linq;

namespace SampleTests
{

    public class PlaceTradeTests
    {
        PlaceATrade bp = new PlaceATrade();

        public void given_a_basket_returns_true(Test t)
        {
            t.Assert(() => bp.Given(new Basket { Id = 1 }));
        }

        public void given_a_basket_returns_false_when_not_passed_a_basket(Test t)
        {
            t.AssertNot(() => bp.Given(null));
        }

        public void execute(Test t)
        {
            bp.Given(new Basket { Id = 1 });
            bp.Execute();
            t.AssertNot(() => bp.Failed);
        }

        public void execute_fails_if_exception_thrown(Test t)
        {
            bp.ReservePositionException = new Exception("whoops");
            bp.Given(new Basket { Id = 1 });
            bp.Execute();
            t.Assert(() => bp.Failed);
            t.AssertNot(() => bp.finished.Last() == sample.PlaceTrade.Step.PlaceTicketsInEms);
        }

        public void can_retry_execute_to_retry_the_same_step(Test t)
        {
            bp.ReservePositionException = new Exception("whoops");
            bp.Given(new Basket { Id = 1 });
            bp.Execute();
            t.Assert(() => bp.Failed);
            t.AssertNot(() => bp.finished.Last() == sample.PlaceTrade.Step.PlaceTicketsInEms);

            // now try final step again
            bp.ReservePositionException = null;
            bp.Execute();
            t.AssertNot(() => bp.Failed);
            t.Assert(() => bp.finished.Last() == sample.PlaceTrade.Step.PlaceTicketsInEms);
        }
    }
}
