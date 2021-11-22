using FagrimBot.Music;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FagrimBot.Core.Managers
{
    public static class DatabaseManager
    {
        private const string CREDENTIALS_FOLDER = "./Resources";
        private const string CREDENTIALS_FILE = "cloudfire.json";
        private const string CREDENTIALS_PATH = $"{CREDENTIALS_FOLDER}/{CREDENTIALS_FILE}";


        private static FirestoreDb? database;
        public static FirestoreDb Database 
        { 
            get => database ?? throw new NullReferenceException("Database is not initialized");
            private set => database = value; 
        }

        public static async Task LoadConnection()
        {
            if (!File.Exists(CREDENTIALS_PATH))
            {
                throw new FileNotFoundException(
                    "Firestore Credentials couldn't be found. Please insert the CloudFire Credentials JSON into Resources folder.",
                    CREDENTIALS_PATH);
            }

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", CREDENTIALS_PATH);

            Database = await FirestoreDb.CreateAsync("fagrimbot");
            Console.WriteLine("Connected to DB.");
        }
    }
}
