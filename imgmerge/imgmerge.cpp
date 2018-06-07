// imgmerge.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "ThumbManager.h"
#include "StoreManager.h"
#include "Messager.h"
#include "StringCoder.h"

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



ThumbManager thumb;

StoreManager store;

Messager messager;

string buildTimeStr(WIN32_FIND_DATA filedata) {
	stringstream ss;
	ss << filedata.ftCreationTime.dwHighDateTime
		<< filedata.ftCreationTime.dwLowDateTime
		<< filedata.ftLastWriteTime.dwHighDateTime
		<< filedata.ftLastWriteTime.dwLowDateTime;
	return ss.str();
}

/*----------------------------
* 功能 : 递归遍历文件夹，找到其中包含的所有文件
*----------------------------
* 函数 : find
* 访问 : public
*
* 参数 : lpPath [in]      需遍历的文件夹目录
* 参数 : fileList [in]    以文件名称的形式存储遍历后的文件
*/
void find(TCHAR* lpPath, std::vector<pair<wstring, string> > &fileList)
{
	TCHAR szFind[MAX_PATH];
	WIN32_FIND_DATA FindFileData;

	_tcscpy_s(szFind, lpPath);
	_tcscat_s(szFind, _T("\\*.*"));

	HANDLE hFind = ::FindFirstFile(szFind, &FindFileData);
	if (INVALID_HANDLE_VALUE == hFind)    return;

	while (true)
	{
		if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			if (FindFileData.cFileName[0] != '.')
			{
				TCHAR szFile[MAX_PATH];
				_tcscpy_s(szFile, lpPath);
				_tcscat_s(szFile, (TCHAR*)(FindFileData.cFileName));

				find(szFile, fileList);
			}
		}
		else
		{
			TCHAR szTarget[MAX_PATH];
			_tcscpy_s(szTarget, lpPath);
			if (lpPath[_tcslen(lpPath) - 1] != _T('\\'))
			{
				_tcscat_s(szTarget, _T("\\"));
			}
			_tcscat_s(szTarget, FindFileData.cFileName);
			fileList.push_back(pair<wstring, string>(wstring(szTarget), buildTimeStr(FindFileData)));
		}
		if (!FindNextFile(hFind, &FindFileData))    break;
	}
	FindClose(hFind);
}

bool endsWith(wstring obj, const WCHAR* suf) {
	wstring tar = suf;
	transform(tar.begin(), tar.end(), tar.begin(), tolower);
	return obj.compare(obj.size() - tar.size(), tar.size(), tar) == 0;
}

void loadEntry(TCHAR* filename) {

	vector<pair<wstring, string>> files;
	find(filename, files);
	for (pair<wstring, string> item : files) {
		wstring pathName = item.first;
		if (endsWith(pathName, _T(".jpg")) 
			|| endsWith(pathName, _T(".jpeg")) 
			|| endsWith(pathName, _T(".png")) 
			|| endsWith(pathName, _T(".tif"))) {
			thumb.load(pathName, item.second);
		}
	}
	cout << endl;
}

void mergeEntry(string filename) {

}

int _tmain(int argc, _TCHAR* argv[])
{
	if (argc < 3)
		return 0;
	
	
	if (_tcscmp(argv[1], _T("-l")) == 0) {
		loadEntry(argv[2]);
	}
	else if (_tcscmp(argv[1], _T("-m")) == 0) 
	{
		mergeEntry(StringCoder::TCHAR2STRING(argv[2]));
	}


	return 0;
}

