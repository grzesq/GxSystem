using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace GxNolApi
{
    public class Message : IMessage
    {
        public Message(MessagegType msgType, string txt, object msg)
        {
            MsgType = msgType;
            MsgTxt = txt;
            MsgObj = msg;
        }

        public Message(MessagegType msgType, string txt)
        {
            MsgType = msgType;
            MsgTxt = txt;
            MsgObj = null;
        }

        public MessagegType MsgType { get; private set; }
        public object MsgObj { get; private set; }
        public string MsgTxt { get; private set; }
    }

    public class SendMessage
    {
        private static Subject<IMessage> message = new Subject<IMessage>();

        private static object _lock = new object();

        public static void Send(MessagegType msgType, string txt)
        {
            lock (_lock)
            {
                IMessage m = new Message(msgType, txt);
                message.OnNext(m);
            }
        }

        public static void Send(MessagegType msgType , string txt, object obj)
        {
            lock (_lock)
            {
                IMessage m = new Message(msgType, txt, obj);
                message.OnNext(m);
            }
        }

        public static IObservable<IMessage> Message
        {  
            get { return message; }
        }
    }
}
        
