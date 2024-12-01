using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPcsharp2._0
{
    internal class Produit
    {
        public string Nom { get; set; }
        public decimal Prix { get; set; } // Utiliser Decimal pour des valeurs monétaires
        public int QuantiteEnStock { get; set; }
        public int maxQuantiteEnStock { get; set; }
        public int CodeProduit { get; set; }
    

       

        public Produit(string nom, decimal prix, int qantiteEnStock, int codeProduit)
        {
            Nom = nom;
            Prix = prix;
            maxQuantiteEnStock = qantiteEnStock;
            QuantiteEnStock = qantiteEnStock;
            CodeProduit = codeProduit;
        }

    }
}
