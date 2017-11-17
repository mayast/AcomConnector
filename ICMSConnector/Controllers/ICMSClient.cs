﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ICMSConnector.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TranslationService.Models;

namespace ICMSConnector.Controllers
{
    class IcmsClient: IICMSClient
    {
        public async Task<string> UploadFile(HttpClient httpClient)
        {
            string path = (@"Files\Source\test.txt"); //Path of the file to be picked up, folder in bin/debug
            var bytes = File.ReadAllBytes(path);
            var icmsFileIdentifier = new IcmsFileIdentifier //ICMS supplied model to store File ID data
            {
                AssetId = "AcomTestConnectorID", //AssetID to identify file in Vantage Point (ICMS UI)
                HostFileReference = @"\\Acom\Files\Test\test.txt", //Host File Reference + Host File Revision + Host Content Store is the unqiue key to identify a file in ICMS
                Locale = "en-us", //Source file locale
                HostFileRevision = 0, //ICMS supports storing different versions of files, should be mapped to the version of file in source location
                ContentType = "Article", //The type of file, can be Article, Art, Token,Manifest, Video
            };

            var icmsMetadata = new IcmsFileMetadata //Model for the metdata about the file
            {
                FileIdentifier = icmsFileIdentifier, //Set File ID data 
                Localizable = true, 
                ContentGroup = "Pilot2HO1_IA_CP66", //High level file grouping in ICMS
                Priority = "1",
                HostFileTags = "exampleTags", //Localization priority
            };

            var icmsFile = new IcmsFile //Model for the file to be uploaded to iCMS
            {
                Content = bytes,
                Metadata = icmsMetadata //Set metadata
            };

            var data = JsonConvert.SerializeObject(icmsFile); //Serializing file into a JSON string
            var content = new StringContent(data, Encoding.UTF8); //Create formatted string data appropriate for http server/client communication
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json"); //Specifiy content type

            var response = await httpClient.PostAsync("api/Files/", content); //Call Files API 
            var stringResponse = response.Content.ReadAsStringAsync(); //Serialize the response content
            return stringResponse.Result; //Get string from awaitable Task
        }

        public async Task<string> Ping(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync("api/Ping"); //Call the ping API to check if the translation service is up
            var responseString = response.Content.ReadAsStringAsync();
            return responseString.Result;
        }

        public async Task<string> GetFileWithStatus(HttpClient httpClient, string status)
        {
            var response = await httpClient.GetAsync(string.Format(@"api/Files/Filestatus?status={0}&count={1}", status,100)); //Get Files with status and specify count retrieved
            var responseString = response.Content.ReadAsStringAsync();
            return responseString.Result;
        }
        public async Task<string> GetLocFilesWithStatus(HttpClient httpClient, string status)
        {
            var response = await httpClient.GetAsync(string.Format(@"api/LocFiles/Locstatus?status={0}&count={1}", status, 100));//Get LocFiles with status and specify count retrieved
            var responseString = response.Content.ReadAsStringAsync();
            return responseString.Result;
        }

        public async Task<string> SetLocFileStatus(HttpClient httpClient,string status, IList<long?> fileIds)
        {
            var locFileStatus = new LocFilesStatus //Create a Loc Status object with all the fileIds that have to be updated, the status and message
            {
                LocFileIds = fileIds.ToArray(),
                LocStatus = status,
                Message = "Setting loc file status from a Test client"
            };

            var content = new StringContent(JsonConvert.SerializeObject(locFileStatus));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await httpClient.PutAsync("api/LocFiles/LocStatus", content); //Call LocStatus API
            var responseString = response.Content.ReadAsStringAsync();
            return responseString.Result;
        }

        public Task<string> GetLocFilesWithContent(HttpClient httpClient, bool locAction, int fileId)
        {
            var response = httpClient.GetAsync(string.Format(@"api/LocFiles/{0}?getLocAction={1}", fileId, locAction));
            var responseString = response.Result.Content.ReadAsStringAsync();
            return responseString;
        }
    }
}
