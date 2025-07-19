using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokeManager.PokeManagement
{
    public class LocationModel : INotifyPropertyChanged
    {
        private bool isVisited_;
        public string Name { get; set; }

        public bool IsVisited
        {
            get => isVisited_;
            set
            {
                if (isVisited_ != value)
                {
                    isVisited_ = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisited)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
