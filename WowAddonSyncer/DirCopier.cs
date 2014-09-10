using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WowAddonSyncer
{
 public class DirectoryCopier
    {
        public static void CopyDirectory(string originalDirectory, string newDirectory)
        {
            MoveDirectory(originalDirectory, newDirectory);
        }

        private static void MoveDirectory(string originalDirectory, string newDirectory)
        {
            if (!Directory.Exists(newDirectory))
                Directory.CreateDirectory(newDirectory);

            DirectoryInfo oldDir = new DirectoryInfo(originalDirectory);

            foreach (FileInfo file in oldDir.GetFiles())
            {
                string oldPath = file.FullName;
                string newPath = newDirectory + "\\" + file.Name;
                File.Copy(oldPath, newPath);
            }

            foreach(DirectoryInfo dir in oldDir.GetDirectories())
            {
                string oldPath = dir.FullName;
                string newPath = newDirectory + "\\" + dir.Name;
                MoveDirectory(oldPath, newPath);
            }
        }
    }
}