using FagrimBot.Core;
using FagrimBot.Core.Managers;
using Google.Cloud.Firestore;
using System.Linq;
using Victoria;

namespace FagrimBot.Music
{
    public static class MusicDBManager
    {
        public static FirestoreDb Database { get => DatabaseManager.Database; }

        public static async Task<List<MusicTrack>?> FetchAll()
        {
            Query musicQuery = Database.Collection("music");

            QuerySnapshot? querySnapshot = await musicQuery.GetSnapshotAsync();
            if (querySnapshot == null)
            {
                Console.WriteLine("Error while trying to fetch music.");
                return null;
            }

            List<MusicTrack> tracks = new();
            foreach (DocumentSnapshot docSnap in querySnapshot)
            {
                tracks.Add(docSnap.ConvertTo<MusicTrack>());
            }

            return tracks;
        }

        public static async Task<List<MusicTrack>?> FetchWithSetting(TrackSetting setting)
        {
            List<MusicTrack>? trackList = await FetchAll();
            if (trackList == null) return null;

            List<MusicTrack> newTracks = new();
            foreach(MusicTrack track in trackList)
            {
                if (track.Setting != setting) continue;
                // if tags are given, check whether all of them are fulfilled
                if (setting.Tags.Count != 0 && !track.Setting.Tags.ContainsAll(setting.Tags)) continue;

                newTracks.Add(track);
            }
            return newTracks;
        }

        // returns exists already
        public static async Task<bool> Add(MusicTrack track)
        {
            CollectionReference music = Database.Collection("music");

            // check if track already exists
            MusicTrack? existingTrack = await GetByUrl(track.Url);
            if (existingTrack != null)
            {
                Console.WriteLine("Track already exists");
                return false;
            }

            DocumentReference docRef = music.Document();
            await docRef.SetAsync(track);
            return true;
        }

        public static async Task<MusicTrack?> GetByUrl(string Url)
        {
            CollectionReference music = Database.Collection("music");
            QuerySnapshot matchingUrlDocs = await music.WhereEqualTo("Url", Url).GetSnapshotAsync();
            if (matchingUrlDocs.Documents.Count == 0)
            {
                return null;
            }

            return matchingUrlDocs[0].ConvertTo<MusicTrack>();
        }
    }
}
