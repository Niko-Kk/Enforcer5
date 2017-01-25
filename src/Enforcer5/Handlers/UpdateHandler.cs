﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Enforcer5.Helpers;
using Enforcer5.Models;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
namespace Enforcer5.Handlers
{

    internal static class UpdateHandler
    {
        public static void UpdateReceived(object sender, UpdateEventArgs e)
        {
            new Task(() => { HandleUpdate(e.Update); }).Start();
        }

        //        private static void AddCount(int id, string command)
        //        {
        //            try
        //            {
        //                if (!UserMessages.ContainsKey(id))
        //                    UserMessages.Add(id, new SpamDetector { Messages = new HashSet<UserMessage>() });
        //                UserMessages[id].Messages.Add(new UserMessage(command));
        //            }
        //            catch
        //            {
        //                // ignored
        //            }
        //        }

        //        internal static void BanMonitor()
        //        {
        //            while (true)
        //            {
        //                try
        //                {
        //                    //first load up the ban list
        //                    using (var db = new WWContext())
        //                    {
        //                        foreach (var id in SpamBanList)
        //                        {
        //                            var p = db.Players.FirstOrDefault(x => x.TelegramId == id);
        //                            var name = p?.Name;
        //                            var count = p?.TempBanCount ?? 0;
        //                            count++;
        //                            if (p != null)
        //                                p.TempBanCount = count; //update the count

        //                            var expireTime = DateTime.Now;
        //                            switch (count)
        //                            {
        //                                case 1:
        //                                    expireTime = expireTime.AddHours(12);
        //                                    break;
        //                                case 2:
        //                                    expireTime = expireTime.AddDays(1);
        //                                    break;
        //                                case 3:
        //                                    expireTime = expireTime.AddDays(3);
        //                                    break;
        //                                default: //perm ban
        //                                    expireTime = (DateTime)SqlDateTime.MaxValue;
        //                                    break;

        //                            }
        //                            db.GlobalBans.Add(new GlobalBan
        //                            {
        //                                BannedBy = "Moderator",
        //                                Expires = expireTime,
        //                                TelegramId = id,
        //                                Reason = "Spam / Flood",
        //                                BanDate = DateTime.Now,
        //                                Name = name
        //                            });
        //                        }
        //                        SpamBanList.Clear();
        //                        db.SaveChanges();

        //                        //now refresh the list
        //                        var list = db.GlobalBans.ToList();
        //#if RELEASE2
        //                        for (var i = list.Count - 1; i >= 0; i--)
        //                        {
        //                            if (list[i].Expires > DateTime.Now) continue;
        //                            db.GlobalBans.Remove(db.GlobalBans.Find(list[i].Id));
        //                            list.RemoveAt(i);
        //                        }
        //                        db.SaveChanges();
        //#endif

        //                        BanList = list;
        //                    }
        //                }
        //                catch
        //                {
        //                    // ignored
        //                }

        //                //refresh every 20 minutes
        //                Thread.Sleep(TimeSpan.FromMinutes(1));
        //            }
        //        }

        //        internal static void SpamDetection()
        //        {
        //            while (true)
        //            {
        //                try
        //                {
        //                    var temp = UserMessages.ToDictionary(entry => entry.Key, entry => entry.Value);
        //                    //clone the dictionary
        //                    foreach (var key in temp.Keys.ToList())
        //                    {
        //                        try
        //                        {
        //                            //drop older messages (1 minute)
        //                            temp[key].Messages.RemoveWhere(x => x.Time < DateTime.Now.AddMinutes(-1));

        //                            //comment this out - if we remove it, it doesn't keep the warns
        //                            //if (temp[key].Messages.Count == 0)
        //                            //{
        //                            //    temp.Remove(key);
        //                            //    continue;
        //                            //}
        //                            //now count, notify if limit hit
        //                            if (temp[key].Messages.Count() >= 20) // 20 in a minute
        //                            {
        //                                temp[key].Warns++;
        //                                if (temp[key].Warns < 2 && temp[key].Messages.Count < 40)
        //                                {
        //                                    Send($"Please do not spam me. Next time is automated ban.", key);
        //                                    //Send($"User {key} has been warned for spamming: {temp[key].Warns}\n{temp[key].Messages.GroupBy(x => x.Command).Aggregate("", (a, b) => a + "\n" + b.Count() + " " + b.Key)}",
        //                                    //    Para);
        //                                    continue;
        //                                }
        //                                if ((temp[key].Warns >= 3 || temp[key].Messages.Count >= 40) & !temp[key].NotifiedAdmin)
        //                                {
        //                                    //Send(
        //                                    //    $"User {key} has been banned for spamming: {temp[key].Warns}\n{temp[key].Messages.GroupBy(x => x.Command).Aggregate("", (a, b) => a + "\n" + b.Count() + " " + b.Key)}",
        //                                    //    Para);
        //                                    temp[key].NotifiedAdmin = true;
        //                                    //ban
        //                                    SpamBanList.Add(key);
        //                                    var count = 0;
        //                                    using (var db = new WWContext())
        //                                    {
        //                                        count = db.Players.FirstOrDefault(x => x.TelegramId == key).TempBanCount ?? 0;
        //                                    }
        //                                    var unban = "";
        //                                    switch (count)
        //                                    {
        //                                        case 0:
        //                                            unban = "12 hours";
        //                                            break;
        //                                        case 1:
        //                                            unban = "24 hours";
        //                                            break;
        //                                        case 2:
        //                                            unban = "3 days";
        //                                            break;
        //                                        default:
        //                                            unban =
        //                                                "Permanent. You have reached the max limit of temp bans for spamming.";
        //                                            break;
        //                                    }
        //                                    Send("You have been banned for spamming.  Your ban period is: " + unban,
        //                                        key);
        //                                }

        //                                temp[key].Messages.Clear();
        //                            }
        //                        }
        //                        catch (Exception e)
        //                        {
        //                            //Console.WriteLine(e.Message);
        //                        }
        //                    }
        //                    UserMessages = temp;
        //                }
        //                catch (Exception e)
        //                {
        //                    //Console.WriteLine(e.Message);
        //                }
        //                Thread.Sleep(2000);
        //            }
        //        }


        private static void Log(Models.Commands command, Update update)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"[{System.DateTime.Now.Date}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(command.Method.GetMethodInfo().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($" {update.Message.From.FirstName} -> [{update.Message.Chat.Title} {update.Message.Chat.Id}]");
        }

        internal static void HandleUpdate(Update update)
        {
            {
                CollectStats(update.Message);
                Bot.MessagesProcessed++;               
                //ignore previous messages

                //if (update.Message?.Chat.Type != ChatType.Private && update.Message?.Chat.Id != -1001077134233)
                //    Bot.Api.LeaveChatAsync(update.Message.Chat.Id);
                if ((update.Message?.Date ?? DateTime.MinValue) < Bot.StartTime.AddSeconds(-10))
                    return; //toss it

                var id = update.Message.Chat.Id;
                
                //Settings.Main.LogText += update?.Message?.Text + Environment.NewLine;   
#if DEBUG           
                try
                { 
#endif
                
                    switch (update.Message.Type)
                    {
                        case MessageType.UnknownMessage:
                            break;
                        case MessageType.TextMessage:

                            if (update.Message.Text.StartsWith("/"))
                            {
                                var args = GetParameters(update.Message.Text);
                                args[0] = args[0].Replace("@" + Bot.Me.Username, "");
                                //check for the command
                                Console.WriteLine("Looking for command");
                                var command = Bot.Commands.FirstOrDefault(
                                        x =>
                                            String.Equals(x.Trigger, args[0],
                                                StringComparison.CurrentCultureIgnoreCase));
                                if (command != null)
                                {
                                    Log(command, update);
                                    
                                    //check that we should run the command
                                    if (command.DevOnly & !Constants.Devs.Contains(update.Message.From.Id))
                                    {
                                        return;
                                    }
                                    if (command.GroupAdminOnly & !Methods.IsGroupAdmin(update) & !Methods.IsGlobalAdmin(update.Message.From.Id))
                                    {
                                        Bot.SendReply(Methods.GetLocaleString(Methods.GetGroupLanguage(update.Message).Doc, "userNotAdmin"), update.Message);
                                        return;
                                    }
                                    if (command.InGroupOnly & update.Message.Chat.Type == ChatType.Private)
                                    {
                                        return;
                                    }
                                    if (command.RequiresReply & update.Message.ReplyToMessage == null)
                                    {
                                        var lang = Methods.GetGroupLanguage(update.Message);
                                        Bot.Send(Methods.GetLocaleString(lang.Doc, "noReply"), update.Message.Chat.Id);
                                        return;
                                    }
                                    Bot.CommandsReceived++;                                      
                                    command.Method.Invoke(update, args);
                                }
                            }
                            break;
                        case MessageType.PhotoMessage:
                            break;
                        case MessageType.AudioMessage:
                            break;
                        case MessageType.VideoMessage:
                            break;
                        case MessageType.VoiceMessage:
                            break;
                        case MessageType.DocumentMessage:
                            break;
                        case MessageType.StickerMessage:
                            break;
                        case MessageType.LocationMessage:
                            break;
                        case MessageType.ContactMessage:
                            break;
                        case MessageType.ServiceMessage:
                            if (update.Message.NewChatMember != null)
                            {
                                if (update.Message.NewChatMember.Id == Bot.Me.Id)
                                {
                                    Service.BotAdded(update.Message);
                                }
                                else
                                {
                                    Service.Welcome(update.Message);
                                }
                            }                          
                            break;
                        case MessageType.VenueMessage:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
#if DEBUG
                }

                catch (Exception ex)
                {
                    Send(ex.Message, id);
                }
#endif
                }
        }

        private static void CollectStats(Message updateMessage)
        {
            Console.WriteLine("Collecting Stats");
            Redis.db.HashIncrement("bot:general", "messages");
            if (updateMessage?.From?.Username != null)
            {
                Redis.db.HashSet("bot:usernames", $"@{updateMessage.From.Username.ToLower()}", updateMessage.From.Id);
                Redis.db.HashSet($"bot:usernames:{updateMessage.Chat.Id}", $"@{updateMessage.From.Username.ToLower()}", updateMessage.From.Id);
            }
            if (updateMessage?.ForwardFrom?.Username != null)
            {
                Redis.db.HashSet("bot:usernames", $"@{updateMessage.ForwardFrom.Username.ToLower()}", updateMessage.ForwardFrom.Id);
                Redis.db.HashSet($"bot:usernames:{updateMessage.Chat.Id}", $"@{updateMessage.ForwardFrom.Username.ToLower()}", updateMessage.ForwardFrom.Id);
            }
            if (updateMessage?.Chat.Type != ChatType.Private)
            {
                if (updateMessage?.From != null)
                {
                    Redis.db.HashIncrement($"chat:{updateMessage.From.Id}", "msgs");
                    Redis.db.HashSet($"chat:{updateMessage.Chat.Id}:userlast", updateMessage.From.Id, System.DateTime.Now.Ticks);
                    Redis.db.StringSet($"chat:{updateMessage.Chat.Id}:chatlast", DateTime.Now.Ticks);
                }                
            }

        }


        public static void CallbackReceived(object sender, CallbackQueryEventArgs e)
        {
            new Task(() => { HandleCallback(e.CallbackQuery); }).Start();
        }


        internal static void HandleCallback(CallbackQuery query)
        {
            //Bot.MessagesProcessed++;
            ////Bot.CommandsReceived++;
            //using (var DB = new WWContext())
            //{
            //    try
            //    {
            //        if (String.IsNullOrEmpty(query.Data))
            //        {
            //            //empty request, from Telegram bot monitoring most likely
            //            Bot.ReplyToCallback(query, "Invalid Callback");
            //            return;
            //        }
            //        string[] args = query.Data.Split('|');
            //        if (args[0] == "update")
            //        {
            //            bool dontUpdate = args[1] == "no";
            //            if (UpdateHelper.Devs.Contains(query.From.Id))
            //            {
            //                if (dontUpdate)
            //                {
            //                    Bot.ReplyToCallback(query, query.Message.Text + "\n\nOkay, I won't do anything D: *sadface*");
            //                    return;
            //                }
            //                //start the update process
            //                Updater.DoUpdate(query);
            //            }
            //            else
            //            {
            //                Bot.ReplyToCallback(query, "You aren't Para! Go Away!!", false, true);
            //            }
            //            return;
            //        }
            //        if (args[0] == "build")
            //        {
            //            bool dontUpdate = args[1] == "no";
            //            if (UpdateHelper.Devs.Contains(query.From.Id))
            //            {
            //                if (dontUpdate)
            //                {
            //                    Bot.ReplyToCallback(query, query.Message.Text + "\n\nOkay, I won't do anything D: *sadface*");
            //                    return;
            //                }
            //                //start the update process
            //                Updater.DoBuild(query);
            //            }
            //            else
            //            {
            //                Bot.ReplyToCallback(query, "You aren't Para! Go Away!!", false, true);
            //            }
            //            return;
            //        }
            //        if (args[0] == "ohai")
            //        {
            //            bool dontUpdate = args[1] == "no";
            //            Bot.ReplyToCallback(query, "Processing...");
            //            if (UpdateHelper.Devs.Contains(query.From.Id))
            //            {
            //                if (dontUpdate)
            //                {
            //                    Bot.Edit(query, query.Message.Text + "\n\nOkay, I won't do anything D: *sadface*");
            //                    return;
            //                }
            //                //update ohaider achievement
            //                var userid = int.Parse(args[2]);


            //                try
            //                {
            //                    var para = DB.Players.FirstOrDefault(x => x.Id == userid);

            //                    //get all the players Para has played with
            //                    var players = (from g in DB.Games
            //                                   join gp in DB.GamePlayers on g.Id equals gp.GameId
            //                                   join gp2 in DB.GamePlayers on g.Id equals gp2.GameId
            //                                   join pl in DB.Players on gp2.PlayerId equals pl.Id
            //                                   where gp.PlayerId == para.Id
            //                                   select pl).Distinct();

            //                    //figure out which players don't have the achievement

            //                    //update the message
            //                    var ohaimsg = $"Found {players.Count()} players that have earned OHAIDER.";
            //                    Bot.Edit(query, ohaimsg);
            //                    var count = 0;
            //                    foreach (var player in players)
            //                    {
            //                        //add the achievement
            //                        if (player.Achievements == null)
            //                            player.Achievements = 0;
            //                        var ach = (Achievements)player.Achievements;
            //                        if (ach.HasFlag(Achievements.OHAIDER)) continue;
            //                        count++;
            //                        var a = Achievements.OHAIDER;
            //                        player.Achievements = (long)(ach | a);
            //                        //log these ids, just in case....
            //                        using (var sw = new StreamWriter(Path.Combine(Bot.RootDirectory, "..\\Logs\\ohaider.log"), true))
            //                        {
            //                            sw.WriteLine(player.Id);
            //                        }
            //                        Send($"Achievement Unlocked!\n{a.GetName().ToBold()}\n{a.GetDescription()}", player.TelegramId);
            //                        Thread.Sleep(200);
            //                    }
            //                    DB.SaveChanges();
            //                    ohaimsg += $"\nAchievement added to {count} players\nFinished";
            //                    Bot.Edit(query, ohaimsg);
            //                }
            //                catch (AggregateException e)
            //                {
            //                    Send(e.InnerExceptions.First().Message, query.From.Id);
            //                }
            //                catch (Exception e)
            //                {
            //                    while (e.InnerException != null)
            //                        e = e.InnerException;
            //                    Send(e.Message, query.From.Id);
            //                }

            //            }
            //            else
            //            {
            //                Bot.ReplyToCallback(query, "You aren't Para! Go Away!!", false, true);
            //            }
            //            return;
            //        }
            //        if (args[0] == "restore")
            //        {
            //            if (UpdateHelper.Devs.Contains(query.From.Id))
            //            {
            //                Bot.ReplyToCallback(query, "Processing...");
            //                if (args[1] == "no")
            //                {
            //                    Bot.Edit(query, "Okay, I won't do anything D: *sadface*");
            //                    return;
            //                }
            //                var oldid = int.Parse(args[1]);
            //                var newid = int.Parse(args[2]);
            //                var result = DB.RestoreAccount(oldid, newid);
            //                var oldname = DB.Players.FirstOrDefault(x => x.TelegramId == oldid)?.Name;
            //                var newname = DB.Players.FirstOrDefault(x => x.TelegramId == newid)?.Name;
            //                Bot.Edit(query, $"Restored stats from {oldname} to {newname}");
            //            }
            //            else
            //            {
            //                Bot.ReplyToCallback(query, "You aren't Para! Go Away!!", false, true);
            //            }
            //        }
            //        if (args[0] == "movelang")
            //        {
            //            if (UpdateHelper.Devs.Contains(query.From.Id))
            //            {
            //                Bot.ReplyToCallback(query, "Processing...");
            //                if (args[1] == "no")
            //                {
            //                    Bot.Edit(query, "Okay, I won't do anything D: *sadface*");
            //                    return;
            //                }

            //                var oldfilename = args[2];
            //                var newfilename = args[3];
            //                int grpcount = 0, plcount = 0;
                            
            //                var groups = (from g in DB.Groups where g.Language == oldfilename select g).ToList();
            //                var players = (from pl in DB.Players where pl.Language == oldfilename select pl).ToList();

            //                foreach (var g in groups)
            //                {
            //                    g.Language = newfilename;
            //                    grpcount++;
            //                }
            //                foreach (var pl in players)
            //                {
            //                    pl.Language = newfilename;
            //                    plcount++;
            //                }
            //                DB.SaveChanges();
            //                var msg = $"Groups changed: {grpcount}\nPlayers changed: {plcount}\n<b>Total rows changed: {grpcount + plcount}</b>";
            //                Bot.Edit(query, msg);

            //                try
            //                {
            //                    System.IO.File.Delete(Path.Combine(Bot.LanguageDirectory, oldfilename + ".xml"));
            //                    msg += $"\n\nSuccessfully deleted {oldfilename}.xml";
            //                }
            //                catch (Exception e)
            //                {
            //                    msg += $"\n\n*Error: *";
            //                    msg += e.Message;
            //                }
            //                Bot.Edit(query, msg);
            //            }
            //            else
            //            {
            //                Bot.ReplyToCallback(query, "You aren't Para! Go Away!!", false, true);
            //            }
            //        }
            //        InlineKeyboardMarkup menu;
            //        Group grp;
            //        Player p = DB.Players.FirstOrDefault(x => x.TelegramId == query.From.Id);
            //        List<InlineKeyboardButton> buttons = new List<InlineKeyboardButton>();
            //        long groupid = 0;
            //        if (args[0] == "vote")
            //        {
            //            var node = Bot.Nodes.FirstOrDefault(x => x.ClientId.ToString() == args[1]);
            //            node?.SendReply(query);
            //            return;
            //        }

            //        groupid = long.Parse(args[1]);

            //        grp = DB.Groups.FirstOrDefault(x => x.GroupId == groupid);
            //        if (grp == null && args[0] != "getlang" && args[0] != "validate" && args[0] != "lang" && args[0] != "setlang" && args[0] != "groups" && args[0] != "upload" && args[0] != "status")
            //            return;
            //        if (grp == null)
            //        {
            //            if (p == null && args[0] != "lang" && args[0] != "setlang" && args[0] != "groups") //why am i doing this????  TODO: update later to array contains...
            //                return;
            //        }

            //        var language = GetLanguage(p?.TelegramId ?? grp.GroupId);
            //        var command = args[0];
            //        var choice = "";
            //        if (args.Length > 2)
            //            choice = args[2];
            //        if (choice == "cancel")
            //        {
            //            Bot.ReplyToCallback(query, GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //            return;
            //        }
            //        if (!nonCommandsList.Contains(command.ToLower()))
            //            if (!UpdateHelper.IsGroupAdmin(query.From.Id, groupid) & !UpdateHelper.Devs.Contains(query.From.Id) && !UpdateHelper.IsGlobalAdmin(query.From.Id))
            //            {
            //                Bot.ReplyToCallback(query, GetLocaleString("GroupAdminOnly", language), false);
            //                return;
            //            }
            //        var Yes = GetLocaleString("Yes", language);
            //        var No = GetLocaleString("No", language);
            //        var Cancel = GetLocaleString("Cancel", language);
            //        switch (command)
            //        {
            //            case "status":
            //                if (args[3] == "null")
            //                {
            //                    //get status
            //                    menu = new InlineKeyboardMarkup(new[] { "Normal", "Overloaded", "Recovering", "API Bug", "Offline", "Maintenance" }.Select(x => new[] { new InlineKeyboardButton(x, $"status|{groupid}|{choice}|{x}") }).ToArray());
            //                    Bot.ReplyToCallback(query, "Set status to?", replyMarkup: menu);
            //                }
            //                else
            //                {
            //                    //update the status
            //                    var bot = DB.BotStatus.FirstOrDefault(x => x.BotName == choice);
            //                    if (bot != null)
            //                    {
            //                        bot.BotStatus = args[3];
            //                        DB.SaveChanges();
            //                    }
            //                    Bot.ReplyToCallback(query, "Status updated");
            //                }
            //                break;
            //            case "groups":
            //                var groups = PublicGroups.ForLanguage(choice).ToList().OrderByDescending(x => x.MemberCount).Take(10).ToList(); //top 10 groups, otherwise these lists will get LONG
            //                Bot.ReplyToCallback(query, GetLocaleString("HereIsList", language, choice));
            //                if (groups.Count() > 5)
            //                {
            //                    //need to split it
            //                    var reply = groups.Take(5).Aggregate("",
            //                        (current, g) =>
            //                            current +
            //                            $"{(g.MemberCount?.ToString() ?? "Unknown")} {GetLocaleString("Members", language)}\n<a href=\"{g.GroupLink}\">{g.Name}</a>\n\n");
            //                    Send(reply, query.Message.Chat.Id);
            //                    Thread.Sleep(500);
            //                    reply = groups.Skip(5).Aggregate("",
            //                        (current, g) =>
            //                            current +
            //                            $"{(g.MemberCount?.ToString() ?? "Unknown")} {GetLocaleString("Members", language)}\n<a href=\"{g.GroupLink}\">{g.Name}</a>\n\n");
            //                    Send(reply, query.Message.Chat.Id);
            //                }
            //                else
            //                {
            //                    var reply = groups.Aggregate("",
            //                        (current, g) =>
            //                            current +
            //                            $"{(g.MemberCount?.ToString() ?? "Unknown")} {GetLocaleString("Members", language)}\n<a href=\"{g.GroupLink}\">{g.Name}</a>\n\n");
            //                    Send(reply, query.Message.Chat.Id);
            //                }
            //                break;
            //            case "validate":
            //                //choice = args[1];
            //                if (choice == "All")
            //                {
            //                    LanguageHelper.ValidateFiles(query.Message.Chat.Id, query.Message.MessageId);
            //                    return;
            //                }

            //                if (args[4] != "base" && args[3] == "All")
            //                {
            //                    LanguageHelper.ValidateFiles(query.Message.Chat.Id, query.Message.MessageId, choice);
            //                    return;
            //                }

            //                menu = new InlineKeyboardMarkup();
            //                var vlang = SelectLanguage(command, args, ref menu);
            //                if (vlang == null)
            //                {
            //                    buttons.Clear();
            //                    Bot.ReplyToCallback(query, GetLocaleString("WhatVariant", language, choice),
            //                           replyMarkup: menu);
            //                    return;
            //                }

            //                //var menu = new ReplyKeyboardHide { HideKeyboard = true, Selective = true };
            //                //Bot.SendTextMessage(id, "", replyToMessageId: update.Message.MessageId, replyMarkup: menu);
            //                LanguageHelper.ValidateLanguageFile(query.Message.Chat.Id, vlang.FilePath, query.Message.MessageId);
            //                return;
            //            case "getlang":
            //                if (choice == "All")
            //                {
            //                    Bot.ReplyToCallback(query, "One moment...");
            //                    LanguageHelper.SendAllFiles(query.Message.Chat.Id);
            //                    return;
            //                }

            //                if (args[4] != "base" && args[3] == "All")
            //                {
            //                    Bot.ReplyToCallback(query, "One moment...");
            //                    LanguageHelper.SendBase(choice, query.Message.Chat.Id);
            //                    return;
            //                }
                            
            //                menu = new InlineKeyboardMarkup();
            //                var glang = SelectLanguage(command, args, ref menu);
            //                if (glang == null)
            //                {
            //                    buttons.Clear();
            //                    Bot.ReplyToCallback(query, GetLocaleString("WhatVariant", language, choice),
            //                           replyMarkup: menu);
            //                    return;
            //                }
            //                Bot.ReplyToCallback(query, "One moment...");
            //                LanguageHelper.SendFile(query.Message.Chat.Id, glang.Name);
            //                break;
            //            case "upload":
            //                Console.WriteLine(choice);
            //                if (choice == "current")
            //                {
            //                    Bot.ReplyToCallback(query, "No action taken.");
            //                    return;
            //                }
            //                Helpers.LanguageHelper.UseNewLanguageFile(choice, query.Message.Chat.Id, query.Message.MessageId);
            //                return;

            //            case "vote":
            //                //send it back to the game;
            //                var node = Bot.Nodes.FirstOrDefault(x => x.ClientId.ToString() == args[1]);
            //                node?.SendReply(query);
            //                break;
            //            case "lang":
            //                //load up each file and get the names
            //                var langs = Directory.GetFiles(Bot.LanguageDirectory).Select(x => new LangFile(x)).ToList();

            //                buttons.Clear();
            //                buttons.AddRange(langs.Select(x => x.Base).Distinct().OrderBy(x => x).Select(x => new InlineKeyboardButton(x, $"setlang|{groupid}|{x}|null|base")));

            //                var baseMenu = new List<InlineKeyboardButton[]>();
            //                for (var i = 0; i < buttons.Count; i++)
            //                {
            //                    if (buttons.Count - 1 == i)
            //                    {
            //                        baseMenu.Add(new[] { buttons[i] });
            //                    }
            //                    else
            //                        baseMenu.Add(new[] { buttons[i], buttons[i + 1] });
            //                    i++;
            //                }

            //                menu = new InlineKeyboardMarkup(baseMenu.ToArray());


            //                var curLang = langs.First(x => x.FileName == (grp?.Language ?? p.Language));
            //                Bot.ReplyToCallback(query, GetLocaleString("WhatLang", language, curLang.Base), replyMarkup: menu);
            //                break;
            //            case "setlang":
            //                menu = new InlineKeyboardMarkup();
            //                var slang = SelectLanguage(command, args, ref menu, false);
            //                if (slang == null)
            //                {
            //                    buttons.Clear();
            //                    var curLangfilePath = Directory.GetFiles(Bot.LanguageDirectory).First(x => Path.GetFileNameWithoutExtension(x) == (grp?.Language ?? p.Language));
            //                    var curVariant = new LangFile(curLangfilePath).Variant;
            //                    Bot.ReplyToCallback(query, GetLocaleString("WhatVariant", language, curVariant),
            //                        replyMarkup: menu);
            //                    return;
            //                }

            //                if (
            //                    Directory.GetFiles(Bot.LanguageDirectory)
            //                        .Any(
            //                            x =>
            //                                String.Equals(Path.GetFileNameWithoutExtension(x), slang.FileName,
            //                                    StringComparison.InvariantCultureIgnoreCase)))
            //                {
            //                    //now get the group
            //                    if (grp != null)
            //                    {
            //                        grp.Language = slang.FileName;
            //                        //check for any games running
            //                        var ig = GetGroupNodeAndGame(groupid);

            //                        ig?.LoadLanguage(slang.FileName);
            //                        menu = GetConfigMenu(groupid);
            //                        Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("LangSet", language, slang.Base + (String.IsNullOrWhiteSpace(slang.Variant) ? "" : ": " + slang.Variant)));
            //                        Bot.ReplyToCallback(query, GetLocaleString("WhatToDo", language), replyMarkup: menu);
            //                    }
            //                    else if (p != null)
            //                    {
            //                        p.Language = slang.FileName;
            //                        Bot.ReplyToCallback(query, GetLocaleString("LangSet", language, slang.Base + (String.IsNullOrWhiteSpace(slang.Variant) ? "" : ": " + slang.Variant)));
            //                    }
            //                }
            //                DB.SaveChanges();
            //                break;
            //            //case "online":
            //            //    buttons.Add(new InlineKeyboardButton("Yes", $"setonline|{groupid}|show"));
            //            //    buttons.Add(new InlineKeyboardButton("No", $"setonline|{groupid}|hide"));
            //            //    buttons.Add(new InlineKeyboardButton("Cancel", $"setonline|{groupid}|cancel"));
            //            //    menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //            //    Edit(query.Message.Chat.Id, query.Message.MessageId,
            //            //        $"Do you want your group to be notified when the bot is online?\nCurrent: {grp.DisableNotification != false}",
            //            //        replyMarkup: menu);
            //            //    break;
            //            //case "setonline":

            //            //    grp.DisableNotification = (choice == "hide");
            //            //    Bot.Api.AnswerCallbackQuery(query.Id,
            //            //        $"Notification will {(grp.DisableNotification == true ? "not " : "")}be shown on startup");
            //            //    Edit(query.Message.Chat.Id, query.Message.MessageId,
            //            //        GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //            //    DB.SaveChanges();
            //            //    break;
            //            case "flee":
            //                buttons.Add(new InlineKeyboardButton(Yes, $"setflee|{groupid}|enable"));
            //                buttons.Add(new InlineKeyboardButton(No, $"setflee|{groupid}|disable"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setflee|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("AllowFleeQ", language, grp.DisableFlee == false ? GetLocaleString("Allow", language) : GetLocaleString("Disallow", language)),
            //                    replyMarkup: menu);
            //                break;
            //            case "setflee":

            //                grp.DisableFlee = (choice == "disable");
            //                Bot.Api.AnswerCallbackQuery(query.Id,
            //                       GetLocaleString("AllowFleeA", language, grp.DisableFlee == true ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "maxplayer":
            //                buttons.Add(new InlineKeyboardButton("10", $"setmaxplayer|{groupid}|10"));
            //                buttons.Add(new InlineKeyboardButton("15", $"setmaxplayer|{groupid}|15"));
            //                buttons.Add(new InlineKeyboardButton("20", $"setmaxplayer|{groupid}|20"));
            //                buttons.Add(new InlineKeyboardButton("25", $"setmaxplayer|{groupid}|25"));
            //                buttons.Add(new InlineKeyboardButton("30", $"setmaxplayer|{groupid}|30"));
            //                buttons.Add(new InlineKeyboardButton("35", $"setmaxplayer|{groupid}|35"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setmaxplayer|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("MaxPlayersQ", language, grp.MaxPlayers ?? Settings.MaxPlayers),
            //                    replyMarkup: menu);
            //                break;
            //            case "setmaxplayer":

            //                grp.MaxPlayers = int.Parse(choice);
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("MaxPlayersA", language, choice));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "roles":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Show", language), $"setroles|{groupid}|show"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Hide", language), $"setroles|{groupid}|hide"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setroles|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("ShowRolesDeathQ", language, (grp.ShowRoles == false ? "Hidden" : "Shown")),
            //                    replyMarkup: menu);
            //                break;
            //            case "setroles":

            //                grp.ShowRoles = (choice == "show");
            //                Bot.Api.AnswerCallbackQuery(query.Id,
            //                    GetLocaleString("ShowRolesDeathA", language, grp.ShowRoles == false ? "hidden" : "shown"));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "mode":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("NormalOnly", language), $"setmode|{groupid}|Normal"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("ChaosOnly", language), $"setmode|{groupid}|Chaos"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("PlayerChoice", language), $"setmode|{groupid}|Player"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setmode|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("GameModeQ", language, grp.Mode), replyMarkup: menu);
            //                break;
            //            case "setmode":

            //                grp.Mode = choice;
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("GameModeA", language, choice));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "endroles":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("ShowNone", language), $"setendroles|{groupid}|None"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("ShowLiving", language), $"setendroles|{groupid}|Living"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("ShowAll", language), $"setendroles|{groupid}|All"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setendroles|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("ShowRolesEndQ", language, grp.ShowRolesEnd),
            //                    replyMarkup: menu);
            //                break;
            //            case "setendroles":
            //                grp.ShowRolesEnd = choice;
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("ShowRolesEndA", language, choice));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "day":
            //                buttons.Add(new InlineKeyboardButton("90", $"setday|{groupid}|30"));
            //                buttons.Add(new InlineKeyboardButton("120", $"setday|{groupid}|60"));
            //                buttons.Add(new InlineKeyboardButton("150", $"setday|{groupid}|90"));
            //                buttons.Add(new InlineKeyboardButton("180", $"setday|{groupid}|120"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setday|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("SetDayTimeQ", language, Settings.TimeDay + 60, (grp.DayTime ?? Settings.TimeDay) + 60),
            //                    replyMarkup: menu);
            //                break;
            //            case "setday":
            //                grp.DayTime = int.Parse(choice);
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("SetDayTimeA", language, choice + 60));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "night":
            //                buttons.Add(new InlineKeyboardButton("30", $"setnight|{groupid}|30"));
            //                buttons.Add(new InlineKeyboardButton("60", $"setnight|{groupid}|60"));
            //                buttons.Add(new InlineKeyboardButton("90", $"setnight|{groupid}|90"));
            //                buttons.Add(new InlineKeyboardButton("120", $"setnight|{groupid}|120"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setnight|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("SetNightTimeQ", language, Settings.TimeNight, grp.NightTime ?? Settings.TimeNight),
            //                    replyMarkup: menu);
            //                break;
            //            case "setnight":

            //                grp.NightTime = int.Parse(choice);
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("SetNightTimeA", language, choice));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "lynch":
            //                buttons.Add(new InlineKeyboardButton("30", $"setlynch|{groupid}|30"));
            //                buttons.Add(new InlineKeyboardButton("60", $"setlynch|{groupid}|60"));
            //                buttons.Add(new InlineKeyboardButton("90", $"setlynch|{groupid}|90"));
            //                buttons.Add(new InlineKeyboardButton("120", $"setlynch|{groupid}|120"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setlynch|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("SetLynchTimeQ", language, Settings.TimeLynch, grp.LynchTime ?? Settings.TimeLynch),
            //                    replyMarkup: menu);
            //                break;
            //            case "setlynch":
            //                grp.LynchTime = int.Parse(choice);
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("SetLynchTimeA", language, choice));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "fool":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Allow", language), $"setfool|{groupid}|true"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Disallow", language), $"setfool|{groupid}|false"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setfool|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("AllowFoolQ", language, grp.AllowFool == false ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)), replyMarkup: menu);
            //                break;
            //            case "setfool":

            //                grp.AllowFool = (choice == "true");
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("AllowFoolA", language, grp.AllowFool == false ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "tanner":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Allow", language), $"settanner|{groupid}|true"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Disallow", language), $"settanner|{groupid}|false"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"settanner|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("AllowTannerQ", language, grp.AllowTanner == false ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)), replyMarkup: menu);
            //                break;
            //            case "settanner":

            //                grp.AllowTanner = (choice == "true");
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("AllowTannerA", language, grp.AllowTanner == false ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "cult":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Allow", language), $"setcult|{groupid}|true"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Disallow", language), $"setcult|{groupid}|false"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setcult|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("AllowCultQ", language, grp.AllowCult == false ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)), replyMarkup: menu);
            //                break;
            //            case "setcult":
            //                grp.AllowCult = (choice == "true");
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("AllowCultA", language, grp.AllowCult == false ? GetLocaleString("Disallow", language) : GetLocaleString("Allow", language)));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "extend":
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Allow", language), $"setextend|{groupid}|true"));
            //                buttons.Add(new InlineKeyboardButton(GetLocaleString("Disallow", language), $"setextend|{groupid}|false"));
            //                buttons.Add(new InlineKeyboardButton(Cancel, $"setextend|{groupid}|cancel"));
            //                menu = new InlineKeyboardMarkup(buttons.Select(x => new[] { x }).ToArray());
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("AllowExtendQ", language, grp.AllowExtend == true ? GetLocaleString("Allow", language) : GetLocaleString("Disallow", language)), replyMarkup: menu);
            //                break;
            //            case "setextend":
            //                grp.AllowExtend = (choice == "true");
            //                Bot.Api.AnswerCallbackQuery(query.Id, GetLocaleString("AllowExtendA", language, grp.AllowExtend == true ? GetLocaleString("Allow", language) : GetLocaleString("Disallow", language)));
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("WhatToDo", language), replyMarkup: GetConfigMenu(groupid));
            //                DB.SaveChanges();
            //                break;
            //            case "done":
            //                Bot.ReplyToCallback(query,
            //                    GetLocaleString("ThankYou", language));
            //                break;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Bot.ReplyToCallback(query, ex.Message, false, true);
            //    }
            //}
        }


        private static string[] GetParameters(string input)
        {
            return input.Contains(" ") ? new[] { input.Substring(1, input.IndexOf(" ")).Trim(), input.Substring(input.IndexOf(" ") + 1) } : new[] { input.Substring(1).Trim(), null };
        }

        internal static Task<Message> Send(string message, long id, bool clearKeyboard = false,
            InlineKeyboardMarkup customMenu = null, ParseMode parseMode = ParseMode.Html)
        {
            return Bot.Send(message, id, clearKeyboard, customMenu, parseMode);
        }

        public static void InlineQueryReceived(object sender, InlineQueryEventArgs e)
        {
            new Task(() => { HandleInlineQuery(e.InlineQuery); }).Start();
        }
        
        internal static void HandleInlineQuery(InlineQuery q)
        {

            //var commands = new InlineCommand[]
            //{
            //    new StatsInlineCommand(q.From),
            //};
            
            //List<InlineCommand> choices;
            //if (String.IsNullOrWhiteSpace(q.Query))
            //{
            //    //show all commands available
            //    choices = commands.ToList();
            //}
            //else
            //{
            //    //let's figure out what they wanted
            //    var com = q.Query;
            //    choices = commands.Where(command => command.Command.StartsWith(com) || Commands.ComputeLevenshtein(com, command.Command) < 3).ToList();
            //}

            //Bot.Api.AnswerInlineQuery(q.Id, choices.Select(c => new InlineQueryResultArticle()
            //{
            //    Description = c.Description,
            //    Id = c.Command,
            //    Title = c.Command,
            //    InputMessageContent = new InputTextMessageContent
            //    {
            //        DisableWebPagePreview = true,
            //        MessageText = c.Content,
            //        ParseMode = ParseMode.Html
            //    }
            //}).Cast<InlineQueryResult>().ToArray(), 0, true);
        }
    }
}
