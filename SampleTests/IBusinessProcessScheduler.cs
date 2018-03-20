using System.Threading;

namespace SampleTests
{
    //TODO: this looks a lots like a task engine, can the cancellation be abstracted further?
    interface IBusinessProcessScheduler
    {
        bool Schedule(IBusinessProcess businessProcess, CancellationToken cancellationToken = default(CancellationToken));
    }
}