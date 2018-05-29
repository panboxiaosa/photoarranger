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

        static void Main(string[] args)
        {
            //Stitcher sticher = new Stitcher(false);
            
            //Mat[] array = new Mat[2];
            //array[0] = new Mat("D:\\part\\a.tif", LoadImageType.Color);
            //array[1] = new Mat("D:\\part\\b.tif", LoadImageType.Color);
            //Image[] ims = new Bitmap[] {array[0].Bitmap, array[1].Bitmap};

            List<string> all = new List<string>(){"D:\\part\\a.tif", "D:\\part\\b.tif"};

            Jpgs2TiffByLibTiffAndCV(all, "output.tif");
        }
    }
}
