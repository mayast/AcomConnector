﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ICMSConnector.Interfaces
{
    interface IHandoffManager
    {
        Task<string> Handoff(HttpClient httpClient);
    }
}