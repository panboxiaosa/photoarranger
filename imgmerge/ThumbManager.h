#pragma once
#include "stdafx.h"
#include <map>

class ThumbManager
{
public:
	ThumbManager();
	~ThumbManager();

	std::string load(std::string path);

private:
	std::string getProfile(std::string profile);

	std::string createThumb(cv::Mat mat);

	map<string, string> cache;

};

