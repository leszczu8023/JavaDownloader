using JavaDownloader;
using System;
using System.Windows.Forms;

namespace TestApplication
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var res = folderBrowserDialog1.ShowDialog();
            if (res == DialogResult.OK)
            {
                if (System.IO.Directory.Exists(folderBrowserDialog1.SelectedPath))
                {
                    textBox1.Text = folderBrowserDialog1.SelectedPath;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var downloader = new JavaDownloader((radioButton1.Checked) ? Arch.I586 : Arch.AMD64, progressBar1, label1, panel1);

            System.Threading.Thread t = new System.Threading.Thread(() => {
                try
                {
                    downloader.Download(textBox1.Text);
                } catch (Exception ex)
                {
                    MessageBox.Show("An unexcepted exception has trown! \n\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    label1.Invoke(new MethodInvoker(() => label1.Text = "Completed with errors!"));
                    panel1.Invoke(new MethodInvoker(() => panel1.Enabled = true));
                    progressBar1.Invoke(new MethodInvoker(() => progressBar1.Value = 0));
                }
            });

            t.SetApartmentState(System.Threading.ApartmentState.MTA);
            t.Start();

            panel1.Enabled = false;
        }
    }

    public class JavaDownloader : AbstractJavaDownloader
    {
        private ProgressBar _pg;
        private Label _lb;
        private Panel _pn;

        public JavaDownloader(Arch architecture, ProgressBar pg, Label lb, Panel pn) : base(architecture)
        {
            _pg = pg;
            _lb = lb;
            _pn = pn;
        }

        public override void OnDownloadCompleted(string targetFileName)
        {        
            MessageBox.Show("Download Completed!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _lb.Invoke(new MethodInvoker(() => _lb.Text = "Completed!"));
            _pn.Invoke(new MethodInvoker(() => _pn.Enabled = true));
            _pg.Invoke(new MethodInvoker(() => _pg.Value = 0));
        }

        public override void ReportProgress(int percentage, long received, long total)
        {
            _lb.Invoke(new MethodInvoker(() => _lb.Text = "Downloading... " + percentage + "% (" + received + "/" + total + " bytes)"));
            _pg.Invoke(new MethodInvoker(() => _pg.Value = percentage));
        }
    }

}
