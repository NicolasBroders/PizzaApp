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

        List<Pizza> pizzas;
        public MainPage()
        {
            InitializeComponent();
            

            listView.IsVisible = false;
            waitLayout.IsVisible = true;

            listView.RefreshCommand = new Command((obj) =>
            {
                DownloadData((pizzas) =>
                {
                    listView.IsRefreshing = false;
                    listView.ItemsSource = pizzas;
                });

            });

            //mettre ma fonction ici
            DownloadData((pizzas) =>
            {
                listView.IsVisible = true;
                waitLayout.IsVisible = false;
                listView.ItemsSource = pizzas;
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
    }
}
