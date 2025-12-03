using System;
using System.Collections.Generic;
using System.Text;

namespace WodToolkit.src.Common
{
    public class Common
    {
        /// <summary>
        /// 获取指定年份的生肖
        /// </summary>
        /// <param name="year">年份</param>
        /// <returns></returns>
        public static string getChineseZodiac(int year)
        {
            string[] zodiacs = new string[] { "鼠", "牛", "虎", "兔", "龙", "蛇", "马", "羊", "猴", "鸡", "狗", "猪" };
            int startYear = 1900;
            int offset = (year - startYear) % 12;
            return zodiacs[offset];
        }
    }
}
