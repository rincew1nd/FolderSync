using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Timers;
using FolderSyncClient.Models;
using Newtonsoft.Json;

namespace FolderSyncClient.Files
{
    class FileWatcher
    {
		// Folder that FileWatcher will watch
        private readonly string _folderToWatch;

		// FileWatcher...
        private readonly FileSystemWatcher _watcher;

		// Timer that will call for sync
		private readonly Timer _aTimer;

		// List of tracked changed files
	    private List<FileModel> _changedFiles;

		// Event + delegate for notify client to send info about changes
        public delegate void FileWatherEventHandler(List<FileModel> changedFiles);
        public event FileWatherEventHandler OnFilesChanged;

        /// <summary>
        /// FileWatcher that watch for file changed
        /// </summary>
        /// <param name="folderToWatch">Patch to tracking folder</param>
        public FileWatcher(string folderToWatch)
        {
			// Initialize list of changed files
			_changedFiles = new List<FileModel>();

	        _folderToWatch = folderToWatch;

            //Initialize FileSystemWatcher to determine is folder changed
            _watcher = new FileSystemWatcher
            {
                Path = folderToWatch,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                               | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = "*.*"
            };
            _watcher.Changed += (sender, args) => this.Changed(sender, args, FileStatus.Edited);
            _watcher.Created += (sender, args) => this.Changed(sender, args, FileStatus.Added);
			_watcher.Deleted += (sender, args) => this.Changed(sender, args, FileStatus.Deleted);
			_watcher.Renamed += this.Renamed;
            _watcher.EnableRaisingEvents = true;

            // Create Timer that will call Comparer every half minute
            this._aTimer = new System.Timers.Timer(30000);
	        this._aTimer.Elapsed += (sender, args) => this.OnFilesChanged.Invoke(_changedFiles);
	        this._aTimer.Enabled = true;
        }

        /// <summary>
        /// Add changed files to list of changed files (captain obvious)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private void Changed(object source, FileSystemEventArgs args, FileStatus status)
        {
			this._changedFiles.Add(
				new FileModel()
				{
					name = args.Name,
					path = args.FullPath,
					lastChange = DateTime.Now,
					status = status
				}
			);
        }

		/// <summary>
		/// Add renamed files to list of changed files (captain obvious)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		public void Renamed(object source, RenamedEventArgs args)
	    {
			this._changedFiles.Add(
				new FileModel()
				{
					name = args.Name,
					path = args.FullPath,
					lastChange = DateTime.Now,
					status = FileStatus.Renamed
				}
			);
		}
    }
}
