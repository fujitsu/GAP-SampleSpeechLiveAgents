using SampleSpeechLiveAgents.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SampleSpeechLiveAgents.Models
{
    internal class ChatRoomModel : INotifyPropertyChanged, IDisposable
    {
        // IDトークン(有効期限があるのでMainVMから参照)
        private string IdToken { get { return App.MainVM.IdToken; } }

        /// <summary>
        /// 対象ルーム
        /// </summary>
        private APIData.TChat _ChatRoom = null;
        public APIData.TChat ChatRoomData
        {
            get { return this._ChatRoom; }
            set
            {
                this._ChatRoom = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// ルーム名
        /// </summary>
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
        /// ランダム性
        /// </summary>
        private float _Temperature = 0.7f;
        public float Temperature
        {
            get { return _Temperature; }
            set
            {
                _Temperature = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 履歴トークン数
        /// </summary>
        private int _HistoryTokenBudget = 1000;
        public int HistoryTokenBudget
        {
            get { return _HistoryTokenBudget; }
            set
            {
                _HistoryTokenBudget = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 参照文書トークン数
        /// </summary>
        private int _DocumentTokenBudget = 1000;
        public int DocumentTokenBudget
        {
            get { return _DocumentTokenBudget; }
            set
            {
                _DocumentTokenBudget = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 回答トークン
        /// </summary>
        private int _AnswerTokenBudget = 1000;
        public int AnswerTokenBudget
        {
            get { return _AnswerTokenBudget; }
            set
            {
                _AnswerTokenBudget = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public ChatRoomModel()
        {
        }

        /// <summary>
        /// チャットルーム取得処理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task<TDataRetriever> GetChatRoomAsync(string id, List<TDataRetriever> retrievers)
        {
            var selectedRetriever = new TDataRetriever();
            var jsonString = await HttpHelper.GetRequestAsync($"/api/v1/chats/{id}", this.IdToken);
            using (var json = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
            {
                var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(APIData.TChat));
                {
                    this.ChatRoomData = ser.ReadObject(json) as APIData.TChat;

                    // 取得値設定
                    this.ChatRoomName = this.ChatRoomData.name.Trim();
                    this.Temperature = this.ChatRoomData.chat_setting.temperature;
                    this.HistoryTokenBudget = this.ChatRoomData.chat_setting.tokens_budget.history;
                    this.DocumentTokenBudget = this.ChatRoomData.chat_setting.tokens_budget.documents;
                    this.AnswerTokenBudget = this.ChatRoomData.chat_setting.tokens_budget.answer;
                    selectedRetriever = retrievers.FirstOrDefault((x) => x.ID == this.ChatRoomData.retriever_ids.FirstOrDefault());
                    if (selectedRetriever == null)
                    {
                        selectedRetriever = retrievers[0];
                    }
                    json.Close();
                }
            }
            return selectedRetriever;
        }

        /// <summary>
        /// チャットルーム作成処理
        /// </summary>
        /// <param name="chatRoomName"></param>
        /// <param name="retrieverID"></param>
        /// <returns></returns>
        internal async Task<string> CreateChatRoomAsync(string chatRoomName, string retrieverID)
        {
            return await App.MainVM.CreateChatRoomAsync(chatRoomName, retrieverID);
        }

        /// <summary>
        /// チャットルーム設定処理
        /// </summary>
        /// <returns></returns>
        internal async Task SetChatRoomAsync(string retrieverID)
        {
            var id = this.ChatRoomData.id;

            // 更新値設定
            if (!string.IsNullOrEmpty(retrieverID) && this.DocumentTokenBudget == 0)
            {
                //history = string.IsNullOrEmpty(retrieverID) ? 125000 : 62500;
                //documents = string.IsNullOrEmpty(retrieverID) ? 0 : 62500;
                //answer = string.IsNullOrEmpty(retrieverID) ? 2048 : 2048;
                this.DocumentTokenBudget = this.HistoryTokenBudget / 2;
                this.HistoryTokenBudget = this.HistoryTokenBudget - this.DocumentTokenBudget;
            }
            this.ChatRoomData.name = this.ChatRoomName.Trim();
            this.ChatRoomData.chat_setting.temperature = this.Temperature;
            this.ChatRoomData.chat_setting.tokens_budget.history = this.HistoryTokenBudget;
            this.ChatRoomData.chat_setting.tokens_budget.documents = this.DocumentTokenBudget;
            this.ChatRoomData.chat_setting.tokens_budget.answer = this.AnswerTokenBudget;
            this.ChatRoomData.retriever_ids = string.IsNullOrEmpty(retrieverID) ? new string[] { } : new string[] { retrieverID };
            this.ChatRoomData.chat_template_id = string.IsNullOrEmpty(retrieverID) ? "builtin.chat" : "builtin.document_combine";

            // 更新APIコール 
            using (var ms = new MemoryStream())
            {
                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(APIData.TChat));
                {
                    serializer.WriteObject(ms, this.ChatRoomData);
                    var bodyJsonString = Encoding.UTF8.GetString(ms.ToArray());
                    var jsonString = await HttpHelper.PutRequestAsync($"/api/v1/chats/{id}", this.IdToken, bodyJsonString);
                }
            }
        }

        /// <summary>
        /// チャットルーム削除処理
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async Task DeleteChatRoomAsync(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                using (var ms = new MemoryStream())
                {
                    var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(TDataChatRoom));
                    {
                        var jsonString = await HttpHelper.DeleteRequestAsync($"/api/v1/chats/{id}", this.IdToken);
                    }
                }
            }
        }

        /// <summary>
        /// 既存チャットルームのレトリーバー置換処理
        /// </summary>
        /// <param name="oldRetrieverID"></param>
        /// <param name="newRetrieverID"></param>
        /// <returns></returns>
        internal async Task ReplaceRetrieverInOwnedRoomsAsync(string oldRetrieverID, string newRetrieverID)
        {
            foreach (var room in App.MainVM.ChatRooms)
            {
                // 置換対象のレトリーバーが設定されているか確認
                if (room.RetrieverIDs.Where((x) => x == oldRetrieverID).FirstOrDefault() != null)
                {
                    var jsonString = await HttpHelper.GetRequestAsync($"/api/v1/chats/{room.ID}", this.IdToken);
                    using (var json = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)))
                    {
                        var ser = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(APIData.TChat));
                        {
                            var roomData = ser.ReadObject(json) as APIData.TChat;

                            // LINQ による置換
                            if (roomData.retriever_ids != null && roomData.retriever_ids.Length > 0)
                            {
                                roomData.retriever_ids = roomData.retriever_ids
                                    .Select(id => string.Equals(id, oldRetrieverID, StringComparison.Ordinal) ? newRetrieverID : id)
                                    .ToArray();
                            }

                            json.Close();

                            // 更新APIコール 
                            using (var ms = new MemoryStream())
                            {
                                var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(APIData.TChat));
                                {
                                    serializer.WriteObject(ms, roomData);
                                    var bodyJsonString = Encoding.UTF8.GetString(ms.ToArray());
                                    jsonString = await HttpHelper.PutRequestAsync($"/api/v1/chats/{roomData.id}", this.IdToken, bodyJsonString);
                                }
                            }
                        }
                    }
                }
            }
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


        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        /// <remarks>Call this method when you are finished using the object to free unmanaged resources
        /// and perform other cleanup operations. After calling Dispose, the object should not be used.</remarks>
        public void Dispose()
        {
        }
    }
}
