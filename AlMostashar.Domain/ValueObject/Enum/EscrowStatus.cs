using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlMostashar.Domain.ValueObject.Enum
{
    public enum EscrowStatus
    {
        NotFunded,
        Funded,
        Released,
        Refunded,
        Disputed
    }
}
