﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Enforcer5.Handlers;
using Enforcer5.Helpers;
using Enforcer5.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
#pragma warning disable CS0168
namespace Enforcer5.Helpers
{
    internal static class Bot
    {
        internal static string TelegramAPIKey;
        public static TelegramBotClient Api;
        public static User Me;
        public static DateTime StartTime = DateTime.UtcNow;
        public static bool Running = true;
        public static long CommandsReceived = 0;
        public static long MessagesProcessed = 0;
        public static long MessagesReceived = 0;
        public static long TotalPlayers = 0;
        public static long TotalGames = 0;
        public static Random R = new Random();
        private static System.Threading.Timer _apiWatch;
        public static int MessagesSent = 0;
        public static bool testing = false;
        public static string CurrentStatus = "";
        internal static string RootDirectory
        {
            get
            {
                var path = AppContext.BaseDirectory;;
                return Path.GetDirectoryName(path);
            }
        }
        internal delegate void ChatCommandMethod(Update u, string[] args);

        internal delegate void ChatCallbackMethod(CallbackQuery u, string[] args);
        internal static List<Models.Commands> Commands = new List<Models.Commands>();
        internal static List<Models.CallBacks> CallBacks = new List<Models.CallBacks>();
        internal static string LanguageDirectory => Path.GetFullPath(Path.Combine(RootDirectory, @"..\..\..\Languages"));
        internal static string TempLanguageDirectory => Path.GetFullPath(Path.Combine(RootDirectory, @"..\..\TempLanguageFiles"));
        public static async void Initialize(string updateid = null)
        {

            //get api token from registry
#if normal
            var key =
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        .OpenSubKey("SOFTWARE\\Werewolf");
            TelegramAPIKey = key.GetValue("EnforcerAPI").ToString();
#endif
#if premium
            var key =
                    RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                        .OpenSubKey("SOFTWARE\\Werewolf");
            TelegramAPIKey = key.GetValue("EnforcerPremiumAPI").ToString();
#endif

            Api = new TelegramBotClient(TelegramAPIKey);
            //Api.Timeout = TimeSpan.FromSeconds(3);
            Api.OnInlineQuery += UpdateHandler.InlineQueryReceived;
            Api.OnUpdate += UpdateHandler.UpdateReceived;
            Api.OnCallbackQuery += UpdateHandler.CallbackHandler;
            Api.OnReceiveError += ApiOnReceiveError;
            Api.OnReceiveGeneralError += ApiOnReceiveGenError;
            Me = Api.GetMeAsync().Result;
            Console.Title += " " + Me.Username;
            StartTime = DateTime.UtcNow;
#if premium
                var offset = Redis.db.StringGetAsync("bot:last_Premium_update").Result;
#endif
#if normal
            var offset = Redis.db.StringGetAsync("bot:last_update").Result;
#endif
            if (offset.HasValue)
            {
                Api.MessageOffset = int.Parse(offset) + 1;
                Console.WriteLine($" database offset is {offset}");
            }
            Send($"Bot Started:\n{System.DateTime.UtcNow.AddHours(2):hh:mm:ss dd-MM-yyyy}", Constants.Devs[0]);          
            //load the commands list
            foreach (var m in typeof(Commands).GetMethods())
            {
                var c = new Models.Commands();
                foreach (var a in m.GetCustomAttributes(true))
                {
                    if (a is Attributes.Command)
                    {
                        var ca = a as Attributes.Command;
                        c.Blockable = ca.Blockable;
                        c.DevOnly = ca.DevOnly;
                        c.GlobalAdminOnly = ca.GlobalAdminOnly;
                        c.GroupAdminOnly = ca.GroupAdminOnly;
                        c.Trigger = ca.Trigger;
                        c.Method = (ChatCommandMethod)m.CreateDelegate(typeof(ChatCommandMethod));
                        c.InGroupOnly = ca.InGroupOnly;
                        c.RequiresReply = ca.RequiresReply;
                        c.UploadAdmin = ca.UploadAdmin;
                        Commands.Add(c);
                    }
                }
            }
            //loadCallbackQuries
            foreach (var m in typeof(CallBacks).GetMethods())
            {
                var c = new Models.CallBacks();
                foreach (var a in m.GetCustomAttributes(true))
                {
                    if (a is Attributes.Callback)
                    {
                        var ca = a as Attributes.Callback;
                        c.Blockable = ca.Blockable;
                        c.DevOnly = ca.DevOnly;
                        c.GlobalAdminOnly = ca.GlobalAdminOnly;
                        c.GroupAdminOnly = ca.GroupAdminOnly;
                        c.Trigger = ca.Trigger;
                        c.Method = (ChatCallbackMethod)m.CreateDelegate(typeof(ChatCallbackMethod));
                        c.InGroupOnly = ca.InGroupOnly;
                        c.RequiresReply = ca.RequiresReply;
                        c.UploadAdmin = ca.UploadAdmin;
                        CallBacks.Add(c);
                    }
                }
            }

           
                //now we can start receiving            
            Api.StartReceiving();
            Console.WriteLine($"Starting ID = {Api.MessageOffset}");
            var wait = TimeSpan.FromSeconds(5);
            _apiWatch = new System.Threading.Timer(WatchAPI, null, wait, wait);

        }

        private static void ApiOnUpdate(object sender, UpdateEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        private static void ApiOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        {
            if (!Api.IsReceiving)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--Not getting updates--");
                Api.StartReceiving();
            }
            var e = receiveErrorEventArgs.ApiRequestException;
                Console.WriteLine($"{DateTime.Now} {e.ErrorCode} - {e.Message}\n{e.Source}\n{e.StackTrace}\\n{sender.ToString()} Offset = {Api.MessageOffset}");
            var offset = Api.MessageOffset;
            Api.MessageOffset = offset + 1;

        }

        private static void ApiOnReceiveGenError(object sender, ReceiveGeneralErrorEventArgs receiveErrorEventArgs)
        {
            if (!Api.IsReceiving)
            {
                Api.StartReceiving();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--Not getting updates--");
            }
            var e = receiveErrorEventArgs.Exception;
            Console.WriteLine($"{DateTime.Now} {e.Source} - {e.Message}\n{e.Source}\n{e.StackTrace}\n{sender.ToString()} Offset = {Api.MessageOffset}");
            var offset = Api.MessageOffset;
            Api.MessageOffset = offset + 1;

        }

        internal static void ReplyToCallback(CallbackQuery query, string text = null, bool edit = true, bool showAlert = false, InlineKeyboardMarkup replyMarkup = null)
        {
            //first answer the callback
            Bot.Api.AnswerCallbackQueryAsync(query.Id, edit ? null : text, showAlert);
            //edit the original message
            if (edit)
                Edit(query, text, replyMarkup);
        }

        internal static Task<Message> Edit(CallbackQuery query, string text, InlineKeyboardMarkup replyMarkup = null)
        {
            return Edit(query.Message.Chat.Id, query.Message.MessageId, text, replyMarkup);
        }

        internal static Task<Message> Edit(long id, int msgId, string text, InlineKeyboardMarkup replyMarkup = null)
        {
            Bot.MessagesSent++;
            return Bot.Api.EditMessageTextAsync(id, msgId, text, replyMarkup: replyMarkup);
        }


        //private static void ApiOnReceiveError(object sender, ReceiveErrorEventArgs receiveErrorEventArgs)
        //{
        //    if (!Api.IsReceiving)
        //    {
        //        Api.StartReceiving();
        //    }
        //    var e = receiveErrorEventArgs.ApiRequestException;
        //    using (var sw = System.IO.File.AppendText(Path.Combine(RootDirectory, "..\\Logs\\apireceiveerror.log")))
        //    {
        //        sw.WriteLine($"{DateTime.Now} {e.ErrorCode} - {e.Message}\n{e.Source}");
        //    }
                
        //}

        private static Message CatchSend(string message, long id, InlineKeyboardMarkup customMenu = null,
            ParseMode parsemode = ParseMode.Default, int messageId = -1)
        {
            Message result = null;
            try
            {
                MessagesSent++;
                if ((customMenu == null) && (messageId != -1))
                {
                    result = Bot.Api.SendTextMessageAsync(id, message, ParseMode.Default, replyToMessageId:messageId).Result;
                }
                else if (messageId != -1 && customMenu != null)
                {
                    result = Bot.Api.SendTextMessageAsync(id, message, ParseMode.Default, replyToMessageId: messageId, replyMarkup: customMenu).Result;
                }
                else if (customMenu != null && messageId == -1)
                {
                    result = Bot.Api.SendTextMessageAsync(id, message, ParseMode.Default, replyMarkup: customMenu).Result;
                }                
                else
                {
                    result = Bot.Api.SendTextMessageAsync(id, message, ParseMode.Default).Result;
                }
                
            }
            catch (AggregateException e)
            {
                try
                {
                    result = Bot.Api.SendTextMessageAsync(-125311351, $"{e.Message}\n\n{e.StackTrace}").Result;
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    
                }
            }
            return result;
        }

        internal static Message SendToPm(string message, int pmId, long chatId, InlineKeyboardMarkup menu = null,
            ParseMode parseMode = ParseMode.Html, int messageId = -1)
        {
            Message result = null;
            try
            {
                result = Send(message, pmId, menu, parseMode);
            }
            catch (AggregateException e)
            {
                if (e.Message.Contains("bot can't initiate") || e.Message.Contains("bot was blocked"))
                {
                    var lang = Methods.GetGroupLanguage(chatId).Doc;
                    var startMe = new Menu(1)
                    {
                        Buttons = new List<InlineButton>
                        {
                            new InlineButton(Methods.GetLocaleString(lang, "StartMe"), url:$"https://t.me/{Bot.Me.Username}")
                        }
                    };
                    if (messageId == -1)
                    {
                       Bot.Send(Methods.GetLocaleString(lang, "botNotStarted"), chatId, Key.CreateMarkupFromMenu(startMe));
                        result = null;
                    }
                    else
                    {
                        Bot.SendReply(Methods.GetLocaleString(lang, "botNotStarted"), chatId, messageId, Key.CreateMarkupFromMenu(startMe));
                        result = null;
                    }
                }
                else
                {
                    result = CatchSend(message, chatId, menu, parseMode, messageId);
                }
            }
            return result;
        }

        internal static Message SendToPm(string message, Update update, InlineKeyboardMarkup menu = null,
            ParseMode parseMode = ParseMode.Html)
        {
            return SendToPm(message, Convert.ToInt32((long)update.Message.From.Id), update.Message.Chat.Id, menu, parseMode,
                update.Message.MessageId);
        }

        internal static Message Send(string message, long id,
            InlineKeyboardMarkup customMenu = null, ParseMode parseMode = ParseMode.Html, [CallerMemberName]string parentMethod = "")
        {
            Message result = null;
            try
            {
                MessagesSent++;
                //message = message.Replace("`",@"\`");
                if (customMenu != null)
                {
                    result = Api.SendTextMessageAsync(id, message, replyMarkup: customMenu,
                        disableWebPagePreview: true,
                        parseMode: parseMode).Result;
                }
                else
                {
                    result = Api.SendTextMessageAsync(id, message, disableWebPagePreview: true, parseMode: parseMode).Result;
                }
            }
            catch (AggregateException e)
            {
                if (e.Message.Contains("Unsupported start tag"))
                {
                    result = CatchSend(message, id, parsemode: ParseMode.Default);
                }
                if (e.Message.Contains("message is too long"))
                {
                    //Console.WriteLine($"HANDLED\n{e.ErrorCode}\n\n{e.Message}\n\n{e.StackTrace}");
                    var messages = Regex.Split(message, "(.+?)(?:\r\n|\n)");
                    var amount = messages.Length / 4;
                    for (int j = 0; j < 4; j++)
                    {
                        var word = "";
                        for (int i = 0; i < amount; i++)
                        {
                            word = $"{word}{messages[i]}";
                        }
                        CatchSend(word, id);
                    }

                }

                if (e.Message.Contains("bot can't initiate") ||
                    e.Message.Contains("bot was blocked"))
                {
                    if (parentMethod.Equals("SendToPm"))
                    {
                        throw;
                    }
                    else
                    {
                        Console.WriteLine($"{e.Message}\n\n{e.StackTrace}");
                        result = null;
                    }
                }
                if (e.Message.Contains("bot can't send messages to bots"))
                {
                    //skip
                    result = null;
                }
                else
                {
                    Bot.CatchSend($"{e.Message}", id);
                    result = Bot.CatchSend($"\n\n{e.Message}\n\n{e.StackTrace}", -1001076212715,
                        parsemode: ParseMode.Default);
                }
            }
            return result;
            
        }
        internal static Message Send(string message, Update chatUpdate,
            InlineKeyboardMarkup customMenu = null, ParseMode parseMode = ParseMode.Html, [CallerMemberName]string parentMethod = "")
        {
            var id = chatUpdate.Message.Chat.Id;
            return Send(message, id, customMenu, parseMode, parentMethod);
        }

        internal static Message SendReply(string message, Message msg, [CallerMemberName]string parentMethod = "")
        {
            return SendReply(message, msg.Chat.Id, msg.MessageId, parentMethod: parentMethod);
        }
        internal static Message SendReply(string message, long chatid, int msgid, InlineKeyboardMarkup keyboard = null, [CallerMemberName]string parentMethod = "")
        {
            MessagesSent++;
            Message result = null;
            try
            {
                if (keyboard == null)
                {
                    result =  Api.SendTextMessageAsync(chatid, message, replyToMessageId: msgid, parseMode: ParseMode.Html,
                        disableWebPagePreview: true).Result;
                }
                else
                {
                    result = Api.SendTextMessageAsync(chatid, message, replyToMessageId: msgid, parseMode: ParseMode.Html,
                        disableWebPagePreview: true, replyMarkup: keyboard).Result;
                }
            }
            catch (AggregateException e)
            {
                if (e.Message.Contains("Unsupported start tag"))
                {
                    //Console.WriteLine($"HANDLED\n{e.ErrorCode}\n\n{e.Message}\n\n{e.StackTrace}");
                    result = CatchSend(message, chatid, parsemode: ParseMode.Default);
                }
                if (e.Message.Contains("reply message not found"))
                {
                    //Console.WriteLine($"HANDLED\n{e.ErrorCode}\n\n{e.Message}\n\n{e.StackTrace}");
                    result = Send(message, chatid);

                }
                if (e.Message.Contains("message is too long"))
                {
                    //Console.WriteLine($"HANDLED\n{e.ErrorCode}\n\n{e.Message}\n\n{e.StackTrace}");
                    var messages = Regex.Split(message, "(.+?)(?:\r\n|\n)");
                    var amount = messages.Length / 4;
                    for (int j = 0; j < 4; j++)
                    {
                        var word = "";
                        for (int i = 0; i < amount; i++)
                        {
                            word = $"{word}{messages[i]}";
                        }
                        CatchSend(word, chatid, messageId: msgid);
                    }
                }
                if (e.Message.Contains("bot can't initiate") ||
                    e.Message.Contains("bot was blocked"))
                {
                    if (parentMethod.Equals("SendToPm"))
                    {
                        throw;
                    }
                    else
                    {
                        Console.WriteLine($"{e.Message}\n\n{e.StackTrace}");
                        result = null;
                        
                    }
                }
                else
                {
                    Bot.CatchSend($"{e.Message}", chatid, messageId: msgid);
                    result =  Bot.CatchSend($"{e.Message}\n\n{e.StackTrace}", -1001076212715, parsemode: ParseMode.Default);
                }
            }
            return result;
        }
        internal static Message SendReply(string message, Update msg,[CallerMemberName]string parentMethod = "")
        {            
            return SendReply(message, msg.Message.Chat.Id, msg.Message.MessageId, parentMethod:parentMethod);
        }
        internal static Message SendReply(string message, Update msg, InlineKeyboardMarkup keyboard, [CallerMemberName]string parentMethod = "")
        {            
            return SendReply(message, msg.Message.Chat.Id, msg.Message.MessageId, keyboard, parentMethod);
        }
       

        private static void WatchAPI(object state)
        {
            while (!Api.IsReceiving)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--Not getting updates--");
                Api.StartReceiving();
            }
        }

        public static Boolean DeleteMessage(long chatId, int msgid)
        {
            return Bot.Api.DeleteMessageAsync(chatId, msgid).Result;
           
        }
    }

    internal static class Redis
    {
        private static string key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Werewolf").GetValue("RedisPass").ToString();
        static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"138.201.172.150:6379, password={key}, allowAdmin=true");        
        public static IDatabase db = redis.GetDatabase(Constants.EnforcerDb);

        public static void SaveRedis()
        {
            redis.GetServer($"138.201.172.150:6379").Save(SaveType.BackgroundSave);
        }

        public static bool Start()
        {
            try
            {
                var res = db.StringSet("testWrite", "trying");
                return res;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }

    internal static class Botan
    {
#if normal
        private static string key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Werewolf").GetValue("EBotan").ToString();
#endif
#if premium
        private static string key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey("SOFTWARE\\Werewolf").GetValue("EPBotan").ToString();
#endif
        public static BotanTrackResponse log(Object update, string eventId, long id)
        {
            var url = $"https://api.botan.io/track?token={key}&uid={id}&name={eventId}";            
            var client = new HttpClient();
            var temp = JsonConvert.SerializeObject(update);
            var content = new StringContent(temp, Encoding.UTF8, "application/json");          
            var response = client.PostAsync(url, content).Result;            
            var data = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<BotanTrackResponse>(data);
            return result;
        }

        public static BotanTrackResponse log(Message update, string eventId)
        {        
            return log(update, eventId, update.From.Id);
        }

        public static BotanTrackResponse log(CallbackQuery update, string eventId)
        {
            return log(update, eventId, update.From.Id);
        }

        public class BotanTrackResponse
        {
            public string Status { get; set; }
            public string Information { get; set; }
        }
    }
}
