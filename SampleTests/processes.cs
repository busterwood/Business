using System;
using System.Transactions;
namespace sample
{

public abstract partial class PlaceTradeProcess: IProcess<PlaceTradeProcess.Step, PlaceTradeProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		CheckRestrictions,
		ReservePosition,
		LocateStockWhenSellingShort,
		GroupOrdersByRestrictionsIntoTickets,
		PlaceTicketsInEms,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a basket on line 52</summary>
	public bool Given(IBasket item) { return GivenABasket(item); }

	protected abstract bool GivenABasket(IBasket item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		CheckRestrictions();
		ReservePosition();
		LocateStockWhenSellingShort();
		GroupOrdersByRestrictionsIntoTickets();
		PlaceTicketsInEms();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.CheckRestrictions;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void CheckRestrictions()
	{
		if (_step != Step.CheckRestrictions) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.CheckRestrictions);
				_context = OnCheckRestrictions();
				_step = Step.ReservePosition;
				OnEnd(Step.CheckRestrictions);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnCheckRestrictions();

	private void ReservePosition()
	{
		if (_step != Step.ReservePosition) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.ReservePosition);
				_context = OnReservePosition();
				_step = Step.LocateStockWhenSellingShort;
				OnEnd(Step.ReservePosition);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnReservePosition();

	private void LocateStockWhenSellingShort()
	{
		if (_step != Step.LocateStockWhenSellingShort) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.LocateStockWhenSellingShort);
				_context = OnLocateStockWhenSellingShort();
				_step = Step.GroupOrdersByRestrictionsIntoTickets;
				OnEnd(Step.LocateStockWhenSellingShort);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnLocateStockWhenSellingShort();

	private void GroupOrdersByRestrictionsIntoTickets()
	{
		if (_step != Step.GroupOrdersByRestrictionsIntoTickets) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.GroupOrdersByRestrictionsIntoTickets);
				_context = OnGroupOrdersByRestrictionsIntoTickets();
				_step = Step.PlaceTicketsInEms;
				OnEnd(Step.GroupOrdersByRestrictionsIntoTickets);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnGroupOrdersByRestrictionsIntoTickets();

	private void PlaceTicketsInEms()
	{
		if (_step != Step.PlaceTicketsInEms) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.PlaceTicketsInEms);
				_context = OnPlaceTicketsInEms();
				_step = Step.Ending;
				OnEnd(Step.PlaceTicketsInEms);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnPlaceTicketsInEms();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class LocateStockProcess: IProcess<LocateStockProcess.Step, LocateStockProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		RequestStock,
		WaitForStockLocation,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		RequestStock();
		WaitForStockLocation();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.RequestStock;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void RequestStock()
	{
		if (_step != Step.RequestStock) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.RequestStock);
				_context = OnRequestStock();
				_step = Step.WaitForStockLocation;
				OnEnd(Step.RequestStock);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnRequestStock();

	private void WaitForStockLocation()
	{
		if (_step != Step.WaitForStockLocation) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.WaitForStockLocation);
				_context = OnWaitForStockLocation();
				_step = Step.Ending;
				OnEnd(Step.WaitForStockLocation);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnWaitForStockLocation();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class BookBrokerOrderProcess: IProcess<BookBrokerOrderProcess.Step, BookBrokerOrderProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		FairlyAllocateAcrossTickets,
		BookTheFairAllocations,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a broker order on line 64</summary>
	public bool Given(IBrokerOrder item) { return GivenABrokerOrder(item); }

	protected abstract bool GivenABrokerOrder(IBrokerOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		FairlyAllocateAcrossTickets();
		BookTheFairAllocations();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.FairlyAllocateAcrossTickets;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void FairlyAllocateAcrossTickets()
	{
		if (_step != Step.FairlyAllocateAcrossTickets) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.FairlyAllocateAcrossTickets);
				_context = OnFairlyAllocateAcrossTickets();
				_step = Step.BookTheFairAllocations;
				OnEnd(Step.FairlyAllocateAcrossTickets);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnFairlyAllocateAcrossTickets();

	private void BookTheFairAllocations()
	{
		if (_step != Step.BookTheFairAllocations) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.BookTheFairAllocations);
				_context = OnBookTheFairAllocations();
				_step = Step.Ending;
				OnEnd(Step.BookTheFairAllocations);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnBookTheFairAllocations();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class AmendBookedBrokerOrderProcess: IProcess<AmendBookedBrokerOrderProcess.Step, AmendBookedBrokerOrderProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		ChangeDetailsOfBrokerOrder,
		FairlyAllocateAcrossTickets,
		AmendExistingBooking,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a broker order that is booked on line 69</summary>
	public bool Given(IBrokerOrder item) { return GivenABrokerOrderThatIsBooked(item); }

	protected abstract bool GivenABrokerOrderThatIsBooked(IBrokerOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		ChangeDetailsOfBrokerOrder();
		FairlyAllocateAcrossTickets();
		AmendExistingBooking();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.ChangeDetailsOfBrokerOrder;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void ChangeDetailsOfBrokerOrder()
	{
		if (_step != Step.ChangeDetailsOfBrokerOrder) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.ChangeDetailsOfBrokerOrder);
				_context = OnChangeDetailsOfBrokerOrder();
				_step = Step.FairlyAllocateAcrossTickets;
				OnEnd(Step.ChangeDetailsOfBrokerOrder);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnChangeDetailsOfBrokerOrder();

	private void FairlyAllocateAcrossTickets()
	{
		if (_step != Step.FairlyAllocateAcrossTickets) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.FairlyAllocateAcrossTickets);
				_context = OnFairlyAllocateAcrossTickets();
				_step = Step.AmendExistingBooking;
				OnEnd(Step.FairlyAllocateAcrossTickets);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnFairlyAllocateAcrossTickets();

	private void AmendExistingBooking()
	{
		if (_step != Step.AmendExistingBooking) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.AmendExistingBooking);
				_context = OnAmendExistingBooking();
				_step = Step.Ending;
				OnEnd(Step.AmendExistingBooking);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnAmendExistingBooking();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class AmendHeldBrokerOrderProcess: IProcess<AmendHeldBrokerOrderProcess.Step, AmendHeldBrokerOrderProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		ChangeDetailsOfBrokerOrder,
		FairlyAllocateAcrossTickets,
		AmendExistingBooking,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a broker order that has failed to book on line 75</summary>
	public bool Given(IBrokerOrder item) { return GivenABrokerOrderThatHasFailedToBook(item); }

	protected abstract bool GivenABrokerOrderThatHasFailedToBook(IBrokerOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		ChangeDetailsOfBrokerOrder();
		FairlyAllocateAcrossTickets();
		AmendExistingBooking();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.ChangeDetailsOfBrokerOrder;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void ChangeDetailsOfBrokerOrder()
	{
		if (_step != Step.ChangeDetailsOfBrokerOrder) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.ChangeDetailsOfBrokerOrder);
				_context = OnChangeDetailsOfBrokerOrder();
				_step = Step.FairlyAllocateAcrossTickets;
				OnEnd(Step.ChangeDetailsOfBrokerOrder);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnChangeDetailsOfBrokerOrder();

	private void FairlyAllocateAcrossTickets()
	{
		if (_step != Step.FairlyAllocateAcrossTickets) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.FairlyAllocateAcrossTickets);
				_context = OnFairlyAllocateAcrossTickets();
				_step = Step.AmendExistingBooking;
				OnEnd(Step.FairlyAllocateAcrossTickets);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnFairlyAllocateAcrossTickets();

	private void AmendExistingBooking()
	{
		if (_step != Step.AmendExistingBooking) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.AmendExistingBooking);
				_context = OnAmendExistingBooking();
				_step = Step.Ending;
				OnEnd(Step.AmendExistingBooking);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnAmendExistingBooking();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class CancelBookedBrokerOrderProcess: IProcess<CancelBookedBrokerOrderProcess.Step, CancelBookedBrokerOrderProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		CancelBooking,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a broker order that is booked on line 81</summary>
	public bool Given(IBrokerOrder item) { return GivenABrokerOrderThatIsBooked(item); }

	protected abstract bool GivenABrokerOrderThatIsBooked(IBrokerOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		CancelBooking();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.CancelBooking;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void CancelBooking()
	{
		if (_step != Step.CancelBooking) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.CancelBooking);
				_context = OnCancelBooking();
				_step = Step.Ending;
				OnEnd(Step.CancelBooking);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnCancelBooking();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class LateFillProcess: IProcess<LateFillProcess.Step, LateFillProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		ChangeOneOrMoreFills,
		FairlyAllocateAcrossTickets,
		AmendExistingBooking,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a broker order that is booked on line 85</summary>
	public bool Given(IBrokerOrder item) { return GivenABrokerOrderThatIsBooked(item); }

	protected abstract bool GivenABrokerOrderThatIsBooked(IBrokerOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		ChangeOneOrMoreFills();
		FairlyAllocateAcrossTickets();
		AmendExistingBooking();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.ChangeOneOrMoreFills;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void ChangeOneOrMoreFills()
	{
		if (_step != Step.ChangeOneOrMoreFills) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.ChangeOneOrMoreFills);
				_context = OnChangeOneOrMoreFills();
				_step = Step.FairlyAllocateAcrossTickets;
				OnEnd(Step.ChangeOneOrMoreFills);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnChangeOneOrMoreFills();

	private void FairlyAllocateAcrossTickets()
	{
		if (_step != Step.FairlyAllocateAcrossTickets) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.FairlyAllocateAcrossTickets);
				_context = OnFairlyAllocateAcrossTickets();
				_step = Step.AmendExistingBooking;
				OnEnd(Step.FairlyAllocateAcrossTickets);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnFairlyAllocateAcrossTickets();

	private void AmendExistingBooking()
	{
		if (_step != Step.AmendExistingBooking) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.AmendExistingBooking);
				_context = OnAmendExistingBooking();
				_step = Step.Ending;
				OnEnd(Step.AmendExistingBooking);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnAmendExistingBooking();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class CorrectInvestorProcess: IProcess<CorrectInvestorProcess.Step, CorrectInvestorProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		ChangeInvestorOfOneOrMoreTickets,
		AmendExistingBooking,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a broker order that is booked on line 91</summary>
	public bool Given(IBrokerOrder item) { return GivenABrokerOrderThatIsBooked(item); }

	protected abstract bool GivenABrokerOrderThatIsBooked(IBrokerOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		ChangeInvestorOfOneOrMoreTickets();
		AmendExistingBooking();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.ChangeInvestorOfOneOrMoreTickets;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void ChangeInvestorOfOneOrMoreTickets()
	{
		if (_step != Step.ChangeInvestorOfOneOrMoreTickets) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.ChangeInvestorOfOneOrMoreTickets);
				_context = OnChangeInvestorOfOneOrMoreTickets();
				_step = Step.AmendExistingBooking;
				OnEnd(Step.ChangeInvestorOfOneOrMoreTickets);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnChangeInvestorOfOneOrMoreTickets();

	private void AmendExistingBooking()
	{
		if (_step != Step.AmendExistingBooking) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.AmendExistingBooking);
				_context = OnAmendExistingBooking();
				_step = Step.Ending;
				OnEnd(Step.AmendExistingBooking);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnAmendExistingBooking();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class CancelOrderProcess: IProcess<CancelOrderProcess.Step, CancelOrderProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		CancelTicketsExecutingInEms,
		CancelReservedPositions,
		OrderIsNowCancelled,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given an order on line 96</summary>
	public bool Given(IOrder item) { return GivenAnOrder(item); }

	protected abstract bool GivenAnOrder(IOrder item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		CancelTicketsExecutingInEms();
		CancelReservedPositions();
		OrderIsNowCancelled();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.CancelTicketsExecutingInEms;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void CancelTicketsExecutingInEms()
	{
		if (_step != Step.CancelTicketsExecutingInEms) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.CancelTicketsExecutingInEms);
				_context = OnCancelTicketsExecutingInEms();
				_step = Step.CancelReservedPositions;
				OnEnd(Step.CancelTicketsExecutingInEms);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnCancelTicketsExecutingInEms();

	private void CancelReservedPositions()
	{
		if (_step != Step.CancelReservedPositions) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.CancelReservedPositions);
				_context = OnCancelReservedPositions();
				_step = Step.OrderIsNowCancelled;
				OnEnd(Step.CancelReservedPositions);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnCancelReservedPositions();

	private void OrderIsNowCancelled()
	{
		if (_step != Step.OrderIsNowCancelled) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.OrderIsNowCancelled);
				_context = OnOrderIsNowCancelled();
				_step = Step.Ending;
				OnEnd(Step.OrderIsNowCancelled);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnOrderIsNowCancelled();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}

public abstract partial class CancelBasketProcess: IProcess<CancelBasketProcess.Step, CancelBasketProcess.Context>
{
	protected Step _step;
	protected Context _context;

	Step IProcess<Step, Context>.Step { get { return _step; } set { _step = value; } }
	Context IProcess<Step, Context>.Context { get { return _context; } set { _context = value; } }

	public enum Step
	{
		Starting,
		CancelTicketsExecutingInEms,
		CancelReservedPositions,
		AllOrdersAreNowCancelled,
		BasketIsNowCancelled,
		Ending,
		Finished,
	}

	public partial class Context
	{
		public Exception Failure { get; set; }
		public bool Failed { get { return Failure != null; } }
	}

	/// <summary>given a basket on line 102</summary>
	public bool Given(IBasket item) { return GivenABasket(item); }

	protected abstract bool GivenABasket(IBasket item);

	public void Execute()
	{
		if (_context == null) throw new InvalidOperationException("Expected the context to be setup in the Given method");
		_context.Failure = null;
		try
		{
			OnExecute();
			return;
		}
		catch (Exception e)
		{
			_context.Failure = e;
		}
		OnFailure(_step, _context.Failure);
	}

	private void OnExecute()
	{
		Starting();
		CancelTicketsExecutingInEms();
		CancelReservedPositions();
		AllOrdersAreNowCancelled();
		BasketIsNowCancelled();
		Ending();
	}

	private void Starting()
	{
		if (_step != Step.Starting) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Starting);
				_context = OnStarting();
				_step = Step.CancelTicketsExecutingInEms;
				OnEnd(Step.Starting);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnStarting();

	private void CancelTicketsExecutingInEms()
	{
		if (_step != Step.CancelTicketsExecutingInEms) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.CancelTicketsExecutingInEms);
				_context = OnCancelTicketsExecutingInEms();
				_step = Step.CancelReservedPositions;
				OnEnd(Step.CancelTicketsExecutingInEms);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnCancelTicketsExecutingInEms();

	private void CancelReservedPositions()
	{
		if (_step != Step.CancelReservedPositions) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.CancelReservedPositions);
				_context = OnCancelReservedPositions();
				_step = Step.AllOrdersAreNowCancelled;
				OnEnd(Step.CancelReservedPositions);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnCancelReservedPositions();

	private void AllOrdersAreNowCancelled()
	{
		if (_step != Step.AllOrdersAreNowCancelled) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.AllOrdersAreNowCancelled);
				_context = OnAllOrdersAreNowCancelled();
				_step = Step.BasketIsNowCancelled;
				OnEnd(Step.AllOrdersAreNowCancelled);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnAllOrdersAreNowCancelled();

	private void BasketIsNowCancelled()
	{
		if (_step != Step.BasketIsNowCancelled) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.BasketIsNowCancelled);
				_context = OnBasketIsNowCancelled();
				_step = Step.Ending;
				OnEnd(Step.BasketIsNowCancelled);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnBasketIsNowCancelled();

	private void Ending()
	{
		if (_step != Step.Ending) return;
		using (var tr = new Transition<Step, Context>(this))
		{
			using (var txn = new TransactionScope())
			{
				OnStart(Step.Ending);
				_context = OnEnding();
				_step = Step.Finished;
				OnEnd(Step.Ending);
				txn.Complete();
			}
			tr.Complete();
		}
	}

	protected abstract Context OnEnding();

	protected virtual void OnStart(Step step) {}

	protected virtual void OnEnd(Step step) {}

	protected virtual void OnFailure(Step step, Exception e) {}
}
}
