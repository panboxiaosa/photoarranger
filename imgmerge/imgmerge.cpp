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


void loadToUi(vector<pair<wstring, string>> files) {
	ThumbManager thumb;
	Messager::sendStr("start");
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
	Messager::sendStr("finish");
}


void loadEntry(wstring filename) {
	vector<pair<wstring, string>> files;
	FileUtil::find(filename.c_str(), files);
	loadToUi(files);
}


void mergeEntry(wstring filename, wstring target) {
	StoreManager store;
	store.load(filename);
	store.build(target);
	
}

void fileEntry(wstring filename) {
	StoreManager store;
	store.load(filename);
	vector<wstring> paths = store.getOriginFiles();
	vector<pair<wstring, string>> all = FileUtil::supply(paths);
	loadToUi(all);
}

void appendEntry(wstring filename) {
	string timeStr = FileUtil::timeProfile(filename);
	ThumbManager thumb;
	thumb.load(filename, timeStr);
}

extern "C" {
	void doNothing(const char* one, const char* two, char * three) {
		//cout<< "warning " << one << ":" << two << ":" << three << endl;
	}

	void doErr(const char* one, const char* two, char * three) {
		//cout<<"error " << one << ":" << two << ":" << three << endl;
	}
}

void testEntry(wstring filename, wstring target) {

	TIFF *tiff = TIFFOpenW(filename.c_str(), "r");

	uint32 count = 0;
	void* p;

	TIFFGetField(tiff, TIFFTAG_ICCPROFILE, &count, &p);
	byte* buf = new byte[count];
	memcpy(buf, p, count);

	TIFFClose(tiff);

	ofstream ost(target.c_str(), ios::binary);
	ost.write((char*)buf, count);
	
	delete[] buf;
	
}


int _tmain(int argc, wchar_t* argv[])
{

	if (argc < 3)
		return 0;
	TIFFSetErrorHandler(doErr);
	TIFFSetWarningHandler(doNothing);
	Gdiplus::GdiplusStartupInput gdiplusStartupInput;
	ULONG_PTR gdiplusToken;
	Gdiplus::GdiplusStartup(&gdiplusToken, &gdiplusStartupInput, NULL);

	wcout.imbue(locale(locale(), "", LC_CTYPE));
	if (_tcscmp(argv[1], L"-l") == 0) {
		loadEntry(argv[2]);
	}
	else if (_tcscmp(argv[1], L"-m") == 0)
	{
		mergeEntry(argv[2], argv[3]);
	}
	else if (_tcscmp(argv[1], L"-f") == 0) {
		fileEntry(argv[2]);
	}
	else if (_tcscmp(argv[1], L"-a") == 0) {
		appendEntry(argv[2]);
	}
	else if (_tcscmp(argv[1], L"-t") == 0){
		testEntry(argv[2], argv[3]);
	}
	Gdiplus::GdiplusShutdown(gdiplusToken);

	BufStorage::releaseStorage();
	return 0;
}

