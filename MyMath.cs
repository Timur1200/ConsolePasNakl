using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolePasNakl
{
    using System;
    using System.Text;

    public static class MyMath
    {
        // Тангенс
        public static double Tg(double angle)
        {
            double c = Math.Cos(angle);
            if (c == 0) return 99999999;
            return Math.Sin(angle) / c;
        }

        // Котангенс
        public static double Ctg(double angle)
        {
            double s = Math.Sin(angle);
            if (s == 0) return 99999999;
            return Math.Cos(angle) / s;
        }

        // Секанс
        public static double Sec(double angle)
        {
            double s = Math.Cos(angle);
            if (s == 0) return 99999999;
            return 1 / s;
        }

        // Косеканс
        public static double CoSec(double angle)
        {
            double s = Math.Sin(angle);
            if (s == 0) return 99999999;
            return 1 / s;
        }

        // Получение целого числа из строки
        public static int GetInteger(string s)
        {
            int result;
            return int.TryParse(s, out result) ? result : 0;
        }

        // Получение числа с плавающей точкой из строки
        public static double GetFloat(string s)
        {
            // Заменяем все символы, которые не являются цифрами или минусом, на точку
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (char.IsDigit(c) || c == '-' || c == '.')
                    sb.Append(c);
                else
                    sb.Append('.');
            }

            double result;
            return double.TryParse(sb.ToString(), out result) ? result : 0;
        }

        // Удаление пробелов из строки
        public static string DelSpace(string s)
        {
            return s.Replace(" ", "");
        }

        // Преобразование строки в верхний регистр
        public static string UpStr(string s)
        {
            return s.ToUpper();
        }

        // Преобразование числа с плавающей точкой в строку с заданной точностью
        public static string RToStr(double r, int prec)
        {
            return r.ToString("F" + prec);
        }

        // Преобразование целого числа в строку
        public static string IntToStr(int x)
        {
            return x.ToString();
        }
    }
}
