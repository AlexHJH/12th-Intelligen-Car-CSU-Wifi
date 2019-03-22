using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Runtime.InteropServices;
using System.Reflection;


namespace Alex_TCP
{
    class Alex_ImageProcess
    {
        #region P/Invoke helpers

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        #endregion

        public bool IsPicStatic = false;
        /// 图像宽度
        /// </summary>
        public int Width = 100;
        /// <summary>
        /// 图像高度
        /// </summary>
        public int Height = 100;

        public double[] MatPerspective = null;
        /// <summary>
        /// 图像链表
        /// </summary>
        public List<Bitmap> ListPic = new List<Bitmap>();
        /// <summary>
        /// 当前显示索引
        /// </summary>
        public int NowFrameCount = 0;
        public bool IsCanny = false;
        public bool IsPicDeal = false;
        public bool IsBinary = false;
        public bool Isperspective = false;
        public bool IsDLL = false;

        public float PicPitch = 40;
        public float PicFocus = 2.2f;
        public float PicSensorHeight = 2.88f;
        public float PicHeight = 300;
        public float PicLimitFar = 1500;
        public bool IsPerspective = false;
        public double NearDistance = 0;
        public double FarDistance = 0;
        public float MatDouble = 1;
        public double OSTUThread = 0;


        public string hLib;
        /// <summary>
        /// 数组转为灰度图
        /// </summary>
        /// <param name="rawValues"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        private  Bitmap ToGrayBitmap(byte[] rawValues, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定  
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

            //// 获取图像参数  
            int stride = bmpData.Stride;  // 扫描线的宽度  
            int offset = stride - width;  // 显示宽度与扫描线宽度的间隙  
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置  
            int scanBytes = stride * height;// 用stride宽度，表示这是内存区域的大小  

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组  
            int posScan = 0, posReal = 0;// 分别设置两个位置指针，指向源数组和目标数组  
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存  

            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描  
                for (int y = 0; y < width; y++)
                {
                    pixelValues[posScan++] = rawValues[posReal++];
                }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”  
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中  
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域  

            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度  
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format8bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            for (int i = 0; i < 256; i++)
            {
                tempPalette.Entries[i] = Color.FromArgb(i, i, i);
            }

            bmp.Palette = tempPalette;

            //// 算法到此结束，返回结果  
            return bmp;
        }

        private Bitmap ToBinaryBitmap(byte[] rawValues, int width, int height)
        {
            //// 申请目标位图的变量，并将其内存区域锁定  
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format1bppIndexed);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
            ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

            //// 获取图像参数  
            int stride = bmpData.Stride;  // 扫描线的宽度  
            int offset = stride - width / 8;  // 显示宽度与扫描线宽度的间隙  
            IntPtr iptr = bmpData.Scan0;  // 获取bmpData的内存起始位置  
            int scanBytes = stride * height;// 用stride宽度，表示这是内存区域的大小  

            //// 下面把原始的显示大小字节数组转换为内存中实际存放的字节数组  
            int posScan = 0, posReal = 0;// 分别设置两个位置指针，指向源数组和目标数组  
            byte[] pixelValues = new byte[scanBytes];  //为目标数组分配内存  

            for (int x = 0; x < height; x++)
            {
                //// 下面的循环节是模拟行扫描  
                for (int y = 0; y < width / 8; y++)
                {
                    pixelValues[posScan++] = rawValues[posReal++];
                }
                posScan += offset;  //行扫描结束，要将目标位置指针移过那段“间隙”  
            }

            //// 用Marshal的Copy方法，将刚才得到的内存字节数组复制到BitmapData中  
            System.Runtime.InteropServices.Marshal.Copy(pixelValues, 0, iptr, scanBytes);
            bmp.UnlockBits(bmpData);  // 解锁内存区域  


            //// 下面的代码是为了修改生成位图的索引表，从伪彩修改为灰度  
            ColorPalette tempPalette;
            using (Bitmap tempBmp = new Bitmap(1, 1, PixelFormat.Format1bppIndexed))
            {
                tempPalette = tempBmp.Palette;
            }
            tempPalette.Entries[1] = Color.FromArgb(0, 0, 0);
            tempPalette.Entries[0] = Color.FromArgb(255, 255, 255);

            bmp.Palette = tempPalette;
            //// 算法到此结束，返回结果  
            return bmp;
        }

        public void BmpAdd(byte[] rawValues, int width, int height, PixelFormat e)
        {
            Bitmap bmptemp = null;
            if(e == PixelFormat.Format8bppIndexed)
                bmptemp = ToGrayBitmap(rawValues, width, height);
            else if(e == PixelFormat.Format1bppIndexed)
                bmptemp = ToBinaryBitmap(rawValues, width, height);
            ListPic.Add(bmptemp);
        }

        private Bitmap PicPerspective(Bitmap bp)
        {
            Image img = bp;
            if (IsPerspective == false)
                return bp;
            Image<Emgu.CV.Structure.Gray, Byte> src = new Image<Gray, Byte>(new Bitmap(img));
            Matrix<float> warp_matrix = new Matrix<float>(3, 3);//旋转矩阵
            Rectangle rc;
            rc = CvInvoke.cvGetImageROI(src);//获取图像的大小
            double SetFarLimit = PicLimitFar;                     ///远端距离限制
            double pitchAngle = PicPitch * Math.PI / 180;  ////俯仰角
            double f = PicFocus;                           ///焦距
            double h = PicHeight;                                ///高度
            double CameraH = PicSensorHeight;

   
            double DetLAngle = pitchAngle - Math.Atan(h / SetFarLimit);
            double DetL = f * Math.Tan(DetLAngle);

            double NearAngle = Math.PI / 2 - pitchAngle + Math.Atan(-CameraH / 2 / f);
            NearDistance = h * Math.Tan(NearAngle);         ////最近端距离
            if(DetL > CameraH / 2)
            {
                DetL = CameraH / 2;
            }
            double FarAngle = Math.PI / 2 - pitchAngle + Math.Atan(DetL / f);
            FarDistance = h * Math.Tan(FarAngle); ////最远端距离

            double TransD = f / Math.Cos(CameraH / 2 / f);
            double TransK = 1000 / rc.Width * f / Math.Cos(DetLAngle);
            int WidthFar = 1000;
            int WidthNear = (int)(rc.Width * TransK * Math.Cos(FarAngle) / Math.Cos(NearAngle) / TransD);
            int HeightAll = (int)(rc.Width * (TransK * Math.Sin(FarAngle) + TransK * Math.Cos(FarAngle) * Math.Tan(NearAngle)) / (CameraH / 2 + DetL));

             int srcheight = -(int)(DetL / CameraH * rc.Height) + src.Height / 2;
             rc.Width = WidthFar;
             rc.Height = HeightAll;
             Image<Gray, byte> dst = new Image<Gray, byte>(rc.Size);
             MatDouble = (float)(h / (TransK * Math.Cos(FarAngle)));

            float xx = 0;
            xx = 1- ((float)WidthNear / (float)WidthFar);
            PointF[] srcTri = new PointF[4];//浮点型的坐标点，用于计算旋转矩阵
            PointF[] dstTri = new PointF[4];
            srcTri[0].X = 0;                //src Top left
            srcTri[0].Y = srcheight;
            srcTri[1].X = src.Width;   //src Top right
            srcTri[1].Y = srcheight;
            srcTri[2].X = 0;              //src Bottom left
            srcTri[2].Y = src.Height;
            srcTri[3].X = src.Width;   //src Bot right
            srcTri[3].Y = src.Height;


            dstTri[0].X = 0;           //dst Top left
            dstTri[0].Y = 0;
            dstTri[1].X = dst.Width;   //dst Top right
            dstTri[1].Y = 0;
            dstTri[2].X = dst.Width * xx / 2;         //dst Bottom left
            dstTri[2].Y = dst.Height;
            dstTri[3].X = dst.Width * (1.0f - xx / 2);        //dst Bottom right
            dstTri[3].Y = dst.Height;

           
            Mat mywarpmat = CvInvoke.GetPerspectiveTransform(srcTri, dstTri);
            CvInvoke.PerspectiveTransform(srcTri, mywarpmat);

            MatPerspective = GetDoubleArray(mywarpmat);
            SetDoubleArray(mywarpmat, MatPerspective);
            CvInvoke.WarpPerspective(src, dst, mywarpmat, dst.Size);//根据旋转矩阵进行透视变换
            return dst.ToBitmap();
        }

        private Bitmap PicBinary(Bitmap bp)
        {
            Image img = bp;
            Image<Emgu.CV.Structure.Gray, Byte> src = new Image<Gray, Byte>(new Bitmap(img));
            Rectangle rc;
            rc = CvInvoke.cvGetImageROI(src);//获取图像的大小
            Image<Gray, byte> dst = new Image<Gray, byte>(rc.Size);
            double thread = 0;
            thread = CvInvoke.Threshold(src, dst, 26, 255, ThresholdType.Binary);
            OSTUThread = thread;
            return dst.ToBitmap();
        }


        private double[] GetDoubleArray(Mat mat)
        {
            double[] temp = new double[mat.Height * mat.Width];
            Marshal.Copy(mat.DataPointer, temp, 0, mat.Height * mat.Width);
            return temp;
        }

        public void SetDoubleArray(Mat mat, double[] data)
        {
            Marshal.Copy(data, 0, mat.DataPointer, mat.Height * mat.Width);
        }
        
        private Bitmap PicCanny(Bitmap bp)
        {
            Image img = bp;
            Image<Gray, Byte> img1 = new Image<Gray, Byte>(new Bitmap(bp));
            Image<Gray, Byte> cannyGray = img1.Canny(30, 70);
            return cannyGray.ToBitmap();
        }
        
        public Bitmap PicShow()
        {
            if(IsPicStatic == false)
                NowFrameCount++;
            if (NowFrameCount > ListPic.LongCount())
                NowFrameCount = (int)ListPic.LongCount();
            if (NowFrameCount > 0)
            {
                Bitmap bp = null;
                bp = ListPic[NowFrameCount - 1];
                if (IsPicDeal && IsBinary)
                    bp =  PicBinary(bp);
                if (IsPicDeal && IsCanny)
                    bp =  PicCanny(bp);
                if (IsPicDeal && Isperspective)
                    bp =  PicPerspective(bp);

               /* if(IsPicDeal && IsDLL)
                {
                    Bitmap bp_out = new Bitmap(bp.Width, bp.Height, PixelFormat.Format8bppIndexed);
                    Image<Gray, Byte> Frame0 = new Image<Gray, Byte>(bp);
                    Image<Gray, Byte> Frame1 = new Image<Gray, Byte>(bp_out);
                    Mat in_img = new Mat();
                    Mat out_img = new Mat();
                    CvInvoke.BitwiseAnd(Frame0, Frame0, in_img);
                    CvInvoke.BitwiseAnd(Frame1, Frame1, in_img);

                    IntPtr pDll = LoadLibrary(hLib);

                    IntPtr pAddressOfFunctionToCall = GetProcAddress(pDll, "ImgSample");

                }*/
              
                return bp;            
            }
            else
            {
                Bitmap bp = new Bitmap(100, 100, PixelFormat.Format8bppIndexed);
                return bp;
             }
        }

        public string PicSave(string filepath, bool IsAllSave)
        {
            if(NowFrameCount == 0)
                return "保存失败";
            if (IsAllSave)
            {
                try
                {
                    int FrameCountTemp = (int)ListPic.LongCount();
                    for(int i = 0; i < FrameCountTemp; i++)
                    {

                        ListPic[i].Save(filepath + i.ToString("D5") + ".bmp");
                    }
                }
                catch (Exception)
                {
                    return "保存失败";
                }
                return "保存成功";
            }
            else
            {
                try
                {
                    ListPic[NowFrameCount - 1].Save(filepath);
                }
                catch (Exception)
                {
                    return "保存失败";
                }
                return "保存成功";
            }
        }

        public string PicLoad(string filepath)
        {
            try
            {
                Bitmap bp = new Bitmap(filepath);
                ListPic.Add(bp);
            }
            catch(Exception)
            {
                return "导入失败";
            }
            return "导入成功";
        }

        public void FramBarChange(float e)
        {
            if (e < 0 || e > 1) return;
            {
                NowFrameCount = (int)(e * (float)ListPic.LongCount());
            }
        }

       
        public void LoadDll(string filename)
        {
            hLib = String.Copy(filename);
        }

        public void FrameClear()
        {
            ListPic.Clear();
        }
    }
}
