//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Timers;
//using Newtonsoft.Json;
//
//namespace FolderSync
//{
//    class FileWatcher
//    {
//        private readonly string _folderToWatch;
//        private readonly FileSystemWatcher _watcher;
//        private readonly Timer _aTimer;
//
//        private List<Files> _filesInFolder = new List<Files>();
//        private string _filesInFolderJson = "";
//        private bool _isChanged = false;
//
//        public delegate void FileWatherEventHandler(string filesInFolderJson);
//        public event FileWatherEventHandler OnFilesChanged;
//
//        /// <summary>
//        /// FileWatcher that watch for file changed
//        /// </summary>
//        /// <param name="folderToWatch">Patch to tracking folder</param>
//        public FileWatcher(string folderToWatch)
//        {
//            _folderToWatch = folderToWatch;
//            _filesInFolder = LoadAllFilesInfo();
//            _filesInFolderJson = JsonConvert.SerializeObject(_filesInFolder);
//
//            //Create FileSystemWatcher to determine is folder changed
//            var _watcher = new FileSystemWatcher
//            {
//                Path = folderToWatch,
//                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
//                               | NotifyFilters.FileName | NotifyFilters.DirectoryName,
//                Filter = "*.*"
//            };
//            _watcher.Changed += Changed;
//            _watcher.Created += Changed;
//            _watcher.Deleted += Changed;
//            _watcher.Renamed += Changed;
//            _watcher.EnableRaisingEvents = true;
//
//            // Create Timer that will call Comparer every half minute
//            _aTimer = new System.Timers.Timer(30000);
//            _aTimer.Elapsed += new ElapsedEventHandler(CompareFiles);
//            _aTimer.Enabled = true;
//        }
//
//        /// <summary>
//        /// Change isChanged to true when changes happens
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="e"></param>
//        private void Changed(object source, FileSystemEventArgs e)
//        {
//            _isChanged = true;
//        }
//
//        /// <summary>
//        /// Load all files in folder
//        /// </summary>
//        private List<Files> LoadAllFilesInfo()
//        {
//            var filesInFolder = new List<Files>();
//            foreach (var filePath in Directory.GetFiles(_folderToWatch, "*", SearchOption.AllDirectories))
//            {
//                filesInFolder.Add(
//                    new Files()
//                    {
//                        _name = Path.GetFileName(filePath),
//                        _path = Path.GetDirectoryName(filePath),
//                        _md5hash = Utils.CalculateMD5(filePath),
//                        _lastChange = File.GetLastWriteTime(filePath)
//                    }
//                );
//            }
//            return filesInFolder;
//        }
//
//        /// <summary>
//        /// Compare all files in folder
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="e"></param>
//        private void CompareFiles(object source, ElapsedEventArgs e)
//        {
//            if (!_isChanged) return;
//            Console.WriteLine("Comparing...");
//
//            var createdFiles = new List<Files>();
//            var deletedFiles = new List<Files>();
//            var renamedFiles = new List<Files>();
//
//            var oldFolder = _filesInFolder.ToList();
//            _filesInFolder = LoadAllFilesInfo();
//            var oldFilesDiff = oldFolder.Where(z => !_filesInFolder.Contains(z)).ToList();
//            var newFilesDiff = _filesInFolder.Where(z => !oldFolder.Contains(z)).ToList();
//
//            foreach (var diffFile in newFilesDiff)
//            {
//                if (oldFilesDiff.Count(file => file._md5hash == diffFile._md5hash) > 0)
//                    renamedFiles.Add(diffFile);
//                else
//                    createdFiles.Add(diffFile);
//            }
//            foreach (var diffFile in oldFilesDiff)
//            {
//                if (newFilesDiff.Count(file => file._md5hash == diffFile._md5hash) == 0)
//                    deletedFiles.Add(diffFile);
//            }
//
//            createdFiles.ForEach(i => Console.WriteLine("Created file {0}", i._name));
//            deletedFiles.ForEach(i => Console.WriteLine("Deleted file {0}", i._name));
//            renamedFiles.ForEach(i => Console.WriteLine("Renamed file {0}", i._name));
//
//            _isChanged = false;
//
//            _filesInFolderJson = JsonConvert.SerializeObject(_filesInFolder);
//            OnFilesChanged(_filesInFolderJson);
//        }
//    }
//}
//