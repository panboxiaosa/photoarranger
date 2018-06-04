using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace phothoflow.setting
{
    class Managable
    {
        string PartName;

        const string CURRENTKEY = "current";

        const string FULLKEY = "full";

        ObservableCollection<float> configs;

        int selected;

        public Managable(string name)
        {
            PartName = name;
        }

        public void Add(float item)
        {
            if (configs.Contains(item))
                return;
            configs.Insert(0, item);
            selected = 0;
            Save();
        }

        public ObservableCollection<float> Get()
        {
            return configs;
        }

        public int Current()
        {
            return selected;
        }

        void UsingDefault()
        {
            string[] vals = SettingManager.GetDefault(PartName).Split(',');
            foreach (string item in vals)
            {
                configs.Add(float.Parse(item));
            }
            selected = 0;
            Save();
        }

        public void Load()
        {
            StringBuilder temp = new StringBuilder(200);
            configs = new ObservableCollection<float>();
            int ret = SettingManager.GetPrivateProfileString(PartName, CURRENTKEY, "",  temp, 200, SettingManager.GetAppPath());
            if (ret <= 0)
            {
                UsingDefault();
                return;
            }
            selected = int.Parse(temp.ToString());

            ret = SettingManager.GetPrivateProfileString(PartName, FULLKEY, "",  temp, 200, SettingManager.GetAppPath());
            string[] vals =  temp.ToString().Split(',');
            foreach(string item in vals) {
                configs.Add(float.Parse(item));
            }
        }

        void Save()
        {
            string iniPath = SettingManager.GetAppPath();
            SettingManager.WritePrivateProfileString(PartName, CURRENTKEY, selected.ToString(), SettingManager.GetAppPath());
            SettingManager.WritePrivateProfileString(PartName, FULLKEY, string.Join(",", configs), SettingManager.GetAppPath());
        }

        public void Select(int index)
        {
            selected = index;
            Save();
        }

    }
}
