using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Huww98.FiveInARow.Engine
{
    class PbrainAdapter
    {
        EngineControl controller;
        private readonly TextReader reader;
        private readonly TextWriter writer;

        public PbrainAdapter(IEngine engine ,TextReader reader, TextWriter writer)
        {
            this.reader = reader;
            this.writer = writer;
            controller = new EngineControl(engine);
        }

        public PbrainAdapter(IEngine engine) 
            : this(engine, new StreamReader(Console.OpenStandardInput()), new StreamWriter(Console.OpenStandardOutput()))
        {}

        public About About { get; set; }

        public async Task StartAsync()
        {
            controller.MoveMade += Controller_MoveMade;
            while (true)
            {
                var line = await reader.ReadLineAsync();
                var command = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (command.Length == 0)
                {
                    continue;
                }
                switch (command[0])
                {
                    case "START":
                        var size = int.Parse(command[1]);
                        controller.StartGame(size, size);
                        await writer.OK();
                        break;
                    case "RECTSTART":
                        var sizes = command[1].Split(',').Select(s => int.Parse(s)).ToList();
                        controller.StartGame(sizes[0], sizes[1]);
                        await writer.OK();
                        break;
                    case "RESTART":
                        controller.NewBoard(Enumerable.Empty<Move>());
                        await writer.OK();
                        break;
                    case "BEGIN":
                        controller.BeginTurn();
                        break;
                    case "TURN":
                        var coordinate = command[1].Split(',').Select(s => int.Parse(s)).ToList();
                        controller.OpponentMove(coordinate[0], coordinate[1]);
                        controller.BeginTurn();
                        break;
                    case "BOARD":
                        await ProcessBoardAsync(reader);
                        controller.BeginTurn();
                        break;
                    case "INFO":
                        var kvp = command[1].Split(' ');
                        ProcessInfo(kvp[0], kvp[1]);
                        break;
                    case "ABOUT":
                        await writer.WriteLineAndFlush(About.ToString());
                        break;
                    case "END":
                        controller.MoveMade -= Controller_MoveMade;
                        return;
                    default:
                        await writer.WriteLineAndFlush($"UNKNOWN command {line}");
                        break;
                }
            }
        }

        private async void Controller_MoveMade(object sender, MoveMadeEventArgs e)
        {
            await writer.WriteLineAndFlush($"{e.X},{e.Y}");
        }

        private void ProcessInfo(string key, string value)
        {
            switch (key)
            {
                case "timeout_turn":
                    controller.TurnTimeout = TimeSpan.FromMilliseconds(int.Parse(value));
                    break;
                case "timeout_match":
                    int v = int.Parse(value);
                    controller.MatchTimeout = v == 0 ? TimeSpan.MaxValue : TimeSpan.FromMilliseconds(v);
                    break;
                case "time_left":
                    controller.TurnTimeout = TimeSpan.FromMilliseconds(int.Parse(value));
                    break;
            }
        }

        private async Task ProcessBoardAsync(TextReader reader)
        {
            List<Move> moves = new List<Move>();
            while (true)
            {
                var line = await reader.ReadLineAsync();
                if (line == "DONE")
                {
                    break;
                }
                var c = line.Split(',').Select(s => int.Parse(s)).ToList();
                moves.Add(new Move
                {
                    X = c[0],
                    Y = c[1],
                    Player = c[2] == 1 ? Player.Own : Player.Opponent
                });
            }
            controller.NewBoard(moves);
        }
    }

    struct About
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Country { get; set; }
        public string WWW { get; set; }
        public string Email { get; set; }

        public override string ToString()
        {
            var me = this;
            var fields = typeof(About).GetProperties()
                .Where(p => p.GetValue(me) != null)
                .Select(p => $"{p.Name.ToLower()}=\"{p.GetValue(me)}\"");
            return string.Join(", ", fields);
        }
    }

    static class TextWriterExtension
    {
        public static async Task WriteLineAndFlush(this TextWriter writer, string value)
        {
            await writer.WriteLineAsync(value);
            await writer.FlushAsync();
        }

        public static Task OK(this TextWriter writer)
            => writer.WriteLineAndFlush("OK");
    }
}
