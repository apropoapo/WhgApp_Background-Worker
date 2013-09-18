using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using HtmlAgilityPack;
using System.Text;
using BackgroundWorker.ServiceReference1;

namespace BackgroundWorker
{
    public class User
    {
        public int ID { get; set; }
        public int changed { get; set; }
        public string PushNotificationURI { get; set; }
        public int delete { get; set; }
        public int UsePushNotifications { get; set; }
        public string ImmoscoutURL { get; set; }
        //    public int oldCount { get; set; }
        //   public int newCount { get; set; }
        public int oldScoutId { get; set; }
        public int newScoutId { get; set; }

        public User(int ID, int changed, string PushNotificationUri, int delete, int UsePushNotifications, string ImmoscoutURL)
        {
            this.ID = ID;
            this.changed = changed;
            this.PushNotificationURI = PushNotificationUri;
            this.delete = delete;
            this.UsePushNotifications = UsePushNotifications;
            this.ImmoscoutURL = ImmoscoutURL;
            oldScoutId = 0;
            //  oldCount = 0;
            // newCount = 0;
            newScoutId = 0;
        }


        //// string s = http://www.immobilienscout24.de/Suche/S-82/Wohnung-Miete/Bayern/Muenchen/Altstadt_Am-Hart_Freimann_Haidhausen_Laim_Lehel_Ludwigsvorstadt-Isarvorstadt_Maxvorstadt_Neuhausen_Nymphenburg_Schwabing_Schwabing-West_Schwanthalerhoehe_Sendling_Thalkirchen/2,00-3,00/-/EURO-450,00-800,00;

        public bool updateResults()
        {
            // WebClient myWebClient = new WebClient();
            // StreamReader sr;
            // Stream myStream;

            // int anzahlPosi;
            //string zeile = "fail";
            //  string anzahl = "noch nix";
            //string such = "data";
            // string suchAnzahl = "aktuelle Angebote";
            // string suchZeileAnzahl = "highlight\">";

            try
            {
                //      oldCount = newCount;
                oldScoutId = newScoutId;

                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                StreamReader reader = new StreamReader(WebRequest.Create(ImmoscoutURL).GetResponse().GetResponseStream(), Encoding.GetEncoding("iso-8859-1"));
                String htmlString = reader.ReadToEnd();
                document.LoadHtml(WebUtility.HtmlDecode(htmlString));


                //  var count = document.DocumentNode.SelectNodes("/descendant::span[attribute::id=\"resultCount\"]");

                //if (count.Count == 1)
                //{

                //    newCount = Int16.Parse(count[0].InnerText.Trim().Replace(".", String.Empty));
                //}

                var id_tag = document.DocumentNode.SelectNodes("/descendant::li[attribute::class=\"media medialist box\"]");

                if (id_tag.Count > 0)
                {
                    newScoutId = Int32.Parse(id_tag[0].Attributes["data-obid"].Value);
                }



                //myStream = myWebClient.OpenRead(ImmoscoutURL);
                //sr = new StreamReader(myStream);




                //int abbrechVar = -1;

                //while (abbrechVar < 0)
                //{
                //    zeile = sr.ReadLine();
                //    if (!String.IsNullOrEmpty(zeile))
                //    {
                //        if (zeile.IndexOf(suchAnzahl) > 0)
                //        {
                //            anzahl = zeile;
                //        }
                //        abbrechVar = zeile.IndexOf(such);
                //    }
                //}

                // extrahiert die Anzahl-Zahl aus dem anzahl-String heraus
                //anzahlPosi = anzahl.IndexOf(suchZeileAnzahl) + suchZeileAnzahl.Length;
                //string anzahlZahlString = "";
                //int i = 0;
                //while (anzahl[anzahlPosi + i] != '<')
                //{
                //    anzahlZahlString += anzahl[anzahlPosi + i];
                //    i++;
                //}
                //newCount = int.Parse(anzahlZahlString);


                // extrahiert die Scoutid aus dem zeile-String
                //newScoutId = int.Parse(zeile.Substring(46, 8));

                reader.Close();

                //sr.Close();
                //myStream.Close();
                return true;
            }
            catch (System.Net.WebException)
            {

                return false;
            }
            catch (FormatException e)
            {

                Console.WriteLine(e.Message);
                return false;
            }
        }


        public bool check()
        {
            if (oldScoutId != 0 && newScoutId > oldScoutId)
            {
                return true;
            }
            return false;
        }

        //to do, im moment nur test
        public bool sendPush()
        {
            string messageText = " ";
            string titleText = "Neues Objekt gefunden:";
            if (this.ImmoscoutURL.Contains("/Wohnung-"))
            {
                titleText = "Neue Wohnung gefunden:";
            }
            else if (this.ImmoscoutURL.Contains("/Haus-"))
            {
                titleText = "Neues Haus gefunden:";
            }

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            StreamReader reader = new StreamReader(WebRequest.Create(ImmoscoutURL).GetResponse().GetResponseStream(), Encoding.GetEncoding("iso-8859-1"));
            String htmlString = reader.ReadToEnd();
            document.LoadHtml(WebUtility.HtmlDecode(htmlString));


            var header = document.DocumentNode.SelectNodes("/descendant::li[attribute::class=\"media medialist box\" and attribute::data-obid=\"" + this.newScoutId + "\"]/descendant::div[attribute::class=\"medialist__heading-wrapper\"]/descendant::a");

            if (header.Count == 1)
            {
                messageText = header[0].InnerText.Trim();
            }

            System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += delegate
            {
                ServiceClient client = new ServiceClient();


                client.SendToast(titleText, messageText, this.PushNotificationURI);



            };
            try
            {
                worker.RunWorkerAsync();
            }
            catch (Exception)
            {
                reader.Close();
                return false;
            }


            reader.Close();

            return true;
        }

    }
}
