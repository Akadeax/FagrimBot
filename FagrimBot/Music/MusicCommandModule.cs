using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FagrimBot.Core.Managers;
using System.Text;
using Victoria;
using Victoria.Responses.Search;

namespace FagrimBot.Music
{
    [Group("music")]
    [Alias("m")]
    public class MusicCommandModule : ModuleBase<SocketCommandContext>
    {
        #region Play
        private async Task<bool> IsValidPlayCommand()
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



        [Command("playsetting")]
        public async Task PlaySetting(
            string situation,
            string location,
            string mood,
            [Remainder] string tagString = "")
        {
            if (!await IsValidPlayCommand()) return;

            List<string>? tags = new();
            if(!string.IsNullOrEmpty(tagString))
            {
                tags = tagString.ToLower().Split(' ').ToList();
            }


            if (
                !Enum.TryParse(situation.ToLower(), out TrackSituation trackSituation)
                || !Enum.TryParse(location.ToLower(), out TrackLocation trackLocation)
                || !Enum.TryParse(mood.ToLower(), out TrackMood trackMood))
            {
                await ReplyAsync("Invalid format!");
                return;
            }

            TrackSetting setting = new()
            {
                Situation = trackSituation,
                Location = trackLocation,
                Mood = trackMood,
                Tags = tags,
            };

            var res = await MusicPlayer.PlaySetting(Context, setting);
            await ReplyAsync(res.message);
        }

        [Command("play")]
        public async Task PlayCommand([Remainder] string input)
        {
            if (!await IsValidPlayCommand()) return;

            // determine whether tags or link
            bool isLink = Uri.IsWellFormedUriString(input, UriKind.Absolute);
            if (isLink)
            {
                var res = await MusicPlayer.PlayLink(Context.Guild, input);
                await ReplyAsync(res.message);
            }
            else
            {
                LavaNode lavaNode = ServiceManager.GetService<LavaNode>();
                SearchResponse search = await lavaNode.SearchYouTubeAsync(input);
                if (search.Tracks == null || search.Tracks.Count == 0)
                {
                    await ReplyAsync($"Nothing could be found for your query '{input}'.");
                    return;
                }

                var res = await MusicPlayer.PlayLink(Context.Guild, search.Tracks.First().Url);
                await ReplyAsync(res.message);
            }
        }
        #endregion


        [Command("add")]
        public async Task AddCommand(
            string url,
            string situation,
            string location,
            string mood,
            [Remainder] string tagString = null)
        {
            List<string>? tags = new();
            if (!string.IsNullOrEmpty(tagString))
            {
                tags = tagString.ToLower().Split(' ').ToList();
            }


            if (
                !Enum.TryParse(situation.ToLower(), out TrackSituation trackSituation)
                || !Enum.TryParse(location.ToLower(), out TrackLocation trackLocation)
                || !Enum.TryParse(mood.ToLower(), out TrackMood trackMood))
            {
                await ReplyAsync("Invalid format!");
                return;
            }

            TrackSetting setting = new()
            {
                Situation = trackSituation,
                Location = trackLocation,
                Mood = trackMood,
                Tags = tags,
            };

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                await ReplyAsync("Invalid Syntax! The given input isn't a link.");
                return;
            }

            LavaTrack? track = await AudioHelper.Search(url);
            if (track == null)
            {
                await ReplyAsync("Invalid Syntax! The given link is invalid.");
                return;
            }


            await ReplyAsync("working on it...");
            bool success = await MusicDBManager.Add(new MusicTrack(track.Title, track.Url, setting));
            if (!success)
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
            if (user.VoiceChannel != null && player.VoiceChannel.Id != user.VoiceChannel.Id)
            {
                await ReplyAsync("You have to be in the same VC as me.");
                return;
            }

            MusicPlayerResult res = await MusicPlayer.Leave(Context.Guild, user);
            await ReplyAsync(res.message);
        }

        [Command("skip")]
        public async Task SkipCommand()
        {
            if (Context.User is not SocketGuildUser user)
            {
                await ReplyAsync("You cannot do that here.");
                return;
            }

            MusicPlayerResult res = await MusicPlayer.Skip(Context.Guild, user);
            await ReplyAsync(res.message);
        }

        [Command("stop")]
        public async Task StopCommand()
        {
            if (Context.User is not SocketGuildUser user)
            {
                await ReplyAsync("You cannot do that here.");
                return;
            }

            MusicPlayerResult res = await MusicPlayer.Stop(Context.Guild, user);
            await ReplyAsync(res.message);
        }
    }
}
