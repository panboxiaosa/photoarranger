#include "ImageLoader.h"
#include "StringCoder.h"
#include "BufStorage.h"
#include "jpeglib.h"
#include <direct.h>  

ImageLoader::ImageLoader() {

}


ImageLoader::ImageLoader(wstring path)
{
	if (StringCoder::endsWith(path, _T(".tif")) || StringCoder::endsWith(path, _T(".TIF"))) {
		loadTif(path);
	}
	else if (StringCoder::endsWith(path, _T(".jpg"))
		|| StringCoder::endsWith(path, _T(".JPG"))
		|| StringCoder::endsWith(path, _T(".JPEG"))
		|| StringCoder::endsWith(path, _T(".jpeg"))){

		loadJpg(path);
	}
}

ImageLoader::~ImageLoader()
{
}

void setDpi(TIFF* tiff, ushort& ref) {
	float xres;
	uint16 res_unit;
	//解析度单位：如是英寸，厘米  
	TIFFGetFieldDefaulted(tiff, TIFFTAG_RESOLUTIONUNIT, &res_unit);

	if (TIFFGetField(tiff, TIFFTAG_XRESOLUTION, &xres) == 0)
	{
		ref = 0;
	}
	else
	{
		if (res_unit == 2)    //英寸  
		{
			ref = round(xres);
		}
		else if (res_unit == 3)    //厘米  
		{
			ref = round(xres * 2.54);
		}
		else
		{
			ref = 0;
		}
	}
}


void ImageLoader::loadJpg(wstring path)
{
	Gdiplus::Bitmap map(path.c_str(), true);

	HBITMAP _hBmp;
	BITMAP bmp;
	map.GetHBITMAP(NULL, &_hBmp);

	GetObject(_hBmp, sizeof(BITMAP), &bmp);
	channel = bmp.bmBitsPixel == 1 ? 1 : bmp.bmBitsPixel / 8;
	height = bmp.bmHeight;
	width = bmp.bmWidth;
	dpiX = round(map.GetHorizontalResolution());
	dpiY = round(map.GetVerticalResolution());

	GetBitmapBits(_hBmp, channel * height * width, BufStorage::getStorage());
	
	byte test[50];
	memcpy(test, BufStorage::getStorage(), 50);

	colorSpace = RGBASPACE;

	
}


void ImageLoader::loadTif(wstring path){
	TIFF *tiff = TIFFOpenW(path.c_str(), "r");

	TIFFGetField(tiff, TIFFTAG_IMAGEWIDTH, &width);
	TIFFGetField(tiff, TIFFTAG_IMAGELENGTH, &height);
	TIFFGetField(tiff, TIFFTAG_SAMPLESPERPIXEL, &channel);
	ushort metric;
	TIFFGetField(tiff, TIFFTAG_PHOTOMETRIC, &metric);
	setDpi(tiff, dpiX);
	setDpi(tiff, dpiY);

	if (metric == PHOTOMETRIC_RGB) {
		colorSpace = RGBSPACE;
	}
	else if (metric == PHOTOMETRIC_SEPARATED) {
		if (channel == 4) {
			colorSpace = CMYKSPACE;
		}
		else if (channel == 5) {
			colorSpace = CMYKASPACE;
		}
	}
	else {
		colorSpace == UNHANDLABLESPACE;
	}
	int step = width * channel;
	byte* stableBuf = BufStorage::getStorage();
	for (int i = 0; i < height; i++) {
		TIFFReadScanline(tiff, stableBuf, i);
		stableBuf += step;
	}
	
	TIFFClose(tiff);
}


string ImageLoader::toString() {
	stringstream ss;
	ss << profileStr<<"$" << width<<"$"  << height<<"$" << dpiX<<"$"<< dpiY;
	return ss.str();
}

wstring ImageLoader::toWstring() {
	wstringstream ss;
	ss << StringCoder::String2WString(profileStr) << L"$" << width << L"$" << height << L"$" << dpiX << L"$" << dpiY;
	return ss.str();
}