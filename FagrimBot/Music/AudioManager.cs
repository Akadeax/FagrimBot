using Discord;
using Discord.WebSocket;
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
        private static readonly DiscordSocketClient client = ServiceManager.GetService<DiscordSocketClient>();
        private static readonly LavaNode lavaNode = ServiceManager.GetService<LavaNode>();

        public static void InitAudio()
        {
            lavaNode.OnTrackEnded += OnTrackEnded;
            client.UserVoiceStateUpdated += OnBotVoiceUpdate;
        }

        private static async Task OnBotVoiceUpdate(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            // if bot left channel (for whatever reason)
            if (
                user.Id == client.CurrentUser.Id && after.VoiceChannel == null
                && user is SocketGuildUser guildUser)
            {
                LavaPlayer? player = GetPlayer(guildUser.Guild);
                if (player == null) return;
                await lavaNode.LeaveAsync(before.VoiceChannel);
            }
        }

        public static LavaPlayer GetPlayer(IGuild guild)
        {
            try
            {
                return lavaNode.GetPlayer(guild);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while fetching Player for {guild.Name}: {ex.Message}");
            }
        }

        public static bool HasPlayerInVC(IGuild guild)
        {
            if (!lavaNode.HasPlayer(guild)) return false;
            return GetPlayer(guild).VoiceChannel != null;
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
                if (player.TextChannel == null) return;

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

        public static async Task PlayOrQueue(this LavaPlayer player, LavaTrack track)
        {
            if(player.PlayerState == PlayerState.Playing)
            {
                player.Queue.Enqueue(track);
            }
            else
            {
                await player.PlayAsync(track);
            }
        }
    }
}
