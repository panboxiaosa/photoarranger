#include "FileUtil.h"
#include "StringCoder.h"

FileUtil::FileUtil()
{
}


FileUtil::~FileUtil()
{
}

string buildTimeStr(WIN32_FIND_DATA filedata) {
	stringstream ss;
	ss << filedata.ftCreationTime.dwHighDateTime
		<< filedata.ftCreationTime.dwLowDateTime
		<< filedata.ftLastWriteTime.dwHighDateTime
		<< filedata.ftLastWriteTime.dwLowDateTime;
	return ss.str();
}

void FileUtil::find(const TCHAR* lpPath, std::vector<pair<wstring, string> > &fileList)
{
	TCHAR szFind[MAX_PATH];
	WIN32_FIND_DATA FindFileData;

	_tcscpy_s(szFind, lpPath);
	_tcscat_s(szFind, _T("\\*.*"));

	HANDLE hFind = ::FindFirstFile(szFind, &FindFileData);
	if (INVALID_HANDLE_VALUE == hFind)    return;

	while (true)
	{
		if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			if (FindFileData.cFileName[0] != '.')
			{
				TCHAR szFile[MAX_PATH];
				_tcscpy_s(szFile, lpPath);
				_tcscat_s(szFile, (TCHAR*)(FindFileData.cFileName));

				find(szFile, fileList);
			}
		}
		else
		{
			TCHAR szTarget[MAX_PATH];
			_tcscpy_s(szTarget, lpPath);
			if (lpPath[_tcslen(lpPath) - 1] != _T('\\'))
			{
				_tcscat_s(szTarget, _T("\\"));
			}
			_tcscat_s(szTarget, FindFileData.cFileName);
			fileList.push_back(pair<wstring, string>(wstring(szTarget), buildTimeStr(FindFileData)));
		}
		if (!FindNextFile(hFind, &FindFileData))    break;
	}
	FindClose(hFind);
}

void FileUtil::find(const TCHAR* lpPath, vector<string> &nameList)
{
	TCHAR szFind[MAX_PATH];
	WIN32_FIND_DATA FindFileData;

	_tcscpy_s(szFind, lpPath);
	_tcscat_s(szFind, _T("\\*.*"));

	HANDLE hFind = ::FindFirstFile(szFind, &FindFileData);
	if (INVALID_HANDLE_VALUE == hFind)    return;

	while (true)
	{
		if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			if (FindFileData.cFileName[0] != '.')
			{
				TCHAR szFile[MAX_PATH];
				_tcscpy_s(szFile, lpPath);
				_tcscat_s(szFile, (TCHAR*)(FindFileData.cFileName));

				find(szFile, nameList);
			}
		}
		else
		{

			wstring name = FindFileData.cFileName;

			nameList.push_back(StringCoder::WString2String(name));
		}
		if (!FindNextFile(hFind, &FindFileData))    break;
	}
	FindClose(hFind);
}
