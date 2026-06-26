using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using SampleSpeechLiveAgents.Commons;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SampleSpeechLiveAgents.Models
{
    internal class SpeechModel : INotifyPropertyChanged, IDisposable
    {
        private SpeechRecognizer Recognizer { get; set; }
        private SpeechConfig Config { get; set; }
        private SynchronizationContext Context { get; } = SynchronizationContext.Current;
        private bool IsRunning { get; set; }
        private bool Disposed { get; set; }
        private bool IsInitializing { get; set; } = false;
        private string Language { get; set; } = "ja-JP";

        private string _RecognizedText = string.Empty;
        public string RecognizedText
        {
            get => _RecognizedText;
            private set
            {
                if (value == _RecognizedText) return;
                _RecognizedText = value;
                // UI スレッドで通知
                if (Context != null)
                    Context.Post(_ => OnPropertyChanged(), null);
                else
                    OnPropertyChanged();
            }
        }

        private bool _IsCompleted = false;
        public bool IsCompleted
        {
            get => _IsCompleted;
            private set
            {
                if (value == _IsCompleted) return;
                _IsCompleted = value;
                // UI スレッドで通知
                if (Context != null)
                    Context.Post(_ => OnPropertyChanged(), null);
                else
                    OnPropertyChanged();
            }
        }

        /// <summary>
        /// 音声認識を開始する
        /// </summary>
        /// <returns></returns>
        internal async Task StartAsync()
        {
            if (this.Disposed) throw new ObjectDisposedException(nameof(SpeechModel));
            if (this.IsRunning) return;

            if (!this.IsInitializing) // 二重初期化を防止
            {
                this.IsInitializing = true;
                try
                {
                    InitializeRecognizer();
                }
                finally
                {
                    this.IsInitializing = false;
                }
            }

            try
            {
                this.IsRunning = true;
                RecognizedText = string.Empty;
                this.IsCompleted = false;
                await this.Recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            }
            catch
            {
                this.IsRunning = false;
                throw;
            }
        }

        /// <summary>
        /// 音声認識を停止する
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException"></exception>
        internal async Task StopAsync(bool isLogin)
        {
            if (this.Disposed) throw new ObjectDisposedException(nameof(SpeechModel));

            try
            {
                if (this.Recognizer != null)
                {
                    await this.Recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
                }
            }
            finally
            {
                this.IsRunning = false;
                if (!isLogin)
                {
                    // isLogin が false の場合は Cleanup 処理を呼び出す
                    this.CleanupRecognizer();
                }
            }
        }

        private void PostRecognizedText(string text, bool partial)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            if (partial)
            {
                // 部分結果は最終結果と区別して表示する
                var display = $"{text}...";
                if (this.Context != null)
                    this.Context.Post(_ => RecognizedText = display, null);
                else
                    RecognizedText = display;
            }
            else
            {
                if (this.Context != null)
                    this.Context.Post(_ => RecognizedText = text, null);
                else
                    RecognizedText = text;
            }
            this.IsCompleted = !partial; // 部分結果でない場合は完了とみなす
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void InitializeRecognizer()
        {
            // サブスクリプション方式で構成する（キーを URL クエリに置くべきではない）
            this.Config = SpeechConfig.FromSubscription(Commons.Config.AzureSpeechKey, Commons.Config.AzureSpeechRegion);
            this.Config.SpeechRecognitionLanguage = Language;

            // マイク入力（必要に応じて AudioConfig を注入可能）
            var audioConfig = AudioConfig.FromDefaultMicrophoneInput();

            // 既存認識器があれば破棄してから作成
            CleanupRecognizer();

            this.Recognizer = new SpeechRecognizer(this.Config, audioConfig);
            this.Recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    PostRecognizedText(e.Result.Text, partial: false);
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    // NoMatch の場合はログや UI 表示を行う（省略）
                }
            };
            this.Recognizer.Recognizing += (s, e) =>
            {
                // 部分結果を扱う（必要なら UI に反映）
                if (e.Result.Reason == ResultReason.RecognizingSpeech)
                {
                    // 一時表示用: 最終結果と区別して扱う実装にする
                    PostRecognizedText(e.Result.Text, partial: true);
                }
            };
            this.Recognizer.Canceled += (s, e) =>
            {
                // エラーやキャンセルを扱う。必要に応じて再初期化や通知を行う
                // 例: e.Reason, e.ErrorDetails に基づく処理
                this.IsCompleted = false;
            };
        }

        /// <summary>
        /// リソース開放
        /// </summary>
        private void CleanupRecognizer()
        {
            if (this.Recognizer != null)
            {
                try { this.Recognizer.Dispose(); } catch { /* ログは必要に応じて */ }
                this.IsRunning = false;
                this.IsInitializing = false;
                this.Recognizer = null;
            }
        }

        // プロパティが変更されたときに通知するイベント
        public event PropertyChangedEventHandler PropertyChanged;

        // プロパティ変更通知を発行するメソッド
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var handler = this.PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            if (this.Disposed) return;
            try { this.StopAsync(false).GetAwaiter().GetResult(); } catch { /* ログ */ }
            this.Disposed = true;
            CleanupRecognizer();
            this.Config = null;
        }
    }
}
