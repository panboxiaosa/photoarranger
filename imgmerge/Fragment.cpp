#include "Fragment.h"
#include "StringCoder.h"

Fragment::Fragment(wstring str)
{
	vector<wstring> parts = StringCoder::split(str, L"$");
	filePath = parts[0];
	left = stoi(parts[1]);
	top = stoi(parts[2]);
	width = stoi(parts[3]);
	height = stoi(parts[4]);
	rotated = stoi(parts[5]);

}


Fragment::~Fragment()
{
}
