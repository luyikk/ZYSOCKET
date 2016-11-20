import clr;
clr.AddReference("ZYSocketShare");
from ZYSocket.share import  *;
from System import *

class UserInfo(Object): 
  
    def __init__(self,asyn,size):
        self.Stream =ZYNetBufferReadStreamV2(size);
        self.Asyn=asyn;

#    def __init__(self,**kw):     
#        super(UserInfo,self).__init__(**kw);



       

  



