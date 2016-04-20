using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FolderSyncClient.Models
{
    class FileModel
    {
        public string name;
        public string path;
        public DateTime lastChange;
	    public FileStatus status;

        public override bool Equals(object obj)
        {
            if (!(obj is FileModel)) return false;

            var two = (FileModel) obj;
            return this.lastChange.Equals(two.lastChange) &&
                   this.name.Equals(two.name) &&
                   this.path.Equals(two.path);
        }
    }
	
	public enum FileStatus
	{
		Added,
		Renamed,
		Edited,
		Deleted,
	}
}
