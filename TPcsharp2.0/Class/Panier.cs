using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPcsharp2._0
{
    internal class Panier
    {

        private List<Produit> panier;
        int nbArticle;
        float totalPanier;

        public Panier(MySqlConnection connexion)
        {
            panier = new List<Produit>();
        }

        public void addProduit(Produit produit)
        {
            panier.Add(produit);
        }
        public void totalProduit(float prixProduit)
        {
            totalPanier += prixProduit;
        }
        public void afficheContenu()
        {
            nbArticle = panier.Count;
            Console.WriteLine("le nombre d'Article dans le panier est : " + nbArticle + " le prix du panier est de : " + totalPanier + "euros");
        }
    }
}
