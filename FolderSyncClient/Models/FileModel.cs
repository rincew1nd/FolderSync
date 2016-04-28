using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FolderSyncClient.Models
{
    class FileModel
    {
        public string name;
		public string oldName;
		public string path;
		public string oldPath;
        public DateTime lastChange;
	    public WatcherChangeTypes status;
		public byte[] fileBytes;

		public override bool Equals(object obj)
        {
            if (!(obj is FileModel)) return false;

            var two = (FileModel) obj;
            return this.lastChange.Equals(two.lastChange) &&
                   this.name.Equals(two.name) &&
                   this.path.Equals(two.path);
        }
    }
}
