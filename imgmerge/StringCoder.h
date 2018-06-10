#pragma once
#include "stdafx.h"

class StringCoder
{
public:
	StringCoder();
	~StringCoder();

	static std::string TCHAR2STRING(TCHAR *STR);

	static std::string string_To_UTF8(const std::string & str);

	static std::string UTF8_To_string(const std::string & str);
	
	// wchar_t to string  
	static void Wchar_tToString(std::string& szDst, wchar_t *wchar);

	static std::string WString2String(const std::wstring& ws);

	static bool endsWith(wstring obj, const WCHAR* suf);

	static std::wstring String2WString(const std::string& s);

	static vector<string> split(const string &s, const string &seperator);

	static vector<string> split(const string &s);

	static vector<wstring> split(const wstring& s, const wstring& seperator);
};

