using System;
using System.Drawing;
using System.Windows.Forms;
using ZedGraph;
using System.Linq;
using System.Collections;
using System.IO;

namespace lab2
{
	public partial class Form1 : Form
	{
		double[] X = new double[10];
		double[] Y = new double[10];

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			GraphPane pane = z1.GraphPane;

			z1.GraphPane.CurveList.Clear();
			z1.IsShowPointValues = true;
			z1.IsEnableHZoom = true;
			z1.IsEnableVZoom = true;

			pane.XAxis.Title.IsVisible = false;
			pane.YAxis.Title.IsVisible = false;
			pane.XAxis.Scale.IsSkipFirstLabel = true;
			pane.XAxis.Scale.IsSkipLastLabel = true;
			pane.XAxis.Scale.IsSkipCrossLabel = true;
			pane.YAxis.Scale.IsSkipLastLabel = true;
			pane.YAxis.Scale.IsSkipCrossLabel = true;
			pane.Title.IsVisible = false;

			pane.XAxis.Cross = 0.0;
			pane.YAxis.Cross = 0.0;

			//pane.XAxis.Scale.Min = X.Min();       // левая граница масштаба
			//pane.XAxis.Scale.Max = X.Max();       // правая граница масштаба
			//pane.YAxis.Scale.Min = Y.Min();      // По оси Y установим автоматический подбор масштаба
			//pane.YAxis.Scale.Max = Y.Max();

			pane.IsBoundedRanges = true;    // при автоматическом подборе масштаба нужно учитывать только видимый интервал графика

			z1.AxisChange();
			z1.Invalidate();
		}

		public (double, double) calcAВ(double sumx, double sumy, double sumxy, double sumxx, double n)    // функция расчета коэфф a и b
		{
			var ab = (A: 0.0, B: 0.0);        // кортеж
			ab.A = (n * sumxy - sumx * sumy) / (n * sumxx - Math.Pow(sumx, 2));
			ab.B = (sumy - ab.A * sumx) / n;
			return ab;
		}

		public (double[], double) Yofline(double a, double b, int n)        // расчет точек Y апрокс прямой и суммарной ошибки
		{
			(double[], double) res = (new double[n], 0);     // кортеж (Y прямой, ошибка)

			for (int i = 0; i < n; i++)
			{
				res.Item1[i] = a * X[i] + b;        // апроксимирующая функция
				res.Item2 += Math.Pow(Y[i] - (a * X[i] + b), 2);    // суммарная ошибка

			}
			return res;
		}

		private void button1_Click(object sender, EventArgs e)      // построить
		{
			z1.GraphPane.CurveList.Clear();

			double sumx = X.Sum();
			double sumy = Y.Sum();
			double sumxy = 0, sumxx = 0;
			int n = X.Length;
			for (int j = 0; j < n; j++)
			{
				sumxy += X[j] * Y[j];
				sumxx += X[j] * X[j];
			}

			var ab = calcAВ(sumx, sumy, sumxy, sumxx, n);       // получаем кортех с коэф a и b
			var Yaprox = Yofline(ab.Item1, ab.Item2, n);
			double[] Yapr = Yaprox.Item1;                   // массив апроксимированных Y
			double error = Yaprox.Item2;            // ошибка апроксимации

			for (int i = 0; i < n; i++)
				dataGridView1[2, i].Value = Math.Round(Yapr[i], 2);
			textBox1.Text = Math.Round(ab.Item1, 2).ToString();       // вывести a
			textBox2.Text = Math.Round(ab.Item2, 2).ToString();        // вывести b
			textBox3.Text = Math.Round(error, 2).ToString();        // вывести ошибку

			LineItem points, line;
			points = z1.GraphPane.AddCurve("F(x)", X, Y, Color.Green, SymbolType.Circle);       // точки
			points.Symbol.Fill.Color = Color.Green;         // цвет заливки
			points.Symbol.Fill.Type = FillType.Solid;       // тип заливки
			points.Line.IsVisible = false;        // линия невидима
			line = z1.GraphPane.AddCurve("ax + b", X, Yapr, Color.Blue, SymbolType.None);      // апроксимирующая линия
			line.Line.Width = 2;

			z1.AxisChange();
			z1.Invalidate();
		}

		private void Button2_Click(object sender, EventArgs e)      // выбрать файл
		{
			if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
				return;
			dataGridView1.Rows.Clear();
			Array.Resize(ref X, 0);             // обнулили длину массива   
			Array.Resize(ref Y, 0);

			string filename = openFileDialog1.FileName;
			StreamReader filereader = new StreamReader(filename);
			int i = 0;
			while (filereader.EndOfStream != true)
			{
				string[] line = filereader.ReadLine().Replace(" ", "").Split(new string[] { "," }, StringSplitOptions.None);     // строка файла "x, y"
				Array.Resize(ref X, X.Length + 1);             // увеличили длину массива на 1 
				Array.Resize(ref Y, Y.Length + 1);
				X[i] = Convert.ToDouble(line[0]);
				Y[i] = Convert.ToDouble(line[1]);
				dataGridView1.Rows.Add(X[i], Y[i]);

				//dataGridView1.Rows[i].Cells[0].Value = X[i];    // добавить x в таблицу
				//dataGridView1.Rows[i].Cells[1].Value = Y[i];    // добавить y в таблицу
				i++;
			}
			filereader.Close();
		}

	}
}
