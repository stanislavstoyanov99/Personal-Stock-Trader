﻿namespace PersonalStockTrader.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    using PersonalStockTrader.Data.Common.Models;

    public class Position : BaseDeletableModel<int>
    {
        public int StockId { get; set; }

        public virtual Stock Stock { get; set; }

        public int AccountId { get; set; }

        public Account Account { get; set; }

        [Range(1, int.MaxValue)]
        public int CountStocks { get; set; }

        public decimal OpenPrice { get; set; }

        public decimal? ClosePrice { get; set; }

        public TypeOfTrade TypeOfTrade { get; set; }

        public OpenClose OpenClose { get; set; }
    }
}