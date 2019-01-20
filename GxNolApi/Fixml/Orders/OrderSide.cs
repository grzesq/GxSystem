using System;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
	public enum OrderSide
	{
		Buy = '1',
		Sell = '2'
	}

	internal static class OrderSideUtil
	{
		public static OrderSide Read(XElement xml, string name)
		{
			char ch = FixmlUtil.ReadChar(xml, name);
			if (!Enum.IsDefined(typeof(OrderSide), (OrderSide)ch))
				FixmlUtil.Error(xml, name, ch, "- unknown OrderSide");
			return (OrderSide)ch;
		}

		public static string Write(OrderSide? value)
		{
			return ((char)value).ToString();
		}
	}
}
