#pragma once
#include "stdafx.h"
class Fragment
{
public:
	Fragment(wstring str);
	~Fragment();

	wstring filePath;
	int top;
	int left;
	bool rotated;
//private:
	int width;
	int height;

};

