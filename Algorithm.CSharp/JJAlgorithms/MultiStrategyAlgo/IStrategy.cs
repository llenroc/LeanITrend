﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Indicators;
using QuantConnect.Data.Market;
using QuantConnect.Algorithm.CSharp.Common;

namespace QuantConnect.Algorithm.CSharp.JJAlgorithms.MultiStrategyAlgo
{
    interface IStrategy
    {
        /// <summary>
        /// Checks the signal.
        /// </summary>
        /// <returns></returns>
        OrderSignal CheckSignal(decimal close);
    }

    public abstract class BaseStrategy : IStrategy
    {
        public StockState Position;

        public Nullable<decimal> EntryPrice;

        public abstract OrderSignal CheckSignal(decimal close);
    }
}