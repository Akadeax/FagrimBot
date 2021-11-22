using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FagrimBot.Music
{
    [FirestoreData]
    public class MusicTrack
    {
        [FirestoreProperty]
        public string Title { get; set; }
        [FirestoreProperty]
        public string Url { get; set; }
        [FirestoreProperty]
        public List<string> Tags { get; set; }

        public MusicTrack(string title, string url, List<string> tags)
        {
            Title = title;
            Url = url;
            Tags = tags;
        }

        public MusicTrack() 
        {
            Title = "";
            Url = "";
            Tags = new List<string>();
        }
    }
}
