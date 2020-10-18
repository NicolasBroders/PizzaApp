using Newtonsoft.Json;
using PizzaApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xamarin.Forms;

namespace PizzaApp
{
    public partial class MainPage : ContentPage
    {
        public enum e_tri { TRI_AUCUN,
                            TRI_NOM,
                            TRI_PRIX,
                            TRI_FAV};

        List<Pizza> pizzas;
        List<String> pizzasFav = new List<String>();
        
        e_tri tri;

        string jsonFile;
        string tempFile;
        string favFile;

        const string KEY_TRI = "tri";
        const string KEY_FAV = "fav";
        public MainPage()
        {
            InitializeComponent();

            /*
            pizzasFav.Add("4 fromages");
            pizzasFav.Add("indienne");
            pizzasFav.Add("tartiflette");
            */

            tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp.json");
            jsonFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pizzas.json");

            favFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "fav.json");

            tri = e_tri.TRI_AUCUN;
            listView.IsVisible = false;
            waitLayout.IsVisible = true;

            if(File.Exists(jsonFile))
            {
                string pizzasJson = File.ReadAllText(jsonFile);
                if (!String.IsNullOrEmpty(pizzasJson))
                {
                    pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzasJson);
                    listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas),pizzasFav);
                    listView.IsVisible = true;
                    waitLayout.IsVisible = false;
                }
            }

            if (Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Application.Current.Properties[KEY_TRI];
                sortImage.Source = GetImageSourceFromTri(tri);
            }


            LoadFavList();



            listView.RefreshCommand = new Command((obj) =>
            {
                DownloadData((pizzas) =>
                {
                    if( pizzas != null){
                        listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas),pizzasFav);
                    }
                    listView.IsRefreshing = false;
                    
                });

            });



            //mettre ma fonction ici
            DownloadData((pizzas) =>
            {
                listView.IsVisible = true;
                waitLayout.IsVisible = false;
                if (pizzas != null)
                {
                    listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas),pizzasFav);
                }
            });


        }

        public void DownloadData(Action<List<Pizza>> action)
        {
            const string URL = "https://drive.google.com/uc?export=download&id=1sfak60ZfnR9O6mL7uefiMmneD4NZnXuM";

            using (var webClient = new WebClient())
            {
                webClient.DownloadFileCompleted += (sender, e) =>
                {

                    Exception ex = e.Error;

                    if(ex == null)
                    {
                        File.Copy(tempFile, jsonFile, true);
                        string pizzaJson = File.ReadAllText(jsonFile);
                        pizzas = JsonConvert.DeserializeObject<List<Pizza>>(pizzaJson);
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            action.Invoke(pizzas);

                        });

                    }
                    else
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("Erreur", "Une erreur réseau s'est produite: " + ex.Message, "OK");
                            action.Invoke(null);
                        });
                    }


                    
                };

                webClient.DownloadFileAsync(new Uri(URL), tempFile);
            }
        }

        private void SortButtonClicked(object sender, EventArgs e)
        {
            switch (tri)
            {
                case e_tri.TRI_AUCUN:
                    tri = e_tri.TRI_NOM;
                    break;

                case e_tri.TRI_NOM:
                    tri = e_tri.TRI_PRIX;
                    break;

                case e_tri.TRI_PRIX:
                    tri = e_tri.TRI_FAV; ;
                    break;
                case e_tri.TRI_FAV:
                    tri = e_tri.TRI_AUCUN;
                    break;
            }
            sortImage.Source = GetImageSourceFromTri(tri);
            listView.ItemsSource = GetPizzaCells(GetPizzasFromTri(tri, pizzas),pizzasFav);

            Application.Current.Properties[KEY_TRI] = (int)tri;
            Application.Current.SavePropertiesAsync();

        }

        public String GetImageSourceFromTri(e_tri t)
        {
            string res = "";
            switch (tri)
            {
                case e_tri.TRI_AUCUN:
                    res = "sort_none";
                    break;

                case e_tri.TRI_NOM:
                    res = "sort_nom";
                    break;

                case e_tri.TRI_PRIX:
                    res = "sort_prix";
                    break;

                case e_tri.TRI_FAV:
                    res = "sort_fav";
                    break;

                default:
                    res = "sort_none";
                    break;
            }
            return res;
        }

        public List<Pizza> GetPizzasFromTri(e_tri t, List<Pizza> l)
        {
            if (l == null)
            {
                return null;
            }

            //copie de la liste de toutes les pizzas
            List<Pizza> copy = new List<Pizza>(l);

            switch (tri)
            {
                case e_tri.TRI_AUCUN:
                    return l;
                    break;

                case e_tri.TRI_NOM:
                    copy.Sort((emp1, emp2) => emp1.Titre.CompareTo(emp2.Titre));
                    break;

                case e_tri.TRI_PRIX:
                    copy.Sort((emp1, emp2) => emp2.prix.CompareTo(emp1.prix));
                    break;

                case e_tri.TRI_FAV:
                    foreach(Pizza p in l)
                    {
                        if (!pizzasFav.Contains(p.nom))
                        {
                            copy.Remove(p);
                        }
                    }
                    copy.Sort((emp1, emp2) => emp1.Titre.CompareTo(emp2.Titre));
                    break;

                default:
                    return l;
                    break;
            }
            return copy;
        }

        private List<PizzaCell> GetPizzaCells(List<Pizza> pizzas, List<String> f)
        {
            List<PizzaCell> res = new List<PizzaCell>();

            if(pizzas == null)
            {
                return res;
            }

            foreach(Pizza p in pizzas)
            {
                bool isFav = f.Contains(p.nom);
                res.Add(new PizzaCell { pizza = p, isFavorite= isFav, favChangedAction = OnFavPizzaChanged });
            }

            return res;

        }

        private void OnFavPizzaChanged(PizzaCell pizzaCell)
        { 

            bool isInFavList = pizzasFav.Contains(pizzaCell.pizza.nom);
            if(isInFavList && !pizzaCell.isFavorite)
            {
                pizzasFav.Remove(pizzaCell.pizza.nom);
            }else if(!isInFavList && pizzaCell.isFavorite)
            {
                pizzasFav.Add(pizzaCell.pizza.nom);
            }
            SaveFavList();
        }

        private void SaveFavList()
        {
            string json = JsonConvert.SerializeObject(pizzasFav);
            Application.Current.Properties[KEY_FAV] = json;
            Application.Current.SavePropertiesAsync();
        }

        private void LoadFavList()
        {
            if (Application.Current.Properties.ContainsKey(KEY_FAV))
            {
                string json = Application.Current.Properties[KEY_FAV].ToString();
                pizzasFav = JsonConvert.DeserializeObject<List<string>>(json);
            }
        }
    }
}
