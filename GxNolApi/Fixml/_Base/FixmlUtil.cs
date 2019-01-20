using System;
using System.Globalization;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    /// <summary>
    /// Wewnętrzne funkcje pomocnicze. Głównie do parsowania parametrów FIXML'a. 
    /// </summary>
    internal static class FixmlUtil
	{

		internal static void Error(XElement xml, string name, object value, string msg)
		{
			msg = "{0}, attribute {1} " + ((value != null) ? "= '{2}' " : "") + msg;
			Exception error = new FixmlException(string.Format(msg, xml.Name, name, value));
            SendMessage.Send(MessagegType.LogE, "Xml Msg : " + xml.ToString());
			throw error;
		}

        internal static string ReadString(XElement xml, string name)
        {
            if (xml == null) return null;
            string v  = "";
            if (xml.Attribute(name) != null)
                v = xml.Attribute(name).Value;
            return v;
        }


        internal static char ReadChar(XElement xml, string name)
        {
            string str = ReadString(xml, name);
            if (String.IsNullOrEmpty(str))
            {
                Error(xml, name, str, "- single character expected");
                return '\0';
            }
            else
            { 
                char ch;
                if (!char.TryParse(str, out ch))
                    Error(xml, name, str, "- single character expected");
                return ch;
            }
        }

        internal static char? ReadCharO(XElement xml, string name)
        {
            string str = ReadString(xml, name);
            if (String.IsNullOrEmpty(str)) return null;
            char ch;
            if (!char.TryParse(str, out ch))
                Error(xml, name, str, "- single character expected");
            return ch;
        }


        internal static decimal ReadDecimal(XElement xml, string name)
        {
            decimal v = -1;
            if (xml.Attribute(name) != null)
                v = Convert.ToDecimal(xml.Attribute(name).Value);
            else
                Error(xml, name, v, "- not valid decimal");
            return v;
        }

        internal static decimal? ReadDecimalO(XElement xml, string name)
        {
            if (xml == null) return null;
            decimal? v = null;
            if (xml.Attribute(name) != null)
                v = Convert.ToDecimal(xml.Attribute(name).Value);
            return v;
        }


        internal static int ReadInt(XElement xml, string name)
        {
            int v = -1;
            if (xml.Attribute(name) != null)
                v = Convert.ToInt32(xml.Attribute(name).Value);
            else
                Error(xml, name, v, "- not valid int");
            return v;
        }

        internal static int? ReadIntO(XElement xml, string name)
        {
            if (xml == null) return null;
            int? v = null;
            if (xml.Attribute(name) != null)
                v = Convert.ToInt32(xml.Attribute(name).Value);
            return v;
        }


        internal static uint ReadUInt(XElement xml, string name)
        {
            uint v = 0;
            if (xml.Attribute(name) != null)
                v = Convert.ToUInt32(xml.Attribute(name).Value);
            else
                Error(xml, name, v, "- not valid uint");
            return v;
        }

        internal static uint? ReadUIntO(XElement xml, string name)
        {
            if (xml == null) return null;
            uint? v = null;
            if (xml.Attribute(name) != null)
                v = Convert.ToUInt32(xml.Attribute(name).Value);
            return v;
        }


        internal static DateTime ReadDateTime(XElement xml, string name)
        {
            return (DateTime)ReadDateTimeC(xml, name, null);
        }

        internal static DateTime? ReadDateTimeO(XElement xml, string name)
        {
            return ReadDateTimeC(xml, name, null);
        }

        internal static DateTime? ReadDateTimeO(XElement xml, string name, string name2)
        {
            return ReadDateTimeC(xml, name, name2);
        }

        internal static DateTime? ReadDateTimeC(XElement xml, string name, string name2)
        {
            string str = ReadString(xml, name);
            if (String.IsNullOrEmpty(str)) return null;
            if (name2 != null)
                str += "-" + ReadString(xml, name2);
            DateTime date;
            string[] formats = new[] { "yyyyMMdd-HH:mm:ss", "yyyyMMdd-HH:mm", "yyyyMMdd" };
            IFormatProvider provider = CultureInfo.InvariantCulture;
            DateTimeStyles styles = DateTimeStyles.None;
            if (!DateTime.TryParseExact(str, formats, provider, styles, out date))
                Error(xml, (name2 == null ? name : name + "+" + name2), str, "- not a valid date/time");
            return date;
        }

        internal static string WriteDate(DateTime date)
        {
            return date.ToString("yyyyMMdd");
        }

        internal static string WriteDateTime(DateTime dt)
        {
            return dt.ToString("yyyyMMdd-HH:mm:ss");
        }

        internal static string WriteDecimal(decimal? value)
        {
            return ((decimal)value).ToString("0.00", CultureInfo.InvariantCulture);
        }


        /*
                internal static string ReadString(XmlElement xml, string name)
                {
                    return ReadString(xml, name, false);
                }

                internal static string ReadString(XmlElement xml, string name, bool optional)
                {
                    int i = name.LastIndexOf('/');
                    XmlElement elem = (i < 0) ? xml : Element(xml, name.Substring(0, i), optional);
                    if (elem == null) return null;
                    string value = elem.GetAttribute(name.Substring(i + 1));
                    if (value != "") return value;
                    if (!optional) Error(xml, name, null, "missing");
                    return null;
                }


                internal static char ReadChar(XmlElement xml, string name)
                {
                    return (char)ReadChar(xml, name, false);
                }

                internal static char? ReadChar(XmlElement xml, string name, bool optional)
                {
                    string str = ReadString(xml, name, optional);
                    if (str == null) return null;
                    char ch;
                    if (!char.TryParse(str, out ch))
                        Error(xml, name, str, "- single character expected");
                    return ch;
                }


                internal static int ReadInt(XmlElement xml, string name)
                {
                    return (int)ReadInt(xml, name, false);
                }

                internal static int? ReadInt(XmlElement xml, string name, bool optional)
                {
                    string str = ReadString(xml, name, optional);
                    if (str == null) return null;
                    int number;
                    if (!int.TryParse(str, out number))
                        Error(xml, name, str, "- not a valid number");
                    return number;
                }


                internal static uint ReadUInt(XmlElement xml, string name)
                {
                    return (uint)ReadUInt(xml, name, false);
                }

                internal static uint? ReadUInt(XmlElement xml, string name, bool optional)
                {
                    int? number = ReadInt(xml, name, optional);
                    if (number == null) return null;
                    if (number < 0)
                        Error(xml, name, number.ToString(), "- unexpected negative number");
                    return (uint)number;
                }


                internal static decimal ReadDecimal(XmlElement xml, string name)
                {
                    return (decimal)ReadDecimal(xml, name, false);
                }

                internal static decimal? ReadDecimal(XmlElement xml, string name, bool optional)
                {
                    string str = ReadString(xml, name, optional);
                    if (str == null) return null;
                    decimal value;
                    NumberFormatInfo format = new NumberFormatInfo() { NumberGroupSeparator = " " };
                    if (!decimal.TryParse(str, NumberStyles.Number, format, out value))
                        Error(xml, name, str, "- not a valid decimal number");
                    return value;
                }

                internal static string WriteDecimal(decimal? value)
                {
                    return ((decimal)value).ToString("0.00", CultureInfo.InvariantCulture);
                }



                internal static DateTime ReadDateTime(XmlElement xml, string name)
                {
                    return ReadDateTime(xml, name, null);
                }

                internal static DateTime? ReadDateTime(XmlElement xml, string name, bool optional)
                {
                    return ReadDateTime(xml, name, null, optional);
                }

                internal static DateTime ReadDateTime(XmlElement xml, string name, string name2)
                {
                    return (DateTime)ReadDateTime(xml, name, name2, false);
                }

                internal static DateTime? ReadDateTime(XmlElement xml, string name, string name2, bool optional)
                {
                    string str = ReadString(xml, name, optional);
                    if (str == null) return null;
                    if (name2 != null)
                        str += "-" + ReadString(xml, name2, false);
                    DateTime date;
                    string[] formats = new[] { "yyyyMMdd-HH:mm:ss", "yyyyMMdd-HH:mm", "yyyyMMdd" };
                    IFormatProvider provider = CultureInfo.InvariantCulture;
                    DateTimeStyles styles = DateTimeStyles.None;
                    if (!DateTime.TryParseExact(str, formats, provider, styles, out date))
                        Error(xml, (name2 == null ? name : name + "+" + name2), str, "- not a valid date/time");
                    return date;
                }

                internal static string WriteDate(DateTime date)
                {
                    return date.ToString("yyyyMMdd");
                }

                internal static string WriteDateTime(DateTime dt)
                {
                    return dt.ToString("yyyyMMdd-HH:mm:ss");
                }
                */
    }
}
