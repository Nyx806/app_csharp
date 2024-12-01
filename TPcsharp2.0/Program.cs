
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using TPcsharp2._0.Class;

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
            List<User> users = new List<User>();
            List<articleCustomers> articleCustomers = new List<articleCustomers>();

            connexion.Open();
            users = loadUser(connexion);
            articleCustomers = loadArticleCustomers(connexion);



            int ajoutPanier = 0;

            Console.WriteLine("Bienvenue dans notre magasin en ligne ! Entrez votre nom (si admin rentrez le nom : admin) : ");
            string nom = Console.ReadLine();

            switch (nom)
            {
                case "admin":
                    adminPage(connexion);
                    break;
                default:
                    addUser(connexion, nom,users);
                    clientPage(connexion, produits, keyProduct, ajoutPanier,nom,users);
                    break;
            }

        }

        static void clientPage(MySqlConnection connexion, List<Produit> produits, List<int> keyProduct, int ajoutPanier,string nom, List<User> users)
        {
            while (true)
            {
                produits = loadProduct(connexion);
                keyProduct = produits.Select(p => p.CodeProduit).ToList();
                string nomUser = afficheUser(connexion, users,nom);

                
                Console.Clear();

                Console.WriteLine("   #####    #####   ### ###   #####    ######           ######   #######  ### ###  #######  ##  ###   ######  #######\r\n  ### ###  ### ###  ### ###  ### ###   # ## #           ### ###  ### ###  ### ###  ### ###  ### ###   # ## #  ### ###\r\n  ### ###  ###      ### ###  ### ###     ##             ### ###  ###      ### ###  ###      #######     ##    ###\r\n  #######  ###      #######  #######     ##             ######   #####    ### ###  #####    #######     ##    #####\r\n  ### ###  ###      ### ###  ### ###     ##             ### ##   ###      ### ###  ###      ### ###     ##    ###\r\n  ### ###  ### ###  ### ###  ### ###     ##             ### ###  ### ###   #####   ### ###  ### ###     ##    ### ###\r\n  ### ###   #####   ### ###  ### ###     ##             ### ###  #######    ###    #######  ### ###     ##    #######\r\n\r\n");
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");
                Console.WriteLine("bonjour " + nomUser);
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");


                foreach (var produit in produits)
                {
                    Console.WriteLine($"Nom: {produit.Nom}, Prix: {produit.Prix}, Quantité en stock: {produit.QuantiteEnStock}, Code produit: {produit.CodeProduit}");

                }

         

                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");


                sumPanier(connexion);

                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");


                Console.Write("pour ajouter un article a votre panier rentré son code produit (pour le retirer du panier ajouter '-' devant le code produit) : ");

                int.TryParse(Console.ReadLine(), out ajoutPanier);

                if (keyProduct.Contains(ajoutPanier))
                {
                    var produitTrouve = produits.FirstOrDefault(p => p.CodeProduit == ajoutPanier);
                    var currentUser = users.FirstOrDefault(u => u.username == nom);

                    if (produitTrouve != null)
                    {
                        addProduct(connexion, produitTrouve, ajoutPanier);
                        if (ajoutPanier > 0)
                        {
                            decrementStock(connexion, ajoutPanier);
                            linkArticleCustomers(connexion, ajoutPanier, currentUser.customer_id);
                        }
                    }
                }else if(keyProduct.Contains(-ajoutPanier))
                {
                    var produitTrouve = produits.FirstOrDefault(p => p.CodeProduit == -ajoutPanier);
                    var currentUser = users.FirstOrDefault(u => u.username == nom);

                    if (produitTrouve != null)
                    {
                        removeProduct(connexion, produitTrouve,currentUser);
                        incrementStock(connexion, -ajoutPanier, produitTrouve.maxQuantiteEnStock);
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

        static List<User> loadUser(MySqlConnection connexion)
        {
            var users = new List<User>();
            string command = "SELECT username,customer_id FROM customers";
            using var requette = new MySqlCommand(command, connexion);
            using var reader = requette.ExecuteReader();
            while (reader.Read())
            {
                var user = new User
                (
                    reader.GetInt32("customer_id"),
                    reader.GetString("username")
                );
                users.Add(user);
            }
            return users;
        }

        static List<articleCustomers> loadArticleCustomers(MySqlConnection connexion)
        {
            var articleCustomers = new List<articleCustomers>();
            string command = "SELECT article_id,customer_id FROM article_customer";
            using var requette = new MySqlCommand(command, connexion);
            using var reader = requette.ExecuteReader();
            while (reader.Read())
            {
                var articleCustomer = new articleCustomers
                (
                    reader.GetInt32("article_id"),
                    reader.GetInt32("customer_id")
                );
                articleCustomers.Add(articleCustomer);
            }
            return articleCustomers;
        }

        static void addProduct(MySqlConnection connexion, Produit produit,int customerId)
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

        static bool RecordExists(MySqlConnection connexion, string table, string column, int value)
        {
            var query = $"SELECT COUNT(*) FROM {table} WHERE {column} = @value";
            using var command = new MySqlCommand(query, connexion);
            command.Parameters.AddWithValue("@value", value);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }

        static void linkArticleCustomers(MySqlConnection connexion, int articleId, int customerId)
        {
            // Vérifier l'existence de article_id
            if (!RecordExists(connexion, "article", "article_id", articleId))
            {
                Console.WriteLine($"Erreur : L'article avec ID {articleId} n'existe pas.");
                return;
            }

            // Vérifier l'existence de customer_id
            if (!RecordExists(connexion, "customers", "customer_id", customerId))
            {
                Console.WriteLine($"Erreur : Le client avec ID {customerId} n'existe pas.");
                return;
            }

            // Insérer le lien
            var requetteInsertLien = "INSERT INTO `article_customer` (`article_id`, `customer_id`) VALUES (@articleId, @customerId)";
            using (var linkCommand = new MySqlCommand(requetteInsertLien, connexion))
            {
                linkCommand.Parameters.AddWithValue("@articleId", articleId);
                linkCommand.Parameters.AddWithValue("@customerId", customerId);
                linkCommand.ExecuteNonQuery();
                Console.WriteLine($"Lien entre le produit et le client créé avec succès.");
            }
        }


        static void addUser(MySqlConnection connexion, string nom,List<User> users)
        {
            // Vérifier si le nom existe déjà dans la liste
            if (users.Any(u => u.username.Equals(nom, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine("Erreur : Ce nom existe déjà dans la base de données.");
                return;
            }

            // Insérer le nom dans la base si non présent
            string requeteInsert = "INSERT INTO customers (username) VALUES (@username)";
            using (var command = new MySqlCommand(requeteInsert, connexion))
            {
                command.Parameters.AddWithValue("@username", nom);
                command.ExecuteNonQuery();
                Console.WriteLine("Nom inséré avec succès !");
            }

            // Mettre à jour la liste en mémoire
            long newId = Convert.ToInt64(new MySqlCommand("SELECT LAST_INSERT_ID()", connexion).ExecuteScalar());
            users.Add(new User((int)newId, nom));
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

        static string afficheUser(MySqlConnection connexion,List<User> users,string nom)
        {

            var user = users.FirstOrDefault(u => u.username == nom);
            if (user != null)
            {
                return user.username;
            }
            else
            {
                return "Utilisateur introuvable.";
            }
        }

        static void sumPanier(MySqlConnection connexion)
        {
            int total = 0;
            string requette = @"SELECT SUM(price_item) AS total FROM panier; SELECT article_id, COUNT(*) AS nombre_articles FROM panier GROUP BY article_id;";
            using var command = new MySqlCommand(requette, connexion);
            using var reader = command.ExecuteReader();

            if (reader.Read() && !reader.IsDBNull(0))
            {
                total = reader.GetInt32(0);
                if (total == 0)
                {
                    Console.WriteLine("Votre panier est vide");
                }
                else
                {
                    Console.WriteLine($"Le total de votre panier est de : {total} euros");
                }
            }

            // Passer à la deuxième requête
            if (reader.NextResult())
            {
                Console.WriteLine("Détail des articles dans le panier :");
                while (reader.Read())
                {
                    int articleId = reader.GetInt32("article_id");
                    int nombreArticles = reader.GetInt32("nombre_articles");

                    Console.WriteLine($"Article ID: {articleId}, Quantité : {nombreArticles}");
                }
            }
        }

        static void removeProduct(MySqlConnection connexion, Produit produit,User user)
        {
            int articleId = produit.CodeProduit;
            decimal price_item = produit.Prix;
            int customerId = user.customer_id;
            // Vérifier si l'article existe dans la base
            var requetteCheckArticle = "SELECT COUNT(*) FROM article WHERE article_id = @articleId";
            using (var checkCommand = new MySqlCommand(requetteCheckArticle, connexion))
            {
                checkCommand.Parameters.AddWithValue("@articleId", articleId);
                checkCommand.Parameters.AddWithValue("@price_item", price_item);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (count > 0) // L'article existe
                {
                    var requetteInsert = "DELETE FROM `panier` WHERE article_id = @articleId LIMIT 1";
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

                // Vérifiez si l'article est toujours dans le panier
                var checkLinkQuery = "SELECT COUNT(*) FROM article_customer WHERE article_id = @articleId AND customer_id = @customerId";
                using (var checkLinkCommand = new MySqlCommand(checkLinkQuery, connexion))
                {
                    checkLinkCommand.Parameters.AddWithValue("@articleId", articleId);
                    checkLinkCommand.Parameters.AddWithValue("@customerId", customerId);
                    int countLink = Convert.ToInt32(checkLinkCommand.ExecuteScalar());

       
                        // Supprimer le lien
                        var deleteLinkQuery = "DELETE FROM article_customer WHERE article_id = @articleId AND customer_id = @customerId";
                        using (var deleteLinkCommand = new MySqlCommand(deleteLinkQuery, connexion))
                        {
                            deleteLinkCommand.Parameters.AddWithValue("@articleId", articleId);
                            deleteLinkCommand.Parameters.AddWithValue("@customerId", customerId);
                            deleteLinkCommand.ExecuteNonQuery();
                            Console.WriteLine($"Lien entre article ID {articleId} et client ID {customerId} supprimé.");
                        }

                }
            }
        }

        static void decrementStock(MySqlConnection connexion,int articleId)
        {
            // Vérifier si l'article existe et récupérer son stock actuel
            string requetteCheckStock = "SELECT numberOfArticle FROM article WHERE article_id = @articleId";
            using var commandCheck = new MySqlCommand(requetteCheckStock, connexion);
            commandCheck.Parameters.AddWithValue("@articleId", articleId);

            int stockActuel;
            using (var reader = commandCheck.ExecuteReader())
            {
                if (reader.Read())
                {
                    stockActuel = reader.GetInt32("numberOfArticle");
                }
                else
                {
                    Console.WriteLine($"Erreur : L'article avec ID {articleId} n'existe pas.");
                    return;
                }
            }

            // Vérifier si le stock est suffisant
            if (stockActuel <= 0)
            {
                Console.WriteLine($"Stock insuffisant pour l'article avec ID {articleId}.");
                return;
            }

            // Décrémenter le stock
            string requetteUpdateStock = "UPDATE article SET numberOfArticle = numberOfArticle - 1 WHERE article_id = @articleId";
            using var commandUpdate = new MySqlCommand(requetteUpdateStock, connexion);
            commandUpdate.Parameters.AddWithValue("@articleId", articleId);

            int rowsAffected = commandUpdate.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine($"Le stock de l'article avec ID {articleId} a été réduit de 1.");
            }
            else
            {
                Console.WriteLine($"Erreur : Impossible de mettre à jour le stock de l'article avec ID {articleId}.");
            }
        }

        static void incrementStock(MySqlConnection connexion, int articleId, int enStock)
        {
            // Vérifier si l'article existe et récupérer son stock actuel
            string requetteCheckStock = "SELECT numberOfArticle FROM article WHERE article_id = @articleId";
            using var commandCheck = new MySqlCommand(requetteCheckStock, connexion);
            commandCheck.Parameters.AddWithValue("@articleId", articleId);
            int stockActuel;
            using (var reader = commandCheck.ExecuteReader())
            {
                if (reader.Read())
                {
                    stockActuel = reader.GetInt32("numberOfArticle");
                }
                else
                {
                    Console.WriteLine($"Erreur : L'article avec ID {articleId} n'existe pas.");
                    return;
                }
            }
            // Incrémenter le stock
            if (stockActuel > enStock)
            {
                Console.WriteLine($"Stock suffisant pour l'article avec ID {articleId}.");
                return;
            }

            string requetteUpdateStock = "UPDATE article SET numberOfArticle = numberOfArticle + 1 WHERE article_id = @articleId";
            using var commandUpdate = new MySqlCommand(requetteUpdateStock, connexion);
            commandUpdate.Parameters.AddWithValue("@articleId", articleId);
            int rowsAffected = commandUpdate.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine($"Le stock de l'article avec ID {articleId} a été augmenté de 1.");
            }
            else
            {
                Console.WriteLine($"Erreur : Impossible de mettre à jour le stock de l'article avec ID {articleId}.");
            }
        }
    }
} 

