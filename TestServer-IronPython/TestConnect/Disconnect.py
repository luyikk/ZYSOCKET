# coding=utf-8
import sys;
import clr;
import Init;
from System import *;
from Init import *;


def Disconn(message, socketAsync, erorr):
    print message;
    socketAsync.UserToken=None;
    socketAsync.AcceptSocket.Close();
    pass;

server.MessageInput=Disconn;