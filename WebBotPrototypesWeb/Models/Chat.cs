using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace WebBotPrototypes.Models
{
    public class Chat
    {
        public string ChatMessage { get; set; }
        public string ChatResponse { get; set; }
        public string watermark { get; set; }

        public string[] ChatResponseParts
        {
            get
            {
                var splitKey = new char[] { '.', ' ' };
                //var input = ChatResponse.Split(splitKey);

                var input = Regex.Split(ChatResponse, @"\.\ ");

                return input;
            }
        }
    }
}