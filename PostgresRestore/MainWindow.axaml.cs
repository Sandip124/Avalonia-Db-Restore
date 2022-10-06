using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaEdit.Highlighting;
using MessageBox.Avalonia.Enums;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PostgresRestore.Constants;
using PostgresRestore.Helper;
using PostgresRestore.Validator;
using PostgresRestore.Vo;

namespace PostgresRestore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadCombobox();
        }

        private void LoadCombobox()
        {
            var actionCombobox = this.Find<ComboBox>("ActionComboBox");
            actionCombobox.Items = new List<object>()
            {
                new { Name = ActionTypeConstants.CreateAndRestore },
                new { Name = ActionTypeConstants.DropAndRestore }
            };
            actionCombobox.SelectedIndex = 0;
        }

        private bool _isRestoring;

        private void Restore(object? sender, RoutedEventArgs e)
        {
            if (_isRestoring) return;
            var progressbar = this.Find<ProgressBar>("ProgressBar");
            var restoreText = this.Find<TextBlock>("RestoreText");
            try
            {
                _isRestoring = true;
                restoreText.Text = "Restoring";
                progressbar.Value = 20;
                progressbar.IsIndeterminate = true;

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
                        ? CommandTypeConstants.PgRestore
                        : CommandTypeConstants.PgDump,
                    ActionTypeValue = backupType.SelectedIndex == 0
                        ? ActionTypeConstants.CreateAndRestore
                        : ActionTypeConstants.DropAndRestore,
                    RestoreFileLocation = restoreFileLocation.Text
                }.Validate();

                var bgw = new BackgroundWorker();

                bgw.DoWork += (_, _) => { CommandExecutor.ExecuteRestore(connection); };

                bgw.RunWorkerCompleted += (_, args) =>
                {
                    if (args.Error == null)
                    {
                        MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("Success",
                                $"Database '{databaseName.Text}' restored successfully", ButtonEnum.Ok,
                                MessageBox.Avalonia.Enums.Icon.Success).Show();
                        
                        restoreFileLocation.Text = string.Empty;
                        databaseName.Text = string.Empty;

                    }
                    else
                    {
                        MessageBox.Avalonia.MessageBoxManager
                            .GetMessageBoxStandardWindow("Error",
                                args.Error?.Message ?? "Error during operation", ButtonEnum.Ok, MessageBox.Avalonia
                                    .Enums.Icon.Error).Show();
                    }
                    
                    restoreText.Text = "Restore";
                    progressbar.Value = 0;
                    progressbar.Background = Brushes.White;
                    progressbar.IsIndeterminate = false;
                    _isRestoring = false;
                };
                bgw.RunWorkerAsync();
            }
            catch (Exception exception)
            {
                MessageBox.Avalonia.MessageBoxManager
                    .GetMessageBoxStandardWindow("Error",
                        exception.Message, ButtonEnum.Ok, MessageBox.Avalonia.Enums.Icon.Info).Show();
            }
            finally
            {
                Environment.SetEnvironmentVariable(PostgresConstants.PasswordKey, string.Empty);
            }
        }

        private async void BrowseBackUpFile(object? sender, RoutedEventArgs e)
        {
            if (_isRestoring) return;

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
        
        private PixelPoint _lastLocation;
        private bool _mouseDown;
        

        private void Header_OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_mouseDown)
            {
                var xPosition = Position.X - _lastLocation.X + e.GetPosition(this).X;
                var yPosition = Position.Y - _lastLocation.Y + e.GetPosition(this).Y;
                Position = new PixelPoint(
                    (int)xPosition, (int)yPosition);
            }
        }
        
        private void Header_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _mouseDown = false;
        }

        private void Header_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _mouseDown = true;
            var point = e.GetPosition(this);
            _lastLocation = new PixelPoint((int)point.X,(int)point.Y);
        }
    }
}