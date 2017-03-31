﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Enforcer5.Helpers;
using Enforcer5.Models;
using Telegram.Bot.Helpers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Enforcer5
{
    public static class OnMessage
    {
        public static async Task AntiFlood(Update update)
        {
            try
            {
                var time = (DateTime.UtcNow - update.Message.Date);
                if (time.TotalSeconds > 5) return;
                var chatId = update.Message.Chat.Id;
                var flood = Redis.db.HashGetAsync($"chat:{chatId}:settings", "Flood").Result;
                if (flood.Equals("yes")) return;
                var watch = Redis.db.SetContainsAsync($"chat:{chatId}:watch", update.Message.From.Id).Result;
                if (watch) return;
                var msgType = Methods.GetContentType(update.Message);               
                var lang = Methods.GetGroupLanguage(update.Message).Doc;
                var ignored = isIgnored(chatId, msgType);
                if (!ignored)
                {
                    var msgs = Redis.db.StringGetAsync($"spam:{chatId}:{update.Message.From.Id}").Result;
                    int num = msgs.HasValue ? int.Parse(msgs.ToString()) : 0;   
                    if (num == 0) num = 1;
                    var maxSpam = 8;
                    if (update.Message.Chat.Type == ChatType.Private) maxSpam = 12;
                    var floodSettings = Redis.db.HashGetAllAsync($"chat:{chatId}:flood").Result;
                    var maxMsgs = floodSettings.Where(e => e.Name.Equals("MaxFlood")).FirstOrDefault();
                    var maxTime = TimeSpan.FromSeconds(5);
                    int maxmsgs;
                    Redis.db.StringSetAsync($"spam:{chatId}:{update.Message.From.Id}", num + 1, maxTime);
                    if (int.TryParse(maxMsgs.Value, out maxmsgs))
                    {
                       // Bot.Send($"{num} of {maxmsgs}", update);
                        if (num == (int.Parse(maxMsgs.Value) + 1))
                        {
                            var action = floodSettings.Where(e => e.Name.Equals("ActionFlood")).FirstOrDefault();
                            var name = update.Message.From.FirstName;
                            if (update.Message.From.Username != null) name = $"{name} (@{update.Message.From.Username})";
                            try
                            {
                                if (action.Value.Equals("ban"))
                                {
                                    await Methods.BanUser(chatId, update.Message.From.Id, lang);
                                    Methods.SaveBan(update.Message.From.Id, "flood");
                                    Methods.AddBanList(chatId, update.Message.From.Id, update.Message.From.FirstName,
                                        Methods.GetLocaleString(lang, "bannedForFlood", ".."));
                                    await Bot.Send(Methods.GetLocaleString(lang, "bannedForFlood", name), update);
                                }
                                else
                                {
                                    await Methods.KickUser(chatId, update.Message.From.Id, lang);
                                    Methods.SaveBan(update.Message.From.Id, "flood");
                                    await Bot.Send(
                                        Methods.GetLocaleString(lang, "kickedForFlood", $"{name}, {update.Message.From.Id}"),
                                        update);
                                }
                            }
                            catch (Exception e)
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                await Bot.Send($"{e.Message}\n\n{e.StackTrace}", -1001076212715);
            }
        }

        public static async Task CheckMedia(Update update)
        {
            try
            {                
                var chatId = update.Message.Chat.Id;
                var watch = Redis.db.SetContainsAsync($"chat:{chatId}:watch", update.Message.From.Id).Result;
                if (watch) return;
                var media = Methods.GetContentType(update.Message);
                var status = Redis.db.HashGetAsync($"chat:{chatId}:media", media).Result;
                XDocument lang;
                try
                {
                    lang = Methods.GetGroupLanguage(update.Message).Doc;
                }
                catch (NullReferenceException e)
                {
                    try
                    {
                        lang = Methods.GetGroupLanguage(-1001076212715).Doc;
                    }
                    catch (NullReferenceException exception)
                    {
                        Console.WriteLine(exception);
                        return;
                    }
                }
                var allowed = status.Equals("allowed");
                if (allowed == false && status.HasValue)
                {
                    var name = $"{update.Message.From.FirstName} [{update.Message.From.Id}]";
                    if (update.Message.From.Username != null)
                        name = $"{name} (@{update.Message.From.Username})";
                    var max = Redis.db.HashGetAsync($"chat:{chatId}:Warnsettings", "mediamax").Result.HasValue
                        ? Redis.db.HashGetAsync($"chat:{chatId}:Warnsettings", "mediamax").Result
                        : 2;
                    var current = Redis.db.HashIncrementAsync($"chat:{chatId}:mediawarn", update.Message.From.Id, 1).Result;
                    if (current >= int.Parse(max))
                    {
                        if (status.Equals("ban"))
                        {
                            await Methods.BanUser(chatId, update.Message.From.Id, lang);
                            Methods.SaveBan(update.Message.From.Id, "media");
                            Methods.AddBanList(chatId, update.Message.From.Id, update.Message.From.FirstName,
                                Methods.GetLocaleString(lang, "bannedformedia", ""));
                            await Bot.SendReply(Methods.GetLocaleString(lang, "bannedformedia", name), update);
                        }
                        else
                        {
                            await Methods.KickUser(chatId, update.Message.From.Id, lang);
                            Methods.SaveBan(update.Message.From.Id, "media");
                            await Bot.SendReply(
                                Methods.GetLocaleString(lang, "kickedformedia", $"{name}"),
                                update);
                        }
                    }
                    else
                    {
                        await Bot.SendReply(Methods.GetLocaleString(lang, "mediaNotAllowed", current, max),
                            update);
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Bot.Send($"{e.Message}\n\n{e.StackTrace}", -1001076212715);
            }
        }
        public static async Task RightToLeft(Update update)
        {
            try
            {
                var msgType = Methods.GetMediaType(update.Message);
                var chatId = update.Message.Chat.Id;
                var watch = Redis.db.SetContainsAsync($"chat:{chatId}:watch", update.Message.From.Id).Result;
                if (watch) return;
                XDocument lang;
                try
                {
                    lang = Methods.GetGroupLanguage(update.Message).Doc;
                }
                catch (NullReferenceException e)
                {
                    try
                    {
                        lang = Methods.GetGroupLanguage(-1001076212715).Doc;
                    }
                    catch (NullReferenceException exception)
                    {
                        Console.WriteLine(exception);
                        return;
                    }
                }
                var rtlStatus = Redis.db.HashGetAsync($"chat:{chatId}:char", "Rtl").Result;
                var status = rtlStatus.HasValue ? rtlStatus.ToString() : "allowed";
                if (status.Equals("ban") || status.Equals("kick"))
                {
                    var name = update.Message.From.FirstName;
                    var rtl = "‮";
                    var lastName = "x";
                    if (update.Message.From.Username != null) name = $"{name} (@{update.Message.From.Username})";
                    if (update.Message.From.LastName != null) lastName = update.Message.From.LastName;
                    var text = update.Message.Text;
                    bool check = text.Contains(rtl) || name.Contains(rtl) || lastName.Contains(rtl);
                    try
                    {
                        if (check)
                        {
                            if (status.Equals("kick"))
                            {
                                await Methods.KickUser(chatId, update.Message.From.Id, lang);
                                Methods.SaveBan(update.Message.From.Id, "rtl");
                                await Bot.Send(
                                    Methods.GetLocaleString(lang, "kickedForRtl", $"{name}, {update.Message.From.Id}"),
                                    update);
                            }
                            else
                            {
                                await Methods.BanUser(chatId, update.Message.From.Id, lang);
                                Methods.SaveBan(update.Message.From.Id, "rtl");
                                Methods.AddBanList(chatId, update.Message.From.Id, update.Message.From.FirstName,
                                    Methods.GetLocaleString(lang, "bannedForRtl", ""));
                                await Bot.Send(
                                        Methods.GetLocaleString(lang, "bannedForRtl", $"{name}, {update.Message.From.Id}"),
                                        update);
                            }                            
                        }
                    }
                    catch (Exception e)
                    {
                        await Bot.Send($"{e.Message}\n\n{e.StackTrace}", -1001076212715);
                    }

                }               
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await Bot.Send($"{e.Message}\n\n{e.StackTrace}", -1001076212715);
            }
        }

        public static async Task ArabDetection (Update update)
        {
            var chatId = update.Message.Chat.Id;
            var watch = Redis.db.SetContainsAsync($"chat:{chatId}:watch", update.Message.From.Id).Result;
            if (watch) return;
            var arabStatus = Redis.db.HashGetAsync($"chat:{chatId}:char", "Arab").Result.ToString();
            if (string.IsNullOrEmpty(arabStatus)) arabStatus = "allowed";
            if (!arabStatus.Equals("allowed"))
            {
                var arabicChars = "[ساینبتسیکبدثصکبثحصخبدوزطئظضچج]";
                var text = $"{update.Message.Text} {update.Message.From.FirstName} {update.Message.From.LastName} {update.Message.ForwardFrom?.FirstName} {update.Message.ForwardFrom?.LastName} {update.Message.From.Username} {update.Message.ForwardFrom?.Username}";
                var found = false;
                for (int i = 0; i < text.Length; i++)
                {
                   
                        //var letter = char.ConvertToUtf32(text[i], text[i + 1]);
                        found = Regex.IsMatch(text[i].ToString(), arabicChars);
                        if (found)
                        {
                            break;
                        }
                                        
                }

                if (found)
                {                   
                    var lang = Methods.GetGroupLanguage(update.Message).Doc;
                    var name = update.Message.From.FirstName;
                    var lastName = "x";
                    if (update.Message.From.Username != null) name = $"{name} (@{update.Message.From.Username})";
                    if (update.Message.From.LastName != null) lastName = update.Message.From.LastName;
                    try
                    {
                        if (arabStatus.Equals("kick"))
                        {
                            await Methods.KickUser(chatId, update.Message.From.Id, lang);
                            Methods.SaveBan(update.Message.From.Id, "arab");
                            await Bot.Send(
                                Methods.GetLocaleString(lang, "kickedForNoEnglishScript", $"{name}, {update.Message.From.Id}"),
                                update);
                        }
                        else
                        {
                            await Methods.BanUser(chatId, update.Message.From.Id, lang);
                            Methods.SaveBan(update.Message.From.Id, "arab");
                            Methods.AddBanList(chatId, update.Message.From.Id, update.Message.From.FirstName,
                                Methods.GetLocaleString(lang, "bannedForNoEnglishScript", "."));
                            await Bot.Send(
                                Methods.GetLocaleString(lang, "bannedForNoEnglishScript", $"{name}, {update.Message.From.Id}"),
                                update);
                        }
                        
                    }
                    catch (Exception e)
                    {

                    }                    

                }
            }
        }

        public static bool isIgnored(long chatId, string msgType)
        {
            var status = Redis.db.HashGetAsync($"chat:{chatId}:floodexceptions", msgType).Result;
            if (status.HasValue)
            {
                return status.Equals("no");
            }
            else
            {
                return true;
            }
        }
    }
}
