
namespace ATAS.Indicators.Technical
{
    using ATAS.Indicators;
    using System.ComponentModel;
    using Utils.Common.Logging;

    [DisplayName("Zeus")]
    public class Zeus : Indicator
    {
        //this method will be executed in realtime for every single candle 
        protected override void OnCalculate(int bar, decimal value)
        {
            this.LogInfo("Start Calculating");
        }

        protected override void OnInitialize()
        {
            this.LogInfo("OnInitialize");

            //Global property CurrentBar
            var candle = GetCandle(CurrentBar - 1);
            if (candle.Close > candle.Open)
            {
                this.LogInfo($"Last bar is bullish");
            }
            else
            {
                this.LogInfo($"Last bar is bearish");
            }
        }
    }
}