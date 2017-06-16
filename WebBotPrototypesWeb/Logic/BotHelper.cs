using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using WebBotPrototypes.Models;

namespace WebBotPrototypes.Logic
{
    public class BotHelper
    {
        private static string directLineSecret = ConfigurationManager.AppSettings["DirectLineSecret"];
        private static string botId = ConfigurationManager.AppSettings["BotId"];
        private static string DiretlineUrl = @"https://directline.botframework.com";

        public async Task<Chat> TalkToTheBot(IPrincipal user, string paramMessage)
        {
            // Connect to the DirectLine service
            DirectLineClient client = new DirectLineClient(directLineSecret);

            // Try to get the existing Conversation
            Conversation conversation =  System.Web.HttpContext.Current.Session["conversation"] as Conversation;
            
            // Try to get an existing watermark             
            // the watermark marks the last message we received
            string watermark = System.Web.HttpContext.Current.Session["watermark"] as string;

            if (conversation == null)
            {
                // There is no existing conversation
                // start a new one
                conversation = client.Conversations.NewConversation();
            }

            // Use the text passed to the method (by the user)
            // to create a new message
            Message message = new Message
            {
                FromProperty = user.Identity.Name,
                Text = paramMessage
            };

            // Post the message to the Bot
            await client.Conversations.PostMessageAsync(conversation.ConversationId, message);

            // Get the response as a Chat object
            Chat objChat = await ReadBotMessagesAsync(client, conversation.ConversationId, watermark);

            // Save values
            System.Web.HttpContext.Current.Session["conversation"] = conversation;
            System.Web.HttpContext.Current.Session["watermark"] = objChat.watermark;

            var msg = objChat.ChatResponse;

            var notMatchFoundText = "No match found!";
            if (msg != null)
            { 
                if (msg.Contains(notMatchFoundText))
                {
                    var sorryText = GetRandomNoMatchFoundText();
                    msg = msg.Replace(notMatchFoundText, sorryText);
                }

                objChat.ChatResponse = msg;
            }

            // Return the response as a Chat object
            return objChat;
        }

        private async Task<Chat> ReadBotMessagesAsync(
            DirectLineClient client, string conversationId, string watermark)
        {
            // Create an Instance of the Chat object
            Chat objChat = new Chat();

            // We want to keep waiting until a message is received
            bool messageReceived = false;
            while (!messageReceived)
            {
                // Get any messages related to the conversation since the last watermark 
                var messages =
                    await client.Conversations.GetMessagesAsync(conversationId, watermark);

                // Set the watermark to the message received
                watermark = messages?.Watermark;

                // Get all the messages 
                var messagesFromBotText = from message in messages.Messages
                                          where message.FromProperty == botId
                                          select message;

                // Loop through each message
                foreach (Message message in messagesFromBotText)
                {
                    // We have Text
                    if (message.Text != null)
                    {
                        // Set the text response
                        // to the message text
                        objChat.ChatResponse
                            += " "
                            + message.Text.Replace("\n\n", "<br />");
                    }

                    // We have an Image
                    if (message.Images.Count > 0)
                    {
                        // Set the text response as an HTML link
                        // to the image
                        objChat.ChatResponse
                            += " "
                            + RenderImageHTML(message.Images[0]);
                    }
                }

                // Mark messageReceived so we can break 
                // out of the loop
                messageReceived = true;
            }

            // Set watermark on te Chat object that will be 
            // returned
            objChat.watermark = watermark;

            // Return a response as a Chat object
            return objChat;
        }

        private static string RenderImageHTML(string ImageLocation)
        {
            // Construct a URL to the image
            string strReturnHTML =
                String.Format(@"<img src='{0}/{1}'><br />",
                DiretlineUrl,
                ImageLocation);

            return strReturnHTML;
        }

        private string GetRandomNoMatchFoundText()
        {
            var rnd = new Random();

            string[] t = new string[]
            {
                "Das habe ich leider nicht verstanden. Vielleicht probierst du es mit einer anderen Frage.",
                //"Diese Frage kann ich leider nicht beantworten.",
                //"Das weiß ich leider nicht."
            };

            int rndNo = rnd.Next(t.Length);

            var result = t[rndNo];
            return result;
        }
    }
}