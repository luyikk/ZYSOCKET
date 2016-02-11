using System;
using System.Collections.Generic;
using System.Text;

namespace ZYSocket.ZYCoroutinesin
{
    public class Miss<SET,RES>:ZYSocket.ZYCoroutinesin.Coroutinesin<SET,RES>
    {

        public Action<Miss<SET, RES>, SET> Target { get; private set; }

        public static SET defset { get; set; }
        public static RES defres { get; set; }

        static Miss()
        {
           
        }


        public Miss()
            : base(defset, defres)
        {

        }

        public Miss(Action<Miss<SET, RES>, SET> action)
            : base(defset, defres)
        {
            Target = action;
        }


        public static Miss<SET, RES> NewMiss(Action<Miss<SET, RES>, SET> action)
        {
            return new Miss<SET, RES>(action);
        }

        public override void Run(SET arg)
        {
            if (Target != null)
                Target(this, arg);
        }

        public SET Give(RES obj)
        {
           return  base.yield(obj);

        }

        public SET Give()
        {
            return base.yield(defres);
        }

        public RES Set()
        {
            return base.Resume();
        }


        public RES Set(SET set)
        {
            return base.Resume(set);
        }
        
    }
}
