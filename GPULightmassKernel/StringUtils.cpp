#include "StringUtils.h"
#include <cstdlib>
#include <cstring>

#ifdef _WIN32
	#include <Windows.h>
#else
	#include <locale>
	#include <codecvt>
#endif

namespace RStringUtils
{

std::wstring WidenFromUTF8(const std::string& str)
{
#ifdef _WIN32
	int size_needed = MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), NULL, 0);
	std::wstring wstr(size_needed, 0);
	MultiByteToWideChar(CP_UTF8, 0, str.c_str(), str.length(), &wstr[0], size_needed);
	return wstr;
#else
	// UTF-8 to wstring conversion for Linux
	std::wstring_convert<std::codecvt_utf8<wchar_t>> converter;
	return converter.from_bytes(str);
#endif
}

std::wstring Widen(const std::string& str)
{
#ifdef _WIN32
	int size_needed = MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), NULL, 0);
	std::wstring wstr(size_needed, 0);
	MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), &wstr[0], size_needed);
	return wstr;
#else
	// Use system locale conversion for Linux
	size_t size_needed = mbstowcs(nullptr, str.c_str(), 0);
	if (size_needed == static_cast<size_t>(-1)) {
		return std::wstring();
	}
	std::wstring wstr(size_needed, 0);
	mbstowcs(&wstr[0], str.c_str(), size_needed);
	return wstr;
#endif
}

std::string Narrow(const std::wstring& wstr)
{
#ifdef _WIN32
	int size_needed = WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.length(), NULL, 0, NULL, NULL);
	std::string str(size_needed, 0);
	WideCharToMultiByte(CP_ACP, 0, wstr.c_str(), wstr.length(), &str[0], size_needed, NULL, NULL);
	return str;
#else
	// Use system locale conversion for Linux
	size_t size_needed = wcstombs(nullptr, wstr.c_str(), 0);
	if (size_needed == static_cast<size_t>(-1)) {
		return std::string();
	}
	std::string str(size_needed, 0);
	wcstombs(&str[0], wstr.c_str(), size_needed);
	return str;
#endif
}

std::string GetFileNameFromPath(std::string path)
{
	for (int i = path.length() - 1; i >= 0; i--)
	{
		if (path[i] == '/' || path[i] == '\\')
			return path.substr(i + 1);
	}
	return path;
}

std::wstring GetFileNameFromPath(std::wstring path)
{
	for (int i = path.length() - 1; i >= 0; i--)
	{
		if (path[i] == L'/' || path[i] == L'\\')
			return path.substr(i + 1);
	}
	return path;
}

std::string GetDirFromPath(std::string path)
{
	for (int i = path.length() - 1; i >= 0; i--)
	{
		if (path[i] == '/' || path[i] == '\\')
			return path.substr(0, i + 1);
	}
	return "";
}

std::wstring GetDirFromPath(std::wstring path)
{
	for (int i = path.length() - 1; i >= 0; i--)
	{
		if (path[i] == L'/' || path[i] == L'\\')
			return path.substr(0, i + 1);
	}
	return L"";
}

bool FileExistTest(const std::string& name)
{
	if (FILE *file = fopen(name.c_str(), "r"))
	{
		fclose(file);
		return true;
	}
	else
	{
		return false;
	}
}

}