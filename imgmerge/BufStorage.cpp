#include "BufStorage.h"
#include "ImageLoader.h"

BufStorage::BufStorage()
{
}

BufStorage::~BufStorage()
{
}

void buildPtr(byte** pointer, uint32 size) 
{
	if (*pointer == nullptr) {
		*pointer = new byte[size];
	}
}

byte* BufStorage::getBoardStorage() {
	buildPtr(&board, BOARDBUF);
	return board;
}

byte* BufStorage::getStorage()
{
	buildPtr(&ptr, 1000000000u);
	return ptr;
}

byte* BufStorage::getSwapStorage() {
	buildPtr(&swap, 1000000000u);
	return swap;
}


void BufStorage::releaseStorage()
{
	delete[] ptr;
	ptr = 0;
	delete[] board;
	board = 0;
	delete[] swap;
	swap = 0;
}

byte* BufStorage::board = nullptr;
byte* BufStorage::ptr = nullptr;
byte* BufStorage::swap = nullptr;

Mat BufStorage::pickRgb(Mat mat) {
	return mat;
}

Mat BufStorage::pickCmyk(Mat mat, ushort colorSpace) {
	Mat cmyk(mat.size(), CV_8UC4, getSwapStorage());
	cmyk = Scalar(0,0,0,0);
	if (CMYKASPACE == colorSpace || CMYKSPACE == colorSpace) {
		const int fromTo[8] = { 0, 0, 1, 1, 2, 2 };
		mixChannels(mat, cmyk, fromTo, 3);
	}
	else if (RGBSPACE == colorSpace) {
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
	}
	else if (RGBASPACE == colorSpace) {
		for (int i = 0; i<cmyk.rows; i++){
			uchar *data = mat.ptr<uchar>(i);
			uchar *dataCMYK = cmyk.ptr<uchar>(i);
			for (int j = 0; j < cmyk.cols; j++){
				uchar b = data[4 * j + 1];
				uchar g = data[4 * j + 2];
				uchar r = data[4 * j + 3];

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
	}
	return cmyk;
}