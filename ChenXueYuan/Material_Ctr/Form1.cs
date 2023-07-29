using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Material_Ctrl
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Weight_PL.tag_name = "WI117";
            LW_KC.tag_name = "LI117";
            random = new Random();
        }

        public class Name_IP
        {//用于记录变量名+变量地址
            public string tag_name = "Non";
            public int Ip = 0;
        };

        Name_IP Weight_PL = new Name_IP();
        Name_IP LW_KC = new Name_IP();

        float PL_Weight = 0;
        float KC_LW = 0;

        private int dataIndex;
        private Random random;

        static int ArraySize = 300;        // 保存数据的数组大小
        int WindowsSize = 10;       // 平滑窗口大小
        int JudgeWindows = 30;     // 判断进料状态的窗口大小
        int UpdateRate = 1;     // 进料状态更新频率（min/次）
        int CountIn = 0;        // 保存更新步长内的进料次数
        int CountNoIn = 0;
        int i = 0;      // 定义计数器
        // 定义数组保存数据
        float[] PL_Wetghts = new float[ArraySize];
        float[] KC_LWs = new float[ArraySize];
        float[] Smooth_PL_Wetghts = new float[ArraySize];
        float[] Smooth_KC_LWs = new float[ArraySize];

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = WindowsSize.ToString();
            this.textBox2.Text = JudgeWindows.ToString();

            // 创建一个存储选项的集合，将集合绑定到 ComboBox 控件
            List<string> options = new List<string> {"1 分钟", "2 分钟", "3 分钟", "5 分钟", "10 分钟", "15 分钟"};
            this.comboBox1.DataSource = options;
            string defaultOption = UpdateRate.ToString() + " 分钟";       // 指定默认选项文本
            if (this.comboBox1.Items.Contains(defaultOption))
            {
                int defaultItem = this.comboBox1.Items.IndexOf(defaultOption);         // 查找默认选项对应的索引
                this.comboBox1.SelectedIndex = defaultItem;        // 设置默认选项
            }
            
            try
            {
                CS_Object.initServerObject.initRemotServer("10.160.8.202", "TagChannel", "8088");
            }
            catch { }
        }

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

        private void timer1_Tick(object sender, EventArgs e)
        {   
            //读取数据
            //ReadTagAI(Weight_PL, ref PL_Weight);
            //ReadTagAI(LW_KC, ref KC_LW);
            //this.label2.Text = PL_Weight.ToString();
            //this.label2.Text = KC_LW.ToString();

            // 生成带噪声的 sin 数据（用于模拟数据采集）
            double noise = random.NextDouble() * 0.2 - 0.1;         // 生成噪声，范围在 -0.1 到 0.1 之间
            double sinValue = Math.Sin(dataIndex * 0.1);        // 此处的 0.1 可以调整频率
            float KC_LW = (float)(sinValue + noise);

            // 更新计数器
            if (i < ArraySize)
            {
                i++;
            }

            // 保存原始数据
            KC_LWs = SaveOrigin(KC_LWs, KC_LW, i);
            // 平滑数据
            Smooth_KC_LWs = SmoothData(KC_LWs, Smooth_KC_LWs, WindowsSize, i);
            // 判断进料状态
            StatusJudge(Smooth_KC_LWs, JudgeWindows, UpdateRate, i);
            // 在chart中展示数据
            ShowChart(KC_LWs, Smooth_KC_LWs, i);

            //String str1 = "";
            //String str2 = "";
            //for(int j = 0; j < 6; j++)
            //{
            //    str1 += KC_LWs[j] + "  ";
            //    str2 += Smooth_KC_LWs[j] + "  ";
            //}
            //this.label13.Text = str1;
            //this.label14.Text = str2;

            dataIndex++;
        }

        /// <summary>
        /// 保存历史数据
        /// </summary>
        /// <param name="origins">保存原始数据的数组</param>
        /// <param name="origin">最新的原始数据</param>
        /// <param name="position">当前保存数据的个数</param>
        /// <returns></returns>
        private float[] SaveOrigin(float[] origins, float origin, int position)
        {
            for (int j = position - 1; j > 0; j--)
            {
                origins[j] = origins[j - 1];
            }
            origins[0] = origin;
            return origins;
        }

        /// <summary>
        /// 平滑和保存数据
        /// </summary>
        /// <param name="origins">保存原始数据的数组</param>
        /// <param name="smoothDatas">保存平滑数据的数组</param>
        /// <param name="windowsSize">平滑窗口大小</param>
        /// <param name="position">当前保存数据的位置</param>
        /// <returns></returns>
        private float[] SmoothData(float[] origins, float[] smoothDatas, int windowsSize, int position)
        {
            for (int j = position - 1; j > 0; j--)
            {
                smoothDatas[j] = smoothDatas[j - 1];
            }
            if(position < WindowsSize)
            {
                smoothDatas[0] = origins.Skip(0).Take(position).Sum() / position;
            }
            else
            {
                smoothDatas[0] = origins.Skip(0).Take(WindowsSize).Sum() / WindowsSize;
            }
            return smoothDatas;
        }

        /// <summary>
        /// 进料状态判断
        /// </summary>
        /// <param name="smoothDatas">保存平滑数据的数组</param>
        /// <param name="judgeWindows">判断进料状态的窗口大小</param>
        /// <param name="updateRate">更新进料状态的频率（默认2分钟每次）</param>
        /// <param name="position">当前保存数据的个数，也可用于计算位置</param>
        private void StatusJudge(float[] smoothDatas, int judgeWindows, int updateRate, int position)
        {
            if (position >= judgeWindows)
            {
                float Smooth_LW_Change = (smoothDatas[judgeWindows - 1] - smoothDatas[0]) * (3600 / judgeWindows);
                this.label8.Text = Smooth_LW_Change.ToString();
                // 判断当前是否为加料状态
                // 料仓有三个变量。进料量（未知），排料量，矿槽料位
                if ((Smooth_LW_Change > 0) || (-Smooth_LW_Change < PL_Weight * 0.8))
                {// 若料位变化大于0，则当前处于进料状态
                    CountIn++;
                }
                else
                {// 若当前料位减少量等于料仓排料量，则当前未进料（若当前料位减少量小于料仓排料量，则当前数据有问题）
                    CountNoIn++;
                }

                // 投票机制判断进料状态
                if ((CountIn + CountNoIn) == (updateRate * 60))
                {
                    if (CountIn >= CountNoIn)
                    {
                        this.label12.Text = "正在进料";
                    }
                    else
                    {
                        this.label12.Text = "未进料";
                    }
                }
            }
        }

        /// <summary>
        /// 绘制展示数据的折线图
        /// </summary>
        /// <param name="origins">保存原始数据的数组</param>
        /// <param name="smooths">保存平滑数据的数组</param>
        /// <param name="position">当前保存数据的个数</param>
        private void ShowChart(float[] origins, float[] smooths, int position)
        {
            float[] reverseOrigin = new float[position];
            float[] reverseSmooth = new float[position];
            for(int j = 0; j < (position + 1) / 2; j++)
            {
                reverseOrigin[position - j - 1] = origins[j];
                reverseOrigin[j] = origins[position - j - 1];

                reverseSmooth[position - j - 1] = smooths[j];
                reverseSmooth[j] = smooths[position - j - 1];
            }

            //String str1 = "";
            //String str2 = "";
            //for(int j = position - 1; j >= 0 && j >= position - 6; j--)
            //{
            //    str1 += reverseOrigin[j].ToString() + "  ";
            //    str2 += reverseSmooth[j].ToString() + "  ";
            //}
            //this.label15.Text = str1;
            //this.label16.Text = str2;

            this.chart1.Series.Clear();
            this.chart1.ChartAreas.Clear();
            this.chart1.ChartAreas.Add(new ChartArea());

            Series series1 = new Series("原始数据");
            series1.ChartType = SeriesChartType.Line;
            series1.Points.DataBindY(reverseOrigin);
            this.chart1.Series.Add(series1);

            Series series2 = new Series("平滑数据");
            series2.ChartType = SeriesChartType.Line;
            series2.Points.DataBindY(reverseSmooth);
            this.chart1.Series.Add(series2);

            float Min = reverseOrigin.Min();
            float Max = reverseOrigin.Max();
            this.chart1.ChartAreas[0].AxisY.Minimum = Min - 0.1;        // Y 轴下限
            this.chart1.ChartAreas[0].AxisY.Maximum = Max + 0.1;        // Y 轴上限
            this.chart1.ChartAreas[0].AxisY.LabelStyle.Format = "F2";
            
            this.chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;      // 设置 X 轴主网格线不显示
            this.chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;      // 设置 Y 轴主网格线不显示

        }

        /// <summary>
        /// button事件：更改平滑窗口大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            int number;
            if (int.TryParse(textBox1.Text, out number) && number > 0 && number < ArraySize)
            {
                WindowsSize = number;
                MessageBox.Show("平滑窗口大小已改为：" + number);
            }
            else
            {
                MessageBox.Show("请输入有效的正整数！");
                textBox1.Text = "";         // 清空输入内容
                textBox1.Focus();       // 将焦点重新设置到 TextBox 控件
            }
        }

        /// <summary>
        /// button事件：更改判断进料状态的窗口大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            int number;
            if (int.TryParse(textBox2.Text, out number) && number > 0 && number < ArraySize)
            {
                JudgeWindows = number;
                MessageBox.Show("进料状态判断窗口大小已改为：" + number);
            }
            else
            {
                MessageBox.Show("请输入有效的正整数！");
                textBox1.Text = "";
                textBox1.Focus();
            }
        }

        /// <summary>
        /// button事件：更改判断进料状态的更新频率
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            string selectedOption = this.comboBox1.SelectedItem.ToString();
            // 使用正则表达式提取选项中的数字部分
            Match match = Regex.Match(selectedOption, @"\d+");
            if (match.Success)
            {
                UpdateRate = int.Parse(match.Value);
                MessageBox.Show("进料状态判断更新频率更改为：" + UpdateRate + "分钟。");
            }
            else
            {
                MessageBox.Show("进料状态判断更新频率更改未成功。");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 在窗体关闭时释放 Timer 控件
            if (this.timer1 != null)
            {
                this.timer1.Stop();
                this.timer1.Tick -= timer1_Tick;
                this.timer1.Dispose();
                this.timer1 = null;
            }
            Close();
        }
    }
}
