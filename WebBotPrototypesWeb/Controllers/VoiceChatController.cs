using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebBotPrototypes.Logic;
using WebBotPrototypes.Models;

namespace WebBotPrototypes.Controllers
{
    public class VoiceChatController : Controller
    {
        BotHelper botHelper = new BotHelper();

        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> SendChat(string text)
        {
            JsonResult result = new JsonResult();

            if (text == null || 
                text.Length == 0)
            {
                return result;
            }

            ViewBag.StatusMessage = $"Chat sent: {text}";

            // Create an Instance of the Chat object
            Chat objChat = new Chat();

            // Only call Bot if logged in
            if (User.Identity.IsAuthenticated)
            {
                // Pass the message to the Bot 
                // and get the response
                objChat = await botHelper.TalkToTheBot(User, text);
            }
            else
            {
                objChat.ChatResponse = "Must be logged in";
            }

            result.Data = objChat;

            return result;
        }

        public async Task<ActionResult> GetTextToSpeech(string text, int no)
        {
            if (text == null)
            {
                text = "Kein text angekommen.";
            }

            Debug.WriteLine(no);

            SpeechResult speechResult = new SpeechResult();
            speechResult.TextPartNo = no;
            speechResult.Text = text;

            #region Bing Speech Authentication

            // Note: The way to get api key:
            // Free: https://www.microsoft.com/cognitive-services/en-us/subscriptions?productId=/products/Bing.Speech.Preview
            // Paid: https://portal.azure.com/#create/Microsoft.CognitiveServices/apitype/Bing.Speech/pricingtier/S0
            string bingApiKey = System.Configuration.ConfigurationManager.AppSettings["BingSpeechApiKey"];

            Authentication auth = new Authentication(bingApiKey);
            string accessToken;
            
            try
            {
                accessToken = auth.GetAccessToken();
                speechResult.AccessToken = accessToken;
            }
            catch (Exception ex)
            {
                speechResult.ErrorMessage = "GetTextToSpeech --> Failed authentication --> " + ex.Message + " " + ex.ToString();

                JsonResult result = new JsonResult()
                {
                    Data = speechResult
                };
                return result;
            }

            #endregion

            #region Text-To-Speech

            // Debug.WriteLine("Starting TTSSample request code execution.");

            string requestUri = "https://speech.platform.bing.com/synthesize";

            var cortana = new Synthesize();

            cortana.OnAudioAvailable += (s, args) => {
                var input = args.EventData;

                byte[] buffer = new byte[20*1024*1024];
                using (MemoryStream ms = new MemoryStream())
                {
                    int read;
                    while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }

                    speechResult.SpeechEventData = ms.ToArray(); ;
                }
            };

            //cortana.OnError += ErrorHandler;
            cortana.OnError += (s, e) =>
            {
                var message = $"Unable to complete the TTS request: {e.ToString()}";
                message += e.EventData.Message;
                speechResult.ErrorMessage = message;
            };

            // Reuse Synthesize object to minimize latency
            await cortana.Speak(CancellationToken.None, 
                new Synthesize.InputOptions()
                {
                    RequestUri = new Uri(requestUri),
                    // Text to be spoken.
                    Text = text,
                    VoiceType = Gender.Female,
                    // Refer to the documentation for complete list of supported locales.
                    Locale = "de-DE",
                    // You can also customize the output voice. Refer to the documentation to view the different
                    // voices that the TTS service can output.
                    VoiceName = "Microsoft Server Speech Text to Speech Voice (de-De, Hedda)",
                    // Service can return audio in different output format.
                    OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                    AuthorizationToken = "Bearer " + accessToken,
                });

            JsonResult jsonResult = new JsonResult()
            {
                Data = speechResult
            };

            #endregion

            return jsonResult;
        }

    }
}