using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        

        public Form1()
        {
            InitializeComponent();
            LI_302.tag_name = "LI_302";
        
        }
       
        public class Name_IP
        {//用于记录变量名+变量地址
            public string tag_name = "Non";
            public int Ip = 0;
        };

        Name_IP LI_302 = new Name_IP();

        float li_302 = 0;

        private bool ReadTagAI(Name_IP Tag, ref float Value)
        {
            bool sta = false;
            int reas = 0;
            bool Fx = false;

            try
            {
                if (!CS_Object.initServerObject.Tagserverobject.APIWGetValue(Tag.tag_name, ref Value, ref sta, ref Tag.Ip, ref reas))//本地读
                {
                    Tag.Ip = 0;
                    //EventLog.WriteEntry("SICES_配料优化模型", Tag.Name + "读不成功！", EventLogEntryType.Error);
                }
                else
                {
                    if (sta) Fx = true;
                }
            }
            catch
            {
                Tag.Ip = 0;
                //EventLog.WriteEntry("SICES_配料优化模型", Tag.Name + "访问不成功！", EventLogEntryType.Error);
            };

            return Fx;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            try
            {
                //MessageBox.Show("这是一个弹窗", "title");
                CS_Object.initServerObject.initRemotServer("10.160.8.202", "TagChannel", "8088");
            }
            catch { };
        }

        static int smoothingWindowSize = 5;
        private DataSmoother smoother = new DataSmoother(smoothingWindowSize);

        // 画图展示的数据曲线
        List<double> li302_list = new List<double>();
        List<double> li302_smoothed_list = new List<double>();

        private ChartDrawer chartDrawer;

        private void timer1_Tick(object sender, EventArgs e)
        {
            // 读取数据
            ReadTagAI(LI_302, ref li_302);

            double smoothed_li302 = smoother.SmoothData(li_302);

            li302_list.Add(li_302);
            li302_smoothed_list.Add(smoothed_li302);

            if (li302_list.Count>=100) {
                li302_list.RemoveAt(0);
                li302_smoothed_list.RemoveAt(0);
            }

            listBox1.Items.Add($"Original : {string.Format("{0:f4}", li_302)},    Smoothed : {string.Format("{0:f4}", smoothed_li302)}");
        
            this.label1.Text = string.Format("{0:f2}", li_302);

            //绘图
            chartDrawer = new ChartDrawer(chart1, li302_list, li302_smoothed_list);
            chartDrawer.DrawChart();
        }
    }

    // 定义数据平滑器,这是一个队列，长度即为平滑窗口大小
    public class DataSmoother
    {
        private Queue<double> dataQueue;
        private int windowSize;
        private double currentSum;

        public DataSmoother(int windowSize)
        {
            this.windowSize = windowSize;
            dataQueue = new Queue<double>(windowSize);
            currentSum = 0;
        }

        public double SmoothData(double newValue)
        {
            if (dataQueue.Count >= windowSize)
            {
                currentSum -= dataQueue.Dequeue();
            }
            dataQueue.Enqueue(newValue);
            currentSum += newValue;
            return currentSum / dataQueue.Count;
        }
    }

    public class ChartDrawer {
        private Chart chart;
        private List<double> dataList;
        private List<double> SmoothedDataList;

        public ChartDrawer(Chart chart,List<double> dataList, List<double> dataListSmoothed) {
            this.chart = chart;
            this.dataList = dataList;
            this.SmoothedDataList = dataListSmoothed;
        }

        // 自适应绘图，以最小的值作为Y轴起始点
        public void AdjustYAsixRange()
        {
            double minY = dataList.Min();
            double maxY = dataList.Max();

            // 加入一些余量
            double margin = 0.5;
            chart.ChartAreas[0].AxisY.Minimum = minY - margin;
            chart.ChartAreas[0].AxisY.Maximum = maxY + margin;
        }

        public void DrawChart()
        {
            // 清空图标的数据系列，以便重新绘制
            chart.Series.Clear();
            
            //创建一个新的数据系列
            Series series = new Series();
            series.ChartType = SeriesChartType.Line;
            series.BorderWidth = 2;
            series.Color = Color.Blue;
            series.Name = "LI_302";
            
            //将数据点添加到数据系列中
            for (int i = 0; i < dataList.Count; i++)
            {
                series.Points.AddXY(i, dataList[i]);
            }
            chart.Series.Add(series);

            //创建一个新的数据系列
            Series series2 = new Series();
            series2.ChartType = SeriesChartType.Line;
            series2.BorderWidth = 2;
            series2.Color = Color.Red;
            series2.Name = "LI_302_Smoothed";

            //将数据点添加到数据系列中
            for (int i = 0; i < SmoothedDataList.Count; i++)
            {
                series2.Points.AddXY(i, SmoothedDataList[i]);
            }
            chart.Series.Add(series2);


            AdjustYAsixRange();
            chart.ChartAreas[0].AxisY.LabelStyle.Format = "F2"; // 设置y轴仅显示两位小数
            chart.ChartAreas[0].AxisX.MajorTickMark.Enabled = false;
            chart.ChartAreas[0].AxisY.MajorTickMark.Enabled = false;// 隐藏刻度线
            chart.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
            chart.ChartAreas[0].AxisY.MajorGrid.Enabled = false;//隐藏标尺线   
        }

        

    }
}
