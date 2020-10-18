using PizzaApp.Extensions;
using System;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;

namespace PizzaApp.Model
{
    public class PizzaCell : INotifyPropertyChanged
    {
        public bool isFavorite { get; set; }

        public Pizza pizza { get; set; }

        public string imageSource
        {
            get { return isFavorite ? "star2.png" : "star1.png"; }
        }

        public Command FavClickCommand { get; set; }

        public Action<PizzaCell> favChangedAction { get; set; }

        public PizzaCell()
        {
            FavClickCommand = new Command((obj) =>
            {
                isFavorite = !isFavorite;
                OnPropertyChanged("imageSource");
                favChangedAction.Invoke(this);
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
