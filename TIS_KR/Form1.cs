using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TIS_KR
{
    public partial class Form1 : Form
    {
        private int[] ToBin(int cod, int sampl)
        {
            string s = Convert.ToString(cod, 2); //Convert to binary in a string

            int[] bits = s.PadLeft(sampl, '0') // Add 0's from left
                         .Select(c => int.Parse(c.ToString())) // convert each char to int
                         .ToArray(); // Convert IEnumerable from select to Array
            return bits;
        }
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button1.Click += button1_click;
            button2.Click += button2_click;
        }
        private void button1_click(object sender, EventArgs e)//Ввод данных
        {
           
            try
            {
                double t = 0;
                if (string.IsNullOrEmpty(textBox1.Text)) { MessageBox.Show("Введите значение амплитуды входного сигнала"); return; }
                double a1 = double.Parse(textBox1.Text); //Амплитуда входного сигнала
                if (string.IsNullOrEmpty(textBox2.Text)) { MessageBox.Show("Введите значение частоты входного сигнала"); return; }
                double f1 = double.Parse(textBox2.Text); //Частота входного сигнала
                if (string.IsNullOrEmpty(textBox3.Text)) { MessageBox.Show("Введите значение частоты сэмплирования сигнала"); return; }
                double f2 = double.Parse(textBox3.Text); //Частота сэмплирующего сигнала 
                if (string.IsNullOrEmpty(textBox4.Text)) { MessageBox.Show("Введите значение глубины сэмплирования сигнала"); return; }
                int sampl = int.Parse(textBox4.Text); //Глубина самплирования сигнала в битах сигнала
                if (a1 >= 1 << sampl) { MessageBox.Show("амплитуда не должна превышать значения двух в степени глубины сэмплирования "); return; }
                double pp2 = 1 / (f2 * 4);// Полупериод сэмплирующего сигнала
                double t1 = -pp2 - pp2;// Начальное время сэмплирования
                double t2 = t1 + pp2 + pp2 + pp2;// Конечное время сэмплирования
                double pp3 = (3 * pp2 )/( 2*sampl);//полупериод сигнала кодирования
                double t3 = 0;//начальное время кодирования
                double t4 = 0;//конечное время кодирования
                double y = 0;// мгновенное значение входного сигнала
                double y1 = 0;//значение входного сигнала
                bool flag1 = false;//флаги запуска кодирования
                bool flag2 = false;  
                int cod = 0;//абсолютное значение сигнала для преобразования в код
                int i = 0;
                int j = 0;
                int[] bits = new int[sampl];//массив двоичного значения кода
                var labels = new[] { label5, label6, label7, label8, label9, label10, label11, label12,
                    label13, label14, label15, label16, label17, label18, label19, label20 };// массив для вывода кодовых последовательностей
                chart1.ChartAreas[0].AxisY.Maximum = 1<<sampl;
                chart1.ChartAreas[0].AxisY.Minimum =0;
                chart2.ChartAreas[0].AxisX.Minimum = 0;
                chart1.Series[1].Points.Clear(); //Входной сигнал
                chart1.Series[0].Points.Clear(); //АИМ
                chart2.Series[1].Points.Clear(); //синхросигнал
                chart2.Series[0].Points.Clear(); //код
                for (j = 0; j < 16; j++) labels[j].Text = "";// очищаем метки
                j = 0;
                while (t <= 1) //Построение графиков несущего, модулирующего и амплитудно-модулированного сигналов
                {
                    y1 = (1<<(sampl-1))+ (a1/2 * Math.Sin(2 * Math.PI * f1 * t));
                    chart1.Series[1].Points.AddXY(t, y1);
                    ;
                    if (t1 < t2 && t2 <= t)
                    {
                        t1 = t1 + 4 * pp2;
                        y = y1;
                        flag1 = false;
                        flag2 = true;
                    }
                    if (t2 < t1 && t1 <= t)
                    {
                        t2 = t2 + 4 * pp2;
                        flag1 = true;
                    }
                    if (flag1 && flag2)
                    {                                                
                        t3 = t1;
                        t4 = t3 + pp3;
                        i = 0;
                        cod = Convert.ToInt32(y);
                        bits = ToBin(cod, sampl);
                        if (j < 16)
                        {
                            labels[j].Text = String.Join("", new List<int>(bits).ConvertAll(k => k.ToString()).ToArray());
                            j++;
                        }
                        flag1 = false;
                    }
                    if (t1 < t2)
                    {
                        chart1.Series[0].Points.AddXY(t, 0);
                        chart2.Series[0].Points.AddXY(t, 0);
                    }
                    if (t1 >= t2)
                    {
                        chart1.Series[0].Points.AddXY(t, y);
                        chart2.Series[0].Points.AddXY(t, 4.8);
                    }                   
                    //
                    if (t3 < t4 && t4 <= t)
                    {                        
                        i++;
                        if (i == sampl) t3 = t4;
                        else t3 = t3 + pp3+pp3;
                    }
                    if (t4 < t3 && t3 <= t)
                    {
                        t4 = t4 + pp3+pp3;                        
                    }
                    if (t3 == t4) chart2.Series[1].Points.AddXY(t, 0);                    
                    if (t3 > t4)
                        chart2.Series[1].Points.AddXY(t, 0);
                    if (t3 < t4)
                    {
                        if (bits[i] == 1)
                            chart2.Series[1].Points.AddXY(t, 3.3);
                        else
                            chart2.Series[1].Points.AddXY(t, 0);
                    }
//
                    t = t + 0.001;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Введены неверные значения");
            }
        }
        private void button2_click(object sender, EventArgs e)//Закрытие формы
        {
            Close();
        }       
    }
}
