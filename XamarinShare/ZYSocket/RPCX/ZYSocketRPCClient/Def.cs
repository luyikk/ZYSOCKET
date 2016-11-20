using System;
using System.Collections.Generic;
using System.Text;

public delegate RES CALLFUN<out RES>();
public delegate RES CALLFUN<T1, out RES>(T1 arg1);
public delegate RES CALLFUN<T1, T2, out RES>(T1 arg1, T2 arg2);
public delegate RES CALLFUN<T1, T2, T3, out RES>(T1 arg1, T2 arg2, T3 arg3);
public delegate RES CALLFUN<T1, T2, T3, T4, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, T9, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
public delegate RES CALLFUN<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, out RES>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);


public delegate void CALLACTION();
public delegate void CALLACTION<T1>(T1 arg1);
public delegate void CALLACTION<T1, T2>(T1 arg1, T2 arg2);
public delegate void CALLACTION<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
public delegate void CALLACTION<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
public delegate void CALLACTION<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
public delegate void CALLACTION<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);


[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class RPCTAG : Attribute
{
    public string Tag { get; set; }

    public RPCTAG(string tag)
    {
        this.Tag = tag;
    }
}




[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class RPCMethod : Attribute
{
    public string Name { get; set; }


    public RPCMethod()
    {
        Name = null;
    }

    public RPCMethod(string name)
    {
        Name = name;
    }

}

