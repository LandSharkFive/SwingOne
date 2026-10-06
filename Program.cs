namespace SwingOne
{
    internal class Program
    {
        /// <summary>
        /// Runs and compares various swing trading strategies and trailing stop 
        /// variations on Intel Corp (INTC) historical price data.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("INTC - Swing Trading Strategy Analysis");
            string filename = $"c:\\bruce\\stock\\intc2yd.csv";

            // Load historical price bars from disk.
            var history = Util.GetHistory(filename);

            // --- Standard Swing Strategy Tests ---
            Util.MovingAverageSwingTest(history);
            Console.WriteLine();

            Util.PSARSwingTest(history);
            Console.WriteLine();

            // SuperTrend with standard 10-period ATR and 2.5 multiplier
            Util.SuperTrendSwingTest(history, stPeriod: 10, stMultiplier: 2.5);
            Console.WriteLine();

            // --- Trailing Stop Variant Tests ---
            // Testing strategies paired with a fixed percentage trailing stop
            Util.MovingAverageTrailingStopSwingTest(history, trailingStopPct: 0.07); // 7% stop
            Console.WriteLine();

            Util.PSARTrailingStopSwingTest(history, trailingStopPct: 0.10); // 10% stop
            Console.WriteLine();

            Util.SuperTrendTrailingStopSwingTest(history, trailingStopPct: 0.10, stPeriod: 10, stMultiplier: 2.5);
        }

    }
}
