#pragma once
#include "stdafx.h"

class BufStorage
{
public:
	BufStorage();
	~BufStorage();

	static byte* getStorage();

	static void releaseStorage();

private:
	static byte* ptr;
};

