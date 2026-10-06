using SwingOne;

namespace TestProjectOne
{
    [TestClass]
    public sealed class SuperTrendTest
    {
        [TestMethod]
        public void GetSupertrend_Uptrend_NoReversal()
        {
            // Verifies that a steady uptrend with no price drop below the lower band
            // keeps the trend at +1 and the Supertrend value tracks the lower band.
            //
            // Setup: uniform 2-point candles, period=3, multiplier=1.0
            // ATR stays constant at 2.0 because range and gaps are all equal.

            // Arrange
            // period=3, multiplier=1.0
            // ATR is constant at 2.0 (uniform 2-point range, no gaps).
            var high = new double[] { 10, 11, 12, 13 };
            var low = new double[] { 8, 9, 10, 11 };
            var close = new double[] { 9, 10, 11, 12 };

            // Act
            var result = Util.GetSupertrend(high, low, close, period: 3, multiplier: 1.0);

            // Assert
            // Trace:
            // ATR: all 2.0
            // Bands: lower = [7, 8, 9, 10], upper = [11, 12, 13, 14]
            // i=0: finalLower=7, trend=1
            // i=1: ratchet lower 8>7 → finalLower=8; close[1]=10 !< 8 → stay long
            // i=2: ratchet lower 9>8 → finalLower=9; close[2]=11 !< 9 → stay long
            // i=3: ratchet lower 10>9 → finalLower=10; close[3]=12 !< 10 → stay long
            Assert.HasCount(4, result.Values);
            Assert.HasCount(4, result.Trend);

            foreach (var t in result.Trend)
            {
                Assert.AreEqual(1, t);
            }
            Assert.AreEqual(7, result.Values[0]);
            Assert.AreEqual(8, result.Values[1]);
            Assert.AreEqual(9, result.Values[2]);
            Assert.AreEqual(10, result.Values[3]);
        }

        [TestMethod]
        public void GetSupertrend_Reversal_UpToDown()
        {  
           // Verifies that a sharp price drop (close breaks below the lower band)
           // flips the trend from +1 to -1, and the Supertrend value switches
           // from tracking the lower band to tracking the upper band.
           //
           // Setup: three uptrend bars followed by a large bearish bar, period=2, multiplier=1.0

            // Arrange
            // period=2, multiplier=1.0
            // Big drop on bar 3 triggers close < finalLower
            var high = new double[] { 10, 11, 12, 8 };
            var low = new double[] { 8, 9, 10, 6 };
            var close = new double[] { 9, 10, 11, 7 };

            // Act
            var result = Util.GetSupertrend(high, low, close, period: 2, multiplier: 1.0);

            // Assert
            // Trace:
            // ATR: [2, 2, 2, 3.5]
            //   i=3: tr=max(2,|8-11|,|6-11|)=5; atr=(2+5)/2=3.5
            // Bands: lower=[7,8,9,3.5], upper=[11,12,13,10.5]
            // i=0: finalLower=7, trend=1
            // i=1: ratchet lower 8>7→8; close[1]=10 !< 8 → long
            // i=2: ratchet lower 9>8→9; close[2]=11 !< 9 → long
            // i=3: ratchet upper 10.5<11→10.5; ratchet lower 3.5>9? No → finalLower=9
            //      close[3]=7 < 9 → REVERSAL, trend=-1
            //      values[3]=finalUpper=10.5
            Assert.AreEqual(1, result.Trend[0]);
            Assert.AreEqual(1, result.Trend[1]);
            Assert.AreEqual(1, result.Trend[2]);
            Assert.AreEqual(-1, result.Trend[3]); // reversed

            Assert.AreEqual(7, result.Values[0]);
            Assert.AreEqual(8, result.Values[1]);
            Assert.AreEqual(9, result.Values[2]);
            Assert.AreEqual(10.5, result.Values[3]); // now tracking upper band
        }

        [TestMethod]
        public void GetSupertrend_RatchetUpper_ResetsOnCloseAbove()
        {
            // Arrange
            // Verifies the `close[i-1] > finalUpper` branch of the upper ratchet.
            // period=2, multiplier=1.0
            var high = new double[] { 10, 15, 16 };
            var low = new double[] { 8, 13, 14 };
            var close = new double[] { 9, 14, 15 };

            // Act
            var result = Util.GetSupertrend(high, low, close, period: 2, multiplier: 1.0);

            // Assert
            // ATR: [2, 4, 3]
            //   i=1: tr=max(2,6,4)=6; atr=(2+6)/2=4
            //   i=2: tr=max(2,2,0)=2; atr=(4+2)/2=3
            // Bands: upper=[11,18,18], lower=[7,10,12]
            // i=0: finalUpper=11, finalLower=7, trend=1
            // i=1: ratchet upper: 18<11? No. close[0]=9>11? No → stays 11
            //      ratchet lower: 10>7 → finalLower=10
            //      close[1]=14 !< 10 → long. values[1]=10
            // i=2: ratchet upper: 18<11? No. close[1]=14>11? YES → finalUpper=18
            //      ratchet lower: 12>10 → finalLower=12
            //      close[2]=15 !< 12 → long. values[2]=12
            Assert.AreEqual(1, result.Trend[0]);
            Assert.AreEqual(1, result.Trend[1]);
            Assert.AreEqual(1, result.Trend[2]);
            Assert.AreEqual(7, result.Values[0]);
            Assert.AreEqual(10, result.Values[1]);
            Assert.AreEqual(12, result.Values[2]);
        }

        [TestMethod]
        public void GetSupertrend_SingleBar_ReturnsInitial()
        {
            // Arrange
            var high = new double[] { 10 };
            var low = new double[] { 8 };
            var close = new double[] { 9 };

            // Act
            var result = Util.GetSupertrend(high, low, close, period: 10, multiplier: 3.0);

            // Assert
            // atr=2, hl2=9, lower=9-6=3, trend=1
            Assert.HasCount(1, result.Values);
            Assert.HasCount(1, result.Trend);
            Assert.AreEqual(3, result.Values[0]);
            Assert.AreEqual(1, result.Trend[0]);
        }

    }
}
