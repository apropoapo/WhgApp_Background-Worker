using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;

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
        public int oldCount { get; set; }
        public int newCount { get; set; }
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
            oldCount = 0;
            newCount = 0;
            newScoutId = 0;
        }


        //// string s = http://www.immobilienscout24.de/Suche/S-82/Wohnung-Miete/Bayern/Muenchen/Altstadt_Am-Hart_Freimann_Haidhausen_Laim_Lehel_Ludwigsvorstadt-Isarvorstadt_Maxvorstadt_Neuhausen_Nymphenburg_Schwabing_Schwabing-West_Schwanthalerhoehe_Sendling_Thalkirchen/2,00-3,00/-/EURO-450,00-800,00;

        public bool updateResults()
        {
            WebClient myWebClient = new WebClient();
            StreamReader sr;
            Stream myStream;
            try
            {
                myStream = myWebClient.OpenRead(ImmoscoutURL);
                sr = new StreamReader(myStream);

                oldCount = newCount;
                oldScoutId = newScoutId;

                string zeile = "fail";
                string anzahl = "noch nix";
                string such = "data";
                string suchAnzahl = "aktuelle Angebote";
                int abbrechVar = -1;

                while (abbrechVar < 0)
                {
                    zeile = sr.ReadLine();
                    if (!String.IsNullOrEmpty(zeile))
                    {
                        if (zeile.IndexOf(suchAnzahl) > 0)
                        {
                            anzahl = zeile;
                        }
                        abbrechVar = zeile.IndexOf(such);
                    }
                }

                newScoutId = int.Parse(zeile.Substring(46, 8));
                newCount = int.Parse(anzahl.Substring(69, 2));
                sr.Close();
                myStream.Close();
                return true;
            }
            catch (System.Net.WebException e)
            {

                return false;
            }
        }


        public bool check()
        {
            if (oldScoutId != 0 && newScoutId != oldScoutId && newCount >= oldCount)
            {
                return true;
            }
            return false;
        }

        //to do, im moment nur test
        public void sendPush()
        {

        }

    }
}
