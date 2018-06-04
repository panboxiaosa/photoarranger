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
            CvInvoke.MixChannels(cmyk, cvted, new int[] { 0, 2, 1, 1, 2, 0 });
            Image<Rgb, byte> wrapper = cvted.ToImage<Rgb, byte>();
            Image<Rgb, byte> reved = wrapper.Convert<Byte>((Byte b) => { return (Byte)(255 - b); });
            return reved.Mat;
        }

        private static Mat TiffToMat(string asTiffFile)
        {
            Tiff tif = Tiff.Open(asTiffFile, "r");
            if (tif == null)
            {
                return null;
            }
            FieldValue[] value = tif.GetField(TiffTag.IMAGEWIDTH);
            int width = value[0].ToInt();

            value = tif.GetField(TiffTag.IMAGELENGTH);
            int height = value[0].ToInt();
            value = tif.GetField(TiffTag.SAMPLESPERPIXEL);
            Mat mat = new Mat(height, width, DepthType.Cv8U, value[0].ToShort());
            value = tif.GetField(TiffTag.PHOTOMETRIC);

            byte[] buf = new byte[mat.Step];
            unsafe
            {
                byte* ptr = (byte*)mat.DataPointer.ToPointer();
                for (int i = 0; i < height; i++)
                {
                    tif.ReadScanline(buf, i);
                    Marshal.Copy(buf, 0, new IntPtr(ptr), mat.Step);
                    ptr += mat.Step;
                }
            }

            tif.Close();
            tif.Dispose();

            return CmykToRgb(mat);
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
