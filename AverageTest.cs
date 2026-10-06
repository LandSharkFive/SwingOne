using SwingOne;

namespace TestProjectOne
{
    [TestClass]
    public sealed class AverageTest
    {
        [TestMethod]
        public void GetSMA_Quick()
        {
            /// Validates point-in-time SMA calculation over a 3-bar window at index 2.
            /// Expected Closes: [9, 11, 12] -> Average = (9 + 11 + 12) / 3 = 10.666...

            // Arrange
            var dt = new DateTime(2000, 1, 1);
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 9, 10, 8, 9),
                new PriceBar(dt, 10, 12, 9, 11),
                new PriceBar(dt, 11, 13, 10, 12)
            };

            // Act
            var result = Util.GetSMA(candles, 2, 3);

            // Assert
            Assert.AreEqual(10.66, result, 0.01);
        }

        [TestMethod]
        public void GetEMA_Quick()
        {
            /// Validates point-in-time EMA calculation over a 2-period window at index 2.
            /// Multiplier k = 2 / (2 + 1) = 0.6667
            /// Initial SMA Seed (Index 1): (9 + 11) / 2 = 10.0
            /// EMA (Index 2): (12 * 0.6667) + (10.0 * 0.3333) = 11.333...

            // Arrange
            var dt = new DateTime(2000, 1, 1);
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 9, 10, 8, 9),
                new PriceBar(dt, 10, 12, 9, 11),
                new PriceBar(dt, 11, 13, 10, 12)
            };

            // Act
            var result = Util.GetEMA(candles, 2, 2);

            // Assert
            Assert.AreEqual(11.33, result, 0.01);
        }

        [TestMethod]
        public void GetSMASeries_Quick()
        {
            /// Validates full series SMA calculation for a 2-period lookback across 5 bars.
            /// Expected Closes: [9, 11, 12, 13, 12]
            /// Index 0: NaN (insufficient bars)
            /// Index 1: (9 + 11) / 2 = 10.0
            /// Index 2: (11 + 12) / 2 = 11.5
            /// Index 3: (12 + 13) / 2 = 12.5
            /// Index 4: (13 + 12) / 2 = 12.5
            
            // Arrange
            var dt = new DateTime(2000, 1, 1);
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 9, 10, 8, 9),
                new PriceBar(dt, 10, 12, 9, 11),
                new PriceBar(dt, 11, 13, 10, 12),
                new PriceBar(dt, 12, 14, 11, 13),
                new PriceBar(dt, 13, 15, 11, 12)
            };

            // Act
            var result = Util.GetSMASeries(candles, 2);

            // Assert
            double delta = 0.01;
            Assert.HasCount(5, result);
            Assert.AreEqual(double.NaN, result[0]);
            Assert.AreEqual(10.0, result[1], delta);
            Assert.AreEqual(11.5, result[2], delta);
            Assert.AreEqual(12.5, result[3], delta);
            Assert.AreEqual(12.5, result[4], delta);
        }

        [TestMethod]
        public void GetEMASeries_Quick()
        {
            /// Validates full series EMA calculation for a 2-period lookback across 5 bars.
            /// Multiplier k = 2 / (2 + 1) = 0.6667
            /// Index 0: NaN
            /// Index 1: Seed SMA = 10.0
            /// Index 2: (12 * 0.6667) + (10.00 * 0.3333) = 11.333
            /// Index 3: (13 * 0.6667) + (11.33 * 0.3333) = 12.444
            /// Index 4: (12 * 0.6667) + (12.44 * 0.3333) = 12.148
            
            // Arrange
            var dt = new DateTime(2000, 1, 1);
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 9, 10, 8, 9),
                new PriceBar(dt, 10, 12, 9, 11),
                new PriceBar(dt, 11, 13, 10, 12),
                new PriceBar(dt, 12, 14, 11, 13),
                new PriceBar(dt, 13, 15, 11, 12)
            };

            // Act
            var result = Util.GetEMASeries(candles, 2);

            // Assert
            double delta = 0.01;
            Assert.HasCount(5, result);
            Assert.AreEqual(double.NaN, result[0]);
            Assert.AreEqual(10.0, result[1], delta);
            Assert.AreEqual(11.33, result[2], delta);
            Assert.AreEqual(12.44, result[3], delta);
            Assert.AreEqual(12.14, result[4], delta);
        }

    }
}
