using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;


namespace mousedisaster
{
    public partial class Form1 : Form
    {
        //老鼠的初始数目,以千只为单位
        private double num = 0.05;
        //天敌的初始数目
        private double number = 0.005;
        //实际情况为0.0005，在这里使用0.05是为了使曲线不至于过陡，一下改变参数的地方都在后边有实际值
        delegate void SetTextCallback(string text);
        Thread thr;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //如果线程处于挂起状态，则继续原有线程
            if(thr!=null){
                thr.Resume();
                return;
            }
            //如果进程尚未创建，则创建并开启这个线程
            thr = new Thread(new ThreadStart(this.run));
            thr.Start();
            //如果输入增加天敌数目不为空，则加入天敌数目
            string FoeNum = txtFoeNum.Text.Trim();
            if (!FoeNum.Equals(""))
                this.number = number + double.Parse(FoeNum);

        }

            //执行线程
            private void run()
            {
                
                //该参数用于绘图参数中的横坐标x
                int paintCfg = 2;
                //计数器
                int count = 1;
                while(true){
                    Thread.Sleep(800);
                    //执行数量分析方法
                    num = this.analyseAmount(num,count);
                    //绘图
                    this.paint(paintCfg,num);
                    this.SetText("" + num);
                    //下一条竖线与前一条的间隔为8像素
                    paintCfg = paintCfg +8;
                    //计数器自加一，表示过了一个月
                    count++;
                }
            }

            //该方法对老鼠数量进行分析
            private double analyseAmount(double num,int count)
            {
                
                    //出生了多少只老鼠
                    num = birthrate(num);
                    //死亡了多少只老鼠
                    num = this.mortality(num);
                    //引入天敌
                    if (this.cbxParameter.Checked == true)
                       num = this.importFoe(num, number);
                    //加入草场密度对老鼠数量的影响
                    if (this.cbxParameter1.Checked == true)
                    {
                        try
                        {
                            double rate = double.Parse(this.txtRate.Text.Trim());
                            num = this.grassplotInfection(num, rate);
                            
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("请输入正确的数据", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            //终止线程
                            if (thr != null)
                            {
                                thr.Suspend();
                            }
                        }
                    }
                    if (cbxParameter2.Checked == true)
                    {
                        num = num * 0.4;
                    }
                
                return num;
            }

            //调度label控件的Text属性
            private void SetText(string text)
            {
                // InvokeRequired required compares the thread ID of the
                // calling thread to the thread ID of the creating thread.
                // If these threads are different, it returns true.
                if (this.lblMouseNumber.InvokeRequired)
                {
                    SetTextCallback d = new SetTextCallback(SetText);
                    this.Invoke(d, new object[] { text });
                }
                else
                {
                    this.lblMouseNumber.Text = text;
                }
            }

            //终止线程
            private void Form1_FormClosing(object sender, FormClosingEventArgs e)
            {
                if(thr!=null){
                    thr.Abort();
                }
            }

            //更具老鼠的出身率和基数计算出一段时间后老鼠的数量
            private double birthrate(double num)
            {
                //出生率
                double birthrate = 1.698;//4.4896
                return num*birthrate;
            }

            //更具老鼠的死亡率计算出一段时间后老鼠的数量
            private double mortality(double num)
            {
                //死亡率
                double mortality = 0.3;//方便演示
                return num-num*mortality;
            }

            //x为绘制直线的横坐标，y为直线最高点的纵坐标
            private void paint(int x,double num)
            {
                int y = 160-(int)(num * 100);
                //MessageBox.Show(""+y);
                Graphics myGraphics = this.groupBox2.CreateGraphics();
                Pen myPen;
                if(num<=0.3)
                    myPen = new Pen(Color.YellowGreen, 2);
                else if(num>0.3&&num<0.6)
                    myPen = new Pen(Color.Yellow, 2);
                else
                    myPen = new Pen(Color.Red, 2);
               myGraphics.DrawLine(myPen, x, 160, x, y);
            }

            //引入天敌后，更具老鼠基数计算出一段时间后老鼠的数量
            private double importFoe(double num,double y)
            {
                //num为老鼠的数量，y为捕食者的数量
                
                //天敌的捕食能力
                double a = 0.5;//8533.372601568
                //老鼠的增长率
                double r = 1.698;//4.4896
                //计算老鼠数量公式
                double num1 = num*(r-a*y);
                return num1;
            }

            //加入草场对老鼠数量的影响,草场的覆盖率为P
            private double grassplotInfection(double num,double p)
            {
                //老鼠的增长率为r,草场覆盖率为P,关系式为:r等于4.804的(-P+0.5)次方
                //草场覆盖率达到0.5时，老鼠的数量达到平衡态
                double r = Math.Pow(4.804,-p+0.5);
                num = r * num;
                
                return num;
            }

            //投放老鼠药对老鼠数量和天敌数目的影响
            private double[] throwMedicament(double num1,double num2,double weight)
            {
                //num1为老鼠的数量，num2为天敌的数量，weight为在此试验田里投放农药的吨数，杀死老鼠百分比和投放灭鼠药
                //重量公式为:y=1-1/(x+1)
                double[] arr = new double[2];
                //在此试验田里投放单位重量灭鼠药杀死老鼠的百分比
                double killMouse = 1-1/(1+weight);
                //在此试验田里投放单位重量灭鼠药杀死天敌的百分比
                double killFoe = 1-1/(1+weight);
                //投放一次灭鼠药后剩余老鼠的数量
                num1 = num1-num1 * killMouse;
                //投放一次灭鼠药后剩余天敌的数量
                num2 = num2 - num2 * killFoe;
                //将剩余老鼠和天敌的数量存入数组
                arr[0]=num1;
                arr[1] = num2;
                return arr;
            }

            //挂起老鼠增长线程
            private void button1_Click(object sender, EventArgs e)
            {
                if(thr!=null){
                thr.Suspend();
                }
            }
            //加老鼠药
            private void button2_Click_1(object sender, EventArgs e)
            {
                double[] arr = this.throwMedicament(num, number, 1);
                //投放灭鼠要一个月后老鼠的数目
                this.num = arr[0];
                //投放老鼠药一个月后天敌的数目
                this.number = arr[0];
            }
            //加入天敌
            private void button3_Click(object sender, EventArgs e)
            {
                double foeNum = double.Parse(txtFoeNum.Text.Trim());
                this.number = number + foeNum;
            }
            //人工植草
            private void button4_Click(object sender, EventArgs e)
            {

            }

    }
}
