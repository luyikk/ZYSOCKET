# coding=utf-8
import sys;
import clr;
clr.AddReferenceToFileAndPath("..\..\北风之神SOCKET框架(ZYSocket)DLL\ZYSocketShare.dll");
clr.AddReferenceToFileAndPath("..\..\北风之神SOCKET框架(ZYSocket)DLL\ZYSocketClientB.dll");
clr.AddReference("ZYSocketShare");
clr.AddReference("ZYSocketClientB");
from ZYSocket.share import  *;
from System import *;
from ZYSocket.ClientB import *;




class SocketManager:
    def __init__(self):
        self.Stream=ZYNetBufferReadStreamV2(1024*1024);
        self.client=SocketClient();
        self.client.BinaryInput+=self.BinaryInput;
        self.client.ErrorLogOut+=self.ErrorLogOut;
        self.client.MessageInput+=self.MessageInput;
        self.IsConnent=False;
        self.DataOn=None;
        self.Disconnet=None;
        pass;


    def MessageInput(self,message):
        self.IsConnent=False;
        if self.Disconnet != None:
            self.Disconnet(message);
        pass;

    def ErrorLogOut(self,msg):
        print msg;
        pass;

    def BinaryInput(self,data):
        try:
            if self.DataOn!= None:
                self.Stream.Write(data);
                isR,pdata=self.Stream.Read();

                while isR:
                    self.DataOn(pdata);
                    isR,pdata=self.Stream.Read();
        except Exception,e:
            print e.ToString();

        pass;

    def Connent(self,host,port):
        if self.IsConnent == False:
            self.IsConnent=self.client.Connect(host,port);
            return self.IsConnent;
        else:
            return True;

    def StartRead(self):
        self.client.StartRead();
        pass;

    def Send(self,data):
        self.client.Send(data);
        pass;

