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

void storeToMat(Gdiplus::Bitmap* pbm)

{
	HBITMAP _hBmp;
	BITMAP bmp;
	pbm->GetHBITMAP(NULL, &_hBmp);

	GetObject(_hBmp, sizeof(BITMAP), &bmp);
	int nChannels = bmp.bmBitsPixel == 1 ? 1 : bmp.bmBitsPixel / 8;
	int depth = bmp.bmBitsPixel == 1 ? IPL_DEPTH_1U : IPL_DEPTH_8U;
	GetBitmapBits(_hBmp, bmp.bmHeight*bmp.bmWidth*nChannels, BufStorage::getStorage());

	//Mat v_mat(bmp.bmWidth, bmp.bmHeight, CV_8UC(nChannels), BufStorage::getStorage());
}

void ImageLoader::loadJpg(wstring path)
{
	int64 one = getTickCount();
	Gdiplus::Bitmap map(path.c_str());
	
	dpiX = round(map.GetHorizontalResolution());
	dpiY = round(map.GetVerticalResolution());

	int64 two = getTickCount();

	struct jpeg_decompress_struct cinfo;
	struct jpeg_error_mgr jerr;
	FILE * infile;
	JSAMPARRAY buffer;
	int row_stride;

	//绑定标准错误处理结构  
	cinfo.err = jpeg_std_error(&jerr);
	//初始化JPEG对象  
	jpeg_create_decompress(&cinfo);
	//指定图像文件  
	_wfopen_s(&infile, path.c_str(), _T("rb"));
	jpeg_stdio_src(&cinfo, infile);

	//读取图像信息  
	(void)jpeg_read_header(&cinfo, TRUE);

	//开始解压缩图像  
	(void)jpeg_start_decompress(&cinfo);


	//分配缓冲区空间  
	row_stride = cinfo.output_width * cinfo.output_components;
	height = cinfo.output_height;
	width = cinfo.output_width;
	channel = cinfo.output_components;

	buffer = (*cinfo.mem->alloc_sarray)((j_common_ptr)&cinfo, JPOOL_IMAGE, row_stride, 1);

	byte* stableBuf = BufStorage::getStorage();
	//读取数据  
	while (cinfo.output_scanline < cinfo.output_height)
	{
		(void)jpeg_read_scanlines(&cinfo, buffer, 1);

		memcpy_s(stableBuf, row_stride, buffer[0], row_stride);
		stableBuf += row_stride;
	}
	//结束解压缩操作  
	(void)jpeg_finish_decompress(&cinfo);

	//释放资源  
	jpeg_destroy_decompress(&cinfo);
	fclose(infile);
	int64 three = getTickCount();

	int cmp1 = two - one;
	int cmp2 = three - two;

}



void ImageLoader::loadTif(wstring path){
	TIFF *tiff = TIFFOpenW(path.c_str(), "r");

	TIFFGetField(tiff, TIFFTAG_IMAGEWIDTH, &width);
	TIFFGetField(tiff, TIFFTAG_IMAGELENGTH, &height);
	TIFFGetField(tiff, TIFFTAG_SAMPLESPERPIXEL, &channel);
	setDpi(tiff, dpiX);
	setDpi(tiff, dpiY);

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