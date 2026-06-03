# 初めに
Fujitsu Cloud Service Genearative AI Platform の API を利用するための C# - WPF(.NET Framework) サンプルになります。  
実行ファイルはフォルダごとコピーすることでインストール作業なく Windows OS 環境で実行することができます。  
実行時の .NET Framework は、Windows 10 / 11 付属のものを利用します。

---
# 使い方
## 事前準備
本サンプルは、音声認識に Azure Speech を利用します。
事前に有効な Azure サブスクリプションにて、 Specch Service を有効化しておく必要要があります。  
https://portal.azure.com/#create/Microsoft.CognitiveServicesSpeechServices  
以下の説明では、リージョンとして「JapanEast」を設定して有効化した前提で説明しています。

## 起動
1. SampleSpeechLiveAgents.exe をクリックして起動
2. 【初回起動時のみ】  
自動的にテナント名とクライアントIDを入力する [ 設定 ] 画面を表示するので、利用している Generative AI Platform 環境のテナント名とクライアント ID を入力します。
3. 事前準備で有効化した Azure Speech のリージョン「JapanEast」とキーの値を設定して [OK] をクリックして保存してください。
4. 自動的にサインインが始まりますので、利用している EntraID の ID とパスワードを入力してください。
5. 「 General Use 」というルームがない場合は、自動的にRAGなしでルームを作成します。
6. 前回利用時のルームの内容を自動的に表示します。初回起動時のみ「 General Use 」ルームが選択されます。
- 「 General Use 」ルームは起動時に会話内容が全クリアされる特別なルームです。
- 他のルームは再起動しても会話内容は維持されます。

### [ 設定 ] 画面詳細
[ ファイル ]-[ 接続設定 ]メニューで接続情報を後から変更可能です。  

|  項目  |  用途  |
| ---- | ---- |
|  対話認証 | EntraID でサインインする場合に選択 |
|  TenantName | 契約時に通知されたテナント名 (gaXXXXXX) を設定 |
|  ClientId | 契約時に通知されたクライアントID |
|  (高度なオプション) | Menloなどブラウザ保護システムを採用するなど対話認証画面が表示できないときのみチェック |
|  非対話認証 | 秘密キーによる非対話が可能な契約時に選択 |
|  ClientSecret | 契約時に通知された秘密キーを設定 |
|  Speech key | Speech Service のキーを設定 |
|  Speech Region | Speech Service のリージョン (例:JapanEast) を設定 |

## 会話関連の操作
### 会話入力
- 画面右下の [ マイク ] アイコンをクリックし、音声を入力します。
- 音声入力が終わったら、再度 [ マイク ] アイコンをクリックして、音声認識結果を送信されます。
- 送信後、 AI からの回答が返ってくるまでも、音声入力はできますが、 送信は、 AI からの回答がくるまで不可となっています。

### 会話の削除
[ 入力 ] 欄の上の右側には [ 会話数 ] と [ ゴミ箱 ] アイコンがあります。  
[ ゴミ箱 ] アイコンをクリックすると、最新の質問とその質問に対する AI の回答が削除されます。  
同時に、[ 入力 ] 欄に最新の質問が自動的に設定されるので、 AI から別回答を得たいときなどに便利です。

### その他の機能については、 SampleCSharpUI の README を参照ください。
https://github.com/fujitsu/GAP-SampleCSharpUI/blob/main/README.md

---
# サンプルコードの build 方法
サンプルコードは、Visual Studio 2026 または、Visual Studio Code を使用して実行ファイルを build できます。  
Visual Studio 2026 であれば、SampleCSharpUI.sln を開いていただければ、あとは UI 上で実行や build が可能です。  
Visual Studio Code の場合は、環境設定などが必要です。

---
# 最後に
このサンプルが、Fujitsu Cloud Service Generative AI Platform を活用し、皆様のユーザーエクスペリエンスを飛躍的に進化させる革新的なアプリケーションを生み出すためのインスピレーションとなれば幸いです。  
さらに、本サンプルではチャットや RAG の管理においても操作性を追求した実装をアプリ側で行っていますので、皆様が開発されるアプリの管理画面設計のヒントとしてぜひご活用ください。