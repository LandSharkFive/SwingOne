
namespace SwingOne
{

    public record struct BacktestResult
    {
        public string StrategyName { get; set; }
        public double StartingCapital { get; set; }
        public double EndingCapital { get; set; }
        public double MaxDrawdown { get; set; }
        public int WinningTrades { get; set; }
        public int LosingTrades { get; set; }
        public double TotalGains { get; set; }
        public double TotalLosses { get; set; }

        // Calculated Properties
        public double TotalReturnPct => StartingCapital > 0 ? ((EndingCapital - StartingCapital) / StartingCapital) * 100.0 : 0.0;
        public int TotalTrades => WinningTrades + LosingTrades;
        public double WinRatePct => TotalTrades > 0 ? ((double)WinningTrades / TotalTrades) * 100.0 : 0.0;
        public double AvgWin => WinningTrades > 0 ? TotalGains / WinningTrades : 0.0;
        public double AvgLoss => LosingTrades > 0 ? TotalLosses / LosingTrades : 0.0;
        public double ProfitFactor => TotalLosses > 0 ? TotalGains / TotalLosses : TotalGains;
    }

    public static class PerformanceReporter
    {
        public static void Print(BacktestResult result)
        {
            Console.WriteLine($"{result.StrategyName}");
            Console.WriteLine($"Starting Capital: ${result.StartingCapital:N2}");
            Console.WriteLine($"Ending Capital:   ${result.EndingCapital:N2}");
            Console.WriteLine($"Total Return:     {result.TotalReturnPct:F2}%");
            Console.WriteLine($"Winning Trades:   {result.WinningTrades}");
            Console.WriteLine($"Losing Trades:    {result.LosingTrades}");
            Console.WriteLine($"Win Rate:         {result.WinRatePct:F2}%");
            Console.WriteLine($"Drawdown:         ${result.MaxDrawdown:N2}");
            Console.WriteLine($"Profit Factor:    {result.ProfitFactor:F2}");
            Console.WriteLine($"Avg Win:          ${result.AvgWin:N2}");
            Console.WriteLine($"Avg Loss:         ${result.AvgLoss:N2}");
            Console.WriteLine();
        }
    }
}
