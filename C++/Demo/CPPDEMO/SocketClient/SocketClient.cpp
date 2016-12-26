// SocketClient.cpp : 定义控制台应用程序的入口点。
//

#include "stdafx.h"
#include "Socket/TCPSocket.h"

#include "..\Proto\C\TestData.Proto.pb.h"




int main()
{
	new TCPSocketManager();

	sSocketMgr.register_process(1000, [=](int tag, ByteBuffer& buffer)
	{
		int res;
		int64 l1;
		float f1;
		double d1;
		bool b1;
		bool b2;
		int16 sh1;
		int8 sb1;
		std::string str1;
		std::string str2;
		buffer >> res;
		buffer >> l1;
		buffer >> f1;
		buffer >> d1;
		buffer >> b1;
		buffer >> b2;
		buffer >> sh1;
		buffer >> sb1;
		buffer >> str1;
		buffer >> str2;

	
		printf("int:%d\r\n", res);
		printf("int64:%d\r\n", l1);
		printf("float:%f\r\n", f1);
		printf("double:%lf\r\n", d1);
		printf("bool:%d\r\n", b1);
		printf("bool:%d\r\n", b2);
		printf("int16:%d\r\n", sh1);
		printf("int8:%d\r\n", sb1);
		printf("str1:%s\r\n", U2G(str1).c_str());
		printf("str2:%s\r\n", U2G(str2).c_str());

		int32 size;
		buffer >> size;
		uint8 * data = (uint8*)malloc(size);
		buffer.Read(data, size);
		printf("data length:%d\r\n",size);
		free(data);

	
		auto obj = buffer.ReadObj<TestData::TestData>();

		printf("obj Id:%d\r\n", obj.id());
		printf("obj Data %s \r\n", obj.data()[0].c_str());
		printf("obj Data %s \r\n", obj.data()[1].c_str());
		printf("obj Data1 a:%d b:%d \r\n", obj.data2(0).a(), obj.data2(0).b());
		printf("obj Data2 a:%d b:%d \r\n", obj.data2(1).a(), obj.data2(1).b());

		printf("full close");
	});
	sSocketMgr.register_connect([=](int tag, bool IsConnect)
	{
		if (!IsConnect)
		{
			printf("Tag:%d Connect fail\r\n", tag);
		}
		else
		{
			printf("Tag:%d Connect OK\r\n", tag);

			ByteBuffer tmp(1000);
			tmp << 5;
			tmp << (int64)555555;
			tmp << (float)1.5f;
			tmp << (double)55555.6666;
			tmp << true;
			tmp << false;
			tmp << (int16)77;
			tmp << (int8)5;
			tmp << std::string("你好");
			tmp << std::string("SSSSSSSSSSSSS");
			uint8 x[] = { 1,2,3,4,5,6 };			
			tmp.WriteBytes(x, 6);


			TestData::TestData obj;
			obj.set_id(5);

			obj.add_data();
			obj.add_data();
			obj.set_data(0, "123123");
			obj.set_data(1, "333333");
			auto xrr= obj.add_data2();
			xrr->set_a(555);
			xrr->set_b(666);

			tmp.WriteObj<TestData::TestData>(obj);

			sSocketMgr.SendPacket(1, &tmp);

		}
	});

	sSocketMgr.register_disconnect([=](int tag)
	{
		printf("Tag:%d Disconnect\r\n",tag);

	});

	sSocketMgr.createSocket("127.0.0.1", 9982, 1);

	getchar();


    return 0;
}

