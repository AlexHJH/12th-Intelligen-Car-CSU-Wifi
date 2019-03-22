using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Alex_TCP
{
    class Alex_RouteShow
    {
        public List<PointF> ListPoint = new List<PointF>();

        public struct RouteType
        {
            public float Data1;
            public float Data2;
            public float Data3;
            public float Data4;
            public float Data5;
            public float Data6;
            public int DataSize;
        }
        public List<RouteType> ListRouteData = new List<RouteType>();

        public int NowFrameCount = 0;

        public int X_Site = 0;

        public int Y_Site = 0;

        private int PicWidth = 0;

        private int PicHeight = 0;

        private float Uintn = 0;

        public bool IsRouteStatic = false;

        public void RouteShow_init(int width, int height)
        {
            X_Site = width / 2;
            Y_Site = height / 2;
            PicWidth = width;
            PicHeight = height;
            Uintn = 1;
            IsRouteStatic = false;
        }

        public Bitmap RouteShow(int width, int height)
        {
            Bitmap objBitmap = new Bitmap(width, height);
            Graphics objGraphics = Graphics.FromImage(objBitmap);
            Pen usepen = new Pen(Color.White, 0.1f);
            Font ruleris = new Font("Consolas", 8);

            int xtemp = X_Site, ytemp = Y_Site;
            float uintn = Uintn;
            objGraphics.DrawLine(usepen, xtemp, 0, xtemp, height);
            objGraphics.DrawLine(usepen, 0, ytemp, width, ytemp);
            if(IsRouteStatic == false)
                NowFrameCount++;
            if(NowFrameCount > ListPoint.LongCount())
            {
                NowFrameCount = (int)ListPoint.LongCount();
            }
            List<PointF> ListTemp = new List<PointF>();
            for(int i = 0; i < ListPoint.LongCount(); i++)
            {
                ListTemp.Add(new PointF(ListPoint[i].X * uintn + xtemp, ListPoint[i].Y * uintn + ytemp));
            }
            if (ListTemp.LongCount() > 1)
            {
                objGraphics.DrawLines(new Pen(Color.Gray, 2f), ListTemp.ToArray());
            }
            if(NowFrameCount > 1)
            {
                objGraphics.DrawLines(new Pen(Color.Gold, 3f), ListTemp.Take(NowFrameCount).ToArray());

                    float[] data = new float[6];
                    data[0] = ListRouteData[NowFrameCount - 1].Data1;
                    data[1] = ListRouteData[NowFrameCount - 1].Data2;
                    data[2] = ListRouteData[NowFrameCount - 1].Data3;
                    data[3] = ListRouteData[NowFrameCount - 1].Data4;
                    data[4] = ListRouteData[NowFrameCount - 1].Data5;
                    data[5] = ListRouteData[NowFrameCount - 1].Data6;
                    for (int i = 0; i < ListRouteData[NowFrameCount - 1].DataSize; i++)
                    {
                        objGraphics.DrawString(data[i].ToString("F3"), ruleris, new SolidBrush(System.Drawing.Color.Gold), 10, i * 15 + 10);
                    }
                objGraphics.DrawEllipse(new Pen(Color.Red, 3f), ListTemp[NowFrameCount - 1].X , ListTemp[NowFrameCount - 1].Y, 6, 6);
            }
            return objBitmap;
        }

        public void RouteXYMove(int x, int y)
        {
            X_Site += x;
            Y_Site += y;
            if (X_Site > PicWidth)
                X_Site = PicWidth;
            else if (X_Site < 0)
                X_Site = 0;
            if (Y_Site > PicHeight)
                Y_Site = PicHeight;
            else if (Y_Site < 0)
                Y_Site = 0;
        }

        public void RouteUintChange(int e)
        {
            if(e > 0)
            {
                Uintn += 0.1f;
                if (Uintn > 10f)
                    Uintn = 10f;
            }
            else
            {
                Uintn -= 0.1f;
                if (Uintn < 0.1f)
                    Uintn = 0.1f;
            }
        }

        public void RouteDataAdd(float x, float y)
        {
            ListPoint.Add(new PointF(x, y));
        }

        public void FramBarChange(float e)
        {
            if (e < 0 || e > 1) return;
            NowFrameCount = (int)(e * (float)ListPoint.LongCount());
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
                strColumn.Append("XSite");
                strColumn.Append(",");
                strColumn.Append("YSite");
                strColumn.Append(",");
                strColumn.Append("Data1");
                strColumn.Append(",");
                strColumn.Append("Data2");
                strColumn.Append(",");
                strColumn.Append("Data3");
                strColumn.Append(",");
                strColumn.Append("Data4");
                strColumn.Append(",");
                strColumn.Append("Data5");
                strColumn.Append(",");
                strColumn.Append("Data6");
                strColumn.Append(",");
                strColumn.Append("DataSize");
                strColumn.Append(",");
                strColumn.Remove(strColumn.Length - 1, 1);
                sw.WriteLine(strColumn);    //write the column name

                for (int i = 0; i < ListPoint.LongCount(); i++)
                {
                    strValue.Remove(0, strValue.Length); //clear the temp row value
                    strValue.Append(ListPoint[i].X);
                    strValue.Append(",");
                    strValue.Append(ListPoint[i].Y);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].Data1);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].Data2);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].Data3);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].Data4);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].Data5);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].Data6);
                    strValue.Append(",");
                    strValue.Append(ListRouteData[i].DataSize);
                    sw.WriteLine(strValue); //write the row value
                }
            }
            catch (Exception)
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

        public string RouteDataSave(string filepath)
        {
            if (SaveDataToCSVFile(filepath))
                return "保存成功";
            else
                return "保存失败";
        }

        private bool LoadDataToList(string filePath)
        {
            PointF datatemp = new PointF();
            StreamReader sReader = null;
            //读取CSV
            sReader = new StreamReader(filePath, Encoding.Unicode);
            string str = null;
            bool IsReadRight = false;
            string[] aryLine = null;
            while ((str = sReader.ReadLine()) != null)
            {
                if (IsReadRight == false)
                {
                    if (str == "XSite,YSite,Data1,Data2,Data3,Data4,Data5,Data6,DataSize")
                    {
                        IsReadRight = true;
                    }
                }
                else
                {
                    float[] dt = new float[10];
                    aryLine = str.Split(',');
                    for (int j = 0; j < aryLine.LongCount(); j++)
                    {
                        dt[j] = Convert.ToSingle(aryLine[j]);
                    }
                    datatemp.X = dt[0];
                    datatemp.Y = dt[1];
                    RouteType RouteDatatemp = new RouteType();
                    RouteDatatemp.Data1 = dt[2];
                    RouteDatatemp.Data2 = dt[3];
                    RouteDatatemp.Data3 = dt[4];
                    RouteDatatemp.Data4 = dt[5];
                    RouteDatatemp.Data5 = dt[6];
                    RouteDatatemp.Data6 = dt[7];
                    RouteDatatemp.DataSize = (int)dt[8];
                    ListPoint.Add(datatemp);
                    ListRouteData.Add(RouteDatatemp);
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

        public void RouteClear()
        {
            ListPoint.Clear();
            NowFrameCount = 0;
        }
    }
}
