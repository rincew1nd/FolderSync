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
            _watcher.Changed += (sender, args) => this.Changed(sender, args, FileStatus.Edited);
            _watcher.Created += (sender, args) => this.Changed(sender, args, FileStatus.Added);
			_watcher.Deleted += (sender, args) => this.Changed(sender, args, FileStatus.Deleted);
			_watcher.Renamed += this.Renamed;
            _watcher.EnableRaisingEvents = true;

            // Create Timer that will call Comparer every half minute
            this._aTimer = new System.Timers.Timer(30000);
	        this._aTimer.Elapsed += (sender, args) => this.OnFilesChanged.Invoke(_changedFiles);
	        this._aTimer.Elapsed += ClearChangedFiles;
	        this._aTimer.Enabled = true;
        }

        /// <summary>
        /// Add changed files to list of changed files (captain obvious)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        /// <param name="status"></param>
        private void Changed(object source, FileSystemEventArgs args, FileStatus status)
        {
	        switch (status)
	        {
		        case FileStatus.Added:
					AddFileToFileList(args, status);
			        break;
				case FileStatus.Deleted:
					DeleteAllFromFileList(args);
					AddFileToFileList(args, status);
			        break;
				case FileStatus.Edited:
			        switch (GetLastFileStatus(args))
			        {
						case FileStatus.Added:
							DeleteAllFromFileList(args);
							AddFileToFileList(args, FileStatus.Added);
							break;
						case FileStatus.Deleted:
							DeleteAllFromFileList(args);
							AddFileToFileList(args, FileStatus.Deleted);
							AddFileToFileList(args, FileStatus.Added);
							break;
						case FileStatus.Renamed:
							Console.WriteLine("That cant be happening! >.<");
							break;
						case FileStatus.Null:
							AddFileToFileList(args, status);
							break;
						default:
							break;
					}
					break;
				default:
					Console.WriteLine("Very strange status passed!");
			        break;
	        }
        }


		/// <summary>
		/// Add renamed files to list of changed files (captain obvious)
		/// </summary>
		/// <param name="source"></param>
		/// <param name="args"></param>
		public void Renamed(object source, RenamedEventArgs args)
		{
			switch (GetLastFileStatus(args.OldName, args.OldFullPath))
			{
				case FileStatus.Added:
					break;
				case FileStatus.Edited:
					DeleteAllFromFileList(args);
					AddFileToFileList(args.OldName, args.OldFullPath, FileStatus.Deleted);
					AddFileToFileList(args.Name, args.FullPath, FileStatus.Added);
					break;
				case FileStatus.Deleted:
					break;
				case FileStatus.Renamed:
					var fileModel = GetLastFile(args);
					DeleteFromFileList(args.OldName, args.OldFullPath, new List<FileStatus>(){FileStatus.Renamed});
					_changedFiles.Add(
						new FileModel()
						{
							name = args.Name,
							path = args.FullPath,
							oldName = fileModel.oldName,
							oldPath = fileModel.oldPath,
							lastChange = DateTime.Now,
							status = FileStatus.Renamed,
						}
					);
					break;
				case FileStatus.Null:
					AddFileToFileList(args, FileStatus.Renamed);
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Delete all files in changed files list with given statuses
		/// </summary>
		/// <param name="file">FileModel</param>
		/// <param name="statuses">List of statuses</param>
		public void DeleteFromFileList(FileModel file, List<FileStatus> statuses)
	    {
			foreach (var changedFile in this._changedFiles.Where(z => z.name == file.name && z.path == file.path && statuses.Contains(z.status)).ToList())
				this._changedFiles.Remove(changedFile);
		}

		/// <summary>
		/// Delete all files in changed files list with given statuses
		/// </summary>
		/// <param name="name">File name</param>
		/// <param name="path">File path</param>
		/// <param name="statuses">List of statuses</param>
		public void DeleteFromFileList(string name, string path, List<FileStatus> statuses)
	    {
		    DeleteFromFileList(new FileModel() { name = name, path = path }, statuses);
	    }

		/// <summary>
		/// Delete all files in changed files list with all statuses
		/// </summary>
		/// <param name="name">File name</param>
		/// <param name="path">File path</param>
		public void DeleteAllFromFileList(string name, string path)
	    {
		    DeleteFromFileList(new FileModel() { name = name, path = path }, new List<FileStatus>() {FileStatus.Added, FileStatus.Deleted, FileStatus.Edited, FileStatus.Renamed});
	    }

	    /// <summary>
	    /// Delete all files in changed files list with all statuses
	    /// </summary>
	    /// <param name="args"></param>
	    public void DeleteAllFromFileList(FileSystemEventArgs args)
	    {
		    DeleteFromFileList(new FileModel() { name = args.Name, path = args.FullPath }, new List<FileStatus>() {FileStatus.Added, FileStatus.Deleted, FileStatus.Edited, FileStatus.Renamed});
	    }

	    /// <summary>
	    /// Add changed file to list of changed files
	    /// </summary>
	    /// <param name="file">FileModel</param>
	    public void AddFileToFileList(FileModel file)
	    {
		    _changedFiles.Add(file);
	    }

	    /// <summary>
	    /// Add changed file to list of changed files
	    /// </summary>
	    /// <param name="args"></param>
	    /// <param name="status"></param>
	    public void AddFileToFileList(RenamedEventArgs args, FileStatus status)
	    {
		    _changedFiles.Add(new FileModel()
		    {
			    name = args.Name,
				path = args.FullPath,
				oldName = args.OldName,
				oldPath = args.OldFullPath,
				lastChange = DateTime.Now,
				status = status
		    });
	    }

		/// <summary>
		/// Add changed file to list of changed files
		/// </summary>
		/// <param name="name">File name</param>
		/// <param name="path">File path</param>
		/// <param name="status">File status</param>
		public void AddFileToFileList(string name, string path, FileStatus status)
	    {
			AddFileToFileList(new FileModel()
			{
				name = name,
				path = path,
				lastChange = DateTime.Now,
				status = status
			});
	    }

	    /// <summary>
	    /// Add changed file to list of changed files
	    /// </summary>
	    /// <param name="args"></param>
	    /// <param name="status">File status</param>
	    public void AddFileToFileList(FileSystemEventArgs args, FileStatus status)
	    {
			AddFileToFileList(new FileModel()
			{
				name = args.Name,
				path = args.FullPath,
				lastChange = DateTime.Now,
				status = status
			});
	    }

		/// <summary>
		/// Get last status of changed file
		/// </summary>
		/// <param name="name"></param>
		/// <param name="path"></param>
		/// <returns></returns>
	    public FileStatus GetLastFileStatus(string name, string path)
	    {
		    return this._changedFiles.Any(z => z.name == name && z.path == path) ?
				this._changedFiles.Where(z => z.name == name && z.path == path).Select(z => z.status).First() : FileStatus.Null;
		}

		/// <summary>
		/// Get last status of changed file
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public FileStatus GetLastFileStatus(FileSystemEventArgs args)
	    {
		    return this._changedFiles.Any(z => z.name == args.Name && z.path == args.FullPath) ?
				this._changedFiles.Where(z => z.name == args.Name && z.path == args.FullPath).Select(z => z.status).First() : FileStatus.Null;
	    }

		/// <summary>
		/// Get last status of changed file
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public FileModel GetLastFile(RenamedEventArgs args)
	    {
		    return this._changedFiles.Any(z => z.name == args.OldName && z.path == args.OldFullPath) ?
				this._changedFiles.Where(z => z.name == args.OldName && z.path == args.OldFullPath).First() : new FileModel();
	    }

		/// <summary>
		/// Clear changed files
		/// </summary>
	    public void ClearChangedFiles(object source, ElapsedEventArgs e)
	    {
		    _changedFiles.Clear();
	    }
	}
}
