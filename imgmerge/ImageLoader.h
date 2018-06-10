#pragma once
#include "stdafx.h"

#define RGBSPACE 0
#define CMYKSPACE 1
#define CMYKASPACE 2
#define RGBASPACE 3
#define UNHANDLABLESPACE 4

class ImageLoader
{
public:
	ImageLoader();
	ImageLoader(wstring path);
	~ImageLoader();

	int width;
	int height;
	short channel;
	ushort dpiX;
	ushort dpiY;
	string profileStr;
	ushort colorSpace;

	void loadJpg(wstring path);
	void loadTif(wstring path);

	string toString();
	wstring toWstring();

};

