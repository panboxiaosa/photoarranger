#include "ThumbManager.h"
#include "md5.h"
#include "stdafx.h"
#include "StringCoder.h"
#include "BufStorage.h"
#include "FileUtil.h"
#include "ImageLoader.h"
#include "Messager.h"


ThumbManager::ThumbManager()
{
	initCache();
}

ThumbManager::~ThumbManager()
{
}

void ThumbManager::load(std::wstring path, string tag){

	string profile = getProfile(path, tag);
	if (cache.find(profile) != cache.end()) {
		ImageLoader loaded = cache[profile];
		Messager::sendStr(loaded.toString() + "$" + StringCoder::WString2String(path));
	}
	else 
	{
		ImageLoader img(path);
		img.profileStr = profile;
		wstring thumbPath = createThumb(img);
		cache[profile] = img;
		Messager::sendStr(img.toString() + "#" + StringCoder::WString2String(path));
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
		std::stringstream oss;
		for (int i = 0; i < 16; ++i)
		{
			oss << std::hex << std::setw(2) << std::setfill('0') << static_cast<unsigned int>(digest[i]);
		}

		return oss.str();
	}
	return "";
}


std::wstring ThumbManager::createThumb(ImageLoader img)
{
	Mat full(img.height, img.width, CV_8UC(img.channel), BufStorage::getStorage());

	Mat thumb;
	resize(full, thumb, Size(0, 0), 0.1, 0.1, INTER_NEAREST);

	wstring path = getThumbDir();
	
	path += _T("\\") + StringCoder::String2WString(img.toString()) + _T(".jpg");

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

	for (string item : files) {
		
		wstring thumbPath = thumDir + _T("\\") + StringCoder::String2WString(item);

		ImageLoader added;
		string name = item.substr(0, item.length() - 4);
		vector<string> parts = StringCoder::split(name);
		added.profileStr = parts[0];
		added.width = stoi(parts[1]);
		added.height = stoi(parts[2]);
		added.dpiX = stoi(parts[3]);
		added.dpiY = stoi(parts[4]);

		cache[parts[0]] = added;

	}
}
