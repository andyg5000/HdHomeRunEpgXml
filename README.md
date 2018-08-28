# HdHomeRunEpgXml #
# This program is provided with no warrenty and is for educational purposes only. #

This program downloads the programming for your connected HdHomeRun devices and converts it to a TVXML file which you can then use on Plex or any other Media Center.  To recieve more than 4 hours of programming you must subscribe to SiliconDust's DVR service which provides the extended EPG data that this program uses.

By default, the program will download **Two Weeks** of programming.

**For Windows**
  To run the program on windows, either download and compile the program (Requires Visual Studio 2017 or greater) or download the Release.Zip file in the project.
  
**For Linux/Others**
  The file HdHomeRun.py is a python 3.7 program which does is a translation of the C# program.  This python program require Pip to be installed as well as the urllib3 library.
 
**For Support, visit https://forums.plex.tv/t/for-those-who-purchase-the-hdhomerun-primium-live-video-service-this-is-how-you-can-get-your-epg/300886/69

**Note:**  It is recommended that you schedule the download of the EPG only ONCE per day, plex will read the TVXML file during maintenance in the middle of the night.  On Linux, use crontab -e to schedule, and for windows use the System Task Scheduler.

  Fairfield Tek L.L.C.
  Copyright (c) 2016, Fairfield Tek L.L.C.
  
  
 THIS SOFTWARE IS PROVIDED BY WINTERLEAF ENTERTAINMENT LLC ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
 PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL WINTERLEAF ENTERTAINMENT LLC BE LIABLE FOR ANY DIRECT, INDIRECT, 
 INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND 
 ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
 OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
 DAMAGE. 
 
 Licensed under the Apache License, Version 2.0 (the "License");
 you may not use this file except in compliance with the License.
 You may obtain a copy of the License at
 
     http://www.apache.org/licenses/LICENSE-2.0
 
 Unless required by applicable law or agreed to in writing, software
 distributed under the License is distributed on an "AS IS" BASIS,
 WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 See the License for the specific language governing permissions and
 limitations under the License.
