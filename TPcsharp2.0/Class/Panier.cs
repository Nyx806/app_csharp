using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPcsharp2._0.Class
{
    internal class Panier
    {
        public int article_id { get; set; }
        public decimal price_item { get; set; }
        public int customer_id { get; set; }  
        public int slot_id { get; set; }   

        public Panier(int slot_id,int article_id, int price_item, int customer_id)
        {
            this.slot_id = slot_id;
            this.article_id = article_id;
            this.customer_id = customer_id; 
            this.price_item = price_item;
        }
    }
}
