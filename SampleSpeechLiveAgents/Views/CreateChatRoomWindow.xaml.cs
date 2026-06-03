using SampleSpeechLiveAgents.ViewModels;
using System;
using System.Windows;

namespace SampleSpeechLiveAgents.Views
{
    public partial class CreateChatRoomWindow : Window
    {
        public CreateChatRoomViewModel ViewModel { get; } = new CreateChatRoomViewModel();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public CreateChatRoomWindow(MainWindow window)
        {
            this.Owner = window;
            InitializeComponent();

            this.Loaded += async (s, e) =>
            {
                this.ViewModel.IsBusy = true;
                try
                {
                    // 初期接続
                    await this.ViewModel.GetRetrieversAsync();
                }
                catch (Exception ex)
                {
                    // ViewModelの処理で例外が発生した場合はここでキャッチしてメッセージ表示
                    MessageBox.Show(this.Owner, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                this.ViewModel.IsBusy = false;
            };

            this.ViewModel.Messaged += (s, e) =>
            {
                if (e.Message == "")
                {
                    // Saveボタン押下時
                    this.Close();
                }
                else
                {
                    MessageBox.Show(this.Owner, e.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            };
        }
    }
}
