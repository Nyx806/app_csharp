
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPcsharp2._0;
using MySQL.Data;
using MySql.Data.MySqlClient;

namespace ConsoleAppTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=localhost;Database=tp_cs;Uid=root"; 
            using var connexion = new MySqlConnection(connectionString);

            string command = "SELECT name,price,numberOfArticle,article_code FROM article";
            connexion.Open();

            List<Produit> produits = new List<Produit>();

            using var requette = new MySqlCommand(command, connexion);
            using var reader = requette.ExecuteReader();

            while(reader.Read()) {
                var produit = new Produit
                (
                    reader.GetString("name"), // Lecture du champ 'Nom'
                    reader.GetDecimal("price"), // Lecture du champ 'Prix'
                    reader.GetInt32("numberOfArticle"),
                    reader.GetInt32("article_code")
                );
                produits.Add(produit);
            }




            Panier panier = new Panier(connexion);



            int ajoutPanier = 0;

           
            
            Console.WriteLine("   #####    #####   ### ###   #####    ######           ######   #######  ### ###  #######  ##  ###   ######  #######\r\n  ### ###  ### ###  ### ###  ### ###   # ## #           ### ###  ### ###  ### ###  ### ###  ### ###   # ## #  ### ###\r\n  ### ###  ###      ### ###  ### ###     ##             ### ###  ###      ### ###  ###      #######     ##    ###\r\n  #######  ###      #######  #######     ##             ######   #####    ### ###  #####    #######     ##    #####\r\n  ### ###  ###      ### ###  ### ###     ##             ### ##   ###      ### ###  ###      ### ###     ##    ###\r\n  ### ###  ### ###  ### ###  ### ###     ##             ### ###  ### ###   #####   ### ###  ### ###     ##    ### ###\r\n  ### ###   #####   ### ###  ### ###     ##             ### ###  #######    ###    #######  ### ###     ##    #######\r\n\r\n");

            foreach (var produit in produits) {
                Console.WriteLine(produits);
            }

            Console.WriteLine(" pour ajouter un article a votre panier rentré son code produit : ");
            ajoutPanier = Console.Read();

            var produitTrouve = produits.FirstOrDefault(p => p.CodeProduit == ajoutPanier); 
            if (produitTrouve != null)
            {
                var requetteUpdate = "";
                using var execCommand = new MySqlCommand(requetteUpdate,connexion);
                using var exec = execCommand.ExecuteReader();

                panier.addProduit(produitTrouve);

            }
           



        }
    }
}

