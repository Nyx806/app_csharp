
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPcsharp2._0;
using MySQL.Data;
using MySql.Data.MySqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;

namespace ConsoleAppTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=localhost;Database=tp_cs;Uid=root";
            using var connexion = new MySqlConnection(connectionString);

            List<Produit> produits = new List<Produit>();
            List<int> keyProduct = new List<int>();

            connexion.Open();


            int ajoutPanier = 0;

            Console.WriteLine("Bienvenue dans notre magasin en ligne ! Entrez votre nom (si admin rentrez le nom : admin) : ");
            string nom = Console.ReadLine();

            switch (nom)
            {
                case "admin":
                    adminPage(connexion);
                    break;
                default:


                    clientPage(connexion, produits, keyProduct, ajoutPanier);
                    break;
            }

        }

        static void clientPage(MySqlConnection connexion, List<Produit> produits, List<int> keyProduct, int ajoutPanier)
        {
            while (true)
            {
                produits = loadProduct(connexion);
                keyProduct = produits.Select(p => p.CodeProduit).ToList();




                Console.WriteLine("   #####    #####   ### ###   #####    ######           ######   #######  ### ###  #######  ##  ###   ######  #######\r\n  ### ###  ### ###  ### ###  ### ###   # ## #           ### ###  ### ###  ### ###  ### ###  ### ###   # ## #  ### ###\r\n  ### ###  ###      ### ###  ### ###     ##             ### ###  ###      ### ###  ###      #######     ##    ###\r\n  #######  ###      #######  #######     ##             ######   #####    ### ###  #####    #######     ##    #####\r\n  ### ###  ###      ### ###  ### ###     ##             ### ##   ###      ### ###  ###      ### ###     ##    ###\r\n  ### ###  ### ###  ### ###  ### ###     ##             ### ###  ### ###   #####   ### ###  ### ###     ##    ### ###\r\n  ### ###   #####   ### ###  ### ###     ##             ### ###  #######    ###    #######  ### ###     ##    #######\r\n\r\n");



                foreach (var produit in produits)
                {
                    Console.WriteLine($"Nom: {produit.Nom}, Prix: {produit.Prix}, Quantité en stock: {produit.QuantiteEnStock}, Code produit: {produit.CodeProduit}");

                }
                sumPanier(connexion);

                Console.WriteLine("pour ajouter un article a votre panier rentré son code produit (pour le retirer du panier ajouter '-' devant le code produit) : ");

                int.TryParse(Console.ReadLine(), out ajoutPanier);

                if (keyProduct.Contains(ajoutPanier))
                {
                    var produitTrouve = produits.FirstOrDefault(p => p.CodeProduit == ajoutPanier);

                    if (produitTrouve != null)
                    {
                        addProduct(connexion, produitTrouve);
                    }
                }else if(keyProduct.Contains(-ajoutPanier))
                {
                    var produitTrouve = produits.FirstOrDefault(p => p.CodeProduit == -ajoutPanier);
                    if (produitTrouve != null)
                    {
                        removeProduct(connexion, produitTrouve);
                    }
                }
                else
                {
                    Console.WriteLine($"Erreur : Le code produit {ajoutPanier} n'est pas valide.");
                }



            }
        }

        static List<Produit> loadProduct(MySqlConnection connexion)
        {
            var produits = new List<Produit>();
            string command = "SELECT name, price, numberOfArticle, article_id FROM article";

            using var requette = new MySqlCommand(command, connexion);
            using var reader = requette.ExecuteReader();

            while (reader.Read())
            {
                var produit = new Produit
                (
                    reader.GetString("name"),
                    reader.GetDecimal("price"),
                    reader.GetInt32("numberOfArticle"),
                    reader.GetInt32("article_id")
                );
                produits.Add(produit);
            }


            return produits;
        }

        static void addProduct(MySqlConnection connexion, Produit produit)
        {
            int articleId = produit.CodeProduit;
            decimal price_item = produit.Prix;

            // Vérifier si l'article existe dans la base
            var requetteCheckArticle = "SELECT COUNT(*) FROM article WHERE article_id = @articleId";
            using (var checkCommand = new MySqlCommand(requetteCheckArticle, connexion))
            {
                checkCommand.Parameters.AddWithValue("@articleId", articleId);
                checkCommand.Parameters.AddWithValue("@price_item", price_item);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count > 0) // L'article existe
                {
                    var requetteInsert = "INSERT INTO `panier` (`slot_id`, `article_id`, `price_item`) VALUES (NULL, @articleId, @price_item)";
                    using (var insertCommand = new MySqlCommand(requetteInsert, connexion))
                    {
                        insertCommand.Parameters.AddWithValue("@articleId", articleId);
                        insertCommand.Parameters.AddWithValue("@price_item", price_item);
                        insertCommand.ExecuteNonQuery();
                        Console.WriteLine($"Produit {produit.Nom} ajouté au panier avec succès.");
                    }
                }
                else
                {
                    Console.WriteLine($"Erreur : L'article avec l'ID {articleId} n'existe pas dans la table 'article'.");
                }

            }


        }
        static void adminPage(MySqlConnection connexion)
        {
            Console.WriteLine("Entrez le nom du produit :");
            string nom = Console.ReadLine();

            Console.WriteLine("Entrez le prix du produit :");
            if (!decimal.TryParse(Console.ReadLine(), out decimal prix))
            {
                Console.WriteLine("Erreur : Le prix doit être un nombre décimal.");
                return;
            }

            Console.WriteLine("Entrez la quantité en stock :");
            if (!int.TryParse(Console.ReadLine(), out int quantiteEnStock))
            {
                Console.WriteLine("Erreur : La quantité doit être un nombre entier.");
                return;
            }


            // Préparer la requête d'insertion
            string requeteInsert = "INSERT INTO article (name, price, numberOfArticle, article_id) VALUES (@nom, @prix, @quantite, NULL)";

            try
            {
                using (var command = new MySqlCommand(requeteInsert, connexion))
                {
                    // Ajouter les paramètres à la commande
                    command.Parameters.AddWithValue("@nom", nom);
                    command.Parameters.AddWithValue("@prix", prix);
                    command.Parameters.AddWithValue("@quantite", quantiteEnStock);


                    // Exécuter la commande
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine($"Produit '{nom}' ajouté avec succès !");
                    }
                    else
                    {
                        Console.WriteLine("Erreur : Aucun produit n'a été ajouté.");
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Erreur lors de l'insertion du produit : {ex.Message}");
            }

        }
        static void sumPanier(MySqlConnection connexion)
        {
            int total = 0;
            string requette = "SELECT SUM(price_item) FROM panier";
            using var command = new MySqlCommand(requette, connexion);
            using var reader = command.ExecuteReader();
            
            if (reader.Read()) // Lire la première ligne
            {
                if (!reader.IsDBNull(0)) // Vérifier si la colonne n'est pas null
                {
                    total = reader.GetInt32(0);
                }
            }


            if (total == 0)
            {
                Console.WriteLine("Votre panier est vide");
            }
            else
            {
                Console.WriteLine($"Le total de votre panier est de : {total} euros");
            }
        }

        static void removeProduct(MySqlConnection connexion, Produit produit)
        {
            int articleId = produit.CodeProduit;
            decimal price_item = produit.Prix;
            // Vérifier si l'article existe dans la base
            var requetteCheckArticle = "SELECT COUNT(*) FROM article WHERE article_id = @articleId";
            using (var checkCommand = new MySqlCommand(requetteCheckArticle, connexion))
            {
                checkCommand.Parameters.AddWithValue("@articleId", articleId);
                checkCommand.Parameters.AddWithValue("@price_item", price_item);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (count > 0) // L'article existe
                {
                    var requetteInsert = "DELETE FROM `panier` WHERE article_id = @articleId";
                    using (var insertCommand = new MySqlCommand(requetteInsert, connexion))
                    {
                        insertCommand.Parameters.AddWithValue("@articleId", articleId);
                        insertCommand.Parameters.AddWithValue("@price_item", price_item);
                        insertCommand.ExecuteNonQuery();
                        Console.WriteLine($"Produit {produit.Nom} retiré du panier avec succès.");
                    }
                }
                else
                {
                    Console.WriteLine($"Erreur : L'article avec l'ID {articleId} n'existe pas dans la table 'article'.");
                }
            }
        }
    }
} 

