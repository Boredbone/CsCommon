using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Boredbone.Utility.Extensions;
using Boredbone.Utility.Notification;
using Reactive.Bindings.Extensions;

namespace Boredbone.Utility.Tools
{
#if !WINDOWS_APP && !WINDOWS_UWP
    public class FolderWatcher : DisposableBase
    {
        private Dictionary<string, IDisposable> Watchers { get; }

        private Subject<ExtendedFileSystemEventArgs> FolderChangedSubject { get; }
        public IObservable<ExtendedFileSystemEventArgs> FolderChanged => this.FolderChangedSubject.AsObservable();

        public FolderWatcher()
        {
            this.Watchers = new Dictionary<string, IDisposable>();

            this.FolderChangedSubject = new Subject<ExtendedFileSystemEventArgs>().AddTo(this.Disposables);

            Disposable.Create(() =>
            {
                this.Watchers.ForEach(x => x.Value.Dispose());
                this.Watchers.Clear();
            })
            .AddTo(this.Disposables);
        }

        public void Add(string path, bool includeSubdirectories)
        {
            var folderDisposable = new CompositeDisposable();

            var watcher = new System.IO.FileSystemWatcher();

            //監視するディレクトリを指定
            watcher.Path = path;


            //最終アクセス日時、最終更新日時、ファイル、フォルダ名の変更を監視する
            watcher.NotifyFilter =
                (System.IO.NotifyFilters.LastAccess
                | System.IO.NotifyFilters.LastWrite
                | System.IO.NotifyFilters.FileName
                | System.IO.NotifyFilters.DirectoryName);
            //すべてのファイルを監視
            watcher.Filter = "*";

            watcher.IncludeSubdirectories = includeSubdirectories;

            var changed =
                Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>
                (h => (sender, e) => h(e), h => watcher.Changed += h, h => watcher.Changed -= h)
                .Select(x => new ExtendedFileSystemEventArgs(x.ChangeType, x.FullPath, null, path));

            var created =
                Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>
                (h => (sender, e) => h(e), h => watcher.Created += h, h => watcher.Created -= h)
                .Select(x => new ExtendedFileSystemEventArgs(x.ChangeType, x.FullPath, null, path));

            var deleted =
                Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>
                (h => (sender, e) => h(e), h => watcher.Deleted += h, h => watcher.Deleted -= h)
                .Select(x => new ExtendedFileSystemEventArgs(x.ChangeType, x.FullPath, null, path));

            var renamed =
                Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>
                (h => (sender, e) => h(e), h => watcher.Renamed += h, h => watcher.Renamed -= h)
                .Select(x => new ExtendedFileSystemEventArgs(x.ChangeType, x.FullPath, x.OldFullPath, path));

            var subscription = Observable
                .Merge(changed, created, deleted, renamed)
                .Subscribe(this.FolderChangedSubject);

            watcher.EnableRaisingEvents = true;

            Disposable.Create(() =>
            {
                watcher.EnableRaisingEvents = false;
                subscription.Dispose();
                watcher.Dispose();
            })
            .AddTo(this.Watchers, path);
            

        }


        public void Remove(string path)
        {
            IDisposable value;
            if (this.Watchers.TryGetValue(path, out value))
            {
                this.Watchers.Remove(path);
                value.Dispose();
            }
        }

    }

    public class ExtendedFileSystemEventArgs
    {
        public WatcherChangeTypes ChangeType { get; }
        public string FullPath { get; }
        public string OldFullPath { get; }
        public string Directory { get; }

        public ExtendedFileSystemEventArgs
            (WatcherChangeTypes changeType, string fullPath, string oldFullPath, string directory)
        {
            this.ChangeType = changeType;
            this.FullPath = fullPath;
            this.OldFullPath = oldFullPath;
            this.Directory = directory;
        }


    }
#endif
}
