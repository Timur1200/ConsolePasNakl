using System.Text;
using System.Text.Json;
using static ConsolePasNakl.Program;

namespace ConsolePasNakl
{
    internal class Program
    {
        private const string EOL = "\r\n";

        // Типы данных
        private delegate double ArrReal(int m, int n); // Заменим указатели на массивы делегатами для доступа к элементам

        private static int NLine;
        private static double Ro0;
        private static double Ro90;
        private static double dRo0;
        private static double dRo90;
        private static double MU;
        private static double P0;
        private static double dMu0y;
        private static double dMu90y;
        private static double G;
        private static double de1;
        private static double de2;
        private static double aPr0;
        private static double aPrK;

        static void LoadConfig(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<Data>(json);

            NLine = config.NLine;
            Ro0 = config.Ro0;
            Ro90 = config.Ro90;
            dRo0 = config.dRo0;
            dRo90 = config.dRo90;
            MU = config.MU;
            P0 = config.P0;
            dMu0y = config.dMu0y;
            dMu90y = config.dMu90y;
            G = config.G;
            de1 = config.de1;
            de2 = config.de2;
            aPr0 = config.aPr0;
            aPrK = config.aPrK;
        }

        // Переменные
        private static string WorkDir;   // Каталог, из которого запущена программа
        private static int ArrSize;


        private static double Alpha;

        private static double[] arX;    // Заменим указатели на массивы
        private static double[] arY;
        private static double[] arKsi;
        private static double[] arSig;

        private static string sStart;

        private static double sgOA;

        // Основные функции и процедуры

        // Присвоить массиву значение по индексам m и n
        private static void SetA(double[] arr, int m, int n, double value)
        {
            arr[n * (NLine + 1) + m] = value;
        }

        // Получить значение X по индексам m и n
        private static double X(int m, int n)
        {
            return arX[n * (NLine + 1) + m];
        }

        // Получить значение Y по индексам m и n
        private static double Y(int m, int n)
        {
            return arY[n * (NLine + 1) + m];
        }

        // Получить значение Ksi по индексам m и n
        private static double Ksi(int m, int n)
        {
            return arKsi[n * (NLine + 1) + m];
        }

        // Получить значение Sig по индексам m и n
        private static double Sig(int m, int n)
        {
            return arSig[n * (NLine + 1) + m];
        }

        // Функция Ro
        private static double Ro(double aKsi, double aY)
        {
            return (Ro0 + dRo0 * aY) * Math.Cos(aKsi) * Math.Cos(aKsi) +
                   (Ro90 + dRo90 * aY) * Math.Sin(aKsi) * Math.Sin(aKsi);
        }

        // Функция RoMN
        private static double Romn(int aM, int aN)
        {
            return Ro(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция k0
        private static double k0(double aY)
        {
            return Ro0 + dRo0 * aY;
        }

        // Функция k0mn
        private static double k0mn(int aM, int aN)
        {
            return k0(Y(aM, aN));
        }

        // Функция k90
        private static double k90(double aY)
        {
            return Ro90 + dRo90 * aY;
        }

        // Функция k90mn
        private static double k90mn(int aM, int aN)
        {
            return k90(Y(aM, aN));
        }

        private static double K(double aKsi, double aY)
        {
            return Tg(Ro(aKsi, aY)); // Tg - аналог функции tg из Pascal
        }

        // Функция Kmn
        private static double Kmn(int aM, int aN)
        {
            return K(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Ro_y
        private static double Ro_y(double aKsi, double aY)
        {
            return dRo0 * Math.Cos(aKsi) * Math.Cos(aKsi) + dRo90 * Math.Sin(aKsi) * Math.Sin(aKsi);
        }

        // Функция Ro_ymn
        private static double Ro_ymn(int aM, int aN)
        {
            return Ro_y(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция K_y
        private static double K_y(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            return Ro_y(aKsi, aY) / (Math.Cos(rro) * Math.Cos(rro));
        }

        // Функция K_ymn
        private static double K_ymn(int aM, int aN)
        {
            return K_y(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Ro_FiFi
        private static double Ro_FiFi(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            double zn = Math.Cos(rro) * Math.Cos(rro);
            double rff1 = 2 * (k0(aY) - k90(aY)) / (1 + Math.Pow(Tg(rro), 2)) * (1 + Math.Pow(Tg(rro), 2));
            double rff2 = Math.Sin(2 * aKsi) * Math.Sin(rro) / (Math.Pow(Math.Cos(rro), 3)) -
                          Math.Cos(2 * aKsi) / Math.Cos(rro);
            return rff1 * rff2;
        }

        // Вспомогательная функция Tg (аналог tg из Pascal)
        private static double Tg(double angle)
        {
            double c = Math.Cos(angle);
            if (c == 0) return 99999999; // Аналог обработки деления на ноль
            return Math.Sin(angle) / c;
        }


        // Функция Ro_FiFimn
        private static double Ro_FiFimn(int aM, int aN)
        {
            return Ro_FiFi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Ro_Fi
        private static double Ro_Fi(double aKsi, double aY)
        {
            return -((Ro0 + dRo0 * aY) - (Ro90 + dRo90 * aY)) * Math.Sin(2 * aKsi);
        }

        // Функция Ro_Fimn
        private static double Ro_Fimn(int aM, int aN)
        {
            return Ro_Fi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция K_FiFi
        private static double K_FiFi(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            return (Ro_FiFi(aKsi, aY) * Math.Cos(rro) + 2 * Math.Sin(rro) * Ro_Fi(aKsi, aY) * Ro_Fi(aKsi, aY)) /
                   (Math.Pow(Math.Cos(rro), 3));
        }

        // Функция K_FiFimn
        private static double K_FiFimn(int aM, int aN)
        {
            return K_FiFi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Ro_Fiy
        private static double Ro_Fiy(double aKsi, double aY)
        {
            return -(dRo0 - dRo90) * Math.Sin(2 * aKsi);
        }

        // Функция Ro_Fiymn
        private static double Ro_Fiymn(int aM, int aN)
        {
            return Ro_Fiy(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция K_Fiy
        private static double K_Fiy(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            return (Ro_Fiy(aKsi, aY) * Math.Cos(rro) + 2 * Math.Sin(rro) * Ro_Fi(aKsi, aY) * Ro_y(aKsi, aY)) /
                   (Math.Pow(Math.Cos(rro), 3));
        }

        // Функция K_Fiymn
        private static double K_Fiymn(int aM, int aN)
        {
            return K_Fiy(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция K_Fi
        private static double K_Fi(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            return Ro_Fi(aKsi, aY) / (Math.Cos(rro) * Math.Cos(rro));
        }

        // Функция K_Fimn
        private static double K_Fimn(int aM, int aN)
        {
            return K_Fi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция C
        private static double C(double aKsi, double aY)
        {
            return 0.5 * (1 + MU + (dMu0y + dMu90y) * aY) +
                   0.5 * (1 - MU + (dMu0y - dMu90y) * aY) * Math.Cos(2 * aKsi);
        }

        private static double Cmn(int aM, int aN)
        {
            return C(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция C_Fi
        private static double C_Fi(double aKsi, double aY)
        {
            return -(1 - MU + (dMu0y - dMu90y) * aY) * Math.Sin(2 * aKsi);
        }

        // Функция C_Fimn
        private static double C_Fimn(int aM, int aN)
        {
            return C_Fi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция C_FiFi
        private static double C_FiFi(double aKsi, double aY)
        {
            return -2 * (1 - MU + (dMu0y - dMu90y) * aY) * Math.Cos(2 * aKsi);
        }

        // Функция C_y
        private static double C_y(double aKsi, double aY)
        {
            return 0.5 * (dMu0y + dMu90y) + 0.5 * (dMu0y - dMu90y) * Math.Cos(2 * aKsi);
        }

        // Функция C_Fiy
        private static double C_Fiy(double aKsi, double aY)
        {
            return -(dMu0y - dMu90y) * Math.Sin(2 * aKsi);
        }

        // Функция C_ymn
        private static double C_ymn(int aM, int aN)
        {
            return C_y(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция C_FiFimn
        private static double C_FiFimn(int aM, int aN)
        {
            return C_FiFi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция C_Fiymn
        private static double C_Fiymn(int aM, int aN)
        {
            return C_Fiy(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция A
        private static double A(double aKsi, double aY)
        {
            double rf = Ro_Fi(aKsi, aY);
            return rf / (2 - rf);
        }

        // Функция Amn
        private static double Amn(int aM, int aN)
        {
            return A(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Eta
        private static double Eta(double aKsi, double aY)
        {
            return Math.Sin(Ro(aKsi, aY)) / (1 - 0.5 * Ro_Fi(aKsi, aY));
        }

        // Функция Etamn
        private static double Etamn(int aM, int aN)
        {
            return Eta(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция H
        private static double H(double aKsi, double aY)
        {
            return Eta(aKsi, aY) * Math.Sin(2 * aKsi - Ro(aKsi, aY)) + A(aKsi, aY) * Math.Cos(2 * aKsi);
        }

        // Функция Hmn
        private static double Hmn(int aM, int aN)
        {
            return H(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция N
        private static double N(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            return Math.Sin(2 * aKsi - rro) - 0.5 * Ro_Fi(aKsi, aY) * Sec(rro) * Math.Sin(2 * aKsi);
        }

        // Функция Nmn
        private static double Nmn(int aM, int aN)
        {
            return N(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция M
        private static double M(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            return Math.Cos(2 * aKsi - rro) - 0.5 * Ro_Fi(aKsi, aY) * Sec(rro) * Math.Cos(2 * aKsi);
        }

        // Функция Mmn
        private static double Mmn(int aM, int aN)
        {
            return M(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Xi
        private static double Xi(double aKsi, double aY)
        {
            return Math.Cos(Ro(aKsi, aY)) / (1 - 0.5 * Ro_Fi(aKsi, aY));
        }

        // Функция Ximn
        private static double Ximn(int aM, int aN)
        {
            return Xi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Xi_Fi
        private static double Xi_Fi(double aKsi, double aY)
        {
            double rro = Ro(aKsi, aY);
            double rro_fi = Ro_Fi(aKsi, aY);
            double xf = 0.5 * Ro_FiFi(aKsi, aY) * Math.Cos(rro) - rro_fi * (1 - 0.5 * rro_fi) * Math.Sin(rro);
            return xf / ((1 - 0.5 * rro_fi) * (1 - 0.5 * rro_fi));
        }

        // Функция Xi_Fimn
        private static double Xi_Fimn(int aM, int aN)
        {
            return Xi_Fi(Ksi(aM, aN), Y(aM, aN));
        }

        // Функция Smn
        private static double Smn(int aM, int aN)
        {
            return Sig(aM, aN) * Kmn(aM, aN) + Cmn(aM, aN);
        }

        // Функция S_Fimn
        private static double S_Fimn(int aM, int aN)
        {
            return Sig(aM, aN) * K_Fimn(aM, aN) + C_Fimn(aM, aN);
        }

        // Функция Fmn
        private static double Fmn(int aM, int aN)
        {
            return Smn(aM, aN) * Xi_Fimn(aM, aN) +
                   0.5 * S_Fimn(aM, aN) * Ximn(aM, aN) * Ro_Fimn(aM, aN);
        }

        // Функция S_FiFimn
        private static double S_FiFimn(int aM, int aN)
        {
            return Sig(aM, aN) * K_FiFimn(aM, aN) + C_FiFimn(aM, aN);
        }

        // Функция Dmn
        private static double Dmn(int aM, int aN)
        {
            return 0.5 * S_FiFimn(aM, aN) * Ximn(aM, aN) + 0.5 * S_Fimn(aM, aN) * Xi_Fimn(aM, aN);
        }

        // Функция Bmn
        private static double Bmn(int aM, int aN)
        {
            double rr = Ro(Ksi(aM, aN), Y(aM, aN));
            double rf = Ro_Fi(Ksi(aM, aN), Y(aM, aN));
            double rff = Ro_FiFi(Ksi(aM, aN), Y(aM, aN));
            return (rff + rf * rf * Tg(rr)) / (2 - rf);
        }

        // Функция Lmn
        private static double Lmn(int aM, int aN)
        {
            return 0.5 * (S_Fimn(aM, aN) * Ximn(aM, aN) + Smn(aM, aN) * Xi_Fimn(aM, aN)) * Ro_Fimn(aM, aN) * Sec(Romn(aM, aN));
        }

        // Функция Numn
        private static double Numn(int aM, int aN)
        {
            return Ximn(aM, aN) * Tg(Romn(aM, aN));
        }

        // Функция Pmn
        private static double Pmn(int aM, int aN)
        {
            double ss = Smn(aM, aN);
            double rr = Romn(aM, aN);
            return (2 * ss * Math.Cos(rr) + Dmn(aM, aN)) * Math.Cos(2 * Ksi(aM, aN) - rr) +
                   Fmn(aM, aN) * Math.Sin(2 * Ksi(aM, aN) - rr) -
                   2 * ss * Amn(aM, aN) * Math.Cos(2 * Ksi(aM, aN)) -
                   (ss * Bmn(aM, aN) + Lmn(aM, aN)) * Math.Sin(2 * Ksi(aM, aN));
        }

        // Функция Qmn
        private static double Qmn(int aM, int aN)
        {
            double ss = Smn(aM, aN);
            double rr = Romn(aM, aN);
            return (2 * ss * Math.Cos(rr) + Dmn(aM, aN)) * Math.Sin(2 * Ksi(aM, aN) - rr) -
                   Fmn(aM, aN) * Math.Cos(2 * Ksi(aM, aN) - rr) -
                   2 * ss * Amn(aM, aN) * Math.Sin(2 * Ksi(aM, aN)) +
                   (ss * Bmn(aM, aN) + Lmn(aM, aN)) * Math.Cos(2 * Ksi(aM, aN));
        }

        // Функция Tmn
        private static double Tmn(int aM, int aN)
        {
            double ss = Smn(aM, aN);
            double rr = Romn(aM, aN);
            return (2 * ss * Math.Cos(rr) + Dmn(aM, aN)) * Math.Sin(rr) -
                   (2 * ss * Numn(aM, aN) + Fmn(aM, aN)) * Amn(aM, aN) * Math.Cos(rr) +
                   (ss * Bmn(aM, aN) + Lmn(aM, aN)) * (Amn(aM, aN) - Numn(aM, aN) * Math.Sin(rr));
        }



        // Вспомогательная функция Sec (аналог sec из Pascal)
        private static double Sec(double angle)
        {
            double c = Math.Cos(angle);
            if (c == 0) return 99999999; // Аналог обработки деления на ноль
            return 1 / c;
        }
        // Функция M1mn
        private static double M1mn(int aM, int aN)
        {
            double pp = Pmn(aM - 1, aN);
            double qq = Qmn(aM - 1, aN);
            double tt = Tmn(aM - 1, aN);
            return (-pp + Math.Sqrt(pp * pp + qq * qq - tt * tt)) / (qq + tt);
        }

        // Функция M2mn
        private static double M2mn(int aM, int aN)
        {
            double pp = Pmn(aM, aN - 1);
            double qq = Qmn(aM, aN - 1);
            double tt = Tmn(aM, aN - 1);
            return (-pp - Math.Sqrt(pp * pp + qq * qq - tt * tt)) / (qq + tt);
        }

        // Функция G1mn
        private static double G1mn(int aM, int aN)
        {
            double pp = Pmn(aM - 1, aN);
            double qq = Qmn(aM - 1, aN);
            return pp * pp + qq * qq;
        }

        // Функция G2mn
        private static double G2mn(int aM, int aN)
        {
            double pp = Pmn(aM, aN - 1);
            double qq = Qmn(aM, aN - 1);
            return pp * pp + qq * qq;
        }

        // Функция Nemn
        private static double Nemn(int aM, int aN)
        {
            return Math.Cos(2 * Ksi(aM, aN) - Romn(aM, aN)) -
                   0.5 * Math.Cos(2 * Ksi(aM, aN)) * Math.Cos(Romn(aM, aN)) * K_Fimn(aM, aN);
        }

        // Функция Nkmn
        private static double Nkmn(int aM, int aN)
        {
            return Math.Sin(2 * Ksi(aM, aN) - Romn(aM, aN)) -
                   0.5 * Math.Sin(2 * Ksi(aM, aN)) * Math.Cos(Romn(aM, aN)) * K_Fimn(aM, aN);
        }

        // Функция Hk_ymn
        private static double Hk_ymn(int aM, int aN)
        {
            return 0.5 * Ximn(aM, aN) * Ro_Fiymn(aM, aN) * Sec(Romn(aM, aN)) - Ro_ymn(aM, aN) * Kmn(aM, aN);
        }

        // Функция Zmn
        private static double Zmn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            return Etamn(aM, aN) * Math.Cos(2 * kks - rro) - Amn(aM, aN) * Math.Sin(2 * kks);
        }

        // Функция S_Fiymn
        private static double S_Fiymn(int aM, int aN)
        {
            return Sig(aM, aN) * K_Fiymn(aM, aN) + C_Fiymn(aM, aN);
        }

        // Функция S_ymn
        private static double S_ymn(int aM, int aN)
        {
            return Sig(aM, aN) * K_ymn(aM, aN) + C_ymn(aM, aN);
        }

        // Функция Temn
        private static double Temn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ss = Smn(aM, aN);
            return ss * Math.Sin(2 * kks - rro);
        }

        // Функция Remn
        private static double Remn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double ss = Smn(aM, aN);
            return 0.5 * ss * Sec(rro) * rf * Math.Sin(2 * kks);
        }

        // Функция demn
        private static double demn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double sf = S_Fimn(aM, aN);
            return 0.5 * sf * Math.Cos(2 * kks - rro);
        }

        // Функция Te0_ymn
        private static double Te0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double sy = S_ymn(aM, aN);
            double ss = Smn(aM, aN);
            return sy * Math.Sin(2 * kks - rro) - ss * Math.Cos(2 * kks - rro) * Ro_ymn(aM, aN);
        }

        // Функция Re0_ymn
        private static double Re0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ry = Ro_ymn(aM, aN);
            double sy = S_ymn(aM, aN);
            double ss = Smn(aM, aN);
            double rfy = Ro_Fiymn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double Re1 = 0.5 * sy * rf + 0.5 * ss * Tg(rro) * ry * rf + 0.5 * ss * rfy;
            return Re1 * Math.Sin(2 * kks) * Sec(rro);
        }

        // Функция de0_ymn
        private static double de0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ry = Ro_ymn(aM, aN);
            double sf = S_Fimn(aM, aN);
            double sfy = S_Fiymn(aM, aN);
            return 0.5 * sfy * Math.Cos(2 * kks - rro) + 0.5 * sf * Math.Sin(2 * kks - rro) * ry;
        }

        // Функция Xi0_ymn
        private static double Xi0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ry = Ro_ymn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double rfy = Ro_Fiymn(aM, aN);
            return (-Math.Sin(rro) * ry * (1 - 0.5 * rf) + 0.5 * rfy * Math.Cos(rro)) / ((1 - 0.5 * rf) * (1 - 0.5 * rf));
        }

        private static double H2mn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            return Xi0_ymn(aM, aN) * (Temn(aM, aN) - Remn(aM, aN) + demn(aM, aN)) +
                   Ximn(aM, aN) * (Te0_ymn(aM, aN) - Re0_ymn(aM, aN) + de0_ymn(aM, aN));
        }

        // Функция Bemn
        private static double Bemn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ss = Smn(aM, aN);
            return ss * Math.Cos(2 * kks - rro);
        }

        // Функция DBemn
        private static double DBemn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double ss = Smn(aM, aN);
            return 0.5 * ss * rf * Sec(rro) * Math.Cos(2 * kks);
        }

        // Функция pcmn
        private static double pcmn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double sf = S_Fimn(aM, aN);
            return 0.5 * sf * Math.Sin(2 * kks - rro);
        }

        // Функция Be0_ymn
        private static double Be0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double sy = S_ymn(aM, aN);
            double ss = Smn(aM, aN);
            return sy * Math.Cos(2 * kks - rro) + ss * Math.Sin(2 * kks - rro) * Ro_ymn(aM, aN);
        }

        // Функция DBe0_ymn
        private static double DBe0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ry = Ro_ymn(aM, aN);
            double sy = S_ymn(aM, aN);
            double ss = Smn(aM, aN);
            double rfy = Ro_Fiymn(aM, aN);
            double rf = Ro_Fimn(aM, aN);
            double Re1 = 0.5 * sy * rf + 0.5 * ss * Tg(rro) * ry * rf + 0.5 * ss * rfy;
            return Re1 * Math.Cos(2 * kks) * Sec(rro);
        }

        // Функция pc0_ymn
        private static double pc0_ymn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            double ry = Ro_ymn(aM, aN);
            double sf = S_Fimn(aM, aN);
            double sfy = S_Fiymn(aM, aN);
            return 0.5 * sfy * Math.Sin(2 * kks - rro) - 0.5 * sf * Math.Cos(2 * kks - rro) * ry;
        }
        // Функция H1mn
        private static double H1mn(int aM, int aN)
        {
            double kks = Ksi(aM, aN);
            double rro = Romn(aM, aN);
            return Xi0_ymn(aM, aN) * (Bemn(aM, aN) - DBemn(aM, aN) + pcmn(aM, aN)) +
                   Ximn(aM, aN) * (Be0_ymn(aM, aN) - DBe0_ymn(aM, aN) + pc0_ymn(aM, aN));
        }

        // Функция D1mn
        private static double D1mn(int aM, int aN)
        {
            double kks = Ksi(aM - 1, aN);
            double rro = Romn(aM - 1, aN);
            double zz = Zmn(aM - 1, aN);
            double qq = Qmn(aM - 1, aN);
            double hh = Hmn(aM - 1, aN);
            double pp = Pmn(aM - 1, aN);
            double mm1 = M1mn(aM, aN);
            return zz * qq + (1 - hh) * pp + (zz * pp - (1 - hh) * qq) / mm1;
        }

        // Функция D2mn
        private static double D2mn(int aM, int aN)
        {
            double kks = Ksi(aM, aN - 1);
            double rro = Romn(aM, aN - 1);
            double zz = Zmn(aM, aN - 1);
            double qq = Qmn(aM, aN - 1);
            double hh = Hmn(aM, aN - 1);
            double pp = Pmn(aM, aN - 1);
            double mm2 = M2mn(aM, aN);
            return zz * qq + (1 - hh) * pp + (zz * pp - (1 - hh) * qq) / mm2;
        }

        // Функция T1mn
        private static double T1mn(int aM, int aN)
        {
            double hh1 = H1mn(aM - 1, aN);
            double hh2 = H2mn(aM - 1, aN);
            double qq = Qmn(aM - 1, aN);
            double pp = Pmn(aM - 1, aN);
            double xxx = -(hh2 * qq + hh1 * pp) * (X(aM, aN) - X(aM - 1, aN)) +
                         (hh2 * pp - hh1 * qq) * (Y(aM, aN) - Y(aM - 1, aN)) -
                         G * qq * (X(aM, aN) - X(aM - 1, aN)) + G * pp * (Y(aM, aN) - Y(aM - 1, aN));
            return xxx;
        }

        // Функция T2mn
        private static double T2mn(int aM, int aN)
        {
            double hh1 = H1mn(aM, aN - 1);
            double hh2 = H2mn(aM, aN - 1);
            double qq = Qmn(aM, aN - 1);
            double pp = Pmn(aM, aN - 1);
            double xxx = -(hh2 * qq + hh1 * pp) * (X(aM, aN) - X(aM, aN - 1)) +
                         (hh2 * pp - hh1 * qq) * (Y(aM, aN) - Y(aM, aN - 1)) -
                         G * qq * (X(aM, aN) - X(aM, aN - 1)) + G * pp * (Y(aM, aN) - Y(aM, aN - 1));
            return xxx;
        }

        // Функция Xmn
        private static double Xmn(int aM, int aN)
        {
            double mm1 = M1mn(aM, aN);
            double mm2 = M2mn(aM, aN);
            double xn1 = X(aM, aN - 1);
            double yn1 = Y(aM, aN - 1);
            double xm1 = X(aM - 1, aN);
            double ym1 = Y(aM - 1, aN);
            return (Y(aM, aN - 1) - Y(aM - 1, aN) + X(aM - 1, aN) * mm1 - X(aM, aN - 1) * mm2) / (mm1 - mm2);
        }

        // Функция Ymn
        private static double Ymn(int aM, int aN)
        {
            double yy = Y(aM - 1, aN);
            double mm1 = M1mn(aM, aN);
            double xx1 = X(aM, aN);
            double xx2 = X(aM - 1, aN);
            return yy + (X(aM, aN) - X(aM - 1, aN)) * mm1;
        }
        // Функция Ksimn
        private static double Ksimn(int aM, int aN)
        {
            double dd2 = D2mn(aM, aN);
            double dd1 = D1mn(aM, aN);
            double gg2 = G2mn(aM, aN);
            double gg1 = G1mn(aM, aN);

            double ka2 = gg2 / dd2 - gg1 / dd1;
            double ka1 = Sig(aM - 1, aN) - Sig(aM, aN - 1) +
                         T1mn(aM, aN) / dd1 -
                         T2mn(aM, aN) / dd2 +
                         gg2 * Ksi(aM, aN - 1) / dd2 -
                         gg1 * Ksi(aM - 1, aN) / dd1;

            return ka1 / ka2;
        }

        // Функция Sigmn
        private static double Sigmn(int aM, int aN)
        {
            double dd2 = D2mn(aM, aN);
            double tt2 = T2mn(aM, aN);
            double gg2 = G2mn(aM, aN);
            double sm1 = Sig(aM, aN - 1);
            double xx = sm1 + gg2 * Ksi(aM, aN) / dd2 - gg2 * Ksi(aM, aN - 1) / dd2 + tt2 / dd2;
            return xx;
        }

        public class Data
        {
            public int NLine { get; set; } // Количество точек для разбиения
            public double Ro0 { get; set; } // Начальное значение Ro0
            public double Ro90 { get; set; } // Начальное значение Ro90
            public double dRo0 { get; set; } // Изменение Ro0
            public double dRo90 { get; set; } // Изменение Ro90
            public double MU { get; set; } // Коэффициент MU
            public double P0 { get; set; } // Начальное значение P0
            public double dMu0y { get; set; } // Изменение Mu0y
            public double dMu90y { get; set; } // Изменение Mu90y
            public double G { get; set; } // Коэффициент G
            public double de1 { get; set; } // Угол de1
            public double de2 { get; set; } // Угол de2
            public double aPr0 { get; set; } // Нижний предел для kp
            public double aPrK { get; set; } // Верхний предел для kp
        }
        private static Data data = new Data();


        // Вспомогательные функции

        // Функция DelSpace (удаление пробелов)
        private static string DelSpace(string s)
        {
            return s.Replace(" ", "");
        }

        // Функция GetInteger (преобразование строки в целое число)
        private static int GetInteger(string s)
        {
            int result;
            return int.TryParse(s, out result) ? result : 0;
        }

        // Функция GetFloat (преобразование строки в число с плавающей точкой)
        private static double GetFloat(string s)
        {
            double result;
            return double.TryParse(s, out result) ? result : 0;
        }

        // Функция RToStr (преобразование числа в строку с заданной точностью)
        private static string RToStr(double r, int prec)
        {
            return r.ToString($"F{prec}");
        }
        private static void Init()
        {
            ArrSize = (NLine + 1) * (NLine + 1) * sizeof(double);
            arX = new double[(NLine + 1) * (NLine + 1)];
            arY = new double[(NLine + 1) * (NLine + 1)];
            arKsi = new double[(NLine + 1) * (NLine + 1)];
            arSig = new double[(NLine + 1) * (NLine + 1)];

            Array.Fill(arX, 0);
            Array.Fill(arY, 0);
            Array.Fill(arKsi, 0);
            Array.Fill(arSig, 0);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Начало программы");
            Console.WriteLine(sStart);
        }

        // Процедура Done
        private static void Done()
        {
            arX = null;
            arY = null;
            arKsi = null;
            arSig = null;
            Console.WriteLine("Окончание программы");
        }

        // Процедура CalcOA
        public static class Obl1
        {


            public static List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();

            public static StringBuilder ToText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Obl1 Points ");
                foreach (var point in Obl1.DataPoints)
                {
                    stringBuilder.AppendLine($"M:N ={point.M,2} : {point.N,2}  {point.X:F4}  {point.Y:F4}  {point.Ksi:F4}  {point.Sig:F4}");
                }
                return stringBuilder;
            }
            public class DataPoint
            {
                public int M { get; set; }
                public int N { get; set; }
                public double X { get; set; }
                public double Y { get; set; }
                public double Ksi { get; set; }
                public double Sig { get; set; }
            }
        }
        private static void CalcOA()
        {
            bool L = false;
            double kp, sg2, sg1;
            double dKs = 0.0001 * Math.PI / 180;
            double ks = 55 * Math.PI / 180 - dKs;
            char Ch = '\0';

            double cos_de_1 = Math.Cos(de1);
            double sin_de_1 = Math.Sin(de1);
            double x_i_ks_0 = Xi(ks, 0);
            double n_ks_0 = N(ks, 0);
            double c_f_i_ks_0 = C_Fi(ks, 0);
            double k_f_i_ks_0 = K_Fi(ks, 0);
            double ro_ks_0 = Ro(ks, 0);
            double c_ks_0 = C(ks, 0);
            double m_ks_0 = M(ks, 0);
            double k_ks_0 = K(ks, 0);

            double cos_spec__ks__ro_ks_0 = Math.Cos(2 * ks - ro_ks_0);
            double sin_spec__ks__ro_ks_0 = Math.Sin(2 * ks - ro_ks_0);

            do
            {
                ks += dKs;

                // Промежуточные данные
                x_i_ks_0 = Xi(ks, 0);
                n_ks_0 = N(ks, 0);
                c_f_i_ks_0 = C_Fi(ks, 0);
                k_f_i_ks_0 = K_Fi(ks, 0);
                ro_ks_0 = Ro(ks, 0);
                c_ks_0 = C(ks, 0);
                m_ks_0 = M(ks, 0);
                k_ks_0 = K(ks, 0);
                cos_spec__ks__ro_ks_0 = Math.Cos(2 * ks - ro_ks_0);
                sin_spec__ks__ro_ks_0 = Math.Sin(2 * ks - ro_ks_0);

                sg1 = (P0 * cos_de_1 / x_i_ks_0 + c_ks_0 * n_ks_0 + 0.5 * c_f_i_ks_0 * cos_spec__ks__ro_ks_0) /
                              (1 / x_i_ks_0 - k_ks_0 * n_ks_0 - 0.5 * k_f_i_ks_0 * cos_spec__ks__ro_ks_0);
                sg2 = (c_f_i_ks_0 * sin_spec__ks__ro_ks_0 - P0 * sin_de_1 / x_i_ks_0 - 2 * c_ks_0 * m_ks_0) /
                              (2 * k_ks_0 * m_ks_0 - k_f_i_ks_0 * sin_spec__ks__ro_ks_0);
                kp = sg1 / sg2;

                if (Console.KeyAvailable)
                {
                    Ch = Console.ReadKey(true).KeyChar;
                }
                L = Ch == (char)27; // ESC

                Console.Write($"ks= {ks:F6} sg1 / sg2 ={sg1,8:F3} : {sg2,8:F3} = {kp:F4}\r");
            } while (!L && !(kp > aPr0 && kp < aPrK));

            if (L) Environment.Exit(0);

            sgOA = (sg1 + sg2) / 2;
            Console.WriteLine($"ks= {ks:F4} sg={sgOA:F4}");

            for (int zi = 0; zi <= NLine; zi++)
            {
                int mi = zi;
                int ni = NLine - zi;
                SetA(arX, mi, ni, 1.0 / NLine * zi);
                SetA(arY, mi, ni, 0);
                SetA(arKsi, mi, ni, ks);
                SetA(arSig, mi, ni, sgOA);

                Obl1.DataPoints.Add(new Obl1.DataPoint
                {
                    M = mi,
                    N = ni,
                    X = X(mi, ni),
                    Y = Y(mi, ni),
                    Ksi = Ksi(mi, ni),
                    Sig = Sig(mi, ni)
                });
            }
        }

        private static void Calc1()
        {
            int NPoint = NLine + ((NLine - 1) * NLine / 2) - 1;
            int NWork = 0;

            for (int zi = 1; zi <= NLine; zi++)
            {
                for (int ki = zi; ki <= NLine; ki++)
                {
                    int mi = NLine - ki + zi;
                    int ni = ki;
                    Console.Write($"M:N ={mi,2} : {ni,2}     ({NWork * 100.0 / NPoint,4:F1}%)    \r");

                    SetA(arX, mi, ni, Xmn(mi, ni));
                    SetA(arY, mi, ni, Ymn(mi, ni));
                    SetA(arKsi, mi, ni, Ksimn(mi, ni));
                    SetA(arSig, mi, ni, Sigmn(mi, ni));

                    Obl1.DataPoints.Add(new Obl1.DataPoint
                    {
                        M = mi,
                        N = ni,
                        X = X(mi, ni),
                        Y = Y(mi, ni),
                        Ksi = Ksi(mi, ni),
                        Sig = Sig(mi, ni)
                    });

                    if (Console.KeyAvailable)
                    {
                        char Ch = Console.ReadKey(true).KeyChar;
                        if (Ch == (char)27) Environment.Exit(0); // ESC
                    }

                    NWork++;
                }
            }
        }

        // Процедура OutRes1
        private static void OutRes1()
        {
            // Выход из процедуры, так как в Pascal есть Exit
            return;
            /*
            using (StreamWriter writer = new StreamWriter("res1.pas"))
            {
                writer.WriteLine(" M:N   X   Y   Ksi   Sig");
                for (int zi = 1; zi <= NLine; zi++)
                {
                    for (int ki = zi; ki <= NLine; ki++)
                    {
                        int mi = NLine - ki + zi;
                        int ni = ki;
                        bool L = (mi == 1) || (mi % 5 == 0) || (mi == NLine) ||
                                 (ki == zi) || (zi % 5 == 0) || (zi == NLine);

                        if (L)
                        {
                            writer.WriteLine($"({mi,2}:{ni,2}) {X(mi, ni)}  {Y(mi, ni)}  {Ksi(mi, ni)}  {Sig(mi, ni)}");
                        }

                        if (ki == NLine) writer.WriteLine();
                    }
                }
            }*/
        }

        // Процедура Move12
        private static void Move12()
        {
            for (int mm = 0; mm <= NLine; mm++)
            {
                // Перенос крайних точек
                SetA(arX, mm, 0, X(mm, NLine));
                SetA(arY, mm, 0, Y(mm, NLine));
                SetA(arKsi, mm, 0, Ksi(mm, NLine));
                SetA(arSig, mm, 0, Sig(mm, NLine));

                // Обнуление оставшегося участка
                for (int nn = 1; nn <= NLine; nn++)
                {
                    SetA(arX, mm, nn, 0);
                    SetA(arY, mm, nn, 0);
                    SetA(arKsi, mm, nn, 0);
                    SetA(arSig, mm, nn, 0);
                }
            }
        }

        // Процедура CalcOO
        public static class Obl2
        {
            public static List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();

            public static StringBuilder ToText()
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("    M :  N      X       Y       Ksi      Sig");
                foreach (var point in Obl2.DataPoints)
                {
                    stringBuilder.AppendLine($"M:N ={point.M,2} : {point.N,2}  {point.X,7:F4}  {point.Y,7:F4}  {point.Ksi,7:F4}  {point.Sig,7:F4}");
                }
                return stringBuilder;
            }

            public class DataPoint
            {
                public int M { get; set; }
                public int N { get; set; }
                public double X { get; set; }
                public double Y { get; set; }
                public double Ksi { get; set; }
                public double Sig { get; set; }
            }
        }

        private static void CalcOO()
        {
            double dKs = 0.1;
            double ks = Ksi(0, 0) + 0.1;
            char Ch = '\0';
            bool L = false;
            double kp;

            // Мухлеж - для 1 sig-формулы m,n-1 => используем точку 0,2 считаем 0,3
            SetA(arX, 0, 2, X(0, 0));
            SetA(arY, 0, 2, Y(0, 0));
            SetA(arY, 0, 3, 0);
            SetA(arKsi, 0, 2, Ksi(0, 0));
            SetA(arSig, 0, 2, Sig(0, 0));

            double tg_de_2 = Tg(de2);

            double c_fi_ks_0 = C_Fi(ks, 0);
            double ro_ks_0 = Ro(ks, 0);
            double k_fi_ks_0 = K_Fi(ks, 0);
            double k_ks_0 = K(ks, 0);
            double m_ks_0 = M(ks, 0);
            double c_ks_0 = C(ks, 0);
            double n_ks_0 = N(ks, 0);
            double x_i_ks_0 = Xi(ks, 0);

            double sin_spec__ks__de_2__ro_ks_0 = Math.Sin(2 * ks - de2 - ro_ks_0);
            double cos_spec__ks__de_2__ro_ks_0 = Math.Cos(2 * ks - de2 - ro_ks_0);

            do
            {
                ks += dKs;
                if (ks < 2.35) dKs = 0.03; else dKs = 0.00005;

                // Промежуточные вычисления
                c_fi_ks_0 = C_Fi(ks, 0);
                ro_ks_0 = Ro(ks, 0);
                k_fi_ks_0 = K_Fi(ks, 0);
                k_ks_0 = K(ks, 0);
                m_ks_0 = M(ks, 0);
                c_ks_0 = C(ks, 0);
                n_ks_0 = N(ks, 0);
                x_i_ks_0 = Xi(ks, 0);

                cos_spec__ks__de_2__ro_ks_0 = Math.Cos(2 * ks - de2 - ro_ks_0);
                sin_spec__ks__de_2__ro_ks_0 = Math.Sin(2 * ks - de2 - ro_ks_0);

                double sg1 = Sig(0, 2) + G2mn(0, 3) * dKs / D2mn(0, 3);
                double sg2 = ((c_ks_0 * n_ks_0 + 0.5 * c_fi_ks_0 * cos_spec__ks__de_2__ro_ks_0) * tg_de_2 +
                              0.5 * (c_fi_ks_0 * sin_spec__ks__de_2__ro_ks_0 - 2 * c_ks_0 * m_ks_0)) /
                              ((1 / x_i_ks_0 - k_ks_0 * n_ks_0 + 0.5 * k_fi_ks_0 * cos_spec__ks__de_2__ro_ks_0) * tg_de_2 +
                               0.5 * (2 * k_ks_0 * m_ks_0 - k_fi_ks_0 * sin_spec__ks__de_2__ro_ks_0));

                SetA(arKsi, 0, 2, ks);
                SetA(arSig, 0, 2, sg1);

                kp = sg1 / sg2;

                if (Console.KeyAvailable)
                {
                    Ch = Console.ReadKey(true).KeyChar;
                }
                L = Ch == (char)27; // ESC

                Console.Write($"ks= {ks:F6} sg1 / sg2 ={sg1,8:F3} : {sg2,8:F3} = {kp:F4}\r");
            } while (!L && !(kp > aPr0 && kp < aPrK));

            if (L) Environment.Exit(0);

            Console.WriteLine();

            SetA(arKsi, 0, 2, 0);
            SetA(arSig, 0, 2, 0);

            double sn = Sig(0, 0);
            for (int i = 1; i <= NLine; i++)
            {
                SetA(arKsi, 0, i, Ksi(0, 0) + i * dKs);
                sn += G2mn(0, i) * dKs / D2mn(0, i);
                SetA(arSig, 0, i, sn);

                Obl2.DataPoints.Add(new Obl2.DataPoint
                {
                    M = 0,
                    N = i,
                    X = X(0, i),
                    Y = Y(0, i),
                    Ksi = Ksi(0, i),
                    Sig = Sig(0, i)
                });
            }

            for (int i = 0; i <= NLine; i++)
            {
                SetA(arX, 0, i, 0);
                SetA(arY, 0, i, 0);
            }
        }

        private static void Calc2()
        {
            int NPoint = NLine * NLine;
            int NWork = 0;

            for (int ni = 1; ni <= NLine; ni++)
            {
                for (int mi = 1; mi <= NLine; mi++)
                {
                    if (mi == 1)
                    {
                        Obl2.DataPoints.Add(new Obl2.DataPoint
                        {
                            M = 0,
                            N = ni,
                            X = X(0, ni),
                            Y = Y(0, ni),
                            Ksi = Ksi(0, ni),
                            Sig = Sig(0, ni)
                        });
                    }

                    Console.Write($"M:N ={mi,2} : {ni,2}     ({NWork * 100.0 / NPoint,4:F1}%)    \r");

                    SetA(arX, mi, ni, Xmn(mi, ni));
                    SetA(arY, mi, ni, Ymn(mi, ni));
                    SetA(arKsi, mi, ni, Ksimn(mi, ni));
                    SetA(arSig, mi, ni, Sigmn(mi, ni));

                    Obl2.DataPoints.Add(new Obl2.DataPoint
                    {
                        M = mi,
                        N = ni,
                        X = X(mi, ni),
                        Y = Y(mi, ni),
                        Ksi = Ksi(mi, ni),
                        Sig = Sig(mi, ni)
                    });

                    if (Console.KeyAvailable)
                    {
                        char Ch = Console.ReadKey(true).KeyChar;
                        if (Ch == (char)27) Environment.Exit(0); // ESC
                    }

                    NWork++;
                }
            }


        }

        static void OutRes2()
        {
            Res2.ToText();
        }

        public static class Res2
        {
            // Пример значения, замените на актуальное

            // Пример вспомогательных функций (замените на реальные)


            public static string ToText()
            {
                StringBuilder result = new StringBuilder();
                result.AppendLine("       M : N       X      Y      Ksi     Sig");

                for (int zi = 0; zi <= NLine; zi++)
                {
                    for (int ki = 0; ki <= NLine; ki++)
                    {
                        int mi = zi;
                        int ni = ki;
                        bool L = (mi == 1) || (mi % 5 == 0) || (mi == NLine) ||
                                 (ki == zi) || (zi % 5 == 0) || (zi == NLine);

                        if (L)
                        {
                            result.AppendLine($"({mi,2}:{ni,2}) {X(mi, ni),7:F2}  {Y(mi, ni),7:F2}  {Ksi(mi, ni),7:F2}  {Sig(mi, ni),7:F2}");
                        }

                        if (ki == NLine) result.AppendLine();
                    }
                }

                return result.ToString();
            }
        }

        // Процедура Move23
        private static void Move23()
        {
            for (int mm = 0; mm <= NLine; mm++)
            {
                // Перенос крайних точек
                SetA(arX, mm, 0, X(mm, NLine));
                SetA(arY, mm, 0, Y(mm, NLine));
                SetA(arKsi, mm, 0, Ksi(mm, NLine));
                SetA(arSig, mm, 0, Sig(mm, NLine));

                // Обнуление оставшегося участка
                for (int nn = 1; nn <= NLine; nn++)
                {
                    SetA(arX, mm, nn, 0);
                    SetA(arY, mm, nn, 0);
                    SetA(arKsi, mm, nn, 0);
                    SetA(arSig, mm, nn, 0);
                }
            }
        }

        private static double CalculateSg1Obl3(double ks)
        {
            int xM = 1;
            int xN = NLine;

            double d2_mn = D2mn(xM, xN);

            return Sig(xM, xN - 1) +
                        G2mn(xM, xN) / d2_mn * (Ksi(xM, xN) - Ksi(xM, xN - 1)) +
                        T2mn(xM, xN) / d2_mn;
        }

        private static double CalculateSg2Obl3(double ks)
        {

            double tg_de_2 = Tg(de2);

            double c_ks_0 = C(ks, 0),
                    n_ks_0 = N(ks, 0),
                    c_f_i_ks_0 = C_Fi(ks, 0),
                    ro_ks_0 = Ro(ks, 0),
                    k_ks_0 = K(ks, 0),
                    k_f_i_ks_0 = K_Fi(ks, 0),
                    x_i_ks_0 = Xi(ks, 0),
                    m_ks_0 = M(ks, 0);

            double cos_spec__ks__de_2__ro_ks_0 = Math.Cos(2 * ks - de2 - ro_ks_0);
            double sin_spec__ks__de_2__ro_ks_0 = Math.Sin(2 * ks - de2 - ro_ks_0);

            return ((c_ks_0 * n_ks_0 + 0.5 * c_f_i_ks_0 * cos_spec__ks__de_2__ro_ks_0) * tg_de_2 +
                          0.5 * (c_f_i_ks_0 * sin_spec__ks__de_2__ro_ks_0 - 2 * c_ks_0 * m_ks_0)) /
                          ((1 / x_i_ks_0 - k_ks_0 * n_ks_0 + 0.5 * k_f_i_ks_0 * cos_spec__ks__de_2__ro_ks_0) * tg_de_2 +
                           0.5 * (2 * k_ks_0 * m_ks_0 - k_f_i_ks_0 * sin_spec__ks__de_2__ro_ks_0));
        }

        // Функция для вычисления отношения sin1 / sig2
        private static double CalculateRatio(double ks, int aM, int aN)
        {
            double sg1 = CalculateSg1Obl3(ks);

            double sg2 = CalculateSg2Obl3(ks);

            double kp = sg1 / sg2;

            Console.Write($"ks= {ks:F3} sg1 / sg2 ={sg1,8:F3} : {sg2,8:F3} = {kp:F4}\r");
            
            return sg1 / sg2;
        }

        // Функция для вычисления первой производной отношения sin1 / sig2 по ks
        private static double CalculateDerivative(double ks, int aM, int aN)
        {
            double delta = 1e-5; // Малое изменение для численного дифференцирования

            // Используем приближенное численное дифференцирование
            double ratioPlus = CalculateRatio(ks + delta, aM, aN);
            double ratioMinus = CalculateRatio(ks - delta, aM, aN);

            return (ratioPlus - ratioMinus) / (2 * delta); // Центральная разность
        }

        // Функция для вычисления второй производной отношения sin1 / sig2 по ks
        private static double CalculateSecondDerivative(double ks, int aM, int aN)
        {
            double delta = 1e-5; // Малое изменение для численного дифференцирования

            // Используем приближенное численное дифференцирование
            double derivativePlus = CalculateDerivative(ks + delta, aM, aN);
            double derivativeMinus = CalculateDerivative(ks - delta, aM, aN);

            return (derivativePlus - derivativeMinus) / (2 * delta); // Центральная разность
        }

        // Функция для оптимизации методом Ньютона
        private static double OptimizeKs(int aM, int aN, double ksInitial, double aPr0, double aPrK)
        {
            double ks = ksInitial; // Начальное значение ks
            int iterations = 0;

            int xM = 1;
            int xN = NLine;

            SetA(arX, 0, xN - 1, X(aM - 1, aN - 1));
            SetA(arY, 0, xN - 1, Y(aM - 1, aN - 1));
            SetA(arKsi, 0, xN - 1, Ksi(aM - 1, aN - 1));
            SetA(arSig, 0, xN - 1, Sig(aM - 1, aN - 1));

            SetA(arX, 1, xN - 1, X(aM, aN - 1));
            SetA(arY, 1, xN - 1, Y(aM, aN - 1));
            SetA(arKsi, 1, xN - 1, Ksi(aM, aN - 1));
            SetA(arSig, 1, xN - 1, Sig(aM, aN - 1));

            SetA(arKsi, xM, xN, ks);

            double ratio = CalculateRatio(ks, aM, aN);

            // Итерации до тех пор, пока ratio не окажется в пределах [aPr0, aPrK]
            while (ratio < aPr0 || ratio > aPrK)
            {
                // Вычисляем первую и вторую производные
                double derivative = CalculateDerivative(ks, aM, aN);
                double secondDerivative = CalculateSecondDerivative(ks, aM, aN);

                // Обновление ks по методу Ньютона
                double step = derivative / secondDerivative;
                ks -= step; // Обновляем значение ks

                // Проверяем, чтобы ks оставался положительным
                if (ks <= 0 || Math.Abs(step) < 1e-6)
                {
                    Random rand = new Random();
                    ks = rand.NextDouble() * 200 + 2.366; // Присваиваем случайное положительное значение (в пределах от 1e-6 до 10)
                    // Console.WriteLine("ks стало отрицательным. Рандомизируем его значение: ks = {0:F6}", ks);
                }

                // Проверяем значение ratio
                SetA(arKsi, xM, xN, ks);

                ratio = CalculateRatio(ks, aM, aN);

                iterations++;
            }

            return ks;
        }

        // Основная функция, которая использует оптимизацию
        private static void CalcOD_N(int aM, int aN)
        {
            double ksInitial = 2.366; // Начальное значение ks

            // Оптимизируем ks, чтобы отношение sin1/sig2 было в нужном интервале
            double optimizedKs = OptimizeKs(aM, aN, ksInitial, aPr0, aPrK);

            double sg1 = CalculateSg1Obl3(optimizedKs);

            Console.WriteLine();

            SetA(arKsi, aM, aN, optimizedKs);
            SetA(arSig, aM, aN, sg1);
        }

        // Процедура CalcOD
        private static void CalcOD(int aM, int aN)
        {
            char Ch = '\0';
            double dKs = 0.001;
            double ks = 2.366;
            double kp, sg1;
            int xM = 1;
            int xN = NLine;

            SetA(arX, 0, xN - 1, X(aM - 1, aN - 1));
            SetA(arY, 0, xN - 1, Y(aM - 1, aN - 1));
            SetA(arKsi, 0, xN - 1, Ksi(aM - 1, aN - 1));
            SetA(arSig, 0, xN - 1, Sig(aM - 1, aN - 1));

            SetA(arX, 1, xN - 1, X(aM, aN - 1));
            SetA(arY, 1, xN - 1, Y(aM, aN - 1));
            SetA(arKsi, 1, xN - 1, Ksi(aM, aN - 1));
            SetA(arSig, 1, xN - 1, Sig(aM, aN - 1));

            double tg_de_2 = Tg(de2);

            double c_ks_0 = C(ks, 0);
            double n_ks_0 = N(ks, 0);
            double c_f_i_ks_0 = C_Fi(ks, 0);
            double ro_ks_0 = Ro(ks, 0);
            double k_ks_0 = K(ks, 0);
            double k_f_i_ks_0 = K_Fi(ks, 0);
            double x_i_ks_0 = Xi(ks, 0);
            double m_ks_0 = M(ks, 0);

            double cos_spec__ks__de_2__ro_ks_0 = Math.Cos(2 * ks - de2 - ro_ks_0);
            double sin_spec__ks__de_2__ro_ks_0 = Math.Sin(2 * ks - de2 - ro_ks_0);

            double d2_mn = 0.0;

            do
            {
                ks += dKs;

                SetA(arKsi, xM, xN, ks);

                // Промежуточные вычисления
                c_ks_0 = C(ks, 0);
                n_ks_0 = N(ks, 0);
                c_f_i_ks_0 = C_Fi(ks, 0);
                ro_ks_0 = Ro(ks, 0);
                k_ks_0 = K(ks, 0);
                k_f_i_ks_0 = K_Fi(ks, 0);
                x_i_ks_0 = Xi(ks, 0);
                m_ks_0 = M(ks, 0);

                cos_spec__ks__de_2__ro_ks_0 = Math.Cos(2 * ks - de2 - ro_ks_0);
                sin_spec__ks__de_2__ro_ks_0 = Math.Sin(2 * ks - de2 - ro_ks_0);

                d2_mn = D2mn(xM, xN);

                sg1 = Sig(xM, xN - 1) +
                            G2mn(xM, xN) / d2_mn * (Ksi(xM, xN) - Ksi(xM, xN - 1)) +
                            T2mn(xM, xN) / d2_mn;

                double sg2 = ((c_ks_0 * n_ks_0 + 0.5 * c_f_i_ks_0 * cos_spec__ks__de_2__ro_ks_0) * tg_de_2 +
                              0.5 * (c_f_i_ks_0 * sin_spec__ks__de_2__ro_ks_0 - 2 * c_ks_0 * m_ks_0)) /
                              ((1 / x_i_ks_0 - k_ks_0 * n_ks_0 + 0.5 * k_f_i_ks_0 * cos_spec__ks__de_2__ro_ks_0) * tg_de_2 +
                               0.5 * (2 * k_ks_0 * m_ks_0 - k_f_i_ks_0 * sin_spec__ks__de_2__ro_ks_0));

                kp = sg1 / sg2;

                Console.Write($"ks= {ks:F3} sg1 / sg2 ={sg1,8:F3} : {sg2,8:F3} = {kp:F4}\r");

                if (Console.KeyAvailable)
                {
                    Ch = Console.ReadKey(true).KeyChar;
                }
            } while (!(kp > aPr0 && kp < aPrK));

            Console.WriteLine();

            SetA(arKsi, aM, aN, ks);
            SetA(arSig, aM, aN, sg1);
        }

        static void Calc3()
        {
            Obl3.Calc3();
        }

        // Процедура Calc3
        public static class Obl3
        {
            // Исходные данные
            public static StringBuilder result = new StringBuilder();

            // Метод для формирования строки с данными
            public static string Calc3()
            {
                result = new StringBuilder();
                result.AppendLine($"NLine={NLine}");
                result.AppendLine($"RO0  ={Ro0:F4}, RO90  ={Ro90:F4}");
                result.AppendLine($"DRO0 ={dRo0:F4}, DRO90 ={dRo90:F4}");
                result.AppendLine($"DE1 ={de1:F4}, DE2 ={de2:F4}");
                result.AppendLine($"MU   ={MU:F4}, P0    ={P0:F4}");
                result.AppendLine($"DMu0Y={dMu0y:F4}, DMu90Y={dMu90y:F4}");
                result.AppendLine();

                int NPoint = NLine + ((NLine - 1) * NLine / 2) - 1;
                int NWork = 0;

                for (int mi = 0; mi <= NLine; mi++)
                {
                    result.AppendLine($"M:N ={mi,2} : {0,2}  {X(mi, 0),7:F4}  {Y(mi, 0),7:F4}  {Ksi(mi, 0),7:F4}  {Sig(mi, 0),7:F4}  {PPmn(mi, 0),7:F4}");
                }

                for (int ni = 1; ni <= NLine; ni++)
                {
                    SetA(arX, ni, ni, Xmn(ni, ni));
                    SetA(arY, ni, ni, 0);
                    CalcOD_N(ni, ni);
                    // CalcOD(ni, ni);

                    result.AppendLine();
                    result.AppendLine($"M:N ={ni,2} : {ni,2}  {X(ni, ni),7:F4}  {Y(ni, ni),7:F4}  {Ksi(ni, ni),7:F4}  {Sig(ni, ni),7:F4}  {PPmn(ni, ni),7:F4}");

                    for (int mi = ni + 1; mi <= NLine; mi++)
                    {
                        Console.Write($"M:N ={mi,2} : {ni,2}     ({NWork * 100.0 / NPoint,4:F1}%)    \r");

                        SetA(arX, mi, ni, Xmn(mi, ni));
                        SetA(arY, mi, ni, Ymn(mi, ni));
                        SetA(arKsi, mi, ni, Ksimn(mi, ni));
                        SetA(arSig, mi, ni, Sigmn(mi, ni));

                        result.AppendLine($"M:N ={mi,2} : {ni,2}  {X(mi, ni),7:F4}  {Y(mi, ni),7:F4}  {Ksi(mi, ni),7:F4}  {Sig(mi, ni),7:F4}  {PPmn(mi, ni),7:F4}");

                        if (Console.KeyAvailable)
                        {
                            char Ch = Console.ReadKey(true).KeyChar;
                            if (Ch == (char)27) Environment.Exit(0); // ESC
                        }

                        NWork++;
                    }
                }

                return result.ToString();
            }

            static public StringBuilder ToText()
            {
                return result;
            }

            // Пример массива (замените на реальные данные)
            //private static double[,] arX = new double[100, 100];
            //private static double[,] arY = new double[100, 100];
            //private static double[,] arKsi = new double[100, 100];
            //private static double[,] arSig = new double[100, 100];
        }

        // Процедура OutRes3
        private static void OutRes3()
        {
            // Выход из процедуры, так как в Pascal есть Exit
            return;
        }

        // Функция PPmn
        private static double PPmn(int aM, int aN)
        {
            double ssg = Sig(aM, aN);
            double rro = Romn(aM, aN);
            double kks = Ksi(aM, aN);
            return (ssg - Ximn(aM, aN) * ((ssg * Kmn(aM, aN) + Cmn(aM, aN)) *
                       (Math.Sin(2 * kks - rro) - 0.5 * Ro_Fimn(aM, aN) * Sec(rro) * Math.Sin(2 * kks)) +
                    0.5 * S_Fimn(aM, aN) * Math.Cos(2 * kks - rro))) / Math.Cos(de2);
        }

        // Вспомогательные функции
        private static void LoadIni()
        {
            // Установка значений по умолчанию или из внешнего источника
            data.NLine = NLine;
            data.Ro0 = Ro0;
            data.Ro90 = Ro90;
            data.dRo0 = dRo0;
            data.dRo90 = dRo90;
            data.MU = MU;
            data.P0 = P0;
            data.dMu0y = dMu0y;
            data.dMu90y = dMu90y;
            data.G = G;
            data.de1 = de1;
            data.de2 = de2;
            data.aPr0 = aPr0;
            data.aPrK = aPrK;
        }


        // Точка входа в программу
        static void Main(string[] args)
        {
            LoadConfig("config.json");

            // ------ Инициализация ------
            LoadIni(); // Загрузка начальных параметров
            Init();    // Инициализация данных

            Console.WriteLine($"NLine={NLine}\r\nRO0 ={Ro0:F4} RO90={Ro90:F4}\r\nDRO0 ={dRo0:F4}, DRO90 ={dRo90:F4}\"\r\n DE1 ={de1:F4}, DE2 ={de2:F4}\r\n\"MU={MU:F4}, P0={P0:F4}\r\n  \"DMu0Y={dMu0y:F4}, DMu90Y={dMu90y:F4}\"");

            // ------ 1-я область ------
            Console.WriteLine("1-я область ...");

            CalcOA(); // Вычисление начальных значений
            Calc1();  // Основные расчеты для 1-й области
            OutRes1(); // Вывод результатов для 1-й области
            Console.WriteLine();

            // ------ 2-я область ------
            Console.WriteLine("2-я область ...");

            Move12(); // Подготовка данных для 2-й области
            CalcOO(); // Вычисление начальных значений для 2-й области
            Calc2();  // Основные расчеты для 2-й области
            OutRes2(); // Вывод результатов для 2-й области
            Console.WriteLine();

            // ------ 3-я область ------
            Console.WriteLine("3-я область ...");

            //Move23(); // Подготовка данных для 3-й области
            Calc3();  // Основные расчеты для 3-й области
            OutRes3(); // Вывод результатов для 3-й области

            string filePath = "результат.txt";

            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(Obl1.ToText());
                writer.WriteLine(Obl2.ToText());
                writer.WriteLine(Obl3.ToText());
            }

            Console.WriteLine($"Данные успешно записаны в файл. {Path.GetFullPath(filePath)}");

            // ----- Очистка -----
            Done(); // Освобождение ресурсов
            Console.ReadKey();
        }
    }
}