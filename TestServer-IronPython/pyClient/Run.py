# coding=utf-8
import sys;
import clr;
import SocketClientManger;
from System import *;
from System.Text import *;
from SocketClientManger import *;
from System.Collections.Generic import *;
clr.AddReference("ZYSocketShare");
from ZYSocket.share import  *;


def DataOn(data):
    pass;

def Disconnet(msg):
    pass;



client=SocketManager();
client.DataOn=DataOn;
client.Disconnet=Disconnet;
if client.Connent("127.0.0.1",9982):
    while 1:
        Console.ReadLine();        
        buff= BufferFormatV2(1000);
        buff.AddItem(1);
        buff.AddItem("测试");
        buff.AddItem(Guid.NewGuid());
        client.Send(buff.Finish());





