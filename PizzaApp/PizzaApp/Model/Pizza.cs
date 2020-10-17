using PizzaApp.Extensions;
using System;

namespace PizzaApp.Model
{
    public class Pizza
    {
        public string nom { get; set; }

        public int prix { get; set; }

        public string url { get; set; }
        public string[] ingredients{ get; set;}

        public string PrixEuros { get { return prix + "€"; } }

        public string IngredientsStr { get {return  String.Join(", ", ingredients); } }

        public string Titre { get { return nom.PremiereLettreMajuscule(); } }

        public Pizza()
        {
        }
    }
}
