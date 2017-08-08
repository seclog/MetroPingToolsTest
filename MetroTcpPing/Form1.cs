using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MetroTcpPing
{
    public partial class MainForm : MetroFramework.Forms.MetroForm
    {

        private static object locker = new object();
        public MainForm()
        {
            InitializeComponent();
        }
        public void Ping(DataTable table, string strAddr, object callback)
        {
            string[] split = strAddr.Split(':');
            string ip = split[0];
            string port = split[1];
            string timeout = "";
            var cdb = callback as InvokeHandler;
            IPAddress ipAddr;
            try
            {
                ipAddr = Dns.GetHostAddresses(ip)[0];
            }
            catch
            {
                timeout = "域名无法解析";
                cdb(table, ip, timeout);
                return;
            }
            ipAddr = Dns.GetHostAddresses(ip)[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, int.Parse(port));
            var times = new List<double>();

            var sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = true;

            var stopwatch = new Stopwatch();

            // Measure the Connect call only
            stopwatch.Start();
            try
            {
                sock.Connect(endPoint);
            }
            catch
            {
                sock.Close();
                timeout = "端口连接失败";

                cdb(table, ip, timeout);

                return;

            }

            stopwatch.Stop();

            double t = stopwatch.Elapsed.TotalMilliseconds;
            times.Add(t);
            sock.Close();

            //Thread.Sleep(1000);

            timeout = times.Average().ToString() + "ms";
            cdb(table, ip, timeout);
        }

        private void pingButton_Click(object sender, EventArgs e)
        {
          
            DataTable table = new DataTable();
            table.Columns.Add("IP", typeof(string));
            table.Columns.Add("延时", typeof(string));

            resultGridView.DataSource = table;


            string[] iplist = ipListTextbox.Text.Trim().Split('\n');
            List<string> timeouts = new List<string>();
            foreach (var ip in iplist)
            {
                //此处线程需要管理，判断所有线程终止后
                InvokeHandler callback = new InvokeHandler(AddRow);
                Thread th = new Thread(() => { Ping(table, ip, callback); });
                th.Start();
            }
     

        }

        private delegate void InvokeHandler(DataTable table, string ip, string timeout);

        private void AddRow(DataTable table, string ip, string timeout)
        {
            DataRow dr = table.NewRow();
            dr["IP"] = ip;
            dr["延时"] = timeout;
            lock (locker)
            {
                table.BeginLoadData();
                table.Rows.Add(dr);
                table.EndLoadData();
                table.AcceptChanges();
                UpdateGV(table);
            }
        }
        private delegate void UpdateDataGridView(DataTable table);
        //private void UpdateGV(DataTable dt)
        //{
        //    if (resultGridView.InvokeRequired)
        //    {
        //        this.BeginInvoke(new UpdateDataGridView(UpdateGV), new object[] { dt });
        //    }
        //    else
        //    {
        //        resultGridView.DataSource = dt;
        //        resultGridView.Refresh();
        //    }
        //}

        private void UpdateGV(DataTable table)
        {
            if (resultGridView.InvokeRequired)
            {
                this.BeginInvoke(new UpdateDataGridView(UpdateGV), new object[] { table });
            }
            else
            {
                resultGridView.Refresh();
                //resultGridView.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                //resultGridView.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
                PerformLayout();
            }


        }

        private void resultGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            lock (locker)
            {
                resultGridView.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
                resultGridView.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;

            }
        }
    }
}
