# coding=utf-8
import sys;
import clr;
import Init;
from System import *;
from Init import *;



def Connection(socketAsync):
    print "User conn:"+socketAsync.AcceptSocket.RemoteEndPoint.ToString();
    return True;


server.Connetions=Connection;
