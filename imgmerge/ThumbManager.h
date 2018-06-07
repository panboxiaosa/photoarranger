#pragma once
#include "stdafx.h"
#include <map>

class ThumbManager
{
public:
	ThumbManager();
	~ThumbManager();

	std::string load(std::wstring path, string tag);

private:
	std::string getProfile(std::wstring profile, string tag);

	std::string createThumb(cv::Mat mat);

	map<string, string> cache;

};

