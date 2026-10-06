
namespace SwingOne
{

    /// <summary>
    /// Represents a single historical price bar containing high and low values.
    /// </summary>
    public record PriceBar(DateTime Date, double Open, double High, double Low, double Close);


    public class Trade
    {
        public double Shares { get; set; }
        public double EntryPrice { get; set; }
        public double InitialStopLoss { get; set; }
        public int EntryIndex { get; set; }
        public bool IsOpen { get; set; } = true;
    }

    public static class Util
    {
        /// <summary>
        /// Backtests a moving average swing trading strategy based on Exponential Moving Averages (EMA).
        /// </summary>
        public static void MovingAverageSwingTest(List<PriceBar> history, int period = 50, double startingCapital = 10000.0, double stopPct = 0.01)
        {
            if (history == null || history.Count < period) return;

            Trade activeTrade = null;
            int winningTrades = 0;
            int losingTrades = 0;
            int positionSize = 1000;
            double capital = startingCapital;
            double peakCapital = startingCapital;
            double maxDrawdown = 0.0;
            double totalGains = 0.0;
            double totalLosses = 0.0;

            // 1. Precalculate indicator series.
            double[] emaSeries = GetEMASeries(history, period);


            // 2. Linear pass over bars.
            for (int i = period; i < history.Count - 1; i++)
            {
                var bar = history[i];

                // --- 1. EXIT CHECK ---
                if (activeTrade != null && activeTrade.IsOpen)
                {
                    double ema = emaSeries[i];
                    bool closeBelowEma = bar.Close < ema;
                    bool hitInitialStop = bar.Low <= activeTrade.InitialStopLoss;

                    if (hitInitialStop)
                    {
                        CloseTrade(activeTrade, activeTrade.InitialStopLoss, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (closeBelowEma)
                    {
                        CloseTrade(activeTrade, bar.Close, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                }

                // --- 2. ENTRY CHECK ---
                if (activeTrade == null || !activeTrade.IsOpen)
                {
                    double ema = emaSeries[i];  
                    bool isUptrend = bar.Close > ema;

                    if (isUptrend)
                    {
                        double entryPrice = history[i + 1].Open;
                        double initialStop = bar.Low * (1.0 - stopPct);

                        double dollarsToInvest = positionSize;
                        double shares = dollarsToInvest / entryPrice;

                        activeTrade = new Trade
                        {
                            Shares = shares,
                            EntryPrice = entryPrice,
                            InitialStopLoss = initialStop,
                            EntryIndex = i,
                            IsOpen = true
                        };
                    }
                }

                if (capital > peakCapital)
                {
                    peakCapital = capital;
                }
                else
                {
                    double currentDrawdown = peakCapital - capital;
                    if (currentDrawdown > maxDrawdown)
                    {
                        maxDrawdown = currentDrawdown;
                    }
                }
            }

            // Save Results.
            var result = new BacktestResult
            {
                StrategyName = "Moving Average",
                StartingCapital = startingCapital,
                EndingCapital = capital,
                MaxDrawdown = maxDrawdown,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                TotalGains = totalGains,
                TotalLosses = totalLosses
            };

            // Print Results.
            PerformanceReporter.Print(result);
        }


        /// <summary>
        /// Backtests a moving average swing trading strategy using EMA trend filtering combined with a trailing stop-loss.
        /// </summary>
        public static void MovingAverageTrailingStopSwingTest(List<PriceBar> history, int period = 50, double trailingStopPct = 0.05, double startingCapital = 10000.0, double stopPct = 0.01)
        {
            if (history == null || history.Count < period) return;

            Trade activeTrade = null;
            int winningTrades = 0;
            int losingTrades = 0;
            int positionSize = 1000;
            double capital = startingCapital;
            double peakCapital = startingCapital;
            double maxDrawdown = 0.0;
            double totalGains = 0.0;
            double totalLosses = 0.0;

            double highestHigh = 0.0;

            // 1. Precalculate indicator series.
            double[] emaSeries = GetEMASeries(history, period);

            // 2. Linear pass over bars.
            for (int i = period; i < history.Count - 1; i++)
            {
                var bar = history[i];

                // --- 1. EXIT CHECK ---
                if (activeTrade != null && activeTrade.IsOpen)
                {
                    // Update the highest price seen since trade entry
                    if (bar.High > highestHigh)
                    {
                        highestHigh = bar.High;
                    }

                    // Calculate current trailing stop level
                    double trailingStop = highestHigh * (1.0 - trailingStopPct);

                    double ema = emaSeries[i];
                    bool closeBelowEma = bar.Close < ema;
                    bool hitInitialStop = bar.Low <= activeTrade.InitialStopLoss;
                    bool hitTrailingStop = bar.Low <= trailingStop;

                    if (hitInitialStop)
                    {
                        CloseTrade(activeTrade, activeTrade.InitialStopLoss, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (hitTrailingStop)
                    {
                        // Exit at trailing stop level (or open price if gap down)
                        double exitPrice = Math.Min(bar.Open, trailingStop);
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (closeBelowEma)
                    {
                        CloseTrade(activeTrade, bar.Close, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                }

                // --- 2. ENTRY CHECK ---
                if (activeTrade == null || !activeTrade.IsOpen)
                {
                    double ema = emaSeries[i];
                    bool isUptrend = bar.Close > ema;

                    if (isUptrend)
                    {
                        double entryPrice = history[i + 1].Open;
                        double initialStop = bar.Low * (1.0 - stopPct);

                        double dollarsToInvest = positionSize;
                        double shares = dollarsToInvest / entryPrice;

                        activeTrade = new Trade
                        {
                            Shares = shares,
                            EntryPrice = entryPrice,
                            InitialStopLoss = initialStop,
                            EntryIndex = i,
                            IsOpen = true
                        };

                        // Reset high watermark on entry
                        highestHigh = bar.High;
                    }
                }

                if (capital > peakCapital)
                {
                    peakCapital = capital;
                }
                else
                {
                    double currentDrawdown = peakCapital - capital;
                    if (currentDrawdown > maxDrawdown)
                    {
                        maxDrawdown = currentDrawdown;
                    }
                }
            }

            // Save Results.
            var result = new BacktestResult
            {
                StrategyName = "Moving Average (Trailing Stop)",
                StartingCapital = startingCapital,
                EndingCapital = capital,
                MaxDrawdown = maxDrawdown,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                TotalGains = totalGains,
                TotalLosses = totalLosses
            };

            // Print Results.
            PerformanceReporter.Print(result);
        }


        /// <summary>
        /// Backtests a swing trading strategy based on Parabolic Stop and Reverse (PSAR) trend flips and 50 EMA trend confirmation.
        /// </summary>
        public static void PSARSwingTest(List<PriceBar> history, int period = 50, double startingCapital = 10000.0, double stopPct = 0.01)
        {
            if (history == null || history.Count < period) return;

            Trade activeTrade = null;
            int positionSize = 1000;
            int winningTrades = 0;
            int losingTrades = 0;
            double capital = startingCapital;
            double peakCapital = startingCapital;
            double maxDrawdown = 0.0;
            double totalGains = 0.0;
            double totalLosses = 0.0;

            // Precalculate indicators.
            var psar = GetPSAR(history);
            double[] emaSeries = GetEMASeries(history, period);

            for (int i = period; i < history.Count - 1; i++)
            {
                bool prevBar = psar.IsLong[i - 1];
                bool bar = psar.IsLong[i];

                // --- 1. EXIT CHECK ---
                if (activeTrade != null && activeTrade.IsOpen)
                {
                    bool hitInitialStop = history[i].Low <= activeTrade.InitialStopLoss;
                    bool sarSellSignal = prevBar && !bar;

                    if (hitInitialStop)
                    {
                        // Stopped out at initial risk level.
                        CloseTrade(activeTrade, activeTrade.InitialStopLoss, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (sarSellSignal)
                    {
                        double stopPrice = psar.SarValues[i];

                        // Handle overnight gap-downs where Open opened below our stop price.
                        double exitPrice = Math.Min(history[i].Open, stopPrice);

                        // Exit on SAR trend reversal at the open price.
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                }

                // --- 2. ENTRY CHECK ---
                if (activeTrade == null || !activeTrade.IsOpen)
                {
                    bool sarBuySignal = !prevBar && bar;
                    double ema = emaSeries[i];
                    bool isUptrend = history[i].Close > ema;

                    if (sarBuySignal && isUptrend)
                    {
                        double entryPrice = history[i + 1].Open;
                        double initialStop = history[i].Low * (1 - stopPct);

                        double dollarsToInvest = positionSize;
                        double shares = dollarsToInvest / entryPrice;

                        activeTrade = new Trade
                        {
                            Shares = shares,
                            EntryPrice = entryPrice,
                            InitialStopLoss = initialStop,
                            EntryIndex = i,
                            IsOpen = true
                        };
                    }
                }

                if (capital > peakCapital)
                {
                    peakCapital = capital;
                }
                else
                {
                    double currentDrawdown = peakCapital - capital;

                    if (currentDrawdown > maxDrawdown)
                    {
                        maxDrawdown = currentDrawdown;
                    }
                }
            }

            // Save Results.
            var result = new BacktestResult
            {
                StrategyName = "PSAR",
                StartingCapital = startingCapital,
                EndingCapital = capital,
                MaxDrawdown = maxDrawdown,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                TotalGains = totalGains,
                TotalLosses = totalLosses
            };

            // Print Results.
            PerformanceReporter.Print(result);
        }

        /// <summary>
        /// Backtests a swing trading strategy based on Parabolic Stop and Reverse (PSAR) trend flips, 
        /// EMA trend confirmation, and a percentage trailing stop off the peak high.
        /// </summary>
        public static void PSARTrailingStopSwingTest(List<PriceBar> history, int period = 100, double trailingStopPct = 0.05, double startingCapital = 10000.0, double stopPct = 0.01)
        {
            if (history == null || history.Count < period) return;

            Trade activeTrade = null;
            int positionSize = 1000;
            int winningTrades = 0;
            int losingTrades = 0;
            double capital = startingCapital;
            double peakCapital = startingCapital;
            double maxDrawdown = 0.0;
            double totalGains = 0.0;
            double totalLosses = 0.0;

            double highestHigh = 0.0;

            // Precalculate indicators.
            var psar = GetPSAR(history);
            double[] emaSeries = GetEMASeries(history, period);

            for (int i = period; i < history.Count - 1; i++)
            {
                bool prevBar = psar.IsLong[i - 1];
                bool bar = psar.IsLong[i];

                // --- 1. EXIT CHECK ---
                if (activeTrade != null && activeTrade.IsOpen)
                {
                    // Update highest price seen since entry
                    if (history[i].High > highestHigh)
                    {
                        highestHigh = history[i].High;
                    }

                    double trailingStop = highestHigh * (1.0 - trailingStopPct);

                    bool hitInitialStop = history[i].Low <= activeTrade.InitialStopLoss;
                    bool hitTrailingStop = history[i].Low <= trailingStop;
                    bool sarSellSignal = prevBar && !bar;

                    if (hitInitialStop)
                    {
                        CloseTrade(activeTrade, activeTrade.InitialStopLoss, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (hitTrailingStop)
                    {
                        // Exit at trailing stop level (or open price if gapped down)
                        double exitPrice = Math.Min(history[i].Open, trailingStop);
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (sarSellSignal)
                    {
                        double stopPrice = psar.SarValues[i];

                        // Handle overnight gap-downs where Open opened below SAR stop price
                        double exitPrice = Math.Min(history[i].Open, stopPrice);
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                }

                // --- 2. ENTRY CHECK ---
                if (activeTrade == null || !activeTrade.IsOpen)
                {
                    bool sarBuySignal = !prevBar && bar;
                    double ema = emaSeries[i];
                    bool isUptrend = history[i].Close > ema;

                    if (sarBuySignal && isUptrend)
                    {
                        double entryPrice = history[i + 1].Open;
                        double initialStop = history[i].Low * (1.0 - stopPct);

                        double dollarsToInvest = positionSize;
                        double shares = dollarsToInvest / entryPrice;

                        activeTrade = new Trade
                        {
                            Shares = shares,
                            EntryPrice = entryPrice,
                            InitialStopLoss = initialStop,
                            EntryIndex = i,
                            IsOpen = true
                        };

                        // Reset high watermark on entry
                        highestHigh = history[i].High;
                    }
                }

                if (capital > peakCapital)
                {
                    peakCapital = capital;
                }
                else
                {
                    double currentDrawdown = peakCapital - capital;

                    if (currentDrawdown > maxDrawdown)
                    {
                        maxDrawdown = currentDrawdown;
                    }
                }
            }

            // Save Results.
            var result = new BacktestResult
            {
                StrategyName = "PSAR (Trailing Stop)",
                StartingCapital = startingCapital,
                EndingCapital = capital,
                MaxDrawdown = maxDrawdown,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                TotalGains = totalGains,
                TotalLosses = totalLosses
            };

            // Print Results.
            PerformanceReporter.Print(result);
        }


        /// <summary>
        /// Backtests a swing trading strategy based on Supertrend trend flips and EMA trend confirmation.
        /// </summary>
        public static void SuperTrendSwingTest(List<PriceBar> history, int period = 50, int stPeriod = 10, double stMultiplier = 3.0, double startingCapital = 10000.0, double stopPct = 0.01)
        {
            if (history == null || history.Count < period) return;

            Trade activeTrade = null;
            int positionSize = 1000;
            int winningTrades = 0;
            int losingTrades = 0;
            double capital = startingCapital;
            double peakCapital = startingCapital;
            double maxDrawdown = 0.0;
            double totalGains = 0.0;
            double totalLosses = 0.0;

            // Precalculate both SuperTrend and EMA series.
            var stResult = GetSupertrendByBars(history, stPeriod, stMultiplier);
            double[] emaSeries = GetEMASeries(history, period);  

            for (int i = period; i < history.Count - 1; i++)
            {
                var bar = history[i];

                bool prevTrendLong = stResult.Trend[i - 1] == 1;
                bool currentTrendLong = stResult.Trend[i] == 1;

                // --- 1. EXIT CHECK ---
                if (activeTrade != null && activeTrade.IsOpen)
                {
                    bool hitInitialStop = bar.Low <= activeTrade.InitialStopLoss;
                    bool supertrendSellSignal = prevTrendLong && !currentTrendLong;

                    if (hitInitialStop)
                    {
                        // Stopped out at initial risk level.
                        CloseTrade(activeTrade, activeTrade.InitialStopLoss, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (supertrendSellSignal)
                    {
                        double stopPrice = stResult.Values[i];

                        // Handle overnight gap-downs where Open opened below our stop price.
                        double exitPrice = Math.Min(bar.Open, stopPrice);

                        // Exit on Supertrend reversal.
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                }

                // --- 2. ENTRY CHECK ---
                if (activeTrade == null || !activeTrade.IsOpen)
                {
                    bool supertrendBuySignal = !prevTrendLong && currentTrendLong;

                    double ema = emaSeries[i];
                    bool isUptrend = bar.Close > ema;

                    if (supertrendBuySignal && isUptrend)
                    {
                        double entryPrice = history[i + 1].Open;
                        double initialStop = history[i].Low * (1.0 - stopPct);

                        double dollarsToInvest = positionSize;
                        double shares = dollarsToInvest / entryPrice;

                        activeTrade = new Trade
                        {
                            Shares = shares,
                            EntryPrice = entryPrice,
                            InitialStopLoss = initialStop,
                            EntryIndex = i,
                            IsOpen = true
                        };
                    }
                }

                if (capital > peakCapital)
                {
                    peakCapital = capital;
                }
                else
                {
                    double currentDrawdown = peakCapital - capital;

                    if (currentDrawdown > maxDrawdown)
                    {
                        maxDrawdown = currentDrawdown;
                    }
                }
            }


            // Save Results.
            var result = new BacktestResult
            {
                StrategyName = "SuperTrend",
                StartingCapital = startingCapital,
                EndingCapital = capital,
                MaxDrawdown = maxDrawdown,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                TotalGains = totalGains,
                TotalLosses = totalLosses
            };

            // Print Results.
            PerformanceReporter.Print(result);
        }

        /// <summary>
        /// Backtests a swing trading strategy based on Supertrend trend flips, EMA trend confirmation,
        /// and a high-watermark percentage trailing stop.
        /// </summary>
        public static void SuperTrendTrailingStopSwingTest(List<PriceBar> history, int period = 100, int stPeriod = 10, double stMultiplier = 3.0, double trailingStopPct = 0.05, double startingCapital = 10000.0, double stopPct = 0.01)
        {
            if (history == null || history.Count < period) return;

            Trade activeTrade = null;
            int positionSize = 1000;
            int winningTrades = 0;
            int losingTrades = 0;
            double capital = startingCapital;
            double peakCapital = startingCapital;
            double maxDrawdown = 0.0;
            double totalGains = 0.0;
            double totalLosses = 0.0;

            double highestHigh = 0.0;

            // Precalculate indicators
            var stResult = GetSupertrendByBars(history, stPeriod, stMultiplier);
            double[] emaSeries = GetEMASeries(history, period);

            for (int i = period; i < history.Count - 1; i++)
            {
                var bar = history[i];

                bool prevTrendLong = stResult.Trend[i - 1] == 1;
                bool currentTrendLong = stResult.Trend[i] == 1;

                // --- 1. EXIT CHECK ---
                if (activeTrade != null && activeTrade.IsOpen)
                {
                    if (bar.High > highestHigh)
                    {
                        highestHigh = bar.High;
                    }

                    double trailingStop = highestHigh * (1.0 - trailingStopPct);

                    bool hitInitialStop = bar.Low <= activeTrade.InitialStopLoss;
                    bool hitTrailingStop = bar.Low <= trailingStop;
                    bool supertrendSellSignal = prevTrendLong && !currentTrendLong;

                    if (hitInitialStop)
                    {
                        CloseTrade(activeTrade, activeTrade.InitialStopLoss, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (hitTrailingStop)
                    {
                        double exitPrice = Math.Min(bar.Open, trailingStop);
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                    else if (supertrendSellSignal)
                    {
                        double stopPrice = stResult.Values[i];
                        double exitPrice = Math.Min(bar.Open, stopPrice);
                        CloseTrade(activeTrade, exitPrice, ref capital, ref losingTrades, ref winningTrades, ref totalGains, ref totalLosses);
                        continue;
                    }
                }

                // --- 2. ENTRY CHECK ---
                if (activeTrade == null || !activeTrade.IsOpen)
                {
                    bool supertrendBuySignal = !prevTrendLong && currentTrendLong;
                    double ema = emaSeries[i];
                    bool isUptrend = bar.Close > ema;

                    if (supertrendBuySignal && isUptrend)
                    {
                        double entryPrice = history[i + 1].Open;
                        double initialStop = history[i].Low * (1.0 - stopPct);

                        double dollarsToInvest = positionSize;
                        double shares = dollarsToInvest / entryPrice;

                        activeTrade = new Trade
                        {
                            Shares = shares,
                            EntryPrice = entryPrice,
                            InitialStopLoss = initialStop,
                            EntryIndex = i,
                            IsOpen = true
                        };

                        highestHigh = bar.High;
                    }
                }

                if (capital > peakCapital)
                {
                    peakCapital = capital;
                }
                else
                {
                    double currentDrawdown = peakCapital - capital;

                    if (currentDrawdown > maxDrawdown)
                    {
                        maxDrawdown = currentDrawdown;
                    }
                }
            }

            var result = new BacktestResult
            {
                StrategyName = "SuperTrend (Trailing Stop)",
                StartingCapital = startingCapital,
                EndingCapital = capital,
                MaxDrawdown = maxDrawdown,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                TotalGains = totalGains,
                TotalLosses = totalLosses
            };

            PerformanceReporter.Print(result);
        }


        /// <summary>
        /// Closes an active trade, updates total capital based on PnL, increments win/loss counters, and marks the trade as closed.
        /// </summary>
        private static void CloseTrade(Trade trade, double exitPrice, ref double capital, ref int losingTrades, ref int winningTrades,
            ref double totalGains, ref double totalLosses)
        {
            // Actual PnL in dollars = number of shares * price difference
            double tradePnl = trade.Shares * (exitPrice - trade.EntryPrice);
            capital += tradePnl;

            if (tradePnl > 0)
            {
                winningTrades++; //
                totalGains += tradePnl;
            }
            else
            {
                losingTrades++; //
                totalLosses += Math.Abs(tradePnl);
            }

            trade.IsOpen = false;
        }


        /// <summary>
        /// Calculates the Simple Moving Average (SMA) of closing prices over a specified lookback period ending at a specific index.
        /// </summary>
        public static double GetSMA(List<PriceBar> history, int currentIndex, int period)
        {
            double sum = 0;
            for (int k = currentIndex - period + 1; k <= currentIndex; k++)
            {
                sum += history[k].Close;
            }
            return sum / period;
        }

        /// <summary>
        /// Calculates an Exponential Moving Average (EMA) series for a sequence of price bars.
        /// </summary>
        public static double GetEMA(List<PriceBar> history, int currentIndex, int period)
        {
            // 1. Seed calculation
            double sum = 0;
            for (int k = 0; k < period; k++)
            {
                sum += history[k].Close;
            }
            double currentEMA = sum / period;

            // 2. Recursive EMA calculation up to the current index bar.
            double alpha = 2.0 / (period + 1);
            for (int k = period; k <= currentIndex; k++)
            {
                currentEMA = (history[k].Close - currentEMA) * alpha + currentEMA;
            }

            return currentEMA;
        }

        /// <summary>
        /// Calculates a Simple Moving Average (SMA) series for a sequence of price bars.
        /// </summary>
        public static double[] GetSMASeries(List<PriceBar> history, int period)
        {
            if (history == null || history.Count == 0 || period <= 0)
            {
                return Array.Empty<double>();
            }

            double[] sma = new double[history.Count];
            double sum = 0;

            for (int i = 0; i < history.Count; i++)
            {
                sum += history[i].Close;

                if (i >= period)
                {
                    // Subtract the price that fell out of the moving window.
                    sum -= history[i - period].Close;
                }

                // Populate SMA value once enough data points exist.
                if (i >= period - 1)
                {
                    sma[i] = sum / period;
                }
                else
                {
                    sma[i] = double.NaN; // Unfilled initial window
                }
            }

            return sma;
        }


        /// <summary>
        /// Calculates an Exponential Moving Average (EMA) series for a sequence of price bars.
        /// </summary>
        public static double[] GetEMASeries(List<PriceBar> history, int period)
        {
            // Guard clauses
            if (history == null || history.Count == 0 || period <= 0 || history.Count < period)
            {
                return Array.Empty<double>();
            }

            double alpha = 2.0 / (period + 1);
            double[] ema = new double[history.Count];

            // Mark pre-warmup bars as NaN.
            for (int k = 0; k < period - 1; k++)
            {
                ema[k] = double.NaN;
            }

            // Seed with SMA
            double sum = 0;
            for (int k = 0; k < period; k++)
            {
                sum += history[k].Close;
            }
            ema[period - 1] = sum / period;

            // Recursive EMA
            for (int k = period; k < history.Count; k++)
            {
                ema[k] = (history[k].Close - ema[k - 1]) * alpha + ema[k - 1];
            }

            return ema;
        }


        /// <summary>
        /// Loads historical price bar data from a CSV file.
        /// </summary>
        public static List<PriceBar> GetHistory(string filename)
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found at {filename}");
                return new List<PriceBar>();
            }
            return CSVLoader.LoadFromCsv(filename);
        }

        /// <summary>
        /// Container class for holding Parabolic SAR calculation outputs.
        /// </summary>
        public class SARResult
        {
            public List<double> SarValues;
            public List<bool> IsLong;
        }

        /// /// <summary>
        /// Calculates the Parabolic Stop and Reverse (PSAR) indicator for a given series of price bars.
        /// </summary>
        public static SARResult GetPSAR(List<PriceBar> candles, double afStep = 0.02, double afMax = 0.2)
        {
            var bars = candles;
            if (bars == null || bars.Count < 2)
            {
                throw new ArgumentException("Price bars collection must contain at least 2 periods.");
            }

            int count = bars.Count;
            var sarValues = new List<double>(new double[count]);
            var isLongList = new List<bool>(new bool[count]);

            // Determine initial trend based on the first two periods.
            bool isLong = bars[1].High >= bars[0].High;
            double af = afStep;

            // Extreme Point (EP).
            double ep = isLong ? Math.Max(bars[0].High, bars[1].High) : Math.Min(bars[0].Low, bars[1].Low);

            // Initial SAR value.
            double sar = isLong ? Math.Min(bars[0].Low, bars[1].Low) : Math.Max(bars[0].High, bars[1].High);

            sarValues[0] = sar;
            isLongList[0] = isLong;

            for (int i = 1; i < count; i++)
            {
                // Calculate SAR for the current period.
                sar = sar + af * (ep - sar);

                if (isLong)
                {
                    // SAR cannot be higher than the previous two periods lows.
                    if (i >= 2)
                    {
                        sar = Math.Min(sar, Math.Min(bars[i - 1].Low, bars[i - 2].Low));
                    }
                    else
                    {
                        sar = Math.Min(sar, bars[i - 1].Low);
                    }

                    // Check for reversal (if current low drops below SAR).
                    if (bars[i].Low < sar)
                    {
                        isLong = false;
                        sar = ep; // New SAR is the previous Extreme Point.
                        ep = bars[i].Low; // Reset EP to current low.
                        af = afStep; // Reset AF.
                    }
                    else
                    {
                        // Update Extreme Point and Acceleration Factor if a new high is reached.
                        if (bars[i].High > ep)
                        {
                            ep = bars[i].High;
                            af = Math.Min(af + afStep, afMax);
                        }
                    }
                }
                else
                {
                    // SAR cannot be lower than the previous two periods' highs.
                    if (i >= 2)
                    {
                        sar = Math.Max(sar, Math.Max(bars[i - 1].High, bars[i - 2].High));
                    }
                    else
                    {
                        sar = Math.Max(sar, bars[i - 1].High);
                    }

                    // Check for reversal (if current high rises above SAR).
                    if (bars[i].High > sar)
                    {
                        isLong = true;
                        sar = ep; // New SAR is the previous Extreme Point.
                        ep = bars[i].High; // Reset EP to current high.
                        af = afStep; // Reset AF
                    }
                    else
                    {
                        // Update Extreme Point and Acceleration Factor if a new low is reached.
                        if (bars[i].Low < ep)
                        {
                            ep = bars[i].Low;
                            af = Math.Min(af + afStep, afMax);
                        }
                    }
                }

                sarValues[i] = sar;
                isLongList[i] = isLong;
            }

            return new SARResult
            {
                SarValues = sarValues,
                IsLong = isLongList
            };
        }


        /// <summary>
        /// Container class for the Supertrend calculation outputs.
        /// </summary>

        public class STResult
        {
            public double[] Values { get; set; }
            public int[] Trend { get; set; } // 1 = up, -1 = down.
        }

        public static STResult GetSupertrendByBars(List<PriceBar> bars, int period = 10, double multiplier = 3.0)
        {
            double[] high = bars.Select(b => b.High).ToArray();
            double[] low = bars.Select(b => b.Low).ToArray();
            double[] close = bars.Select(b => b.Close).ToArray();

            return GetSupertrend(high, low, close, period, multiplier);
        }

        /// <summary>
        /// Calculates the Supertrend indicator values and trend directions.
        /// </summary>
        public static STResult GetSupertrend(double[] high, double[] low, double[] close, int period = 10, double multiplier = 3.0)
        {
            int n = close.Length;
            var values = new double[n];
            var trends = new int[n];

            // 1. ATR
            double[] tr = new double[n];
            double[] atr = new double[n];
            for (int i = 0; i < n; i++)
            {
                if (i == 0)
                {
                    tr[i] = high[i] - low[i];
                    atr[i] = tr[i];
                }
                else
                {
                    tr[i] = Math.Max(high[i] - low[i],
                        Math.Max(Math.Abs(high[i] - close[i - 1]),
                        Math.Abs(low[i] - close[i - 1])));
                    atr[i] = (atr[i - 1] * (period - 1) + tr[i]) / period;
                }
            }

            // 2. Basic bands
            double[] basicUpper = new double[n];
            double[] basicLower = new double[n];
            for (int i = 0; i < n; i++)
            {
                double hl2 = (high[i] + low[i]) / 2.0;
                basicUpper[i] = hl2 + multiplier * atr[i];
                basicLower[i] = hl2 - multiplier * atr[i];
            }

            // 3. Ratchet + Trend.
            double finalUpper = basicUpper[0];
            double finalLower = basicLower[0];
            int trend = 1;
            values[0] = finalLower;
            trends[0] = 1;

            for (int i = 1; i < n; i++)
            {
                // Ratchet upper (can only tighten downward).
                finalUpper = (basicUpper[i] < finalUpper || close[i - 1] > finalUpper)
                           ? basicUpper[i] : finalUpper;

                // Ratchet lower (can only tighten upward).
                finalLower = (basicLower[i] > finalLower || close[i - 1] < finalLower)
                           ? basicLower[i] : finalLower;

                // Flip logic
                if (trend == 1 && close[i] < finalLower)
                    trend = -1;
                else if (trend == -1 && close[i] > finalUpper)
                    trend = 1;

                values[i] = (trend == 1) ? finalLower : finalUpper;
                trends[i] = trend;
            }

            return new STResult
            {
                Values = values,
                Trend = trends
            };
        }

    }
}
