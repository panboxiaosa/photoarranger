#pragma once
#include "stdafx.h"
class Fragment
{
public:
	Fragment(string str);
	~Fragment();

	string filePath;
	int top;
	int left;
	bool rotated;


};

