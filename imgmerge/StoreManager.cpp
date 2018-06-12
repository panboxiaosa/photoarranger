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
		Mat cmyk = BufStorage::pickCmyk(full, img.colorSpace);

		float ratex = (float)dpi / img.dpiX;
		float ratey = (float)dpi / img.dpiY;

		if (frag.rotated) {
			Mat useful(frag.width, frag.height, CV_8UC4, BufStorage::getStorage());
			resize(cmyk, useful, Size(0, 0), ratex, ratey, INTER_NEAREST);
			transpose(useful, useful);
			flip(useful, useful, 1);

			Rect roi(frag.left + margin, frag.top + margin, useful.cols, useful.rows);
			useful.copyTo(drawBoard(roi));
		}
		else{
			Mat useful(frag.height, frag.width, CV_8UC4, BufStorage::getStorage());
			resize(cmyk, useful, Size(0, 0), ratex, ratey, INTER_NEAREST);

			Rect roi(frag.left + margin, frag.top + margin, useful.cols, useful.rows);
			useful.copyTo(drawBoard(roi));
		}

		Mat title = createText(frag);
		Rect titlePos(frag.left + margin, frag.top + margin + frag.height, title.cols, title.rows);
		title.copyTo(drawBoard(titlePos));

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
		int offset = i * eachHeight;
		byte* data = buildPart(offset, eachHeight);
		
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
		int offset = eachHeight * steps;
		byte* data = buildPart(offset, restHeight);
		
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

bool StoreManager::between(Fragment& item, int offset, int height) {
	int top = item.top + margin;

	int bottom = top + item.height;
	if (top < offset + height && bottom > offset) {
		return true;
	}
	else {
		return false;
	}
}

void StoreManager::cutPast(Mat& prepare, int height,int offset, Fragment& frag, Mat& drawBoard) {
	int actualWidth = prepare.cols;
	int actualHeight = prepare.rows;

	Rect src(0, 0, actualWidth, actualHeight);
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
}

void GetStringSize(HDC hDC, const char* str, int* w, int* h)
{
	SIZE size;
	GetTextExtentPoint32A(hDC, str, strlen(str), &size);
	if (w != 0) *w = size.cx;
	if (h != 0) *h = size.cy;
}

void putTextZH(Mat &dst, const char* str, Point org, Scalar color, int fontSize, const char* fn, bool italic = false, bool underline = false)
{
	CV_Assert(dst.data != 0 && (dst.channels() == 1 || dst.channels() == 3));

	int x, y, r, b;
	if (org.x > dst.cols || org.y > dst.rows) return;
	x = org.x < 0 ? -org.x : 0;
	y = org.y < 0 ? -org.y : 0;

	LOGFONTA lf;
	lf.lfHeight = -fontSize;
	lf.lfWidth = 0;
	lf.lfEscapement = 0;
	lf.lfOrientation = 0;
	lf.lfWeight = 5;
	lf.lfItalic = italic;   //斜体
	lf.lfUnderline = underline; //下划线
	lf.lfStrikeOut = 0;
	lf.lfCharSet = DEFAULT_CHARSET;
	lf.lfOutPrecision = 0;
	lf.lfClipPrecision = 0;
	lf.lfQuality = PROOF_QUALITY;
	lf.lfPitchAndFamily = 0;
	strcpy_s(lf.lfFaceName, fn);

	HFONT hf = CreateFontIndirectA(&lf);
	HDC hDC = CreateCompatibleDC(0);
	HFONT hOldFont = (HFONT)SelectObject(hDC, hf);

	int strBaseW = 0, strBaseH = 0;
	int singleRow = 0;
	char buf[1 << 12];
	strcpy_s(buf, str);
	char *bufT[1 << 12];  // 这个用于分隔字符串后剩余的字符，可能会超出。
	//处理多行
	{
		int nnh = 0;
		int cw, ch;

		const char* ln = strtok_s(buf, "\n", bufT);
		while (ln != 0)
		{
			GetStringSize(hDC, ln, &cw, &ch);
			strBaseW = max(strBaseW, cw);
			strBaseH = max(strBaseH, ch);

			ln = strtok_s(0, "\n", bufT);
			nnh++;
		}
		singleRow = strBaseH;
		strBaseH *= nnh;
	}

	if (org.x + strBaseW < 0 || org.y + strBaseH < 0)
	{
		SelectObject(hDC, hOldFont);
		DeleteObject(hf);
		DeleteObject(hDC);
		return;
	}

	r = org.x + strBaseW > dst.cols ? dst.cols - org.x - 1 : strBaseW - 1;
	b = org.y + strBaseH > dst.rows ? dst.rows - org.y - 1 : strBaseH - 1;
	org.x = org.x < 0 ? 0 : org.x;
	org.y = org.y < 0 ? 0 : org.y;

	BITMAPINFO bmp = { 0 };
	BITMAPINFOHEADER& bih = bmp.bmiHeader;
	int strDrawLineStep = strBaseW * 3 % 4 == 0 ? strBaseW * 3 : (strBaseW * 3 + 4 - ((strBaseW * 3) % 4));

	bih.biSize = sizeof(BITMAPINFOHEADER);
	bih.biWidth = strBaseW;
	bih.biHeight = strBaseH;
	bih.biPlanes = 1;
	bih.biBitCount = 24;
	bih.biCompression = BI_RGB;
	bih.biSizeImage = strBaseH * strDrawLineStep;
	bih.biClrUsed = 0;
	bih.biClrImportant = 0;

	void* pDibData = 0;
	HBITMAP hBmp = CreateDIBSection(hDC, &bmp, DIB_RGB_COLORS, &pDibData, 0, 0);

	CV_Assert(pDibData != 0);
	HBITMAP hOldBmp = (HBITMAP)SelectObject(hDC, hBmp);

	SetTextColor(hDC, RGB(255, 255, 255));
	SetBkColor(hDC, 0);

	strcpy_s(buf, str);
	const char* ln = strtok_s(buf, "\n", bufT);
	int outTextY = 0;
	while (ln != 0)
	{
		TextOutA(hDC, 0, outTextY, ln, strlen(ln));
		outTextY += singleRow;
		ln = strtok_s(0, "\n", bufT);
	}
	uchar* dstData = (uchar*)dst.data;
	int dstStep = dst.step / sizeof(dstData[0]);
	unsigned char* pImg = (unsigned char*)dst.data + org.x * dst.channels() + org.y * dstStep;
	unsigned char* pStr = (unsigned char*)pDibData + x * 3;
	for (int tty = y; tty <= b; ++tty)
	{
		unsigned char* subImg = pImg + (tty - y) * dstStep;
		unsigned char* subStr = pStr + (strBaseH - tty - 1) * strDrawLineStep;
		for (int ttx = x; ttx <= r; ++ttx)
		{
			for (int n = 0; n < dst.channels(); ++n){
				double vtxt = subStr[n] / 255.0;
				int cvv = vtxt * color.val[n] + (1 - vtxt) * subImg[n];
				subImg[n] = cvv > 255 ? 255 : (cvv < 0 ? 0 : cvv);
			}

			subStr += 3;
			subImg += dst.channels();
		}
	}

	SelectObject(hDC, hOldBmp);
	SelectObject(hDC, hOldFont);
	DeleteObject(hf);
	DeleteObject(hBmp);
	DeleteDC(hDC);
}

Mat StoreManager::createText(Fragment& frag) {
	string path = StringCoder::WString2String(frag.filePath);
	vector<string> parts = StringCoder::split(path, "\\");
	Mat mat(margin, frag.width - margin * 2, CV_8UC3, Scalar(255,255,255));
	putTextZH(mat, parts[parts.size() - 1].c_str(), Point(0, 0), Scalar(0, 0, 0), min(18,margin), "微软雅黑");
	Mat cmyk(margin, frag.width - margin * 2, CV_8UC4, Scalar(0,0,0,0));
	for (int i = 0; i<cmyk.rows; i++){
		uchar *data = mat.ptr<uchar>(i);
		uchar *dataCMYK = cmyk.ptr<uchar>(i);
		for (int j = 0; j < cmyk.cols; j++){
			uchar b = data[3 * j];
			uchar g = data[3 * j + 1];
			uchar r = data[3 * j + 2];

			uchar c = 255 - r;
			uchar m = 255 - g;
			uchar y = 255 - b;
			uchar k = min(min(c, m), y);
			dataCMYK[4 * j] = c - k;
			dataCMYK[4 * j + 1] = m - k;
			dataCMYK[4 * j + 2] = y - k;
			dataCMYK[4 * j + 3] = k;
		}
	}
	return cmyk;
}

byte* StoreManager::buildPart(int offset, int height) {

	Mat drawBoard(height, pixelWidth, CV_8UC4, BufStorage::getBoardStorage());
	drawBoard = Scalar(0, 0, 0, 0);

	for (Fragment frag : fragments) {

		if (!between(frag, offset, height)) {
			continue;
		}

		ImageLoader img(frag.filePath);
		Mat full(img.height, img.width, CV_8UC(img.channel), BufStorage::getStorage());
		float ratex = (float)dpi / img.dpiX;
		float ratey = (float)dpi / img.dpiY;

		Mat cmyk = BufStorage::pickCmyk(full, img.colorSpace);
		if (frag.rotated) {
			Mat prepare(frag.width, frag.height, CV_8UC4, BufStorage::getStorage());
			resize(cmyk, prepare, Size(0, 0), ratex, ratey, INTER_NEAREST);
			transpose(prepare, prepare);
			flip(prepare, prepare, 1);
			cutPast(prepare, height, offset, frag, drawBoard);

		}
		else {
			Mat prepare(frag.height, frag.width, CV_8UC4, BufStorage::getStorage());
			resize(cmyk, prepare, Size(0, 0), ratex, ratey, INTER_NEAREST);
			cutPast(prepare, height, offset, frag, drawBoard);
		}
		
		Mat title = createText(frag);
		Rect src(0, 0, title.cols, title.rows);
		Rect dst(frag.left + margin, frag.top + margin + frag.height - offset, title.cols, title.rows);

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
		if (src.height > 0) {
			title(src).copyTo(drawBoard(dst));
		}

		cout << "渲染图片 " << StringCoder::WString2String(frag.filePath) << " 完成" << endl;
	}

	return BufStorage::getBoardStorage();
}