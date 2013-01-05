using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Data;
using System.ComponentModel;
using BackgroundWorker.ServiceReference1;


namespace BackgroundWorker
{
    class Users
    {
        const string CONSTRING = "Server=instance29437.db.xeround.com;Port=19153;Database=users;Uid=appharbor;Pwd=NNDKjRzh";
        public User[] userArray = new User[100000];
        public int count;
        public int max = 0;


        // if init == true dann alle user, else nur die wo update;
        public bool updateUserArray(bool init)
        {
            //connect
            MySqlConnection con = new MySqlConnection(CONSTRING);
            con.Open();

            //adapter
            MySqlDataAdapter adapter = new MySqlDataAdapter();

            //SQL Abfrage erstellen
            string cmdText = "SELECT * FROM myapptable";
            MySqlCommand cmd = new MySqlCommand(cmdText, con);

            //Datatable abrufen
            DataTable dt = new DataTable();
            adapter.SelectCommand = cmd;
            adapter.Fill(dt);
            DataRowCollection dataRowC = dt.Rows;

            // User Array füllen!! NICHT GANZ SAUBER PROGRAMMIERT, DA MIT EXCEPTION ALS VERARBEITUNGSMITTEL GEARBEITET WIRD!!
            int i = 0;
            try
            {
                while (dataRowC[i] != null)
                {
                    DataRow dr = dataRowC[i];

                    int changed = (int)dr["changed"];
                    if (changed == 1 || init)
                    {

                        // frägt daten ab
                        int id = (int)dr["ID"];
                        string pushURI = (string)dr["PushNotificationURI"];
                        int delete = (int)dr["delete"];
                        int usePush = (int)dr["UsePushNotifications"];
                        string immoURL = (string)dr["ImmoscoutURL"];

                        // max id rausfinden
                        if (max < id)
                            max = id;

                        // fügt es ins array ein
                        userArray[id] = new User(id, changed, pushURI, delete, usePush, immoURL);

                        // setze update auf 0
                        string cmdText2 = "UPDATE myapptable SET changed=0 WHERE ID=" + id;
                        MySqlCommand cmd2 = new MySqlCommand(cmdText2, con);
                        cmd2.ExecuteNonQuery();
                    }
                    i++;

                }
            }

            catch (IndexOutOfRangeException e)
            {
                count = i;
            }
            con.Close();
            return true;
        }

        //public bool getUsersUpdate()
        //{
        //    //connect
        //    MySqlConnection con = new MySqlConnection(CONSTRING);
        //    con.Open();

        //    //adapter
        //    MySqlDataAdapter adapter = new MySqlDataAdapter();

        //    //SQL Abfrage erstellen
        //    string cmdText = "SELECT * FROM myapptable";
        //    MySqlCommand cmd = new MySqlCommand(cmdText, con);

        //    //Datatable abrufen
        //    DataTable dt = new DataTable();
        //    adapter.SelectCommand = cmd;
        //    adapter.Fill(dt);
        //    DataRowCollection dataRowC = dt.Rows;

        //    // User Array füllen!! NICHT GANZ SAUBER PROGRAMMIERT, DA MIT EXCEPTION ALS VERARBEITUNGSMITTEL GEARBEITET WIRD!!
        //    int i = 0;
        //    try
        //    {
        //        while (dataRowC[i] != null)
        //        {
        //            DataRow dr = dataRowC[i];


        //            int changed = (int)dr["changed"];
        //            if (changed == 1)
        //            {
        //                // frägt daten ab
        //                int id = (int)dr["ID"];
        //                string pushURI = (string)dr["PushNotificationURI"];
        //                int delete = (int)dr["delete"];
        //                int usePush = (int)dr["UsePushNotifications"];

        //                // max id rausfinden
        //                if (max < id)
        //                    max = id;

        //                // fügt es ins array ein
        //                userArray[id] = new User(id, changed, pushURI, delete, usePush);

        //                // setze update auf 0
        //                string cmdText2 = "UPDATE myapptable SET changed=0 WHERE ID=" + id;
        //                MySqlCommand cmd2 = new MySqlCommand(cmdText2, con);
        //                cmd2.ExecuteNonQuery();
        //            }
        //        }
        //    }
        //    catch (IndexOutOfRangeException e)
        //    {
        //        count = i;
        //    }
        //    return true;
        //}

        public string[] checkUsers()
        {
            //testzweck:
            string[] s = new string[max + 1];

            for (int i = 0; i <= max; i++)
            {
                //          try {
                if (userArray[i] != null)
                {
                    User u = userArray[i];
                    if (u.delete == 1)
                    {
                        //connect
                        MySqlConnection con = new MySqlConnection(CONSTRING);
                        con.Open();
                        string cmd = "delete from myapptable where id=" + u.ID;
                        userArray[u.ID] = null;
                    }
                    else
                    {
                        if (u.updateResults())
                        {
                            if (u.check() && u.UsePushNotifications == 1)
                            {
                                //  u.sendPush(); TESTZWECK:
                                s[i] = "sende push an den " + i + ". User. newCount= " + u.newCount + " newId: " + u.newScoutId + " oldCount= " + u.oldCount + " oldId: " + u.oldScoutId;

                                System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
                                worker.DoWork += delegate
                                {
                                    ServiceClient client = new ServiceClient();

                                    string Title = "Neue Wohnung";
                                    string Message = "TestNachricht";

                                    client.SendToast(Title, Message, u.PushNotificationURI);
                                    client.Close();      

                                };
                                worker.RunWorkerAsync();

                                Console.WriteLine("Push gesendet");


                            }
                            else
                            {
                                //testzweck:
                                s[i] = "sende KEIN push an den " + i + ". User. newCount= " + u.newCount + " newId: " + u.newScoutId + " oldCount= " + u.oldCount + " oldId: " + u.oldScoutId;
                            }

                        }
                        else if (u.UsePushNotifications != 1)
                        {
                            s[i] = "usepushnotiflag ist auf 0 -> kein push erwünscht";
                        }
                        else
                        {
                            //TODO
                            s[i] = "updateResults ging schief";
                        }

                    }
                }
            }

            return s;
        }

    }
}
