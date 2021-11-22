using Discord;
using FagrimBot.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace FagrimBot.Music
{
    public static class AudioManager
    {
        private static readonly LavaNode lavaNode = ServiceManager.GetService<LavaNode>();

        static AudioManager()
        {
            lavaNode.OnTrackEnded += OnTrackEnded;
        }

        public static LavaPlayer? GetPlayer(IGuild guild)
        {
            try
            {
                return lavaNode.GetPlayer(guild);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while fetching Player for {guild.Name}: {ex.Message}");
                return null;
            }
        }

        private static async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Queue completed.");
                return;
            }

            if (queueable is not LavaTrack track)
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync(
                $"{args.Reason}: {args.Track.Title}\nNow playing: {track.Title}");
        }
    }

    public static class AudioHelper
    {
        private static readonly LavaNode lavaNode = ServiceManager.GetService<LavaNode>();

        public static async Task<LavaTrack?> Search(string query)
        {
            SearchResponse search = Uri.IsWellFormedUriString(query, UriKind.Absolute)
                        ? await lavaNode.SearchAsync(SearchType.Direct, query)
                        : await lavaNode.SearchYouTubeAsync(query);

            return search.Tracks.FirstOrDefault();
        }

        
    }
}
