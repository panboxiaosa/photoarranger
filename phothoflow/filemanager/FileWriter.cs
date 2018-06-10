using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using phothoflow.location;
using phothoflow.setting;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Diagnostics;

namespace phothoflow.filemanager
{
    class FileWriter
    {

        public void Save(string target, List<Item> objs, float height)
        {

                int dpi = (int)SettingManager.GetDpi();
                int pixelWidth = (int)(SettingManager.GetWidth() * dpi);
                int pixelHeight = (int)(height * dpi);
                int margin = (int)(dpi * SettingManager.GetMargin());

                string[] lines = new string[objs.Count + 1];

                lines[0] = "" + pixelWidth + "$" + pixelHeight + "$" + dpi + "$" + margin;
                for (int i = 0; i < objs.Count; i++)
                {
                    Item one = objs[i];
                    lines[i + 1] = one.OriginPath + 
                        "$" + (int)(one.Left * dpi) + 
                        "$" + (int)(one.Top * dpi) + 
                        "$" + (int)(one.RealWidth * dpi) +
                        "$" + (int)(one.RealHeight * dpi) +
                        "$" + (one.Rotated ? 1 : 0);
                }
                
                System.IO.File.WriteAllLines(target, lines, Encoding.Default);

        }

        public void Write(string target, List<Item> objs, float height)
        {
            string des = target.Replace(".tif", ".pbf");
            Save(des, objs, height);
            Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + "imgmerge.exe ", "-m " + des + " " + target);

        }

    }
}
