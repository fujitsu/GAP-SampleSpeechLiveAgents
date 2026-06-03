using SampleSpeechLiveAgents.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SampleSpeechLiveAgents.Views
{
    public partial class ManageRetrieversWindow : Window
    {
        public ManageRetrieversViewModel ViewModel { get; } = new ManageRetrieversViewModel();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ManageRetrieversWindow(MainWindow window)
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

            this.ViewModel.Messaged += async (s, e) =>
            {
                this.ViewModel.IsBusy = true;
                try
                {
                    switch (e.Message)
                    {
                        case "Edit":
                            {
                                // 編集画面を表示
                                var subWindow = new EditRetrieverWindow(this.ViewModel.Retriever) { Owner = this };
                                if (subWindow.ShowDialog() == true)
                                {
                                    // ルーム編集
                                }
                                subWindow = null;

                                // リトリーバー一覧を再取得
                                await this.ViewModel.GetRetrieversAsync();
                            }
                            break;
                        case "ConfirmationRemove":
                            {
                                // 削除ボタン押下
                                var result = MessageBox.Show(this.Owner, "削除します。よろしいですか。", this.Title, MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                                if (result == MessageBoxResult.No)
                                {
                                    // いいえが選択された場合
                                }
                                else
                                {
                                    this.ViewModel.DeleteRetrieverAsync();
                                }
                            }
                            break;
                        default:
                            MessageBox.Show(this.Owner, e.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    // ViewModelの処理で例外が発生した場合はここでキャッチしてメッセージ表示
                    MessageBox.Show(this.Owner, ex.Message, this.Title, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                this.ViewModel.IsBusy = false;
            };
        }
    }
}
