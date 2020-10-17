using Newtonsoft.Json;
using PizzaApp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                            TRI_PRIX };

        List<Pizza> pizzas;
        e_tri tri;

        const string KEY_TRI = "tri";
        public MainPage()
        {
            InitializeComponent();


            tri = e_tri.TRI_AUCUN;
            listView.IsVisible = false;
            waitLayout.IsVisible = true;

            if (Application.Current.Properties.ContainsKey(KEY_TRI))
            {
                tri = (e_tri)Application.Current.Properties[KEY_TRI];
                sortImage.Source = GetImageSourceFromTri(tri);
            }

            listView.RefreshCommand = new Command((obj) =>
            {
                DownloadData((pizzas) =>
                {
                    listView.IsRefreshing = false;
                    listView.ItemsSource = GetPizzasFromTri(tri, pizzas);
                });

            });

            //mettre ma fonction ici
            DownloadData((pizzas) =>
            {
                listView.IsVisible = true;
                waitLayout.IsVisible = false;
                listView.ItemsSource = GetPizzasFromTri(tri, pizzas);
            });


        }

        public void DownloadData(Action<List<Pizza>> action)
        {
            const string URL = "https://drive.google.com/uc?export=download&id=1sfak60ZfnR9O6mL7uefiMmneD4NZnXuM";

            using (var webClient = new WebClient())
            {
                webClient.DownloadStringCompleted += (sender, e) =>
                {
                    try
                    {
                        pizzas = JsonConvert.DeserializeObject<List<Pizza>>(e.Result);
                    }
                    catch (Exception ex)
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            DisplayAlert("Erreur", "Une erreur réseau s'est produite: " + ex.Message, "OK");
                            action.Invoke(null);
                        });
                    }


                    Device.BeginInvokeOnMainThread(() =>
                    {
                        action.Invoke(pizzas);

                    });

                };

                webClient.DownloadStringAsync(new Uri(URL));
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
                    tri = e_tri.TRI_AUCUN;
                    break;
            }
            sortImage.Source = GetImageSourceFromTri(tri);
            listView.ItemsSource = GetPizzasFromTri(tri, pizzas);

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

                default:
                    return l;
                    break;
            }
            return copy;
        }
    }
}
