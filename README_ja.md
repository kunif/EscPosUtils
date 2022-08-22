# EscPosUtils

これはESC/POSコマンドのためのユーティリティです。

開発途中のため、API・機能・構成などは大きく変わる可能性があります。

現在は以下4つのプロジェクトがあります。

- EscPosUtils : ESC/POSコマンドのTokenize,Decode,Encode(その他追加予定)のためのライブラリ

- EscPosDecode : ESC/POSコマンドのバイナリデータファイルを解析するコマンドラインツール

- EscPosEncode : ESC/POSコマンドのバイナリデータファイルを作成するサンプルプログラム

- TestEscPosUtils : EscPosUtilsライブラリのテストプログラム


## 開発/実行環境

このプログラムの開発および実行には以下が必要です。

- Visual Studio 2022またはVisual Studio Community 2022 version 17.3.1
- .NET 6.0-Windows
- .NET Standard 2.1
- .NET Core App 3.1
- KGySoft.Drawing 6.3.2
- System.Drawing.Common 6.0.0
- System.Drawing.Primitives 4.3.0
- System.Text.Encoding.CodePages 6.0.0
- Microsoft.NET.Test.sdk 17.3.0
- MSTest.TestAdapter 2.2.10
- MSTest.TestFramework 2.2.10
- coverlet.collector 3.1.2


## EscPosUtilsの機能とAPI

namespase & using : kunif.EscPosUtils

Enum : EscPosCmdType

class : EscPosCmd, EscPosTokenizer, EscPosDecoder, EscPosEncoder

EscPosCmdType : ESC/POSの各コマンドをEnumとして定義したもの

EscPosCmd : ESC/POSコマンドデータおよび解析結果を保持するクラス
- cmdtype : ESC/POSコマンド種別のEnum値
- cmddata : コマンドのバイト配列データ
- cmdlength : コマンドのバイト単位長さ
- paramdetail : EscPosDecoder.Convertで解析したパラメータ値の詳細
- somebinary : EscPosDecoder.Convertで解析したグラフィックおよびユーザー定義文字のBitmap

EscPosTokenizer : ESC/POSコマンドデータをコマンド毎に切り分けるクラス
- Scan : 切り分けメソッド

  - パラメータ

    - 切り分け対象ESC/POSコマンドデータのバイト配列
    - 初期状態のPrinter/LineDisplay種別
    - SBCSのフォントサイズパターン
    - CJK MBCSのフォントサイズパターン
    - LineDisplayのフォントサイズパターン

  - 戻り値

    - 切り分けた結果のEscPosCmdのList

EscPosDecoder : EscPosTokenizer.Scanで切り分けたコマンドの詳細を付加するクラス
- Convert : パラメータ詳細変換メソッド

  - パラメータ

    - EscPosTokenizer.Scan結果のEscPosCmdのList

  - 戻り値

    - パラメータの解析結果を付加したEscPosCmdのList

- s_StringESCRICS : 国際文字セット処理用の辞書

- s_cp???? : 印刷・表示可能データの可視化処理の変換用辞書

- GetEmbeddedESCtCodePage, PrtESCtCodePage, VfdESCtCodePage : 上記処理の補助用メソッドと辞書

EscPosEncoder : ESC/POSコマンドデータを作成してEscPosCmdのListとして保持するクラス


## EscPosDecodeの使い方

ESC/POSコマンドを記録したバイナリファイルをパラメータに指定して解析します。

メインヘルプ

    Usage: EscPosDecode  InputFilePath  [Options]
      InputFilePath :  specify input ESC/POS binary file path. required.
      -H :  display this usage message.
      -F :  display supported font size pattern detail message.
      -L :  display supported CodePage and International character set type list message.
      -D {Printer | LineDisplay} :  specify initial device type. default is Printer.
      -O OutputFilePath :  specify output tokenized (and decoded) text file path. default is STDOUT.
      -T :  specify Tokenize only. default are both tokenize and decode.
      -C CodePage :  specify initial CodePage number. default system setting of Language for non-Unicode programs.
      -I International character set type :  specify initial International character set type. default 0
      -G GraphicsFolderPath :  specify decoded graphics&font output folder path. default&tokenize only are not output.
      -S FontPattern :  specify SBCS supported font size pattern 1 to 9. default is 1.
      -M FontPattern :  specify CJK MBCS supported font size pattern 1 to 5. default is 1.
      -V FontPattern :  specify LineDisplay supported font size pattern 1 or 2. default is 1.

フォントサイズパターンヘルプ

    Supported Font size pattern:
    SBCS(Single Byte Character Sequence)
    Pattern   1            2            3            4            5
    Font Width Height Width Height Width Height Width Height Width Height
      A    12 x 24      12 x 24      12 x 24      12 x 24      12 x 24
      B    10 x 24      10 x 24      10 x 24       8 x 16       9 x 17
      C     8 x 16       8 x 16       9 x 17
    sp.A                24 x 48

    Pattern   6            7            8            9
    Font Width Height Width Height Width Height Width Height
      A    12 x 24      12 x 24      12 x 24       9 x 9
      B     9 x 24       9 x 24       9 x 24       7 x 9
      C     9 x 17       9 x 17
      D    10 x 24      10 x 24
      E     8 x 16       8 x 16
    sp.A                12 x 24      12 x 24
    sp.B                 9 x 24       9 x 24

    CJK MBCS(Multi Byte Character Sequence)
    Pattern   1            2            3            4            5
    Font Width Height Width Height Width Height Width Height Width Height
      A    24 x 24      24 x 24      24 x 24      24 x 24      16 x 16
      B    20 x 24      20 x 24      16 x 16
      C    16 x 16

    LineDisplay
    Pattern   1            2
         Width Height Width Height
            8 x 16       5 x 7

コードページと国際文字セットヘルプ

    Supported CodePgae list:
    System or bundled package supported
      437: USA, Standard Europe  720: Arabic                737: Greek                 775: Baltic Rim
      850: Multilingual          852: Latin 2               855: Cyrillic              857: Turkish
      858: Euro                  860: Portuguese            861: Icelandic             862: Hebrew
      863: Canadian-French       864: Arabic                865: Nordic                866: Cyrillic #2
      869: Greek                 932: Japanese              1250: Latin 2              1251: Cyrillic
      1252: Windows              1253: Greek                1254: Turkish              1255: Hebrew
      1256: Arabic               1257: Baltic Rim           1258: Vietnamese           28592: ISO8859-2 Latin 2
      28597: ISO8859-7 Greek     28605: ISO8859-15 Latin 9  57002: Devanagari, Marathi 57003: Bengali
      57004: Tamil               57005: Telugu              57006: Assamese            57007: Oriya
      57008: Kannada             57009: Malayalam           57010: Gujarati            57011: Punjabi

    Depends on Printer Model
      949: Korea                 950: Traditiona Chinese    54936: GB18030             65001: UTF-8

    Special embedding support
      6: Hiragana                7: OnePass Kanji 1         8:OnePass Kanji 2          11: PC851 Greek
      12: PC853 Turkish          20-26: ASCII+ThaiSpecific  30: TCVN-3 1               31: TCVN-3 2
      41: PC1098 Farsi           42: PC1118 Lithuanian      43: PC1119 Lithuanian      44: PC1125 Ukrainian
      53: KZ-1048 Kazakhstan     254-255: ASCII+HexaDecimal

    International Character Set type:
      0: U.S.A.       1: France       2: Germany      3: U.K.         4: Denmark I    5: Sweden
      6: Italy        7: Spain I      8: Japan        9: Norway       10: Denmark II  11: Spain II
      12: Latin America   13: Korea   14: Slovenia/Croatia   15: China   16: Vietnam   17: Arabia
     India
      66: Devanagari  67: Bengali     68: Tamil       69: Telugu      70: Assamese    71: Oriya
      72: Kannada     73: Malayalam   74: Gujarati    75: Punjabi     82: Marathi


## TestEscPosUtilsについて   

主にEscPosTokenizer.Scanの処理結果についてテストしています。


## 既知の課題項目   

既知の課題は以下になります。

- 作成しただけであまり詳細な動作確認は行っていません。またプリンタでの印刷結果との比較も行っていません。  
- タイ語文字の変換処理用辞書は、枠組みを作成しただけで実際の文字コードを反映していません。  
- Web上で入手出来る仕様情報に曖昧・不足・間違い等があるため、一部の処理が間違っている可能性があります。
- API・機能・構成などは大きく変わる可能性があります。
- 例えばプリンタ・ラインディスプレイデバイスのシミュレーション機能など。  
- あるいは既存の機能でも組み込む対象やパラメータを変える可能性があります。

## 検証とカスタマイズ

グラフィック系・バーコード系・ユーザー定義文字・ページモード処理などのコマンドデータサンプルを検証用に送ってくださると幸いです。

あとは、カット紙・ラベル印刷・MICR・チェック読み取りなどについても良ければ送ってください。

ただし、当面EPSONのデスクトップ向けPOSプリンタへの対応だけを考えています。その他の会社のプリンタやモバイルプリンタは対象にしていません。

もし特定プリンタ/ベンダー固有の処理等のためのカスタマイズを加えたい場合、それは貴方自身で自由に行ってください。 
 
ただしその場合は、このサービスオブジェクトと同時に並行して使用しても問題無いように、ファイル名やnamespaceといった情報をすべて変更して独立したファイルにしてください。  

## ライセンス

[zlib License](./LICENSE) の下でライセンスされています。
