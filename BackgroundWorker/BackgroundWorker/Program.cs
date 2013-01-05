using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace BackgroundWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            long i = 0;
            Users myUsers = new Users();
            bool exit = false;

            myUsers.updateUserArray(true);

            while (!exit)
            {
                Console.WriteLine("---------------------------- Neuer Durchlauf --------------------------------");

                myUsers.updateUserArray(false);

                //SERVER
                myUsers.checkUsers();
                //LOKAL DEBUGGING TEST 
                //string[] sArray = myUsers.checkUsers();
                //foreach (string s in sArray)
                //{
                //    if (s != null)
                //    {
                //        Console.WriteLine(s);
                //    }
                //}


                Thread.Sleep(10000);

            }
        }
    }
}
