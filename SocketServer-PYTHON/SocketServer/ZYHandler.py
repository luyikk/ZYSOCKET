import logging;
from SocketServer import  BaseRequestHandler;
from socket import *;


class PackHandler(BaseRequestHandler):

    def LogTemplate(self,s):
        return '[IP.'+str(self.client_address)+']:'+str(s);
    def Log(self,s):
        ss=self.LogTemplate(s);
        print ss;
        logging.info(ss);
    def LogErr(self,s):
        ss=self.LogTemplate(s);
        print ss;
        logging.error(ss);


    def DataIn(self,data):
        pass;

    def Connect(self,address):
        return True;

    def Disconnect(self,address):
        pass;

    def handle(self):
       while 1:
           try:
               dataReceived=self.request.recv(1024);
               if not dataReceived:
                   break;
               else:
                   self.DataIn(dataReceived);
           except Exception,e:
               print e;
               break;


    def setup(self):
        self.Log(" is Connect");
        if not self.Connect(self.client_address):
           self.request.close();

    def finish(self):
        self.Log(" is Disconnect");






