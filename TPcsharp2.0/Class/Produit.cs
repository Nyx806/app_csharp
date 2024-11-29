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
        public int CodeProduit { get; set; }
    

       

        public Produit(string nom, decimal prix, int qantiteEnStock, int codeProduit)
        {
            Nom = nom;
            Prix = prix;
            QuantiteEnStock = qantiteEnStock;
            CodeProduit = codeProduit;
        }

        public void  affiche(MySqlConnection connexion)
        {
            string selectQuery = "SELECT * FROM article ";
            var command = new MySqlCommand(selectQuery, connexion);
            using var reader = command.ExecuteReader();

            Console.WriteLine("produit proposer");
            while (reader.Read())
            {
                Console.WriteLine($"Nom : {reader["name"]}, Prix : {reader["price"]}, Nombre d'article : {reader["numberOfArticle"]} ");
            }
        }

        public void creationProduit()
        {
            string newName;
            float newPrix;
            int newQantiteEnStock;
            int newCodeProduit;

            Console.Write("entrez le nom de votre produit : ");
            newName = Console.ReadLine();

            Console.Write("entrez le prix du produit : ");
            newPrix = Console.Read();

            Console.Write("entrez le nombre de de produit que vous possédez : ");
            newQantiteEnStock = Console.Read();

            Console.Write("choisissez le code produit : ");
            newCodeProduit = Console.Read();

            Console.WriteLine("merci d'avoir enregistrer le produit");

        }
    }
}
