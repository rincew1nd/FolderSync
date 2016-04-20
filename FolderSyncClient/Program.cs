using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSyncClient.Models;

namespace FolderSyncClient
{
	class Program
	{
		static void Main(string[] args)
		{
			FolderSyncClient.Instance.Run();
		}
	}
}
