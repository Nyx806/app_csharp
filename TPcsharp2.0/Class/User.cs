using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TPcsharp2._0.Class
{
    internal class User
    {
        public int customer_id { get; set; }
        public string username { get; set; }

        public User(int customer_id, string name)
        {
            this.customer_id = customer_id;
            username = name;
        }

    }
}
