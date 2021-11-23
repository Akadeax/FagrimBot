using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FagrimBot.Core;
using FagrimBot.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace FagrimBot.Music
{
    public static class MusicPlayer
    {
        public static async Task<MusicPlayerResult> PlayTags(SocketCommandContext context, List<string> tags)
        {
            LavaPlayer? lavaPlayer = AudioManager.GetPlayer(context.Guild);
            if(lavaPlayer == null)
            {
                return Res(false, "I'm not connected to a voice channel.");
            }

            if(tags == null || tags.Count == 0)
            {
                return Res(false, "That isn't valid syntax.");
            }

            // get tracks from DB
            List<MusicTrack>? tracks = await MusicDBManager.FetchMusicWithTags(tags);
            if(tracks == null || tracks.Count == 0)
            {
                return Res(false, "No Results could be found that match your tags.");
            }

            tracks.Shuffle();

            // reset for next batch of music
            await lavaPlayer.StopAsync();
            lavaPlayer.Queue.Clear();

            // Enqueue all tracks
            foreach(MusicTrack musicTrack in tracks)
            {
                LavaTrack? lavaTrack = await AudioHelper.Search(musicTrack.Url);
                if(lavaTrack == null)
                {
                    Console.WriteLine($"Failed to locate linked track: {musicTrack.Url}");
                    continue;
                }

                await lavaPlayer.PlayOrQueue(lavaTrack);
            }

            return Res(true, $"Successfully Enqueued {tracks.Count} shuffled songs.");
        }

        public static async Task<MusicPlayerResult> PlayLink(IGuild guild, string link)
        {
            LavaPlayer? lavaPlayer = AudioManager.GetPlayer(guild);
            if (lavaPlayer == null)
            {
                return Res(false, "I'm not connected to a voice channel.");
            }

            if (string.IsNullOrWhiteSpace(link))
            {
                return Res(false, "That isn't valid syntax.");
            }

            LavaTrack? lavaTrack = await AudioHelper.Search(link);
            if (lavaTrack == null)
            {
                return Res(false, "Failed to locate linked track.");
            }

            if(lavaPlayer.PlayerState == PlayerState.Playing)
            {
                lavaPlayer.Queue.Enqueue(lavaTrack);
                return Res(true, $"Enqueued '*{lavaTrack.Title}*' (Position {lavaPlayer.Queue.Count})");
            }
            else
            {
                await lavaPlayer.PlayAsync(lavaTrack);
                return Res(true, $"Now Playing: '*{lavaTrack.Title}*'.");
            }
        }



        public static async Task<MusicPlayerResult> Join(IGuild guild, SocketGuildUser user)
        {
            if(AudioManager.HasPlayerInVC(guild))
            {
                return Res(false, "I'm already connected to a VC.");
            }

            if(user.VoiceChannel == null)
            {
                return Res(false, "You must be connected to a VC to do that.");
            }

            try
            {
                LavaNode lavaNode = ServiceManager.GetService<LavaNode>();
                await lavaNode.JoinAsync(user.VoiceChannel);
                return Res(true, $"Joined {user.VoiceChannel.Name}.");
            }
            catch(Exception)
            {
                return Res(false, "Failed to Join.");
            }
        }

        public static async Task<MusicPlayerResult> Leave(IGuild guild, SocketGuildUser user)
        {
            if (!AudioManager.HasPlayerInVC(guild))
            {
                return Res(false, "I'm not connected to a VC at the moment.");
            }

            LavaNode lavaNode = ServiceManager.GetService<LavaNode>();
            await lavaNode.LeaveAsync(user.VoiceChannel);
            return Res(true, "Leaving.");
        }

        public static async Task<MusicPlayerResult> Skip(IGuild guild, SocketGuildUser user)
        {
            if(!AudioManager.HasPlayerInVC(guild))
            {
                return Res(false, "I'm not connected to a VC at the moment.");
            }
            LavaPlayer player = AudioManager.GetPlayer(guild);
            if(
                player.VoiceChannel != null 
                && user.VoiceChannel != null 
                && player.VoiceChannel.Id == user.VoiceChannel.Id)
            {
                await player.SkipAsync();
                return Res(true, "Skipping...");
            }

            return Res(false, "You need to be in the same VC as me for that.");
        }

        private static MusicPlayerResult Res(bool success, string error)
        {
            return new MusicPlayerResult
            {
                success = success,
                message = error,
            };
        }
    }

    public struct MusicPlayerResult
    {
        public bool success;
        public string? message;
    }
}
