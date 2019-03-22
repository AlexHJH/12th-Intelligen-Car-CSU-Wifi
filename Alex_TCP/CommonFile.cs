using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Alex_TCP
{
    public enum FrameType
    {
        Picture = 0,
        Wave = 1,
        Route = 2,
        Typemax = 3
    }
    public delegate void StrInvoke(string str);      ///UI调用
    public delegate void PicInvoke(Bitmap bp);


}
