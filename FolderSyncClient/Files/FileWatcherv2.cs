using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
            _watcher.Changed += this.Changed;
            _watcher.Created += this.Changed;
			_watcher.Deleted += this.Changed;
			_watcher.Renamed += this.Renamed;
            _watcher.EnableRaisingEvents = true;

            // Create Timer that will call Comparer every half minute
            this._aTimer = new System.Timers.Timer(30000);
	        this._aTimer.Elapsed += (sender, args) => this.OnFilesChanged.Invoke(_changedFiles);
	        this._aTimer.Elapsed += ClearChangedFiles;
	        this._aTimer.Enabled = true;
		}

		/// <summary>
		/// Clear changed files
		/// </summary>
		public void ClearChangedFiles(object source, ElapsedEventArgs e)
		{
			_changedFiles.Clear();
		}

		#region RenamedEventArgs

		/// <summary>
		/// Add renamed files to list of changed files (captain obvious)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		public void Renamed(object source, RenamedEventArgs args)
		{
			var lastAction = GetLastFile(args);

			if (lastAction == null)
				AddFile(args);
			else
			{
				_changedFiles.Remove(lastAction);
				switch (lastAction.status)
				{
					case WatcherChangeTypes.Created:
						AddFile(new FileSystemEventArgs(WatcherChangeTypes.Created, args.FullPath, args.Name));
						break;
					case WatcherChangeTypes.Changed:
						AddFile(new FileSystemEventArgs(WatcherChangeTypes.Deleted, lastAction.path, lastAction.name));
						AddFile(new FileSystemEventArgs(WatcherChangeTypes.Created, args.FullPath, args.Name));
						break;
					case WatcherChangeTypes.Deleted:
						_changedFiles.Add(lastAction);
						break;
					case WatcherChangeTypes.Renamed:
						AddFile(new RenamedEventArgs(WatcherChangeTypes.Renamed, lastAction.oldPath, lastAction.name, lastAction.oldName));
						break;
				}
			}
		}

		/// <summary>
		/// Create FileModel
		/// </summary>
		public FileModel CreateFileModel(RenamedEventArgs args, WatcherChangeTypes status)
		{
			return new FileModel()
			{
				name = args.Name,
				path = args.FullPath,
				oldName = args.OldName,
				oldPath = args.OldFullPath,
				lastChange = DateTime.Now,
				status = status
			};
		}

		/// <summary>
		/// Get last status of changed file
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public FileModel GetLastFile(RenamedEventArgs args)
		{
			var lastFileModel = this._changedFiles.Where(z => z.name == args.OldName && z.path == args.OldFullPath).ToList();
			return (lastFileModel.Count == 0) ? null : lastFileModel.First();
		}
		
		/// <summary>
		/// Add filemodel to collection of changed files
		/// </summary>
		/// <param name="args"></param>
		private void AddFile(RenamedEventArgs args)
		{
			_changedFiles.Add(CreateFileModel(args, args.ChangeType));
		}

		/// <summary>
		/// Add filemodel to collection of changed files
		/// </summary>
		/// <param name="args"></param>
		/// <param name="status"></param>
		private void AddFile(RenamedEventArgs args, WatcherChangeTypes status)
		{
			_changedFiles.Add(CreateFileModel(args, status));
		}
		#endregion

		#region FileSystemEventArgs

		/// <summary>
		/// Add changed files to list of changed files (captain obvious)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		/// <param name="status"></param>
		private void Changed(object source, FileSystemEventArgs args)
		{
			var lastAction = GetLastFile(args);

			if (lastAction == null)
				AddFile(args);
			else
			{
				_changedFiles.Remove(lastAction);
				switch (args.ChangeType)
				{
					case WatcherChangeTypes.Created:
						switch (lastAction.status)
						{
							case WatcherChangeTypes.Deleted:
								_changedFiles.Add(lastAction);
								AddFile(args);
								break;
							case WatcherChangeTypes.Changed:
								AddFile(args);
								break;
							case WatcherChangeTypes.Renamed:
								AddFile(new FileSystemEventArgs(WatcherChangeTypes.Deleted, lastAction.path, lastAction.name));
								AddFile(args);
								break;
							case WatcherChangeTypes.Created:
								AddFile(args);
								break;
						}
						break;
					case WatcherChangeTypes.Changed:
						switch (lastAction.status)
						{
							case WatcherChangeTypes.Deleted:
								AddFile(args);
								break;
							case WatcherChangeTypes.Changed:
								AddFile(args);
								break;
							case WatcherChangeTypes.Renamed:
								AddFile(new FileSystemEventArgs(WatcherChangeTypes.Deleted, lastAction.oldPath, lastAction.oldName));
								AddFile(new FileSystemEventArgs(WatcherChangeTypes.Created, args.FullPath, args.Name));
								break;
							case WatcherChangeTypes.Created:
								_changedFiles.Add(lastAction);
								break;
						}
						break;
					case WatcherChangeTypes.Deleted:
						switch (lastAction.status)
						{
							case WatcherChangeTypes.Deleted:
								AddFile(args);
								break;
							case WatcherChangeTypes.Changed:
								_changedFiles.Add(lastAction);
								break;
							case WatcherChangeTypes.Renamed:
								AddFile(new FileSystemEventArgs(WatcherChangeTypes.Deleted, lastAction.path, lastAction.name));
								AddFile(new FileSystemEventArgs(WatcherChangeTypes.Deleted, lastAction.oldPath, lastAction.oldName));
								break;
							case WatcherChangeTypes.Created:
								break;
						}
						break;
					default:
						Console.WriteLine($"ERROR in FileWatcher! Wrong change type {args.ChangeType}!");
						break;
				}
			}
		}

		/// <summary>
		/// Create FileModel
		/// </summary>
		public FileModel CreateFileModel(FileSystemEventArgs args, WatcherChangeTypes status)
		{
			return new FileModel()
			{
				name = args.Name,
				path = args.FullPath,
				lastChange = DateTime.Now,
				status = status
			};
		}

		/// <summary>
		/// Get last status of changed file
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public FileModel GetLastFile(FileSystemEventArgs args)
		{
			var lastFileModel = this._changedFiles.Where(z => z.name == args.Name && z.path == args.FullPath).ToList();
			return (lastFileModel.Count == 0) ? null : lastFileModel.First();
		}

		/// <summary>
		/// Add filemodel to collection of changed files
		/// </summary>
		/// <param name="args"></param>
	    private void AddFile(FileSystemEventArgs args)
	    {
		    _changedFiles.Add(CreateFileModel(args, args.ChangeType));
	    }

	    /// <summary>
	    /// Add filemodel to collection of changed files
	    /// </summary>
	    /// <param name="args"></param>
	    /// <param name="status"></param>
	    private void AddFile(FileSystemEventArgs args, WatcherChangeTypes status)
	    {
		    _changedFiles.Add(CreateFileModel(args, status));
	    }
		#endregion
	}
}
