# coding=utf-8
import sys;
import clr;
clr.AddReferenceToFileAndPath("..\..\北风之神SOCKET框架(ZYSocket)DLL\ZYSocketFrame.dll");
clr.AddReferenceToFileAndPath("..\..\北风之神SOCKET框架(ZYSocket)DLL\ZYSocketShare.dll");
clr.AddReference("ZYSocketFrame");
clr.AddReference("ZYSocketShare");
from ZYSocket.Server import  *;
from System import *;
from System.Net.Sockets import SocketAsyncEventArgs;
from ZYSocket.share import  *;


server = ZYSocketSuper("127.0.0.1",9982,10000,4096);

def send(socketAsync,data):
    server.SendData(socketAsync.AcceptSocket,data);
    pass;
