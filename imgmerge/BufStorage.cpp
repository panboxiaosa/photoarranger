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

	if (CMYKASPACE == colorSpace || CMYKSPACE == colorSpace) {
		const int fromTo[8] = { 0, 0, 1, 1, 2, 2, 3, 3 };
		mixChannels(mat, cmyk, fromTo, 4);
	}
	else if (RGBSPACE == colorSpace || RGBASPACE == colorSpace) {
		cmyk = Scalar(0, 0, 0, 0);
		const int fromTo[6] = { 2, 0, 1, 1, 0, 2 };
		mixChannels(mat, cmyk, fromTo, 3);
		cmyk = Scalar(255, 255, 255, 0) - cmyk;
	}
	return cmyk;
}