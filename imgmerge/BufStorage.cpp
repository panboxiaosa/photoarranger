#include "BufStorage.h"


BufStorage::BufStorage()
{
}


BufStorage::~BufStorage()
{
	
}

byte* BufStorage::getStorage()
{
	if (ptr == nullptr) {
		ptr = new byte[4000000000u];
	}
	return ptr;
}

void BufStorage::releaseStorage()
{
	delete[] ptr;
	ptr = 0;
}


byte* BufStorage::ptr = nullptr;

