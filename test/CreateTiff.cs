using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace SaveMultipageTiffArticle
{
    public class CreateTiff
    {
        public bool CreateTiffFile(Image[] img, string dstFile)
        {
            return GetTiffFile(img, dstFile);
        }

        public bool AddTiffFile(string srcFile, string dstFile)
        {
            Image i1 = Image.FromFile(srcFile);

            Image loadImage = new Bitmap(i1);
            //FileStream fr = File.Open(dstFile, FileMode.Open, FileAccess.ReadWrite);
            Image origionalFile = Image.FromFile(dstFile);
            int PageNumber = getPageNumber(origionalFile);
            Image[] img = new Image[PageNumber + 1];
            for (int i = 0; i < PageNumber; i++)
            {
                origionalFile.SelectActiveFrame(FrameDimension.Page, i);
                img[i] = new Bitmap(origionalFile);
            }
            img[PageNumber] = loadImage;
            origionalFile.Dispose();
            i1.Dispose();



            return GetTiffFile(img, dstFile);
        }
        private bool GetTiffFile(Image[] img, string dstFile)
        {
            try
            {
                if (img == null) return false;
                if (img.Length < 2) return false;//如果只有一个文件，直接存成TIFF就好了，没有必要在这里处理
                ImageCodecInfo codecInfo = ImageCodecInfo.GetImageEncoders()[3];
                if (codecInfo.FormatDescription != "TIFF") return false;

                for (int i = 0; i < img.Length; i++)
                {
                    if (img[i] == null)
                        break;
                    img[i] = (Image)ConvertToBitonal((Bitmap)img[i]);

                }
                if (img.Length < 2) return false;

                Encoder saveEncoder = Encoder.SaveFlag;
                Encoder compressionEncoder = Encoder.Compression;
                EncoderParameter SaveEncodeParam = new EncoderParameter(saveEncoder, (long)EncoderValue.MultiFrame);
                EncoderParameter CompressionEncodeParam = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionCCITT4);
                EncoderParameters EncoderParams = new EncoderParameters(2);
                EncoderParams.Param[0] = CompressionEncodeParam;
                EncoderParams.Param[1] = SaveEncodeParam;

                if (File.Exists(dstFile)) File.Delete(dstFile);
                img[0].Save(dstFile, codecInfo, EncoderParams);
                for (int i = 1; i < img.Length; i++)
                {
                    SaveEncodeParam = new EncoderParameter(saveEncoder, (long)EncoderValue.FrameDimensionPage);
                    CompressionEncodeParam = new EncoderParameter(compressionEncoder, (long)EncoderValue.CompressionCCITT4);
                    EncoderParams.Param[0] = CompressionEncodeParam;
                    EncoderParams.Param[1] = SaveEncodeParam;
                    img[0].SaveAdd(img[i], EncoderParams);

                }

                SaveEncodeParam = new EncoderParameter(saveEncoder, (long)EncoderValue.Flush);
                EncoderParams.Param[0] = SaveEncodeParam;
                img[0].SaveAdd(EncoderParams);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private Bitmap ConvertToBitonal(Bitmap original)
        {
            Bitmap source = null;

            // If original bitmap is not already in 32 BPP, ARGB format, then convert
            if (original.PixelFormat != PixelFormat.Format32bppArgb)
            {
                source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                using (Graphics g = Graphics.FromImage(source))
                {
                    g.DrawImageUnscaled(original, 0, 0);
                }
            }
            else
            {
                source = original;
            }

            // Lock source bitmap in memory
            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            // Copy image data to binary array
            int imageSize = sourceData.Stride * sourceData.Height;
            byte[] sourceBuffer = new byte[imageSize];
            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

            // Unlock source bitmap
            source.UnlockBits(sourceData);

            // Create destination bitmap
            Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);

            // Lock destination bitmap in memory
            BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            // Create destination buffer
            imageSize = destinationData.Stride * destinationData.Height;
            byte[] destinationBuffer = new byte[imageSize];

            int sourceIndex = 0;
            int destinationIndex = 0;
            int pixelTotal = 0;
            byte destinationValue = 0;
            int pixelValue = 128;
            int height = source.Height;
            int width = source.Width;
            int threshold = 500;

            // Iterate lines
            for (int y = 0; y < height; y++)
            {
                sourceIndex = y * sourceData.Stride;
                destinationIndex = y * destinationData.Stride;
                destinationValue = 0;
                pixelValue = 128;

                // Iterate pixels
                for (int x = 0; x < width; x++)
                {
                    // Compute pixel brightness (i.e. total of Red, Green, and Blue values)
                    pixelTotal = sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2] + sourceBuffer[sourceIndex + 3];
                    if (pixelTotal > threshold)
                    {
                        destinationValue += (byte)pixelValue;
                    }
                    if (pixelValue == 1)
                    {
                        destinationBuffer[destinationIndex] = destinationValue;
                        destinationIndex++;
                        destinationValue = 0;
                        pixelValue = 128;
                    }
                    else
                    {
                        pixelValue >>= 1;
                    }
                    sourceIndex += 4;
                }
                if (pixelValue != 128)
                {
                    destinationBuffer[destinationIndex] = destinationValue;
                }
            }

            // Copy binary image data to destination bitmap
            Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

            // Unlock destination bitmap
            destination.UnlockBits(destinationData);

            // Return
            return destination;
        }

        private int getPageNumber(Image img)
        {

            Guid objGuid = img.FrameDimensionsList[0];
            FrameDimension objDimension = new FrameDimension(objGuid);

            //Gets the total number of frames in the .tiff file
            int PageNumber = img.GetFrameCount(objDimension);

            return PageNumber;
        }
    }
}
