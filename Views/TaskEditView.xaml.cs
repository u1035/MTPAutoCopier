using System.Windows;

namespace MTPAutoCopier.Views
{
    /// <summary>
    /// Interaction logic for TaskEditView.xaml
    /// </summary>
    public partial class TaskEditView
    {
        public TaskEditView()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Owner = Application.Current.MainWindow;
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SourcePathDialog_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = @"Please select source path",
                SelectedPath = SourcePathTextBox.Text,
                ShowNewFolderButton = true
            })
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    SourcePathTextBox.Text = dialog.SelectedPath;
                }
            }
        }

        private void DestinationPathDialog_OnClick(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = @"Please select destination path",
                SelectedPath = DestinationPathTextBox.Text,
                ShowNewFolderButton = true
            })
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    DestinationPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
