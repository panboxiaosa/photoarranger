#include "ThumbManager.h"
#include "md5.h"
#include "stdafx.h"

ThumbManager::ThumbManager()
{

}


ThumbManager::~ThumbManager()
{
}


std::string ThumbManager::load(std::wstring path, string tag){

	string profile = getProfile(path, tag);

	if (cache.find(profile) != cache.end()) {
		return cache[profile];
	}
	else 
	{
		TIFF *tiff = TIFFOpenW(path.c_str(), "r");
		TIFFClose(tiff);
		Mat full = imread(StringCoder::WString2String(), IMREAD_REDUCED_COLOR_8);
		int c = full.channels();
		string thumb = createThumb(full);
		cache[profile] = thumb;
		return thumb;
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


std::string ThumbManager::createThumb(cv::Mat mat)
{
	return "";
}