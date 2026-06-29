using SampleSpeechLiveAgents.Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace SampleSpeechLiveAgents.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Models.ChatModel Model = new Models.ChatModel();
        private Models.SpeechModel Speech = new Models.SpeechModel();
        private SynchronizationContext Context { get; } = SynchronizationContext.Current;

        /// <summary>
        /// IDトークン
        /// </summary>
        public string IdToken { get { return this.Model.IdToken; } }

        /// <summary>
        /// GAP接続済みかどうかを示すプロパティ
        /// </summary>
        public bool IsLogin
        {
            get { return this.Model.IsLogin; }
            set { OnPropertyChanged(); }
        }

        /// <summary>
        /// 初期設定済みかどうかを示すプロパティ
        /// </summary>
        public bool IsSettings
        {
            get { return this.Model.IsSettings; }
            set
            {
                OnMessaged("Disconnect");
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the username associated with the current user.
        /// </summary>
        public string UserName
        {
            get { return this.Model.UserName; }
        }

        /// <summary>
        /// チャットルーム一覧を表すコレクション。
        /// Model側のObservableCollectionをそのまま公開します。Viewはこのコレクションをバインドして利用します。
        /// <para>setterはModel.ChatRoomsへ代入するだけで、追加の通知処理は行いません（Model側での通知を期待します）。</para>
        /// </summary>
        public ObservableCollection<Models.TDataChatRoom> ChatRooms
        {
            get { return this.Model.ChatRooms; }
            set { this.Model.ChatRooms = value; }
        }

        /// <summary>
        /// 利用チャットルーム
        /// </summary>
        public Models.TDataChatRoom SelectedChatRoom
        {
            get { return this.Model.SelectedChatRoom; }
            set { this.Model.SelectedChatRoom = value; }
        }

        /// <summary>
        /// 会話一覧を表すコレクション。
        /// </summary>
        public ObservableCollection<Models.TMessage> Messages
        {
            get { return this.Model.Messages; }
            set { this.Model.Messages = value; }
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
                // UI スレッドで通知
                _IsBusy = value;
                OnPropertyChanged();
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
        /// 入力用テキスト
        /// </summary>
        private string _InputText = string.Empty;
        public string InputText
        {
            get { return _InputText; }
            set
            {
                if (_InputText != value)
                {
                    _InputText = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 参照ドキュメント一覧
        /// </summary>
        private List<string> _Refs { get; set; } = new List<string>();
        public List<string> Refs
        {
            get { return _Refs; }
            set
            {
                if (_Refs != value)
                {
                    _Refs = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 音声認識確定後自動送信フラグ
        /// </summary>
        private bool _IsAutoSend = false;
        public bool IsAutoSend
        {
            get { return _IsAutoSend; }
            set
            {
                if (_IsAutoSend != value)
                {
                    _IsAutoSend = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainViewModel()
        {
            // ModelのイベントをViewModelのイベントに転送する
            this.Model.PropertyChanged += async (s, e) =>
            {
                OnPropertyChanged(e.PropertyName);

                // チャットルームが変更になったので会話一覧を取得
                if (e.PropertyName == "SelectedChatRoom" && this.SelectedChatRoom != null)
                {
                    this.IsBusy = true;
                    try
                    {
                        await this.Model.GetChatsAsync(this.SelectedChatRoom.ID);
                    }
                    catch (Exception ex)
                    {
                        // コンストラクター内で例外が発生した場合はここでキャッチしてメッセージ表示
                        OnMessaged(ex.Message);
                    }
                    this.IsBusy = false;
                }
                else if (e.PropertyName == "IsLogin" && this.IsLogin == false)
                {
                    this.Messages.Clear();
                    this.ChatRooms.Clear();
                    this.SelectedChatRoom = null;

                    var cmd = this.SpeechCommand;
                    if (cmd != null)
                    {
                        // UI スレッドで実行
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (cmd.CanExecute(false))
                                cmd.Execute(false);
                        });
                    }
                }

                // Streaming完了通知を受け取った場合、処理中フラグをOFF
                if (e.PropertyName == "IsStreaming" && this.Model.IsStreaming == false)
                {
                    this.EndPreviewKeyDownCommand();
                }
            };

            this.Speech.PropertyChanged += async (s, e) =>
            {
                // 音声認識の結果を受け取る
                if (e.PropertyName == "RecognizedText")
                {
                    this.InputText = this.Speech.RecognizedText;
                }
                else if (e.PropertyName == "IsCompleted")
                {
                    if (this.IsAutoSend && this.Speech.IsCompleted)
                    {
                        try
                        {
                            // メッセージを追加（ここではローカルに追加するのみ）
                            var content = this.Speech.RecognizedText.Trim();

                            // 自動送信モードの場合はすぐに送信せず、Model側のキューに入れる
                            this.Model.EnqueueMessage(content);
                        }
                        catch (Exception ex)
                        {
                            // Command内で例外が発生した場合はここでキャッチしてメッセージ表示
                            OnMessaged(ex.Message);
                        }
                    }
                }
            };
        }

        /// <summary>
        /// 接続処理を非同期で実行
        /// </summary>
        /// <returns></returns>
        internal async Task ConnectAsync()
        {
            // 接続情報設定済の時は、ログイン処理を実行
            if (this.Model.IsSettings)
            {
                await this.Model.ConnectAsync();
            }
            else
            {
                OnMessaged("Settings");
            }
        }

        /// <summary>
        /// 切断処理を非同期で実行
        /// </summary>
        /// <returns></returns>
        internal async Task DisconnectAsync()
        {
            // 接続情報設定済の時は、ログアウト処理を実行
            if (this.Model.IsSettings)
            {
                await this.Model.DisconnectAsync();
            }
        }

        /// <summary>
        /// ルーム一覧取得
        /// </summary>
        /// <returns></returns>
        internal async Task GetChatRoomsAsync(bool isUseNone = false)
        {
            await this.Model.GetChatRoomsAsync(isUseNone);
        }

        /// <summary>
        /// チャットルーム作成処理
        /// </summary>
        /// <returns></returns>
        internal async Task<string> CreateChatRoomAsync(string name, string retrieverID)
        {
            return await this.Model.CreateChatRoomAsync(name, retrieverID);
        }


        /// <summary>
        /// チャットルーム内会話クリア処理
        /// </summary>
        /// <returns></returns>
        internal async Task ClearChatRoomAsync()
        {
            if (this.SelectedChatRoom != null)
            {
                await this.Model.ClearChatRoomAsync(this.SelectedChatRoom.ID);
                await this.Model.GetChatsAsync(this.SelectedChatRoom.ID);
            }
        }

        /// <summary>
        /// メッセージの内容をMarkdown形式で保存する
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal async Task SaveMessagesAsMarkdownAsync(string path)
        {
            this.IsBusy = true;
            try
            {
                await this.Model.SaveMessagesAsMarkdownAsync(path);
            }
            catch (Exception ex)
            {
                // ViewModel 内で発生したエラーは UI に通知するため OnMessaged で投げる
                OnMessaged(ex.Message);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        /// <summary>
        /// サブ画面表示
        /// </summary>
        RelayCommand<string> _ShowDialogCommand;
        public RelayCommand<string> ShowDialogCommand
        {
            get
            {
                if (_ShowDialogCommand == null)
                {
                    _ShowDialogCommand = new RelayCommand<string>((target) =>
                    {
                        this.IsBusy = true;
                        try
                        {
                            OnMessaged(target);
                        }
                        catch (Exception ex)
                        {
                            // Command内で例外が発生した場合はここでキャッチしてメッセージ表示
                            OnMessaged(ex.Message);
                        }
                        this.IsBusy = false;
                    });
                }
                return _ShowDialogCommand;
            }
            set
            {
                _ShowDialogCommand = value;
            }
        }

        /// <summary>
        /// 最新の入力メッセージまでを削除するコマンド（XAMLからバインド）
        /// </summary>
        private RelayCommand _DeleteMessageCommand;
        public RelayCommand DeleteMessageCommand
        {
            get
            {
                if (_DeleteMessageCommand == null)
                {
                    _DeleteMessageCommand = new RelayCommand(async () =>
                    {
                        this.IsBusy = true;
                        try
                        {
                            this.InputText = await this.Model.DeleteMessageAsync();
                        }
                        catch (Exception ex)
                        {
                            // Command内で例外が発生した場合はここでキャッチしてメッセージ表示
                            OnMessaged(ex.Message);
                        }
                        this.IsBusy = false;
                    });
                }
                return _DeleteMessageCommand;
            }
            set
            {
                _DeleteMessageCommand = value;
            }
        }

        /// <summary>
        /// PreviewKeyDown を受け取るコマンド（XAMLからバインド） 
        /// </summary>
        private RelayCommand<bool> _SpeechCommand;
        public RelayCommand<bool> SpeechCommand
        {
            get
            {
                if (_SpeechCommand == null)
                {
                    _SpeechCommand = new RelayCommand<bool>(async (target) =>
                    {
                        // 処理中でなければ送信処理を実行
                        if (!this.IsBusy)
                        {
                            this.IsBusy = true;
                            try
                            {
                                if (target)
                                {
                                    await this.Speech.StartAsync();
                                }
                                else
                                {
                                    await this.Speech.StopAsync(this.IsLogin);

                                    // メッセージを追加（ここではローカルに追加するのみ）
                                    var content = this.InputText.Trim();
                                    this.Model.EnqueueMessage(content);
                                }
                            }
                            catch (Exception ex)
                            {
                                this.IsBusy = false;
                                // Command内で例外が発生した場合はここでキャッチしてメッセージ表示
                                OnMessaged(ex.Message);
                            }
                            this.IsBusy = false;
                        }
                    });
                }
                return _SpeechCommand;
            }
            set
            {
                _SpeechCommand = value;
            }
        }
        private void EndPreviewKeyDownCommand()
        {
            this.InputText = string.Empty;
            this.IsBusy = false;
            OnMessaged("PreviewKeyDownCommand");
        }

        /// <summary>
        /// 参照ドキュメント表示コマンド
        /// </summary>
        RelayCommand<List<string>> _DisplayReferenceCommand;
        public RelayCommand<List<string>> DisplayReferenceCommand
        {
            get
            {
                if (_DisplayReferenceCommand == null)
                {
                    _DisplayReferenceCommand = new RelayCommand<List<string>>(async (refs) =>
                    {
                        this.IsBusy = true;
                        try
                        {
                            this.Refs = refs;
                            OnMessaged("DisplayReference");
                        }
                        catch (Exception ex)
                        {
                            // Command内で例外が発生した場合はここでキャッチしてメッセージ表示
                            OnMessaged(ex.Message);
                        }
                        this.IsBusy = false;
                    });
                }
                return _DisplayReferenceCommand;
            }
            set
            {
                _DisplayReferenceCommand = value;
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
