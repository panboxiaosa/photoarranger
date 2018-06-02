using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.IO;
using phothoflow.setting;
using BitMiracle.LibTiff.Classic;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing;

namespace phothoflow.filemanager
{
    public class ImageReader
    {

        static Mat CmykToRgb(Mat cmyk)
        {
            Mat cvted = new Mat(cmyk.Rows, cmyk.Cols, DepthType.Cv8U, 3);
            CvInvoke.MixChannels(cmyk, cvted, new int[] { 0, 0, 1, 1, 2, 2 });
            CvInvoke.CvtColor(cvted, cvted, ColorConversion.YCrCb2Rgb, 3);
            return cvted;
        }

        private static Mat TiffToMat(string asTiffFile)
        {
            //Tiff tif = Tiff.Open(asTiffFile, "r");
            //if (tif == null)
            //{
            //    return null;
            //}
            //FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
            //int width = value[0].ToInt();

            //value = tif.GetField(TiffTag.IMAGELENGTH);
            //int height = value[0].ToInt();

            //value = tif.GetField(TiffTag.SAMPLESPERPIXEL);

            //Mat mat = new Mat(height, width, DepthType.Cv8U, value[0].ToShort());
            //value = tif.GetField(TiffTag.PHOTOMETRIC);

            //for (int i = 0; i < height; i++)
            //{
            //    byte[] data = mat.GetData(i);
            //    tif.ReadScanline(data, i);
            //}

            //tif.Close();
            //tif.Dispose();

            //return CmykToRgb(mat);

            Bitmap map = TiffToBitmap(asTiffFile);
            Image<Bgr, byte> frame = new Image<Bgr, byte>(map);
            return frame.Mat;
        }

        private static Bitmap TiffToBitmap(string asTiffFile)
        {
            Tiff tif = Tiff.Open(asTiffFile, "r");
            if (tif == null)
            {
                return null;
            }
            // Find the width and height of the image
            FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = tif.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();

            // Read the image into the memory buffer
            int[] raster = new int[height * width];

            if (!tif.ReadRGBAImage(width, height, raster))
            {
                tif.Close();
                tif.Dispose();
                return null;
            }
            tif.Close();
            tif.Dispose();
            // bitmap作成
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpdata = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            byte[] bits = new byte[bmpdata.Stride * bmpdata.Height];

            for (int y = 0; y < bmp.Height; y++)
            {
                int rasterOffset = y * bmp.Width;
                int bitsOffset = (bmp.Height - y - 1) * bmpdata.Stride;

                for (int x = 0; x < bmp.Width; x++)
                {
                    int rgba = raster[rasterOffset++];
                    bits[bitsOffset++] = (byte)((rgba >> 16) & 0xff);
                    bits[bitsOffset++] = (byte)((rgba >> 8) & 0xff);
                    bits[bitsOffset++] = (byte)(rgba & 0xff);
                }
            }
            Marshal.Copy(bits, 0, bmpdata.Scan0, bits.Length);
            bmp.UnlockBits(bmpdata);
            return bmp;
        }

        public static Bitmap readAsBitmap(string path)
        {
            Bitmap img = null;
            try
            {
                Mat mat = new Mat(path, LoadImageType.Color);
                img = mat.Bitmap;
            }
            catch
            {
                img = TiffToBitmap(path);
            }
            return img;
        }

       

        public static Mat readAsMat(string path)
        {
            try
            {
                Mat mat = new Mat(path, LoadImageType.Color);
                return mat;
            }
            catch
            {
                return TiffToMat(path);
            }
        }
    }
}
