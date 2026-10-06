using SwingOne;

namespace TestProjectOne
{
    [TestClass]
    public sealed class PSARTest
    {
        [TestMethod]
        public void GetPSAR_QuickStart()
        {
            var dt = new DateTime(2000, 1, 1);
            // Arrange: Bar 1 High >= Bar 0 High implies Long start.
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 9, 10, 8, 9),
                new PriceBar(dt, 10, 12, 9, 11),
                new PriceBar(dt, 11, 13, 10, 12)
            };

            // Act
            var result = Util.GetPSAR(candles);

            // Assert
            Assert.IsNotNull(result);
            Assert.HasCount(3, result.SarValues);
            Assert.HasCount(3, result.IsLong);
            Assert.IsTrue(result.IsLong[0]); 
            Assert.IsTrue(result.IsLong[1]); 
            Assert.AreEqual(8, result.SarValues[0]); 
            Assert.AreEqual(8, result.SarValues[1]); 
        }

        [TestMethod]
        public void GetPSAR_ShortStart()
        {
            var dt = new DateTime(2000, 1, 1);
            // bars[1].High (9) < bars[0].High (12) → isLong = false
            // ep = min(11, 8) = 8, sar = max(12, 9) = 12
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 10, 12, 11, 11),
                new PriceBar(dt, 9, 9, 8, 8),
                new PriceBar(dt, 8, 8, 7, 7)
            };

            // Act
            var result = Util.GetPSAR(candles);

            // Assert
            Assert.IsFalse(result.IsLong[0]);
            Assert.IsFalse(result.IsLong[1]);
            Assert.IsFalse(result.IsLong[2]);
            Assert.AreEqual(12, result.SarValues[0]);
        }

        [TestMethod]
        public void GetPSAR_Reversal()
        {
            var dt = new DateTime(2000, 1, 1);
            // Start long, then bar 2 low drops below SAR → reversal to short.
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 10, 11, 9, 10),
                new PriceBar(dt, 10, 12, 10, 11),
                new PriceBar(dt, 11, 11, 7, 8)  // low=7 < SAR → reversal.
            };

            var result = Util.GetPSAR(candles);

            Assert.IsTrue(result.IsLong[0]);
            Assert.IsTrue(result.IsLong[1]);
            Assert.IsFalse(result.IsLong[2]); // reversed
        }

        [TestMethod]
        public void GetPSAR_Medium()
        {
            // Test seven price bars with one decimal place.

            // Arrange
            var dt = new DateTime(2000, 1, 1);
            var candles = new List<PriceBar>
            {
                new PriceBar(dt, 10.3, 10.5, 10.0, 10.2),
                new PriceBar(dt, 10.5, 10.8, 10.2, 10.4),
                new PriceBar(dt, 10.7, 10.9, 10.4, 10.6),
                new PriceBar(dt, 10.5, 10.7, 10.3, 10.5),
                new PriceBar(dt, 10.3, 10.6, 10.1, 10.3),
                new PriceBar(dt, 10.8, 11.0, 10.5, 10.7),
                new PriceBar(dt, 11.0, 11.2, 10.8, 11.0)
            };

            // Act
            var result = Util.GetPSAR(candles);

            // Assert
            Assert.HasCount(7, result.IsLong);
            for (int i = 0; i < result.IsLong.Count; i++)
            {
                Assert.IsTrue(result.IsLong[i]);
            }

            double delta = 0.01;
            Assert.HasCount(7, result.SarValues);
            Assert.AreEqual(10.0, result.SarValues[0], delta);
            Assert.AreEqual(10.0, result.SarValues[1], delta);
            Assert.AreEqual(10.0, result.SarValues[2], delta);
            Assert.AreEqual(10.03, result.SarValues[3], delta);
            Assert.AreEqual(10.07, result.SarValues[4], delta);
            Assert.AreEqual(10.1, result.SarValues[5], delta);
            Assert.AreEqual(10.1, result.SarValues[6], delta);
        }

    }
}
