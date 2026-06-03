using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SampleSpeechLiveAgents.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private SynchronizationContext Context { get; set; } = SynchronizationContext.Current;

        public ViewModels.MainViewModel ViewModel { get; } = App.MainVM;

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                this.ViewModel.IsBusy = true;
                try
                {
                    // 初期接続
                    await App.MainVM.ConnectAsync();
                }
                catch (Exception ex)
                {
                    // ViewModelの処理で例外が発生した場合はここでキャッチしてメッセージ表示
                    App.MainVM.OnMessaged(ex.Message);
                }
                this.ViewModel.IsBusy = false;
            };

            // サブ画面表示
            App.MainVM.Messaged += async (s, e) =>
            {
                this.ViewModel.IsBusy = true;
                try
                {
                    switch (e.Message)
                    {
                        case "Connect":
                            {
                                await App.MainVM.ConnectAsync();
                            }
                            break;

                        case "Disconnect":
                            {
                                await App.MainVM.DisconnectAsync();
                            }
                            break;

                        case "CreateRetriever":
                            {
                                this.ShowDialog(new CreateRetrieverWindow(this) { Owner = this });
                            }
                            break;

                        case "ManageRetrievers":
                            {
                                this.ShowDialog(new ManageRetrieversWindow(this) { Owner = this });
                            }
                            break;

                        case "Settings":
                            {
                                this.ShowDialog(new SettingsWindow(this) { Owner = this });
                            }
                            break;

                        case "CreateChatRoom":
                            {
                                this.ShowDialog(new CreateChatRoomWindow(this) { Owner = this });
                            }
                            break;

                        case "ManageChatRooms":
                            {
                                this.ShowDialog(new ManageChatRoomsWindow(this) { Owner = this });
                            }
                            break;

                        case "ClearChatRoom":
                            {
                                await App.MainVM.ClearChatRoomAsync();
                            }
                            break;

                        case "EditChatRoomSettings":
                            {
                                // 編集画面を表示
                                var subWindow = new EditChatRoomWindow(this.ViewModel.SelectedChatRoom) { Owner = this };
                                if (subWindow.ShowDialog() == true)
                                {
                                    // チャットルーム一覧を再取得
                                    await this.ViewModel.GetChatRoomsAsync();
                                }
                                subWindow = null;
                            }
                            break;

                        case "About":
                            {
                                this.ShowDialog(new AboutWindow(this) { Owner = this });
                            }
                            break;

                        case "PreviewKeyDownCommand":
                            {
                                this.Focus();
                            }
                            break;

                        case "DisplayReference":
                            {
                                this.ShowDialog(new DisplayReferenceWindow(this) { Owner = this });
                            }
                            break;

                        case "Save":
                            {
                                var dlg = new Microsoft.Win32.SaveFileDialog()
                                {
                                    Filter = Properties.Resources.SaveFilter,
                                    DefaultExt = "md",
                                    FileName = $"Chat_{DateTime.Now:yyyyMMdd_HHmmss}.md",
                                    AddExtension = true,
                                    OverwritePrompt = true
                                };
                                var result = dlg.ShowDialog(this);
                                if (result == true)
                                {
                                    await this.ViewModel.SaveMessagesAsMarkdownAsync(dlg.FileName);
                                    MessageBox.Show(this, Properties.Resources.Saved, this.Title, MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            break;

                        default:
                            {
                                // ViewModelの処理で例外が発生した場合はここでキャッチしてメッセージ表示
                                try
                                {
                                    MessageBox.Show(this, e.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }
                                catch (Exception) { }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        MessageBox.Show(this, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    catch (Exception) { }
                }
                this.ViewModel.IsBusy = false;
            };

            App.MainVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Messages_Item")
                {
                    // 最新メッセージが追加されたときに最下部までスクロールする
                    var border = VisualTreeHelper.GetChild(this.listBoxMessage, 0) as Border;
                    if (border != null)
                    {
                        var listBoxScroll = border.Child as ScrollViewer;
                        if (listBoxScroll != null)
                        {
                            // スクロールバーを末尾に移動 
                            listBoxScroll.ScrollToEnd();
                        }
                    }
                }
            };
        }

        // メイン画面を使用不可にして疑似的なDialogとする（サブスクリーン含め移動などは可能）
        private void ShowDialog(Window target)
        {
            target.Closed += (_, __) =>
            {
                this.IsEnabled = true;
                this.Activate();
            };
            this.IsEnabled = false;
            target.Show();
        }
    }
}
