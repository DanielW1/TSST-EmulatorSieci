using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode
{
    /// <summary>
    /// Tabela z zajętymi i wolnymi częstotliwościami.
    /// </summary>
    public class EONTable
    {
        /// <summary>
        /// Tablica zawierajaca poczatkowe czestotliwosci, pasmo zajete w  odbieraniu przez router. 
        /// Jeden rząd reprezentuje jeden tunel.
        /// </summary>
        public List<EONTableRowIN> TableIN { get; set; }

        /// <summary>
        /// Tablica zawierajaca poczatkowe czestotliwosci, pasmo zajete w wysyłaniu przez router. 
        /// Jeden rząd reprezentuje jeden tunel.
        /// </summary>
        public List<EONTableRowOut> TableOut { get; set; }

        /// <summary>
        /// Lista mowiaca o tym, ktora czestotliwosc jest wolna w odbieraniu.
        /// -1 - dana częstotliwość jest wolna
        /// n - częstotliwośc jest zajęta i zaczyna się na n-tej czestotliwości 
        /// </summary>
        /// <example>
        /// [0] = -1
        /// [1] = 1
        /// [2] = 1
        /// [3] = -1
        /// Oznacza, że częstotliwość [0] jest wolna, a częstotliwości nr 1 i 2 są zajęte 
        /// na tym samym kanale, który zaczyna się w częstotliwości 1. Kanał zajmuje pasmo równe 2 (x12.5GHz).
        /// 
        /// </example>
        public List<short> InFrequencies { get; set; }

        /// <summary>
        /// Lista mowiaca o tym, ktora czestotliwosc jest wolna w wysylaniu.
        /// </summary>
        public List<short> OutFrequencies { get; set; }

        /// <summary>
        /// Ile kanalow czestotliwosci ma router? Domylnie 64, 64x12.5GHz = 800GHz.
        /// </summary>
        public static int capacity = 64;

        /// <summary>
        /// 
        /// </summary>
        public EONTable()
        {
            InFrequencies = new List<short>(capacity);
            OutFrequencies = new List<short>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                //-1 oznacza, ze dana czestotliwosc jest wolna
                InFrequencies.Add(-1);
                OutFrequencies.Add(-1);
            }

            TableIN = new List<EONTableRowIN>();
            TableOut = new List<EONTableRowOut>();
        }

        /// <summary>
        /// Sprawdza, czy pasmo na zadanej częstotliwości jest wolne w tym routerze. in_or_out ma wartości "in" lub "out
        /// </summary>
        /// <param name="frequency"></param>
        /// <param name="band"></param>
        /// <param name="in_or_out"></pacam>
        public bool CheckAvailability(short frequency, short band, string in_or_out)
        {
            bool flag = true;
            try
            {
                if (in_or_out == "in")
                {
                    for (short i = frequency; i < frequency + band; i++)
                        if (InFrequencies[i] != -1)
                            flag = false;
                }
                else if (in_or_out == "out")
                {
                    for (short i = frequency; i < frequency + band; i++)
                        if (OutFrequencies[i] != -1)
                            flag = false;
                }
                else
                {
                    flag = false;
                    throw new Exception("EONTable.ChackAvailability: bad input argument. in_or_out is " + in_or_out +
                                        ", but should be either \"in\" or \"out\".");
                }
            }
            catch (Exception E)
            {
                Console.WriteLine(E.Message);
            }


            return flag;
        }

        /// <summary>
        /// prawdza, czy pasmo na zadanej częstotliwości jest wolne w tym routerze.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool CheckAvailability(EONTableRowIN row)
        {
            return CheckAvailability(row.busyFrequency, row.busyBandIN, "in");
        }

        /// <summary>
        /// prawdza, czy pasmo na zadanej częstotliwości jest wolne w tym routerze.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool CheckAvailability(EONTableRowOut row)
        {
            return CheckAvailability(row.busyFrequency, row.busyBandOUT, "in");
        }

        /// <summary>
        /// Dodanie rzędu do tablicy EON, wraz z aktualizacją tablic zajętości z częstotliwościami IN.
        /// </summary>
        /// <param name="row"></param>
        public bool addRow(EONTableRowIN row)
        {
            //sprawdzenie, czy wpis nie bedzie kolidowal
            if (CheckAvailability(row.busyFrequency, row.busyBandIN, "in"))
            {
                //dodanie do tabeli
                this.TableIN.Add(row);

                //Ustawienie wartosci zajetych pol w tabeli
                for (int i = row.busyFrequency; i < row.busyFrequency + row.busyBandIN; i++)
                    this.InFrequencies[i] = row.busyFrequency;

                return true;
            }
            else
                throw new Exception("EONTable.addRow(in): failed to add a row!");

            return false;
        }

        /// <summary>
        /// Dodanie rzędu do tablicy EON, wraz z aktualizacją tablic zajętości z częstotliwościami OUT.
        /// </summary>
        /// <param name="row"></param>
        public bool addRow(EONTableRowOut row)
        {
            //sprawdzenie, czy wpis nie bedzie kolidowal
            if (CheckAvailability(row.busyFrequency, row.busyBandOUT, "out"))
            {
                //dodanie do tabeli
                this.TableOut.Add(row);

                //Ustawienie wartosci zajetych pol w tabeli
                for (int i = row.busyFrequency; i < row.busyFrequency + row.busyBandOUT; i++)
                    this.OutFrequencies[i] = row.busyFrequency;

                return true;
            }
            else
                throw new Exception("EONTable.addRow(in): failed to add a row!");

            return false;
        }

        /// <summary>
        /// Funkcja, ktora usuwa wpis z wejsciowej tablicy EONowej i zwalnia zajete pasma
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool deleteRow(EONTableRowIN row)
        {
            //Czy taki wpis jest w tabeli?
            if (TableIN.Contains(row))
            {
                //zwolnienie wszystkich szczelin zwiazanych z danym wierszem
                for (int i = row.busyFrequency; i < row.busyBandIN+row.busyFrequency; i++)
                {
                    InFrequencies[i] = -1;
                }

                //wyrzucenie z tabeli wiersza
                TableIN.Remove(row);

                return true;
            }
            else
                //Nie ma takiego wpisu w tabeli, wiec sie go nie da usunac
                return false;
        }

        /// <summary>
        /// Funkcja, ktora usuwa wpis z wyjsciowej tablicy EONowej i zwalnia zajete pasma
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool deleteRow(EONTableRowOut row)
        {
            //Czy taki wpis jest w tabeli?
            if (TableOut.Contains(row))
            {
                //zwolnienie wszystkich szczelin zwiazanych z danym wierszem
                for (int i = row.busyFrequency; i < row.busyBandOUT + row.busyFrequency; i++)
                {
                    OutFrequencies[i] = -1;
                }

                //wyrzucenie z tabeli wiersza
                TableOut.Remove(row);

                return true;
            }
            else
                //Nie ma takiego wpisu w tabeli, wiec sie go nie da usunac
                return false;
        }
    }
}
