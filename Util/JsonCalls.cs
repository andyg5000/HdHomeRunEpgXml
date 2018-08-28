//  Fairfield Tek L.L.C.
//  Copyright (c) 2016, Fairfield Tek L.L.C.
//  
//  
// THIS SOFTWARE IS PROVIDED BY FairfieldTek LLC ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL WINTERLEAF ENTERTAINMENT LLC BE LIABLE FOR ANY DIRECT, INDIRECT, 
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND 
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
// DAMAGE. 
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HdHomeRunEpgXml.Data;
using Newtonsoft.Json;

namespace HdHomeRunEpgXml.Util
{
    public static class JsonCalls
    {
        public static List<HdConnectChannel> GetHdConnectChannelPrograms(string deviceAuth, string guideNumber, double maxTimeStamp)
        {
            var uri = new Uri("http://my.hdhomerun.com/api/guide.php?DeviceAuth=" + deviceAuth + "&Channel=" + guideNumber + "&Start=" + maxTimeStamp +
                              "&SynopsisLength=160");
            var hc = new HttpClient();
            var result = hc.GetByteArrayAsync(uri).Result;
            string json = Encoding.UTF8.GetString(result);
            return JsonConvert.DeserializeObject<List<HdConnectChannel>>(json);
        }

        public static List<HdConnectChannel> GetHdConnectChannels(this HdConnectDiscover discover)
        {
            var uri = new Uri("http://my.hdhomerun.com/api/guide.php?DeviceAuth=" + discover.DeviceAuth);
            var hc = new HttpClient();
            var result = hc.GetByteArrayAsync(uri).Result;
            string json = Encoding.UTF8.GetString(result);
            return JsonConvert.DeserializeObject<List<HdConnectChannel>>(json);
        }

        public static List<HdConnectDevice> GetHdConnectDevices()
        {
            string json = "";
            try
            {
                var uri = new Uri("http://my.hdhomerun.com/discover");
                var hc = new HttpClient();
                byte[] result = hc.GetByteArrayAsync(uri).Result;
                
                
                json = Encoding.UTF8.GetString(result);
                return JsonConvert.DeserializeObject<List<HdConnectDevice>>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Response from Webserver: ");
                Console.WriteLine(json);
                throw;
            }
            
        }

        public static HdConnectDiscover GetHdConnectDiscover(this HdConnectDevice device)
        {
            var uri = new Uri(device.DiscoverURL);
            var hc = new HttpClient();
            var result = hc.GetByteArrayAsync(uri).Result;
            string json = Encoding.UTF8.GetString(result);
            return JsonConvert.DeserializeObject<HdConnectDiscover>(json);
        }
    }
}