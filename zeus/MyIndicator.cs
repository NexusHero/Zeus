using ATAS.Indicators.Drawing;


namespace ATAS.Indicators.Technical
{
    using System.ComponentModel;
    using System.Drawing;
    using Utils.Common.Logging;

    //https://atas.net/volume-analysis/strategies-and-trading-patterns/programming-a-profitable-trading-strategy/

    [DisplayName("Zeus_Indicator")]
    public class MyIndicator : Indicators.Indicator
    {
        private decimal multiplier = 1.5m;
        private int barcount;
        private decimal takeprofit;
        private decimal stoploss;


        //Will be shown as parameters  in ATAS (indicatormanager)
        [DisplayName("Multiplier")]
        private decimal Multiplier
        {
            get { return multiplier; }
            set
            {
                multiplier = value;
                RecalculateValues();
            }
        }

        [DisplayName("Barcount")]
        private int Barcount
        {
            get { return barcount; }
            set
            {
                barcount = value;
                RecalculateValues();
            }
        }

        [DisplayName("StopLoss")]
        private decimal StopLoss
        {
            get { return stoploss; }
            set
            {
                stoploss = value;
                RecalculateValues();
            }
        }

        [DisplayName("TakeProfit")]
        private decimal TakeProfit
        {
            get { return takeprofit; }
            set
            {
                takeprofit = value;
                //This method recalculates all values when one of the indicator properties changes. 
                RecalculateValues();
            }
        }



        public List<DeltaInfo> deltaInfos = new List<DeltaInfo>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            deltaInfos.Clear();

            for (int i = 0; i < CurrentBar - 1; i++)
            {
                var candle = GetCandle(i);
                var delta = candle.Delta;
                DeltaInfo item = new DeltaInfo()
                {
                    Nubmer = i,
                    Delta = delta
                };
                deltaInfos.Add(item);
            }

            var positive = deltaInfos.Where(h => h.Delta > 0).Average(h => h.Delta);
            var negative = deltaInfos.Where(h => h.Delta < 0).Average(h => h.Delta);

            var ControlPositive = deltaInfos.Where(h => h.Delta > positive * Multiplier).ToList();
            var ControlNegative = deltaInfos.Where(h => h.Delta > negative * Multiplier).ToList();

            this.LogInfo($"Positive: {ControlPositive.Count()}");
            this.LogInfo($"Negative: {ControlNegative.Count()}");

            Pen pen = new Pen(Color.Black);
            pen.Width = 1;
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(125, 0, 195, 10));

            Pen pen2 = new Pen(Color.Black);
            pen2.Width = 1;
            System.Drawing.SolidBrush myBrush2 = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            myBrush2 = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(125, 255, 195, 10));



            foreach (var item in ControlPositive)
            {
                if (item.Nubmer + Barcount < CurrentBar - 1)
                {

                    var controlcandle = GetCandle(item.Nubmer);
                    var nextcandle = GetCandle(item.Nubmer + Barcount);

                    if (controlcandle.Open < controlcandle.Close)//проверка для растущих баров
                    {
                        if (controlcandle.Open < nextcandle.Close)
                        {

                            DrawingRectangle drawingRectangles = new DrawingRectangle(item.Nubmer, controlcandle.Open, CurrentBar - 1, controlcandle.Open - 10, pen, myBrush);
                            Rectangles.Add(drawingRectangles);

                            for (int i = item.Nubmer + Barcount; i < CurrentBar - 1; i++)
                            {
                                if (GetCandle(i).Low <= controlcandle.Open && GetCandle(i).Low > controlcandle.Open - SL)
                                {
                                    for (int y = i + 1; y < CurrentBar - 1; y++)
                                    {
                                        if (GetCandle(y).Low <= controlcandle.Open - SL)
                                        {
                                            item.Profit = SL * -1;
                                            break;

                                        }
                                        else
                                        if (GetCandle(y).High >= controlcandle.Open + TP)
                                        {
                                            item.Profit = TP;

                                            break;
                                        }
                                    }
                                    break;
                                }
                                else
                                if (GetCandle(i).Low <= controlcandle.Open && GetCandle(i).Low > controlcandle.Close - SL)
                                {
                                    item.Profit = SL * -1;

                                    break;
                                }

                            }

                        }


                    }
                }



            }

            foreach (var item in ControlNegative)
            {
                if (item.Nubmer + Barcount < CurrentBar - 1)
                {

                    var controlcandle = GetCandle(item.Nubmer);
                    var nextcandle = GetCandle(item.Nubmer + Barcount);

                    if (controlcandle.Close < controlcandle.Open)//проверика для нисходящих баров
                    {
                        if (controlcandle.Open > nextcandle.Close)
                        {

                            DrawingRectangle drawingRectangles = new DrawingRectangle(item.Nubmer, controlcandle.Open, CurrentBar - 1, controlcandle.Open + 10, pen2, myBrush2);
                            Rectangles.Add(drawingRectangles);

                            for (int i = item.Nubmer + Barcount; i < CurrentBar - 1; i++)
                            {
                                if (GetCandle(i).High >= controlcandle.Open && GetCandle(i).High < controlcandle.Open + SL)
                                {
                                    for (int y = i + 1; y < CurrentBar - 1; y++)
                                    {
                                        if (GetCandle(y).High >= controlcandle.Open + SL)
                                        {
                                            item.Profit = SL * -1;

                                            break;

                                        }
                                        else
                                        if (GetCandle(y).Low >= controlcandle.Open - TP)
                                        {
                                            item.Profit = TP;

                                            break;
                                        }
                                    }
                                    break;
                                }
                                else
                                 if (GetCandle(i).High >= controlcandle.Open && GetCandle(i).High > controlcandle.Open + SL)
                                {
                                    item.Profit = SL * -1;

                                    break;
                                }

                            }

                        }


                    }
                }



            }

            this.LogInfo($"Профит по положительной дельте: {ControlPositive.Sum(h => h.Profit)}");


            this.LogInfo($"Профит по отрицательной дельте: {ControlNegative.Sum(h => h.Profit)}");

            this.LogInfo($"Общая прибыль/убыток: {ControlPositive.Sum(h => h.Profit) + ControlNegative.Sum(h => h.Profit)}");

        }



        protected override void OnCalculate(int bar, decimal value)
        {
        }
    }

    public class DeltaInfo
    {
        public int Nubmer { get; set; }
        public decimal Delta { get; set; }
        public decimal Profit { get; set; }
    }
}


    }
}