using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Stitching;
using Emgu.Util;
using Emgu.CV.Util;
using SaveMultipageTiffArticle;
using System.Drawing;
using BitMiracle.LibTiff;
using BitMiracle.LibTiff.Classic;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;

namespace test
{
    class Program
    {
        
        static void Jpg2TiffByLibTiffAndCV(Tiff tiff, short pageIndex,string imgPath)
        {
            if(tiff== null)
                return;

            Mat firstImg = new Mat(imgPath, LoadImageType.Color);
            int firstWidth = firstImg.Cols;
            int firstHeight = firstImg.Rows;

            tiff.SetDirectory(pageIndex);
            tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
            tiff.SetField(TiffTag.IMAGEWIDTH, firstWidth);
            tiff.SetField(TiffTag.IMAGELENGTH, firstHeight);
            tiff.SetField(TiffTag.SAMPLESPERPIXEL, 3);
            tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
            tiff.SetField(TiffTag.COMPRESSION, Compression.NONE);
            tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);


    for (int i = 0; i < firstHeight; i++)
    {
        tiff.WriteScanline(firstImg.GetData(0), i);
    }

    tiff.WriteDirectory();
}

    static void Jpgs2TiffByLibTiffAndCV(List<string> vecImgPath, string tifPath)
    {
    Tiff tiff = Tiff.Open(tifPath, "w");
    if (tiff == null)
        return;
    short pageIndex = 0;
    foreach (string path in vecImgPath)
    {
        Jpg2TiffByLibTiffAndCV(tiff, pageIndex++, path);
    }   

    tiff.Close();
    }

    static Mat CmykToRgb(Mat cmyk)
    {
        Mat cvted = new Mat(cmyk.Rows, cmyk.Cols, DepthType.Cv8U, 3);
        CvInvoke.MixChannels(cmyk, cvted, new int[] { 0, 0, 1, 1, 2, 2 });
        CvInvoke.CvtColor(cvted, cvted, ColorConversion.YCrCb2Rgb, 3);
        return cvted;
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
        for (int i = 0; i < height; i++)
        {
            byte[] data = mat.GetData(new int[]{i});
            tif.ReadScanline(data, i);
        }

        tif.Close();
        tif.Dispose();

        return CmykToRgb(mat);
    }

        static void Main(string[] args)
        {

            Mat mat = TiffToMat("E:\\temp\\a.tif");
            Bitmap map = mat.Bitmap;
            map.Save("E:\\temp\\b.jpg");
            map.Dispose();
            //List<string> all = new List<string>() { "D:\\part\\a.tif", "D:\\part\\b.tif" };

            //Jpgs2TiffByLibTiffAndCV(all, "output.tif");
        }
    }
}
