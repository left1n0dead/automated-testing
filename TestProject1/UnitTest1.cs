using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Xml;
using System.Globalization;
using System.Net;
using System.IO;
using System.Text;

namespace TestProject1
{
    [TestFixture]
    public class Tests
    {

        //Constants
        private const string URL_1 = "http://demo.macroscop.com:8080/command?type=gettime&login=root&password="; //URL to get server's XML response including its DateTime 
        private const string URL_2 = "http://demo.macroscop.com:8080/configex?login=root&password="; //URL to get server's config XML
        private const string URL_3 = "http://demo.macroscop.com:8080/command?type=gettime&login=root&password=&responsetype=json"; // URL to get server's JSON response including its DateTime

        //UnitRequestMethods required for----------------------------------------------------------------------------------------------------------------------------------------
       
        public string GetTimeRequestXML(string URL)
        {
           
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.KeepAlive = false;
          
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      
            string s = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();

            s = Regex.Match(s, "<?.*</string>").Value;
            char[] CharsToTrim = new char[] { '<', 's', 't', 'r', 'i', 'n', 'g', '/', '>' };
            string t = s.Trim(CharsToTrim);
            return t;
        }

        public int GetConfigRequest(string URL)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.KeepAlive = false;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            string s = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();

            String sSearch = "<channelinfo";

            String[] words = s.Split(new char[] { ' ', '\n' });

            int counter = 0;

            foreach (var word in words)
            {
                if (sSearch == word.ToLower())
                    counter++;
            }
            return counter;
        }

        public string GetTimeRequestJSON(string URL)
        {
            var http = (HttpWebRequest)WebRequest.Create(new Uri(URL));
            http.Accept = "application/json";
            http.ContentType = "application/json";
            http.Method = "POST";

            string parsedContent = "Parsed JSON Content should be there";
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] bytes = encoding.GetBytes(parsedContent);

            Stream newStream = http.GetRequestStream();
            newStream.Write(bytes, 0, bytes.Length);
            newStream.Close();

            var response = http.GetResponse();

            var stream = response.GetResponseStream();
            var sr = new StreamReader(stream);
            var content = sr.ReadToEnd();
            string date = content.ToString();
            string datea = String.Empty;

            for (int x = 0; x < date.Length; x++)
            {
                datea += date[x].ToString().Replace("\"", "");
            }

            return datea;
        }

        //UnitTestMethods required for----------------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void RetryIsFiredDueToTimeout()//Test for task ¹1 
        {
            DateTime ServDate = new DateTime();                  
            DateTime LocalDate = DateTime.Now;
            ServDate = DateTime.Parse(GetTimeRequestXML(URL_1));
            ServDate = ServDate.ToUniversalTime();
            LocalDate = LocalDate.ToUniversalTime();
            TestContext.Out.WriteLine($"Local time: " + LocalDate.ToString());
            TestContext.Out.WriteLine($"Requested server time: " + ServDate.ToString());
            int diff = Math.Abs(LocalDate.Second - ServDate.Second);
            TestContext.Out.WriteLine($"The difference: " + diff.ToString()+ "s.");

            if (diff>15)
            {
                Assert.Fail("ERROR: ServerRequestTimeout");
            }
        }

        [Test]
        public void ChannelsInConfigCount()//Test for task ¹3
        {
            if(GetConfigRequest(URL_2)<6)
            {
                Assert.Fail("ERROR: TooFewChannels: "+ GetConfigRequest(URL_2).ToString());
            }
            TestContext.Out.WriteLine($"Channels count: " + GetConfigRequest(URL_2).ToString());
        }

        [Test]
        public void RetryIsFiredDueToTimeout_2()//Test for task#2
        {
            DateTime ServDate = new DateTime();
            DateTime LocalDate = DateTime.Now;

            ServDate = DateTime.Parse(GetTimeRequestJSON(URL_3));
            ServDate = ServDate.ToUniversalTime();
            LocalDate = LocalDate.ToUniversalTime();
            TestContext.Out.WriteLine($"Local time: " + LocalDate.ToString());
            TestContext.Out.WriteLine($"Requested server time: " + ServDate.ToString());
            int diff = Math.Abs(LocalDate.Second - ServDate.Second);
            TestContext.Out.WriteLine($"The difference: " + diff.ToString()+ "s.");
            if (diff > 15)
            {
                Assert.Fail("ERROR: ServerRequestTimeout");
            }
        }
       
    }
}