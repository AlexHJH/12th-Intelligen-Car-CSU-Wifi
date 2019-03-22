using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alex_TCP
{
    class Alex_Text
    {
        public bool IsSendNew = false;
        public bool IsShowOc = false;

        private int TextCount = 0;


        public string TextShow(List<byte> m, long l)
        {
            string struse = null; 
            if (IsShowOc)
            {
                for (int i = TextCount; i < l; i++)
                {
                    struse += ("0x" + m[i].ToString("X2") + " ");
                }
            }
            else
            {
                struse = Encoding.Default.GetString(m.ToArray(), TextCount, (int)(l - TextCount));
            }
            TextCount = (int)l;
            return struse;
        }

        public string TextSend(string s)
        {
            if (IsSendNew)
                return (s + "\r\n");
            else
                return s;
        }
    }
}
