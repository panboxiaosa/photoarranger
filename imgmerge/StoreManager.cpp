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


void StoreManager::setTag(TIFF** tiff) 
{
	TIFFSetDirectory(*tiff, 0);
	TIFFSetField(*tiff, TIFFTAG_SUBFILETYPE, FILETYPE_PAGE);
	TIFFSetField(*tiff, TIFFTAG_IMAGEWIDTH, pixelWidth);
	TIFFSetField(*tiff, TIFFTAG_IMAGELENGTH, pixelHeight);
	TIFFSetField(*tiff, TIFFTAG_SAMPLESPERPIXEL, 4);
	TIFFSetField(*tiff, TIFFTAG_BITSPERSAMPLE, 8);
	TIFFSetField(*tiff, TIFFTAG_COMPRESSION, COMPRESSION_LZW);
	TIFFSetField(*tiff, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_SEPARATED);
	TIFFSetField(*tiff, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);
	TIFFSetField(*tiff, TIFFTAG_RESOLUTIONUNIT, 2);
	TIFFSetField(*tiff, TIFFTAG_XRESOLUTION, (float)dpi);
	TIFFSetField(*tiff, TIFFTAG_YRESOLUTION, (float)dpi);
}


void StoreManager::load(wstring tar) {
	string path = StringCoder::WString2String(tar);
	std::wifstream fin(path);
	fin.imbue(std::locale("chs"));
	wstring head;
	getline(fin, head);
	vector<wstring> info = StringCoder::split(head, L"$");
	
	pixelWidth = std::stoi(info[0]);
	pixelHeight = std::stoi(info[1]);
	dpi = std::stoi(info[2]);
	margin = std::stoi(info[3]);

	wstring content;
	while (getline(fin, content))
	{
		if (content.length() <= 1)
			continue;
		Fragment piece(content);
		fragments.push_back(piece);
	}

	fin.close();

}

vector<wstring> StoreManager::getOriginFiles() {
	vector<wstring> all;
	for (Fragment frag : fragments) {
		all.push_back(frag.filePath);
	}
	return all;
}

void StoreManager::build(wstring tar) {

	int64 together = pixelWidth;
	together *= pixelHeight * 4;

	if (together > BOARDBUF) {
		buildByStep(tar);
		return;
	}

	Mat drawBoard(pixelHeight, pixelWidth, CV_8UC4, BufStorage::getBoardStorage());
	drawBoard = Scalar(0,0,0,0);

	for (Fragment frag : fragments) {
		ImageLoader img(frag.filePath);
		Mat full(img.height, img.width, CV_8UC(img.channel), BufStorage::getStorage());
		float ratex = (float)dpi / img.dpiX;
		float ratey = (float)dpi / img.dpiY;

		Mat cmyk = BufStorage::pickCmyk(full, img.colorSpace);

		Mat useful(img.height, img.width, CV_8UC4, BufStorage::getStorage());
		resize(cmyk, useful, Size(0, 0), ratex, ratey, INTER_NEAREST);
		Rect roi(frag.left + margin, frag.top + margin, useful.cols, useful.rows);
		useful.copyTo(drawBoard(roi));
		cout << "渲染图片 " << StringCoder::WString2String(frag.filePath) << " 完成" << endl;
	}

	TIFF *tiff = TIFFOpenW(tar.c_str(), "w");

	setTag(&tiff);
	byte* data = BufStorage::getBoardStorage();
	int step = pixelWidth * 4;
	int infoUnit = pixelHeight / 100;

	for (int i = 0; i < pixelHeight; i++)
	{
		TIFFWriteScanline(tiff, data, i);
		data += step;
		if (i % infoUnit == 0) {
			cout << "写入tif文件进度完成 " << i / infoUnit << "%" << endl;
		}
	}

	TIFFClose(tiff);
}

void StoreManager::buildByStep(wstring tar) {

	int eachHeight = BOARDBUF / (pixelWidth << 2);
	int steps = pixelHeight / eachHeight;

	TIFF *tiff = TIFFOpenW(tar.c_str(), "w");
	setTag(&tiff);
	int infoUnit = pixelHeight / 100;

	int step = pixelWidth * 4;

	for (int i = 0; i < steps; i++) {
		byte* data = buildPart(i, eachHeight);
		int offset = i * eachHeight;
		for (int j = 0; j < eachHeight; j++)
		{
			int cur = j + offset;
			TIFFWriteScanline(tiff, data, cur);
			data += step;
			if (cur % infoUnit == 0) {
				cout << "写入tif文件进度完成 " << cur / infoUnit << "%" << endl;
			}
		}
	}

	int restHeight = pixelHeight - eachHeight * steps;
	if (restHeight > 0) {
		byte* data = buildPart(steps, restHeight);
		int offset = eachHeight * steps;
		for (int i = 0; i < restHeight; i++)
		{
			int cur = i + offset;
			TIFFWriteScanline(tiff, data, cur);
			data += step;
			if (cur % infoUnit == 0) {
				cout << "写入tif文件进度完成 " << cur / infoUnit << "%" << endl;
			}
		}
	}

	TIFFClose(tiff);
}

bool StoreManager::between(Fragment item, int offset, int height) {
	int top = item.top + margin;
	int bottom = top + item.height;
	if (top < offset + height && bottom > offset) {
		return true;
	}
	else {
		return false;
	}
}

byte* StoreManager::buildPart(int index, int height) {

	Mat drawBoard(height, pixelWidth, CV_8UC4, BufStorage::getBoardStorage());
	drawBoard = Scalar(0, 0, 0, 0);

	int offset = index * height;

	for (Fragment frag : fragments) {

		if (!between(frag, offset, height)) {
			continue;
		}

		ImageLoader img(frag.filePath);
		Mat full(img.height, img.width, CV_8UC(img.channel), BufStorage::getStorage());
		float ratex = (float)dpi / img.dpiX;
		float ratey = (float)dpi / img.dpiY;

		Mat cmyk = BufStorage::pickCmyk(full, img.colorSpace);

		Mat prepare(img.height * ratey, img.width * ratex, CV_8UC4, BufStorage::getStorage());

		resize(cmyk, prepare, Size(0, 0), ratex, ratey, INTER_NEAREST);

		int actualWidth = prepare.cols;
		int actualHeight = prepare.rows;

		Rect src(0,0,actualWidth, actualHeight);
		Rect dst(frag.left + margin, frag.top + margin - offset, actualWidth, actualHeight);
		if (dst.y < 0) {
			src.y -= dst.y;
			src.height += dst.y;
			dst.height += dst.y;
			dst.y = 0;
			
		}
		if (dst.y + dst.height > height) {
			src.height -= dst.y + dst.height - height;
			dst.height = src.height;
		}

		(prepare(src)).copyTo(drawBoard(dst));
		cout << "渲染图片 " << StringCoder::WString2String(frag.filePath) << " 完成" << endl;
	}

	return BufStorage::getBoardStorage();
}