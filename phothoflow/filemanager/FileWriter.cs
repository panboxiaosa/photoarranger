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
            int pixelWidth = (int)(SettingManager.GetWidth() * SettingManager.GetDpi());
            int pixelHeight = (int)(height * SettingManager.GetDpi());

            if (pixelHeight * pixelWidth < 300000000)
            {
                WriteDirectly(target, objs, pixelWidth, pixelHeight);
            }
            else
            {
                
            }
        }

        void WriteDirectly(string target, List<Item> objs,int width, int height)
        {
            int processors = Environment.ProcessorCount;

            float currentDpi = SettingManager.GetDpi();
            Image<Bgr, Byte> imageCV = new Image<Bgr, byte>(width, height);
            Mat mat = imageCV.Mat;
            mat.SetTo(new MCvScalar(255, 255, 255));

            Size zero = new Size(0, 0);

            foreach (Item slice in objs)
            {
                Mat current = new Mat(slice.ImagePath, LoadImageType.Color);
                Mat shrink = new Mat();
                float ratex = currentDpi / slice.Density_x;
                float ratey = currentDpi / slice.Density_y;
                CvInvoke.Resize(current, shrink, zero, ratex, ratey, Inter.Nearest);

                int x = (int)(currentDpi * slice.Left);
                int y = (int)(currentDpi * slice.Top);

                Mat roi = new Mat(mat, new Rectangle(x, y, shrink.Width, shrink.Height));
                shrink.CopyTo(roi);

                roi.Dispose();
                shrink.Dispose();
                current.Dispose();
            }

            Bitmap map = mat.Bitmap;
            map.SetResolution(currentDpi, currentDpi);
            map.Save(target);

            map.Dispose();
            mat.Dispose();
            imageCV.Dispose();
            
        }

        void WriteSegs(string target, List<Item> objs, float totalHeight)
        {

        }
    }
}
