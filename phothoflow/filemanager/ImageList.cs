using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace phothoflow.filemanager
{
    class ImageList
    {
        public static List<string> listDirectory(String path)
        {
            
            List<String> result = new List<String>();

            DirectoryInfo theFolder = new DirectoryInfo(path);

            //遍历文件
            foreach (FileInfo NextFile in theFolder.GetFiles())
            {
                String fullPath = NextFile.FullName;
                String lower = fullPath.ToLower();
                if (lower.EndsWith(".jpg") || lower.EndsWith(".tif") || lower.EndsWith(".jpeg"))
                {
                    result.Add(fullPath);
                }
                
            }

            //遍历文件夹
            foreach (DirectoryInfo NextFolder in theFolder.GetDirectories())
            {
                result.AddRange(listDirectory(NextFolder.FullName));
            }

            return result;
        }
    }
}
