using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusterWood.Business
{
    abstract class Basket
    {
        public async Task StartTrading()
        {
            if (!BasketIsValid())
                return;

            do
            {
                await CheckCompliance();
                if (ComplianceFailed())
                    await ComplianceResolution();
            }
            while (!ComplianceSucceeded());

            await GenerateTickets();
            if (TicketGenerationFailed())
                return;

            SendTicketsToEms();
        }

        internal abstract bool BasketIsValid();
        internal abstract Task CheckCompliance();
        internal abstract bool ComplianceFailed();
        internal abstract Task ComplianceResolution();
        internal abstract bool ComplianceSucceeded();
        internal abstract Task GenerateTickets();
        internal abstract bool TicketGenerationFailed();
        internal abstract void SendTicketsToEms();
    }
}
