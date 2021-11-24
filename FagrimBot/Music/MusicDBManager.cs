using FagrimBot.Core;
using FagrimBot.Core.Managers;
using Google.Cloud.Firestore;
using System.Linq;
using Victoria;

namespace FagrimBot.Music
{
    public static class MusicDBManager
    {
        private static FirestoreDb Database { get => DatabaseManager.Database; }

        public static async Task<List<MusicTrack>?> GetAllMusic()
        {
            Query musicWithTagsQuery = Database.Collection("music");

            QuerySnapshot? querySnapshot = await musicWithTagsQuery.GetSnapshotAsync();
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

        // return all tracks that contain all tags
        public static async Task<List<MusicTrack>?> FetchMusicWithTags(List<string> tags)
        {
            List<MusicTrack>? tracks = await GetAllMusic();
            if (tracks == null) return null;

            return tracks.Where(x => x.Tags.ContainsAll(tags)).ToList();
        }

        // returns exists already
        public static async Task<bool> AddToMusic(MusicTrack track)
        {
            CollectionReference music = Database.Collection("music");

            // check if track already exists
            MusicTrack? existingTrack = await GetDBTrackByUrl(track.Url);
            if (existingTrack != null)
            {
                Console.WriteLine("Track already exists");
                return false;
            }

            DocumentReference docRef = music.Document();
            await docRef.SetAsync(track);
            return false;
        }

        public static async Task<MusicTrack?> GetDBTrackByUrl(String Url)
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
