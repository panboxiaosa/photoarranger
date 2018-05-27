using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.IO;


namespace phothoflow.location
{
    class Item
    {
        public float Top;
        public float Left;

        public float Width;
        public float Height;

        public float Density_x;
        public float Density_y;

        public string ImagePath { get; set; }
        public string Name { get; set; }

        public BitmapImage Preview { get; set; }

        public bool ThumbnailCallback()
        {
            return false;
        }

        public Item(string path)
        {
            ImagePath = path;
            Name = path.Substring(path.LastIndexOf("\\") + 1);
            Preview = InitBitmap(path);
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

        BitmapImage InitBitmap(string path)
        {

            Mat mat = new Mat(path, LoadImageType.Color);

            Bitmap img = mat.Bitmap;

            Density_x = img.HorizontalResolution;
            Density_y = img.VerticalResolution;
            Width = (img.Width / Density_x);
            Height = (img.Height / Density_y);
            BitmapImage thumb = BuildThumb(img);

            mat.Dispose();
            img.Dispose();
            
            return thumb;

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
            if ((Top + Height > r2.Top + 0.001) && (r2.Top + r2.Height > Top + 0.001)
                     && (Left + Width > r2.Left + 0.001) && (r2.Left + r2.Width > Left + 0.001))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
