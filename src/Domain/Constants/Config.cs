using System;
using System.Collections.Generic;
using System.Text;

namespace RMS.Domain.Constants;

public static class Config
{
    public static class Store
    {
        public const string UPLOAD_ROOT = "Upload";
        public const string ROOT_PATH = UPLOAD_ROOT; // relative path, resolved at runtime
        public const string APPLICATION_PATH = "Application";
        public static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };
    }
}
