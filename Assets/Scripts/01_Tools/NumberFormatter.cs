public static class NumberFormatter
{
    //3자리 단위 수 마다 , 추가. 예를 들어 1234567 -> 1,234,567
    public static string ToCommaString(this int number)
    {
        return string.Format("{0:n0}", number);
    }

    public static string ToKString(this int number)
    {
        return (number / 1000f).ToString("0.#") + "K";
    }

    public static int TrimUnit(this int value, int unit)
    {
        return (value / unit) * unit;
    }
}
