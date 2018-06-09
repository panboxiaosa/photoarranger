#include "StoreManager.h"
#include "StringCoder.h"
#include "BufStorage.h"
#include "ImageLoader.h"

StoreManager::StoreManager()
{
}


StoreManager::~StoreManager()
{
}


void StoreManager::saveTo(wstring tar) 
{
	std::ofstream fout(tar.c_str(), std::ifstream::out);
	fout << pixelWidth << "$" << pixelHeight << "$" << dpi;
	for (Fragment frag : fragments) {
		fout << endl << frag.filePath<<"$" << frag.left<<"$" << frag.top<<"$" << frag.rotated ? 1 : 0;
	}
	fout.close();
}

void StoreManager::saveDefault() {

}

void StoreManager::loadDefault() {

}

void StoreManager::load(wstring tar) {
	std::ifstream fin(tar.c_str(), std::ifstream::in);
	string head;
	fin >> head;
	vector<string> info = StringCoder::split(head);
	
	pixelWidth = std::stoi(info[0]);
	pixelHeight = std::stoi(info[1]);
	dpi = std::stoi(info[2]);
	margin = std::stoi(info[3]);
	while (!fin.eof())
	{
		string content;
		fin >> content;
		Fragment piece(content);
		fragments.push_back(piece);
	}

	fin.close();

}

void StoreManager::build(wstring tar) {

	Mat drawBoard(pixelHeight, pixelWidth, CV_8UC4, BufStorage::getStorage());

	for (Fragment frag : fragments) {
		ImageLoader img(StringCoder::String2WString(frag.filePath));
		Mat full(img.height, img.width, CV_8UC(img.channel), BufStorage::getStorage());
		float ratex = dpi / img.dpiX;
		float ratey = dpi / img.dpiY;
		Mat useful;
		resize(full, useful, Size(0, 0), ratex, ratey, INTER_NEAREST);

		Rect roi(frag.left, frag.top, useful.cols, useful.rows);
		useful.copyTo(drawBoard(roi));
	}

	TIFF *tiff = TIFFOpenW(tar.c_str(), "w");

	TIFFSetDirectory(tiff, 0);
	TIFFSetField(tiff, TIFFTAG_SUBFILETYPE, FILETYPE_PAGE);
	TIFFSetField(tiff, TIFFTAG_IMAGEWIDTH, pixelWidth);
	TIFFSetField(tiff, TIFFTAG_IMAGELENGTH, pixelHeight);
	TIFFSetField(tiff, TIFFTAG_SAMPLESPERPIXEL, 4);
	TIFFSetField(tiff, TIFFTAG_BITSPERSAMPLE, 8);
	TIFFSetField(tiff, TIFFTAG_COMPRESSION, COMPRESSION_LZW);
	TIFFSetField(tiff, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_SEPARATED);
	TIFFSetField(tiff, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);

	byte* data = BufStorage::getStorage();
	int step = pixelWidth * 4;

	for (int i = 0; i < pixelHeight; i++)
	{
		TIFFWriteScanline(tiff, data, i);
		data += step;
	}

	TIFFClose(tiff);

}