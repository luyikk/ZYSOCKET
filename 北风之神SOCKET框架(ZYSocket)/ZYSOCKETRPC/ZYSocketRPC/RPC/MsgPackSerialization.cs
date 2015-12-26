using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MsgPack.Serialization;

namespace ZYSocket.RPC
{
    public static class MsgPackSerialization
    {


        public static Dictionary<Type, IMessagePackSingleObjectSerializer> MsgPackSerializationObj { get; set; }

        static MsgPackSerialization()
        {
            MsgPackSerializationObj = new Dictionary<Type, IMessagePackSingleObjectSerializer>();
        }

        public static IMessagePackSingleObjectSerializer GetMsgPack(Type type)
        {
            if (MsgPackSerializationObj.ContainsKey(type))
            {
                return MsgPackSerializationObj[type];
            }
            else
            {
                var tmp= MsgPack.Serialization.SerializationContext.Default.GetSerializer(type);

                MsgPackSerializationObj[type] = tmp;

                return tmp;
            }
        }
    }
}
