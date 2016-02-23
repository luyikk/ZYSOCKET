using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


[RPCTAG("IClient")]
public class ClientCall
{

    public event CALLACTION<float,float,float> SetPostionEvent;
    [RPCMethod]
    public void SetPostion(float x,float y,float z)
    {
        if (SetPostionEvent != null)
            SetPostionEvent(x,y,z);
    }

}

