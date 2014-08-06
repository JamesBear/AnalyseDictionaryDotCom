using DictManiac.Sources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DictManiac
{
    public partial class DictManiac : Form
    {
        private DownloadTask downloadTask;
        private DownloadTask downloadTask2;
        private DownloadTask downloadTask3;

        public DictManiac()
        {
            InitializeComponent();
            MyRandom.Initialize();
        }

        private void buttonTest_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("It worked!");
            MyLogger.Attach("hello, there");
            for (int i = 0; i < 100000; i++)
            {
                MyLogger.Attach("Hello, my initials are " + MyRandom.NextChar() + MyRandom.NextChar() + ".");
            }
        }

        private void LoggerUpdate_Tick(object sender, EventArgs e)
        {
            if (MyLogger.AnythingNew())
            {
                textBox1.Text = MyLogger.GetContent();
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MyLogger.Attach("Hello, my initials are " + MyRandom.NextChar() + MyRandom.NextChar());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (downloadTask == null)
            {
                downloadTask = new DownloadTask();
            }

            if (downloadTask.GetState() == DownloadTaskState.Idle)
            {
                downloadTask.Start();
            }
            else
            {
                MyLogger.Attach("Cannot download because state is not " + DownloadTaskState.Idle.ToString() + "." );
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            if (downloadTask2 == null)
            {
                downloadTask2 = new DownloadTask();
                downloadTask2.OptionOutputPath("BaiduHtmls\\");
                downloadTask2.OptionWordToUrl(word => "http://dict.baidu.com/s?wd=" + word);
            }

            if (downloadTask2.GetState() == DownloadTaskState.Idle)
            {
                downloadTask2.Start();
            }
            else
            {
                MyLogger.Attach("Cannot download because state is not " + DownloadTaskState.Idle.ToString() + "." );
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {


            if (downloadTask3 == null)
            {
                downloadTask3 = new DownloadTask();
                downloadTask3.OptionOutputPath("ReferHtmls\\");
                downloadTask3.OptionWordToUrl(word => "http://dictionary.reference.com/browse/" + word);
            }

            if (downloadTask3.GetState() == DownloadTaskState.Idle)
            {
                downloadTask3.Start();
            }
            else
            {
                MyLogger.Attach("Cannot download because state is not " + DownloadTaskState.Idle.ToString() + "." );
            }
        }

        private void buttonAnalyse_Click(object sender, EventArgs e)
        {
            string htmlContent;
            if (FileUtil.ReadFile("ReferHtmls\\take.html", out htmlContent))
            {
                var result = ReferDotComAnalyser.Analyse(htmlContent);
                MyLogger.Attach(result.definition);
            }
            else
            {
                MyLogger.Attach("Fail to read file ReferHtmls\\take.html");
            }

            if (FileUtil.ReadFile("ReferHtmls\\abandon.html", out htmlContent))
            {
                var result = ReferDotComAnalyser.Analyse(htmlContent);
                MyLogger.Attach(result.definition);
            }
            else
            {
                MyLogger.Attach("Fail to read file ReferHtmls\\take.html");
            }
        }
    }
}
