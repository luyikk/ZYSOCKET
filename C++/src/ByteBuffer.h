/****************************************************************************
 *
 * ByteBuffer Class
 *
 */

#ifndef _BYTEBUFFER_H
#define _BYTEBUFFER_H

#include <sstream>
#include <vector>
#include "DefType.h"
//#include "Vector3.h"



class  ByteBuffer
{
#define DEFAULT_SIZE 0x1000
#define DEFAULT_INCREASE_SIZE 200

    uint8* m_buffer;
    size_t m_readPos;
    size_t m_writePos;
    uint32 m_buffersize;

public:
    
    /** Creates a bytebuffer with the default size
     */
    ByteBuffer(int32 cmd)
    {
        m_buffer = 0;
        m_readPos  = 0;
		m_writePos = 4;
        m_buffersize = 0;
        reserve(DEFAULT_SIZE);
		this->Write<int32>(cmd);
    }

    /** Creates a bytebuffer with the specified size
     */
    ByteBuffer(size_t res)
    {
        m_buffer = 0;
        m_readPos = 0;
		m_writePos = 0;
        m_buffersize = 0;
        reserve(res);
		
    }

	


    /** Frees the allocated buffer
     */
    ~ByteBuffer()
    {
        free(m_buffer);
    }


    /** Allocates/reallocates buffer with specified size.
     */
    void reserve(size_t res)
    {
        if (m_buffer)
            m_buffer = (uint8*)realloc(m_buffer, res);
        else
            m_buffer = (uint8*)malloc(res);

        m_buffersize = res;
    }


    /** Resets read/write indexes
     */
    inline void clear()
    {
        m_readPos = m_writePos = 0;
    }

    /** Sets write position
     */
    inline void resize(size_t size)
    {
        m_writePos = size;
    }

    /** Returns the buffer pointer
     */
    inline const uint8 * contents()
    {
		uint32 length = m_writePos;
		m_writePos = 0;
		this->Write<uint32>(length);
		m_writePos = length;
        return m_buffer;
    }


    /** Gets the buffer size.
     */
    uint32 GetBufferSize() { return m_buffersize; }

    /* 以16进制文本呈现当前缓存区字节 */
    std::string ToString() {
        std::string str;
        for (size_t i = 0; i < this->size(); i++)
        {
            int c = m_buffer[i];
            char string[16];
            //_itoa(c, string, 16);
            sprintf(string, "%x", c);
            std::stringstream ret;
            ret << string;
            str += (c < 10 ? "0" : "") + ret.str() + " ";
        }
        return str;
    }

    /** Reads sizeof(T) bytes from the buffer
     * @return the bytes read
     */
    template<typename T>
    T Read()
    {
        if (m_readPos + sizeof(T) > m_writePos)
            return (T)0;
        T ret = *(T*)&m_buffer[m_readPos];
        m_readPos += sizeof(T);
        return ret;
    }

    void skip(size_t len)
    {
        if (m_readPos + len > m_writePos)
            len = (m_writePos - m_readPos);
        m_readPos += len;
    }


	template<typename T>
	T ReadObj()
	{
		int32 size = Read<int32>();
				
		uint8 * data = (uint8*)malloc(size);
		Read(data, size);
		
		T obj;
		obj.ParseFromArray(data, size);
		free(data);

		return obj;
	}

    /** Reads x bytes from the buffer
     */
    void Read(uint8 * buffer, size_t len)
    {
        if (m_readPos + len > m_writePos)
            len = (m_writePos - m_readPos);

        memcpy(buffer, &m_buffer[m_readPos], len);
        m_readPos += len;
    }

    /** Read const char* by len from the buffer
    */
    void Read(const char* *buffer, size_t len)
    {
        if (m_readPos + len > m_writePos)
            len = (m_writePos - m_readPos);

        memcpy(buffer, &m_buffer[m_readPos], len);
        m_readPos += len;
    }

    /** Writes sizeof(T) bytes to the buffer, while checking for overflows.
     * @param T data The data to be written
     */
    template<typename T>
    void Write(const T & data)
    {
        size_t  new_size = m_writePos + sizeof(T);
        if (new_size > m_buffersize)
        {
            new_size = (new_size / DEFAULT_INCREASE_SIZE + 1) * DEFAULT_INCREASE_SIZE;
            reserve(new_size);
        }

        //*(T*)&m_buffer[m_writePos] = data;
        memcpy(&m_buffer[m_writePos], &data, sizeof(T));
        m_writePos += sizeof(T);
    }

    /** writes x bytes to the buffer, while checking for overflows
     * @param ptr the data to be written
     * @param size byte count
     */
    void Write(const uint8 * data, size_t size)
    {
        size_t  new_size = m_writePos + size;
        if (new_size > m_buffersize)
        {
            new_size = (new_size / DEFAULT_INCREASE_SIZE + 1) * DEFAULT_INCREASE_SIZE;
            reserve(new_size);
        }

        memcpy(&m_buffer[m_writePos], data, size);
        m_writePos += size;
    }

	void WriteBytes(const uint8 * data, uint32 size)
	{
		Write<uint32>(size);
		Write(data, size);
	}

	template<typename T>
	void WriteObj(T obj)
	{
		std::ostringstream stream;
		obj.SerializeToOstream(&stream);
		std::string text = stream.str();
		const char* ctext = text.c_str();
		auto size = obj.ByteSize();
		Write<uint32>(size);
		Write(ctext, size);
	}

    void Write(const char * data, size_t size)
    {
        size_t  new_size = m_writePos + size;
        if (new_size > m_buffersize)
        {
            new_size = (new_size / DEFAULT_INCREASE_SIZE + 1) * DEFAULT_INCREASE_SIZE;
            reserve(new_size);
        }

        memcpy(&m_buffer[m_writePos], data, size);
        m_writePos += size;
    }

    /** Ensures the buffer is big enough to fit the specified number of bytes.
     * @param bytes number of bytes to fit
     */
    inline void EnsureBufferSize(uint32 Bytes)
    {
        size_t  new_size = m_writePos + Bytes;
        if (new_size > m_buffersize)
        {
            new_size = (new_size / DEFAULT_INCREASE_SIZE + 1) * DEFAULT_INCREASE_SIZE;
            reserve(new_size);
        }

    }

    /** These are the default read/write operators.
     */
#define DEFINE_BUFFER_READ_OPERATOR(type) void operator >> (type& dest) { dest = Read<type>(); }
#define DEFINE_BUFFER_WRITE_OPERATOR(type) void operator << (const type src) { Write<type>(src); }

    /** Fast read/write operators without using the templated read/write functions.
     */
#define DEFINE_FAST_READ_OPERATOR(type, size) \
         ByteBuffer& operator >> (type& dest) \
         { \
           if(m_readPos + size > m_writePos) \
           { \
	         dest = (type)0; return *this; \
		   } \
		   else \
	       { \
		       memcpy(&dest, &m_buffer[m_readPos], size); \
				   /*dest = *(type*)&m_buffer[m_readPos];*/ \
			   dest = EndianConvertBToL(dest); \
			   m_readPos += size; \
			   return *this; \
		   } } \

#define DEFINE_FAST_WRITE_OPERATOR(type, size) \
        ByteBuffer& operator << (const type src) \
	    { \
	        if(m_writePos + size > m_buffersize) \
            { \
	         reserve(m_buffersize + DEFAULT_INCREASE_SIZE); } \
             type n_src = EndianConvertLToB<type>(src); \
             memcpy(&m_buffer[m_writePos], &n_src, size);\
             /* *(type*)&m_buffer[m_writePos] = EndianConvertLToB<type>(src); */ \
             m_writePos += size; \
             return *this; \
	    }\

    /** Integer/float r/w operators
    */
    DEFINE_FAST_READ_OPERATOR(uint64, 8);
    DEFINE_FAST_READ_OPERATOR(uint32, 4);
    DEFINE_FAST_READ_OPERATOR(uint16, 2);
    DEFINE_FAST_READ_OPERATOR(uint8, 1);
    DEFINE_FAST_READ_OPERATOR(int64, 8);
    DEFINE_FAST_READ_OPERATOR(int32, 4);
    DEFINE_FAST_READ_OPERATOR(int16, 2);
    DEFINE_FAST_READ_OPERATOR(int8, 1);
    DEFINE_FAST_READ_OPERATOR(float, 4);
    DEFINE_FAST_READ_OPERATOR(double, 8);

    DEFINE_FAST_WRITE_OPERATOR(uint64, 8);
    DEFINE_FAST_WRITE_OPERATOR(uint32, 4);
    DEFINE_FAST_WRITE_OPERATOR(uint16, 2);
    DEFINE_FAST_WRITE_OPERATOR(uint8, 1);
    DEFINE_FAST_WRITE_OPERATOR(int64, 8);
    DEFINE_FAST_WRITE_OPERATOR(int32, 4);
    DEFINE_FAST_WRITE_OPERATOR(int16, 2);
    DEFINE_FAST_WRITE_OPERATOR(int8, 1);
    DEFINE_FAST_WRITE_OPERATOR(float, 4);
    DEFINE_FAST_WRITE_OPERATOR(double, 8);

    //DEFINE_FAST_READ_OPERATOR(double, 8);
   // ByteBuffer& operator >> (double& dest) { if (m_readPos + 4 > m_writePos) { dest = (double)0; return *this; } else { int temp; memcpy(&temp, &m_buffer[m_readPos], 4); temp = EndianConvertBToL(temp); dest = static_cast<double>(temp / 1000.0f);  m_readPos += 4; return *this; } }

    /** boolean (1-byte) read/write operators
     */
    DEFINE_FAST_WRITE_OPERATOR(bool, 1);
    ByteBuffer& operator >> (bool & dst) { dst = (Read<char>() > 0 ? true : false); return *this; }

//     /** string (null-terminated) operators
//      */
//     virtual ByteBuffer& operator << (const std::string & value) { EnsureBufferSize(value.length() + 1); memcpy(&m_buffer[m_writePos], value.c_str(), value.length() + 1); m_writePos += (value.length() + 1); return *this; }
//     virtual ByteBuffer& operator >> (std::string & dest)
//     {
//         dest.clear();
//         char c;
//         for (;;)
//         {
//             c = Read<char>();
//             if (c == 0) break;
//             dest += c;
//         }
//         return *this;
//     }

//     ByteBuffer& operator << (const int64 &num) {
//         if (m_writePos + 8 > m_buffersize)
//         {
//             reserve(m_buffersize + DEFAULT_INCREASE_SIZE);
//         }
//         int64 src = EndianConvertLToB<int64>(num);
//         memcpy(&m_buffer[m_writePos], &src, 8);
//         m_writePos += 8;
//         return *this;
//     }
// 
//     ByteBuffer& operator << (const uint64 &num) {
//         if (m_writePos + 8 > m_buffersize)
//         {
//             reserve(m_buffersize + DEFAULT_INCREASE_SIZE);
//         }
// //         *(uint32*)&m_buffer[m_writePos] = EndianConvertLToB<uint32>(num.second);
// //         *(uint32*)&m_buffer[m_writePos] = EndianConvertLToB<uint32>(num.first);
//         //*(uint64*)&m_buffer[m_writePos] = EndianConvertLToB<uint64>(num);
//         uint64 u64 = EndianConvertLToB<uint64>(num);
//         memcpy(&m_buffer[m_writePos], &u64, 8);
//         m_writePos += 8;
//         return *this;
//     }

//     ByteBuffer& operator >> (uint64 &num) {
//         if (m_readPos + 8 > m_writePos) 
//         { 
//             num = (uint64)0;
//             return *this;
//         }
//         else
//         {
// //             num.second = *(uint32*)&m_buffer[m_readPos];
// //             num.first = *(uint32*)&m_buffer[m_readPos];
// //             num.second = EndianConvertBToL(num.second);
// //             num.first = EndianConvertBToL(num.first);
//             m_readPos += 8;
//             return *this;
//         }
//     }


	

    // override write string
    ByteBuffer& operator << (const std::string& str) {
        uint32 len = (uint32)str.size();

		const char * stxxr = str.c_str();

        *this << len;
        if (len > 0)
            Write((const uint8 *)str.c_str(), len);
        return *this;
    }

    // override read string
    ByteBuffer& operator >> (std::string &value) {
        value.clear();
        uint32 len;
        *this >> len;


        char c;
        for (int i = 0; i < len; i++)
        {
            c = Read<char>();
			value += c;
        }
        return *this;
    }

    // write another ByteBuffer
    ByteBuffer& operator << (ByteBuffer& buffer) {
        Write(buffer.contents(), buffer.size());
        return *this;
    }

    /** Gets the write position
     * @return buffer size
     */
    inline size_t size() { return m_writePos; }

    /** read/write position setting/getting
     */
    inline size_t rpos() { return m_readPos; }
    inline size_t wpos() { return m_writePos; }
    inline void rpos(size_t p) { assert(p <= m_writePos); m_readPos = p; }
    inline void wpos(size_t p) { assert(p <= m_buffersize); m_writePos = p; }

    template<typename T> size_t writeVector(std::vector<T> &v)
    {
        for (typename std::vector<T>::const_iterator i = v.begin(); i != v.end(); i++) {
            Write<T>(*i);
        }
        return v.size();

    }
    template<typename T> size_t readVector(size_t vsize, std::vector<T> &v)
    {

        v.clear();
        while (vsize--) {
            T t = Read<T>();
            v.push_back(t);
        }
        return v.size();
    }

    template<typename T> size_t writeList(std::list<T> &v)
    {
        for (typename std::list<T>::const_iterator i = v.begin(); i != v.end(); i++) {
            Write<T>(*i);
        }
        return v.size();

    }
    template<typename T> size_t readList(size_t vsize, std::list<T> &v)
    {

        v.clear();
        while (vsize--) {
            T t = Read<T>();
            v.push_back(t);
        }
        return v.size();
    }

    template <typename K, typename V> size_t writeMap(const std::map<K, V> &m)
    {
        for (typename std::map<K, V>::const_iterator i = m.begin(); i != m.end(); i++) {
            Write<K>(i->first);
            Write<V>(i->second);
        }
        return m.size();
    }

    template <typename K, typename V> size_t readMap(size_t msize, std::map<K, V> &m)
    {
        m.clear();
        while (msize--) {
            K k = Read<K>();
            V v = Read<V>();
            m.insert(make_pair(k, v));
        }
        return m.size();
    }





    template<typename T>
    T EndianConvertLToB(T num)
    {
#ifdef MINHAND
		return num;
#else
        if (sizeof(T) == 2) 
        {
            return htons(num);
        }
        else if (sizeof(T) == 4)
        {
            return htonl(num);
        }
        else if (sizeof(T) == 8)
        {
            return htonll(num);
        }
        else return num;
#endif
    }

    template<typename T>
    T EndianConvertBToL(T num) 
    {
#ifdef MINHAND
		return num;
#else
        if (sizeof(T) == 2)
        {
            return ntohs(num);
        }
        else if (sizeof(T) == 4)
        {
            return ntohl(num);
        }
        else if (sizeof(T) == 8)
        {
            return ntohll(num);
        }
        else return num;
#endif
    }
    
#if PLATFORM != PLATFORM_APPLE
    unsigned long long ntohll(unsigned long long val)
    {
        return (((unsigned long long)htonl((int)((val << 32) >> 32))) << 32) | (unsigned int)htonl((int)(val >> 32));
    }

    unsigned long long htonll(unsigned long long val)
    {
        return (((unsigned long long)htonl((int)((val << 32) >> 32))) << 32) | (unsigned int)htonl((int)(val >> 32));
    }
#endif
};

///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////////////////////////

//template <typename T> ByteBuffer &operator<<(ByteBuffer &b, const std::vector<T> & v)
//{
//    b << (uint32)v.size();
//    for (typename std::vector<T>::const_iterator i = v.begin(); i != v.end(); i++) {
//        b << *i;
//    }
//    return b;
//}

//template <typename T> ByteBuffer &operator>>(ByteBuffer &b, std::vector<T> &v)
//{
//    uint32 vsize;
//    b >> vsize;
//    v.clear();
//    while (vsize--) {
//        T t;
//        b >> t;
//        v.push_back(t);
//    }
//    return b;
//}

template<typename T>
ByteBuffer &operator >> (ByteBuffer &b, std::vector<T> &v)
{
    uint8 has;
    b >> has;
    if (has == 0)
    {
        short count;
        b >> count;
		for (auto i = 0; i < count; i++)
		{
			T t;
			b >> t;
			v.push_back(t);
		}
    }
    return b;
}

template <typename K, typename V>
ByteBuffer &operator >> (ByteBuffer &b, std::map<K, V> &m)
{
    uint8 has;
    b >> has;
    if (has == 0)
    {
        short count;
        b >> count;
		for (auto i = 0; i < count; i++)
		{
			K k;
			V v;
			b >> k >> v;
			m[k] = v;
		}
    }
    return b;
}

//template <typename T> ByteBuffer &operator<<(ByteBuffer &b, const std::list<T> & v)
//{
//    b << (uint32)v.size();
//    for (typename std::list<T>::const_iterator i = v.begin(); i != v.end(); i++) {
//        b << *i;
//    }
//    return b;
//}
//
//template <typename T> ByteBuffer &operator>>(ByteBuffer &b, std::list<T> &v)
//{
//    uint32 vsize;
//    b >> vsize;
//    v.clear();
//    while (vsize--) {
//        T t;
//        b >> t;
//        v.push_back(t);
//    }
//    return b;
//}
//
//template <typename K, typename V> ByteBuffer &operator<<(ByteBuffer &b, const std::map<K, V> &m)
//{
//    b << (uint32)m.size();
//    for (typename std::map<K, V>::const_iterator i = m.begin(); i != m.end(); i++) {
//        b << i->first << i->second;
//    }
//    return b;
//}

#endif
