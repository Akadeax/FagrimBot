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
            CollectionReference coll = Database.Collection("music");
            DocumentReference docRef = coll.Document(track.Url.GetHashCode().ToString());
            DocumentSnapshot docSnapshot = await docRef.GetSnapshotAsync();

            if (docSnapshot.Exists)
            {
                Console.WriteLine("Song already found");
                return true;
            }

            await docRef.SetAsync(track);
            return false;
        }
    }
}
