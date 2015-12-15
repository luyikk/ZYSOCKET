# coding=gb2312
import sys;
import SocketServer;
import struct;
from ZYHandler import *;

class MyHandler(PackHandler):
    def Connect(self, address):
        print str(address)+" Connect";
        return True;

    def Disconnect(self, address):         
         print str(address)+" Disconnect";

    def DataIn(self, data):
        str=data.decode("gb2312");
        print str;
        self.request.send(data);
       

myServer=SocketServer.ThreadingTCPServer(("",788),MyHandler);
myServer.serve_forever();
