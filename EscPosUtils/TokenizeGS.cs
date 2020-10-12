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
    using System.Collections.Generic;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;

    public partial class EscPosTokenizer
    {
        /// <summary>
        /// GS(0x1D) XX started fixed size printer command type and related length information
        /// </summary>
        private static readonly Dictionary<byte, SeqInfo> s_PrtGSType = new Dictionary<byte, SeqInfo>()
        {
            { 0x21, new SeqInfo { seqtype = EscPosCmdType.GsSelectCharacterSize,                  length = 3 } },
            { 0x24, new SeqInfo { seqtype = EscPosCmdType.GsSetAbsoluteVerticalPrintPositionInPageMode, length = 4 } },
            { 0x26, new SeqInfo { seqtype = EscPosCmdType.GsSetAbsoluteVerticalPrintPosition,     length = 4 } },
            { 0x2F, new SeqInfo { seqtype = EscPosCmdType.GsObsoletePrintDownloadedBitimage,      length = 3 } },
            { 0x3A, new SeqInfo { seqtype = EscPosCmdType.GsStartEndMacroDefinition,              length = 2 } },
            { 0x42, new SeqInfo { seqtype = EscPosCmdType.GsTurnWhiteBlackReversePrintMode,       length = 3 } },
            { 0x45, new SeqInfo { seqtype = EscPosCmdType.GsObsoleteSelectHeadControlMethod,      length = 3 } },
            { 0x48, new SeqInfo { seqtype = EscPosCmdType.GsSelectPrintPositionHRICharacters,     length = 3 } },
            { 0x49, new SeqInfo { seqtype = EscPosCmdType.GsTransmitPrinterID,                    length = 3 } },
            { 0x4C, new SeqInfo { seqtype = EscPosCmdType.GsSetLeftMargin,                        length = 4 } },
            { 0x50, new SeqInfo { seqtype = EscPosCmdType.GsSetHorizontalVerticalMotionUnits,     length = 4 } },
            { 0x54, new SeqInfo { seqtype = EscPosCmdType.GsSetPrintPositionBeginningOfPrintLine, length = 3 } },
            { 0x57, new SeqInfo { seqtype = EscPosCmdType.GsSetPrintAreaWidth,                    length = 4 } },
            { 0x5C, new SeqInfo { seqtype = EscPosCmdType.GsSetRelativeVerticalPrintPosition,     length = 4 } },
            { 0x5E, new SeqInfo { seqtype = EscPosCmdType.GsExecuteMacro,                         length = 5 } },
            { 0x61, new SeqInfo { seqtype = EscPosCmdType.GsEnableDisableAutomaticStatusBack,     length = 3 } },
            { 0x62, new SeqInfo { seqtype = EscPosCmdType.GsTurnSmoothingMode,                    length = 3 } },
            { 0x63, new SeqInfo { seqtype = EscPosCmdType.GsObsoletePrintCounter,                 length = 2 } },
            { 0x66, new SeqInfo { seqtype = EscPosCmdType.GsSelectFontHRICharacters,              length = 3 } },
            { 0x68, new SeqInfo { seqtype = EscPosCmdType.GsSetBarcodeHight,                      length = 3 } },
            { 0x6A, new SeqInfo { seqtype = EscPosCmdType.GsEnableDisableAutomaticStatusBackInk,  length = 3 } },
            { 0x72, new SeqInfo { seqtype = EscPosCmdType.GsTransmitStatus,                       length = 3 } },
            { 0x77, new SeqInfo { seqtype = EscPosCmdType.GsSetBarcodeWidth,                      length = 3 } },
            { 0x7A, new SeqInfo { seqtype = EscPosCmdType.GsSetOnlineRecoveryWaitTime,            length = 5 } }
        };

        /// <summary>
        /// GS(0x1D) ( C pL pH XX started printer command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtGSCType = new Dictionary<byte, EscPosCmdType>()
        {
            { 0x00, EscPosCmdType.GsDeleteSpecifiedRecord                          },
            { 0x01, EscPosCmdType.GsStoreDataSpecifiedRecord                       },
            { 0x02, EscPosCmdType.GsTransmitDataSpecifiedRecord                    },
            { 0x03, EscPosCmdType.GsTransmitCapacityNVUserMemory                   },
            { 0x04, EscPosCmdType.GsTransmitRemainingCapacityNVUserMemory          },
            { 0x05, EscPosCmdType.GsTransmitKeycodeList                            },
            { 0x06, EscPosCmdType.GsDeleteAllDataNVMemory                          },
            { 0x30, EscPosCmdType.GsDeleteSpecifiedRecord                          },
            { 0x31, EscPosCmdType.GsStoreDataSpecifiedRecord                       },
            { 0x32, EscPosCmdType.GsTransmitDataSpecifiedRecord                    },
            { 0x33, EscPosCmdType.GsTransmitCapacityNVUserMemory                   },
            { 0x34, EscPosCmdType.GsTransmitRemainingCapacityNVUserMemory          },
            { 0x35, EscPosCmdType.GsTransmitKeycodeList                            },
            { 0x36, EscPosCmdType.GsDeleteAllDataNVMemory                          }
        };

        /// <summary>
        /// GS(0x1D) ( E pL pH XX started printer command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtGSEType = new Dictionary<byte, EscPosCmdType>()
        {
            { 0x01, EscPosCmdType.GsChangeIntoUserSettingMode            },
            { 0x02, EscPosCmdType.GsEndUserSettingMode                   },
            { 0x03, EscPosCmdType.GsChangeMeomorySwitch                  },
            { 0x04, EscPosCmdType.GsTransmitSettingsMemorySwitch         },
            { 0x05, EscPosCmdType.GsSetCustomizeSettinValues             },
            { 0x06, EscPosCmdType.GsTransmitCustomizeSettingValues       },
            { 0x07, EscPosCmdType.GsCopyUserDefinedPage                  },
            { 0x08, EscPosCmdType.GsDefineColumnFormatCharacterCodePage  },
            { 0x09, EscPosCmdType.GsDefineRasterFormatCharacterCodePage  },
            { 0x0A, EscPosCmdType.GsDeleteCharacterCodePage              },
            { 0x0B, EscPosCmdType.GsSetSerialInterface                   },
            { 0x0C, EscPosCmdType.GsTransmitSerialInterface              },
            { 0x0D, EscPosCmdType.GsSetBluetoothInterface                },
            { 0x0E, EscPosCmdType.GsTransmitBluetoothInterface           },
            { 0x0F, EscPosCmdType.GsSetUSBInterface                      },
            { 0x10, EscPosCmdType.GsTransmitUSBInterface                 },
            { 0x30, EscPosCmdType.GsDeletePaperLayout                    },
            { 0x31, EscPosCmdType.GsSetPaperLayout                       },
            { 0x32, EscPosCmdType.GsTransmitPaperLayout                  },
            { 0x33, EscPosCmdType.GsSetControlLabelPaperAndBlackMarks    },
            { 0x34, EscPosCmdType.GsTransmitControlSettingsLabelPaperAndBlackMarks },
            { 0x63, EscPosCmdType.GsSetInternalBuzzerPatterns            },
            { 0x64, EscPosCmdType.GsTransmitInternalBuzzerPatterns       }
        };

        /// <summary>
        /// GS(0x1D) ( G pL pH XX started printer command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtGSGType = new Dictionary<byte, EscPosCmdType>()
        {
            { 0x20, EscPosCmdType.GsTransmitStatusOfCutSheet                         },
            { 0x30, EscPosCmdType.GsSelectSideOfSlipFaceOrBack                       },
            { 0x3C, EscPosCmdType.GsReadMagneticInkCharacterAndTransmitReadingResult },
            { 0x3D, EscPosCmdType.GsRetransmitMagneticInkCharacterReadingResult      },
            { 0x40, EscPosCmdType.GsReadDataAndTransmitResultingInformation          },
            { 0x41, EscPosCmdType.GsScanImageDataAndTransmitImageScanningResult      },
            { 0x42, EscPosCmdType.GsRetransmitImageScanningResult                    },
            { 0x43, EscPosCmdType.GsExecutePreScan                                   },
            { 0x44, EscPosCmdType.GsDeleteImageScanningResultWithSpecifiedDataID     },
            { 0x45, EscPosCmdType.GsDeleteAllImageScanningResult                     },
            { 0x46, EscPosCmdType.GsTransmitDataIDListOfImageScanningResult          },
            { 0x47, EscPosCmdType.GsTransmitRemainingCapacityOfNVMemoryForImageDataStorage },
            { 0x50, EscPosCmdType.GsSelectActiveSheet                                },
            { 0x51, EscPosCmdType.GsStartPreProcessForCutSheetInsertion              },
            { 0x52, EscPosCmdType.GsEndPreProcessForCutSheetInsertion                },
            { 0x53, EscPosCmdType.GsExecuteWaitingProcessForCutSheetInsertion        },
            { 0x54, EscPosCmdType.GsFeedToPrintStartingPositionForSlip               },
            { 0x55, EscPosCmdType.GsFinishProcessingOfCutSheet                       }
        };

        /// <summary>
        /// GS(0x1D) (or8 L pL pH/p0,p1,p2,p3 XX started printer graphics command type information
        /// </summary>
        private static readonly Dictionary<byte, EscPosCmdType> s_PrtGSLType = new Dictionary<byte, EscPosCmdType>()
        {
            { 0x00, EscPosCmdType.GsTransmitNVGraphicsMemoryCapacity                  },
            { 0x01, EscPosCmdType.GsSetReferenceDotDensityGraphics                    },
            { 0x02, EscPosCmdType.GsPrintGraphicsDataInPrintBuffer                    },
            { 0x03, EscPosCmdType.GsTransmitRemainingCapacityNVGraphicsMemory         },
            { 0x04, EscPosCmdType.GsTransmitRemainingCapacityDownloadGraphicsMemory   },
            { 0x30, EscPosCmdType.GsTransmitNVGraphicsMemoryCapacity                  },
            { 0x31, EscPosCmdType.GsSetReferenceDotDensityGraphics                    },
            { 0x32, EscPosCmdType.GsPrintGraphicsDataInPrintBuffer                    },
            { 0x33, EscPosCmdType.GsTransmitRemainingCapacityNVGraphicsMemory         },
            { 0x34, EscPosCmdType.GsTransmitRemainingCapacityDownloadGraphicsMemory   },
            { 0x40, EscPosCmdType.GsTransmitKeycodeListDefinedNVGraphics              },
            { 0x41, EscPosCmdType.GsDeleteAllNVGraphicsData                           },
            { 0x42, EscPosCmdType.GsDeleteSpecifiedNVGraphicsData                     },
            { 0x43, EscPosCmdType.GsDefineNVGraphicsDataRasterW                       },
            { 0x44, EscPosCmdType.GsDefineNVGraphicsDataColumnW                       },
            { 0x45, EscPosCmdType.GsPrintSpecifiedNVGraphicsData                      },
            { 0x50, EscPosCmdType.GsTransmitKeycodeListDefinedDownloadGraphics        },
            { 0x51, EscPosCmdType.GsDeleteAllDownloadGraphicsData                     },
            { 0x52, EscPosCmdType.GsDeleteSpecifiedDownloadGraphicsData               },
            { 0x53, EscPosCmdType.GsDefineDownloadGraphicsDataRasterW                 },
            { 0x54, EscPosCmdType.GsDefineDownloadGraphicsDataColumnW                 },
            { 0x55, EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData                },
            { 0x70, EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterW             },
            { 0x71, EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnW             }
        };

        /// <summary>
        /// GS(0x1D) ( k pL pH XX XX started printer barcode command type information
        /// </summary>
        private static readonly Dictionary<string, EscPosCmdType> s_PrtGSkType = new Dictionary<string, EscPosCmdType>()
        {
            { "0A", EscPosCmdType.GsPDF417SetNumberOfColumns                  },
            { "0B", EscPosCmdType.GsPDF417SetNumberOfRows                     },
            { "0C", EscPosCmdType.GsPDF417SetWidthOfModule                    },
            { "0D", EscPosCmdType.GsPDF417SetRowHeight                        },
            { "0E", EscPosCmdType.GsPDF417SetErrorCollectionLevel             },
            { "0F", EscPosCmdType.GsPDF417SelectOptions                       },
            { "0P", EscPosCmdType.GsPDF417StoreData                           },
            { "0Q", EscPosCmdType.GsPDF417PrintSymbolData                     },
            { "0R", EscPosCmdType.GsPDF417TransmitSizeInformation             },
            { "1A", EscPosCmdType.GsQRCodeSelectModel                         },
            { "1C", EscPosCmdType.GsQRCodeSetSizeOfModule                     },
            { "1E", EscPosCmdType.GsQRCodeSetErrorCollectionLevel             },
            { "1P", EscPosCmdType.GsQRCodeStoreData                           },
            { "1Q", EscPosCmdType.GsQRCodePrintSymbolData                     },
            { "1R", EscPosCmdType.GsQRCodeTransmitSizeInformation             },
            { "2A", EscPosCmdType.GsMaxiCodeSelectMode                        },
            { "2P", EscPosCmdType.GsMaxiCodeStoreData                         },
            { "2Q", EscPosCmdType.GsMaxiCodePrintSymbolData                   },
            { "2R", EscPosCmdType.GsMaxiCodeTransmitSizeInformation           },
            { "3C", EscPosCmdType.Gs2DGS1DBSetWidthOfModule                   },
            { "3G", EscPosCmdType.Gs2DGS1DBSetExpandStackedMaximumWidth       },
            { "3P", EscPosCmdType.Gs2DGS1DBStoreData                          },
            { "3Q", EscPosCmdType.Gs2DGS1DBPrintSymbolData                    },
            { "3R", EscPosCmdType.Gs2DGS1DBTransmitSizeInformation            },
            { "4C", EscPosCmdType.GsCompositeSetWidthOfModule                 },
            { "4G", EscPosCmdType.GsCompositeSetExpandStackedMaximumWidth     },
            { "4H", EscPosCmdType.GsCompositeSelectHRICharacterFont           },
            { "4P", EscPosCmdType.GsCompositeStoreData                        },
            { "4Q", EscPosCmdType.GsCompositePrintSymbolData                  },
            { "4R", EscPosCmdType.GsCompositeTransmitSizeInformation          },
            { "5B", EscPosCmdType.GsAztecCodeSetModeTypesAndDataLayer         },
            { "5C", EscPosCmdType.GsAztecCodeSetSizeOfModule                  },
            { "5E", EscPosCmdType.GsAztecCodeSetErrorCollectionLevel          },
            { "5P", EscPosCmdType.GsAztecCodeStoreData                        },
            { "5Q", EscPosCmdType.GsAztecCodePrintSymbolData                  },
            { "5R", EscPosCmdType.GsAztecCodeTransmitSizeInformation          },
            { "6B", EscPosCmdType.GsDataMatrixSetSymbolTypeColumnsRows        },
            { "6C", EscPosCmdType.GsDataMatrixSetSizeOfModule                 },
            { "6P", EscPosCmdType.GsDataMatrixStoreData                       },
            { "6Q", EscPosCmdType.GsDataMatrixPrintSymbolData                 },
            { "6R", EscPosCmdType.GsDataMatrixTransmitSizeInformation         }
        };

        /// <summary>
        /// GS(0x1D) started variable bytes printer command sequence tokenize routine
        /// </summary>
        private void TokenizeGS()
        {
            if (s_PrtGSType.ContainsKey(ctlByte1))
            {
                ctlType = s_PrtGSType[ctlByte1].seqtype;
                blockLength = s_PrtGSType[ctlByte1].length;
            }
            else
            {
                blockLength = 3;
                switch (ctlByte1)
                {
                    case 0x28: // GS (
                        switch (ctlByte2)
                        {
                            case 0x41: // GS ( A
                            case 0x42: // GS ( B
                            case 0x43: // GS ( C
                            case 0x44: // GS ( D
                            case 0x45: // GS ( E
                            case 0x47: // GS ( G
                            case 0x48: // GS ( H
                            case 0x4B: // GS ( K
                            case 0x4C: // GS ( L
                            case 0x4D: // GS ( M
                            case 0x4E: // GS ( N
                            case 0x50: // GS ( P
                            case 0x51: // GS ( Q
                            case 0x6B: // GS ( k
                            case 0x7A: // GS ( z
                                ctlType = EscPosCmdType.None;
                                break;

                            default: // GS ( ??
                                ctlType = EscPosCmdType.GsUnknown;
                                break;
                        }
                        if (ctlType != EscPosCmdType.GsUnknown)
                        {
                            byte ctlByte5 = (byte)(((curIndex + 5) < dataLength) ? baData[curIndex + 5] : 0xFF);
                            byte ctlByte6 = (byte)(((curIndex + 6) < dataLength) ? baData[curIndex + 6] : 0xFF);
                            blockLength = 5 + ctlByte4 * 0x100 + ctlByte3;
                            switch (ctlByte2)
                            {
                                case 0x41: // GS ( A
                                    ctlType = (blockLength == 7) ? EscPosCmdType.GsExecuteTestPrint : EscPosCmdType.GsUnknown;
                                    break;

                                case 0x42: // GS ( B
                                    ctlType = EscPosCmdType.GsCustomizeASBStatusBits;
                                    break;

                                case 0x43: // GS ( C
                                    if ((ctlByte5 == 0x00) && ((blockLength >= 8) && (baData[curIndex + 7] == 0x00)))
                                    {
                                        if (s_PrtGSCType.ContainsKey(ctlByte6))
                                        {
                                            ctlType = s_PrtGSCType[ctlByte6];
                                        }
                                        else // GS ( C dL dH 00 ?? 00
                                        {
                                            ctlType = EscPosCmdType.GsUnknown;
                                        }
                                    }
                                    else // GS ( C dL dH ?? xx ??
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x44: // GS ( D
                                    if ((ctlByte5 == 0x14) && ((blockLength == 8) || (blockLength == 10)))
                                    {
                                        ctlType = EscPosCmdType.GsEnableDisableRealtimeCommand;
                                    }
                                    else // GS ( C dL dH ?? xx xx xx xx
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x45: // GS ( E
                                    if (s_PrtGSEType.ContainsKey(ctlByte5))
                                    {
                                        ctlType = s_PrtGSEType[ctlByte5];
                                    }
                                    else // GS ( E dL dH ??
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x47: // GS ( G
                                    if (s_PrtGSGType.ContainsKey(ctlByte5))
                                    {
                                        ctlType = s_PrtGSGType[ctlByte5];
                                    }
                                    else // GS ( G dL dH ??
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x48: // GS ( H
                                    if ((ctlByte5 == 0x30) && (blockLength == 11))
                                    {
                                        ctlType = EscPosCmdType.GsSpecifiesProcessIDResponse;
                                    }
                                    else if ((ctlByte5 == 0x31) && (blockLength == 8))
                                    {
                                        ctlType = EscPosCmdType.GsSpecifiesOfflineResponse;
                                    }
                                    else // GS ( H dL dH ??
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x4B: // GS ( K
                                    ctlType = ctlByte5 switch
                                    {
                                        // GS ( K dL dH 0
                                        0x30 => EscPosCmdType.GsSelectPrintControlMode,
                                        // GS ( K dL dH 1
                                        0x31 => EscPosCmdType.GsSelectPrintDensity,
                                        // GS ( K dL dH 2
                                        0x32 => EscPosCmdType.GsSelectPrintSpeed,
                                        // GS ( K dL dH a
                                        0x61 => EscPosCmdType.GsSelectNumberOfPartsThermalHeadEnergizing,
                                        // GS ( K dL dH ??
                                        _ => EscPosCmdType.GsUnknown,
                                    };
                                    break;

                                case 0x4C: // GS ( L
                                    if (ctlByte5 == 0x30)
                                    {
                                        if (s_PrtGSLType.ContainsKey(ctlByte6))
                                        {
                                            ctlType = s_PrtGSLType[ctlByte6];
                                        }
                                        else
                                        {
                                            ctlType = EscPosCmdType.GsUnknown;
                                        }
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x4D: // GS ( M
                                    ctlType = ctlByte5 switch
                                    {
                                        0x01 => EscPosCmdType.GsSaveSettingsValuesFromWorkToStorage,
                                        0x31 => EscPosCmdType.GsSaveSettingsValuesFromWorkToStorage,
                                        0x02 => EscPosCmdType.GsLoadSettingsValuesFromStorageToWork,
                                        0x32 => EscPosCmdType.GsLoadSettingsValuesFromStorageToWork,
                                        0x03 => EscPosCmdType.GsSelectSettingsValuesToWorkAfterInitialize,
                                        0x33 => EscPosCmdType.GsSelectSettingsValuesToWorkAfterInitialize,
                                        _ => EscPosCmdType.GsUnknown,
                                    };
                                    break;

                                case 0x4E: // GS ( N
                                    ctlType = ctlByte5 switch
                                    {
                                        0x30 => EscPosCmdType.GsSetCharacterColor,
                                        0x31 => EscPosCmdType.GsSetBackgroundColor,
                                        0x32 => EscPosCmdType.GsTurnShadingMode,
                                        _ => EscPosCmdType.GsUnknown,
                                    };
                                    break;

                                case 0x50: // GS ( P
                                    if ((ctlByte5 == 0x30) && (blockLength == 13))
                                    {
                                        ctlType = EscPosCmdType.GsSetPrinttableArea;
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x51: // GS ( Q
                                    if ((ctlByte5 == 0x30) && (blockLength == 17))
                                    {
                                        ctlType = EscPosCmdType.GsDrawLineInPageMode;
                                    }
                                    else if ((ctlByte5 == 0x31) && (blockLength == 19))
                                    {
                                        ctlType = EscPosCmdType.GsDrawRectangleInPageMode;
                                    }
                                    else if ((ctlByte5 == 0x32) && (blockLength == 14))
                                    {
                                        ctlType = EscPosCmdType.GsDrawHorizontalLineInStandardMode;
                                    }
                                    else if ((ctlByte5 == 0x33) && (blockLength == 12))
                                    {
                                        ctlType = EscPosCmdType.GsDrawVerticalLineInStandardMode;
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x6B: // GS ( k
                                    if ((ctlByte5 >= 0x30) && (ctlByte5 <= 0x36))
                                    {
                                        byte[] bFunc = new byte[2] { ctlByte5, ctlByte6 };
                                        string sFunc = Encoding.ASCII.GetString(bFunc);
                                        if (s_PrtGSkType.ContainsKey(sFunc))
                                        {
                                            ctlType = s_PrtGSkType[sFunc];
                                        }
                                        else
                                        {
                                            ctlType = EscPosCmdType.GsUnknown;
                                        }
                                    }
                                    else
                                    {
                                        ctlType = EscPosCmdType.GsUnknown;
                                    }
                                    break;

                                case 0x7A: // GS ( z
                                    ctlType = ctlByte5 switch
                                    {
                                        0x2A => EscPosCmdType.GsSetReadOperationsOfCheckPaper,
                                        0x30 => EscPosCmdType.GsSetsCancelsOperationToFeedCutSheetsToPrintStartingPosition,
                                        0x3E => ctlByte6 switch
                                        {
                                            0x30 => EscPosCmdType.GsStartSavingReverseSidePrintData,
                                            0x31 => EscPosCmdType.GsFinishSavingReverseSidePrintData,
                                            0x33 => EscPosCmdType.GsSetCounterForReverseSidePrint,
                                            _ => EscPosCmdType.GsUnknown,
                                        },
                                        0x3F => EscPosCmdType.GsReadCheckDataContinuouslyAndTransmitDataRead,
                                        _ => EscPosCmdType.GsUnknown,
                                    };
                                    break;
                            }
                        }
                        break;

                    case 0x38: // GS 8
                        if (remainLength < 9)
                        {
                            blockLength = 9;
                        }
                        else if (ctlByte2 == 0x4C) // GS 8 L
                        {
                            blockLength = 7 + BitConverter.ToUInt32(baData, (int)(curIndex + 3));
                            ctlType = (baData[curIndex + 8]) switch
                            {
                                0x43 => EscPosCmdType.GsDefineNVGraphicsDataRasterDW,
                                0x44 => EscPosCmdType.GsDefineNVGraphicsDataColumnDW,
                                0x53 => EscPosCmdType.GsDefineDownloadGraphicsDataRasterDW,
                                0x54 => EscPosCmdType.GsDefineDownloadGraphicsDataColumnDW,
                                0x70 => EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterDW,
                                0x71 => EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnDW,
                                _ => EscPosCmdType.GsUnknown,
                            };
                        }
                        else
                        {
                            ctlType = EscPosCmdType.GsUnknown;
                        }
                        break;

                    case 0x2A: // GS *
                        if (remainLength < 12)
                        {
                            blockLength = 12;
                        }
                        else
                        {
                            long Xbytes = ctlByte2;
                            long Ybytes = ctlByte3;
                            blockLength = 4 + (Xbytes * Ybytes * 8);
                            ctlType = EscPosCmdType.GsObsoleteDefineDownloadedBitimage;
                        }
                        break;

                    case 0x43: // GS C
                        switch (ctlByte2)
                        {
                            case 0x30: // GS C 0
                                blockLength = 5;
                                ctlType = EscPosCmdType.GsObsoleteSelectCounterPrintMode;
                                break;

                            case 0x31: // GS C 1
                                blockLength = 9;
                                ctlType = EscPosCmdType.GsObsoleteSelectCounterModeA;
                                break;

                            case 0x32: // GS C 2
                                blockLength = 5;
                                ctlType = EscPosCmdType.GsObsoleteSetCounter;
                                break;

                            case 0x3B: // GS C ;
                                {
                                    ctlType = EscPosCmdType.GsObsoleteSelectCounterModeB;
                                    blockLength = 0;
                                    long workIndex = curIndex + 3;
                                    for (long i = 0; (i < 5) && (workIndex < dataLength); workIndex++)
                                    {
                                        if (baData[workIndex] == 0x3B)
                                        {
                                            i++;
                                        }
                                        else if ((baData[workIndex] < 0x30) || (baData[workIndex] > 0x39))
                                        {
                                            blockLength = workIndex - curIndex + 1;
                                            break;
                                        }
                                    }
                                    if (blockLength == 0)
                                    {
                                        blockLength = workIndex - curIndex;
                                    }
                                }
                                break;

                            default:
                                ctlType = EscPosCmdType.GsUnknown;
                                break;
                        }
                        break;

                    case 0x44: // GS D
                        blockLength = 15;
                        if (remainLength < 15)
                        {
                            break;
                        }
                        else if ((ctlByte2 == 0x30) && ((ctlByte3 == 0x43) || (ctlByte3 == 0x53)) && (ctlByte4 == 0x30) && (baData[curIndex + 8] == 0x31))
                        {
                            using Stream stream = new MemoryStream(baData, (int)(curIndex + 9), (int)(remainLength - 9), false);
                            using System.Drawing.Image img = System.Drawing.Image.FromStream(stream);
                            //if (img.RawFormat.Equals(ImageFormat.Bmp) ||
                            //    img.RawFormat.Equals(ImageFormat.Gif) ||
                            //    img.RawFormat.Equals(ImageFormat.Jpeg) ||
                            //    img.RawFormat.Equals(ImageFormat.Png))
                            //{ }
                            if (img.RawFormat.Equals(ImageFormat.Bmp))
                            {
                                blockLength = 9 + BitConverter.ToUInt32(baData, (int)(curIndex + 11));
                                ctlType = ctlByte3 switch
                                {
                                    0x43 => EscPosCmdType.GsDefineWindowsBMPNVGraphicsData,
                                    0x53 => EscPosCmdType.GsDefineWindowsBMPDownloadGraphicsData,
                                    _ => EscPosCmdType.GsUnknown,
                                };
                            }
                        }
                        else
                        {
                            ctlType = EscPosCmdType.GsUnknown;
                        }
                        break;

                    case 0x51: // GS Q
                        if (remainLength < 8)
                        {
                            blockLength = 8;
                        }
                        else
                        {
                            if ((ctlByte2 == 0x30) && ((ctlByte3 <= 3) || ((ctlByte3 >= 0x30) && (ctlByte3 <= 0x33))))
                            {
                                ctlType = EscPosCmdType.GsObsoletePrintVariableVerticalSizeBitimage;
                            }
                            else
                            {
                                ctlType = EscPosCmdType.GsUnknown;
                            }
                            long Xbytes = BitConverter.ToUInt16(baData, (int)(curIndex + 4));
                            long Ydots = BitConverter.ToUInt16(baData, (int)(curIndex + 6));
                            blockLength = 8 + (Xbytes * Ydots);
                        }
                        break;

                    case 0x56: // GS V
                        ctlType = ctlByte2 switch
                        {
                            // GS V 00
                            0x00 => EscPosCmdType.GsPaperFullCut,
                            // GS V 0
                            0x30 => EscPosCmdType.GsPaperFullCut,
                            // GS V 01
                            0x01 => EscPosCmdType.GsPaperPartialCut,
                            // GS V 1
                            0x31 => EscPosCmdType.GsPaperPartialCut,
                            // GS V A
                            0x41 => EscPosCmdType.GsPaperFeedAndFullCut,
                            // GS V B
                            0x42 => EscPosCmdType.GsPaperFeedAndPartialCut,
                            // GS V a
                            0x61 => EscPosCmdType.GsPaperReservedFeedAndFullCut,
                            // GS V b
                            0x62 => EscPosCmdType.GsPaperReservedFeedAndPartialCut,
                            // GS V g
                            0x67 => EscPosCmdType.GsPaperFeedAndFullCutAndTopOfForm,
                            // GS V h
                            0x68 => EscPosCmdType.GsPaperFeedAndPartialCutAndTopOfForm,
                            _ => EscPosCmdType.GsUnknown,
                        };
                        blockLength = ctlType switch
                        {
                            EscPosCmdType.GsPaperFullCut => 3,
                            EscPosCmdType.GsPaperPartialCut => 3,
                            EscPosCmdType.GsUnknown => 3,
                            _ => 4,
                        };
                        break;

                    case 0x67: // GS g
                        ctlType = ctlByte2 switch
                        {
                            // GS g 0
                            0x30 => EscPosCmdType.GsInitializeMaintenanceCounter,
                            // GS g 2
                            0x32 => EscPosCmdType.GsTransmitMaintenanceCounter,
                            _ => EscPosCmdType.GsUnknown,
                        };
                        blockLength = ctlType != EscPosCmdType.GsUnknown ? 6 : 3;
                        break;

                    case 0x6B: // GS k
                        if (ctlByte2 <= 6)
                        {
                            ctlType = EscPosCmdType.GsPrintBarcodeAsciiz;
                            long terminateIndex = 0;
                            for (long i = curIndex + 3; i < dataLength; i++)
                            {
                                if (baData[i] == 0x00)
                                {
                                    terminateIndex = i;
                                    break;
                                }
                            }
                            if (terminateIndex != 0)
                            {
                                blockLength = terminateIndex - curIndex + 1;
                            }
                            else
                            {
                                ctlType = EscPosCmdType.GsUnknown;
                                blockLength = dataLength - curIndex;
                            }
                        }
                        else if ((ctlByte2 >= 0x41) && (ctlByte2 <= 0x4F))
                        {
                            ctlType = EscPosCmdType.GsPrintBarcodeSpecifiedLength;
                            blockLength = 4 + ctlByte3;
                        }
                        else
                        {
                            ctlType = EscPosCmdType.GsUnknown;
                        }
                        break;

                    case 0x76: // GS v
                        if (remainLength < 8)
                        {
                            blockLength = 8;
                        }
                        else
                        {
                            if ((ctlByte2 == 0x30) && (ctlByte3 <= 3) || ((ctlByte3 >= 0x30) && (ctlByte3 <= 0x33)))
                            {
                                ctlType = EscPosCmdType.GsObsoletePrintRasterBitimage;
                            }
                            else
                            {
                                ctlType = EscPosCmdType.GsUnknown;
                            }
                            long Xbytes = BitConverter.ToUInt16(baData, (int)(curIndex + 4));
                            long Ydots = BitConverter.ToUInt16(baData, (int)(curIndex + 6));
                            blockLength = 8 + (Xbytes * Ydots);
                        }
                        break;

                    default:
                        ctlType = EscPosCmdType.GsUnknown;
                        blockLength = 2;
                        break;
                }
            }
        }
    }
}