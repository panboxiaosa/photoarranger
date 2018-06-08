// imgmerge.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "ThumbManager.h"
#include "StoreManager.h"
#include "Messager.h"
#include "StringCoder.h"
#include "FileUtil.h"
#include "BufStorage.h"

using namespace cv;
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


Messager messager;


void loadEntry(TCHAR* filename) {
	
	ThumbManager thumb;

	vector<pair<wstring, string>> files;
	FileUtil::find(filename, files);
	for (pair<wstring, string> item : files) {
		wstring pathName = item.first;
		if (StringCoder::endsWith(pathName, _T(".jpg")) 
			|| StringCoder::endsWith(pathName, _T(".jpeg")) 
			|| StringCoder::endsWith(pathName, _T(".png")) 
			|| StringCoder::endsWith(pathName, _T(".tif"))
			|| StringCoder::endsWith(pathName, _T(".JPG"))
			|| StringCoder::endsWith(pathName, _T(".JPEG"))
			|| StringCoder::endsWith(pathName, _T(".PNG"))
			|| StringCoder::endsWith(pathName, _T(".TIF"))) {
			thumb.load(pathName, item.second);
		}
	}
}

void mergeEntry(wstring filename) {
	StoreManager store;
	store.load(filename);
	
}

extern "C" {

	void doNothing(const char*, const char*, char *) {
	}
}


int _tmain(int argc, _TCHAR* argv[])
{

	if (argc < 3)
		return 0;
	wcout.imbue(locale(locale(), "", LC_CTYPE));
	if (_tcscmp(argv[1], _T("-l")) == 0) {

		TIFFSetErrorHandler(doNothing);
		TIFFSetWarningHandler(doNothing);
		loadEntry(argv[2]);
	}
	else if (_tcscmp(argv[1], _T("-m")) == 0) 
	{
		mergeEntry(argv[2]);
	}
	BufStorage::releaseStorage();
	return 0;
}

