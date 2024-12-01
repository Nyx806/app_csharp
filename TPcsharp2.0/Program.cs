
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
            // Chaîne de connexion pour accéder à la base de données MySQL
            string connectionString = "Server=localhost;Database=tp_cs;Uid=root";

            // Création d'une connexion MySQL avec fermeture automatique en fin de bloc
            using var connexion = new MySqlConnection(connectionString);

            // Initialisation des listes pour stocker les données récupérées depuis la base
            List<Produit> produits = new List<Produit>(); 
            List<int> keyProduct = new List<int>();     
            List<User> users = new List<User>();         
            List<Panier> panier = new List<Panier>();   
            List<articleCustomers> articleCustomers = new List<articleCustomers>(); 

            // Ouverture de la connexion à la base de données
            connexion.Open();

            // Chargement des utilisateurs existants depuis la base
            users = loadUser(connexion);

            // Chargement des liens entre articles et clients depuis la base
            articleCustomers = loadArticleCustomers(connexion);

            // Initialisation de la variable pour gérer l'ajout au panier
            int addPanier = 0;

            // Demande à l'utilisateur d'entrer son nom
            Console.Write("Bienvenue dans notre magasin en ligne ! Entrez votre nom (si admin rentrez le nom : admin) : ");
            string nom = Console.ReadLine();

            // Gestion des différentes pages selon le type d'utilisateur
            switch (nom)
            {
                case "admin":
                    // Si l'utilisateur entre "admin", accéder à la page admin
                    adminPage(connexion);
                    break;

                default:
                    // Pour un utilisateur classique, on vérifie/ajoute l'utilisateur dans la base
                    addUser(connexion, nom, users);

                    // Charger la page client avec les données nécessaires
                    clientPage(connexion, produits, keyProduct, addPanier, nom, users, panier);
                    break;
            }

        }

       

        static void clientPage(MySqlConnection connexion, List<Produit> produits, List<int> keyProduct, int addPanier,string nom, List<User> users, List<Panier> panier)
        {
            // Début d'une boucle infinie pour maintenir l'application active
            while (true)
            {
                // Chargement des produits depuis la base de données à chaque itération
                produits = loadProduct(connexion);

                // Création d'une liste des codes de produits à partir des produits chargés
                keyProduct = produits.Select(p => p.CodeProduit).ToList();

                // Affichage du nom de l'utilisateur connecté
                string nomUser = afficheUser(connexion, users, nom);

                // Recherche de l'utilisateur actuel dans la liste des utilisateurs
                var currentUser = users.FirstOrDefault(u => u.username == nom);

                // Recherche du panier de l'utilisateur actuel
                var currentPanier = panier.FirstOrDefault(p => p.customer_id == currentUser.customer_id);

                // Efface l'écran pour afficher le menu principal
                Console.Clear();

                
                Console.WriteLine("   #####    #####   ### ###   #####    ######           ######   #######  ### ###  #######  ##  ###   ######  #######\r\n  ### ###  ### ###  ### ###  ### ###   # ## #           ### ###  ### ###  ### ###  ### ###  ### ###   # ## #  ### ###\r\n  ### ###  ###      ### ###  ### ###     ##             ### ###  ###      ### ###  ###      #######     ##    ###\r\n  #######  ###      #######  #######     ##             ######   #####    ### ###  #####    #######     ##    #####\r\n  ### ###  ###      ### ###  ### ###     ##             ### ##   ###      ### ###  ###      ### ###     ##    ###\r\n  ### ###  ### ###  ### ###  ### ###     ##             ### ###  ### ###   #####   ### ###  ### ###     ##    ### ###\r\n  ### ###   #####   ### ###  ### ###     ##             ### ###  #######    ###    #######  ### ###     ##    #######\r\n\r\n");

                
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");

               
                Console.WriteLine("bonjour " + nomUser);

                
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");

                // Affichage des informations sur chaque produit
                foreach (var produit in produits)
                {
                    Console.WriteLine($"Nom: {produit.Nom}, Prix: {produit.Prix}, Quantité en stock: {produit.QuantiteEnStock}, Code produit: {produit.CodeProduit}");
                }

                
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");

                // Affichage du total du panier de l'utilisateur
                sumPanier(connexion, currentUser.customer_id);

                
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------");

                
                Console.Write("pour ajouter un article a votre panier rentré son code produit (pour le retirer du panier ajouter '-' devant le code produit) : ");

                // conversion de l'entrée utilisateur en entier
                int.TryParse(Console.ReadLine(), out addPanier);

                // Vérification si le code produit entré existe dans la liste des produits
                if (keyProduct.Contains(addPanier))
                {
                    // Recherche du produit correspondant au code produit entré
                    var foundProduct = produits.FirstOrDefault(p => p.CodeProduit == addPanier);

                    // Si le produit existe, on l'ajoute au panier de l'utilisateur
                    if (foundProduct != null)
                    {
                        // Appel de la fonction pour ajouter le produit au panier
                        addProduct(connexion, addPanier, foundProduct, currentUser);

                        // Si l'ajout concerne un produit (code positif), on décrémente le stock
                        if (addPanier > 0)
                        {
                            decrementStock(connexion, addPanier);
                        }
                    }
                }
                // Vérification si l'utilisateur veut retirer un produit du panier (code négatif)
                else if (keyProduct.Contains(-addPanier))
                {
                    // Recherche du produit correspondant au code produit retiré
                    var foundProduct = produits.FirstOrDefault(p => p.CodeProduit == -addPanier);

                    // Si le produit existe, on le retire du panier
                    if (foundProduct != null)
                    {
                        // Appel de la fonction pour retirer le produit du panier
                        removeProduct(connexion, foundProduct, addPanier, currentPanier, currentUser);

                        // Incrémentation du stock du produit retiré
                        incrementStock(connexion, -addPanier, foundProduct.maxQuantiteEnStock);
                    }
                }
                else
                {
                    
                    Console.WriteLine($"Erreur : Le code produit {addPanier} n'est pas valide.");
                }
            }
        }

        static  List<Panier> loadPanier(MySqlConnection connexion)
        {
            // Initialisation d'une liste pour stocker les paniers récupérés depuis la base de données
            var paniers = new List<Panier>();

            // Définition de la commande SQL pour sélectionner les colonnes nécessaires depuis la table `panier`
            string command = "SELECT slot_id, article_id, price_item, customer_id FROM panier";

            // Création d'une commande MySQL avec la requête et la connexion active
            using var request = new MySqlCommand(command, connexion);

            // Exécution de la commande et récupération des résultats sous forme de lecteur
            using var reader = request.ExecuteReader();

            // Parcours des lignes du résultat de la requête
            while (reader.Read())
            {
                // Création d'un nouvel objet `Panier` à partir des colonnes de la ligne actuelle
                var panier = new Panier
                (
                    reader.GetInt32("slot_id"),      // Lecture de la colonne `slot_id` comme entier
                    reader.GetInt32("article_id"),  
                    reader.GetInt32("price_item"),  
                    reader.GetInt32("customer_id")  
                );
                // Ajout de l'objet `Panier` à la liste des paniers
                paniers.Add(panier);
            }
            return paniers;
        }

        static List<Produit> loadProduct(MySqlConnection connexion)
        {
            var produits = new List<Produit>();
            string command = "SELECT name, price, numberOfArticle, article_id FROM article";

            using var request = new MySqlCommand(command, connexion);
            using var reader = request.ExecuteReader();

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
            using var request = new MySqlCommand(command, connexion);
            using var reader = request.ExecuteReader();
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
            string command = "SELECT slot_id,customer_id FROM article_customer";
            using var request = new MySqlCommand(command, connexion);
            using var reader = request.ExecuteReader();
            while (reader.Read())
            {
                var articleCustomer = new articleCustomers
                (
                    reader.GetInt32("slot_id"),
                    reader.GetInt32("customer_id")
                );
                articleCustomers.Add(articleCustomer);
            }
            return articleCustomers;
        }

        static void addProduct(MySqlConnection connexion, int addPanier, Produit produit, User currentUser)
        {
            int produitId = produit.CodeProduit; 
            decimal price_item = produit.Prix;
            var currentPanier = loadPanier(connexion).FirstOrDefault(p => p.customer_id == currentUser.customer_id);

            // Vérifier si l'article existe dans la base
            var requetteCheckArticle = "SELECT COUNT(*) FROM article WHERE article_id = @articleId";
            using (var checkCommand = new MySqlCommand(requetteCheckArticle, connexion))
            {
                checkCommand.Parameters.AddWithValue("@articleId", produitId);
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());

                if (count > 0) // L'article existe
                {
                    // Insérer le produit dans le panier (création d'un nouveau slot_id)
                    var requetteInsert = "INSERT INTO `panier` (`slot_id`, `article_id`, `price_item`, `customer_id`) VALUES (NULL, @articleId, @price_item, @customerId)";
                    using (var insertCommand = new MySqlCommand(requetteInsert, connexion))
                    {
                        insertCommand.Parameters.AddWithValue("@articleId", produitId);
                        insertCommand.Parameters.AddWithValue("@price_item", price_item);
                        insertCommand.Parameters.AddWithValue("@customerId", currentUser.customer_id);
                        insertCommand.ExecuteNonQuery();
                    }

                    // Recharger le panier pour récupérer le dernier slot_id
                    currentPanier = loadPanier(connexion).LastOrDefault(p => p.customer_id == currentUser.customer_id);

                    // Insérer le lien dans la table article_customer avec le nouveau slot_id
                    var insertLinkQuery = "INSERT INTO article_customer (slot_id, customer_id) VALUES (@slot_id, @customerId)";
                    using (var insertLinkCommand = new MySqlCommand(insertLinkQuery, connexion))
                    {
                        insertLinkCommand.Parameters.AddWithValue("@slot_id", currentPanier.slot_id); // Utilisez le dernier slot_id récupéré
                        insertLinkCommand.Parameters.AddWithValue("@customerId", currentUser.customer_id);
                        insertLinkCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine($"Produit {produit.Nom} ajouté au panier avec succès.");
                }
                else
                {
                    Console.WriteLine($"Erreur : L'article avec l'ID {produitId} n'existe pas dans la table 'article'.");
                }
            }
        }

        static void addUser(MySqlConnection connexion, string nom,List<User> users)
        {
            // Vérifier si le nom existe déjà dans la liste
            if (users.Any(u => u.username.Equals(nom, StringComparison.OrdinalIgnoreCase)))
            {
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

        static void sumPanier(MySqlConnection connexion, int customerId)
        {
            int total = 0;

            // Requête SQL combinée pour :
            // 1. Calculer la somme totale des prix des articles dans le panier
            // 2. Compter les articles groupés par leur ID
            string request = @"
            SELECT SUM(p.price_item) AS total FROM panier p INNER JOIN article_customer ac ON p.slot_id = ac.slot_id WHERE ac.customer_id = @customerId;

            SELECT p.article_id, COUNT(*) AS nombre_articles FROM panier p INNER JOIN article_customer ac ON p.slot_id = ac.slot_id WHERE ac.customer_id = @customerId GROUP BY p.article_id;";

            // Préparation de la commande avec paramètre pour sécuriser la requête
            using var command = new MySqlCommand(request, connexion);
            command.Parameters.AddWithValue("@customerId", customerId);

            // Exécution de la requête et lecture des résultats
            using var reader = command.ExecuteReader();

            // Lecture du total (première partie de la requête)
            if (reader.Read() && !reader.IsDBNull(0))
            {
                total = reader.GetInt32(0); // Récupération du total
                Console.WriteLine($"Le total de votre panier est de : {total} euros"); 
            }
            else
            {
                Console.WriteLine("Votre panier est vide");
            }

            // Lecture des détails des articles (deuxième partie de la requête)
            if (reader.NextResult()) // Passe au résultat suivant
            {
                Console.WriteLine("Détail des articles dans le panier :");
                while (reader.Read()) // Parcours des lignes
                {
                    int articleId = reader.GetInt32("article_id"); // Récupération de l'ID de l'article
                    int nombreArticles = reader.GetInt32("nombre_articles"); // Récupération du nombre d'articles
                    Console.WriteLine($"Article ID: {articleId}, Quantité : {nombreArticles}");
                }
            }
        }

        static void removeProduct(MySqlConnection connexion,Produit produit, int addPanier, Panier panier ,User user)
        {
            int slot_id = panier.slot_id;
            int article_id = Math.Abs(addPanier);
            decimal price_item = produit.Prix;
            int customerId = user.customer_id;

            // Vérifier si l'article existe dans la base
            var requetteCheckArticle = "SELECT COUNT(*) FROM article WHERE article_id = @articleId"; 
            using (var checkCommand = new MySqlCommand(requetteCheckArticle, connexion))
            {
                checkCommand.Parameters.AddWithValue("@articleId", article_id); //
                var count = Convert.ToInt32(checkCommand.ExecuteScalar());
                if (count > 0) // L'article existe
                {
                    // Supprimer l'article du panier
                    var requetteInsert = "DELETE FROM `panier` WHERE article_id = @articleId LIMIT 1";
                    using (var insertCommand = new MySqlCommand(requetteInsert, connexion))
                    {
                        insertCommand.Parameters.AddWithValue("@articleId", article_id);
                        insertCommand.ExecuteNonQuery();
                        Console.WriteLine($"Produit {produit.Nom} retiré du panier avec succès.");
                    }
                }
                else
                {
                    Console.WriteLine($"Erreur : L'article avec l'ID {article_id} n'existe pas dans la table 'article'.");
                }

                // Vérifiez si l'article est toujours dans le panier
                var checkLinkQuery = "SELECT COUNT(*) FROM article_customer WHERE slot_id = @slot_id AND customer_id = @customerId";
                using (var checkLinkCommand = new MySqlCommand(checkLinkQuery, connexion))
                {
                 
                    checkLinkCommand.Parameters.AddWithValue("@slot_id", slot_id);
                    checkLinkCommand.Parameters.AddWithValue("@customerId", customerId);
                    int countLink = Convert.ToInt32(checkLinkCommand.ExecuteScalar());

       
                        // Supprimer le lien
                        var deleteLinkQuery = "DELETE FROM article_customer WHERE slot_id = @slot_id AND customer_id = @customerId LIMIT 1";
                        using (var deleteLinkCommand = new MySqlCommand(deleteLinkQuery, connexion))
                        {
                            deleteLinkCommand.Parameters.AddWithValue("@slot_id", slot_id);
                            deleteLinkCommand.Parameters.AddWithValue("@customerId", customerId);
                            deleteLinkCommand.ExecuteNonQuery();
                            Console.WriteLine($"Lien entre article ID {slot_id} et client ID {customerId} supprimé.");
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
                    stockActuel = reader.GetInt32("numberOfArticle"); // Récupération du stock actuel
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

            int rowsAffected = commandUpdate.ExecuteNonQuery(); // 
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

            // Incrémenter le stock

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

