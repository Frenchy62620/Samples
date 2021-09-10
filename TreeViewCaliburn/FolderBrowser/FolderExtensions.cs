using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TreeViewCaliburn.FolderBrowser
{
    public static class FolderExtensions
    {


        public static string GetParentDirOfFile(this string path) => new FileInfo(path).Directory.FullName;

        public static string GetDirFromFile(this string path) => path.Replace(".htm", "_fichiers");

        public static string GetFileFromDir(this string path) => path.Replace("_fichiers", ".htm");

        public static string GetFirstParentDirOfFile(this string path) => Directory.GetParent(new FileInfo(path).Directory.FullName).FullName;

        public static string GetParentDir(this string folder) => Directory.GetParent(folder).FullName;

        public static bool IsFolderExist(this string folder) => Directory.Exists(folder);

        public static bool IsFileExist(this string path) => File.Exists(path);

        public static string GetShortName(this string path, bool htm = true)
        {
            var p = path.Split('/', '\\').Last();
            return htm && !p.EndsWith(".htm") ? $"{p}.htm" : p; 
        }
        public static string GetMasterName(this string path, bool fullname = false)
        {
            var pattern = "href=..*(master.*?css).";
            var content = File.ReadAllText(path);
            var found = Regex.Match(content, pattern);
            return fullname ? Path.Combine(path.GetDirFromFile(), found.Groups[1].Value) : found.Groups[1].Value;
        }

        //public static string[] getDir<T>(this string path)
        //{
        //    var dirs = Directory.GetDirectories(path)
        //    .Where(dir => (File.GetAttributes(dir) & FileAttributes.Hidden) != FileAttributes.Hidden)
        //    .Select(dir => new FolderTreeItemViewModel(firstparent, this, dir));
        //}

        public static Task ParallelForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> funcBody, int maxDoP = 4)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        //await Console.Out.WriteLineAsync($"****Processing Message: {(partition.Current as FolderTreeItemViewModel).Name}");
                        await Task.Yield(); // prevents a sync/hot thread hangup
                        await funcBody(partition.Current);
                    }
                }
            }

            return Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDoP)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
        }


    }
}
