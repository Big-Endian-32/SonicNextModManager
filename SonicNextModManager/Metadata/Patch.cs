﻿using SonicNextModManager.Lua;
using System.Collections.ObjectModel;

namespace SonicNextModManager.Metadata
{
    public class Patch : MetadataBase
    {
        /// <summary>
        /// The category that represents this patch.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// The short description for this patch.
        /// </summary>
        public string? Blurb { get; set; }

        /// <summary>
        /// The name of the first function to call in the Lua script - if null, the script in its entirety will be executed.
        /// </summary>
        public string? Function { get; set; }

        /// <summary>
        /// A collection of described declared variables used in the Lua script - used for configuration.
        /// </summary>
        public ObservableCollection<Declaration> Declarations { get; set; } = new();

        [JsonIgnore]
        public string? Code { get; set; }

        public Patch() { }

        public Patch(string? in_file)
        {
            if (!File.Exists(in_file))
                throw new FileNotFoundException($"Patch does not exist: {in_file}");

            var lines = File.ReadAllLines(in_file);

            var json = new StringBuilder();
            var lua = new StringBuilder();

            var i = 0;
            var isJsonReached = false;

            // Extract JSON from header.
            for (; i < lines.Length; i++)
            {
                var line = lines[i];

                if (line.StartsWith('{'))
                {
                    json.AppendLine(line);

                    isJsonReached = true;

                    continue;
                }

                if (isJsonReached)
                {
                    json.AppendLine(line);

                    if (line.StartsWith('}'))
                        break;
                }
            }

            // Extract Lua code from the remainder of the file.
            for (i++; i < lines.Length; i++)
                lua.AppendLine(lines[i]);

            var patch = JsonConvert.DeserializeObject<Patch>(json.ToString()) ??
                throw new JsonException("Failed to parse patch metadata.");

            Title        = patch.Title;
            Author       = patch.Author;
            Platform     = patch.Platform;
            Date         = patch.Date;
            Description  = patch.Description;
            Category     = patch.Category;
            Blurb        = patch.Blurb;
            Function     = patch.Function;
            Declarations = patch.Declarations;
            Code         = lua.ToString();
            Location     = in_file;
        }

        public static Patch Parse(string? in_file)
        {
            return new Patch(in_file);
        }

        public static void Write(Patch in_mod, string? in_file)
        {
            ArgumentException.ThrowIfNullOrEmpty(in_file);

            File.WriteAllText(in_file, JsonConvert.SerializeObject(in_mod, Formatting.Indented));
        }

        public void Write(string? in_file)
        {
            Write(this, in_file);
        }

        public void Write()
        {
            Write(this, Location);
        }

        public void Install()
        {
            // Initialise patch symbols.
            Patcher.AddSymbol("Executable", App.Settings.Path_GameExecutable!);
            Patcher.AddSymbol("Platform", App.CurrentPlatform.ToString());
            Patcher.AddSymbol("Root", App.Settings.GetGameDirectory()!);
            Patcher.AddSymbol("Work", Path.GetDirectoryName(Location)!);

            // Initialise script interpreter.
            var l = new Script().Initialise();

            // Initialise global declarations.
            foreach (var decl in Declarations)
            {
                // TODO: use config values.
                l.Globals[decl.Name] = decl.DefaultValue;
            }

            l.DoString(Code);

            if (string.IsNullOrEmpty(Function))
                return;

            // Call the user-specified function.
            l.Call(l.Globals[Function]);
        }
    }
}
