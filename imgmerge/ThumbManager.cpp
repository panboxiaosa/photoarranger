#include "ThumbManager.h"

ThumbManager::ThumbManager()
{

}


ThumbManager::~ThumbManager()
{
}


std::string ThumbManager::load(std::string path){

	string profile = getProfile(path);
	if (cache.find(profile) != cache.end()) {
		return cache[profile];
	}
	else 
	{
		Mat full = imread(path);
		string thumb = createThumb(full);
		cache[profile] = thumb;
		return thumb;
	}
}

std::string ThumbManager::getProfile(std::string profile) {
	return "";
}


std::string ThumbManager::createThumb(cv::Mat mat)
{
	return "";
}