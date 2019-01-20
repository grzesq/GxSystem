using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GxNolApi.NolClient.Fixml
{
    public enum StatementFundType
    {
        Cash,                   // gotówka do dyspozycji (konto zwykłe)
        CashBlocked,            // gotówka blokowana pod zlecenia
        CashReceivables,        // razem do dyspozycji (wolna gotówka + należności)
        Receivables,            // należności odblokowane, do dyspozycji
        ReceivablesBlocked,     // należności blokowane pod zlecenia
        SecuritiesValue,        // wartość papierów / wartość teoretyczna otwartych pozycji
        PortfolioValue,         // wartość całego portfela / wartość środków własnych
        Deposit,                // depozyt - razem
        DepositBlocked,         // depozyt - zablokowany
        DepositFree,            // depozyt - do dyspozycji
        DepositDeficit,         // depozyt - do uzupełnienia
        DepositSurplus,         // depozyt - nadwyżka (ponad minimalny depozyt)
        SecSafeties,            // zabezpieczenie pap.wart. - ogółem
        SecSafetiesUsed,        // zabezpieczenie pap.wart. - wykorzystane
        Loans,                  // kredyty - aktualne zaangażowanie
        Liabilities,            // zobowiązania wobec DM BOŚ - łączne (P+T)
        LiabilitiesP,           // zobow. wobec DM BOŚ - typu P (zabezpieczone)
        LiabilitiesT,           // zobow. wobec DM BOŚ - typu T (niezabezpieczone)
        LiabilitiesLimitMax,    // kwota, o jaką można zwiększyć limit należności
        LiabilitiesPLimitMax,   // o ile można zwiększyć limit nal. typu P (zabezp.)
        LiabilitiesTLimitMax,   // o ile można zwiększyć limit nal. typu T (niezabezp.)
        MaxBuy,                 // maksymalne kupno
        MaxOtpBuy,              // maksymalne kupno na Odroczony Termin Płatności
        MaxOtpPBuy,             // maksymalne kupno na OTP typu P (zabezpieczone)
        MaxOtpTBuy,             // maksymalne kupno na OTP typu T (niezabezpieczone)
        OptionBonus             // opcionaly bonus?
    }

    public class StatementData
	{
        private static Dictionary<string, StatementFundType> dict =
            new Dictionary<string, StatementFundType> {
                { "Cash",                   StatementFundType.Cash },
                { "Recivables",             StatementFundType.Receivables },
                { "CashRecivables",         StatementFundType.CashReceivables },
                { "CashBlocked",            StatementFundType.CashBlocked },
                { "RecivablesBlocked",      StatementFundType.ReceivablesBlocked },
                { "Loans",                  StatementFundType.Loans },
                { "Liabilities",            StatementFundType.Liabilities },
                { "LiabilitiesP",           StatementFundType.LiabilitiesP },
                { "LiabilitiesT",           StatementFundType.LiabilitiesT },
                { "MaxBuy",                 StatementFundType.MaxBuy },
                { "MaxOtpBuy",              StatementFundType.MaxOtpBuy },
                { "MaxOtpPBuy",             StatementFundType.MaxOtpPBuy },
                { "MaxOtpTBuy",             StatementFundType.MaxOtpTBuy },
                { "LiabilitiesLimitMax",    StatementFundType.LiabilitiesLimitMax },
                { "LiabilitiesPLimitMax",   StatementFundType.LiabilitiesPLimitMax },
                { "LiabilitiesTLimitMax",   StatementFundType.LiabilitiesTLimitMax },
                { "Deposit",                StatementFundType.Deposit },
                { "BlockedDeposit",         StatementFundType.DepositBlocked },
                { "SecSafeties",            StatementFundType.SecSafeties },
                { "SecSafetiesUsed",        StatementFundType.SecSafetiesUsed },
                { "FreeDeposit",            StatementFundType.DepositFree },
                { "DepositSurplus",         StatementFundType.DepositSurplus },
                { "DepositDeficit",         StatementFundType.DepositDeficit },
                { "SecValueSum",            StatementFundType.SecuritiesValue },
                { "PortfolioValue",         StatementFundType.PortfolioValue },
                { "OptionBonus",            StatementFundType.OptionBonus },
            };

        public class PosQuantity
		{
			public int Acc110 { get; private set; }
			public int Acc120 { get; private set; }
			internal PosQuantity(int acc110, int acc120)
			{
				Acc110 = acc110;
				Acc120 = acc120;
			}
		}

		public string AccountNumber { get; private set; }
		public Dictionary<FixmlInstrument, PosQuantity> Positions { get; private set; }
		public Dictionary<StatementFundType, decimal> Funds { get; private set; }

		public StatementData(XElement xml)
		{
			AccountNumber = xml.Attribute("Acct").Value;

			Funds = new Dictionary<StatementFundType, decimal>();
			Positions = new Dictionary<FixmlInstrument, PosQuantity>();

            foreach (XElement elem in xml.Descendants("Fund"))
            {
				StatementFundType key;
                string str = FixmlUtil.ReadString(elem, "name");
                if (!dict.ContainsKey(str))
                    throw new FixmlException(string.Format("Unknown StatementFundType: '{0}'", str));
                key = dict[str];

                decimal value = FixmlUtil.ReadDecimal(elem, "value");
				Funds.Add(key, value);
			}

			foreach (XElement elem in xml.Descendants("Position"))
			{
                XElement inst = elem.FirstNode as XElement;
                FixmlInstrument key = FixmlInstrument.Find(FixmlUtil.ReadString(inst, "Sym"), FixmlUtil.ReadString(inst, "ID"));
                int acc110 = FixmlUtil.ReadInt(elem, "Acc110");
				int acc120 = FixmlUtil.ReadInt(elem, "Acc120") ;
				Positions.Add(key, new PosQuantity(acc110, acc120));
			}
			// nie zaszkodzi się upewnić - czy to, co NOL3 podesłał, stanowi jakąś integralną całość...
			// (i czy ja w ogóle słusznie zakładam, jakie powinny być zależności między tymi wartościami)
			// - dla rachunku akcyjnego:
			if (CheckFundsSum(StatementFundType.CashReceivables, StatementFundType.Cash, StatementFundType.Receivables))
				CheckFundsSum(StatementFundType.PortfolioValue, StatementFundType.CashReceivables, StatementFundType.SecuritiesValue);
			// - dla rachunku kontraktowego:
			if (CheckFundsSum(StatementFundType.Deposit, StatementFundType.DepositBlocked, StatementFundType.DepositFree))
				CheckFundsSum(StatementFundType.PortfolioValue, StatementFundType.Cash, StatementFundType.CashBlocked, StatementFundType.Deposit);
		}

		// sprawdza, czy podane "Fundy" w ogóle istnieją... i jeśli tak - czy pierwszy jest równy sumie pozostałych
		private bool CheckFundsSum(params StatementFundType[] types)
		{
			if (types.All(t => Funds.ContainsKey(t)))
			{
				var values = types.Select(t => Funds[t]);
				if (values.First() == values.Skip(1).Sum()) return true;
				SendMessage.Send(MessagegType.LogE, string.Format("Unexpected '{0}' value!  ({1} != {2})", 
					types.First(), values.First(),
					string.Join(" + ", values.Skip(1).Select(v => v.ToString()).ToArray())));
			}
			return false;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("- " + AccountNumber + " -");
			foreach (var p in Positions)
				sb.Append(string.Format("\n  {1} x {0}", p.Key, p.Value.Acc110 + p.Value.Acc120));
			foreach (var f in Funds.OrderBy(t => t.Key))
				sb.Append(string.Format("\n {0,-20} {1,8}", f.Key, f.Value));
			return sb.ToString();
		}
	}
}
