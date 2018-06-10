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
	int height;
	int width;
	bool rotated;


};

