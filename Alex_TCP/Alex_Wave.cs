using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Alex_TCP
{
    class Alex_WaveShowBmp
    {
        /// <summary>
        ///波形数据空间，直接使用结构体
        /// </summary>
        public struct WaveDatatype
        {
            public float Wave1;
            public float Wave2;
            public float Wave3;
            public float Wave4;
            public float Wave5;
            public float Wave6;
        }
        public bool[] WaveEnable = { false, false, false, false, false, false };
        /// <summary>
        /// 波形数据
        /// </summary>
        public List<WaveDatatype> ListWave = new List<WaveDatatype>();
        /// <summary>
        /// 当前显示的帧数
        /// </summary>
        public int NowFrameCount = 0;
        /// <summary>
        /// 波形是否静止
        /// </summary>
        public bool IsWaveStatic = false;
        /// <summary>
        /// 波形更新速度
        /// </summary>
        public int UpdateSpeed = 1;
        /// <summary>
        /// 初始化完成标志
        /// </summary>
        private bool Isinit = false;
        /// <summary>
        /// X轴起始绝对坐标
        /// </summary>
        private int X0;
        /// <summary>
        /// Y轴起始坐标
        /// </summary>
        private int Y0;
        /// <summary>
        /// 网格线间隔
        /// </summary>
        private int interval;
        /// <summary>
        /// bmp宽度
        /// </summary>
        private int width;
        /// <summary>
        /// bmp高度
        /// </summary>
        private int height;
        /// <summary>
        /// /一个像素点所表示的X数值
        /// </summary>
        private float[] Uinty = {0.001f, 0.002f, 0.004f, 0.008f,
                                 0.01f, 0.02f, 0.04f, 0.08f,
                                 0.1f, 0.2f, 0.4f, 0.8f,
                                 1f, 2f, 4f, 8f,
                                 10f, 20f, 40f, 80f};
        private int Uintyindex = 0;
        /// <summary>
        /// 一个像素点所表示的Y数值
        /// </summary>
        private float[] Uintx = {0.02f, 0.04f, 0.08f,
                                0.1f, 0.2f, 0.4f, 0.6f, 
                                0.8f, 0.9f, 1.0f, 1.1f, 1.2f,
                                1.3f, 1.4f, 1.5f, 1.6f, 1.7f,
                                1.8f, 1.9f, 2f};
        private int Uintxindex = 0;
        private float ShowXstart = 0;
        private float ShowYstart = 0;
        /// <summary>
        /// 波形图初始化
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="w"></param>
        /// <param name="h"></param>
        public void Wave_init(int x, int y, int w, int h)
        {
            this.X0 = x;
            this.Y0 = y;
            this.width = w;
            this.height = h;
            this.interval = w / 12;
            this.NowFrameCount = 0;
            this.IsWaveStatic = false;
            this.NowFrameCount = 0;
            this.UpdateSpeed = 5;
            this.ShowYstart = 0;
            this.ShowXstart = 0;
            Uintxindex = 4;
            Uintyindex = 4;
            this.ListWave.Clear();
            this.Isinit = true;      ////初始化完成标志
        }
        /// <summary>
        /// 波形数据清零
        /// </summary>
        public void WaveDataClear()
        {
            NowFrameCount = 0;
            ListWave.Clear();
        }
        /// <summary>
        /// 波形图更新       
        /// </summary>
        /// <param name="uintx"></param>
        /// <param name="uinty"></param>
        /// <param name="Xstart"></param>
        /// <param name="Ystart"></param>
        /// <returns></returns>
        public Bitmap WaveUpdate()
        {
            Bitmap objBitmap = new Bitmap(width, height);
            Graphics objGraphics = Graphics.FromImage(objBitmap);
            if (Isinit == false)
                return objBitmap; ////如果没有完成初始化则不进行下面所有的操作
                                  /////绘制网格线
            FrameCountUpdate();


            float Xstart = ShowXstart, Ystart = ShowYstart;              ////同样复制
            float uintx = Uintx[Uintxindex], uinty = Uinty[Uintyindex];  ////复制过来
            Font ruleris = new Font("Consolas", 8);
            Pen usepen = new Pen(Color.Gray, 0.2f);
            usepen.DashStyle = System.Drawing.Drawing2D.DashStyle.Custom;
            usepen.DashPattern = new float[] { 5, 5 };
            float Yis = 0, Xis = 0;     ////Y轴坐标刻度
            UInt16 xn = 0;
            UInt16 yn = 0;
            for (int x = X0; x < width - interval; x += interval)
            {
                Xis = xn * uintx * (float)interval + Xstart;
                objGraphics.DrawString(Xis.ToString("F1"), ruleris, new SolidBrush(Color.Yellow), x, height - Y0);
                objGraphics.DrawLine(usepen, x, 0, x, height - Y0);
                xn++;
            }
            for (int y = height - Y0; y > 0; y -= interval)
            {
                Yis = yn * uinty * (float)interval + Ystart;
                objGraphics.DrawString(Yis.ToString("F1"), ruleris, new SolidBrush(Color.Yellow), X0 - 60, y - 5);
                objGraphics.DrawLine(usepen, X0, y, width, y);
                yn++;
            }

            int WaveStart = (int)Xstart;
            int WaveEnd = WaveStart + (int)((float)(width - X0) * uintx);///因为外部也会调用并更改NowFrame所以复制一个值去显示
            if (NowFrameCount < WaveEnd)
                WaveEnd = NowFrameCount;

            if ((WaveEnd - WaveStart) < 2)
                return objBitmap;   //////小于2个点的话则不进行波形的绘制

            List<PointF> Point_1 = new List<PointF>();
            List<PointF> Point_2 = new List<PointF>();
            List<PointF> Point_3 = new List<PointF>();
            List<PointF> Point_4 = new List<PointF>();
            List<PointF> Point_5 = new List<PointF>();
            List<PointF> Point_6 = new List<PointF>();
            for (int n = 0; n < (WaveEnd - WaveStart); n++)
            {
                if (WaveEnable[0])
                {
                    Point_1.Add(new PointF(X0 + n / uintx, height - Y0 - ((ListWave[n + WaveStart].Wave1 - Ystart) / uinty)));
                }
                if (WaveEnable[1])
                {
                    Point_2.Add(new PointF(X0 + n / uintx, height - Y0 - ((ListWave[n + WaveStart].Wave2 - Ystart) / uinty)));
                }
                if (WaveEnable[2])
                {
                    Point_3.Add(new PointF(X0 + n / uintx, height - Y0 - ((ListWave[n + WaveStart].Wave3 - Ystart) / uinty)));
                }
                if (WaveEnable[3])
                {
                    Point_4.Add(new PointF(X0 + n / uintx, height - Y0 - ((ListWave[n + WaveStart].Wave4 - Ystart) / uinty)));
                }
                if (WaveEnable[4])
                {
                    Point_5.Add(new PointF(X0 + n / uintx, height - Y0 - ((ListWave[n + WaveStart].Wave5 - Ystart) / uinty)));
                }
                if (WaveEnable[5])
                {
                    Point_6.Add(new PointF(X0 + n / uintx, height - Y0 - ((ListWave[n + WaveStart].Wave6 - Ystart) / uinty)));
                }
            }
            float Width = 2;
            if (WaveEnable[0])
            {
                objGraphics.DrawLines(new Pen(Color.Red, Width), Point_1.ToArray());
            }
            if (WaveEnable[1])
            {
                objGraphics.DrawLines(new Pen(Color.Blue, Width), Point_2.ToArray());
            }
            if (WaveEnable[2])
            {
                objGraphics.DrawLines(new Pen(Color.Olive, Width), Point_3.ToArray());
            }
            if (WaveEnable[3])
            {
                objGraphics.DrawLines(new Pen(Color.Lime, Width), Point_4.ToArray());
            }
            if (WaveEnable[4])
            {
                objGraphics.DrawLines(new Pen(Color.Gold, Width), Point_5.ToArray());
            }
            if (WaveEnable[5])
            {
                objGraphics.DrawLines(new Pen(Color.Purple, Width), Point_6.ToArray());
            }


            return objBitmap;
        }
        /// <summary>
        /// 显示帧数的更新
        /// </summary>
        private int FrameCountUpdate()
        {
            if(IsWaveStatic)
            {
                if (NowFrameCount > ListWave.LongCount())
                {
                    NowFrameCount = (int)ListWave.LongCount();
                }
            }
            else
            {
                NowFrameCount += UpdateSpeed;
                if(NowFrameCount > ListWave.LongCount())
                {
                    NowFrameCount = (int)ListWave.LongCount();
                }
                ShowXstart = NowFrameCount - (float)(width - X0) * Uintx[Uintxindex];
                if (ShowXstart < 0)
                    ShowXstart = 0;
            }
            return NowFrameCount; 
        }
        /// <summary>
        /// Y轴间隔改变
        /// </summary>
        /// <param name="det"></param>
        public void UintyChange(int det)
        {
            float LastMid = ShowYstart + Uinty[Uintyindex] * (height - Y0) / 2;
            if (det > 0)
            {
                Uintyindex++;
                if(Uintyindex >= Uinty.Length)
                {
                    Uintyindex = Uinty.Length - 1;
                }
            }
            else
            {
                Uintyindex--;
                if (Uintyindex < 0)
                    Uintyindex = 0;
            }
            ShowYstart = LastMid - Uinty[Uintyindex] * (height - Y0) / 2;
            if ((ShowYstart + Uinty[Uintyindex] * (height - Y0)) > 100000)
                ShowYstart = 100000 - Uinty[Uintyindex] * (height - Y0);
            else if (ShowYstart < -100000)
                ShowYstart = -100000;
        }
        /// <summary>
        /// X轴间隔改变
        /// </summary>
        /// <param name="det"></param>
        public void UintxChange(int det)
        {
            if(det > 0)
            {
                Uintxindex++;
                if (Uintxindex >= Uintx.Length)
                {
                    Uintxindex = Uintx.Length - 1;
                }
            }
            else
            {
                Uintxindex--;
                if (Uintxindex < 0)
                    Uintxindex = 0;
            }
        }
        /// <summary>
        /// Y轴起始改变
        /// </summary>
        /// <param name="det"></param>
        public void YStartChange(int det)
        {
            float range = ((float)det) * Uinty[Uintyindex];
            ShowYstart += range;
            if ((ShowYstart + Uinty[Uintyindex] * (height - Y0)) > 100000)
                ShowYstart = 100000 - Uinty[Uintyindex] * (height - Y0);
            else if (ShowYstart < -100000)
                ShowYstart = -100000;
        }
        /// <summary>
        /// X轴起始改变
        /// </summary>
        /// <param name="det"></param>
        public void XStartChange(int det)
        {
            if (IsWaveStatic == false) return;
            float range = ((float)det) * Uintx[Uintxindex];
            ShowXstart -= range;
            if(ShowXstart < 0)
                ShowXstart = 0;
        }

        private bool SaveDataToCSVFile(string filePath)
        {
            bool successFlag = true;

            StringBuilder strColumn = new StringBuilder();
            StringBuilder strValue = new StringBuilder();
            StreamWriter sw = null;
            try
            {
                sw = new StreamWriter(filePath, false, Encoding.Unicode);
                for (int i = 0; i < 6; i++)
                {
                    strColumn.Append("Wave" + (i + 1).ToString());
                    strColumn.Append(",");
                }
                strColumn.Remove(strColumn.Length - 1, 1);
                sw.WriteLine(strColumn);    //write the column name

                for (int i = 0; i < ListWave.LongCount(); i++)
                {
                    strValue.Remove(0, strValue.Length); //clear the temp row value
                    strValue.Append(ListWave[i].Wave1);
                    strValue.Append(",");
                    strValue.Append(ListWave[i].Wave2);
                    strValue.Append(",");
                    strValue.Append(ListWave[i].Wave3);
                    strValue.Append(",");
                    strValue.Append(ListWave[i].Wave4);
                    strValue.Append(",");
                    strValue.Append(ListWave[i].Wave5);
                    strValue.Append(",");
                    strValue.Append(ListWave[i].Wave6);
                    sw.WriteLine(strValue); //write the row value
                }
            }
            catch (Exception ex)
            {
                successFlag = false;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Dispose();
                }
            }
            return successFlag;
        }

        public string SaveWaveData(string filePath)
        {
            bool IsSave = SaveDataToCSVFile(filePath);
            if (IsSave)
                return "保存成功";
            else
                return "保存失败";
        }

        private bool LoadDataToList(string filePath)
        {
            WaveDatatype datatemp = new WaveDatatype();
            StreamReader sReader = null;
            //读取CSV
            sReader = new StreamReader(filePath, Encoding.Unicode);
            string str = null;
            bool IsReadRight = false;
            string[] aryLine = null;
            while ((str = sReader.ReadLine()) != null)
            {
                if(IsReadRight == false)
                {
                    if(str == "Wave1,Wave2,Wave3,Wave4,Wave5,Wave6")
                    {
                        IsReadRight = true;
                    }
                }
                else
                {
                    float[] dt = new float[6];
                    aryLine = str.Split(',');
                    for (int j = 0; j < 6; j++)
                    {
                         dt[j] = Convert.ToSingle(aryLine[j]);
                    }
                    datatemp.Wave1 = dt[0];
                    datatemp.Wave2 = dt[1];
                    datatemp.Wave3 = dt[2];
                    datatemp.Wave4 = dt[3];
                    datatemp.Wave5 = dt[4];
                    datatemp.Wave6 = dt[5];
                    ListWave.Add(datatemp);
                }
            }
            return IsReadRight;
        }

        public string LoadData(string filePath)
        {
            if (LoadDataToList(filePath))
                return "载入成功";
            else
                return "载入失败";
        }

        public void FramBarChange(float e)
        {
            if (e < 0 || e > 1) return;
            if(IsWaveStatic)
            {
                ShowXstart = (int)(e * (float)ListWave.LongCount());
                NowFrameCount = (int)(ShowXstart + (float)(width - X0) * Uintx[Uintxindex]);
                if (NowFrameCount > ListWave.LongCount())
                    NowFrameCount = (int)ListWave.LongCount();
            }
            else
            {
                NowFrameCount = (int)(e * (float)ListWave.LongCount());
            }
        }

    }
}
