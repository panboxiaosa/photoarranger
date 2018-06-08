#pragma once
#include "stdafx.h"
#include <map>

class ThumbManager
{
public:
	ThumbManager();
	~ThumbManager();

	std::wstring load(std::wstring path, string tag);

private:
	std::string getProfile(std::wstring profile, string tag);

	std::wstring createThumb(cv::Mat mat, string name);

	void initCache();

	wstring getThumbDir();

	map<string, wstring> cache;

};

