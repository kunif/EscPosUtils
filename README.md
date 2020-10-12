# EscPosUtils

This is a utility for ESC/POS commands.

Since it is under development, the API, functions, configuration, etc. may change significantly.

Currently, it have the following three projects.

- EscPosUtils: Library for Tokenize, Decode (and others) for ESC/POS commands

- EscPosDecode: A command line tool that parses binary data files for ESC/POS commands.

- TestEscPosUtils: EscPosUtils library test program.

## Development/Execuion environment

To develop and execute this program you need:

- Visual Studio 2019 or Visual Studio Community 2019  version 16.7.5
- .NET Standard 2.1
- .NET Core 3.1
- System.Drawing.Common 4.7.0
- System.Drawing.Primitives 4.3.0
- Microsoft.NET.Test.sdk 16.7.1
- MSTest.TestAdapter 2.1.2
- MSTest.TestFramework 2.1.2
- coverlet.collector 1.3.0


## EscPosUtils Features and API

namespase & using: kunif.EscPosUtils

Enum: EscPosCmdType

class: EscPosCmd, EscPosTokenizer, EscPosDecoder

EscPosCmdType: ESC/POS commands defined as Enums

EscPosCmd: Class that holds the analysis result of ESC/POS command data
- cmdtype: Enum value of ESC/POS command type
- cmddata: Command byte array data
- cmdlength: Command byte length
- paramdetail: Details of parameter values ​​analyzed by EscPosDecoder.Convert
- somebinary: Bitmap of graphics and user-defined characters parsed by EscPosDecoder.Convert

EscPosTokenizer: A class that separates ESC/POS command data for each command
- Scan: Tokenization method

  - Parameters

    - Byte array of ESC/POS command data to be separated
    - Initial state Printer/LineDisplay type
    - SBCS font size pattern
    - CJK MBCS font size pattern
    - LineDisplay font size pattern

  - Return value

    - List of EscPosCmd as a result of carving

EscPosDecoder: A class that adds the details of the command isolated by EscPosTokenizer.Scan.
- Convert: Parameter detail conversion Method

  - Parameters

    - List of EscPosCmd of EscPosTokenizer.Scan result

  - Return value

    - List of EscPosCmd with parameter analysis results added

- s_StringESCRICS : Dictionary for international character set processing

- s_cp???? : Dictionary for conversion of printable/displayable data visualization process

- GetEmbeddedESCtCodePage, PrtESCtCodePage, VfdESCtCodePage : Auxiliary methods and dictionaries for the above processing


## How to use EscPosDecode

The binary file in which the ESC/POS command is recorded is specified as a parameter and analyzed.

Main help

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

Font size pattern help

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

Code page and international character set help

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


## About TestEscPosUtils

There are mainly testing the processing results of EscPosTokenizer.Scan.


## Known issue items

The known issues are:

- Haven not checked the operation in detail just by creating it. Also, it is not compared with the print result of the printer.  
- The Thai character conversion processing dictionary only created the framework and does not reflect the actual character code.
- Some processing may be incorrect due to ambiguity, lack, mistakes, etc. in the specification information available on the Web.
- API, functions, configuration, etc. are subject to change.
- For example, the function to assemble ESC/POS commands and the simulation function of printers and linedisplay devices.
- Alternatively, existing functions may change the targets and parameters to be incorporated.


## Verification and customization

I would be grateful if you could send me command data samples for graphics, barcodes, user-defined characters, page mode processing, etc. for verification.

Another things, please send me the cut paper, label printing, MICR, check reading, etc. too. if you like.

However, I am only considering support for EPSON desktop POS printers for the time being. Printers of other companies and mobile printers are not covered.

If you want to add customizations for specific printer/vendor specific processing etc., you are free to do it yourself.

However, in that case, change all the information such as the file name and namespace to make it an independent file so that it can be used in parallel with this component.


## License

Licensed under the [zlib License](./LICENSE).
