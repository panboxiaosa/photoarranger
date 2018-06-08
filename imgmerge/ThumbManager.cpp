#include "ThumbManager.h"
#include "md5.h"
#include "stdafx.h"
#include "StringCoder.h"
#include "jpeglib.h"
#include <direct.h>  
#include "BufStorage.h"
#include "FileUtil.h"


ThumbManager::ThumbManager()
{
	initCache();
}

ThumbManager::~ThumbManager()
{
}

std::wstring ThumbManager::load(std::wstring path, string tag){

	string profile = getProfile(path, tag);
	cout << profile << "\t";
	wcout << path << endl;
	if (cache.find(profile.c_str()) != cache.end()) {
		return cache[profile.c_str()];
	}
	else 
	{
		int width, height;
		short channel;
		if (StringCoder::endsWith(path, _T(".tif")) || StringCoder::endsWith(path, _T(".TIF"))) {

			TIFF *tiff = TIFFOpenW(path.c_str(), "r");

			TIFFGetField(tiff, TIFFTAG_IMAGEWIDTH, &width);
			TIFFGetField(tiff, TIFFTAG_IMAGELENGTH, &height);
			TIFFGetField(tiff, TIFFTAG_SAMPLESPERPIXEL, &channel);
			int step = width * channel;
			byte* stableBuf = BufStorage::getStorage();
			for (int i = 0; i < height; i++) {
				TIFFReadScanline(tiff, stableBuf , i);
				stableBuf += step;
			}

			TIFFClose(tiff);
			
		}
		else if (StringCoder::endsWith(path, _T(".jpg")) 
			|| StringCoder::endsWith(path, _T(".JPG"))
			|| StringCoder::endsWith(path, _T(".JPEG"))
			|| StringCoder::endsWith(path, _T(".jpeg"))){

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

		}

		Mat full(height, width, CV_8UC(channel), BufStorage::getStorage());
		
		wstring thumbPath = createThumb(full, profile);
		cache[profile] = thumbPath;
		return thumbPath;
	}
}

std::string ThumbManager::getProfile(std::wstring path, string tag) {

	std::ifstream fin(path.c_str(), std::ifstream::in | std::ifstream::binary);
	if (fin) {
		MD5_CTX context;
		MD5Init(&context);

		fin.seekg(0, fin.end);
		const auto fileLength = fin.tellg();
		fin.seekg(fileLength / 2, fin.beg);

		const int bufferLen = 1024;
		std::unique_ptr<unsigned char[]> buffer{ new unsigned char[bufferLen] {} };
		decltype(fin.gcount()) readCount = 0;
		// 读取文件内容，调用MD5Update()更新MD5值
		fin.read(reinterpret_cast<char*>(buffer.get()), bufferLen);
		readCount = fin.gcount();
		MD5Update(&context, buffer.get(), static_cast<unsigned int>(readCount));
		fin.close();
		char tagBuf[1024];
		strcpy_s(tagBuf, tag.c_str());
		MD5Update(&context, (unsigned char*)tagBuf, tag.length());

		unsigned char digest[16];
		MD5Final(digest, &context);
		std::ostringstream oss;
		for (int i = 0; i < 16; ++i)
		{
			oss << std::hex << std::setw(2) << std::setfill('0') << static_cast<unsigned int>(digest[i]);
		}
		oss << std::ends;

		return move(oss.str());
	}
	return "";
}


std::wstring ThumbManager::createThumb(cv::Mat mat, string name)
{
	Mat thumb;
	resize(mat, thumb, Size(0, 0), 0.1, 0.1, INTER_NEAREST);

	wstring path = getThumbDir();
	path += _T("\\") + StringCoder::String2WString(name) + _T(".jpg");
	vector <int> params;
	if (thumb.channels() > 3) {
		Mat test(thumb.size(), CV_8UC3);
		const int fromTo[6] = { 1, 0, 2, 1, 3, 2 };
		mixChannels(thumb, test, fromTo, 3);
		imwrite(StringCoder::WString2String(path), test);
	}
	else {
		imwrite(StringCoder::WString2String(path), thumb);
	}
	
	return path;
}

std::wstring ThumbManager::getThumbDir()
{
	TCHAR _szPath[MAX_PATH + 1] = { 0 };
	GetModuleFileName(NULL, _szPath, MAX_PATH);
	(_tcsrchr(_szPath, _T('\\')))[1] = 0;//删除文件名，只获得路径 字串


	const WCHAR* thumbdir = _T("thumb");
	_tcscat_s(_szPath, thumbdir);
	if (-1 == _waccess(_szPath, 0)) {
		_wmkdir(_szPath);
	}

	wstring path = _szPath;
	return path;
}

void ThumbManager::initCache()
{
	wstring thumDir = getThumbDir();
	vector<string> files;
	FileUtil::find(thumDir.c_str(), files);
	wstring thumbPath = getThumbDir();
	for (string item : files) {
		string md5 = item.substr(0, item.length() - 4);
		wstring fullPath = thumbPath + _T("\\") + StringCoder::String2WString(item);
		cache[md5] = fullPath;

	}
}
