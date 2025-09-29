using BasicCalcWpf.ViewModels;
using System.Windows;

namespace BasicCalcWpf.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
