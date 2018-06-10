#pragma once
#include "stdafx.h"
#define BOARDBUF 1000000000u

class BufStorage
{
public:
	BufStorage();
	~BufStorage();

	static byte* getStorage();

	static byte* getBoardStorage();

	static byte* getSwapStorage();

	static void releaseStorage();

	static Mat pickRgb(Mat mat);

	static Mat pickCmyk(Mat mat, ushort);

private:
	static byte* ptr;
	static byte* board;
	static byte* swap;
};

