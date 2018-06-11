using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;

using System.IO;
using phothoflow.setting;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using phothoflow.filemanager;


namespace phothoflow.location
{
    public class Item
    {
        public float Top;
        public float Left;

        public float RealWidth;
        public float RealHeight;

        public float Width;
        public float Height;

        public float Density_x;
        public float Density_y;

        public bool Rotated;

        public string OriginPath;

        public string ImagePath { get; set; }
        public string Name { get; set; }

        public BitmapSource Preview { get; set; }

        public bool ThumbnailCallback()
        {
            return false;
        }

        public void Settle(string thumName, int pixelWidth, int pixelHeight, int dpiX, int dpiY)
        {
            Density_x = dpiX;
            Density_y = dpiY;
            RealWidth = (float)pixelWidth / dpiX;
            RealHeight = (float)pixelHeight / dpiY;
            Height = RealHeight + SettingManager.GetMargin() * 2;
            Width = RealWidth + SettingManager.GetMargin() * 2;
            
        }

        public Item(string name)
        {
            string[] parts = name.Split('$');
            OriginPath = parts[5];
            Settle(parts[0],int.Parse(parts[1]),int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]));

            ImagePath = System.AppDomain.CurrentDomain.BaseDirectory + "thumb\\" + parts[0] + "$" + parts[1] + "$" + parts[2] + "$" + parts[3]+"$" + parts[4] + ".jpg";
            Preview = new BitmapImage(new Uri(ImagePath));
            Name = OriginPath.Substring(OriginPath.LastIndexOf("\\") + 1);

        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            BitmapImage bImage = new BitmapImage();
            bImage.BeginInit();
            bImage.StreamSource = new MemoryStream(ms.ToArray());
            bImage.EndInit();
            ms.Dispose();
            return bImage;
        }


        

        BitmapImage BuildThumb(Bitmap origin)
        {
            int width = origin.Width / 10;
            int height = origin.Height / 10;

            Image thumBitmap = origin.GetThumbnailImage(width, height, () => { return false; }, IntPtr.Zero);
            BitmapImage img = BitmapToBitmapImage(new Bitmap(thumBitmap));
            thumBitmap.Dispose();
            
            return img;
        }

        public bool IsOverlap(Item r2)
        {
            if ((Top + Height > r2.Top + 0.0000001f) && (r2.Top + r2.Height > Top + 0.0000001f)
                     && (Left + Width > r2.Left + 0.0000001f) && (r2.Left + r2.Width > Left + 0.0000001f))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Contains(float x, float y)
        {
            if (x >= Left && x < Left + Width & y >= Top && y < Top + Height)
                return true;
            else
                return false;
        }

    }
}
