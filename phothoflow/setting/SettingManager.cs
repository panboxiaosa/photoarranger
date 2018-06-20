using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace phothoflow.setting
{
    class SettingManager
    {
        [DllImport("kernel32")] //返回0表示失败，非0为成功
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")] //返回取得字符串缓冲区的长度
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        const string DEFAULTDPI = "30,60,72,120";

        const string DEFAULTMARGIN = "2,4,8";

        const string DEFAULTWIDTH = "160,180,200,220,250,208,320,400,500";

        const string DPISETTING = "dpi";
        
        const string MARGINSETTING = "margin";

        const string WIDTHSETTING = "width";

        public const string WIDTHWORDING = "宽度设置(cm)";
        public const string MARGINGWORDING = "边距设置(cm)";
        public const string DPIWORDING = "分辨率设置";

        public static string GetDefault(string name) {
            if (name == DPISETTING) {
                return DEFAULTDPI;
            }else if (name == MARGINSETTING) {
                return DEFAULTMARGIN;
            }else if (name == WIDTHSETTING) {
                return DEFAULTWIDTH;
            } else {
                return "";
            }
        }

        public static string GetAppPath()
        {
            string str = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "setting.ini";
            return str;
        }

        static Managable width = new Managable(WIDTHSETTING);
        static Managable dpi = new Managable(DPISETTING);
        static Managable margin = new Managable(MARGINSETTING);

        public static void Init()
        {
            width.Load();
            dpi.Load();
            margin.Load();
        }

        public static float GetDpi()
        {
            return dpi.Get()[dpi.Current()];
        }

        public static float GetWidth()
        {
            float w = width.Get()[width.Current()];
            return w * 0.3937008f;
        }

        public static float GetMargin()
        {
            return margin.Get()[margin.Current()] * 0.3937008f;
        }

        public static Managable Get(string currentDeal) {
            Managable choose = null;
            switch (currentDeal)
            {
                case WIDTHWORDING:
                    choose = width;
                    break;
                case MARGINGWORDING:
                    choose = margin;
                    break;
                case DPIWORDING:
                    choose = dpi;
                    break;
                default:
                    break;
            }
            return choose;
        }

    }
}
