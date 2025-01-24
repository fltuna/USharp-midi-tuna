[[English]](./README.md) [日本語]

# tuna's USharp MIDI

- [tuna's USharp MIDI](#tunas-usharp-midi)
  - [機能](#%E6%A9%9F%E8%83%BD)
  - [セットアップ](#%E3%82%BB%E3%83%83%E3%83%88%E3%82%A2%E3%83%83%E3%83%97)
    - [音声フォルダの構成](#%E9%9F%B3%E5%A3%B0%E3%83%95%E3%82%A9%E3%83%AB%E3%83%80%E3%81%AE%E6%A7%8B%E6%88%90)
  - [設定項目](#%E8%A8%AD%E5%AE%9A%E9%A0%85%E7%9B%AE)
    - [Use individual Sound Sources](#use-individual-sound-sources)
    - [Debug Mode](#debug-mode)
    - [Debug Log Output Target](#debug-log-output-target)
    - [ACCEPTABLE MIDI CHANNEL](#acceptable-midi-channel)
    - [ACCEPTABLE MIDI CCS](#acceptable-midi-ccs)
  - [VRChatで使用する際の問題](#vrchat%E3%81%A7%E4%BD%BF%E7%94%A8%E3%81%99%E3%82%8B%E9%9A%9B%E3%81%AE%E5%95%8F%E9%A1%8C)


## 機能

- MIDIの再生 (リアルタイム同期はまだ未実装です)
- 実験的なVelocityのサポート
- カスタムサウンド (個別で用意、もしくはピッチ変更で音階を確保)

## セットアップ


1. TextMeshProUGUIをインポート
2. `tuna's midi piano.unitypackage`をインポート
3. 音声の準備
    - 一つのノート毎に一つの音を再生する場合
        1. 音声ファイルとフォルダを準備する (詳細は [音声フォルダの構成](#sounds-folder-structure) を参照)
        2. 次の手順でツールを開く: Tools -> tuna -> Midi -> Audio Setup Tool
        3. `Audio Files Folder Path` に音声のルートフォルダを指定する (Assets/Sounds/5/ = Assets/Sounds/)
        4. `Parent Object` には、Prefab内にある `AudioSources` をドラッグアンドドロップで入れる
        5. "Setup Audio Objects" を押す
        6. `Use Individual Sound Sources` にチェックを入れる
        7. 音声の準備おｋ
    - 一つの音のピッチを変更する形で音を再生する場合
        1. 音声ファイルとフォルダを準備する (詳細は [音声フォルダの構成](#sounds-folder-structure) を参照)
        2. C5の音声ファイルを `5/C5` の様に配置する
        3. `Use Individual Sound Sources` にチェックを外す
        4. 音声の準備おｋ
4. Prefabのルートオブジェクトをクリックして、Scriptの設定を調整する (詳細は [設定項目](#Settings) を参照)
5. 完了


### 音声フォルダの構成

1. 音声ファイルは音階の名前で設定する必要があります。 (例: C1, CS2, D3, DS2) またスケール表記はFL Studio準拠なので、中心のC(MIDI 60)はC5になります。
2. フォルダはオクターブでネーミングする必要があります。 (例: 2, 3, 4)

フォルダ構成の例:

```
- root
    - Editor
    - Scripts
    - Sounds
        - 2
            - C2
            - CS2
            - D2
            - DS2
            - E2
            - FS2
            - G2
            - GS2
            - A2
            - AS2
            - B
        - 3
            - C3
            - CS3
            - ...
```



## 設定項目

### Use individual Sound Sources

これが無効の場合は、スクリプトは個別の音ではなくピッチ変更されたC5の音声を使用します。

### Debug Mode

デバッグモードを選択します。

- WORLD_TEXT: ワールドテキストに現在のMIDIの入力情報を出力します。
- CONSOLE: 全てのログがVRChatのコンソールに出力されます。 (このログはVRChatフォルダーのログにも保存されます)


### Debug Log Output Target

もしデバッグモードのWORLD_TEXTを使用したい場合は、Text Mesh Pro UGUIコンポーネントをここに入れてください。

### ACCEPTABLE MIDI CHANNEL

許可するMIDIチャンネルを指定します。

### ACCEPTABLE MIDI CCS

MIDI CCは今のところは実装していません。



## VRChatで使用する際の問題

VRChatでは同時に30個を超える音声を流すと、後から流す音声が再生されない問題(仕様?)があります。
そのため、同時になる音声の数を制限する形で対処しています。
