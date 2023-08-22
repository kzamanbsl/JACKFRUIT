using System;

namespace KGERP.Utility
{
    public class SelectModel
    {
        public object Text { get; set; }
        public object Value { get; set; }
    }
    public class SelectModelType
    {
        public string Text { get; set; }
        public int Value { get; set; }
    }
    public class SelectDDLModel
    {
        public string Text { get; set; }
        public long Value { get; set; }
    }
    public class SelectModelInstallmentType
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public bool IsOneTime { get; set; }
        public double IntervalMonths { get; set; }
        public decimal Amount { get; set; }
    }
    public class SceduleInstallment
    {
        public string Text { get; set; }
        public string Title { get; set; }
        public long Value { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string StringDate { get; set; }

    }
}
