// imgmerge.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

using namespace cv;
using namespace std;

void Jpg2TiffByLibTiffAndCV(TIFF *tiff, int pageIndex, std::string imgPath)
{
	if (!tiff)
		return;

	cv::Mat firstImg = cv::imread(imgPath);
	int c = firstImg.channels();
	cv::cvtColor(firstImg, firstImg, CV_BGR2RGB);
	int firstWidth = firstImg.cols;
	int firstHeight = firstImg.rows;

	TIFFSetDirectory(tiff, pageIndex);

	TIFFSetField(tiff, TIFFTAG_SUBFILETYPE, FILETYPE_PAGE);
	TIFFSetField(tiff, TIFFTAG_IMAGEWIDTH, firstWidth);
	TIFFSetField(tiff, TIFFTAG_IMAGELENGTH, firstHeight);
	TIFFSetField(tiff, TIFFTAG_SAMPLESPERPIXEL, 3);
	TIFFSetField(tiff, TIFFTAG_BITSPERSAMPLE, 8);
	TIFFSetField(tiff, TIFFTAG_COMPRESSION, COMPRESSION_LZW);
	TIFFSetField(tiff, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_RGB);
	TIFFSetField(tiff, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);

	uchar *firstImgData = firstImg.data;
	for (int i = 0; i < firstHeight; ++i)
	{
		TIFFWriteScanline(tiff, &firstImgData[i * firstWidth * 3], i);
	}

	TIFFWriteDirectory(tiff);
}

void Jpgs2TiffByLibTiffAndCV(std::vector<std::string> vecImgPath, std::string tifPath)
{
	TIFF *tiff = TIFFOpen(tifPath.c_str(), "w");
	if (!tiff)
		return;
	int pageIndex = 0;
	for (auto vecIter = vecImgPath.begin(); vecIter != vecImgPath.end(); ++vecIter)
	{
		Jpg2TiffByLibTiffAndCV(tiff, pageIndex++, *vecIter);
	}

	TIFFClose(tiff);
}

void write_huge() {

	TIFF *tiff = TIFFOpen("test.tif", "w");

	TIFFSetDirectory(tiff, 0);

#define WIDTH 12000
#define HEIGHT 80000

	TIFFSetField(tiff, TIFFTAG_SUBFILETYPE, FILETYPE_PAGE);
	TIFFSetField(tiff, TIFFTAG_IMAGEWIDTH, WIDTH);
	TIFFSetField(tiff, TIFFTAG_IMAGELENGTH, HEIGHT);
	TIFFSetField(tiff, TIFFTAG_SAMPLESPERPIXEL, 3);
	TIFFSetField(tiff, TIFFTAG_BITSPERSAMPLE, 8);
	TIFFSetField(tiff, TIFFTAG_COMPRESSION, COMPRESSION_LZW);
	TIFFSetField(tiff, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_RGB);
	TIFFSetField(tiff, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);


	uchar data[WIDTH * 3];
	memset(data, 0, WIDTH * 3 / 2);
	memset(data + WIDTH * 3 / 2, 180, WIDTH * 3 / 2);
	for (int i = 0; i < HEIGHT; i++)
	{
		TIFFWriteScanline(tiff, data, i);
	}

	
	TIFFClose(tiff);
}

int _tmain(int argc, _TCHAR* argv[])
{
	//vector<string> src;
	//src.push_back("D:\\a.tif");
	//src.push_back("D:\\b.tif");
	//Jpgs2TiffByLibTiffAndCV(src, "output.tif");
	Mat* test = new Mat(200000, 10000, CV_8UC3, Scalar(255, 255, 255));
	vector<int> compression_params;

	imwrite("test.tif", *test, compression_params);
	//write_huge();

	return 0;
}

