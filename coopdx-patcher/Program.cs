﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net;

static class Program {
    static void Main(string[] args) {
        Patcher.CreateFolder(Patcher.appDataPath);

        string version = new WebClient().DownloadString("https://sm64coopdx.com/download/version.txt");
        string path = Path.Combine(Patcher.appDataPath, "version.txt");
        if (File.Exists("sm64coopdx/sm64coopdx.exe") && File.Exists(path) && version == File.ReadAllText(path)) {
            Patcher.WriteLine("sm64coopdx is up to date!.", ConsoleColor.Yellow);
            Patcher.WriteLine("Press any key to exit.", ConsoleColor.Yellow);
            Console.ReadKey();
            return;
        }

        Patcher.WriteLine($"coopdx-patcher v{Patcher.version}", ConsoleColor.Cyan);

        if (!Patcher.CheckVersion()) {
            Patcher.WriteLine("Press any key to exit.", ConsoleColor.Yellow);
            Console.ReadKey();
            return;
        }

        Patcher.GetROM(args.Length > 0 ? args[0] : "");

        // ask for renderer but also keep compatibility with earlier versions
        string renderer = "OpenGL_";
        Patcher.Write("Use OpenGL or DirectX? (OpenGL is default) [OpenGL|DirectX] ", ConsoleColor.Yellow);
        string option = Console.ReadLine().ToLower().Trim();
        while (option != "" && option != "opengl" && option != "directx") {
            Patcher.WriteLine("Invalid renderer", ConsoleColor.Yellow);
            Patcher.Write("Use OpenGL or DirectX? [OpenGL|DirectX] ", ConsoleColor.Yellow);
            option = Console.ReadLine().ToLower().Trim();
        }
        if (option == "directx") {
            renderer = "DirectX_";
        }

        string bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        if (!Patcher.Download("patch file", $"https://sm64coopdx.com/download/sm64coopdx_{renderer}Windows_{bit}.bsdiff", Patcher.patchPath, true)) {
            Patcher.WriteLine("Press any key to exit.", ConsoleColor.Yellow);
            Console.ReadKey();
            return;
        }
        Patcher.CreateFolder(Patcher.outPath, true);
        Patcher.CreateExecutable(version);

        Patcher.Write("Downloading symbols...", ConsoleColor.DarkGray);
        Patcher.Download("coop.map", $"https://sm64coopdx.com/download/maps/{bit}/{renderer}/coop.map", Path.Combine(Patcher.outPath, "coop.map"));
        Patcher.Write(" Done!\n", ConsoleColor.Green);

        Patcher.Write("Downloading DLLs...", ConsoleColor.DarkGray);
        Patcher.Download("bass.dll", $"https://sm64coopdx.com/download/dlls/{bit}/bass.dll", Path.Combine(Patcher.outPath, "bass.dll"));
        Patcher.Download("bass_fx.dll", $"https://sm64coopdx.com/download/dlls/{bit}/bass_fx.dll", Path.Combine(Patcher.outPath, "bass_fx.dll"));
        Patcher.Download("discord_game_sdk.dll", $"https://sm64coopdx.com/download/dlls/{bit}/discord_game_sdk.dll", Path.Combine(Patcher.outPath, "discord_game_sdk.dll"));
        Patcher.Write(" Done!\n", ConsoleColor.Green);

        Patcher.Write("Downloading resources...", ConsoleColor.DarkGray);
        Patcher.Download("resources.zip", "https://sm64coopdx.com/download/resources.zip", Patcher.resourcesPath);
        Patcher.Write(" Done!\n", ConsoleColor.Green);

        // extract resources zip
        Patcher.Write("Extracting resources...", ConsoleColor.DarkGray);

        // keep original dynos and mods folders intact
        string modsBackup = Path.Combine(Patcher.outPath, "mods_backup");
        string dynosBackup = Path.Combine(Patcher.outPath, "dynos_backup");
        if (Directory.Exists(modsBackup)) {
            Directory.Delete(modsBackup);
        }
        if (Directory.Exists(dynosBackup)) {
            Directory.Delete(dynosBackup);
        }
        Patcher.RenameFolder(Path.Combine(Patcher.outPath, "mods"), modsBackup);
        Patcher.RenameFolder(Path.Combine(Patcher.outPath, "dynos"), dynosBackup);

        // extract resources (dynos, lang and mods)
        Patcher.DeleteFolder(Path.Combine(Patcher.outPath, "lang"));
        ZipFile.ExtractToDirectory(Patcher.resourcesPath, Patcher.outPath);

        Patcher.Write(" Done!\n", ConsoleColor.Green);

        // clean up
        Patcher.Write("Cleaning up...", ConsoleColor.DarkGray);
        File.Delete(Patcher.resourcesPath);
        Patcher.Write(" Done!\n", ConsoleColor.Green);

        Patcher.WriteLine("sm64coopdx has been created, have fun :)", ConsoleColor.Yellow);
        Patcher.WriteLine("Press any key to exit.", ConsoleColor.Yellow);
        Console.ReadKey();
    }
}