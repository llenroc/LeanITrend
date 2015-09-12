﻿namespace QuantConnect.Algorithm.CSharp.BizcadAlgorithm
{
    public interface IPositionInventory
    {
        void Add(OrderTransaction transaction);
        OrderTransaction Remove(string direction);
        OrderTransaction RemoveBuy();
        OrderTransaction RemoveSell();
        int BuysCount();
        int SellsCount();
        string GetSymbol();
    }
}