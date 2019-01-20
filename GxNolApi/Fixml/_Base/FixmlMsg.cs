using System;
using System.Text;
using System.Net.Sockets;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public class FixmlMsg
    {
        public enum FixmlMsgType
        {
            Blank,
            UserRespons,
            BizMessageReject,
            StatementMsg,
            ExecutionReportMsg,
            SessionStatusMsg,
            MarketDataIncRefMsg,
            MarketDataFullRefMsg,
            MarketDataRejMsg,
            NewsMsg,
        }

        // reprezentuje cały dokument XML
        protected XDocument xmlDoc;
        protected XElement xml;
        protected virtual void PrepareXml() { }
        protected virtual void PrepareXmlMessage(XElement element)
        {
            xmlDoc = new XDocument(
                new XElement("FIXML",
                    new XAttribute("v", "5.0"),
                    new XAttribute("r", "20080317"),
                    new XAttribute("s", "20080314")));
            xmlDoc.Root.Add(element);
        }


        public FixmlMsgType MsgType {get; protected set;}
        public string XMLName { get; private set; }

        // Konstruktor używany wewnętrznie dla komunikatów przychodzących - 
        // "opakowywuje" odebrany komunikat w inną, bardziej precyzyjną klasę pochodną. 
        protected FixmlMsg(FixmlMsg msg)
        {
            xmlDoc = msg.xmlDoc;
            ParseXmlMessage(null);
        }

        public FixmlMsg() {}

        public FixmlMsg(Socket socket)
        {
            string text = Receive(socket);
            if (text == "") throw new FixmlException("Received empty message!");
            xmlDoc = XDocument.Parse(text);
            XMLName = (xmlDoc.Root.FirstNode as XElement).Name.LocalName;
            ParseXmlMessage(null);
        }

        private string Receive(Socket socket)
        {
            byte[] size_buf = new byte[4];
            if (socket.Receive(size_buf) < size_buf.Length) throw new FixmlSocketException();
            byte[] data_buf = new byte[BitConverter.ToInt32(size_buf, 0)];
            if (socket.Receive(data_buf) < data_buf.Length) throw new FixmlSocketException();
            string text = Encoding.Default.GetString(data_buf);
            SendMessage.Send(MessagegType.InXml, text);
            return text.TrimEnd('\0');   // znak '\0' na końcu nam by tylko przeszkadzał (np. w okienku Output)
        }
        
        public void Send(Socket socket)
        {
            if (xmlDoc == null) PrepareXml();
            string text = xmlDoc.ToString() + '\0';   // NOL3 czasem głupieje, gdy zabraknie terminatora
            byte[] data_buf = Encoding.ASCII.GetBytes(text);
            byte[] size_buf = BitConverter.GetBytes(data_buf.Length);
            if (socket.Send(size_buf) < size_buf.Length) throw new FixmlSocketException();
            if (socket.Send(data_buf) < data_buf.Length) throw new FixmlSocketException();
            SendMessage.Send(MessagegType.OutXml, text);
        }

        protected virtual void ParseXmlMessage(string name)
        {
            if (xmlDoc.Root  == null) throw new FixmlException("XML data missing!");
            if (xmlDoc.Root.Name != "FIXML") throw new FixmlException("FIXML root element missing!");
            if (xmlDoc.Root.Attribute("v").Value != "5.0") throw new FixmlException("Unknown FIXML protocol version!");
            xml = xmlDoc.Root.FirstNode as XElement;
            if (xml == null) throw new FixmlException("Empty FIXML message!");
            if ((name != null) && (xml.Name != name))
            {
                if ((xml.Name == BizMessageRejectMsg.MsgName) && !(this is BizMessageRejectMsg))
                    throw new BizMessageRejectException(new BizMessageRejectMsg(this));
                //if ((xml.Name == MarketDataReqRejectMsg.MsgName) && !(this is MarketDataReqRejectMsg))
                   // throw new FixmlErrorMsgException(new MarketDataReqRejectMsg(this));
                throw new FixmlException("Unexpected FIXML message name: " + xml.Name);
            }

        }

        public override string ToString()
        {
            return string.Format("[{0}]", "");
        }
    }
}
