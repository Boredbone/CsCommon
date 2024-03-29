﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;

namespace Boredbone.Utility.Tools
{

    public enum GraphicFileType
    {
        Unknown,
        Bmp,
        Jpeg,
        Gif,
        Png,
        Psd,
        Wmf,
        Emf,
        Tiff,
        Webp,
        Avif,
    }

    /// <summary>
    /// 画像ファイルのヘッダからサイズを読み取る
    /// </summary>
    public readonly struct GraphicInformation
    {
        public Size GraphicSize { get; }
        public long FileSize { get; }
        public GraphicFileType Type { get; }
        public int BlankHeaderLength { get; }

        public bool IsMetaImage
            => (this.Type == GraphicFileType.Wmf || this.Type == GraphicFileType.Emf);

        /// <summary>
        /// 指定パスのファイルを開く
        /// </summary>
        /// <param name="path"></param>
        public GraphicInformation(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var size = default(Size);
                if (CheckSizeMain(stream, out size,
                    out var fileSize, out var blankHeaderLength, out var graphicType))
                {
                    this.GraphicSize = size;
                    this.FileSize = fileSize;
                    this.BlankHeaderLength = blankHeaderLength;
                    this.Type = graphicType;
                }
                else
                {
                    this.GraphicSize = default;
                    this.FileSize = default;
                    this.BlankHeaderLength = default;
                    this.Type = default;
                }
            }
        }

        /// <summary>
        /// ストリームから読み取る
        /// </summary>
        /// <param name="stream"></param>
        public GraphicInformation(Stream stream)
        {
            var size = default(Size);
            if (CheckSizeMain(stream, out size,
                out var fileSize, out var blankHeaderLength, out var graphicType))
            {
                this.GraphicSize = size;
                this.FileSize = fileSize;
                this.BlankHeaderLength = blankHeaderLength;
                this.Type = graphicType;
            }
            else
            {
                this.GraphicSize = default;
                this.FileSize = default;
                this.BlankHeaderLength = default;
                this.Type = default;
            }
        }

        /// <summary>
        /// ファイル先頭からファイルの種類を判断してヘッダを読み取り
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool CheckSizeMain(Stream stream, out Size size,
            out long fileSize, out int blankHeaderLength, out GraphicFileType fileType)
        {
            fileSize = stream.Length;
            blankHeaderLength = 0;
            fileType = GraphicFileType.Unknown;

            if (fileSize < 16)
            {
                // too small
                size = default(Size);
                return false;
            }

            var type = BinaryHelper.ReadInt16(stream, true);
            blankHeaderLength = 0;

            //ブランクを読み飛ばし
            while (type == 0x0000
                && blankHeaderLength < 8)
            {
                type = BinaryHelper.ReadInt16(stream, true);
                blankHeaderLength += 2;
            }

            switch (type)
            {

                case 0x424D:
                    //BMP: 0x42,0x4D_"BM"
                    return GetBmpSize(stream, out size, ref fileType, blankHeaderLength);
                case 0xFFD8:
                    //JPG: 0xFF,0xD8
                    return GetJpegSize(stream, out size, ref fileType, blankHeaderLength);
                case 0x4749:
                    //GIF: 0x47,0x49_"GI"
                    return GetGifSize(stream, out size, ref fileType, blankHeaderLength);
                case 0x8950:
                    //PNG: 0x89,0x50_'P'
                    return GetPngSize(stream, out size, ref fileType, blankHeaderLength);
                case 0x3842:
                    //PSD: 0x38,0x42_"8B"
                    return GetPsdSize(stream, out size, ref fileType, blankHeaderLength);
                case 0xD7CD:
                    //WMF: 0xD7,0xCD
                    return GetWmfSize(stream, out size, ref fileType, blankHeaderLength);
                case 0x0100:
                    //EMF: 0x01,0x00
                    return GetEmfSize(stream, out size, ref fileType, blankHeaderLength);
                case 0x4949:
                    //TIFF-I:0x49,0x49
                    return GetTiffSize(stream, false, out size, ref fileType, blankHeaderLength);
                case 0x4D4D:
                    //TIFF-M:0x4D,0x4D
                    return GetTiffSize(stream, true, out size, ref fileType, blankHeaderLength);
                case 0x5249:
                    //RIFF:0x52,0x49
                    return GetWebpSize(stream, out size, ref fileType, blankHeaderLength);
            }

            int boxSize = type;
            if (blankHeaderLength == 0)
            {
                boxSize = (boxSize << 16) + BinaryHelper.ReadInt16(stream, true);
            }
            if (boxSize >= 8)
            {
                var buffer = new byte[8];
                stream.Read(buffer, 0, buffer.Length);
                if (Encoding.ASCII.GetString(buffer, 0, buffer.Length) == "ftypavif")
                {
                    //AVIF
                    blankHeaderLength -= 2;
                    if(blankHeaderLength < 0) { blankHeaderLength = 0; }

                    return GetAvifSize(stream, out size, ref fileType, blankHeaderLength, boxSize);
                }
            }

            size = default(Size);
            return false;
        }


        /// <summary>
        /// BMP
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetBmpSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {

            var fileSize = BinaryHelper.ReadInt32(stream, false);//0x02
            SetStreamPosition(stream, 0x0E, blankHeaderLength);
            var infoSize = BinaryHelper.ReadInt32(stream, false);//0x0E

            if (infoSize < 12)
            {
                //Invalid Header
                size = default(Size);
                return false;
            }

            if (infoSize == 12)
            {
                // OS2 Ver 1.x
                var width = BinaryHelper.ReadInt16(stream, false);
                var height = Math.Abs(BinaryHelper.ReadInt16(stream, false));
                size = new Size(width, height);
            }
            else
            {
                // OS2 Ver 2.x,  Windows V3, V4, V5
                var width = BinaryHelper.ReadInt32(stream, false);
                var height = Math.Abs(BinaryHelper.ReadInt32(stream, false));
                size = new Size(width, height);
            }
            fileType = GraphicFileType.Bmp;
            return true;

        }

        /// <summary>
        /// JPEG
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetJpegSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            while (stream.Position < stream.Length)
            {

                var marker = BinaryHelper.ReadInt16(stream, true);
                if (marker >= 0xFFC0 && marker <= 0xFFCF
                    && marker != 0xFFC4 && marker != 0xFFC8
                    && marker != 0xFFCC)
                {
                    break;
                }

                if (marker == 0xFFD9)// End Of Image
                {
                    size = default(Size);
                    return false;
                }


                var segmentLength = BinaryHelper.ReadInt16(stream, true);

                var pos = GetStreamPosition(stream, blankHeaderLength) + segmentLength - 2;

                SetStreamPosition(stream, pos, blankHeaderLength);
                if (pos + 4 >= stream.Length)
                {
                    size = default(Size);
                    return false;
                }
            }

            SetStreamPosition(stream, GetStreamPosition(stream, blankHeaderLength) + 3, blankHeaderLength);
            var height = BinaryHelper.ReadInt16(stream, true);
            var width = BinaryHelper.ReadInt16(stream, true);

            size = new Size(width, height);

            fileType = GraphicFileType.Jpeg;
            return true;

        }

        /// <summary>
        /// GIF
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetGifSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            var type = new byte[4];
            stream.Read(type, 0, type.Length);//0x02
            if (type[0] != 'F' || type[1] != '8'
                || (type[2] != '7' && type[2] != '9') || type[3] != 'a')
            {
                //This is not GIF
                size = default(Size);
                return false;
            }

            var width = BinaryHelper.ReadInt16(stream, false);
            var height = BinaryHelper.ReadInt16(stream, false);
            size = new Size(width, height);

            fileType = GraphicFileType.Gif;
            return true;
        }


        /// <summary>
        /// PNG
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetPngSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            var type = new byte[6];
            stream.Read(type, 0, type.Length);//0x02
            if (type[0] != 'N' || type[1] != 'G'
                || type[2] != (byte)0x0D || type[3] != (byte)0x0A
                || type[4] != (byte)0x1A || type[5] != (byte)0x0A)
            {
                //This is not PNG
                size = default(Size);
                return false;
            }

            SetStreamPosition(stream, 0x0C, blankHeaderLength);

            var ihdr = new byte[4];
            stream.Read(ihdr, 0, ihdr.Length);//0x0C
            if (ihdr[0] != 'I' || ihdr[1] != 'H'
                || ihdr[2] != 'D' || ihdr[3] != 'R')
            {
                //This is not PNG
                size = default(Size);
                return false;
            }

            var width = BinaryHelper.ReadInt32(stream, true);
            var height = BinaryHelper.ReadInt32(stream, true);
            size = new Size(width, height);

            fileType = GraphicFileType.Png;
            return true;

        }

        /// <summary>
        /// PSD
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetPsdSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            var type = new byte[4];
            stream.Read(type, 0, type.Length);//0x02
            if (type[0] != 'P' || type[1] != 'S'
                || type[2] != (byte)0x00 || type[3] != (byte)0x01)
            {
                //This is not PSD
                size = default(Size);
                return false;
            }

            SetStreamPosition(stream, 0x0E, blankHeaderLength);

            var height = BinaryHelper.ReadInt32(stream, true);
            var width = BinaryHelper.ReadInt32(stream, true);

            size = new Size(width, height);

            fileType = GraphicFileType.Psd;
            return true;
        }

        /// <summary>
        /// WMF
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetWmfSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            var type = new byte[2];
            stream.Read(type, 0, type.Length);//0x02
            if (type[0] != (byte)0xC6 || type[1] != (byte)0x9A)
            {
                //This is not WMF
                size = default(Size);
                return false;
            }

            SetStreamPosition(stream, 0x0A, blankHeaderLength);

            var left = BinaryHelper.ReadInt16(stream, false);
            var top = BinaryHelper.ReadInt16(stream, false);
            var right = BinaryHelper.ReadInt16(stream, false);
            var bottom = BinaryHelper.ReadInt16(stream, false);

            var width = Math.Abs(right - left);
            var height = Math.Abs(bottom - top);


            size = new Size(width, height);

            fileType = GraphicFileType.Wmf;
            return true;
        }

        /// <summary>
        /// EMF
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetEmfSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            var type = new byte[2];
            stream.Read(type, 0, type.Length);//0x02
            if (type[0] != (byte)0x00 || type[1] != (byte)0x00)
            {
                //This is not EMF
                size = default(Size);
                return false;
            }

            SetStreamPosition(stream, 0x08, blankHeaderLength);

            var left = BinaryHelper.ReadInt32(stream, false);
            var top = BinaryHelper.ReadInt32(stream, false);
            var right = BinaryHelper.ReadInt32(stream, false);
            var bottom = BinaryHelper.ReadInt32(stream, false);

            var width = Math.Abs(right - left) + 1;
            var height = Math.Abs(bottom - top) + 1;


            SetStreamPosition(stream, 0x28, blankHeaderLength);

            var sign = new byte[4];
            stream.Read(sign, 0, sign.Length);//0x0C
            if (sign[0] != (byte)0x20 || sign[1] != 'E'
                || sign[2] != 'M' || sign[3] != 'F')
            {
                //This is not EMF
                size = default(Size);
                return false;
            }

            size = new Size(width, height);

            fileType = GraphicFileType.Emf;
            return true;
        }

        /// <summary>
        /// TIFF
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="isBigEndian"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static bool GetTiffSize(Stream stream, bool isBigEndian,
            out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            var version = BinaryHelper.ReadInt16(stream, isBigEndian);
            if (version != 0x2A)
            {
                size = default(Size);
                return false;
            }

            var offset = BinaryHelper.ReadInt32(stream, isBigEndian);

            SetStreamPosition(stream, offset, blankHeaderLength);

            var entryCount = BinaryHelper.ReadInt16(stream, isBigEndian);

            int? width = null;
            int? height = null;

            var position = GetStreamPosition(stream, blankHeaderLength);

            for (var i = 0; i < entryCount; i++)
            {

                var tagId = BinaryHelper.ReadInt16(stream, isBigEndian);

                if (tagId == 0x100 || tagId == 0x101)
                {

                    var dataType = BinaryHelper.ReadInt16(stream, isBigEndian);
                    var count = BinaryHelper.ReadInt32(stream, isBigEndian);

                    int num;

                    switch (dataType)
                    {
                        case 3:
                            //2byte
                            num = BinaryHelper.ReadInt16(stream, isBigEndian);
                            break;
                        case 4:
                            //4byte
                            num = BinaryHelper.ReadInt32(stream, isBigEndian);
                            break;
                        default:
                            continue;
                    }

                    if (tagId == 0x100)
                    {
                        width = num;
                    }
                    else
                    {
                        height = num;
                    }

                }

                if (width.HasValue && height.HasValue)
                {
                    size = new Size(width.Value, height.Value);
                    fileType = GraphicFileType.Tiff;
                    return true;
                }

                SetStreamPosition(stream, position + (i + 1) * 12, blankHeaderLength);
            }

            size = default(Size);
            return false;

        }

        private static bool GetWebpSize(Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength)
        {
            byte[] header = new byte[30];
            stream.Read(header, 0, header.Length);

            size = default;

            if (header[6] != 'W' || header[7] != 'E' || header[8] != 'B' || header[9] != 'P'
                 || header[10] != 'V' || header[11] != 'P' || header[12] != '8')
            {
                return false;
            }
            if (header[13] == 0x20)
            {
                if (header[21] != 0x9D || header[22] != 0x01 || header[23] != 0x2A)
                {
                    return false;
                }
                var w = BitConverter.ToUInt16(header, 24);
                var h = BitConverter.ToUInt16(header, 26);

                var width = w & 0x3fff;
                var horiz_scale = w >> 14;
                var height = h & 0x3fff;
                var vert_scale = h >> 14;
                size = new Size(width, height);
            }
            else if (header[13] == 'L')
            {
                if (header[18] != 0x2F)
                {
                    return false;
                }
                var ofst = 19;
                var width = 1 + (((header[ofst + 1] & 0x3F) << 8) | header[ofst + 0]);
                var height = 1 + (((header[ofst + 3] & 0xF) << 10)
                    | (header[ofst + 2] << 2) | ((header[ofst + 1] & 0xC0) >> 6));
                size = new Size(width, height);
            }
            else if (header[13] == 'X')
            {
                var w = BitConverter.ToUInt16(header, 22);
                var h = BitConverter.ToUInt16(header, 25);

                var width = (w & 0xffffff) + 1;
                var height = (h & 0xffffff) + 1;
                size = new Size(width, height);
            }
            else
            {
                return false;
            }

            fileType = GraphicFileType.Webp;
            return true;
        }

        private static bool GetAvifSize(
            Stream stream, out Size size, ref GraphicFileType fileType, int blankHeaderLength,
            int ftypBoxLength)
        {
            // https://nokiatech.github.io/heif/technical.html

            (int boxSize, string id) ReadBoxHeader()
            {
                var boxSize = BinaryHelper.ReadInt32(stream, true);

                var buf = new byte[4];
                stream.Read(buf, 0, buf.Length);
                var id = Encoding.ASCII.GetString(buf, 0, buf.Length);
                return (boxSize, id);
            }
            size = default;
            stream.Position += (ftypBoxLength - 12);

            while (stream.Position < stream.Length)
            {
                var box = ReadBoxHeader();
                //Console.WriteLine($"box [{box.id}] {box.boxSize}bytes");
                if (box.id == "meta")
                {
                    stream.Position += 4;
                    continue;
                }
                else if (box.id == "iprp")
                {
                    continue;
                }
                else if (box.id == "ipco")
                {
                    continue;
                }
                else if (box.id == "ispe")
                {
                    stream.Position += 4;

                    var w = BinaryHelper.ReadInt32(stream, true);
                    var h = BinaryHelper.ReadInt32(stream, true);

                    if (w > 0 && h > 0)
                    {
                        size = new Size(w, h);
                    }
                    break;
                }
                stream.Position += (box.boxSize - 8);
            }

            fileType = GraphicFileType.Avif;
            return (size.Width > 0 && size.Height > 0);
        }

        /// <summary>
        /// ストリームの位置を設定
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="position"></param>
        private static void SetStreamPosition(Stream stream, long position, int blankHeaderLength)
        {
            stream.Position = position + blankHeaderLength;
        }

        private static long GetStreamPosition(Stream stream, int blankHeaderLength)
        {
            return stream.Position - blankHeaderLength;
        }
    }


    class BinaryHelper
    {
        public static uint ReadIInt32(Stream stream, bool inBigEndian)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            if (BitConverter.IsLittleEndian ^ inBigEndian)
            {
                return BitConverter.ToUInt32(buffer, 0);
            }
            else
            {
                return BitConverter.ToUInt32(Reverse(buffer), 0);

            }
        }
        public static int ReadInt32(Stream stream, bool inBigEndian)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);

            if (BitConverter.IsLittleEndian ^ inBigEndian)
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            else
            {
                return BitConverter.ToInt32(Reverse(buffer), 0);

            }
        }
        public static int ReadInt16(Stream stream, bool inBigEndian)
        {
            var buffer = new byte[4];
            var buffer2 = new byte[2];
            stream.Read(buffer2, 0, 2);

            if (BitConverter.IsLittleEndian && !inBigEndian)
            {
                buffer[0] = buffer2[0];
                buffer[1] = buffer2[1];
                return BitConverter.ToInt32(buffer, 0);
            }
            else if (BitConverter.IsLittleEndian && inBigEndian)
            {
                buffer[1] = buffer2[0];
                buffer[0] = buffer2[1];
                return BitConverter.ToInt32(buffer, 0);
            }
            else if (!BitConverter.IsLittleEndian && !inBigEndian)
            {
                buffer[3] = buffer2[0];
                buffer[2] = buffer2[1];
                return BitConverter.ToInt32(buffer, 0);
            }
            else// if (!BitConverter.IsLittleEndian && inBigEndian)
            {
                buffer[2] = buffer2[0];
                buffer[3] = buffer2[1];
                return BitConverter.ToInt32(buffer, 0);
            }
        }

        /*
        public static int ReadIntMain(Stream stream, bool inBigEndian,int length)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, length);

            if (BitConverter.IsLittleEndian && !inBigEndian)
            {
                return BitConverter.ToInt32(buffer, 0);
            }
            else
            {
                return BitConverter.ToInt32(Reverse(buffer), 0);

            }
        }*/


        private static byte[] Reverse(byte[] source)
        {
            var reversed = new byte[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                reversed[i] = source[source.Length - 1 - i];
            }
            return reversed;
        }
    }
}
