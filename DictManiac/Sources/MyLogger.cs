using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DictManiac.Sources
{
    class MyLogger
    {
        public const int MAX_CHARS = 10000 * 20;
        private static MyLogger instance = new MyLogger();

        private StringBuilder sb;
        private string lastContent;
        int version;
        int lastVersion;
        private bool isProcessing;

        private MyLogger()
        {
            sb = new StringBuilder();
            version = 0;
            lastVersion = -1;
            lastContent = null;
            isProcessing = false;
        }

        private void AttachImpl(string msg)
        {
            //totalMsg += (msg + "\r\n");
            sb.Append(msg + "\r\n");
            //if (display != null)
            //    display.Text = totalMsg;
            version++;
        }

        private string GetContentImpl()
        {
            if (lastVersion == version)
            {
                return lastContent;
            }

            lastContent = sb.ToString();
            lastVersion = version;

            if (lastContent.Length > MAX_CHARS)
            {
                lastContent = lastContent.Substring(lastContent.Length / 2);
                sb.Clear();
                sb.Append(lastContent);
            }

            return lastContent;
        }

        public static void Attach(string msg)
        {
            instance.AttachImpl(msg);
        }

        public static string GetContent()
        {
            return instance.GetContentImpl();   
        }

        public static bool AnythingNew()
        {
            return (instance.lastVersion != instance.version);
        }
    }
}
