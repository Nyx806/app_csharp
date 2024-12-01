using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPcsharp2._0.Class
{
    internal class articleCustomers
    {
        public int customer_id { get; set; }
        public int slot_id { get; set; }
        public articleCustomers(int customer_id, int slot_id)
        {
            this.customer_id = customer_id;
            this.slot_id = slot_id;
        }
    }
}
