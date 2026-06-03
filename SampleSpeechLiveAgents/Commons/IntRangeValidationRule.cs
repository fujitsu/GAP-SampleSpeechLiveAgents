using System;
using System.Globalization;
using System.Windows.Controls;


namespace SampleSpeechLiveAgents.Commons
{
    internal class IntRangeValidationRule : ValidationRule
    {
        public int Min { get; set; } = 0;
        public int Max { get; set; } = 128000;
        public bool AllowEmpty { get; set; } = false;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var str = (value ?? string.Empty).ToString().Trim();
            if (string.IsNullOrEmpty(str))
            {
                if (AllowEmpty) return ValidationResult.ValidResult;
                return new ValidationResult(false, "値を入力してください。");
            }

            if (!int.TryParse(str, out int v))
            {
                return new ValidationResult(false, "整数を入力してください。");
            }

            if (v < Min || v > Max)
            {
                return new ValidationResult(false, $"範囲は {Min} 〜 {Max} の整数です。");
            }

            return ValidationResult.ValidResult;
        }
    }
}