using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoyaltyService
{
    public static class Messages
    {
        public abstract class RedemptionBase
        {
            public long Gpid { get; private set; }

            protected RedemptionBase(long gpid)
            {
                Gpid = gpid;
            }
        }
    }
}
