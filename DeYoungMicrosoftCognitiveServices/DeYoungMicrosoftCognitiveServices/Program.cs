using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;

namespace DeYoungMicrosoftCognitiveServices
{
    class Program
    {
        static void Main(string[] args)
        {
            UserInput();
            SpellCheck();

            //There must be some sort of threading going on from some of the example API code I'm using 
            //because my Translate method kept happening before the SpellCheck method ended.
            //I tried to find a way to keep my Translate method in the main method, but my solution only works sometimes,
            //so I ended up putting that method at the end of spell check

            //while (actualWord == "")
            //{
            //    if (actualWord != "")
            //    {
            //        TranslateText();
            //    }
            //}
            

            Console.ReadLine();
        }

        #region Variables

        //Spell Check API Variables
        static string host = "https://api.cognitive.microsoft.com";
        static string path = "/bing/v7.0/spellcheck?";
        static string market = "en-US";
        static string mode = "proof";
        //Need Subscription Key -- This is the one I used but it will expire
        static string key = "b6bbe2c43bb7432399cf3e1129294f0e";

        //Translator API Variables
        static string host2 = "https://api.microsofttranslator.com";
        static string path2 = "/V2/Http.svc/Translate";
        //Need a valid key for API to work
        static string key2 = "48c0322f629c4f5cb61407b58e18b5e0";

        static string text = "";
        static string fixedWord = "";
        static string actualWord = "";
        //static string chosenLanguage = "";
        static List<string> suggestions = new List<string>();
        static int counter = 0;

        #endregion

        #region UserInput
        static void UserInput()
        {
            bool isWord = false;
            int counter;
            string word = "", letters = "abcdefghijklmnopqrstuvwxyz";
            //char[] wordArray, letterArray = letters.ToCharArray();



            while (isWord == false)
            {
                Console.Write("Type in a word to be spell checked: ");
                word = Console.ReadLine().ToLower();
                //wordArray = word.ToCharArray();
                Console.WriteLine();

                isWord = true;

                foreach (char character in word)
                {
                    counter = 0;
                    foreach (char letter in letters)
                    {
                        if (character == letter)
                        {
                            break;
                        }
                        if (character == ' ' || counter >= 25)
                        {
                            isWord = false;
                            break;
                        }
                        counter++;
                    }
                    if (isWord == false)
                    {
                        break;
                    }
                }
            }

            text = word;
        }

        #endregion

        #region SpellCheckAPI
        async static void SpellCheck()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);

            HttpResponseMessage response = new HttpResponseMessage();
            string uri = host + path;

            List<KeyValuePair<string, string>> values = new List<KeyValuePair<string, string>>();
            values.Add(new KeyValuePair<string, string>("mkt", market));
            values.Add(new KeyValuePair<string, string>("mode", mode));
            values.Add(new KeyValuePair<string, string>("text", text));

            using (FormUrlEncodedContent content = new FormUrlEncodedContent(values))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                response = await client.PostAsync(uri, content);
            }

            //This string holds the entire request given by the API
            string contentString = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(contentString);
            GetSuggestion(contentString);
        }

        static void GetSuggestion(string content)
        {
            StringBuilder sb = new StringBuilder();
            bool wordFound = false, suggestionFound = false, textCorrect = true;
            //string fixedWord = "";

            //All API responses that do not correct the given word have a lenght of 44
            //This checks to see if the given word is already correct
            if (content.Length > 44)
            {
                textCorrect = false;
                foreach (char ch in content)
                {
                    //If the character is a quotation mark, this either begins or ends the building of the string
                    if (ch == '"')
                    {
                        //The first quotation mark starts building the string
                        if (wordFound == false)
                        {
                            sb.Clear();
                            wordFound = true;
                        }
                        //The second quotation mark ends the building of the string
                        //If the word was marked as a suggestion, it gets added to the list of suggestions
                        else
                        {
                            if (suggestionFound == true)
                            {
                                fixedWord = sb.ToString();
                                suggestions.Add(fixedWord);
                                suggestionFound = false;
                                //break;
                            }
                            wordFound = false;
                        }
                    }
                    //After the first quotation mark, begins adding letters to the string
                    if (wordFound == true && ch != '"')
                    {
                        sb.Append(ch);
                    }

                    //After the last quotation mark, checks to see if string was a given suggestion
                    if (wordFound == false && sb.ToString() == "suggestion")
                    {
                        suggestionFound = true;
                    }



                }
            }

            //If the word did not get any suggestions, output the word as correct
            if (textCorrect == true)
            {
                Console.WriteLine("The word {0} is spelled correctly", text);
                actualWord = text;
            }

            //If the word did get suggestions, print all suggestions
            if (textCorrect == false)
            {
                foreach (string word in suggestions)
                {
                    Console.WriteLine("A suggested spelling for your word is: {0}", word);
                }
                actualWord = suggestions[0];
            }

            TranslateText();

        }
        #endregion

        #region TranslatorAPI
        async static void TranslateText()
        {
            //I was going to allow the user to choose one of three languages to translate the word into,
            //but I decided to just print the word in all three languages

            //while (language == 0)
            //{
            //    Console.WriteLine("1) French");
            //    Console.WriteLine("2) German");
            //    Console.WriteLine("3) Spanish");
            //    Console.Write("Translate to: ");
            //    language = Convert.ToInt32(Console.ReadLine());

            //    switch (language)
            //    {
            //        case 1:
            //            chosenLanguage = "French: ";
            //            break;
            //        case 2:
            //            chosenLanguage = "German: ";
            //            break;
            //        case 3:
            //            chosenLanguage = "Spanish: ";
            //            break;
            //        default:
            //            language = 0;
            //            Console.Clear();
            //            break;
            //    }
            //}



            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key2);

            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string> (actualWord, "fr-fr"),
                new KeyValuePair<string, string> (actualWord, "de-de"),
                new KeyValuePair<string, string> (actualWord, "es-es")
                
            };

            foreach (KeyValuePair<string, string> i in list)
            {
                string uri2 = host2 + path2 + "?to=" + i.Value + "&text=" + System.Net.WebUtility.UrlEncode(i.Key);

                HttpResponseMessage response = await client.GetAsync(uri2);

                //Displays the word that was put into spell check in three different languages - French, German, Spanish
                Console.WriteLine();
                counter++;
                switch (counter)
                {
                    case 1:
                        Console.Write("French: ");
                        break;
                    case 2:
                        Console.Write("German: ");
                        break;
                    case 3:
                        Console.Write("Spanish: ");
                        break;
                    default:
                        Console.WriteLine("Not Working");
                        break;

                 } 

                string result = await response.Content.ReadAsStringAsync();
                var content2 = XElement.Parse(result).Value;
                Console.WriteLine(content2);

            }
        }
        #endregion
    }
}
