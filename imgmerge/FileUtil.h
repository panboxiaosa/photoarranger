#pragma once
#include "stdafx.h"

class FileUtil
{
public:
	FileUtil();
	~FileUtil();

	static void find(const TCHAR* lpPath, std::vector<pair<wstring, string> > &fileList);

	static void find(const TCHAR* lpPath, vector<string> &nameList);

	static vector<pair<wstring, string>> supply(vector<wstring> path);

	static string timeProfile(wstring filepath);

};

