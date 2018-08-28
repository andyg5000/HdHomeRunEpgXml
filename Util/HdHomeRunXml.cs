//  Fairfield Tek L.L.C.
//  Copyright (c) 2016, Fairfield Tek L.L.C.
//  
//  
// THIS SOFTWARE IS PROVIDED BY FairfieldTek LLC ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL FAIRFIELDTEK LLC BE LIABLE FOR ANY DIRECT, INDIRECT, 
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
using System.Xml;
using HdHomeRunEpgXml.Data;

namespace HdHomeRunEpgXml.Util
{
    public static class HdHomeRunXml
    {
        private const string DateFormat = "yyyyMMddHHmmss";

        public static string DateTimeToEpisode()
        {
            var dt = DateTime.Now;
            string season = dt.ToString("yyyy");
            string episode = dt.ToString("MMddhhmmss");
            return season + " . " + episode + " . 0/1";
        }

        public static string DatetimeToEpisodeFriendly()
        {
            var dt = DateTime.Now;
            string season = dt.ToString("yyyy");
            string episode = dt.ToString("MMddhhmmss");
            return "S" + season + "E" + episode;
        }

        public static XmlElement LoadShow(this XmlDocument doc, string guidName, HdConnectProgram program)
        {
            Console.WriteLine("Loading Program: " + program.Title);

            //Create the entry for the program
            var eleProgram = doc.CreateElement(string.Empty, "programme", string.Empty);
            eleProgram.SetAttribute("start", Time.UnixTimeStampToDateTime(program.StartTime).ToLocalTime().ToString(DateFormat) + " " + Time.GetOffset());
            eleProgram.SetAttribute("stop", Time.UnixTimeStampToDateTime(program.EndTime).ToLocalTime().ToString(DateFormat) + " " + Time.GetOffset());
            eleProgram.SetAttribute("channel", guidName);

            //Add the title
            var eleTitle = doc.CreateElement(string.Empty, "title", string.Empty);
            eleTitle.SetAttribute("lang", "en");
            var eleTitleText = doc.CreateTextNode(program.Title);
            eleTitle.AppendChild(eleTitleText);
            eleProgram.AppendChild(eleTitle);

            //Add a sub-title
            var eleEpisodeTitle = doc.CreateElement(string.Empty, "sub-title", string.Empty);
            eleEpisodeTitle.SetAttribute("lang", "en");
            var eleEpisodeTitleText = doc.CreateTextNode(program.EpisodeTitle);
            eleEpisodeTitle.AppendChild(eleEpisodeTitleText);
            eleProgram.AppendChild(eleEpisodeTitle);

            //Add a Description 
            var eleDesc = doc.CreateElement(string.Empty, "desc", string.Empty);
            eleDesc.SetAttribute("lang", "en");
            var eleDescText = doc.CreateTextNode(program.Synopsis);
            eleDesc.AppendChild(eleDescText);
            eleProgram.AppendChild(eleDesc);

            //Hd home run doesn't provide credits, put an empty xml entry there.
            var eleCredits = doc.CreateElement(string.Empty, "credits", string.Empty);
            eleProgram.AppendChild(eleCredits);

            /*We need to provide the episode information in xmltv_ns for Plex to recognize it as a series,
             * but HdHomeRun doesn't provide it, so we need to derive if from there
             * Friendly version.
            */
            bool addedEpisode = false;

            if (!string.IsNullOrEmpty(program.EpisodeNumber))
            {
                var eleEpisodeNumber = doc.CreateElement(string.Empty, "episode-num", string.Empty);
                eleEpisodeNumber.SetAttribute("system", "onscreen");
                var eleEpisodeNumberText = doc.CreateTextNode(program.EpisodeNumber);
                eleEpisodeNumber.AppendChild(eleEpisodeNumberText);
                eleProgram.AppendChild(eleEpisodeNumber);

                var parts = program.EpisodeNumber.Split('E');
                string season = parts[0].Substring(1);
                string episode = parts[1];
                string v = season + " . " + episode + " . 0/1";

                var eleEpisodeNumber1 = doc.CreateElement(string.Empty, "episode-num", string.Empty);
                eleEpisodeNumber1.SetAttribute("system", "xmltv_ns");
                var eleEpisodeNumberText1 = doc.CreateTextNode(v);
                eleEpisodeNumber1.AppendChild(eleEpisodeNumberText1);
                eleProgram.AppendChild(eleEpisodeNumber1);

                //Add a category of series.
                var eleSeries = doc.CreateElement(string.Empty, "category", string.Empty);
                eleSeries.SetAttribute("lang", "en");
                var eleSeriesText = doc.CreateTextNode("series");
                eleSeries.AppendChild(eleSeriesText);
                eleProgram.AppendChild(eleSeries);

                addedEpisode = true;
            }

            if (program.OriginalAirdate > 0)
            {
                //Add when it was previously shown
                var prevShown = doc.CreateElement(string.Empty, "previously-shown", string.Empty);
                prevShown.SetAttribute("start", Time.UnixTimeStampToDateTime(program.OriginalAirdate).ToString(DateFormat) + " " + Time.GetOffset());
                eleProgram.AppendChild(prevShown);
            }

            //What image to show for the thumbnail
            var elePosterUrl = doc.CreateElement(string.Empty, "icon", string.Empty);
            elePosterUrl.SetAttribute("src", program.ImageURL);
            eleProgram.AppendChild(elePosterUrl);

            //Just put stereo, HdHomeRun doesn't provide this info
            var eleAudio = doc.CreateElement(string.Empty, "audio", string.Empty);
            var eleAudioChild = doc.CreateElement(string.Empty, "stereo", string.Empty);
            var eleAudioChildText = doc.CreateTextNode("stereo");
            eleAudioChild.AppendChild(eleAudioChildText);
            eleAudio.AppendChild(eleAudioChild);
            eleProgram.AppendChild(eleAudio);

            //Just put normal subtitles, HdHomeRun doesn't provide this information
            var eleSubTitles = doc.CreateElement(string.Empty, "subtitles", string.Empty);
            eleSubTitles.SetAttribute("type", "teletext");
            eleProgram.AppendChild(eleSubTitles);

            if (program.Filter == null)
                return eleProgram;

            bool foundMovieCategory = false;

            foreach (string filter in program.Filter)
            {
                if (filter.Equals("movies"))
                    foundMovieCategory = true;
            }

            //If it is not a Movie... then give it a series so that it doesn't show up in the movie listing...

            if (!foundMovieCategory && !addedEpisode)
            {
                var eleFE = doc.CreateElement(string.Empty, "episode-num", string.Empty);
                eleFE.SetAttribute("xmltv_ns", DateTimeToEpisode());
                eleProgram.AppendChild(eleFE);

                var eleFE1 = doc.CreateElement(string.Empty, "episode-num", string.Empty);
                eleFE1.SetAttribute("onscreen", DatetimeToEpisodeFriendly());
                eleProgram.AppendChild(eleFE1);

                var eleCategory1 = doc.CreateElement(string.Empty, "category", string.Empty);
                eleCategory1.SetAttribute("lang", "en");
                var eleCategoryText1 = doc.CreateTextNode("series");
                eleCategory1.AppendChild(eleCategoryText1);
                eleProgram.AppendChild(eleCategory1);

            }

            //For each entry in filter, add it as a category
            foreach (string filter in program.Filter)
            {
                var eleCategory = doc.CreateElement(string.Empty, "category", string.Empty);
                eleCategory.SetAttribute("lang", "en");
                var eleCategoryText = doc.CreateTextNode(filter.ToLower());
                eleCategory.AppendChild(eleCategoryText);
                eleProgram.AppendChild(eleCategory);
            }
            return eleProgram;
        }

        public static List<XmlElement> ProcessChannel(this XmlDocument doc, XmlElement eleTv, HdConnectChannel channel, string deviceAuth)
        {
            Console.WriteLine("Processing Channel: " + channel.GuideNumber + " : " + channel.GuideName);

            var tvShows = new List<XmlElement>();

            var eleChan = doc.CreateElement(string.Empty, "channel", string.Empty);
            eleChan.SetAttribute("id", channel.GuideName);
            eleTv.AppendChild(eleChan);

            var eleDn1 = doc.CreateElement(string.Empty, "display-name", string.Empty);
            var eleDn1T = doc.CreateTextNode(channel.GuideName);
            eleDn1.AppendChild(eleDn1T);
            eleChan.AppendChild(eleDn1);

            var eleDn2 = doc.CreateElement(string.Empty, "display-name", string.Empty);
            var eleDn2T = doc.CreateTextNode(channel.GuideNumber);
            eleDn2.AppendChild(eleDn2T);
            eleChan.AppendChild(eleDn2);

            if (!string.IsNullOrEmpty(channel.Affiliate))
            {
                var eleDn3 = doc.CreateElement(string.Empty, "display-name", string.Empty);
                var eleDn3T = doc.CreateTextNode(channel.Affiliate);
                eleDn3.AppendChild(eleDn3T);
                eleChan.AppendChild(eleDn3);
            }

            if (!string.IsNullOrEmpty(channel.ImageURL))
            {
                var eleImageUrl = doc.CreateElement(string.Empty, "icon", string.Empty);
                eleImageUrl.SetAttribute("url", channel.ImageURL);
                eleChan.AppendChild(eleImageUrl);
            }

            double maxTimeStamp = 0;
            foreach (var program in channel.Guide)
            {
                tvShows.Add(LoadShow(doc, channel.GuideName, program));
                if (program.EndTime > maxTimeStamp)
                    maxTimeStamp = program.EndTime;
            }

            //Move the timestamp forward one second to start next hour
            maxTimeStamp++;

            int counter = 0;

            try
            {
                //Each request represents 4 hours, so this will fetch 25 * 4 or 100 hours of programming
                while (counter < 24)
                {
                    //Request the next programming for the channel

                    var moreProgramming = JsonCalls.GetHdConnectChannelPrograms(deviceAuth, channel.GuideNumber, maxTimeStamp);
                    //Add the shows
                    foreach (var program in moreProgramming[0].Guide)
                    {
                        tvShows.Add(LoadShow(doc, channel.GuideName, program));
                        if (program.EndTime > maxTimeStamp)
                            maxTimeStamp = program.EndTime;
                    }
                    counter++;
                    //Move the timestamp forward one second to start next hour
                    maxTimeStamp++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("!!!!!!!!!!It appears you do not have the HdHomeRun Dvr service.!!!!!!!!!!");
            }

            return tvShows;
        }
    }
}