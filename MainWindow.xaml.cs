using System.Windows;
using MTPAutoCopier.ViewModels;

namespace MTPAutoCopier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainVm ViewModel = new MainVm();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
