using Microsoft.Win32;
using SampleSpeechLiveAgents.ViewModels;
using System;
using System.Windows;

namespace SampleSpeechLiveAgents.Views
{
    public partial class CreateRetrieverWindow : Window
    {
        public ManageRetrieversViewModel ViewModel { get; } = new ManageRetrieversViewModel();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public CreateRetrieverWindow(MainWindow window)
        {
            this.Owner = window;
            InitializeComponent();

            this.ViewModel.Messaged += (s, e) =>
            {
                switch (e.Message)
                {
                    case "":
                        {
                            // 作成成功時
                            MessageBox.Show(this.Owner, this.ViewModel.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                        break;
                    case "SelectFile":
                        {
                            // ファイル指定
                            var dlg = new OpenFileDialog
                            {
                                Filter = "RAG Files (*.txt;*.text;*.pdf;*.docx;*.pptx;*.html;*.md;*.csv;*.xlsx)|*.txt;*.text;*.pdf;*.docx;*.pptx;*.html;*.md;*.csv;*.xlsx",
                                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                            };
                            var ok = dlg.ShowDialog(this.Owner);
                            if (ok == true)
                            {
                                this.ViewModel.FileName = dlg.FileName;
                            }
                            else
                            {
                                this.ViewModel.FileName = string.Empty;
                            }
                        }
                        break;
                    case "SelectFolder":
                        {
                            // ファイル指定
                            var dlg = new System.Windows.Forms.FolderBrowserDialog() { SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) };
                            var result = dlg.ShowDialog();
                            if (result == System.Windows.Forms.DialogResult.OK)
                            {
                                this.ViewModel.FolderName = dlg.SelectedPath;
                            }
                            else
                            {
                                this.ViewModel.FolderName = string.Empty;
                            }
                        }
                        break;
                    default:
                        {
                            MessageBox.Show(this.Owner, e.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);

                        }
                        break;
                }
            };
        }
    }
}
