using System;
using System.Collections.Generic;
using System.Text;

namespace RMS.Domain.Constants;

public class Config
{
    public class Store
    {
        public const string ROOT_PATH = "/Upload";
        public const string APPLICATION_PATH = "/Application";
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
