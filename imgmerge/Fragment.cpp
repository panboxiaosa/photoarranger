#include "Fragment.h"
#include "StringCoder.h"

Fragment::Fragment(string str)
{
	vector<string> parts = StringCoder::split(str);
	filePath = parts[0];
	left = stoi(parts[1]);
	top = stoi(parts[2]);
	rotated = stoi(parts[3]);

}


Fragment::~Fragment()
{
}
