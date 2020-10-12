/*

   Copyright (C) 2020 Kunio Fukuchi

   This software is provided 'as-is', without any express or implied
   warranty. In no event will the authors be held liable for any damages
   arising from the use of this software.

   Permission is granted to anyone to use this software for any purpose,
   including commercial applications, and to alter it and redistribute it
   freely, subject to the following restrictions:

   1. The origin of this software must not be misrepresented; you must not
      claim that you wrote the original software. If you use this software
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.

   2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original software.

   3. This notice may not be removed or altered from any source distribution.

   Kunio Fukuchi

 */

namespace kunif.EscPosUtils
{
    using System;

    /// <summary>
    /// Enum define of ESC/POS command type
    ///   //c are w/Cut Sheet Printers command docs
    /// </summary>
    public enum EscPosCmdType
    {
        None,
        NotEnough,
        Unknown,
        Controls,                               //  0x00-0x1F Code not used for the following control sequences
        PrtPrintables,                          //  0x20-0xFF
        HorizontalTab,                          //  HT    09
        PrintAndLineFeed,                       //  LF    0A
        FormFeedPrintAndReturnToStandardMode,   //  FF    0C  // in PageMode
        EndOfJob,                               //  FF    0C  // in StandardMode
        PrintAndReturnToStandardMode,           //c FF    0C  // in PageMode
        PrintAndEjectCutSheet,                  //c FF    0C  // in Standard Mode
        PrintAndCarriageReturn,                 //  CR    0D
        Cancel,                                 //  CAN   18
        PrintAndCarriageReturnLineFeed,         //  CR LF 0D 0A
        PrintAndLineFeedCarriageReturn,         //  LF CR 0A 0D

        // VFD
        VfdDisplayables,        //  0x20-0xFF

        VfdMoveCursorLeft,      //  BS    08
        VfdMoveCursorRight,     //  HT    09
        VfdMoveCursorDown,      //  LF    0A
        VfdHomePosition,        //  HOME  0B
        VfdClearScreen,         //  CLR   0C
        VfdMoveCursorLeftMost,  //  CR    0D
        VfdClearCursorLine,     //  CAN   18

        // DLE 0x10
        DleTransmitRealtimeStatus,          //  DLE EOT 10 04 01/02/03/04/07/08/12 [01/02/03]

        DleSendRealtimeRequest,             //  DLE ENQ 10 05 00/01/02
        DleGeneratePulseRealtime,           //  DLE DC4 10 14 01 00/01 01-08
        DleExecPowerOff,                    //  DLE DC4 10 14 02 01 08
        DleSoundBuzzRealtime,               //  DLE DC4 10 14 03 00-07 00 00-08 01 00
        DleTransmitSpcifiedStatusRealtime,  //  DLE DC4 10 14 07 01/02/04/05
        DleClearBuffer,                     //  DLE DC4 10 14 08 01 03 14 01 06 02 08
        DleUnknown,

        // ESC 0x1B
        EscPageModeFormFeed,                        //  ESC FF  1B 0C  // PrintData in Page Mode

        EscRightSideSpacing,                        //  ESC SP  1B 20 00-FF
        EscSelectPrintMode,                         //  ESC !   1B 21 bx0xxx00x
        EscAbsoluteVerticalPosition,                //  ESC $   1B 24 0000-FFFF
        EscSelectUserDefinedCharacterSet,           //  ESC %   1B 25 bnnnnnnnx
        EscDefineUserDefinedCharacters1224,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscDefineUserDefinedCharacters1024,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscDefineUserDefinedCharacters0924,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscDefineUserDefinedCharacters0917,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscDefineUserDefinedCharacters0909,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscDefineUserDefinedCharacters0709,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscDefineUserDefinedCharacters0816,         //  ESC &   1B 26 02/03 20-7E 20-7E 00-FF...
        EscBeeperBuzzer,                            //  ESC ( A 1B 28 41 04 00 30 30-3A 01-3F 0A-FF
        EscBeeperBuzzerM1a,                         //  ESC ( A 1B 28 41 03 00 61 01-07 00-FF
        EscBeeperBuzzerM1b,                         //  ESC ( A 1B 28 41 05 00 61 64 00-3F 00-FF 00-FF
        EscBeeperBuzzerOffline,                     //  ESC ( A 1B 28 41 07 00 62 30-33 01 64 00/FF 01-32/FF 01-32
        EscBeeperBuzzerNearEnd,                     //  ESC ( A 1B 28 41 07 00 63 30 01 64 00/FF 01-32/FF 01-32
        EscSpecifyBatchPrint,                       //  ESC ( Y 1B 28 59 02 00 00/01/30/31 00/01/30/31
        EscSelectBitImageMode,                      //  ESC *   1B 2A 00/01/20/21 0001-0960 00-FF...
        EscUnderlineMode,                           //  ESC -   1B 2D 00-02/30-32
        EscSelectDefaultLineSpacing,                //  ESC 2   1B 32
        EscLineSpacing,                             //  ESC 3   1B 33 00-FF
        EscReturnHome,                              //  ESC <   1B 3C
        EscSelectPeripheralDevice,                  //  ESC =   1B 3D 01-03 or 00-FF
        EscCancelUserDefinedCharacters,             //  ESC ?   1B 3F 20-7E
        EscInitialize,                              //  ESC @   1B 40
        EscSetCutSheetEjectLength,                  //c ESC C   1B 43 00-FF
        EscHorizontalTabPosition,                   //  ESC D   1B 44 [01-FF]... 00
        EscTurnEmphasizedMode,                      //  ESC E   1B 45 bnnnnnnnx
        EscSetCancelCutSheetReverseEject,           //c ESC F   1B 46 bnnnnnnnx
        EscTurnDoubleStrikeMode,                    //  ESC G   1B 47 bnnnnnnnx
        EscPrintAndFeedPaper,                       //  ESC J   1B 4A 00-FF
        EscPrintAndReverseFeed,                     //  ESC K   1B 4B 00-30
        EscSelectPageMode,                          //  ESC L   1B 4C
        EscSelectCharacterFont,                     //  ESC M   1B 4D 00-04/30-34/61/62
        EscSelectInternationalCharacterSet,         //  ESC R   1B 52 00-11/42-4B/52
        EscSelectStandardMode,                      //  ESC S   1B 53
        EscSelectPrintDirection,                    //  ESC T   1B 54 00-03/30-33
        EscTurnUnidirectionalPrintMode,             //  ESC U   1B 55 bnnnnnnnx
        EscTurn90digreeClockwiseRotationMode,       //  ESC V   1B 56 00-02/30-32
        EscSetPrintAreaInPageMode,                  //  ESC W   1B 57 0000-FFFF 0000-FFFF 0001-FFFF 0001-FFFF
        EscSetRelativeHprizontalPrintPosition,      //  ESC \   1B 5C 8000-7FFF
        EscSelectJustification,                     //  ESC a   1B 61 00-02/30-32
        EscSelectPaperTypesPrinting,                //c ESC c 0 1B 63 30 b0000xxxx
        EscSelectPaperTypesCommandSettings,         //c ESC c 1 1B 63 31 b0x00xxxx
        EscSelectPaperSensorsPaperEndSignals,       //  ESC c 3 1B 63 33 b0000xxxx
        EscSelectPaperSensorsStopPrinting,          //  ESC c 4 1B 63 34 b0000xxxx
        EscEnableDisablePanelButton,                //  ESC c 5 1B 63 35 bnnnnnnnx
        EscPrintAndFeedNLines,                      //  ESC d   1B 64 00-FF
        EscPrintAndReverseFeedNLines,               //  ESC e   1B 65 00-02
        EscCutSheetWaitTime,                        //c ESC f   1B 66 00-0F 00-40
        EscObsoletePartialCut1Point,                //  ESC i   1B 69
        EscObsoletePartialCut3Point,                //  ESC m   1B 6D
        EscGeneratePulse,                           //  ESC p   1B 70 00/01/30/31 00-FF 00-FF
        EscReleasePaper,                            //c ESC q   1B 71
        EscSelectPrinterColor,                      //  ESC r   1B 72 00/01/30/31
        EscSelectCharacterCodeTable,                //  ESC t   1B 74 00-08/0B-1A/1E-35/42-4B/52/FE/FF
        EscObsoleteTransmitPeripheralDeviceStatus,  //  ESC u   1B 75 00/30
        EscObsoleteTransmitPaperSensorStatus,       //  ESC v   1B 76
        EscTurnUpsideDownPrintMode,                 //  ESC {   1B 7B bnnnnnnnx
        EscUnknown,

        // ESC 0x1B VFD
        VfdEscSelectCancelUserDefinedCharacterSet,  //  ESC %   1B 25 00-FF

        VfdEscDefineUserDefinedCharacters0816,      //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
        VfdEscDefineUserDefinedCharacters0507,      //  ESC &   1B 26 00/01 20-7E 20-7E 00-08 00-FF...
        VfdEscSelectPeripheralDevice,               //  ESC =   1B 3D 01/02/03
        VfdEscCancelUserDefinedCharacters,          //  ESC ?   1B 3F 20-7E
        VfdEscInitialize,                           //  ESC @   1B 40
        VfdEscSelectInternationalCharacterSet,      //  ESC R   1B 52 00-11
        VfdEscCancelWindowArea,                     //  ESC W   1B 57 01-04 00/30
        VfdEscSelectWindowArea,                     //  ESC W   1B 57 01-04 01/31 01-14 01-14 01/02 01/02
        VfdEscSelectCharacterCodeTable,             //  ESC t   1B 74 00-13/1E-35/FE/FF
        VfdEscUnknown,

        // FS 0x1C
        FsSelectPrintModeKanji,                                             //  FS  !   1C 21 bx000xx00

        FsSelectKanjiCharacterMode,                                         //  FS  &   1C 26
        FsSelectKanjiCharacterFont,                                         //  FS  ( A 1C 28 41 02 00 30 00-02/30-32
        FsSelectCharacterEncodeSystem,                                      //  FS  ( C 1C 28 43 02 00 30 01/02/31/32
        FsSetFontPriority,                                                  //  FS  ( C 1C 28 43 03 00 3C 00/01 00/0B/14/1E/29
        FsCancelSetValuesTopBottomLogo,                                     //  FS  ( E 1C 28 45 06 00 3C 02 30/31 43 4C 52
        FsTransmitSetValuesTopBottomLogo,                                   //  FS  ( E 1C 28 45 03 00 3D 02 30-32
        FsSetTopLogoPrinting,                                               //  FS  ( E 1C 28 45 06 00 3E 02 20-7E 20-7E 30-32 00-FF
        FsSetBottomLogoPrinting,                                            //  FS  ( E 1C 28 45 05 00 3F 02 20-7E 20-7E 30-32
        FsMakeExtendSettingsTopBottomLogoPrinting,                          //  FS  ( E 1C 28 45 0004-000C 40 02 [30/40-43 30/31]...
        FsEnableDisableTopBottomLogoPrinting,                               //  FS  ( E 1C 28 45 04 00 41 02 30/31 30/31
        FsPaperLayoutSetting,                                               //  FS  ( L 1C 28 4C 0008-001a 21 30-33 [[30-39]... 3B]...
        FsPaperLayoutInformationTransmission,                               //  FS  ( L 1C 28 4C 02 00 22 40/50
        FsTransmitPositioningInformation,                                   //  FS  ( L 1C 28 4C 02 00 30 30
        FsFeedPaperLabelPeelingPosition,                                    //  FS  ( L 1C 28 4C 02 00 41 30/31
        FsFeedPaperCuttingPosition,                                         //  FS  ( L 1C 28 4C 02 00 42 30/31
        FsFeedPaperPrintStartingPosition,                                   //  FS  ( L 1C 28 4C 02 00 43 30-32
        FsPaperLayoutErrorSpecialMarginSetting,                             //  FS  ( L 1C 28 4C 02 00 50 30-39 [30-39]
        FsEnableDisableAutomaticStatusBackOptional,                         //  FS  ( e 1C 28 65 02 00 33 b0000x000
        FsSelectMICRDataHandling,                                           //c FS  ( f 1C 28 66 0002-FFFF [00-03/30-33 00-FF|00/01/30/31]...
        FsSelectImageScannerCommandSettings,                                //c FS  ( g 1C 28 67 02 00 20 30/31
        FsSetBasicOperationOfImageScanner,                                  //c FS  ( g 1C 28 67 05 00 28 30 01/08 31/32 80-7F
        FsSetScanningArea,                                                  //c FS  ( g 1C 28 67 05 00 29 00-62 00-E4 00/02-64 00/02-E6
        FsSelectCompressionMethodForImageData,                              //c FS  ( g 1C 28 67 03 00 32 30-32 30-32
        FsDeleteCroppingArea,                                               //c FS  ( g 1C 28 67 02 00 38 00-0A
        FsSetCroppingArea,                                                  //c FS  ( g 1C 28 67 06 00 39 00-0A 00-64 00-E4 02-64 02-E6
        FsSelectTransmissionFormatForImageScanningResult,                   //c FS  ( g 1C 28 67 02 00 3C 30-32
        FsTransmitSettingValueForBasicOperationsOfImageScanner,             //c FS  ( g 1C 28 67 02 00 50 30
        FsTransmitSettingValueOfScanningArea,                               //c FS  ( g 1C 28 67 02 00 51 30
        FsTransmitSettingValueOfCompressionMethdForImageData,               //c FS  ( g 1C 28 67 02 00 5A 30
        FsTransmitSettingValueOfCroppingArea,                               //c FS  ( g 1C 28 67 02 00 61 30
        FsTransmitSettingValueOfTransmissionFormatForImageScanningResult,   //c FS  ( g 1C 28 67 02 00 64 30
        FsTurnKanjiUnderlineMode,                                           //  FS  -   1C 2D 00-02/30-32
        FsCancelKanjiCharacterMode,                                         //  FS  .   1C 2E
        FsDefineUserDefinedKanjiCharacters2424,                             //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 72
        FsDefineUserDefinedKanjiCharacters2024,                             //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 60
        FsDefineUserDefinedKanjiCharacters1616,                             //  FS  2   1C 32 7721-777E/EC40-EC9E/FEA1-FEFE 00-FF x 32
        FsCancelUserDefinedKanjiCharacters,                                 //  FS  ?   1C 3F 7721-777E/EC40-EC9E/FEA1-FEFE
        FsSelectKanjiCharacterCodeSystem,                                   //  FS  C   1C 43 00-02/30-32
        FsSelectDoubleDensityPageMode,                                      //c FS  L   1C 4C
        FsSetKanjiCharacerSpacing,                                          //  FS  S   1C 53 00-FF/00-20 00-FF/00-20
        FsTurnQuadrupleSizeMode,                                            //  FS  W   1C 57 bnnnnnnnx
        FsObsoleteReadCheckPaper,                                           //c FS  a 0 1C 61 30 b000000xx
        FsObsoleteLoadCheckPaperToPrintStartingPosition,                    //c FS  a 1 1C 61 31
        FsObsoleteEjectCheckPaper,                                          //c FS  a 2 1C 61 32
        FsObsoleteRequestTransmissionOfCheckPaperReadingResult,             //c FS  b  1C 62
        FsCleanMICRMechanism,                                               //c FS  c   1C 63
        FsObsoleteWriteNVUserMemory,                                        //  FS  g 1 1C 67 31 00 00000000-000003FF 0001-0400 20-FF...
        FsObsoleteReadNVUserMemory,                                         //  FS  g 2 1C 67 32 00 00000000-000003FF 0001-0400
        FsObsoletePrintNVBitimage,                                          //  FS  p   1C 70 01-FF 00-03/30-33
        FsObsoleteDefineNVBitimage,                                         //  FS  q   1C 71 01-FF [0001-03FF 0001-0240 00-FF...]...
        FsUnknown,

        // GS 0x1D
        GsSelectCharacterSize,                                          //  GS  !   1D 21 b0xxx0yyy

        GsSetAbsoluteVerticalPrintPositionInPageMode,                   //c GS  $   1D 24 0000-FFFF
        GsSetAbsoluteVerticalPrintPosition,                             //  GS  &   1D 26 0000-FFFF
        GsExecuteTestPrint,                                             //  GS  ( A 1D 28 41 02 00 00-02/30-32 01-03/31-33/40
        GsCustomizeASBStatusBits,                                       //c GS  ( B 1D 28 42 0002/0003/0005/0007/0009 61 00|[31/33/45/46 2C/2D/37/38]...
        GsDeleteSpecifiedRecord,                                        //  GS  ( C 1D 28 43 05 00 00 00/30 00 20-7E 20-7E
        GsStoreDataSpecifiedRecord,                                     //  GS  ( C 1D 28 43 0006-FFFF 00 01/31 00 20-7E 20-7E 20-FF...
        GsTransmitDataSpecifiedRecord,                                  //  GS  ( C 1D 28 43 05 00 00 02/32 00 20-7E 20-7E
        GsTransmitCapacityNVUserMemory,                                 //  GS  ( C 1D 28 43 03 00 00 03/33 00
        GsTransmitRemainingCapacityNVUserMemory,                        //  GS  ( C 1D 28 43 03 00 00 04/34 00
        GsTransmitKeycodeList,                                          //  GS  ( C 1D 28 43 03 00 00 05/35 00
        GsDeleteAllDataNVMemory,                                        //  GS  ( C 1D 28 43 06 00 00 06/36 00 43 4C 52
        GsEnableDisableRealtimeCommand,                                 //  GS  ( D 1D 28 44 0003/0005 14 [01/02 00/01/30/31]...
        GsChangeIntoUserSettingMode,                                    //  GS  ( E 1D 28 45 03 00 01 49 4E
        GsEndUserSettingMode,                                           //  GS  ( E 1D 28 45 04 00 02 4F 55 54
        GsChangeMeomorySwitch,                                          //  GS  ( E 1D 28 45 000A-FFFA 03 [01-08 30-32...]...
        GsTransmitSettingsMemorySwitch,                                 //  GS  ( E 1D 28 45 02 00 04 01-08
        GsSetCustomizeSettinValues,                                     //  GS  ( E 1D 28 45 0004-FFFD 05 [01-03/05-0D/14-16/46-48/61/62/64-69/6F/70/74-C2 0000-FFFF]...
        GsTransmitCustomizeSettingValues,                               //  GS  ( E 1D 28 45 02 00 06 01-03/05-0D/14-16/46-48/61/62/64-69/6F-71/74-C1
        GsCopyUserDefinedPage,                                          //  GS  ( E 1D 28 45 04 00 07 0A/0C11/12 30/31 31/30
        GsDefineColumnFormatCharacterCodePage,                          //  GS  ( E 1D 28 45 0005-FFFF 08 02/03 20-7E 20-7E [08/09/0A/0C 00-FF...]...
        GsDefineRasterFormatCharacterCodePage,                          //  GS  ( E 1D 28 45 0005-FFFF 09 01/02 20-7E 20-7E [10/11/18 00-FF...]...
        GsDeleteCharacterCodePage,                                      //  GS  ( E 1D 28 45 03 00 0A 80-FF 80-FF
        GsSetSerialInterface,                                           //  GS  ( E 1D 28 45 0003-0008 0B 01-04 30-39...
        GsTransmitSerialInterface,                                      //  GS  ( E 1D 28 45 02 00 0C 01-04
        GsSetBluetoothInterface,                                        //  GS  ( E 1D 28 45 0003-0021 0D [31/41/46/49 20-7E...]...
        GsTransmitBluetoothInterface,                                   //  GS  ( E 1D 28 45 02 00 0E 30/31/41/46/49
        GsSetUSBInterface,                                              //  GS  ( E 1D 28 45 03 00 0F 01/20 30/31
        GsTransmitUSBInterface,                                         //  GS  ( E 1D 28 45 02 00 10 01/20
        GsDeletePaperLayout,                                            //  GS  ( E 1D 28 45 04 00 30 43 4C 52
        GsSetPaperLayout,                                               //  GS  ( E 1D 28 45 0009-0024 31 {34 38/34 39/36 34} 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
        GsTransmitPaperLayout,                                          //  GS  ( E 1D 28 45 02 00 32 40/50
        GsSetControlLabelPaperAndBlackMarks,                            //  GS  ( E 1D 28 45 002D-0048 33 20-7E...
        GsTransmitControlSettingsLabelPaperAndBlackMarks,               //  GS  ( E 1D 28 45 11 00 34 20-7E... 00
        GsSetInternalBuzzerPatterns,                                    //  GS  ( E 1D 28 45 000E-FFFF 63 [01-05 [00/01 00-64]...]...
        GsTransmitInternalBuzzerPatterns,                               //  GS  ( E 1D 28 45 02 00 64 01-05
        GsTransmitStatusOfCutSheet,                                     //c GS  ( G 1D 28 47 02 00 20 b0100xxxx
        GsSelectSideOfSlipFaceOrBack,                                   //c GS  ( G 1D 28 47 02 00 30 04/44
        GsReadMagneticInkCharacterAndTransmitReadingResult,             //c GS  ( G 1D 28 47 04 00 3C 01 00 00/01
        GsRetransmitMagneticInkCharacterReadingResult,                  //c GS  ( G 1D 28 47 03 00 3D 01 00
        GsReadDataAndTransmitResultingInformation,                      //c GS  ( G 1D 28 47 0005-0405 40 0000-FFFF 00/01 01-03 00 30 0000 00-FF...
        GsScanImageDataAndTransmitImageScanningResult,                  //c GS  ( G 1D 28 47 0005-0405 41 0001-FFFF 30/31 30 00-FF...
        GsRetransmitImageScanningResult,                                //c GS  ( G 1D 28 47 0003-0004 42 0001-FFFF [30/31]
        GsExecutePreScan,                                               //c GS  ( G 1D 28 47 02 00 43 30
        GsDeleteImageScanningResultWithSpecifiedDataID,                 //c GS  ( G 1D 28 47 04 00 44 30 0001-FFFF
        GsDeleteAllImageScanningResult,                                 //c GS  ( G 1D 28 47 05 00 45 30 43 4C 52
        GsTransmitDataIDListOfImageScanningResult,                      //c GS  ( G 1D 28 47 04 00 46 30 49 44
        GsTransmitRemainingCapacityOfNVMemoryForImageDataStorage,       //c GS  ( G 1D 28 47 02 00 47 30
        GsSelectActiveSheet,                                            //c GS  ( G 1D 28 47 02 00 50 b00xxxxxx
        GsStartPreProcessForCutSheetInsertion,                          //c GS  ( G 1D 28 47 02 00 51 30
        GsEndPreProcessForCutSheetInsertion,                            //c GS  ( G 1D 28 47 02 00 52 30
        GsExecuteWaitingProcessForCutSheetInsertion,                    //c GS  ( G 1D 28 47 02 00 53 30
        GsFeedToPrintStartingPositionForSlip,                           //c GS  ( G 1D 28 47 02 00 54 01
        GsFinishProcessingOfCutSheet,                                   //c GS  ( G 1D 28 47 02 00 55 30/31
        GsSpecifiesProcessIDResponse,                                   //  GS  ( H 1D 28 48 06 00 30 30 20-7E 20-7E 20-7E 20-7E
        GsSpecifiesOfflineResponse,                                     //  GS  ( H 1D 28 48 03 00 31 30 00-02/30-32
        GsSelectPrintControlMode,                                       //  GS  ( K 1D 28 4B 02 00 30 00-04/30-34
        GsSelectPrintDensity,                                           //  GS  ( K 1D 28 4B 02 00 31 80-7F
        GsSelectPrintSpeed,                                             //  GS  ( K 1D 28 4B 02 00 32 00-0D/30-39
        GsSelectNumberOfPartsThermalHeadEnergizing,                     //  GS  ( K 1D 28 4B 02 00 61 00-04/30-34/80
        GsDefineNVGraphicsDataRasterW,                                  //  GS  ( L 1D 28 4C 000C-FFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        GsDefineNVGraphicsDataRasterDW,                                 //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 43 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        GsDefineNVGraphicsDataColumnW,                                  //  GS  ( L 1D 28 4C 000C-FFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        GsDefineNVGraphicsDataColumnDW,                                 //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 44 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        GsDefineDownloadGraphicsDataRasterW,                            //  GS  ( L 1D 28 4C 000C-FFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        GsDefineDownloadGraphicsDataRasterDW,                           //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 53 30/34 20-7E 20-7E 01-04 0001-2000 0001-0900 [31-34 00-FF...]...
        GsDefineDownloadGraphicsDataColumnW,                            //  GS  ( L 1D 28 4C 000C-FFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        GsDefineDownloadGraphicsDataColumnDW,                           //  GS  8 L 1D 38 4C 0000000C-FFFFFFFF 30 54 30 30 20-7E 20-7E 01/02 0001-2000 0001-0900 [31-33 00-FF...]...
        GsStoreGraphicsDataToPrintBufferRasterW,                        //  GS  ( L 1D 28 4C 000B-FFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
        GsStoreGraphicsDataToPrintBufferRasterDW,                       //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 70 30/34 01/02 01/02 31-34 0001-0960 0001-0960 00-FF...
        GsStoreGraphicsDataToPrintBufferColumnW,                        //  GS  ( L 1D 28 4C 000B-FFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
        GsStoreGraphicsDataToPrintBufferColumnDW,                       //  GS  8 L 1D 38 4C 0000000B-FFFFFFFF 30 71 30 01/02 01/02 31-33 0001-0800 0001-0080 00-FF...
        GsTransmitNVGraphicsMemoryCapacity,                             //  GS  ( L 1D 28 4C 02 00 30 00/30
        GsSetReferenceDotDensityGraphics,                               //  GS  ( L 1D 28 4C 04 00 30 01/31 32/33 32/33
        GsPrintGraphicsDataInPrintBuffer,                               //  GS  ( L 1D 28 4C 02 00 30 02/32
        GsTransmitRemainingCapacityNVGraphicsMemory,                    //  GS  ( L 1D 28 4C 02 00 30 03/33
        GsTransmitRemainingCapacityDownloadGraphicsMemory,              //  GS  ( L 1D 28 4C 02 00 30 04/34
        GsTransmitKeycodeListDefinedNVGraphics,                         //  GS  ( L 1D 28 4C 04 00 30 40 4B 43
        GsDeleteAllNVGraphicsData,                                      //  GS  ( L 1D 28 4C 05 00 30 41 43 4C 52
        GsDeleteSpecifiedNVGraphicsData,                                //  GS  ( L 1D 28 4C 04 00 30 42 20-7E 20-7E
        GsPrintSpecifiedNVGraphicsData,                                 //  GS  ( L 1D 28 4C 06 00 30 45 20-7E 20-7E 01/02 01/02
        GsTransmitKeycodeListDefinedDownloadGraphics,                   //  GS  ( L 1D 28 4C 04 00 30 50 4B 43
        GsDeleteAllDownloadGraphicsData,                                //  GS  ( L 1D 28 4C 05 00 30 51 43 4C 52
        GsDeleteSpecifiedDownloadGraphicsData,                          //  GS  ( L 1D 28 4C 04 00 30 52 20-7E 20-7E
        GsPrintSpecifiedDownloadGraphicsData,                           //  GS  ( L 1D 28 4C 06 00 30 55 20-7E 20-7E 01/02 01/02
        GsSaveSettingsValuesFromWorkToStorage,                          //  GS  ( M 1D 28 4D 02 00 01/31 01/31
        GsLoadSettingsValuesFromStorageToWork,                          //  GS  ( M 1D 28 4D 02 00 02/32 00/01/30/31
        GsSelectSettingsValuesToWorkAfterInitialize,                    //  GS  ( M 1D 28 4D 02 00 03/33 00/01/30/31
        GsSetCharacterColor,                                            //  GS  ( N 1D 28 4E 02 00 30 30-33
        GsSetBackgroundColor,                                           //  GS  ( N 1D 28 4E 02 00 31 30-33
        GsTurnShadingMode,                                              //  GS  ( N 1D 28 4E 03 00 32 00/01/30/31 30-33
        GsSetPrinttableArea,                                            //  GS  ( P 1D 28 50 08 00 30 FFFF 0001-FFFF 0000 01
        GsDrawLineInPageMode,                                           //  GS  ( Q 1D 28 51 0C 00 30 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30
        GsDrawRectangleInPageMode,                                      //  GS  ( Q 1D 28 51 0E 00 31 0000-FFFF 0000-FFFF 0000-FFFF 0000-FFFF 01 01-06 30 30 01
        GsDrawHorizontalLineInStandardMode,                             //  GS  ( Q 1D 28 51 09 00 32 0000-023F 0000-023F 01-FF 01 01-06 30
        GsDrawVerticalLineInStandardMode,                               //  GS  ( Q 1D 28 51 07 00 33 0000-023F 00/01 01 01-06 30
        GsPDF417SetNumberOfColumns,                                     //  GS  ( k 1D 28 6B 03 00 30 41 00-1E
        GsPDF417SetNumberOfRows,                                        //  GS  ( k 1D 28 6B 03 00 30 42 00/3-5A
        GsPDF417SetWidthOfModule,                                       //  GS  ( k 1D 28 6B 03 00 30 43 01-08
        GsPDF417SetRowHeight,                                           //  GS  ( k 1D 28 6B 03 00 30 44 02-08
        GsPDF417SetErrorCollectionLevel,                                //  GS  ( k 1D 28 6B 04 00 30 45 30/31 30-38/00-28
        GsPDF417SelectOptions,                                          //  GS  ( k 1D 28 6B 03 00 30 46 00/01
        GsPDF417StoreData,                                              //  GS  ( k 1D 28 6B 0004-FFFF 30 50 30 00-FF...
        GsPDF417PrintSymbolData,                                        //  GS  ( k 1D 28 6B 03 00 30 51 30
        GsPDF417TransmitSizeInformation,                                //  GS  ( k 1D 28 6B 03 00 30 52 30
        GsQRCodeSelectModel,                                            //  GS  ( k 1D 28 6B 04 00 31 41 31-33 00
        GsQRCodeSetSizeOfModule,                                        //  GS  ( k 1D 28 6B 03 00 31 43 01-10
        GsQRCodeSetErrorCollectionLevel,                                //  GS  ( k 1D 28 6B 03 00 31 45 30-33
        GsQRCodeStoreData,                                              //  GS  ( k 1D 28 6B 0004-1BB4 31 50 30 00-FF...
        GsQRCodePrintSymbolData,                                        //  GS  ( k 1D 28 6B 03 00 31 51 30
        GsQRCodeTransmitSizeInformation,                                //  GS  ( k 1D 28 6B 03 00 31 52 30
        GsMaxiCodeSelectMode,                                           //  GS  ( k 1D 28 6B 03 00 32 41 32-36
        GsMaxiCodeStoreData,                                            //  GS  ( k 1D 28 6B 0004-008D 32 50 30 00-FF...
        GsMaxiCodePrintSymbolData,                                      //  GS  ( k 1D 28 6B 03 00 32 51 30
        GsMaxiCodeTransmitSizeInformation,                              //  GS  ( k 1D 28 6B 03 00 32 52 30
        Gs2DGS1DBSetWidthOfModule,                                      //  GS  ( k 1D 28 6B 03 00 33 43 02-08
        Gs2DGS1DBSetExpandStackedMaximumWidth,                          //  GS  ( k 1D 28 6B 04 00 33 47 0000/006A-0F70
        Gs2DGS1DBStoreData,                                             //  GS  ( k 1D 28 6B 0006-0103 33 50 30 20-22/25-2F/30-39/3A-3F/41-5A/61-7A...
        Gs2DGS1DBPrintSymbolData,                                       //  GS  ( k 1D 28 6B 03 00 33 51 30
        Gs2DGS1DBTransmitSizeInformation,                               //  GS  ( k 1D 28 6B 03 00 33 52 30
        GsCompositeSetWidthOfModule,                                    //  GS  ( k 1D 28 6B 03 00 34 43 02-08
        GsCompositeSetExpandStackedMaximumWidth,                        //  GS  ( k 1D 28 6B 04 00 34 47 0000/006A-0F70
        GsCompositeSelectHRICharacterFont,                              //  GS  ( k 1D 28 6B 03 00 34 48 00-05/30-35/61/62
        GsCompositeStoreData,                                           //  GS  ( k 1D 28 6B 0006-093E 34 50 30 00-FF...
        GsCompositePrintSymbolData,                                     //  GS  ( k 1D 28 6B 03 00 34 51 30
        GsCompositeTransmitSizeInformation,                             //  GS  ( k 1D 28 6B 03 00 34 52 30
        GsAztecCodeSetModeTypesAndDataLayer,                            //  GS  ( k 1D 28 6B 04 00 35 42 00/01/30/31 00-20
        GsAztecCodeSetSizeOfModule,                                     //  GS  ( k 1D 28 6B 03 00 35 43 02-10
        GsAztecCodeSetErrorCollectionLevel,                             //  GS  ( k 1D 28 6B 03 00 35 45 05-5F
        GsAztecCodeStoreData,                                           //  GS  ( k 1D 28 6B 0004-0EFB 35 50 30 00-FF...
        GsAztecCodePrintSymbolData,                                     //  GS  ( k 1D 28 6B 03 00 35 51 30
        GsAztecCodeTransmitSizeInformation,                             //  GS  ( k 1D 28 6B 03 00 35 52 30
        GsDataMatrixSetSymbolTypeColumnsRows,                           //  GS  ( k 1D 28 6B 05 00 36 42 00/01/30/31 00-90 00-90
        GsDataMatrixSetSizeOfModule,                                    //  GS  ( k 1D 28 6B 03 00 36 43 02-10
        GsDataMatrixStoreData,                                          //  GS  ( k 1D 28 6B 0004-0C2F 36 50 30 00-FF...
        GsDataMatrixPrintSymbolData,                                    //  GS  ( k 1D 28 6B 03 00 36 51 30
        GsDataMatrixTransmitSizeInformation,                            //  GS  ( k 1D 28 6B 03 00 36 52 30
        GsSetReadOperationsOfCheckPaper,                                //c GS  ( z 1D 28 7A 0003-FFFF 2A [3C/40-42/46 00/01/30/31]...
        GsSetsCancelsOperationToFeedCutSheetsToPrintStartingPosition,   //c GS  ( z 1D 28 7A 02 00 30 30
        GsStartSavingReverseSidePrintData,                              //c GS  ( z 1D 28 7A 02 00 3E 30
        GsFinishSavingReverseSidePrintData,                             //c GS  ( z 1D 28 7A 02 00 3E 31
        GsSetCounterForReverseSidePrint,                                //c GS  ( z 1D 28 7A 0C 00 3E 33 01-09 00000000-3B9ACAFF 00/20/30 00000000-3B9ACAFF
        GsReadCheckDataContinuouslyAndTransmitDataRead,                 //c GS  ( z 1D 28 7A 09 00 3F 01 00 30 00-03 00 30 00 00
        GsObsoleteDefineDownloadedBitimage,                             //  GS  *   1D 2A 01-FF 01-FF 00-FF...
        GsObsoletePrintDownloadedBitimage,                              //  GS  /   1D 2F 00-03/30-33
        GsStartEndMacroDefinition,                                      //  GS  :   1D 3A
        GsTurnWhiteBlackReversePrintMode,                               //  GS  B   1D 42 bnnnnnnnx
        GsObsoleteSelectCounterPrintMode,                               //  GS  C 0 1D 43 30 00-05 00-02/30-32
        GsObsoleteSelectCounterModeA,                                   //  GS  C 1 1D 43 31 0000-FFFF 0000-FFFF 00-FF 00-FF
        GsObsoleteSetCounter,                                           //  GS  C 2 1D 43 32 0000-FFFF
        GsObsoleteSelectCounterModeB,                                   //  GS  C ; 1D 43 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B [30-39...] 3B
        GsDefineWindowsBMPNVGraphicsData,                               //  GS  D   1D 44 30 43 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
        GsDefineWindowsBMPDownloadGraphicsData,                         //  GS  D   1D 44 30 53 30 20-7E 20-7E 30/34 31 42 4D 00000042-FFFFFFFF 00-FF...
        GsObsoleteSelectHeadControlMethod,                              //c GS  E   1D 45 b000x0x0x
        GsSelectPrintPositionHRICharacters,                             //  GS  H   1D 48 00-03/30-33
        GsTransmitPrinterID,                                            //  GS  I   1D 49 01-03/31-33/21/23/24/41-45/60/6E-70
        GsSetLeftMargin,                                                //  GS  L   1D 4C 0000-FFFF
        GsSetHorizontalVerticalMotionUnits,                             //  GS  P   1D 50 00-FF 00-FF
        GsObsoletePrintVariableVerticalSizeBitimage,                    //  GS  Q 0 1D 51 30 00-03/30-33 0001-10A0 0001-0010 00-FF...
        GsSetPrintPositionBeginningOfPrintLine,                         //  GS  T   1D 54 00/01/30/31
        GsPaperFullCut,                                                 //  GS  V   1D 56 00/30
        GsPaperPartialCut,                                              //  GS  V   1D 56 01/31
        GsPaperFeedAndFullCut,                                          //  GS  V   1D 56 41 00-FF
        GsPaperFeedAndPartialCut,                                       //  GS  V   1D 56 42 00-FF
        GsPaperReservedFeedAndFullCut,                                  //  GS  V   1D 56 61 00-FF
        GsPaperReservedFeedAndPartialCut,                               //  GS  V   1D 56 62 00-FF
        GsPaperFeedAndFullCutAndTopOfForm,                              //  GS  V   1D 56 67 00-FF
        GsPaperFeedAndPartialCutAndTopOfForm,                           //  GS  V   1D 56 68 00-FF
        GsSetPrintAreaWidth,                                            //  GS  W   1D 57 0000-FFFF
        GsSetRelativeVerticalPrintPosition,                             //  GS  \   1D 5C 8000-7FFF
        GsExecuteMacro,                                                 //  GS  ^   1D 5E 01-FF 00-FF 00/01
        GsEnableDisableAutomaticStatusBack,                             //  GS  a   1D 61 b0x00xxxx
        GsTurnSmoothingMode,                                            //  GS  b   1D 62 bnnnnnnnx
        GsObsoletePrintCounter,                                         //  GS  c   1D 63
        GsSelectFontHRICharacters,                                      //  GS  f   1D 66 00-04/30-34/61/62
        GsInitializeMaintenanceCounter,                                 //  GS  g 0 1D 67 30 00 000A-004F
        GsTransmitMaintenanceCounter,                                   //  GS  g 2 1D 67 32 00 000A-004F/008A-00CF
        GsSetBarcodeHight,                                              //  GS  h   1D 68 01-FF
        GsEnableDisableAutomaticStatusBackInk,                          //  GS  j   1D 6A b000000xx
        GsPrintBarcodeAsciiz,                                           //  GS  k   1D 6B 00-06 20/24/25/2A/2B/2D-2F/30-39/41-5A/61-64... 00
        GsPrintBarcodeSpecifiedLength,                                  //  GS  k   1D 6B 41-4F 01-FF 00-FF...
        GsTransmitStatus,                                               //  GS  r   1D 72 01/02/04/31/32/34
        GsObsoletePrintRasterBitimage,                                  //  GS  v 0 1D 76 30 00-03/30-33 0001-FFFF 0001-11FF 00-FF...
        GsSetBarcodeWidth,                                              //  GS  w   1D 77 02-06/44-4C
        GsSetOnlineRecoveryWaitTime,                                    //  GS  z 0 1D 7A 30 00-FF 00-FF
        GsUnknown,

        // US 0x1F VFD
        VfdUsOverwriteMode,                         //  US  \1  1F 01

        VfdUsVerticalScrollMode,                    //  US  \1  1F 02
        VfdUsHorizontalScrollMode,                  //  US  \1  1F 03
        VfdUsMoveCursorUp,                          //  US  LF  1F 0A
        VfdUsMoveCursorRightMost,                   //  US  LF  1F 0D
        VfdUsTurnAnnounciatorOnOff,                 //  US  #   1F 23 00/01/30/31 00-14
        VfdUsMoveCursorSpecifiedPosition,           //  US  $   1F 24 01-14 01/02
        VfdUsSelectDisplays,                        //  US  ( A 1F 28 41 0003-FFFF 30 [30/31 00-FF]...
        VfdUsChangeIntoUserSettingMode,             //  US  ( E 1F 28 45 03 00 01 49 4E
        VfdUsEndUserSettingMode,                    //  US  ( E 1F 28 45 04 00 02 4F 55 54
        VfdUsSetMemorySwitchValues,                 //  US  ( E 1F 28 45 000A-FFFA 03 [09-0F 30-32]...
        VfdUsSendingDisplayingMemorySwitchValues,   //  US  ( E 1F 28 45 02 00 04 09-0F/6D-70/72/73
        VfdUsKanjiCharacterModeOnOff,               //  US  ( G 1F 28 47 02 00 60 00/01/30/31
        VfdUsSelectKanjiCharacterCodeSystem,        //  US  ( G 1F 28 47 02 00 61 00/01/30/31
        VfdUsDisplayCharWithComma,                  //  US  ,   1F 2C 20-7E/80-FF
        VfdUsDisplayCharWithPeriod,                 //  US  .   1F 2E 20-7E/80-FF
        VfdUsStartEndMacroDefinition,               //  US  :   1F 3A
        VfdUsDisplayCharWithSemicolon,              //  US  ;   1F 3B 20-7E/80-FF
        VfdUsExecuteSelfTest,                       //  US  @   1F 40
        VfdUsMoveCursorBottom,                      //  US  B   1F 42
        VfdUsTurnCursorDisplayModeOnOff,            //  US  C   1F 43 00/01/30/31
        VfdUsSetDisplayBlinkInterval,               //  US  E   1F 45 00-FF
        VfdUsSetAndDisplayCountTime,                //  US  T   1F 54 00-17 00-3B
        VfdUsDisplayCounterTime,                    //  US  U   1F 55
        VfdUsBrightnessAdjustment,                  //  US  X   1F 58 01-04
        VfdUsExecuteMacro,                          //  US  ^   1F 5E 00-FF 00-FF
        VfdUsTurnReverseMode,                       //  US  r   1F 72 00/01/30/31
        VfdUsStatusConfirmationByDTRSignal,         //  US  v   1F 76 00/01/30/31
        VfdUsUnknown
    }

    /// <summary>
    /// data structure of ESC/POS token block
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1034: Nested types should not be visible", Justification = "<Pending>")]
    public class EscPosCmd
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        public EscPosCmdType cmdtype;
        public byte[] cmddata;
        public long cmdlength;
        public string paramdetail;
        public object somebinary;
#pragma warning restore CA1051 // Do not declare visible instance fields

        //internal EscPos()
        //{
        //    datatype = EscType.Unknown;
        //    data = null;
        //    length = 0;
        //}
        public EscPosCmd(EscPosCmdType escTypes, ref byte[] buffer, long startIndex, long size)
        {
            cmdtype = escTypes;
            cmddata = new byte[size]; Buffer.BlockCopy(buffer, (int)startIndex, cmddata, 0, (int)size);
            cmdlength = size;
        }

        public EscPosCmd(EscPosCmdType escTypes, byte[] buffer)
        {
            cmdtype = escTypes;
            cmddata = buffer;
            cmdlength = (buffer != null) ? buffer.Length : 0;
        }
    }
}