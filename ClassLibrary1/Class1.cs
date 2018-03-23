using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ClassLibrary1
{
    using System;

    interface IMessaging
    {

    }

    internal interface IProcess<TStep, TContext> 
        where TStep : struct, IConvertible // enum
        where TContext : class
    {
        TStep Step { get; set; }
        TContext Context { get; set; }

    }

    internal class Transition<TStep, TContext> : IDisposable
        where TStep : struct, IConvertible // enum
        where TContext : class
    {
        IProcess<TStep, TContext> process;
        TStep step;
        TContext context;
        bool complete;

        public Transition(IProcess<TStep, TContext> process)
        {
            this.process = process;
            this.step = process.Step;
            this.context = process.Context;
            this.complete = false;
        }

        public void Complete()
        {
            complete = true;
        }

        public void Dispose()
        {
            if (!complete)
            {
                process.Context = this.context;
                process.Step = this.step;
            }
        }
    }

    public abstract class PlaceTradeProcess : IProcess<PlaceTradeProcess.Step, PlaceTradeProcess.Context>
    {
        protected Step _step;

        protected Context _context;

        public enum Step
        {
            Starting,
            CheckRestrictions,
            ReservePosition,
            LocateStockWhenSellingShort,
            GroupOrdersByRestrictionsIntoTickets,
            PlaceTicketsInEms,
            Finished,
        }

        public partial class Context
        {
            public Exception Failure { get; set; }

            public bool Failed { get { return Failure != null; } }

        }

        Step IProcess<PlaceTradeProcess.Step, PlaceTradeProcess.Context>.Step { get => _step; set => _step = value; }

        Context IProcess<PlaceTradeProcess.Step, PlaceTradeProcess.Context>.Context { get => _context; set => _context = value; }


        /// <summary>given a basket on line 52</summary>
        public bool Given(IBasket item) { return GivenABasket(item); }

        // TODO: set context from this basket
        protected abstract bool GivenABasket(IBasket item);

        public async Task Execute()
        {
            _context.Failure = null;
            try
            {
                await OnExecute();
            }
            catch (Exception e)
            {
                _context.Failure = e;
                await OnFailure(_step, e);
            }
        }

        private async Task OnExecute()
        {
            await Starting();
            await CheckRestrictions();
            await ReservePosition();
            await LocateStockWhenSellingShort();
            await GroupOrdersByRestrictionsIntoTickets();
            await PlaceTicketsInEms();
            await Finished();
        }

        private async Task Starting()
        {
            if (_step != Step.Starting) return;
            await OnStart(Step.Starting);
            _step = Step.CheckRestrictions;
            await OnEnd(Step.Starting);
        }

        private async Task CheckRestrictions()
        {
            if (_step != Step.CheckRestrictions) return;
            using (var tr = new Transition<Step, Context>(this))
            {
                using (var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await OnStart(Step.CheckRestrictions);
                    _context = await OnCheckRestrictions(_context);
                    _step = Step.ReservePosition;
                    await OnEnd(Step.CheckRestrictions);
                    txn.Complete();
                }
                tr.Complete();
            }
        }

        protected abstract Task<Context> OnCheckRestrictions(Context context);


        private async Task ReservePosition()
        {
            if (_step != Step.ReservePosition) return;
            OnStart(Step.ReservePosition);
            OnReservePosition();
            SetNextStep(Step.LocateStockWhenSellingShort);
            OnEnd(Step.ReservePosition);
        }

        protected abstract Task OnReservePosition();

        private async Task LocateStockWhenSellingShort()
        {
            if (_step != Step.LocateStockWhenSellingShort) return;
            OnStart(Step.LocateStockWhenSellingShort);
            OnLocateStockWhenSellingShort();
            SetNextStep(Step.GroupOrdersByRestrictionsIntoTickets);
            OnEnd(Step.LocateStockWhenSellingShort);
        }

        protected abstract Task OnLocateStockWhenSellingShort();

        private async Task GroupOrdersByRestrictionsIntoTickets()
        {
            if (_step != Step.GroupOrdersByRestrictionsIntoTickets) return;
            OnStart(Step.GroupOrdersByRestrictionsIntoTickets);
            OnGroupOrdersByRestrictionsIntoTickets();
            SetNextStep(Step.PlaceTicketsInEms);
            OnEnd(Step.GroupOrdersByRestrictionsIntoTickets);
        }

        protected abstract Task OnGroupOrdersByRestrictionsIntoTickets();

        private async Task PlaceTicketsInEms()
        {
            if (_step != Step.PlaceTicketsInEms) return;
            OnStart(Step.PlaceTicketsInEms);
            OnPlaceTicketsInEms();
            SetNextStep(Step.Finished);
            OnEnd(Step.PlaceTicketsInEms);
        }

        protected abstract Task OnPlaceTicketsInEms();

        private async Task Finished()
        {
            if (_step != Step.Finished) return;
            OnStart(Step.Finished);
            OnFinished();
            OnEnd(Step.Finished);
        }

        protected virtual async Task SetNextStep(Step s) { _step = s; }

        protected virtual async Task OnStarting() { }

        protected virtual async Task OnFinished() { }

        protected virtual async Task OnStart(Step step) { }

        protected virtual async Task OnEnd(Step step) { }

        protected virtual async Task OnFailure(Step step, Exception e) { }


    }

}
