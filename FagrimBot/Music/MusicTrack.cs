using Google.Cloud.Firestore;
using System.Diagnostics.CodeAnalysis;

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

        public TrackSetting Setting { get; set; }


        public MusicTrack(string title, string url, TrackSetting setting)
        {
            Title = title;
            Url = url;
            Setting = setting;
        }

        public MusicTrack()
        {
            Title = "";
            Url = "";
            Setting = new TrackSetting();
        }
    }

    [FirestoreData]
    public struct TrackSetting
    {
        [FirestoreProperty]
        public TrackSituation Situation { get; set; }

        [FirestoreProperty]
        public TrackLocation Location { get; set; }

        [FirestoreProperty]
        public TrackMood Mood { get; set; }

        [FirestoreProperty]
        public List<string> Tags { get; set; }

        #region overrides
        public static bool operator ==(TrackSetting left, TrackSetting right)
        {
            Console.WriteLine(left.ToString());
            Console.WriteLine(right.ToString());
            return left.Situation == right.Situation
            && left.Location == right.Location
            && left.Mood == right.Mood;
        }

        public static bool operator !=(TrackSetting left, TrackSetting right)
        {
            return left.Situation != right.Situation
            || left.Location != right.Location
            || left.Mood != right.Mood;
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Situation} {Location} {Mood}";
        }
        #endregion
    }

    public enum TrackSituation
    {
        combat, ambience
    }

    public enum TrackLocation
    {
        tavern, city, wilderness
    }

    public enum TrackMood
    {
        casual, dark, epic
    }
}
