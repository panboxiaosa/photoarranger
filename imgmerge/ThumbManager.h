#pragma once
#include "stdafx.h"
#include "ImageLoader.h"

class ThumbManager
{
public:
	ThumbManager();
	~ThumbManager();

	void load(std::wstring path, string tag);

private:
	std::string getProfile(std::wstring profile, string tag);

	std::wstring createThumb(ImageLoader);

	void initCache();

	wstring getThumbDir();

	map<string, ImageLoader> cache;

};

