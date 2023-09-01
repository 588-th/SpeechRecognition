using Common;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Interface.Controls
{
    /// <summary>
    /// Логика взаимодействия для FileExplorer.xaml
    /// </summary>
    public partial class FileExplorer : UserControl
    {
        public event EventHandler<string> FileSelected;
        public event EventHandler<string> DoubleClick;
        public string CurrentDirectory;
        public string CurrentFile;

        public FileExplorer()
        {
            InitializeComponent();
            CurrentDirectory = ProjectSettings.AudioTrainingFolderPath;
            DisplayFilesAndDirectories(CurrentDirectory);
        }

        private void DisplayFilesAndDirectories(string directoryPath)
        {
            try
            {
                var files = Directory.GetFiles(directoryPath);
                var directories = Directory.GetDirectories(directoryPath);

                FileListView.Items.Clear();

                FileListView.Items.Add(new ListViewItem { Content = "..", Tag = Directory.GetParent(CurrentDirectory).FullName, FontWeight = FontWeights.Bold });

                foreach (var directory in directories)
                {
                    var directoryName = new DirectoryInfo(directory).Name;
                    FileListView.Items.Add(new ListViewItem { Content = directoryName, Tag = directory, FontWeight = FontWeights.Bold });
                }

                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    FileListView.Items.Add(new ListViewItem { Content = $"{fileName}", Tag = file });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error occurred while accessing the directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FileListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (ListViewItem)FileListView.SelectedItem;
            if (selectedItem != null)
            {
                var selectedItemTag = (string)selectedItem.Tag;

                if (File.Exists(selectedItemTag))
                {
                    DoubleClick?.Invoke(this, selectedItemTag);
                }
                else if (Directory.Exists(selectedItemTag))
                {
                    CurrentDirectory = selectedItemTag;
                    DisplayFilesAndDirectories(CurrentDirectory);
                }
            }
        }

        private void FileListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ListViewItem)FileListView.SelectedItem;
            if (selectedItem != null)
            {
                var selectedItemTag = (string)selectedItem.Tag;

                if (File.Exists(selectedItemTag))
                {
                    CurrentFile = selectedItemTag;
                    FileSelected?.Invoke(this, selectedItemTag);
                }
            }
        }
    }
}