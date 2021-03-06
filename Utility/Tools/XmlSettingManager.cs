﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
#if WINDOWS_APP || WINDOWS_UWP
using Windows.Storage;
#endif

namespace Boredbone.Utility
{
    /// <summary>
    /// xmlファイルへのオブジェクトの保存と読み込みを行う
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class XmlSettingManager<T> where T : class, new()
    {
        private static readonly string backUpNameHeader = "backup_";
        private string filePath;

        //#if WINDOWS_APP || WINDOWS_UWP
        //        
        //#else
        //        public string Directory { get; set; }
        //        private string BasePath
        //            => (this.Directory == null || this.Directory.Length <= 0) ? "" : this.Directory + @"\";
        //#endif

        /// <summary>
        /// 保存するファイル名を指定してインスタンスを初期化
        /// </summary>
        /// <param name="filePath">xmlファイル名</param>
        public XmlSettingManager(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>
        /// オブジェクトをシリアライズしてxmlファイルに保存
        /// </summary>
        /// <param name="obj"></param>
#if WINDOWS_APP || WINDOWS_UWP
        public async Task SaveXmlAsync(T obj)
#else
        public async Task SaveXmlAsync(T obj)
        {
            await Task.Run(() => this.SaveXml(obj));
        }

        public void SaveXml(T obj)
#endif
        {

            if (obj == null)
            {
                throw new ArgumentException();
            }


            try
            {

                var setting = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new System.Text.UTF8Encoding(false)
                };

#if WINDOWS_APP || WINDOWS_UWP

                //アプリ固有のフォルダにファイルを生成
                var folder = ApplicationData.Current.LocalFolder;

                var file = await folder.CreateFileAsync
                    (this.GetFileName(), CreationCollisionOption.ReplaceExisting);

                //ファイルストリームからライターを生成
                using (var stream = await file.OpenStreamForWriteAsync())
                using (var xw = XmlWriter.Create(stream, setting))
#else
                //ライターを生成
                using (var xw = XmlWriter.Create(this.filePath, setting))
#endif
                {
                    //シリアライズして保存
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(xw, obj);
                    xw.Flush();
                }

            }
            catch (Exception)
            {
                //例外はそのまま投げる
                throw;
            }
        }

        /// <summary>
        /// xmlファイルを読み込み
        /// ファイルが見つからなかった場合はオブジェクトをnewして返す
        /// 正常に読み込めたらそのファイルを自動でバックアップ
        /// </summary>
        /// <returns></returns>
#if WINDOWS_APP || WINDOWS_UWP
        public async Task<LoadedObjectContainer<T>> LoadXmlAsync()
        {
            return await LoadXmlAsync(XmlLoadingOptions.UseBackup | XmlLoadingOptions.IgnoreNotFound);
        }
#else
        public async Task<LoadedObjectContainer<T>> LoadXmlAsync()
        {
            return await Task.Run(() => this.LoadXml());
        }
        public LoadedObjectContainer<T> LoadXml()
        {
            return LoadXml(XmlLoadingOptions.UseBackup | XmlLoadingOptions.IgnoreNotFound);
        }
#endif

        /// <summary>
        /// xmlファイルを読み込み
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
#if WINDOWS_APP || WINDOWS_UWP
        public async Task<LoadedObjectContainer<T>> LoadXmlAsync(XmlLoadingOptions options)
#else
        public async Task<LoadedObjectContainer<T>> LoadXmlAsync(XmlLoadingOptions options)
        {
            return await Task.Run(() => this.LoadXml(options));
        }
        public LoadedObjectContainer<T> LoadXml(XmlLoadingOptions options)
#endif
        {


            Exception errorMessage = null;

            try
            {
                //ファイルから読み込み
#if WINDOWS_APP || WINDOWS_UWP
                var folder = ApplicationData.Current.LocalFolder;

                var file = await folder.GetFileAsync(this.GetFileName());

                var loaded = await this.LoadMainAsync(folder, file);
#else
                var loaded = this.LoadMain(this.filePath);
#endif

                //自動バックアップを使用する場合、正常に読み込めたファイルを別名でコピー
                if (options.HasFlag(XmlLoadingOptions.UseBackup) || options.HasFlag(XmlLoadingOptions.DoBackup))
                {
                    try
                    {
#if WINDOWS_APP || WINDOWS_UWP
                        var copied = await file.CopyAsync
                            (folder, this.GetBackupFileName(), NameCollisionOption.ReplaceExisting);
#else
                        File.Copy(this.filePath, this.GetBackupFilePath(), true);
#endif
                    }
                    catch (Exception e)
                    {
                        return new LoadedObjectContainer<T>(loaded, e);
                    }
                }

                //コンテナに入れて返す
                return new LoadedObjectContainer<T>(loaded, null);
            }
            catch (FileNotFoundException)
            {
                //ファイルが存在しない場合

                if (options.HasFlag(XmlLoadingOptions.UseBackup))
                {
                    //バックアップを使用する設定の場合はスルー
                    //errorMessage = null;
                }
                else if (options.HasFlag(XmlLoadingOptions.IgnoreAllException)
                   || options.HasFlag(XmlLoadingOptions.IgnoreNotFound))
                {
                    //例外を無視する設定の場合はnewして返す
                    return new LoadedObjectContainer<T>(
                        (options.HasFlag(XmlLoadingOptions.ReturnNull) ? null : new T()),
                        null);
                }
                else
                {
                    //例外を投げる
                    throw;
                }
            }
            catch (Exception e)
            {
                //その他の例外

                if (options.HasFlag(XmlLoadingOptions.UseBackup))
                {
                    //例外を記憶
                    errorMessage = e;
                }
                else if (options.HasFlag(XmlLoadingOptions.IgnoreAllException))
                {
                    //例外を無視する設定の場合はnewして返す
                    return new LoadedObjectContainer<T>
                        ((options.HasFlag(XmlLoadingOptions.ReturnNull) ? null : new T()), e);
                }
                else
                {
                    //例外を投げる
                    throw;
                }
            }


            //バックアップを使用する設定の場合、バックアップファイルを読み込む
#if WINDOWS_APP || WINDOWS_UWP
            return await this.LoadBackupMainAsync(options, errorMessage);
#else
            return this.LoadBackupMain(options, errorMessage);
#endif

        }

        /// <summary>
        /// バックアップのxmlファイルを読み込む
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
#if WINDOWS_APP || WINDOWS_UWP
        public async Task<LoadedObjectContainer<T>> LoadBackupXmlAsync(XmlLoadingOptions options)
        {
            return await this.LoadBackupMainAsync(options, null);
        }
#else
        public LoadedObjectContainer<T> LoadBackupXml(XmlLoadingOptions options)
        {
            return this.LoadBackupMain(options, null);

        }
#endif


#if WINDOWS_APP || WINDOWS_UWP
        public async Task<LoadedObjectContainer<T>> LoadBackupMainAsync(XmlLoadingOptions options, Exception errorMessage)
#else
        private LoadedObjectContainer<T> LoadBackupMain(XmlLoadingOptions options, Exception errorMessage)
#endif
        {

            try
            {
                //バックアップファイルを読み込む
#if WINDOWS_APP || WINDOWS_UWP
                var folder = ApplicationData.Current.LocalFolder;

                var file = await folder.GetFileAsync(this.GetBackupFileName());

                var loaded = await this.LoadMainAsync(folder, file);
#else
                var loaded = this.LoadMain(this.GetBackupFilePath());
#endif

                return new LoadedObjectContainer<T>(loaded, errorMessage);
            }
            catch (FileNotFoundException)
            {
                //バックアップファイルも存在しなかった場合

                if (options.HasFlag(XmlLoadingOptions.IgnoreAllException)
                   || options.HasFlag(XmlLoadingOptions.IgnoreNotFound))
                {
                    //例外を無視する設定の場合はnewして返す
                    return new LoadedObjectContainer<T>
                        ((options.HasFlag(XmlLoadingOptions.ReturnNull) ? null : new T()), errorMessage);
                }
                else
                {
                    //例外を投げる
                    throw;
                }
            }
            catch (Exception e)
            {
                //その他の例外

                if (options.HasFlag(XmlLoadingOptions.IgnoreAllException))
                {
                    return new LoadedObjectContainer<T>
                        ((options.HasFlag(XmlLoadingOptions.ReturnNull) ? null : new T()), errorMessage ?? e);
                }
                else
                {
                    throw;
                }
            }
        }

#if WINDOWS_APP || WINDOWS_UWP
        private async Task<T> LoadMainAsync(StorageFolder folder,StorageFile file)
        {
            using (var stream = await file.OpenStreamForWriteAsync())
            {
                var serializer = new DataContractSerializer(typeof(T));
                stream.Position = 0;
                var value = serializer.ReadObject(stream);

                return (T)value;
            }
        }
#else
        private T LoadMain(string path)
        {
            using (var xr = XmlReader.Create(path))
            {
                var serializer = new DataContractSerializer(typeof(T));
                var value = serializer.ReadObject(xr);

                return (T)value;
            }
        }
#endif

#if WINDOWS_APP || WINDOWS_UWP
        public async Task DeleteFileAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.GetFileAsync(this.GetFileName());
                if (file != null)
                {
                    await file.DeleteAsync();
                }
            }
            catch
            {

            }
        }
#else
        public void DeleteFile()
        {
            //TODO
        }
#endif



#if WINDOWS_APP || WINDOWS_UWP

        private string GetFileName() => Path.GetFileName(this.filePath);

        private string GetBackupFileName() => backUpNameHeader + this.GetFileName();
#else
        private string GetBackupFilePath()
        {
            var dir = Path.GetDirectoryName(this.filePath);
            var name = Path.GetFileName(this.filePath);

            return Path.Combine(dir, backUpNameHeader + name);
        }

#endif

    }

    /// <summary>
    /// xmlファイルから読み込まれたオブジェクトと読み込み時に発生した例外の情報
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LoadedObjectContainer<T>
    {
        public T Value { get; private set; }
        public Exception Message { get; private set; }

        public LoadedObjectContainer(T value, Exception message)
        {
            this.Value = value;
            this.Message = message;
        }
    }

    [Flags]
    public enum XmlLoadingOptions
    {
        /// <summary>
        /// 発生した全ての例外を投げる
        /// </summary>
        ThrowAll = 0x00,

        /// <summary>
        /// バックアップファイルを使用する
        /// </summary>
        UseBackup = 0x01,

        /// <summary>
        /// FileNotFoundExceptionを内部で処理する
        /// </summary>
        IgnoreNotFound = 0x02,

        /// <summary>
        /// 全ての例外を内部で処理する
        /// </summary>
        IgnoreAllException = 0x04,

        /// <summary>
        /// ロードに失敗した場合はnullを返す
        /// </summary>
        ReturnNull = 0x08,

        /// <summary>
        /// バックアップを行うが読み込まない
        /// </summary>
        DoBackup = 0x10,
    }
}
