﻿using DiIiS_NA.Core.Discord.Modules;
using DiIiS_NA.Core.Logging;
using DiIiS_NA.Core.MPQ;
using DiIiS_NA.Core.Storage;
using DiIiS_NA.Core.Storage.AccountDataBase.Entities;
using DiIiS_NA.GameServer.AchievementSystem;
using DiIiS_NA.GameServer.CommandManager;
using DiIiS_NA.GameServer.GSSystem.ActorSystem;
using DiIiS_NA.GameServer.GSSystem.GameSystem;
using DiIiS_NA.GameServer.GSSystem.ItemsSystem;
using DiIiS_NA.LoginServer;
using DiIiS_NA.LoginServer.AccountsSystem;
using DiIiS_NA.LoginServer.Base;
using DiIiS_NA.LoginServer.Battle;
using DiIiS_NA.LoginServer.GuildSystem;
using DiIiS_NA.LoginServer.Toons;
using DiIiS_NA.REST;
using DiIiS_NA.REST.Manager;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Npgsql;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
using DiIiS_NA.Core.Extensions;
using DiIiS_NA.D3_GameServer;
using Spectre.Console;
using Environment = System.Environment;
using FluentNHibernate.Utils;

namespace DiIiS_NA
{
    public enum TypeBuildEnum
    {
        Alpha,
        Beta,
        Test,
        Release
    }
    class Program
    {
        private static readonly Logger Logger = LogManager.CreateLogger("Blizzless");
        public static readonly DateTime StartupTime = DateTime.Now;
        public static BattleBackend BattleBackend { get; set; }
        public bool GameServersAvailable = true;

        public const int MAX_LEVEL = 70;

        public static GameServer.ClientSystem.GameServer GameServer;
        public static Watchdog Watchdog;

        public static Thread GameServerThread;
        public static Thread WatchdogThread;

        public static string LoginServerIp = LoginServerConfig.Instance.BindIP;
        public static string GameServerIp = DiIiS_NA.GameServer.GameServerConfig.Instance.BindIP;
        public static string RestServerIp = RestConfig.Instance.IP;
        public static string PublicGameServerIp = DiIiS_NA.GameServer.NATConfig.Instance.PublicIP;

        public const int BUILD = 30;
        public const int STAGE = 3;
        public static TypeBuildEnum TypeBuild => TypeBuildEnum.Beta;
        private static bool _diabloCoreEnabled = DiIiS_NA.GameServer.GameServerConfig.Instance.CoreActive;

        private static readonly CancellationTokenSource CancellationTokenSource = new();
        public static readonly CancellationToken Token = CancellationTokenSource.Token;
        public static void Cancel() => CancellationTokenSource.Cancel();
        public static void CancelAfter(TimeSpan span) => CancellationTokenSource.CancelAfter(span);
        public static bool IsCancellationRequested() => CancellationTokenSource.IsCancellationRequested;

        public void MergeCancellationWith(params CancellationToken[] tokens) =>
            CancellationTokenSource.CreateLinkedTokenSource(tokens);
        static void WriteBanner()
        {
            void RightTextRule(string text, string ruleStyle) => AnsiConsole.Write(new Rule(text).RuleStyle(ruleStyle));
            string Url(string url) => $"[link={url}]{url}[/]";
            RightTextRule("[dodgerblue1]Blizz[/][deepskyblue2]less[/]", "steelblue1");
            RightTextRule($"[dodgerblue3]Build [/][deepskyblue3]{BUILD}[/]", "steelblue1_1");
            RightTextRule($"[dodgerblue3]Stage [/][deepskyblue3]{STAGE}[/]", "steelblue1_1");
            RightTextRule($"[deepskyblue3]{TypeBuild}[/]", "steelblue1_1");
            RightTextRule($"Diablo III [red]RoS 2.7.4.84161[/] - {Url("https://github.com/blizzless/blizzless-diiis")}",
                "red");
            AnsiConsole.MarkupLine("");
            AnsiConsole.MarkupLine("");
        }
        
        static async Task StartAsync(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

            DbProviderFactories.RegisterFactory("Npgsql", NpgsqlFactory.Instance);
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            string name = $"Blizzless: Build {BUILD}, Stage: {STAGE} - {TypeBuild}";
            SetTitle(name);
            if (LogConfig.Instance.Targets.Any(x => x.MaximizeWhenEnabled && x.Enabled))
                Maximize();
            WriteBanner();
            InitLoggers();
#if DEBUG
            _diabloCoreEnabled = true;
            Logger.Info("Forcing Diablo III Core to be $[green]$enabled$[/]$ on debug mode.");
#else
            if (!_diabloCoreEnabled)
                Logger.Warn("Diablo III Core is $[red]$disabled$[/]$.");
#endif
            var mod = GameModsConfig.Instance;
#pragma warning disable CS4014
            Task.Run(async () =>
#pragma warning restore CS4014
            {
                while (true)
                {
                    try
                    {
                        var uptime = (DateTime.Now - StartupTime);
                        // get total memory from process
                        var totalMemory =
                            (double)((double)Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024 / 1024);
                        // get CPU time
                        using var proc = Process.GetCurrentProcess();
                        var cpuTime = proc.TotalProcessorTime;
                        var text =
                            $"{name} | " +
                            $"{PlayerManager.OnlinePlayers.Count} onlines in {PlayerManager.OnlinePlayers.Count(s => s.InGameClient?.Player?.World != null)} worlds | " +
                            $"Memory: {totalMemory:0.000} GB | " +
                            $"CPU Time: {cpuTime.ToSmallText()} | " +
                            $"Uptime: {uptime.ToSmallText()}";

                        if (IsCancellationRequested())
                            text = "SHUTTING DOWN: " + text;
                        if (SetTitle(text))
                            await Task.Delay(1000);
                        else
                        {
                            Logger.Info(text);
                            await Task.Delay(TimeSpan.FromMinutes(1));
                        }
                    }
                    catch (Exception e)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5));
                    }
                }
            });
            AchievementManager.Initialize();
            Core.Storage.AccountDataBase.SessionProvider.RebuildSchema();

            string GeneratePassword(int size) =>
                new(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", size)
                    .Select(s => s[new Random().Next(s.Length)]).ToArray());

            void LogAccountCreated(string username, string password)
                => Logger.Success(
                    $"Created account: $[springgreen4]${username}$[/]$ with password: $[springgreen4]${password}$[/]$");
#if DEBUG
            if (!DBSessions.SessionQuery<DBAccount>().Any())
            {
                var password1 = GeneratePassword(6);
                var password2 = GeneratePassword(6);
                var password3 = GeneratePassword(6);
                var password4 = GeneratePassword(6);
                Logger.Info($"Initializing account database...");
                var account = AccountManager.CreateAccount("owner@", password1, "owner", Account.UserLevels.Owner);
                var gameAccount = GameAccountManager.CreateGameAccount(account);
                LogAccountCreated("owner@", password1);
                var account1 = AccountManager.CreateAccount("gm@", password2, "gm", Account.UserLevels.GM);
                var gameAccount1 = GameAccountManager.CreateGameAccount(account1);
                LogAccountCreated("gm@", password2);
                var account2 = AccountManager.CreateAccount("tester@", password3, "tester", Account.UserLevels.Tester);
                var gameAccount2 = GameAccountManager.CreateGameAccount(account2);
                LogAccountCreated("tester@", password3);
                var account3 = AccountManager.CreateAccount("user@", password4, "test3", Account.UserLevels.User);
                var gameAccount3 = GameAccountManager.CreateGameAccount(account3);
                LogAccountCreated("user@", password4);
            }
#else
            if (!Enumerable.Any(DBSessions.SessionQuery<DBAccount>()))
            {
                var password = GeneratePassword(6);
                var account =
 AccountManager.CreateAccount("iwannatry@", password, "iwannatry", Account.UserLevels.User);
                var gameAccount = GameAccountManager.CreateGameAccount(account);
                LogAccountCreated("iwannatry@", password);
            }
#endif

            if (DBSessions.SessionQuery<DBAccount>().Any())
            {
                Logger.Success("Database connection has been $[underline bold italic]$successfully established$[/]$.");
            }

            //*/
            StartWatchdog();

            AccountManager.PreLoadAccounts();
            GameAccountManager.PreLoadGameAccounts();
            ToonManager.PreLoadToons();
            GuildManager.PreLoadGuilds();

            Logger.Info("Loading Diablo III - Core...");
            if (_diabloCoreEnabled)
            {
                if (!MPQStorage.Initialized)
                {
                    throw new Exception("MPQ archives not found...");
                }

                Logger.Info("Loaded - {0} items.", ItemGenerator.TotalItems);
                Logger.Info("Diablo III Core - Loaded");
            }
            else
            {
                Logger.Fatal("Diablo III Core - Disabled");
            }

            var restSocketServer = new SocketManager<RestSession>();
            if (!restSocketServer.StartNetwork(RestServerIp, RestConfig.Instance.Port))
                throw new Exception($"Failed to start REST server on {RestServerIp}:{RestConfig.Instance.Port} - please check your configuration and if the port is in use.");

            Logger.Success(
                $"$[darkgreen]$REST$[/]$ server started - {RestConfig.Instance.IP}:{RestConfig.Instance.Port}");

            //BGS
            var loginConfig = LoginServerConfig.Instance;
            ServerBootstrap serverBootstrap = new ServerBootstrap();
            IEventLoopGroup boss = new MultithreadEventLoopGroup(1),
                worker = new MultithreadEventLoopGroup();
            serverBootstrap.LocalAddress(loginConfig.BindIP, loginConfig.Port);
            Logger.Success(
                $"Blizzless server $[underline]$started$[/]$ - $[lightseagreen]${loginConfig.BindIP}:{loginConfig.Port}$[/]$");
            BattleBackend = new BattleBackend(loginConfig.BindIP, loginConfig.WebPort);

            //Diablo 3 Game-Server
            if (_diabloCoreEnabled)
                StartGameServer();
            else Logger.Fatal("Game server is disabled in the configs.");

            try
            {
                serverBootstrap.Group(boss, worker)
                    .Channel<TcpServerSocketChannel>()
                    .Handler(new LoggingHandler(LogLevel.DEBUG))
                    .ChildHandler(new ConnectHandler());

                IChannel boundChannel = await serverBootstrap.BindAsync(loginConfig.Port);

                Logger.Info("$[bold deeppink4]$Gracefully$[/]$ shutdown with $[red3_1]$CTRL+C$[/]$ or $[deeppink4]$!q[uit]$[/]$.");
                Logger.Info("{0}", IsCancellationRequested());
                while (!IsCancellationRequested())
                {
                    var line = Console.ReadLine();
                    if(line == null){
                        continue;
                    }
                    if (line == "!q" || line == "!quit" || line == "!exit")
                    {
                        Logger.Info("Break !quit");
                        break;
                    }

                    if (line == "!cls" || line == "!clear" || line == "cls" || line == "clear")
                    {
                        AnsiConsole.Clear();
                        AnsiConsole.Cursor.SetPosition(0, 0);
                        continue;
                    }

                    if (line.StartsWith("!sno", StringComparison.OrdinalIgnoreCase))
                    {
                        if (IsTargetEnabled("ansi"))
                            Console.Clear();
                        
                        MPQStorage.Data.SnoBreakdown(
                            line.Equals("!sno 1", StringComparison.OrdinalIgnoreCase) || 
                            line.Equals("!sno true", StringComparison.OrdinalIgnoreCase)
                        );
                        continue;
                    }

                    CommandManager.Parse(line);
                }

                if (PlayerManager.OnlinePlayers.Count > 0)
                {
                    Logger.Success("Gracefully shutting down...");
                    Logger.Info(
                        $"Server is shutting down in 1 minute, $[blue]${PlayerManager.OnlinePlayers.Count} players$[/]$ are still online.");
                    PlayerManager.SendWhisper("Server is shutting down in 1 minute.");
                 
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }

                Shutdown();
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());
                Shutdown(e);
            }
            finally
            {
                Logger.Trace("Shutdown in progress !");
                await Task.WhenAll(
                    boss.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                    worker.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
            }
        }

        private static bool _shuttingDown = false;
        public static void Shutdown(Exception exception = null)
        
        {
            Logger.Trace("Shutdown here");
            Logger.Trace("Stack trace at shutdown: " + Environment.StackTrace); // Log the stack trace
            if (_shuttingDown) return;
            _shuttingDown = true;
            if (!IsCancellationRequested())
                Cancel();
         
            AnsiTarget.StopIfRunning(IsTargetEnabled("ansi"));
            if (exception != null)
            {
                AnsiConsole.WriteLine(
                    "An unhandled exception occured at initialization. Please report this to the developers.");
                AnsiConsole.WriteException(exception);
            }

            AnsiConsole.Progress().Start(ctx =>
            {
                var task = ctx.AddTask("[darkred_1]Shutting down[/] [white]in[/] [red underline]10 seconds[/]");
                for (int i = 1; i < 11; i++)
                {
                    task.Description = $"[darkred_1]Shutting down[/] [white]in[/] [red underline]{11 - i} seconds[/]";
                    for (int j = 0; j < 10; j++)
                    {
                        task.Increment(1);
                        Thread.Sleep(100);
                    }
                }

                task.Description = $"[darkred_1]Shutting down now.[/]";
                task.StopTask();
            });

            Environment.Exit(exception is null ? 0 : -1);
        }

        static async Task Main(string[] args)
        {
            args ??= Array.Empty<string>();

            try
            {
                await StartAsync(args);
            }
            catch (Exception ex)
            {
                Shutdown(ex);
            }
        }

        [SecurityCritical]
        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            if (e.IsTerminating)
            {
                Shutdown(ex);
            }
            else
                Logger.ErrorException(ex, "A root error of the server was detected but was handled.");
        }

        static int TargetsEnabled(string target) => LogConfig.Instance.Targets.Count(t => t.Target.ToLower() == target && t.Enabled);
        public static bool IsTargetEnabled(string target) => TargetsEnabled(target) > 0;
        private static void InitLoggers()
        {
            LogManager.Enabled = true;
            
            if (TargetsEnabled("ansi") > 1 || (IsTargetEnabled("console") && IsTargetEnabled("ansi")))
            {
                AnsiConsole.MarkupLine("[underline red on white]Fatal:[/] [red]It is impossible to have both ANSI and Console targets activated concurrently.[/]");
                Shutdown();
            }
            foreach (var targetConfig in LogConfig.Instance.Targets)
            {
                if (!targetConfig.Enabled)
                    continue;

                LogTarget target = targetConfig.Target.ToLower() switch
                {
                    "ansi" => new AnsiTarget(targetConfig.MinimumLevel, targetConfig.MaximumLevel,
                        targetConfig.IncludeTimeStamps, targetConfig.TimeStampFormat),
                    "console" => new ConsoleTarget(targetConfig.MinimumLevel, targetConfig.MaximumLevel,
                        targetConfig.IncludeTimeStamps, targetConfig.TimeStampFormat),
                    "file" => new FileTarget(targetConfig.FileName, targetConfig.MinimumLevel,
                        targetConfig.MaximumLevel, targetConfig.IncludeTimeStamps, targetConfig.TimeStampFormat,
                        targetConfig.ResetOnStartup),
                    _ => null
                };

                if (target != null)
                    LogManager.AttachLogTarget(target);
            }
        }
        public static void StartWatchdog()
        {
            Watchdog = new Watchdog();
            WatchdogThread = new Thread(Watchdog.Run) { Name = "Watchdog", IsBackground = true };
            WatchdogThread.Start();
        }
        public static void StartGameServer()
        {
            if (GameServer != null) return;

            GameServer = new DiIiS_NA.GameServer.ClientSystem.GameServer();
            GameServerThread = new Thread(GameServer.Run) { Name = "GameServerThread", IsBackground = true };
            GameServerThread.Start();
            if (Core.Discord.Config.Instance.Enabled)
            {
                Logger.Info("Starting Discord bot handler..");
                GameServer.DiscordBot = new Core.Discord.Bot();
                GameServer.DiscordBot.MainAsync().GetAwaiter().GetResult();
            }
            else
            {
                Logger.Trace("Discord bot Disabled..");
            }
            DiIiS_NA.GameServer.GSSystem.GeneratorsSystem.SpawnGenerator.RegenerateDensity();
            Logger.Trace("We are here first");
            DiIiS_NA.GameServer.ClientSystem.GameServer.GSBackend = new GsBackend(LoginServerConfig.Instance.BindIP, LoginServerConfig.Instance.WebPort);
        }

        static bool SetTitle(string text)
        {
            try
            {
                Console.Title = text;
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
        }
        
        [DllImport("kernel32.dll", ExactSpelling = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]

        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        const int HIDE = 0;
        const int MAXIMIZE = 3;
        const int MINIMIZE = 6;
        const int RESTORE = 9;
        private static void Maximize()
        {
            try
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    ShowWindow(GetConsoleWindow(), MAXIMIZE);
                }
            }
            catch{ /*ignore*/ }
        }
    }
}
