#include "FileUtil.h"
#include "StringCoder.h"

FileUtil::FileUtil()
{
}


FileUtil::~FileUtil()
{
}

string buildTimeStr(FILETIME ftCreate, FILETIME ftModify) {
	stringstream ss;
	ss << ftCreate.dwHighDateTime
		<< ftCreate.dwLowDateTime
		<< ftModify.dwHighDateTime
		<< ftModify.dwLowDateTime;
	return ss.str();
}

string buildTimeStr(WIN32_FIND_DATA filedata) {
	
	return buildTimeStr(filedata.ftCreationTime, filedata.ftLastWriteTime);
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

vector<pair<wstring, string>> FileUtil::supply(vector<wstring> path) {
	vector<pair<wstring, string>> ret;
	for (wstring str : path) {
		FILETIME ftCreate, ftAccess, ftModify;

		HANDLE hFile = CreateFile(str.c_str(),
			GENERIC_READ,
			FILE_SHARE_READ,
			NULL,
			OPEN_EXISTING,
			FILE_FLAG_BACKUP_SEMANTICS,
			NULL);

		if (GetFileTime(hFile, &ftCreate, &ftAccess, &ftModify))
		{
			string timeStr = buildTimeStr(ftCreate, ftModify);
			ret.push_back(pair<wstring, string>(str, timeStr));
		}
	}
	return ret;
}

string FileUtil::timeProfile(wstring filepath) {
	FILETIME ftCreate, ftAccess, ftModify;

	HANDLE hFile = CreateFile(filepath.c_str(),
		GENERIC_READ,
		FILE_SHARE_READ,
		NULL,
		OPEN_EXISTING,
		FILE_FLAG_BACKUP_SEMANTICS,
		NULL);

	if (GetFileTime(hFile, &ftCreate, &ftAccess, &ftModify))
	{
		return buildTimeStr(ftCreate, ftModify);
	}
	return "";
}
