# SwingOne

**SwingOne** is a C# .NET 10 backtesting engine and strategy analysis platform built for evaluating swing trading algorithms against historical market data.

---

## Key Features

* **Multiple Strategy Implementations:**
  * **Exponential Moving Average (EMA) Swing Strategy:** Identifies trend direction and filters trade setups using EMA crossovers/levels.
  * **Parabolic Stop and Reverse (PSAR):** Captures momentum and trend reversals paired with EMA confirmation filters.
  * **SuperTrend Indicator:** Calculates Average True Range (ATR) based upper/lower bands to trigger trend-following entries and exits.
* **Trailing Stop Variants:** Optional high-watermark percentage-based trailing stops for all core strategies to protect unrealized gains.
* **Performance Analytics & Metrics:** Built-in calculation of starting/ending capital, win rate percentage, profit factor, average gains/losses, and maximum drawdown.
* **CSV Data Ingestion:** Yahoo Finance CSV loader.

## License

This project is open source and available under the MIT License.
