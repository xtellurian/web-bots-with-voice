
using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

using Microsoft.Bot.Connector.DirectLine.Models;
using WebBotPrototypes.Logic;
using WebBotPrototypes.Models;

namespace WebBotPrototypes.Controllers
{

    public class DirectLineChatController : Controller
    {
        BotHelper botHelper = new BotHelper();

        public async Task<ActionResult> Index()
        {
            // Create an Instance of the Chat object
            Chat objChat = new Chat();

            // Only call Bot if logged in
            if (User.Identity.IsAuthenticated)
            {
                // Pass the message to the Bot 
                // and get the response
                objChat = await botHelper.TalkToTheBot(User, "Hello");
            }
            else
            {
                objChat.ChatResponse = "Must be logged in";
            }

            // Return response
            return View(objChat);
        }

        [HttpPost]
        public async Task<ActionResult> Index(Chat model)
        {
            // Create an Instance of the Chat object
            Chat objChat = new Chat();

            // Only call Bot if logged in
            if (User.Identity.IsAuthenticated)
            {
                // Pass the message to the Bot and get the response
                objChat = await botHelper.TalkToTheBot(User, model.ChatMessage);
            }
            else
            {
                objChat.ChatResponse = "Must be logged in";
            }

            // Return response
            return View(objChat);
        }

        
    }
}

