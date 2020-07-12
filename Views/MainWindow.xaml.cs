using System.Windows;
using MTPAutoCopier.ViewModels;

namespace MTPAutoCopier.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainVm _viewModel = new MainVm();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }
    }
}
