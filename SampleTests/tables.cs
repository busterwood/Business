using System.Collections.Generic;

public interface IUser
{
	string Name { get; }
}

public interface ISecurity
{
	string Name { get; }
}

public interface IInvestor
{
	string Name { get; }
}

public interface IBasket
{
	IUser DecisionMaker { get; }
	IEnumerable<IOrder> Orders { get; }
}

public interface IOrder
{
	IEnumerable<IAllocation> Allocations { get; }
	string Security { get; }
}

public interface IAllocation
{
	string Investor { get; }
	string Security { get; }
	string Quantity { get; }
}

public interface ITicket
{
	IEnumerable<IAllocation> Allocations { get; }
	string Security { get; }
	string Quantity { get; }
}

public interface IBrokerOrder
{
	IEnumerable<ITicket> Tickets { get; }
	IEnumerable<IFill> Fills { get; }
	IEnumerable<IBooking> Bookings { get; }
}

public interface IFill
{
	string Broker { get; }
	string Security { get; }
	string Price { get; }
	string Quantity { get; }
}

public interface IBooking
{
}

public interface IBroker
{
	string Name { get; }
}
