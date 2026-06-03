using SampleSpeechLiveAgents.Commons;
using SampleSpeechLiveAgents.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SampleSpeechLiveAgents.ViewModels
{
    public class CreateChatRoomViewModel : INotifyPropertyChanged
    {
        private Models.ChatRoomModel Model = new Models.ChatRoomModel();
        private Models.RetrieverModel RetrieverModel = new Models.RetrieverModel();

        private string _ChatRoomName = string.Empty;
        public string ChatRoomName
        {
            get { return _ChatRoomName; }
            set
            {
                _ChatRoomName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// リトリーバー一覧を表すコレクション
        /// </summary>
        public ObservableCollection<Models.TDataRetriever> Retrievers
        {
            get { return this.RetrieverModel.Retrievers; }
        }

        /// <summary>
        /// 利用リトリーバー
        /// </summary>
        public Models.TDataRetriever SelectedRetriever
        {
            get { return this.RetrieverModel.SelectedRetriever; }
            set { this.RetrieverModel.SelectedRetriever = value; }
        }

        /// <summary>
        /// Busy表示用
        /// </summary>
        private bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                _IsBusy = value;
                OnPropertyChanged("IsBusy");
                if (value)
                {
                    App.Current.MainWindow.Cursor = System.Windows.Input.Cursors.Wait;
                }
                else
                {
                    App.Current.MainWindow.Cursor = null;
                }
            }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public CreateChatRoomViewModel()
        {
            this.Model.PropertyChanged += (s, e) => { OnPropertyChanged(e.PropertyName); };
            this.RetrieverModel.PropertyChanged += (s, e) => { OnPropertyChanged(e.PropertyName); };
        }

        /// <summary>
        /// リトリーバー一覧取得
        /// </summary>
        /// <returns></returns>
        internal async Task GetRetrieversAsync()
        {
            await this.RetrieverModel.GetRetrieversAsync(true);
        }

        /// <summary>
        /// 設定保存
        /// </summary>
        RelayCommand _SaveCommand;
        public RelayCommand SaveCommand
        {
            get
            {
                if (_SaveCommand == null)
                {
                    _SaveCommand = new RelayCommand(async() =>
                    {
                        this.IsBusy = true;
                        try
                        {
                            var id = await this.Model.CreateChatRoomAsync(this.ChatRoomName, this.SelectedRetriever?.ID);
                            if (!string.IsNullOrEmpty(id))
                            {
                                // 作成したチャットルームを選択状態にする(MainVM側で管理しているため、MainVM経由で設定)
                                var item = App.MainVM.ChatRooms.FirstOrDefault((x) => x.ID == id);
                                if (item != null)
                                {
                                    App.MainVM.SelectedChatRoom = item;
                                }
                            }
                            OnMessaged("");
                        }
                        catch (Exception ex)
                        {
                            OnMessaged(ex.Message);
                        }
                        finally
                        {
                            this.IsBusy = false;
                        }
                    });
                }
                return _SaveCommand;
            }
            set
            {
                _SaveCommand = value;
            }
        }

        /// <summary>
        /// ダイアログ表示用イベント
        /// </summary>
        public event MessagedEventHandler Messaged;
        internal virtual void OnMessaged(String message = "")
        {
            this.Messaged?.Invoke(this, new MessageEventArgs(message));
        }

        // プロパティが変更されたときに通知するイベント
        public event PropertyChangedEventHandler PropertyChanged;

        // プロパティ変更通知を発行するメソッド
        protected virtual void OnPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}