using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using MessageBox.Avalonia.Enums;
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
                new { Name = Constants.ActionTypeConstants.CreateAndRestore },
                new { Name = Constants.ActionTypeConstants.DropAndRestore }
            };
            actionCombobox.SelectedIndex = 0;
        }

        private bool isRestoring = false;

        private void Restore(object? sender, RoutedEventArgs e)
        {
            if (isRestoring) return;
            var progressbar = this.Find<ProgressBar>("ProgressBar");
            var restoreText = this.Find<TextBlock>("RestoreText");
            try
            {
                isRestoring = true;
                restoreText.Text = "Restoring";
                progressbar.Value = 20;
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
                    DatabaseBackupType = pgRestore.IsChecked ?? false
                        ? Constants.CommandTypeConstants.PgRestore
                        : Constants.CommandTypeConstants.PgDump,
                    ActionTypeValue = backupType.SelectedIndex == 0
                        ? Constants.ActionTypeConstants.CreateAndRestore
                        : Constants.ActionTypeConstants.DropAndRestore,
                    RestoreFileLocation = restoreFileLocation.Text
                };

                var bgw = new BackgroundWorker();

                bgw.DoWork += (_, _) => { CommandExecutor.ExecuteRestore(connection); };

                bgw.RunWorkerCompleted += (_, args) =>
                {
                    try
                    {
                        if (args.Error != null) return;
                        MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("Success",
                                $"Database #{databaseName.Text} restored successfully", ButtonEnum.Ok,
                                MessageBox.Avalonia.Enums.Icon.Info).Show();
                        restoreText.Text = "Restore";
                        progressbar.Value = 0;
                        progressbar.Background = Brushes.White;
                        progressbar.IsIndeterminate = false;
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("Error",
                                exception.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info).Show();
                    }
                    finally
                    {
                        isRestoring = false;
                        FinalizeLoadingFinished();
                    }
                };
                bgw.RunWorkerAsync();
            }
            catch (Exception exception)
            {
                MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Error",
                        exception.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info).Show();
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
            if (isRestoring) return;

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
            if (fileName.Contains('_'))
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