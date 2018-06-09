#pragma once
#include "stdafx.h"

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

	void loadJpg(wstring path);
	void loadTif(wstring path);

	string toString();
	wstring toWstring();

};

