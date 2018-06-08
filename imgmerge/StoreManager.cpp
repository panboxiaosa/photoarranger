#include "StoreManager.h"


StoreManager::StoreManager()
{
}


StoreManager::~StoreManager()
{
}


void StoreManager::saveTo(wstring tar) 
{

}

void StoreManager::saveDefault() {

}

void StoreManager::loadDefault() {

}

void StoreManager::load(wstring tar) {
	std::ifstream fin(tar.c_str(), std::ifstream::in | std::ifstream::binary);
	
	while (!fin.eof())
	{
		string content;
		fin >> content;

	}

	fin.close();


}

void StoreManager::build(string tar) {

}