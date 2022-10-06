using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PostgresRestore.Helper;
using PostgresRestore.Vo;

namespace PostgresRestore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadCombobox();
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            
        }
        
        private void LoadCombobox()
        {
            var actionCombobox = this.Find<ComboBox>("ActionComboBox");
            actionCombobox.Items = new List<object>()
            {
                new { Name = Constants.ActionTypeConstants.CreateAndRestore},
                new { Name = Constants.ActionTypeConstants.DropAndRestore }
            };
            actionCombobox.SelectedIndex = 0;
        }

        private void Restore(object? sender, RoutedEventArgs e)
        {
            var progressbar = this.Find<ProgressBar>("ProgressBar");
            try
            {
                progressbar.Value = 30;
                progressbar.IsIndeterminate = true;
                PersistPostgresCredentials();

                var databaseUsername = this.Find<TextBox>("PostgresUsername");
                var databasePassword = this.Find<TextBox>("PostgresPassword");
                var databaseName = this.Find<TextBox>("DatabaseName");
                var pgRestore = this.Find<RadioButton>("TypePgRestore");
                var backupType = this.Find<ComboBox>("ActionComboBox");
                var restoreFileLocation = this.Find<TextBox>("FileTextBox");

                var connection = new UserConnectionVo()
                {
                    UserName = databaseUsername.Text,
                    Password = databasePassword.Text,
                    DatabaseName = databaseName.Text,
                    ActionTypeValue = pgRestore.IsChecked ?? false
                        ? Constants.ActionTypeConstants.CreateAndRestore
                        : Constants.ActionTypeConstants.DropAndRestore,
                    DatabaseBackupType = backupType.Name ?? string.Empty,
                    RestoreFileLocation = restoreFileLocation.Text

                };

                var bgw = new BackgroundWorker();

                bgw.DoWork += (object _, DoWorkEventArgs args) => { CommandExecutor.ExecuteRestore(connection); };

                bgw.RunWorkerCompleted += (object _, RunWorkerCompletedEventArgs args) =>
                {
                    if (args.Error == null)
                    {
                        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("title",
                                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed...");
                        messageBoxStandardWindow.Show();

                        // RestoreBtn.Text = "✅ Restore Completed";
                        // MessageBox.Show($"Database #{DatabaseElem.Text} restored successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // var msg = args.Error?.Message ?? "Error during operation";
                        // MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    progressbar.Value = 100;
                    progressbar.IsIndeterminate = false;
                    
                    var window = MessageBox.Avalonia.MessageBoxManager
                        .GetMessageBoxStandardWindow("Success",
                            "restore success");
                    window.Show();
                    FinalizeLoadingFinished();
                };
                bgw.RunWorkerAsync();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            finally
            {
                progressbar.Value = 0;
            }
        }
        
        private void FinalizeLoadingFinished()
        {
           
        }

        private void PersistPostgresCredentials()
        {
            
        }

        private async void BrowseBackUpFile(object? sender, RoutedEventArgs e)
        {
            var fileBrowser = new OpenFileDialog
            {
                AllowMultiple = false,
            };
            var fileName = (await fileBrowser.ShowAsync(this))?[0] ?? string.Empty;
            
            var databaseTextBox = this.Find<TextBox>("DatabaseName");
            var fileTextBox = this.Find<TextBox>("FileTextBox");

            fileTextBox.Text = fileName;

            if (string.IsNullOrWhiteSpace(fileName)) return;
            
            var databaseName = fileName.Split('/', '\\').LastOrDefault();
            if(fileName.Contains('_'))
            {
                databaseTextBox.Text = databaseName?.Split('_').FirstOrDefault();
            }
        }

        private void OnPasswordRemember(object? sender, RoutedEventArgs e)
        {
           //for now nothing happens
        }
    }
}