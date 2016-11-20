# coding=utf-8
import sys;
import clr;
clr.AddReference("ZYSocketShare");
import Init;
from System import *;
from System.Collections.Generic import *;
from System.Net.Sockets import *;
from Init import *;
from UserInfo import *;
from ZYSocket.share import  *;



def PackIn(data,userInfo):
    read= ReadBytesV2(data);
    r1,lengt=read.ReadInt32();
    r2,cmd=read.ReadInt32();
    if r1 and r2 and lengt==read.Length:       
        if cmd == 1000:
            r1,Id=read.ReadInt32();
            r2,msg=read.ReadString();
            r3,guid=read.ReadObject[Guid]();
            if r1 and r2 and r3:
                print Id.ToString()+":"+msg+":"+guid.ToString();
                pass;


    pass;


def DataOn(data, socketAsync): 
     if socketAsync.UserToken== None:
         socketAsync.UserToken=UserInfo(socketAsync,1024*1024);   
     stream=socketAsync.UserToken.Stream;      
     stream.Write(data); 
     a,pdata= stream.Read()
     while a:
         PackIn(pdata,socketAsync.UserToken);
         a,pdata= stream.Read()

server.BinaryInput=DataOn;

