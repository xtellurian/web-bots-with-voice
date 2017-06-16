using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace WebBotPrototypes.Models
{
    public class SpeechResult
    {
        public string ErrorMessage { get; internal set; }
        public string AccessToken { get; internal set; }
        public byte[] SpeechEventData { get; internal set; }
        public int TextPartNo { get; internal set; }
        public string Text { get; internal set; }
    }
}