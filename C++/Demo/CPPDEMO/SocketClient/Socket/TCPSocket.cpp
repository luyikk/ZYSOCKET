

#include "TCPSocket.h"
#include <sys/timeb.h>


#if CC_TARGET_PLATFORM==CC_PLATFORM_WIN32||CC_TARGET_PLATFORM==CC_PLATFORM_WINRT
#define bzero(a, b)             memset(a, 0, b)
#endif

int64 TCPSocket::getSystemTime()
{
	struct timeb t;
	ftime(&t);
	int64 ttime = t.time;
	ttime = ttime * 1000;
	ttime += t.millitm;
	return ttime;
}

TCPSocket::TCPSocket()
{
    // ≥ı ºªØ
    memset(m_bufOutput, 0, sizeof(m_bufOutput));
    memset(m_bufInput, 0, sizeof(m_bufInput));
	lastDataTime = 0;
}

void TCPSocket::closeSocket()
{
#ifdef WIN32

    closesocket(m_sockClient);
    WSACleanup();

#else
    
    close(m_sockClient);
#endif
}

void TCPSocket::ResOutBuff()
{
	memset(m_bufInput, 0, sizeof(m_bufInput));
	m_nInbufLen = 0;		// ÷±Ω”«Âø’INBUF
	m_nInbufStart = 0;
	lastDataTime = 0;
}

//域名解析


bool TCPSocket::Create(const char* pszServerIP, int nServerPort, int tagid, int nBlockSec, bool bKeepAlive /*= FALSE*/)
{
	// ºÏ≤È≤Œ ˝
	if (pszServerIP == 0 || strlen(pszServerIP) > 15) {
		return false;
	}

#ifdef WIN32

	WSADATA wsaData;
	WORD version = MAKEWORD(2, 0);
	int ret = WSAStartup(version, &wsaData);//win sock start up
	if (ret != 0) {
		return false;
	}

#endif


	m_sockClient = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	if (m_sockClient == INVALID_SOCKET) {
		closeSocket();
		return false;
	}



	// …Ë÷√SOCKETŒ™KEEPALIVE
	if (bKeepAlive)
	{
		int		optval = 1;
		if (setsockopt(m_sockClient, SOL_SOCKET, SO_KEEPALIVE, (char *)&optval, sizeof(optval)))
		{
			closeSocket();
			return false;
		}
	}

#ifdef WIN32

	DWORD nMode = 1;
	int nRes = ioctlsocket(m_sockClient, FIONBIO, &nMode);
	if (nRes == SOCKET_ERROR) {
		closeSocket();
		return false;
	}

#else
	// …Ë÷√Œ™∑«◊Ë»˚∑Ω Ω
	fcntl(m_sockClient, F_SETFL, O_NONBLOCK);
#endif

	unsigned long serveraddr = inet_addr(pszServerIP);
	if (serveraddr == INADDR_NONE)	// ºÏ≤ÈIPµÿ÷∑∏Ò Ω¥ÌŒÛ
	{
		closeSocket();
		return false;
	}


	sockaddr_in	addr_in;
	memset((void *)&addr_in, 0, sizeof(addr_in));
	addr_in.sin_family = AF_INET;
	addr_in.sin_port = htons(nServerPort);
#ifdef WIN32
	addr_in.sin_addr.s_addr = serveraddr;
#else
	if (inet_aton(pszServerIP, &addr_in.sin_addr) == 0)
	{
		closeSocket();
		return false;
	}
#endif
	//inet_pton(AF_INET, pszServerIP, &addr_in.sin_addr);


	if (connect(m_sockClient, (sockaddr *)&addr_in, sizeof(addr_in)) == SOCKET_ERROR) {
		if (hasError()) {
			closeSocket();
			return false;
		}
		else	// WSAWOLDBLOCK
		{
			timeval timeout;
			timeout.tv_sec = nBlockSec;
			timeout.tv_usec = 0;
			fd_set writeset, exceptset;
			FD_ZERO(&writeset);
			FD_ZERO(&exceptset);
			FD_SET(m_sockClient, &writeset);
			FD_SET(m_sockClient, &exceptset);

			int ret = select(FD_SETSIZE, NULL, &writeset, &exceptset, &timeout);
			if (ret == 0 || ret < 0) {
				closeSocket();
				return false;
			}
			else	// ret > 0
			{
				ret = FD_ISSET(m_sockClient, &exceptset);
				if (ret)		// or (!FD_ISSET(m_sockClient, &writeset)
				{
					closeSocket();
					return false;
				}
			}
		}
	}

	    
    
    m_nInbufLen = 0;
    m_nInbufStart = 0;
    m_nOutbufLen = 0;

    struct linger so_linger;
    so_linger.l_onoff = 1;
    so_linger.l_linger = 500;
    setsockopt(m_sockClient, SOL_SOCKET, SO_LINGER, (const char*)&so_linger, sizeof(so_linger));
    m_tag = tagid;
    return true;
}

bool TCPSocket::SendMsg(void* pBuf, int nSize)
{
    if (pBuf == 0 || nSize <= 0) {
        return false;
    }

    if (m_sockClient == INVALID_SOCKET) {
        return false;
    }

    // ºÏ≤ÈÕ®—∂œ˚œ¢∞¸≥§∂»
    int packsize = 0;
    packsize = nSize;

    // ºÏ≤‚BUF“Á≥ˆ
    if (m_nOutbufLen + nSize > OUTBUFSIZE) {
        // ¡¢º¥∑¢ÀÕOUTBUF÷–µƒ ˝æ›£¨“‘«Âø’OUTBUF°£
        Flush();
        if (m_nOutbufLen + nSize > OUTBUFSIZE) {
            // ≥ˆ¥Ì¡À
            Destroy();
            return false;
        }
    }
    //  ˝æ›ÃÌº”µΩBUFŒ≤
    memcpy(m_bufOutput + m_nOutbufLen, pBuf, nSize);
    m_nOutbufLen += nSize;
    return true;
}

bool TCPSocket::ReceiveMsg(void* pBuf, int& nSize)
{
    //ºÏ≤È≤Œ ˝
    if (pBuf == NULL || nSize <= 0) {
        return false;
    }

    if (m_sockClient == INVALID_SOCKET) {
        return false;
    }

    // ºÏ≤È «∑Ò”–“ª∏ˆœ˚œ¢(–°”⁄2‘ÚŒﬁ∑®ªÒ»°µΩœ˚œ¢≥§∂»)
    if (m_nInbufLen < 4) {
        //  »Áπ˚√ª”–«Î«Û≥…π¶  ªÚ’ﬂ   »Áπ˚√ª”– ˝æ›‘Ú÷±Ω”∑µªÿ
        if (!recvFromSock() || m_nInbufLen < 4) {		// ’‚∏ˆm_nInbufLen∏¸–¬¡À
            return false;
        }
    }

#ifdef MINHAND
    // 小头
         int packsize = (unsigned char)m_bufInput[m_nInbufStart] + (unsigned char)m_bufInput[(m_nInbufStart + 1) % INBUFSIZE] * 256
             + (unsigned char)m_bufInput[(m_nInbufStart + 2) % INBUFSIZE] * 256 + (unsigned char)m_bufInput[(m_nInbufStart + 3) % INBUFSIZE] * 256;
#else

    int packsize = (unsigned char)m_bufInput[(m_nInbufStart) % INBUFSIZE] * 256
        + (unsigned char)m_bufInput[(m_nInbufStart + 1) % INBUFSIZE] * 256
        + (unsigned char)m_bufInput[(m_nInbufStart + 2) % INBUFSIZE] * 256
        + (unsigned char)m_bufInput[(m_nInbufStart + 3)];
#endif

    //     int32 packsize;
    //     ByteBuffer buffer;
    //     buffer.Write((uint8*)pBuf, INBUFSIZE);
    //     buffer >> packsize;

    // ºÏ≤‚œ˚œ¢∞¸≥ﬂ¥Á¥ÌŒÛ ‘›∂®◊Ó¥Û16k
    if (packsize <= 0 || packsize > _MAX_MSGSIZE) {

		ResOutBuff();
   
        return false;
    }

    // ºÏ≤Èœ˚œ¢ «∑ÒÕÍ’˚(»Áπ˚Ω´“™øΩ±¥µƒœ˚œ¢¥Û”⁄¥À ±ª∫≥Â«¯ ˝æ›≥§∂»£¨–Ë“™‘Ÿ¥Œ«Î«ÛΩ” ’ £”‡ ˝æ›)
    if (packsize > m_nInbufLen) {
        // »Áπ˚√ª”–«Î«Û≥…π¶   ªÚ’ﬂ    “¿»ªŒﬁ∑®ªÒ»°µΩÕÍ’˚µƒ ˝æ›∞¸  ‘Ú∑µªÿ£¨÷±µΩ»°µ√ÕÍ’˚∞¸
        if (!recvFromSock() || packsize > m_nInbufLen) {	// ’‚∏ˆm_nInbufLen“—∏¸–¬

			if (lastDataTime == 0)
			{
				lastDataTime = getSystemTime();
			}
			else
			{
				auto time = getSystemTime();

				if (time - lastDataTime > 2000)
				{
					ResOutBuff();
				
					return false;
				}
			}
			
            return false;
        }
    }
    
    lastDataTime=0;    
    
    if (m_nInbufStart + packsize > INBUFSIZE) {
        
        int copylen = INBUFSIZE - m_nInbufStart;
        memcpy(pBuf, m_bufInput + m_nInbufStart, copylen);       
        memcpy((unsigned char *)pBuf + copylen, m_bufInput, packsize - copylen);
        nSize = packsize;
    }
    else {        
        memcpy(pBuf, m_bufInput + m_nInbufStart, packsize);
        nSize = packsize;
    }
	    
    m_nInbufStart = (m_nInbufStart + packsize) % INBUFSIZE;
    m_nInbufLen -= packsize;
    return	true;
}

bool TCPSocket::hasError()
{
#ifdef WIN32

    int err = WSAGetLastError();
    if (err != WSAEWOULDBLOCK) {
        return true;
    }

#else
    int err = errno;
    if(err != EINPROGRESS && err != EAGAIN) 
        return true;
#endif
    return false;
}

// ¥”Õ¯¬Á÷–∂¡»°æ°ø…ƒ‹∂‡µƒ ˝æ›£¨ µº œÚ∑˛ŒÒ∆˜«Î«Û ˝æ›µƒµÿ∑Ω
bool TCPSocket::recvFromSock(void)
{
    if (m_nInbufLen >= INBUFSIZE || m_sockClient == INVALID_SOCKET) {
        return false;
    }

    // Ω” ’µ⁄“ª∂Œ ˝æ›
    int	savelen, savepos;			//  ˝æ›“™±£¥Êµƒ≥§∂»∫ÕŒª÷√
    if (m_nInbufStart + m_nInbufLen < INBUFSIZE)	{	// INBUF÷–µƒ £”‡ø’º‰”–ªÿ»∆
        savelen = INBUFSIZE - (m_nInbufStart + m_nInbufLen);		// ∫Û≤øø’º‰≥§∂»£¨◊Ó¥ÛΩ” ’ ˝æ›µƒ≥§∂»
    }
    else {
        savelen = INBUFSIZE - m_nInbufLen;
    }

    // ª∫≥Â«¯ ˝æ›µƒƒ©Œ≤
    savepos = (m_nInbufStart + m_nInbufLen) % INBUFSIZE;
    //CHECKF(savepos + savelen <= INBUFSIZE);
    int inlen = recv(m_sockClient, m_bufInput + savepos, savelen, 0);
    if (inlen > 0) {
        // ”–Ω” ’µΩ ˝æ›
        m_nInbufLen += inlen;

        if (m_nInbufLen > INBUFSIZE) {
            return false;
        }

        // Ω” ’µ⁄∂˛∂Œ ˝æ›(“ª¥ŒΩ” ’√ª”–ÕÍ≥…£¨Ω” ’µ⁄∂˛∂Œ ˝æ›)
        if (inlen == savelen && m_nInbufLen < INBUFSIZE) {
            int savelen = INBUFSIZE - m_nInbufLen;
            int savepos = (m_nInbufStart + m_nInbufLen) % INBUFSIZE;
            //CHECKF(savepos + savelen <= INBUFSIZE);
            inlen = recv(m_sockClient, m_bufInput + savepos, savelen, 0);
            if (inlen > 0) {
                m_nInbufLen += inlen;
                if (m_nInbufLen > INBUFSIZE) {
                    return false;
                }
            }
            else if (inlen == 0) {
                Destroy();
                return false;
            }
            else {
                // ¡¨Ω”“—∂œø™ªÚ’ﬂ¥ÌŒÛ£®∞¸¿®◊Ë»˚£©
                if (hasError()) {
                    Destroy();
                    return false;
                }
            }
        }
    }
    else if (inlen == 0) {
        Destroy();
        return false;
    }
    else {
        // ¡¨Ω”“—∂œø™ªÚ’ﬂ¥ÌŒÛ£®∞¸¿®◊Ë»˚£©
        if (hasError()) {
            Destroy();
            return false;
        }
    }

    return true;
}

bool TCPSocket::Flush(void)		//? »Áπ˚ OUTBUF > SENDBUF ‘Ú–Ë“™∂‡¥ŒSEND£®£©
{
    if (m_sockClient == INVALID_SOCKET|| m_sockClient==(SOCKET)(0)) {
        return false;
    }

    if (m_nOutbufLen <= 0) {
        return true;
    }

    // ∑¢ÀÕ“ª∂Œ ˝æ›
    int	outsize;
    outsize = send(m_sockClient, m_bufOutput, m_nOutbufLen, 0);
    if (outsize > 0) {
        // …æ≥˝“—∑¢ÀÕµƒ≤ø∑÷
        if (m_nOutbufLen - outsize > 0) {
            memcpy(m_bufOutput, m_bufOutput + outsize, m_nOutbufLen - outsize);
        }

        m_nOutbufLen -= outsize;

        if (m_nOutbufLen < 0) {
            return false;
        }
    }
    else {
        if (hasError()) {
            Destroy();
            return false;
        }
    }
    return true;
}

bool TCPSocket::Check(void)
{
    // ºÏ≤È◊¥Ã¨
    if (m_sockClient == INVALID_SOCKET) {
        return false;
    }

    char buf[1];
    int	ret = recv(m_sockClient, buf, 1, MSG_PEEK);
    if (ret == 0) {
        Destroy();
        return false;
    }
    else if (ret < 0) {
        if (hasError()) {
            Destroy();
            return false;
        }
        else {	// ◊Ë»˚
            return true;
        }
    }
    else {	// ”– ˝æ›
        return true;
    }

    return true;
}

void TCPSocket::Destroy(void)
{
    // πÿ±’
    struct linger so_linger;
    so_linger.l_onoff = 1;
    so_linger.l_linger = 500;
    int ret = setsockopt(m_sockClient, SOL_SOCKET, SO_LINGER, (const char*)&so_linger, sizeof(so_linger));

    closeSocket();

    m_sockClient = INVALID_SOCKET;
    m_nInbufLen = 0;
    m_nInbufStart = 0;
    m_nOutbufLen = 0;

    memset(m_bufOutput, 0, sizeof(m_bufOutput));
    memset(m_bufInput, 0, sizeof(m_bufInput));
}

TCPSocketManager *TCPSocketManager::mSingleton = 0;
TCPSocketManager::TCPSocketManager()
{
    assert(!mSingleton);
    this->mSingleton = this;
#ifdef WIN32

    _beginthread(TCPSocketManager::socket_thread, 0, (void *)mSingleton);
    InitializeCriticalSection(&mCs);	

#else
    pthread_t id;
    pthread_create(&id,NULL,TCPSocketManager::socket_thread, (void *)mSingleton);
    pthread_mutex_init(&m_mutex, NULL);
#endif
}

TCPSocketManager::~TCPSocketManager()
{
#ifdef WIN32
    EnterCriticalSection(&mCs);
#else
    pthread_mutex_destroy(&m_mutex);
#endif
}
// œﬂ≥Ã∫Ø ˝
#ifdef WIN32


void TCPSocketManager::socket_thread(void *pThread){
    while (true){
        Sleep(20);
        ((TCPSocketManager*)mSingleton)->update();
    }
}

#else
void *TCPSocketManager::socket_thread(void *mgr){
    while(true){
        sleep(20);
        ((TCPSocketManager*)mSingleton)->update();
    }
    return (void*)0;
}
#endif

void TCPSocketManager::createSocket(const char* pszServerIP,	// IPµÿ÷∑
    int nServerPort,			// ∂Àø⁄
    int _tag,					// ±Í ∂ID
    int nBlockSec,			// ◊Ë»˚ ±º‰ms
    bool bKeepAlive)
{
    SocketAddress address;
    address.nPort = nServerPort;
    address.pserverip = pszServerIP;
    address.nTag = _tag;
    m_lstAddress.push_back(address);
}

bool	TCPSocketManager::connect(SocketAddress *pAddress)
{
    TCPSocket *pSocket = new TCPSocket();
    if (pSocket->Create(pAddress->pserverip.c_str(), pAddress->nPort, pAddress->nTag))
    {
        addSocket(pSocket);
        if (_pOnConnect){
            _pOnConnect(pAddress->nTag, true);
        }
        return true;
    }
    else
    {
        if (_pOnConnect){
            _pOnConnect(pAddress->nTag, false);
        }
    }

    delete pSocket;
    return false;
}

bool	TCPSocketManager::addSocket(TCPSocket *pSocket)
{
    std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
    for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
    {
        if ((*iter)->GetSocket() == pSocket->GetSocket())
        {
            return false;
        }
    }
    m_lstSocket.push_back(pSocket);
    return true;
}

// …æ≥˝socket
bool	TCPSocketManager::removeSocket(int _tag)
{
    std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
    for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
    {
        if ((*iter)->getTagID() == _tag)
        {
            (*iter)->Destroy();
            m_lstSocket.erase(iter);
            return true;
        }
    }
    return false;
}

bool TCPSocketManager::clecrSocketConnect()
{
	m_lstAddress.clear();	
	return true;
}

TCPSocket *TCPSocketManager::GetSocket(int _tag)
{
    std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
    for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
    {
        if ((*iter)->getTagID() == _tag)
        {
            return *iter;
        }
    }
    return NULL;
}

void	TCPSocketManager::update()
{
    if (!m_lstAddress.empty())
    {// ¥¶¿Ì¥˝¡¨Ω”
        std::list<SocketAddress>::iterator iter, enditer = m_lstAddress.end();
        for (iter = m_lstAddress.begin(); iter != enditer;)
        {
            SocketAddress & address = *iter;
            if (connect(&address))
            {
				iter = m_lstAddress.erase(iter);
                continue;
            }
			else
			{
				iter = m_lstAddress.erase(iter);
				continue;
			}

            ++iter;
        }
        return;
    }

   
    std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
    for (iter = m_lstSocket.begin(); iter != enditer;)
    {
        TCPSocket *pSocket = *iter;
        int _tag = pSocket->getTagID();
        if (!pSocket->Check())
        {// µÙœﬂ¡À
            if (_OnDisconnect != 0){
                _OnDisconnect(_tag);
            }
            iter = m_lstSocket.erase(iter);
            continue;
        }


        while (true)
        {
            char buffer[_MAX_MSGSIZE] = { 0 };
            int nSize = sizeof(buffer);
            char *pbufMsg = buffer;
			if (!pSocket->ReceiveMsg(pbufMsg, nSize))
			{
				break;
			}
            int32 skipLen;           
            int32 cmd;
#ifdef WINDOWS
			ByteBuffer* packet = new ByteBuffer((uint32)nSize);
#else
            ByteBuffer* packet = new ByteBuffer((size_t)nSize);
#endif
            (*packet).Write((uint8*)pbufMsg, nSize);
            (*packet) >> skipLen >> cmd;


		

            if (_pProcess == 0 || !_pProcess(pSocket->getTagID(), cmd, *packet))
            {
               
                std::map<int32, ProFunc>::iterator mapi = _mapProcess.find(cmd);
                if (mapi == _mapProcess.end())
                    continue;
                mapi->second(pSocket->getTagID(), *packet);
				delete packet;
            }

			
        }
        ++iter;
    }
}

void    TCPSocketManager::register_process(const int32 &entry, ProFunc callback)
{
    _mapProcess[entry] = callback;
}

bool	TCPSocketManager::SendPacket(int _tag, ByteBuffer *packet)
{
	if (packet == nullptr)
		return false;

    std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
    for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
    {
        if ((*iter)->getTagID() == _tag)
        {
            (*iter)->SendMsg((void *)packet->contents(), packet->size());
            return (*iter)->Flush();
        }
    }

    return false;
}

bool	TCPSocketManager::SendPacket(int _tag, void *packet,int length)
{
	if (packet == nullptr)
		return false;

	std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
	for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
	{
		if ((*iter)->getTagID() == _tag)
		{
			(*iter)->SendMsg(packet, length);
			return (*iter)->Flush();
		}
	}

	return false;
}



void TCPSocketManager::ResBuffOut()
{
	std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
	for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
	{
		(*iter)->ResOutBuff();
		
	}
}

void	TCPSocketManager::disconnect(int _tag)
{
    std::list<TCPSocket*>::iterator iter, enditer = m_lstSocket.end();
    for (iter = m_lstSocket.begin(); iter != enditer; ++iter)
    {
        if ((*iter)->getTagID() == _tag)
        {
            (*iter)->Destroy();
            if (_OnDisconnect){
                _OnDisconnect(_tag);
            }
            break;
        }
    }
}

