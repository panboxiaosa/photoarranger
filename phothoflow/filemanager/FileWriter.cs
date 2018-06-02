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
using BitMiracle.LibTiff.Classic;
using System.Threading;
using System.IO.MemoryMappedFiles;  

namespace phothoflow.filemanager
{
    class FileWriter
    {
        const int PART_LEN = 160000000;

        public void Write(string target, List<Item> objs, float height)
        {
            new Thread(() => {
                int pixelWidth = (int)(SettingManager.GetWidth() * SettingManager.GetDpi());
                int pixelHeight = (int)(height * SettingManager.GetDpi());

                if (pixelHeight * pixelWidth < PART_LEN)
                {
                    WriteDirectly(target, objs, pixelWidth, pixelHeight);
                    System.Windows.MessageBox.Show("保存成功");
                }
                else
                {
                    WriteSegs(target, objs, pixelWidth, pixelHeight);
                    System.Windows.MessageBox.Show("保存成功");
                }
            }).Start();
            
        }

        void WriteDirectly(string target, List<Item> objs,int width, int height)
        {
            
            float currentDpi = SettingManager.GetDpi();
            int marginOffset = (int)(currentDpi * SettingManager.GetMargin());
            Mat mat = new Mat(height, width, DepthType.Cv8U, 3);
            mat.SetTo(new MCvScalar(255, 255, 255));

            foreach (Item slice in objs)
            {
                Mat shrink = LoadRequired(slice);
                int x = (int)(currentDpi * slice.Left) + marginOffset;
                int y = (int)(currentDpi * slice.Top) + marginOffset;

                Mat roi = new Mat(mat, new Rectangle(x, y, shrink.Width, shrink.Height));
                shrink.CopyTo(roi);

                roi.Dispose();
                shrink.Dispose();
            }

            Bitmap map = mat.Bitmap;
            map.SetResolution(currentDpi, currentDpi);
            map.Save(target);

            map.Dispose();
            mat.Dispose();
            
        }

        Mat LoadRequired(Item slice)
        {
            Size zero = new Size(0, 0);
            float currentDpi = SettingManager.GetDpi();
            Mat current = ImageReader.readAsMat(slice.ImagePath);
            Mat shrink = new Mat();
            float ratex = currentDpi / slice.Density_x;
            float ratey = currentDpi / slice.Density_y;
            CvInvoke.Resize(current, shrink, zero, ratex, ratey, Inter.Nearest);
            current.Bitmap.Dispose();
            current.Dispose();
            return shrink;
        }

        bool InsidePart(int height, int offset, Item obj)
        {
            int top = (int)(obj.Top * SettingManager.GetDpi());
            int bottom = (int)((obj.Top + obj.Height) * SettingManager.GetDpi());
            if (top < offset + height && bottom > offset)
                return true;
            else
                return false;
        }


        void WritePart(Tiff tiff, List<Item> objs, short index, int eachHeight, int pixelWidth)
        {
            Mat mat = AsMat(objs, index, eachHeight, pixelWidth);

            //tiff.SetDirectory(index);
            //tiff.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
            //tiff.SetField(TiffTag.IMAGEWIDTH, pixelWidth);
            //tiff.SetField(TiffTag.IMAGELENGTH, eachHeight);
            //tiff.SetField(TiffTag.SAMPLESPERPIXEL, 3);
            //tiff.SetField(TiffTag.BITSPERSAMPLE, 8);
            //tiff.SetField(TiffTag.COMPRESSION, Compression.NONE);
            //tiff.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
            //tiff.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

            //byte[] temp = new byte[pixelWidth * 3];
            //for (int i = 0; i < eachHeight; i++)
            //{
                

            //    tiff.WriteScanline(temp, i);
            //}

            //tiff.WriteDirectory();
            //mat.Dispose();

        }

        Mat AsMat(List<Item> objs, short index, int eachHeight, int pixelWidth)
        {
            Mat mat = new Mat(eachHeight, pixelWidth, DepthType.Cv8U, 3);
            mat.SetTo(new MCvScalar(255, 255, 255));
            float currentDpi = SettingManager.GetDpi();
            float margin = SettingManager.GetMargin();
            int marginOffset = (int)(currentDpi * SettingManager.GetMargin());

            int yOffset = eachHeight * index;

            foreach (Item slice in objs)
            {
                if (InsidePart(eachHeight, yOffset, slice))
                {
                    Mat shrink = LoadRequired(slice);

                    int x = (int)(currentDpi * slice.Left) + marginOffset;
                    int y = (int)(currentDpi * slice.Top) + marginOffset - yOffset;

                    Rectangle src = new Rectangle(0, 0, shrink.Width, shrink.Height);
                    Rectangle dst = new Rectangle(x, y, shrink.Width, shrink.Height);
                    if (y < 0)
                    {
                        dst.Y -= y;
                        dst.Height += y;

                        src.Y -= y;
                        src.Height += y;
                    }
                    if (y + dst.Height > eachHeight)
                    {
                        int over = y + dst.Height - eachHeight;
                        dst.Height -= over;
                        src.Height -= over;
                    }

                    Mat roiDst = new Mat(mat, dst);
                    Mat roiSrc = new Mat(shrink, src);
                    roiSrc.CopyTo(roiDst);

                    roiDst.Dispose();
                    roiSrc.Dispose();
                    shrink.Dispose();
                }
            }
            return mat;
        }

        void WritePic(string target, List<Item> objs, short index, int eachHeight, int pixelWidth)
        {
            MemoryMappedFile mmf = MemoryMappedFile.CreateNew("store", eachHeight * pixelWidth * 3);
            System.IO.MemoryMappedFiles.MemoryMappedViewStream stream;
            
            Mat one = AsMat(objs, index, eachHeight, pixelWidth);
            one.Bitmap.Save(target);
            one.Bitmap.Dispose();
            one.Dispose();
        }

        void WriteSegs(string target, List<Item> objs, int pixelWidth, int pixelHeight)
        {
            
            int eachHeight = (PART_LEN / pixelWidth) + 1;
            short segs = (short)(pixelHeight / eachHeight);
            int restHeight = pixelHeight - eachHeight * segs;

            //Tiff tiff = Tiff.Open(target, "w");

            for (short i = 0; i < segs; i++)
            {
                //WritePart(tiff, objs, i,eachHeight, pixelWidth);
                string name = target.Substring(0,target.Length - 4) + i + ".tif";
                WritePic(name, objs, i, eachHeight, pixelWidth);
            }

            if (restHeight > 0)
            {
                string name = target.Substring(0, target.Length - 4) + segs + ".tif";
                WritePic(name, objs, segs, eachHeight, pixelWidth);
                //WritePart(tiff, objs, segs, restHeight, pixelWidth);
            }
            //tiff.Close();
        }
    }
}
