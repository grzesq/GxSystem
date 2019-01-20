using GxNolApi.DTO;
using GxNolApi.NolClient.Fixml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GxNolApi.NolClient
{
    class NolMsgHandler
    {
        private INOLClient Api;

        public NolMsgHandler(INOLClient api)
        {
            Api = api;
        }

        public void AsyncUserResponseMsgHandler(UserResponseMsg msg)
        {
            SendMessage.Send(MessagegType.AsncUserRespo, "NOL nowy status", msg.NolStatus);
        }

        public void AsyncStatmentHandler(StatementMsg msg)
        {
            foreach (var statement in msg.Statements)
            {
                var account = new AccountData();
                account.Number = statement.AccountNumber;
                // otwarte pozycje...
                account.Papers = statement.Positions.
                    Select(p => new Paper
                    {
                        Instrument = p.Key.Convert(),
                        Account110 = p.Value.Acc110,
                        Account120 = p.Value.Acc120,
                    }).ToArray();
                // najważniejsze kwoty...
                account.AvailableCash = statement.Funds[StatementFundType.Cash];
                if (!statement.Funds.ContainsKey(StatementFundType.Deposit))
                {
                    // rachunek akcyjny
                    account.AvailableFunds = statement.Funds[StatementFundType.CashReceivables];
                }
                else
                {
                    // rachunek kontraktowy
                    account.AvailableFunds = account.AvailableCash + statement.Funds[StatementFundType.DepositFree];
                    if (statement.Funds.ContainsKey(StatementFundType.DepositDeficit))
                        account.DepositDeficit = statement.Funds[StatementFundType.DepositDeficit];
                    account.DepositBlocked = statement.Funds[StatementFundType.DepositBlocked];
                    account.DepositValue = statement.Funds[StatementFundType.Deposit];
                }
                account.PortfolioValue = statement.Funds[StatementFundType.PortfolioValue];

                var acc = Api.Accounts[account.Number];
                acc.Update(account);

                SendMessage.Send(MessagegType.AsncStatment, account.Number);
            }
        }

        public void AsyncSessionStatusHandler(TradingSessionStatusMsg msg)
        {
            SendMessage.Send(MessagegType.AsncSessionStatus, "Session status", msg.SessionPhase);
        }

        public void AsyncNewsMsgHandler(NewsMsg msg)
        {
            SendMessage.Send(MessagegType.AsncNews, msg.Headline, msg.Text);
        }

        #region Exec Report Handler
        public void AsyncExecReportHandler(ExecutionReportMsg msg)
        {
            var order = new OrderData();
            order.AccountNumber = msg.Account;
            order.BrokerId = msg.BrokerOrderId2;
            order.ClientId = msg.ClientOrderId;
            if (msg.ExecType == ExecReportType.Trade)
            {
                order.TradeReport = new OrderTradeData();
                order.TradeReport.Time = msg.TransactionTime;
                order.TradeReport.Price = (decimal)msg.Price;  // LastPrice !?
                order.TradeReport.Quantity = (uint)msg.Quantity;  // LastQuantity !?
                order.TradeReport.NetValue = (decimal)msg.NetMoney;
                order.TradeReport.Commission = (decimal)msg.CommissionValue;
            }
            else
            {
                // w pozostałych przypadkach wygląda na to, że lepiej się oprzeć na polu "Status"
                // (bo ExecType czasem jest, czasem nie ma - różnie to z nim bywa... a Status jest chyba zawsze)
                order.StatusReport = new OrderStatusData();
                order.StatusReport.Status = ExecReport_GetStatus(msg);
                order.StatusReport.Quantity = (uint)msg.CumulatedQuantity;
                order.StatusReport.NetValue = (decimal)msg.NetMoney;
                order.StatusReport.Commission = (decimal)msg.CommissionValue;  // czasem == 0, ale dlaczego!? kto to wie... 

                // pozostałe dane - żeby się nie rozdrabniać - też aktualizujemy za każdym razem
                // (teoretycznie wystarczyłoby przy "new" i "replace"... ale czasem jako pierwsze
                // przychodzi np. "filled" i kto wie co jeszcze innego, więc tak będzie bezpieczniej)
                order.MainData = new OrderMainData();
                order.MainData.CreateTime = (DateTime)msg.TransactionTime;
                order.MainData.Instrument = msg.Instrument.Convert();
                order.MainData.Side = (msg.Side == Fixml.OrderSide.Buy) ? BosOrderSide.Buy : BosOrderSide.Sell;
                order.MainData.PriceType = ExecReport_GetPriceType(msg);
                if (order.MainData.PriceType == PriceType.Limit)
                    order.MainData.PriceLimit = msg.Price;
                if ((msg.Type == OrderType.StopLimit) || (msg.Type == OrderType.StopLoss))
                    order.MainData.ActivationPrice = msg.StopPrice;
                order.MainData.Quantity = (uint)msg.Quantity;
                order.MainData.MinimumQuantity = (msg.TimeInForce == OrdTimeInForce.WuA) ? msg.Quantity : msg.MinimumQuantity;
                order.MainData.VisibleQuantity = msg.DisplayQuantity;
                order.MainData.ImmediateOrCancel = (msg.TimeInForce == OrdTimeInForce.WiA);
                order.MainData.ExpirationDate = (msg.TimeInForce == OrdTimeInForce.Date) ? msg.ExpireDate : null;
            }

            var account = Api.Accounts[order.AccountNumber];
            account.Orders.Update(order);

        }

        // funkcja pomocnicza zamieniająca status zlecenia FIXML na nasz BosOrderStatus
        private static BosOrderStatus ExecReport_GetStatus(ExecutionReportMsg msg)
        {
            switch (msg.Status)
            {
                case ExecReportStatus.New: return BosOrderStatus.Active;
                case ExecReportStatus.PartiallyFilled: return BosOrderStatus.ActiveFilled;
                case ExecReportStatus.Canceled: return ((msg.CumulatedQuantity ?? 0) > 0) ? BosOrderStatus.CancelledFilled : BosOrderStatus.Cancelled;
                case ExecReportStatus.Filled: return BosOrderStatus.Filled;
                case ExecReportStatus.Expired: return BosOrderStatus.Expired;
                case ExecReportStatus.Rejected: return BosOrderStatus.Rejected;
                case ExecReportStatus.PendingReplace: return BosOrderStatus.PendingReplace;
                case ExecReportStatus.PendingCancel: return BosOrderStatus.PendingCancel;
                default: throw new ArgumentException("Unknown ExecReport-Status");
            }
        }

        // funkcja pomocnicza zamieniająca typ zlecenia FIXML na nasz DTO.PriceType
        private static PriceType ExecReport_GetPriceType(ExecutionReportMsg msg)
        {
            switch (msg.Type)
            {
                case OrderType.Limit:
                case OrderType.StopLimit: return PriceType.Limit;
                case OrderType.PKC:
                case OrderType.StopLoss: return PriceType.PKC;
                case OrderType.PCR_PCRO:
                    var time = msg.TimeInForce;
                    var pcro = ((time == OrdTimeInForce.Opening) || (time == OrdTimeInForce.Closing));
                    return pcro ? PriceType.PCRO : PriceType.PCR;
                default: throw new ArgumentException("Unknown ExecReport-OrderType");
            }
        }
        #endregion

        #region Market Data
        public void AsyncMarketDataHandler(MarketDataIncRefreshMsg msg)
        {
            var list = new List<MarketData>();
            foreach (MDEntry entry in msg.Entries)
            {
                var data = MarketData_GetData(entry);
                if (data != null) list.Add(data);
            }
            if (list.Count > 0)
                SendMessage.Send(MessagegType.AsncMDUpdate, "Session status", list);

            //MarketUpdateEvent(list.ToArray());
        }

        // funkcja pomocnicza konwertująca obiekt Fixml.MDEntry na DTO.MarketData
        private static MarketData MarketData_GetData(MDEntry entry)
        {
            var data = new MarketData();
            data.Instrument = entry.Instrument.Convert();
            switch (entry.EntryType)
            {
                case MDEntryType.Buy: data.BuyOffer = MarketData_GetOfferData(entry); break;
                case MDEntryType.Sell: data.SellOffer = MarketData_GetOfferData(entry); break;
                case MDEntryType.Trade: data.Trade = MarketData_GetTradeData(entry); break;
                case MDEntryType.Lop: data.OpenInt = entry.Size; break;
                case MDEntryType.Vol:
                case MDEntryType.Open:
                case MDEntryType.Close:
                case MDEntryType.High:
                case MDEntryType.Low:
                case MDEntryType.Ref: data.Stats = MarketData_GetStatsData(entry); break;
                default: return null;   // pozostałe pomijamy
            }
            return data;
        }

        // funkcja pomocnicza konwertująca dane z Fixml.MDEntry na DTO.MarketOfferData
        private static MarketOfferData MarketData_GetOfferData(MDEntry entry)
        {
            var offer = new MarketOfferData();
            offer.Level = (int)entry.Level;
            offer.Update = (entry.UpdateAction != MDUpdateAction.New);
            if (entry.UpdateAction != MDUpdateAction.Delete)
            {
                switch (entry.PriceStr)
                {
                    case "PKC": offer.PriceType = PriceType.PKC; break;
                    case "PCR": offer.PriceType = PriceType.PCR; break;
                    case "PCRO": offer.PriceType = PriceType.PCRO; break;
                }
                offer.PriceLimit = entry.Price;
                offer.Volume = (uint)entry.Size;
                offer.Count = (uint)entry.Orders;
            }
            return offer;
        }

        // funkcja pomocnicza konwertująca dane z Fixml.MDEntry na DTO.MarketTradeData
        private static MarketTradeData MarketData_GetTradeData(MDEntry entry)
        {
            var trade = new MarketTradeData();
            trade.Time = entry.DateTime;
            trade.Price = (decimal)entry.Price;
            trade.Quantity = entry.Size ?? 0;  // może być null dla notowań indeksów
            return trade;
        }

        // funkcja pomocnicza konwertująca dane z Fixml.MDEntry na DTO.MarketStatsData
        private static MarketStatsData MarketData_GetStatsData(MDEntry entry)
        {
            var stats = new MarketStatsData();
            switch (entry.EntryType)
            {
                case MDEntryType.Vol:
                    stats.TotalVolume = entry.Size;
                    stats.TotalTurnover = entry.Turnover;
                    break;
                case MDEntryType.Open:
                    stats.OpeningPrice = entry.Price;
                    stats.OpeningTurnover = entry.Turnover ?? 0;
                    break;
                case MDEntryType.Close:
                    stats.ClosingPrice = entry.Price;
                    stats.ClosingTurnover = entry.Turnover ?? 0;
                    break;
                case MDEntryType.High:
                    stats.HighestPrice = entry.Price;
                    break;
                case MDEntryType.Low:
                    stats.LowestPrice = entry.Price;
                    break;
                case MDEntryType.Ref:
                    stats.ReferencePrice = entry.Price;
                    break;
            }
            return stats;
        }
        #endregion

    }
}
