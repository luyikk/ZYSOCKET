#ifndef __DEFTYPE
#define __DEFTYPE


#include <functional>
#include <map>
#include <list>
#include <assert.h>
#include <string>
#define MINHAND


#ifdef WIN32
#include <windows.h>
typedef  char int8;
typedef  unsigned char uint8;
typedef  unsigned char byte;
typedef  short int16;
typedef  int int32;
typedef __int64 int64;
typedef unsigned short uint16;
typedef unsigned int uint32;
typedef unsigned long long uint64;
#define CP_UTF8                   65001       // UTF-8 translation
#define CP_ACP                    0           // default to ANSI code page



inline char* U2GW(const char* utf8)
{
	int len = MultiByteToWideChar(CP_UTF8, 0, utf8, -1, NULL, 0);
	wchar_t* wstr = new wchar_t[len + 1];
	memset(wstr, 0, len + 1);
	MultiByteToWideChar(CP_UTF8, 0, utf8, -1, wstr, len);
	len = WideCharToMultiByte(CP_ACP, 0, wstr, -1, NULL, 0, NULL, NULL);
	char* str = new char[len + 1];
	memset(str, 0, len + 1);
	WideCharToMultiByte(CP_ACP, 0, wstr, -1, str, len, NULL, NULL);
	if (wstr) delete[] wstr;
	return str;
}
//GB2312µ½UTF-8µÄ×ª»»
inline char* G2UW(const char* gb2312)
{
	int len = MultiByteToWideChar(CP_ACP, 0, gb2312, -1, NULL, 0);
	wchar_t* wstr = new wchar_t[len + 1];
	memset(wstr, 0, len + 1);
	MultiByteToWideChar(CP_ACP, 0, gb2312, -1, wstr, len);
	len = WideCharToMultiByte(CP_UTF8, 0, wstr, -1, NULL, 0, NULL, NULL);
	char* str = new char[len + 1];
	memset(str, 0, len + 1);
	WideCharToMultiByte(CP_UTF8, 0, wstr, -1, str, len, NULL, NULL);
	if (wstr) delete[] wstr;
	return str;
}


#else

#include <iconv.h>
#include <unistd.h>
typedef  char int8;
typedef  unsigned char uint8;
typedef  unsigned char byte;
typedef  short int16;
typedef  int int32;
typedef long long int64;
typedef unsigned short uint16;
typedef unsigned int uint32;
typedef unsigned long long uint64;

inline int code_convert(char *from_charset, char *to_charset, char *inbuf, int inlen, char *outbuf, int outlen)
{
	iconv_t cd;
	
	char **pin = &inbuf;
	char **pout = &outbuf;

	cd = iconv_open(to_charset, from_charset);
	if (cd == 0) return  -1;
	memset(outbuf, 0, outlen);
    
	if (iconv(cd, pin, (size_t*)&inlen, pout, (size_t*)&outlen) == -1) return  -1;
	//printf("pin is %d,pout is %d\n",inlen,outlen);
	iconv_close(cd);
	return 0;
}

inline int u2g(const char *inbuf, int inlen, char *outbuf, int outlen)
{
	return code_convert("utf-8", "gb2312", (char*)inbuf, inlen, outbuf, outlen);
}

inline int g2u(const char *inbuf, int inlen, char *outbuf, int outlen)
{
	return code_convert("gb2312", "utf-8", (char*)inbuf, inlen, outbuf, outlen);
}



#endif




inline std::string U2G(std::string utf8)
{
#ifdef WIN32
	char* x =U2GW(utf8.c_str());
	std::string tmp;
	tmp.append(x);
	delete[] x;
	return tmp;
#else
	char gb[255];
	u2g(utf8.c_str(), (int)utf8.size(), gb, 255);
	std::string tmp;
	tmp.append(gb);
	return tmp;
#endif
}

inline std::string G2U(std::string gb2312)
{
#ifdef WIN32
	char* x = G2UW(gb2312.c_str());
	std::string tmp;
	tmp.append(x);
	delete[] x;
	return tmp;
#else
	char utf8[255];
	g2u(gb2312.c_str(), (int)gb2312.size(), utf8, 255);
	std::string tmp;
	tmp.append(utf8);
	return tmp;
#endif
}




#endif //  __DEFTYP

