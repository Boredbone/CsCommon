using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Boredbone.Utility.Tools
{

    /// <summary>
    /// http://sumikko8note.blog.fc2.com/blog-entry-30.html
    /// </summary>
    public class BuildTimeStamp
    {
        public static DateTime GetCurrentAssemblyTimeStamp()
        {
            var assembly = Assembly.GetEntryAssembly();
            return GetDateTimeUtcFrom(assembly.Location);
        }

        /// <summary>
        /// アセンブリファイルのビルド日時を取得する。
        /// </summary>
        /// <param name="asmPath">exeやdll等のアセンブリファイルのパス。
        /// <returns>取得したビルド日時。</returns>
        public static DateTime GetDateTimeUtcFrom(string asmPath)
        {
            using (var fs = new FileStream(asmPath, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                byte[] signature = { 0x50, 0x45, 0x00, 0x00 };// "PE\0\0"

                var currentSignatureIndex = 0;

                for (int j = 0; j < 512; j++)
                {
                    var data = br.ReadByte();

                    if (currentSignatureIndex > 0 && signature[currentSignatureIndex] == data)
                    {
                        currentSignatureIndex++;
                        if (currentSignatureIndex >= signature.Length)
                        {
                            break;
                        }
                    }
                    else if (signature[0] == data)
                    {
                        currentSignatureIndex = 1;
                    }
                }

                if (currentSignatureIndex == 0)
                {
                    throw new ArgumentException("no signatune");
                }

                // COFFファイルヘッダを読み取る

                var machine = br.ReadBytes(2);
                var numberOfSections = br.ReadBytes(2);
                var timeDateStamp = br.ReadBytes(4);

                //var pointerToSymbolTable = br.ReadBytes(4);
                //var numberOfSymbols = br.ReadBytes(4);
                //var sizeOfOptionalHeader = br.ReadBytes(2);
                //var characteristics = br.ReadBytes(2);


                // タイムスタンプをDateTimeに変換
                int timestamp = BitConverter.ToInt32(timeDateStamp, 0);
                var buildDateTimeUtc = UnixTime.ToDateTime(timestamp);

                return buildDateTimeUtc;
            }
        }
    }
}
