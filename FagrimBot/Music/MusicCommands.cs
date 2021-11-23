using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FagrimBot.Core.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;

namespace FagrimBot.Music
{
    [Group("music")]
    [Alias("m")]
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        private async Task<bool> IsValidPlayCommand(string input)
        {
            if (Context.User is not SocketGuildUser user)
            {
                await ReplyAsync("You cannot do that here.");
                return false;
            }

            if (!AudioManager.HasPlayerInVC(Context.Guild))
            {
                var joinResult = await MusicPlayer.Join(Context.Guild, user);
                if (!joinResult.success)
                {
                    await ReplyAsync(joinResult.message);
                    return false;
                }
            }

            LavaPlayer player = AudioManager.GetPlayer(Context.Guild);

            // check if bot isn't in same channel as sender
            if (
                user.VoiceChannel == null
                || player.VoiceChannel == null
                || user.VoiceChannel.Id != player.VoiceChannel.Id)
            {
                await ReplyAsync("You're not connected to the same Voice Channel as me.");
            }

            return true;
        }


        [Command("playtags")]
        public async Task PlayTagsCommand([Remainder] string input)
        {
            if (!await IsValidPlayCommand(input)) return;

            List<string> tags = input.Split(' ').ToList();
            var res = await MusicPlayer.PlayTags(Context, tags);
            await ReplyAsync(res.message);
        }

        [Command("play")]
        public async Task PlayCommand([Remainder] string input)
        {
            if (!await IsValidPlayCommand(input)) return;

            // determine whether tags or link
            bool isLink = Uri.IsWellFormedUriString(input, UriKind.Absolute);
            if(isLink)
            {
                var res = await MusicPlayer.PlayLink(Context.Guild, input);
                await ReplyAsync(res.message);
            }
            else
            {
                LavaNode lavaNode = ServiceManager.GetService<LavaNode>();
                SearchResponse search = await lavaNode.SearchYouTubeAsync(input);
                if(search.Tracks == null || search.Tracks.Count == 0) 
                {
                    await ReplyAsync($"Nothing could be found for your query '{input}'.");
                    return;
                }

                var res = await MusicPlayer.PlayLink(Context.Guild, search.Tracks.First().Url);
                await ReplyAsync(res.message);
            }
        }

        [Command("add")]
        public async Task AddCommand(string url, [Remainder] string tagString)
        {
            List<string> tags = tagString.Split(' ').ToList();
            if(tags.Count == 0)
            {
                await ReplyAsync("Invalid Syntax! No valid tags have been given.");
                return;
            }

            if(!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                await ReplyAsync("Invalid Syntax! The given input isn't a link.");
                return;
            }

            LavaTrack? track = await AudioHelper.Search(url);
            if(track == null)
            {
                await ReplyAsync("Invalid Syntax! The given link is invalid.");
                return;
            }


            await ReplyAsync("working on it...");
            bool songExists = await MusicDBManager.AddToMusic(new MusicTrack(track.Title, url, tags));
            if(songExists)
            {
                await ReplyAsync($"The URL '{track.Url}' already exists in the music library.");
            }
            else
            {
                await ReplyAsync($"Successfully Added {track.Title} ({string.Join(", ", tags)}) to the music library!");
            }
        }

    

        [Command("leave")]
        public async Task LeaveCommand()
        {
            if (Context.User is not SocketGuildUser user)
            {
                await ReplyAsync("You cannot do that here.");
                return;
            }

            // check if user is in same VC as player+
            LavaPlayer player = AudioManager.GetPlayer(Context.Guild);
            if(user.VoiceChannel != null && player.VoiceChannel.Id != user.VoiceChannel.Id)
            {
                await ReplyAsync("You have to be in the same VC as me.");
                return;
            }

            MusicPlayerResult res = await MusicPlayer.Leave(Context.Guild, user);
            await ReplyAsync(res.message);
        }

        [Command("skip")]
        public async Task Skip()
        {
            if (Context.User is not SocketGuildUser user)
            {
                await ReplyAsync("You cannot do that here.");
                return;
            }

            MusicPlayerResult res = await MusicPlayer.Skip(Context.Guild, user);
            await ReplyAsync(res.message);
        }
    }
}
