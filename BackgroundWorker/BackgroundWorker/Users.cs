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
        const string CONSTRING = "server=8a1732a4-5501-4f63-ae2f-a213015171ab.mysql.sequelizer.com;database=db8a1732a455014f63ae2fa213015171ab;uid=tvzwzpkdytttjbtm;pwd=LZDsGqmDUXQBiRJrfALZZCZmiWgwBZgi4JYdu4pBNzgg467pjBP4z4Ki8rjSjWc4";
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

                    SByte changed = (SByte)dr[3];

               //     Console.WriteLine(
                //    dr.Table.Columns["changed"].DataType);

                    if (changed == 1 || init)
                    {

                        // frägt daten ab
                        int id = (int)dr["ID"];
                        string pushURI = (string)dr["PushNotificationUri"];
                        SByte delete = (SByte)dr["deleted"];
                        SByte usePush = (SByte)dr["UsePushNotifications"];
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

            catch (IndexOutOfRangeException)
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
                        if (u.UsePushNotifications == 1 && u.updateResults())
                        {
                            if (u.check())
                            {
                                // TESTZWECK:
                                s[i] = "push an " + i + ". newId: " + u.newScoutId + " oldId: " + u.oldScoutId;


                                // Toast wird gesendet
                                if (u.sendPush())
                                    Console.WriteLine("Push gesendet");


                            }
                            else
                            {
                                //testzweck:
                                s[i] = "KEIN push an " + i + ". newId: " + u.newScoutId + " oldId: " + u.oldScoutId;
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
