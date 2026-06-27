# Home Scene Setup (ホーム画面の作成手順)

以下は Unity Editor でホーム画面（スタートボタン + 何人で遊ぶか選択）を作る手順です。

1. 新しいシーンを作成して名前を `Home` にする（例: `Assets/Scenes/Home.unity`）。
2. シーンを保存して Build Settings に追加する（`File > Build Settings` -> `Add Open Scenes`）。
3. Hierarchy に次のオブジェクトを作成する:
   - `Canvas` (Render Mode = Screen Space - Overlay)
     - `Panel` (任意の背景)
       - `Title` (Text / TMP - 例: "Bowling")
       - `PlayerCountLabel` (Text/TMP - 例: "Players")
       - `PlayerCountValue` (Text/TMP) ← `HomeMenuManager.playerCountTextTMP` または `playerCountTextUI` に割り当て
       - `MinusButton` (Button) ← OnClick に `HomeMenuManager.DecreasePlayerCount()` を割り当て
       - `PlusButton` (Button) ← OnClick に `HomeMenuManager.IncreasePlayerCount()` を割り当て
       - （代替）`Dropdown` (UI->Dropdown) を使用する場合は、Options を `1,2,3,4` にして `OnValueChanged(int)` に `HomeMenuManager.SetPlayerCountFromDropdown(int)` を割り当て
       - `StartButton` (Button) ← OnClick に `HomeMenuManager.StartGame()` を割り当て

4. 空の GameObject を作成して名前を `HomeMenuManager` にする。
   - `Add Component` で `HomeMenuManager` スクリプトをアタッチ
   - Inspector で `PlayerCountValue` の Text/TMP を割り当て
   - `gameSceneName` に実際のゲームシーン名（例: `BowlingGame`）を設定

5. ビルド設定にゲームシーン（`BowlingGame` など）を追加して、`Home` シーンからロードできるようにする。

6. 実行して Start ボタンを押すと `GameSettings.PlayerCount` に選択値が保存され、指定したシーンがロードされます。

Notes:
- TextMesh Pro のテキストを使う場合は `playerCountTextTMP` を割り当て、通常の UI Text を使う場合は `playerCountTextUI` を割り当ててください。
- 初期プレイヤー数は 1、最大は 4 に制限されています。
