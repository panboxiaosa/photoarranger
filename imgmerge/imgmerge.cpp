// imgmerge.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "tiffio.h"
#include <iostream>
#include <windef.h>
#include <wingdi.h>
#include <opencv2/opencv.hpp>
#include <opencv2/highgui/highgui.hpp>
#include <opencv2/imgproc//imgproc.hpp>

using namespace std;

void Jpg2TiffByLibTiffAndCV(TIFF *tiff, int pageIndex, std::string imgPath)
{
	if (!tiff)
		return;

	cv::Mat firstImg = cv::imread(imgPath);
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

#define WIDTH 10000
#define HEIGHT 70000

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
	/*vector<string> src;
	src.push_back("D:\\part\\c.tif");
	src.push_back("D:\\part\\b.tif");
	Jpgs2TiffByLibTiffAndCV(src, "output.tif");*/

	write_huge();

	return 0;
}

