﻿namespace SonicNextModManager.Lua
{
    public class Declaration
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public DeclarationType? Type { get; set; }

        public object? DefaultValue { get; set; }
    }
}
