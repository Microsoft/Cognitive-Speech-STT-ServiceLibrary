﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpeechWithLuis.Src.AuthorizationProvider;
using SpeechWithLuis.Src.Static;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SpeechWithLuis.Src.Services
{
    public class SpeechRestService
    {
        private static string baseUri = "https://speech.platform.bing.com/recognize";

        private static string uriForUsing;

        static SpeechRestService()
        {
            string requestUri = baseUri.Trim(new char[] { '/', '?' });

            /* URI Params. Refer to the README file for more information. */
            requestUri += @"?scenarios=smd";                                  // websearch is the other main option.
            requestUri += @"&appid=D4D52672-91D7-4C74-8AD8-42B1D98141A5";     // You must use this ID.
            requestUri += @"&locale=en-us";                                   // We support several other languages.  Refer to README file.
            requestUri += @"&device.os=wp7";
            requestUri += @"&version=3.0";
            requestUri += @"&format=json";
            requestUri += @"&instanceid=565D69FF-E928-4B7E-87DA-9A750B96D9E3";
            requestUri += @"&requestid=" + Guid.NewGuid().ToString();

            uriForUsing = requestUri;
        }

       
        private string host = @"speech.platform.bing.com";

        private string contentType = @"audio/wav; codec=""audio/pcm""; samplerate=16000";

        //private static string subKey = "1da1bed1e00a46c5a3a953235417381c";

        public dynamic SendAudio(Stream stream)
        {
            stream.Position = 0;
            var request = (HttpWebRequest)HttpWebRequest.Create(uriForUsing);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Host = host;
            request.ContentType = contentType;
            request.Headers["Authorization"] = "Bearer " + InstanceFactory.Authentication.GetAccessToken();

            byte[] buffer = null;
            int bytesRead = 0;
            using (Stream requestStream = request.GetRequestStream())
            {
                /*
                 * Read 1024 raw bytes from the input audio file.
                 */
                buffer = new Byte[checked((uint)Math.Min(1024, (int)stream.Length))];
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }

                // Flush
                requestStream.Flush();
            }

            var responseString = "";
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                }

                //Console.WriteLine(responseString);
            }

            //return JsonConvert.DeserializeObject<dynamic>(responseString);
            return JObject.Parse(responseString);
        }

        public dynamic SendAudio(byte[] audioArray, int length)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(uriForUsing);
            request.SendChunked = true;
            request.Accept = @"application/json;text/xml";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.Host = host;
            request.ContentType = contentType;
            request.Headers["Authorization"] = "Bearer " + InstanceFactory.Authentication.GetAccessToken();
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(audioArray, 0, audioArray.Count<byte>());
                // Flush
                requestStream.Flush();
            }

            var responseString = "";
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    responseString = sr.ReadToEnd();
                }

                //Console.WriteLine(responseString);
            }

            //return JsonConvert.DeserializeObject<dynamic>(responseString);
            return JObject.Parse(responseString);
        }

        public SpeechRestService UseLocale(string locale)
        {
            uriForUsing = uriForUsing.Replace("&locale=en-us", "&locale=" + locale);
            return this;
        }

    }
}