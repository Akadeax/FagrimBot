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
        LavaNode lavaNode = ServiceManager.GetService<LavaNode>();

        [Command("play")]
        public async Task PlayCommand([Remainder] string input)
        {
            LavaPlayer? player = AudioManager.GetPlayer(Context.Guild);
            if(player == null)
            {
                await ReplyAsync("I am not connected to your voice channel!");
                return;
            }

            bool isLink = Uri.IsWellFormedUriString(input, UriKind.Absolute);
            if(isLink)
            {
                LavaTrack? track = await AudioHelper.Search(input);
                if (track == null)
                {
                    await ReplyAsync($"Couldn't find anything for '{input}'.");
                    return;
                }

                await player.PlayAsync(track);
                await ReplyAsync($"Added '{track.Title}' to the queue.");
            }
            else
            {
                List<string> tags = input.Split(' ').ToList();
                if(tags.Count == 0 || tags == null)
                {
                    await ReplyAsync("Your input is invalid.");
                    return;
                }

                List<MusicTrack>? tracks = await MusicDBManager.FetchMusicWithTags(tags);
                if(tracks == null || tracks.Count == 0)
                {
                    await ReplyAsync($"No Results could be found for '{input}'.");
                    return;
                }

                await player.StopAsync();
                player.Queue.Clear();

                foreach(MusicTrack mt in tracks)
                {
                    LavaTrack? lt = await AudioHelper.Search(mt.Url);
                    if (lt == null) continue;
                    if (player.PlayerState != PlayerState.Playing)
                    {
                        Console.WriteLine("PLAY");
                        await player.PlayAsync(lt);
                    }
                    else
                    {
                        Console.WriteLine("ENQUEUE");
                        player.Queue.Enqueue(lt);
                    }
                }


                Console.WriteLine(player.Queue.Count);
                await ReplyAsync($"Successfully enqueued {tracks.Count} Tracks with the tags '{input}'.");
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

        [Command("join")]
        public async Task JoinCommand()
        {
            if (lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Already connected to a VC.");
                return;
            }
            if (Context.User is not SocketGuildUser user) return;
            if(user.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel.");
                return;
            }

            try
            {
                await lavaNode.JoinAsync(user.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {user.VoiceChannel.Name}.");
            }
            catch (Exception ex)
            {
                await ReplyAsync($"Error: {ex.Message}");
            }
        }
    }
}
