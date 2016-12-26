#ifndef __CC_TCPSOCKET_H__
#define __CC_TCPSOCKET_H__



#include "ByteBuffer.h"

#ifdef WIN32

	#include <windows.h>
	#include <WinSock.h>
	#include <process.h>
  
	#pragma comment( lib, "ws2_32.lib" )

	
#else
	#include <sys/socket.h>
	#include <fcntl.h>
	#include <errno.h>
	#include <netinet/in.h>
	#include <arpa/inet.h>
	#include <pthread.h>

	#define SOCKET int
	#define SOCKET_ERROR -1
	#define INVALID_SOCKET -1  
#endif


#ifndef CHECKF
#define CHECKF(x) \
	do \
{ \
	if (!(x)) { \
	LOG("CHECKF:%s", #x, __FILE__, __LINE__); \
	return 0; \
	} \
} while (0)
#endif

#define _MAX_MSGSIZE 16 * 1024*8		// 暂定一个消息最大为32k
#define BLOCKSECONDS	30			// INIT函数阻塞时间
#define INBUFSIZE	(64*1024)		//?	具体尺寸根据剖面报告调整  接收数据的缓存
#define OUTBUFSIZE	(8*1024)		//? 具体尺寸根据剖面报告调整。 发送数据的缓存，当不超过8K时，FLUSH只需要SEND一次



class  TCPSocket
{
public:
	TCPSocket(void);
	int64 getSystemTime();
    std::string  domainToIP(const char* pDomain);
    bool    IsIPV6NetWork();
	bool	Create(const char* pszServerIP, int nServerPort, int tagid, int nBlockSec = BLOCKSECONDS, bool bKeepAlive = false);
	bool	SendMsg(void* pBuf, int nSize);
	bool	ReceiveMsg(void* pBuf, int& nSize);
	bool	Flush(void);
	bool	Check(void);
	void	Destroy(void);
	SOCKET	GetSocket(void) const { return m_sockClient; }
	void	ResOutBuff();
	int		getTagID(){ return m_tag; }
private:
	bool	recvFromSock(void);		// 从网络中读取尽可能多的数据
	bool    hasError();			// 是否发生错误，注意，异步模式未完成非错误
	void    closeSocket();

	int64   lastDataTime;
	SOCKET	m_sockClient;

	// 发送数据缓冲
	char	m_bufOutput[OUTBUFSIZE];	//? 可优化为指针数组
	int		m_nOutbufLen;

	// 环形缓冲区
	char	m_bufInput[INBUFSIZE];
	int		m_nInbufLen;
	int		m_nInbufStart;				// INBUF使用循环式队列，该变量为队列起点，0 - (SIZE-1)
	int		m_tag;
};

typedef std::function<bool(int,int,ByteBuffer&)> ProAllFunc;	// 接收所有协议，自行处理，@socket标识,@协议头，@数据包,返回是否分发
typedef std::function<void(int,ByteBuffer&)> ProFunc;	// 接收单个协议，@socket标识,@数据包
typedef std::function<void(int,bool)> sckConnectFunc;	// 连接成功/失败
typedef std::function<void(int)> sckDisconnectFunc;	// 连接断开


 // 说明：func函数名, _Object指针, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3表示回调函数的三个参数
#define SCT_CALLBACK_1(func, _Object) std::bind(&func,_Object, std::placeholders::_1, std::placeholders::_2, std::placeholders::_3)
#define SCT_CALLBACK_2(func, _Object) std::bind(&func,_Object, std::placeholders::_1, std::placeholders::_2)
#define SCT_CALLBACK_3(func, _Object) std::bind(&func,_Object, std::placeholders::_1)
// 创建SOCKET管理器
#define CREATE_TCPSOCKETMGR()	new TCPSocketManager()
// PING包定义
#define CMSG_PING		0x01
// PING包发送间隔
#define PINGINTERVAL_TIMER	10

struct SocketAddress
{
	std::string pserverip;
	int nPort;
	int nTag;
	SocketAddress()
	{
		pserverip = "";
		nPort = 0;
		nTag = 0;
	}
};

class TCPSocketManager
{
public:
	TCPSocketManager();
	~TCPSocketManager();
	// 创建socket并添加到管理器
	void createSocket(const char* pszServerIP,	// IP地址
							int nServerPort,			// 端口
							int _tag,					// 标识ID
							int nBlockSec = BLOCKSECONDS, // 阻塞时间ms
							bool bKeepAlive = false);
	// 连接服务器
	bool	connect(SocketAddress *pAddress);
	// 注册协议包
	void	register_process(const int32 &entry, ProFunc callback);
	// 注册接收所有协议
	void	register_all_process(ProAllFunc callback){ _pProcess = callback; }
	// 注册socket连接成功事件
	void	register_connect(sckConnectFunc callback){ _pOnConnect = callback; }
	// 注册socket断线事件
	void	register_disconnect(sckDisconnectFunc callback){ _OnDisconnect = callback; }
	// 单独添加socket到管理器
	bool	addSocket(TCPSocket *pSocket);
	// 删除socket
	bool	removeSocket(int _tag);

	bool    clecrSocketConnect();
	// 断开socket
	void	disconnect(int _tag);
	// 获取socket
	TCPSocket	*GetSocket(int _tag);
	// 发送消息
    bool	SendPacket(int _tag, ByteBuffer *packet);
	bool	SendPacket(int _tag, void *packet,int length);

	void	ResBuffOut();

	void	update();

	static TCPSocketManager &getSingleton(){ assert(mSingleton); return *mSingleton;}

#ifdef WIN32

	static void socket_thread(void *mgr);
	CRITICAL_SECTION mCs;	

#else
	static void *socket_thread(void *mgr);
	mutable pthread_mutex_t m_mutex;  
#endif
	
private:	
	ProAllFunc _pProcess;
	sckConnectFunc _pOnConnect;
	sckDisconnectFunc _OnDisconnect;
	std::list<TCPSocket*> m_lstSocket;
	std::map<int32, ProFunc> _mapProcess;
	static TCPSocketManager * mSingleton;
	time_t m_sendping_last_time;
	std::list<SocketAddress> m_lstAddress;
};

#define sSocketMgr	TCPSocketManager::getSingleton()


#endif //__CC_TCPSOCKET_H__
