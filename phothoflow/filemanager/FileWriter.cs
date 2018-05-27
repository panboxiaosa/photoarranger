using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using phothoflow.location;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using phothoflow.setting;
using System.Drawing;
using System.Drawing.Imaging;

namespace phothoflow.filemanager
{
    class FileWriter
    {
        public void Write(string target, List<Item> objs, float height)
        {
            int pixelWidth = (int)(SettingManager.GetWidth() * 60);
            int pixelHeight = (int)(height * 60);

            //Mat mat = new Mat();
            //mat.Create(pixelHeight, pixelWidth, DepthType.Cv8U, 3);
            Bitmap result = new Bitmap(pixelWidth, pixelHeight, PixelFormat.Format32bppPArgb);

        }
    }
}
