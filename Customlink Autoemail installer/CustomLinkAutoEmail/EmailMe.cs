using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using CallRestAPI;
using AutoEmailPartner.Responces;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ThreadSafeCall;

namespace AutoEmailPartner
{
    public partial class EmailMe : Form
    {
        public String path = "C:\\kpconfig\\BOSKPAutoEmailAgent.ini";
        IniFile ini;
        private string firstHour, secondHour, thirdHour, resetHour,hours;
        bool state;
        public EmailMe()
        {
            InitializeComponent();
            ini = new IniFile(path);
            hours = ini.IniReadValue("Time Checking", "time");
            string[] splitHours = hours.Split(',');
            firstHour = splitHours[0].ToString();
            secondHour = splitHours[1].ToString();
        }
        public Uri getBaseAddress()
        {
            string url = ini.IniReadValue("BaseAddress", "baseAddress");
            Uri uri = new Uri(url);
            return uri;
        }
        private void EmailBtn_Click(object sender, EventArgs e)
        {
            if (EmailBTN.Text == "Start")
            {
               
                        timer1.Start();
                        EmailBTN.SafeInvoke(d => d.Text = "Stop");
                        state = true;
                        Task.Factory.StartNew(() =>
                        {
                            doWhileAuto();
                        });
            }
            else 
            {
                state = false;
                timer1.Stop();
                EmailBTN.SafeInvoke(d => d.Text = "Start");
            }
        }
        public String sendRequest(Uri uri)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11;
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string jsonString = null;

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Timeout = Timeout.Infinite;

            var webresponse = (HttpWebResponse)request.GetResponse();
            Stream response = webresponse.GetResponseStream();
            using (StreamReader sr = new StreamReader(response))
            {
                jsonString = sr.ReadToEnd();
                sr.Close();
            } 

            return jsonString;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            label1.SafeInvoke(d => d.Text = DateTime.Now.ToString("T"));
            
        }

        private void EmailMe_Load(object sender, EventArgs e)
        {
            EmailBTN.PerformClick();
        }
        public void doWhileAuto() 
        {
            String json;
            do
            {
                try
                {
                    
                        if (label1.Text == firstHour || label1.Text == secondHour || label1.Text == thirdHour)
                        {
                            Indication.SafeInvoke(d => d.Text = "Sending Email");
                            log("[Starting to Send Email - " + DateTime.Now.ToString()+"]");
                            List<responce> partTOemail = new List<responce>();
                            Uri strUrl = new Uri(getBaseAddress().ToString() + ("/sendingCustDetails"));
                            json = sendRequest(strUrl);
                            responce resp = JsonConvert.DeserializeObject<responce>(json);
                            if (resp.rescode == "1") 
                            {
                                Indication.SafeInvoke(d => d.Text = resp.resmessage);
                                log(resp.resmessage);
                                Thread.Sleep(5000);
                                Indication.SafeInvoke(d => d.Text = "");
                            }
                            else 
                            {
                                Indication.SafeInvoke(d => d.Text = resp.resmessage);
                                log(resp.resmessage);
                            }
                        }
                }
                catch (Exception ex)
                {
                    Indication.SafeInvoke(d => d.Text = "Error Found");
                    log(ex.ToString());
                }
            }
            while (state == true);
        }
        private void log(String message) 
        {
            string path = @"C:\BOSKPAutoEmailAppAppLogs\";
            System.IO.Directory.CreateDirectory(path);
            StreamWriter logWriter;

            if (!File.Exists(path + "BOSKPAutoEmail.txt"))
            {
                File.Create(path + "BOSKPAutoEmail.txt").Dispose();
            }
            logWriter = new StreamWriter(path + "BOSKPAutoEmail.txt", true, Encoding.ASCII);
            logWriter.Write(message);
            logWriter.WriteLine();
            logWriter.Close();
        }
    }
}
