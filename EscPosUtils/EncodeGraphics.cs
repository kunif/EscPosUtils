/*

   Copyright (C) 2022 Kunio Fukuchi

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
    using KGySoft.Drawing;
    using KGySoft.Drawing.Imaging;
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;

    public partial class EscPosEncoder
    {
        public enum BufferType
        {
            Immediate,
            PrintBuffer,
            NVGraphics,
            DownloadGraphics
        }

        public enum ImageFormat
        {
            Column08dot,
            Column24dot,
            Rasters,
            Columns,
            WindowsBMP
        }

        public class Plane
        {
            public byte color;
            public byte[] data = Array.Empty<byte>();
        }

        public class EscPosBitmap
        {
            public ImageFormat imageFormat;
            public int width;
            public int height;
            public Boolean monochrome;
            public int planeCount;
            public int totalSize;
            public Plane[] planes = Array.Empty<Plane>();
        }

        public enum DithererType
        {
            None,
            Bayer2x2,
            Bayer3x3,
            Bayer4x4,
            Bayer8x8,
            BlueNoise,
            DottedHalftone,
            Atkinson,
            Burkes,
            FloydSteinberg,
            JarvisJudiceNinke,
            Sierra2,
            Sierra3,
            SierraLite,
            StevensonArce,
            Stucki
        }

        /// <summary>
        /// Bitmap画像データのリサイズ
        /// </summary>
        /// <param name="original">元のBitmapクラスオブジェクト</param>
        /// <param name="width">リサイズ後の幅</param>
        /// <param name="height">リサイズ後の高さ</param>
        /// <param name="interpolationMode">補間モード</param>
        /// <returns>リサイズされたBitmap</returns>
        //private Bitmap ResizeBitmap(Bitmap original, int width, int height, System.Drawing.Drawing2D.InterpolationMode interpolationMode)
        //{
        //    Bitmap bmpResize;
        //    Bitmap bmpResizeColor;
        //    Graphics graphics = null;

        //    try
        //    {
        //        System.Drawing.Imaging.PixelFormat pf = original.PixelFormat;

        //        if (original.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
        //        {
        //            // モノクロの時は仮に24bitとする
        //            pf = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        //        }

        //        bmpResizeColor = new Bitmap(width, height, pf);
        //        var dstRect = new RectangleF(0, 0, width, height);
        //        var srcRect = new RectangleF(-0.5f, -0.5f, original.Width, original.Height);
        //        graphics = Graphics.FromImage(bmpResizeColor);
        //        graphics.Clear(Color.Transparent);
        //        graphics.InterpolationMode = interpolationMode;
        //        graphics.DrawImage(original, dstRect, srcRect, GraphicsUnit.Pixel);

        //    }
        //    finally
        //    {
        //        if (graphics != null)
        //        {
        //            graphics.Dispose();
        //        }
        //    }

        //    if (original.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
        //    {
        //        // モノクロ画像のとき、24bit→8bitへ変換

        //        // モノクロBitmapを確保
        //        bmpResize = new Bitmap(
        //            bmpResizeColor.Width,
        //            bmpResizeColor.Height,
        //            System.Drawing.Imaging.PixelFormat.Format8bppIndexed
        //            );

        //        var pal = bmpResize.Palette;
        //        for (int i = 0; i < bmpResize.Palette.Entries.Length; i++)
        //        {
        //            pal.Entries[i] = original.Palette.Entries[i];
        //        }
        //        bmpResize.Palette = pal;

        //        // カラー画像のポインタへアクセス
        //        var bmpDataColor = bmpResizeColor.LockBits(
        //                new Rectangle(0, 0, bmpResizeColor.Width, bmpResizeColor.Height),
        //                System.Drawing.Imaging.ImageLockMode.ReadWrite,
        //                bmpResizeColor.PixelFormat
        //                );

        //        // モノクロ画像のポインタへアクセス
        //        var bmpDataMono = bmpResize.LockBits(
        //                new Rectangle(0, 0, bmpResize.Width, bmpResize.Height),
        //                System.Drawing.Imaging.ImageLockMode.ReadWrite,
        //                bmpResize.PixelFormat
        //                );

        //        int colorStride = bmpDataColor.Stride;
        //        int monoStride = bmpDataMono.Stride;

        //        unsafe
        //        {
        //            var pColor = (byte*)bmpDataColor.Scan0;
        //            var pMono = (byte*)bmpDataMono.Scan0;
        //            for (int y = 0; y < bmpDataColor.Height; y++)
        //            {
        //                for (int x = 0; x < bmpDataColor.Width; x++)
        //                {
        //                    // R,G,B同じ値のため、Bの値を代表してモノクロデータへ代入
        //                    pMono[x + y * monoStride] = pColor[x * 3 + y * colorStride];
        //                }
        //            }
        //        }

        //        bmpResize.UnlockBits(bmpDataMono);
        //        bmpResizeColor.UnlockBits(bmpDataColor);

        //        //　解放
        //        bmpResizeColor.Dispose();
        //    }
        //    else
        //    {
        //        // カラー画像のとき
        //        bmpResize = bmpResizeColor;
        //    }

        //    return bmpResize;
        //}
        public static EscPosBitmap[] ConvertEscPosBitmaps(Bitmap bitmap, ImageFormat imageFormat, int width, int height, DithererType dithererType)
        {
            bool IsAvailable = imageFormat switch
            {
                ImageFormat.Column08dot => true,
                ImageFormat.Column24dot => true,
                ImageFormat.Rasters => false,
                ImageFormat.Columns => false,
                ImageFormat.WindowsBMP => false,
                _ => false
            };
            if (!IsAvailable) { throw new InvalidEnumArgumentException(String.Format("Unsupported target format {0}", nameof(imageFormat))); }

            BitmapData TempBitmap = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            Bitmap workBitmap = new(bitmap.Width, bitmap.Height, TempBitmap.Stride, bitmap.PixelFormat, TempBitmap.Scan0);
            bitmap.UnlockBits(TempBitmap);
            int internalwidth = width;
            int internalheight = height;
            //workBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if ((internalwidth <= 0) && (internalheight <= 0))
            {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
                internalwidth = workBitmap.Width;
                internalheight = workBitmap.Height;
#pragma warning restore IDE0059 // Remove unnecessary value assignment
            }
            else
            {
                if (internalwidth <= 0)
                {
                    internalwidth = (int)Math.Round((double)workBitmap.Width * ((double)internalheight / (double)workBitmap.Height), 0);
                }
                else if (internalheight <= 0)
                {
                    internalheight = (int)Math.Round((double)workBitmap.Height * ((double)internalwidth / (double)workBitmap.Width), 0);
                }
                Bitmap workImage = new(internalwidth, internalheight, PixelFormat.Format1bppIndexed);
                using (Graphics g = System.Drawing.Graphics.FromImage(workImage))
                {
                    g.DrawImage(workBitmap, 0, 0, internalwidth, internalheight);
                }
                workBitmap.Dispose();
                workBitmap = workImage;
            }
            if (workBitmap.PixelFormat != PixelFormat.Format1bppIndexed)
            {
                IDitherer? ditherer = dithererType switch
                {
                    DithererType.None => null,
                    DithererType.Bayer2x2 => OrderedDitherer.Bayer2x2,
                    DithererType.Bayer3x3 => OrderedDitherer.Bayer3x3,
                    DithererType.Bayer4x4 => OrderedDitherer.Bayer4x4,
                    DithererType.Bayer8x8 => OrderedDitherer.Bayer8x8,
                    DithererType.BlueNoise => OrderedDitherer.BlueNoise,
                    DithererType.DottedHalftone => OrderedDitherer.DottedHalftone,
                    DithererType.Atkinson => ErrorDiffusionDitherer.Atkinson,
                    DithererType.Burkes => ErrorDiffusionDitherer.Burkes,
                    DithererType.FloydSteinberg => ErrorDiffusionDitherer.FloydSteinberg,
                    DithererType.JarvisJudiceNinke => ErrorDiffusionDitherer.JarvisJudiceNinke,
                    DithererType.Sierra2 => ErrorDiffusionDitherer.Sierra2,
                    DithererType.Sierra3 => ErrorDiffusionDitherer.Sierra3,
                    DithererType.SierraLite => ErrorDiffusionDitherer.SierraLite,
                    DithererType.StevensonArce => ErrorDiffusionDitherer.StevensonArce,
                    DithererType.Stucki => ErrorDiffusionDitherer.Stucki,
                    _ => null
                };
                workBitmap = workBitmap.ConvertPixelFormat(PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(Color.Silver), ditherer);
            }

            ColorPalette palette = workBitmap.Palette;
            bool invertRequired = (palette.Entries[0].ToArgb() == Color.Black.ToArgb());
            EscPosBitmap[] stripes = Array.Empty<EscPosBitmap>();
            System.Drawing.Imaging.BitmapData? bmpData = null;
            try
            {
                workBitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                Rectangle rect = new(0, 0, workBitmap.Height, workBitmap.Width);
                bmpData = workBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, workBitmap.PixelFormat);
                int stride = (workBitmap.Height + 7) / 8;
                bool isFlagmented = (workBitmap.Height % 8) != 0;
                byte lastByteMask = (byte)(0xFF00 >> (workBitmap.Height % 8));

                int unit = imageFormat == ImageFormat.Column08dot ? 1 : 3;
                int bytes = unit * workBitmap.Width;
                int count = imageFormat == ImageFormat.Column08dot ? stride : (stride + unit - 1) / unit;
                Array.Resize<EscPosBitmap>(ref stripes, count);

                for (int lpcount = 0; lpcount < count; lpcount++)
                {
                    IntPtr ptr = IntPtr.Add(bmpData.Scan0, (lpcount * unit));
                    byte[] bitValues = new byte[bytes];
                    for (int i = 0; i < workBitmap.Width; i++)
                    {
                        int unitadjusted = ((i + 1) * unit) <= stride ? unit : (stride % unit);
                        System.Runtime.InteropServices.Marshal.Copy(ptr, bitValues, (i * unit), unitadjusted);
                        if (invertRequired)
                        {
                            for (int j = 0; j < unitadjusted; j++)
                            {
                                bitValues[((i * unit) + j)] ^= 0xFF;
                            }
                            if ((lpcount == (count - 1)) && (isFlagmented))
                            {
                                bitValues[((i * unit) + (unitadjusted - 1))] &= lastByteMask;
                            }
                        }
                        ptr = IntPtr.Add(ptr, bmpData.Stride);
                    }
                    EscPosBitmap data = new();
                    data.imageFormat = imageFormat;
                    data.width = width;
                    data.height = unit * 8;
                    data.monochrome = true;
                    data.planeCount = 1;
                    data.planes = new Plane[] { new Plane() { color = 0x31, data = bitValues.ToArray() } };
                    stripes[lpcount] = data;
                }
            }
            finally
            {
                if (bmpData != null)
                {
                    workBitmap.UnlockBits(bmpData);
                }
            }
            return stripes;
        }

        public static EscPosBitmap ConvertEscPosBitmap(Bitmap bitmap, ImageFormat imageFormat, int width, int height, DithererType dithererType)
        {
            bool IsAvailable = imageFormat switch
            {
                ImageFormat.Column08dot => false,
                ImageFormat.Column24dot => false,
                ImageFormat.Rasters => true,
                ImageFormat.Columns => true,
                ImageFormat.WindowsBMP => false,
                _ => false
            };
            if (!IsAvailable) { throw new InvalidEnumArgumentException(String.Format("Unsupported target format {0}", nameof(imageFormat))); }

            BitmapData TempBitmap = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            Bitmap workBitmap = new(bitmap.Width, bitmap.Height, TempBitmap.Stride, bitmap.PixelFormat, TempBitmap.Scan0);
            bitmap.UnlockBits(TempBitmap);
            //workBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            if ((width <= 0) && (height <= 0))
            {
                width = workBitmap.Width;
                height = workBitmap.Height;
            }
            else
            {
                if (width <= 0)
                {
                    width = (int)Math.Round((double)workBitmap.Width * ((double)height / (double)workBitmap.Height), 0);
                }
                else if (height <= 0)
                {
                    height = (int)Math.Round((double)workBitmap.Height * ((double)width / (double)workBitmap.Width), 0);
                }
                Bitmap workImage = new(width, height, PixelFormat.Format1bppIndexed);
                using (Graphics g = System.Drawing.Graphics.FromImage(workImage))
                {
                    g.DrawImage(workBitmap, 0, 0, width, height);
                }
                workBitmap.Dispose();
                workBitmap = workImage;
            }
            if (workBitmap.PixelFormat != PixelFormat.Format1bppIndexed)
            {
                IDitherer? ditherer = dithererType switch
                {
                    DithererType.None => null,
                    DithererType.Bayer2x2 => OrderedDitherer.Bayer2x2,
                    DithererType.Bayer3x3 => OrderedDitherer.Bayer3x3,
                    DithererType.Bayer4x4 => OrderedDitherer.Bayer4x4,
                    DithererType.Bayer8x8 => OrderedDitherer.Bayer8x8,
                    DithererType.BlueNoise => OrderedDitherer.BlueNoise,
                    DithererType.DottedHalftone => OrderedDitherer.DottedHalftone,
                    DithererType.Atkinson => ErrorDiffusionDitherer.Atkinson,
                    DithererType.Burkes => ErrorDiffusionDitherer.Burkes,
                    DithererType.FloydSteinberg => ErrorDiffusionDitherer.FloydSteinberg,
                    DithererType.JarvisJudiceNinke => ErrorDiffusionDitherer.JarvisJudiceNinke,
                    DithererType.Sierra2 => ErrorDiffusionDitherer.Sierra2,
                    DithererType.Sierra3 => ErrorDiffusionDitherer.Sierra3,
                    DithererType.SierraLite => ErrorDiffusionDitherer.SierraLite,
                    DithererType.StevensonArce => ErrorDiffusionDitherer.StevensonArce,
                    DithererType.Stucki => ErrorDiffusionDitherer.Stucki,
                    _ => null
                };
                workBitmap = workBitmap.ConvertPixelFormat(PixelFormat.Format8bppIndexed, PredefinedColorsQuantizer.BlackAndWhite(Color.Silver), ditherer);
            }
            EscPosBitmap data = new();
            data.imageFormat = imageFormat;
            data.width = width;
            data.height = height;
            data.monochrome = workBitmap.PixelFormat == PixelFormat.Format1bppIndexed;
            data.planeCount = 1;
            System.Drawing.Imaging.BitmapData? bmpData = null;
            byte[] bitValues = Array.Empty<byte>();
            ColorPalette palette = workBitmap.Palette;
            bool invertRequired = (palette.Entries[0].ToArgb() == Color.Black.ToArgb());
            try
            {
                if (imageFormat == ImageFormat.Rasters)
                {
                    Rectangle rect = new(0, 0, workBitmap.Width, workBitmap.Height);
                    bmpData = workBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, workBitmap.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    int stride = (workBitmap.Width + 7) / 8;
                    bool isFlagmented = (workBitmap.Width % 8) != 0;
                    byte lastByteMask = (byte)(0xFF00 >> (workBitmap.Width % 8));

                    int bytes = stride * workBitmap.Height;
                    bitValues = new byte[bytes];
                    if (bmpData.Stride == stride)
                    {
                        System.Runtime.InteropServices.Marshal.Copy(ptr, bitValues, 0, bytes);
                        if (invertRequired)
                        {
                            for (int i = 0; i < bytes; i++)
                            {
                                bitValues[i] ^= 0xFF;
                            }
                            if (isFlagmented)
                            {
                                for (int i = 0; i < workBitmap.Height; i++)
                                {
                                    bitValues[((i * stride) + stride - 1)] &= lastByteMask;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < workBitmap.Height; i++)
                        {
                            System.Runtime.InteropServices.Marshal.Copy(ptr, bitValues, (i * stride), stride);
                            if (invertRequired)
                            {
                                for (int j = 0; j < stride; j++)
                                {
                                    bitValues[((i * stride) + j)] ^= 0xFF;
                                }
                                if (isFlagmented)
                                {
                                    bitValues[((i * stride) + stride - 1)] &= lastByteMask;
                                }
                            }
                            ptr = IntPtr.Add(ptr, bmpData.Stride);
                        }
                    }
                }
                else if (imageFormat == ImageFormat.Columns)
                {
                    workBitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                    Rectangle rect = new(0, 0, workBitmap.Height, workBitmap.Width);
                    bmpData = workBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, workBitmap.PixelFormat);
                    IntPtr ptr = bmpData.Scan0;
                    int stride = (workBitmap.Height + 7) / 8;
                    bool isFlagmented = (workBitmap.Height % 8) != 0;
                    byte lastByteMask = (byte)(0xFF00 >> (workBitmap.Height % 8));

                    int bytes = stride * workBitmap.Width;
                    bitValues = new byte[bytes];
                    if (bmpData.Stride == stride)
                    {
                        System.Runtime.InteropServices.Marshal.Copy(ptr, bitValues, 0, bytes);
                        if (invertRequired)
                        {
                            for (int i = 0; i < bytes; i++)
                            {
                                bitValues[i] ^= 0xFF;
                            }
                            if (isFlagmented)
                            {
                                for (int i = 0; i < workBitmap.Width; i++)
                                {
                                    bitValues[((i * stride) + stride - 1)] &= lastByteMask;
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < workBitmap.Width; i++)
                        {
                            System.Runtime.InteropServices.Marshal.Copy(ptr, bitValues, (i * stride), stride);
                            if (invertRequired)
                            {
                                for (int j = 0; j < stride; j++)
                                {
                                    bitValues[((i * stride) + j)] ^= 0xFF;
                                }
                                if (isFlagmented)
                                {
                                    bitValues[((i * stride) + stride - 1)] &= lastByteMask;
                                }
                            }
                            ptr = IntPtr.Add(ptr, bmpData.Stride);
                        }
                    }
                }
            }
            finally
            {
                if (bmpData != null)
                {
                    workBitmap.UnlockBits(bmpData);
                    data.planes = new Plane[] { new Plane() { color = 0x31, data = bitValues.ToArray() } };
                    data.totalSize = data.planes[0].data.Length + 1;
                }
            }
            return data;
        }

        public static EscPosBitmap LoadEscPosBitmap(string filepath)
        {
            EscPosBitmap data = new();
            data.imageFormat = ImageFormat.WindowsBMP;
            data.planeCount = 1;
            data.planes = new Plane[] { new Plane() { color = 0x31, data = File.ReadAllBytes(filepath) } };
            data.totalSize = data.planes[0].data.Length + 1;
            using (MemoryStream ms = new(data.planes[0].data))
            using (Bitmap bitmap = new(ms))
            {
                data.width = bitmap.Width;
                data.height = bitmap.Height;
                data.monochrome = (bitmap.PixelFormat == PixelFormat.Format1bppIndexed);
            }
            return data;
        }

        //--------------------------------------

        public void DefineGraphics(BufferType bufferType, EscPosBitmap escPosBitmap, Boolean density = true, byte xscale = 1, byte yscale = 1, byte kc1 = 0x20, byte kc2 = 0x20)
        {
            byte[] cmd = Array.Empty<byte>();
            Boolean dwordsize = escPosBitmap.totalSize >= 65525;
#pragma warning disable CS8524 // The switch expression does not handle all possible inputs when it does
            EscPosCmdType cmdType = bufferType switch
            {
                BufferType.Immediate => EscPosCmdType.EscSelectBitImageMode,
#pragma warning disable CS8509 // The switch expression does not handle all possible inputs when it does
                BufferType.PrintBuffer => escPosBitmap.imageFormat switch
                {
                    ImageFormat.Rasters => dwordsize switch
                    {
                        false => EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterW,
                        true => EscPosCmdType.GsStoreGraphicsDataToPrintBufferRasterDW,
                    },
                    ImageFormat.Columns => dwordsize switch
                    {
                        false => EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnW,
                        true => EscPosCmdType.GsStoreGraphicsDataToPrintBufferColumnDW,
                    },
                },
                BufferType.NVGraphics => escPosBitmap.imageFormat switch
                {
                    ImageFormat.Rasters => dwordsize switch
                    {
                        false => EscPosCmdType.GsDefineNVGraphicsDataRasterW,
                        true => EscPosCmdType.GsDefineNVGraphicsDataRasterDW,
                    },
                    ImageFormat.Columns => dwordsize switch
                    {
                        false => EscPosCmdType.GsDefineNVGraphicsDataColumnW,
                        true => EscPosCmdType.GsDefineNVGraphicsDataColumnDW,
                    },
                    ImageFormat.WindowsBMP => EscPosCmdType.GsDefineWindowsBMPNVGraphicsData,
                },
                BufferType.DownloadGraphics => escPosBitmap.imageFormat switch
                {
                    ImageFormat.Rasters => dwordsize switch
                    {
                        false => EscPosCmdType.GsDefineDownloadGraphicsDataRasterW,
                        true => EscPosCmdType.GsDefineDownloadGraphicsDataRasterDW,
                    },
                    ImageFormat.Columns => dwordsize switch
                    {
                        false => EscPosCmdType.GsDefineDownloadGraphicsDataColumnW,
                        true => EscPosCmdType.GsDefineDownloadGraphicsDataColumnDW,
                    },
                    ImageFormat.WindowsBMP => EscPosCmdType.GsDefineWindowsBMPDownloadGraphicsData,
                },
#pragma warning restore CS8509 // The switch expression does not handle all possible inputs when it does
            };
#pragma warning restore CS8524 // The switch expression does not handle all possible inputs when it does
            switch (bufferType)
            {
                case BufferType.Immediate:
                    {
#pragma warning disable CS8509 // The switch expression does not handle all possible inputs when it does
                        byte mode = escPosBitmap.imageFormat switch
                        {
                            ImageFormat.Column08dot => density switch { false => 0, true => 1, },
                            ImageFormat.Column24dot => density switch { false => 32, true => 33, },
                        };
#pragma warning restore CS8509 // The switch expression does not handle all possible inputs when it does
                        cmd = new byte[] { 0x1B, 0x2A, mode, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8) };
                        cmd = cmd.Concat(escPosBitmap.planes[0].data).ToArray();
                    }
                    break;

                case BufferType.PrintBuffer:
                    {
                        int length = escPosBitmap.totalSize + 9;
                        byte func = (byte)(escPosBitmap.imageFormat == ImageFormat.Rasters ? 0x70 : 0x71);
                        byte color = (byte)(escPosBitmap.monochrome ? 0x30 : 0x34);
                        if (dwordsize)
                        {
                            cmd = new byte[] { 0x1D, 0x38, 0x4C, (byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24), 0x30, func, color, xscale, yscale, escPosBitmap.planes[0].color, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8), (byte)escPosBitmap.height, (byte)(escPosBitmap.height >> 8) };
                        }
                        else
                        {
                            cmd = new byte[] { 0x1D, 0x28, 0x4C, (byte)length, (byte)(length >> 8), 0x30, func, color, xscale, yscale, escPosBitmap.planes[0].color, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8), (byte)escPosBitmap.height, (byte)(escPosBitmap.height >> 8) };
                        }
                        cmd = cmd.Concat(escPosBitmap.planes[0].data).ToArray();
                    }
                    break;

                case BufferType.NVGraphics:
                    if (escPosBitmap.imageFormat == ImageFormat.WindowsBMP)
                    {
                        byte color = (byte)(escPosBitmap.monochrome ? 0x30 : 0x34);
                        cmd = new byte[] { 0x1D, 0x44, 0x30, 0x43, 0x30, kc1, kc2, color, 0x31 };
                        cmd = cmd.Concat(escPosBitmap.planes[0].data).ToArray();
                    }
                    else
                    {
                        int length = escPosBitmap.totalSize + 10;
                        byte func = (byte)(escPosBitmap.imageFormat == ImageFormat.Rasters ? 0x43 : 0x44);
                        byte color = (byte)(escPosBitmap.monochrome ? 0x30 : 0x34);
                        if (dwordsize)
                        {
                            cmd = new byte[] { 0x1D, 0x38, 0x4C, (byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24), 0x30, func, color, kc1, kc2, (byte)escPosBitmap.planeCount, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8), (byte)escPosBitmap.height, (byte)(escPosBitmap.height >> 8) };
                        }
                        else
                        {
                            cmd = new byte[] { 0x1D, 0x28, 0x4C, (byte)length, (byte)(length >> 8), 0x30, func, color, kc1, kc2, (byte)escPosBitmap.planeCount, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8), (byte)escPosBitmap.height, (byte)(escPosBitmap.height >> 8) };
                        }
                        foreach (Plane planedata in escPosBitmap.planes)
                        {
                            cmd = cmd.Concat(new byte[] { planedata.color }).ToArray();
                            cmd = cmd.Concat(planedata.data).ToArray();
                        }
                    }
                    break;

                case BufferType.DownloadGraphics:
                    if (escPosBitmap.imageFormat == ImageFormat.WindowsBMP)
                    {
                        byte color = (byte)(escPosBitmap.monochrome ? 0x30 : 0x34);
                        cmd = new byte[] { 0x1D, 0x44, 0x30, 0x53, 0x30, kc1, kc2, color, 0x31 };
                        cmd = cmd.Concat(escPosBitmap.planes[0].data).ToArray();
                    }
                    else
                    {
                        int length = escPosBitmap.totalSize + 10;
                        byte func = (byte)(escPosBitmap.imageFormat == ImageFormat.Rasters ? 0x53 : 0x54);
                        byte color = (byte)(escPosBitmap.monochrome ? 0x30 : 0x34);
                        if (dwordsize)
                        {
                            cmd = new byte[] { 0x1D, 0x38, 0x4C, (byte)length, (byte)(length >> 8), (byte)(length >> 16), (byte)(length >> 24), 0x30, func, color, kc1, kc2, (byte)escPosBitmap.planeCount, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8), (byte)escPosBitmap.height, (byte)(escPosBitmap.height >> 8) };
                        }
                        else
                        {
                            cmd = new byte[] { 0x1D, 0x28, 0x4C, (byte)length, (byte)(length >> 8), 0x30, func, color, kc1, kc2, (byte)escPosBitmap.planeCount, (byte)escPosBitmap.width, (byte)(escPosBitmap.width >> 8), (byte)escPosBitmap.height, (byte)(escPosBitmap.height >> 8) };
                        }
                        foreach (Plane planedata in escPosBitmap.planes)
                        {
                            cmd = cmd.Concat(new byte[] { planedata.color }).ToArray();
                            cmd = cmd.Concat(planedata.data).ToArray();
                        }
                    }
                    break;
            }
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public void PrintGraphics(BufferType bufferType, byte xscale = 1, byte yscale = 1, byte kc1 = 0x20, byte kc2 = 0x20)
        {
            EscPosCmdType cmdType = EscPosCmdType.None;
            byte[] cmd = { 0x1D, 0x28, 0x4C };
            switch (bufferType)
            {
                case BufferType.PrintBuffer:
                    cmdType = EscPosCmdType.GsPrintGraphicsDataInPrintBuffer;
                    cmd = cmd.Concat(new byte[] { 0x02, 0x00, 0x30, 0x32 }).ToArray();
                    break;

                case BufferType.NVGraphics:
                    cmdType = EscPosCmdType.GsPrintSpecifiedNVGraphicsData;
                    cmd = cmd.Concat(new byte[] { 0x06, 0x00, 0x30, 0x45, kc1, kc2, xscale, yscale }).ToArray();
                    break;

                case BufferType.DownloadGraphics:
                    cmdType = EscPosCmdType.GsPrintSpecifiedDownloadGraphicsData;
                    cmd = cmd.Concat(new byte[] { 0x06, 0x00, 0x30, 0x55, kc1, kc2, xscale, yscale }).ToArray();
                    break;
            }
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public void DeleteGraphics(BufferType bufferType, Boolean all = false, byte kc1 = 0x20, byte kc2 = 0x20)
        {
            EscPosCmdType cmdType = EscPosCmdType.None;
            byte[] cmd = { 0x1D, 0x28, 0x4C };
            switch (bufferType)
            {
                case BufferType.NVGraphics:
                    if (all)
                    {
                        cmdType = EscPosCmdType.GsDeleteAllNVGraphicsData;
                        cmd = cmd.Concat(new byte[] { 0x05, 0x00, 0x30, 0x41, 0x43, 0x4c, 0x52 }).ToArray();
                    }
                    else
                    {
                        cmdType = EscPosCmdType.GsDeleteSpecifiedNVGraphicsData;
                        cmd = cmd.Concat(new byte[] { 0x04, 0x00, 0x30, 0x42, kc1, kc2 }).ToArray();
                    }
                    break;

                case BufferType.DownloadGraphics:
                    if (all)
                    {
                        cmdType = EscPosCmdType.GsDeleteAllDownloadGraphicsData;
                        cmd = cmd.Concat(new byte[] { 0x05, 0x00, 0x30, 0x51, 0x43, 0x4c, 0x52 }).ToArray();
                    }
                    else
                    {
                        cmdType = EscPosCmdType.GsDeleteSpecifiedDownloadGraphicsData;
                        cmd = cmd.Concat(new byte[] { 0x04, 0x00, 0x30, 0x52, kc1, kc2 }).ToArray();
                    }
                    break;
            }
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------

        public enum GraphicsInfomationType
        {
            NVGraphicsMemoryCapacity,
            RemainingCapacity,
            KeycodeList
        }

        public void TransmitGraphicsInformation(BufferType bufferType, GraphicsInfomationType graphicsInfomation)
        {
            EscPosCmdType cmdType = EscPosCmdType.None;
            byte[] cmd = { 0x1D, 0x28, 0x4C };
            switch (bufferType)
            {
                case BufferType.NVGraphics:
                    switch (graphicsInfomation)
                    {
                        case GraphicsInfomationType.NVGraphicsMemoryCapacity:
                            cmdType = EscPosCmdType.GsTransmitNVGraphicsMemoryCapacity;
                            cmd = cmd.Concat(new byte[] { 0x02, 0x00, 0x30, 0x30 }).ToArray();
                            break;

                        case GraphicsInfomationType.RemainingCapacity:
                            cmdType = EscPosCmdType.GsTransmitRemainingCapacityNVGraphicsMemory;
                            cmd = cmd.Concat(new byte[] { 0x02, 0x00, 0x30, 0x31 }).ToArray();
                            break;

                        case GraphicsInfomationType.KeycodeList:
                            cmdType = EscPosCmdType.GsTransmitKeycodeListDefinedNVGraphics;
                            cmd = cmd.Concat(new byte[] { 0x04, 0x00, 0x30, 0x40, 0x4B, 0x43 }).ToArray();
                            break;
                    }
                    break;

                case BufferType.DownloadGraphics:
                    switch (graphicsInfomation)
                    {
                        case GraphicsInfomationType.RemainingCapacity:
                            cmdType = EscPosCmdType.GsTransmitRemainingCapacityDownloadGraphicsMemory;
                            cmd = cmd.Concat(new byte[] { 0x02, 0x00, 0x30, 0x34 }).ToArray();
                            break;

                        case GraphicsInfomationType.KeycodeList:
                            cmdType = EscPosCmdType.GsTransmitKeycodeListDefinedDownloadGraphics;
                            cmd = cmd.Concat(new byte[] { 0x04, 0x00, 0x30, 0x50, 0x4B, 0x43 }).ToArray();
                            break;
                    }
                    break;
            }
            CommandList.Add(new EscPosCmd(cmdType, cmd));
        }

        //--------------------------------------
    }
}