# Written by Vincent Gee
# Derived from the work at https://forum.libreelec.tv/thread/12228-tvheadend-epg-guide-from-hdhomerun/
# 8/26/2018
#
# Description:  Downloads the EPG from HdHomeRun's server and converts it to a XMLTV format
#				So it can be loaded into Plex.

import sys, json, urllib3, datetime, subprocess,time,os
import xml.etree.cElementTree as ET
from datetime import datetime
from xml.dom import minidom
from pprint import pprint

def get_utc_offset_str():
    """
    Returns a UTC offset string of the current time suitable for use in the
    most widely used timestamps (i.e. ISO 8601, RFC 3339). For example:
    10 hours ahead, 5 hours behind, and time is UTC: +10:00, -05:00, +00:00
    """

    # Calculate the UTC time difference in seconds.

    timestamp = time.time()
    time_now = datetime.fromtimestamp(timestamp)
    time_utc = datetime.utcfromtimestamp(timestamp)
    utc_offset_secs = (time_now - time_utc).total_seconds()

    # Flag variable to hold if the current time is behind UTC.
    is_behind_utc = False

    # If the current time is behind UTC convert the offset
    # seconds to a positive value and set the flag variable.
    if utc_offset_secs < 0:
        is_behind_utc = True
        utc_offset_secs *= -1

    # Build a UTC offset string suitable for use in a timestamp.

    if is_behind_utc:
        pos_neg_prefix = "-"
    else:
        pos_neg_prefix = "+"

    utc_offset = time.gmtime(utc_offset_secs)
    utc_offset_fmt = time.strftime("%H:%M", utc_offset)
    utc_offset_str = pos_neg_prefix + utc_offset_fmt

    return utc_offset_str

def ProcessProgram(xml, program, guideName):

	WriteLog ("Processing Show: " + program['Title'])

	timezone_offset = get_utc_offset_str().replace(":","")
	#program
	#Create the "programme" element and set the Channel attribute to "GuideName" from json
	xmlProgram = ET.SubElement(xml, "programme", channel = guideName)
	#	 channel=channel['GuideName'])

	#set the start date and time from the feed
	xmlProgram.set("start", datetime.fromtimestamp(program['StartTime']).strftime('%Y%m%d%H%M%S') + " " + timezone_offset)

	#set the end date and time from the feed
	xmlProgram.set("stop", datetime.fromtimestamp(program['EndTime']).strftime('%Y%m%d%H%M%S') + " " + timezone_offset)

	#Title
	ET.SubElement(xmlProgram, "title", lang="en").text = program['Title']

		
	#Sub Title
	if 'EpisodeTitle' in program:
		ET.SubElement(xmlProgram, "sub-title", lang="en" ).text = program['EpisodeTitle']

	#Description
	if 'Synopsis' in program:
		ET.SubElement(xmlProgram, "desc").text = program['Synopsis']

	#Credits
	#We add a blank entry to satisfy Plex
	ET.SubElement(xmlProgram,"credits").text = ""

	addedEpisode = False

	if 'EpisodeNumber' in program:
		#add the friendly display
		ET.SubElement(xmlProgram, "episode-num", system="onscreen").text = program['EpisodeNumber']
		#Fake the xml version
		en = program['EpisodeNumber']
		parts = en.split("E")
		season = parts[0].replace("S","")
		episode = parts[1]
		#Assign the fake xml version
		ET.SubElement(xmlProgram, "episode-num", system="xmltv_ns").text = (season + " . " + episode  + " . 0/1")
		#set the category flag to series
		ET.SubElement(xmlProgram, "category", lang="en" ).text = "series"
		addedEpisode = True

	if 'OriginalAirdate' in program:
		ET.SubElement(xmlProgram, "previously-shown", start = datetime.fromtimestamp(program['OriginalAirdate']).strftime('%Y%m%d%H%M%S') + " " + timezone_offset)
				
	if 'ImageURL' in program:
		ET.SubElement(xmlProgram, "icon", src=program['ImageURL'])

	xmlAudio = ET.SubElement(xmlProgram,"audio")
	ET.SubElement( xmlAudio, "stereo").text = "stereo"
	ET.SubElement(xmlProgram, "subtitles", type="teletext")		


	if 'Filter' in program:
		for filter in program['Filter']:
			#Lowercase the filter... apearenttly Plex is case sensitive
			filterstringLower = str(filter).lower()
			#add the filter as a category
			ET.SubElement(xmlProgram, "category",lang="en").text = filterstringLower
			#If the filter is news or sports...
			if (filterstringLower == "news" or filterstringLower == "sports"):
				#And the show didn't have it's own episode number...
				if ( addedEpisode == False ):
					WriteLog("-------> Creating Fake Season and Episode for News or Sports show.")
					#add a category for series
					ET.SubElement(xmlProgram, "category",lang="en").text = "series"
					#create a fake episode number for it
					ET.SubElement(xmlProgram, "episode-num", system="xmltv_ns").text = DateTimeToEpisode()
					ET.SubElement(xmlProgram, "episode-num", system="onscreen").text = DateTimeToEpisodeFriendly()
	return program['EndTime']

	
def processChannel(xml, data, deviceAuth):
	
	WriteLog ("Processing Channel: " + data.get('GuideNumber') + " " + data.get('GuideName'))

	#channel
	xmlChannel = ET.SubElement(xml, "channel", id = data.get('GuideName'))
		
	#display name
	ET.SubElement(xmlChannel, "display-name").text = data.get('GuideName')
		
	#display name
	ET.SubElement(xmlChannel, "display-name").text = data.get('GuideNumber')

	#display name
	if 'Affiliate' in data:
		ET.SubElement(xmlChannel, "display-name").text = data.get('Affiliate')


	if 'ImageURL' in data:
		ET.SubElement(xmlChannel, "icon", src= data.get('ImageURL'))

	maxTime = 0
		
	for program in data.get("Guide"):
		maxTime = ProcessProgram(xml,program, data.get('GuideName'))
		
	maxTime = maxTime + 1
	counter = 0

	#The first pull is for 4 hours, each of these are 8 hours
	#So if we do this 21 times we will have fetched the complete week
	while ( counter < 21 ):
		
		chanData = GetHdConnectChannelPrograms( deviceAuth, data.get('GuideNumber'), maxTime)
		

		for chan in chanData:
			for program in chan["Guide"]:
				maxTime = ProcessProgram( xml, program, data.get('GuideName'))

		counter = counter + 1
	

				
def saveStringToFile(strData, filename):
	with open(filename, 'wb') as outfile:
		outfile.write(strData)
					
def loadJsonFromFile(filename):
	return json.load(open(filename))

def saveJsonToFile(data, filename):
	with open(filename, 'w') as outfile:
		json.dump(data, outfile, indent=4)

def GetHdConnectDevices():
	WriteLog("Getting Connected Devices.")
	http = urllib3.PoolManager()
	discover_url_response = http.request('GET',"http://my.hdhomerun.com/discover")
	data = discover_url_response.data
	WriteLog(data)
	obj = json.loads(data)
	return obj

def GetHdConnectDiscover(discover_url):
	WriteLog("Discovering...")
	http = urllib3.PoolManager()
	device_auth_response = http.request('GET',discover_url)
	data = device_auth_response.data
	WriteLog(data)
	device_auth = json.loads(data)['DeviceAuth']
	return device_auth

def GetHdConnectDiscoverLineUpUrl(discover_url):
	WriteLog("Getting Lineup Url")
	http = urllib3.PoolManager()
	device_auth_response = http.request('GET',discover_url)
	data = device_auth_response.data
	LineupURL = json.loads(data)['LineupURL']
	return LineupURL

	#public class RootObject
	#{
	#    public string GuideNumber { get; set; }
	#    public string GuideName { get; set; }
	#    public string VideoCodec { get; set; }
	#    public string AudioCodec { get; set; }
	#    public int HD { get; set; }
	#    public string URL { get; set; }
	#}	

def GetHdConnectLineUp(lineupUrl):
	WriteLog("Getting Lineup")
	http = urllib3.PoolManager()
	device_auth_response = http.request('GET',lineupUrl)
	data = device_auth_response.data
	Lineup = json.loads(data)
	return Lineup


def GetHdConnectChannels(device_auth):
	WriteLog("Getting Channels.")
	http = urllib3.PoolManager()
	response = http.request('GET',"http://my.hdhomerun.com/api/guide.php?DeviceAuth=%s" % device_auth)
	data = response.data
	WriteLog(data)
	return json.loads(data)

def GetHdConnectChannelPrograms(device_auth, guideNumber, timeStamp):
	WriteLog("Getting Extended Programs")
	http = urllib3.PoolManager()
	response = http.request('GET',"http://my.hdhomerun.com/api/guide.php?DeviceAuth=" + device_auth +"&Channel=" + guideNumber +"&Start=" + str(timeStamp) + "&SynopsisLength=160")
	data = response.data
	WriteLog(data)
	return json.loads(data)	

def InList(l , value):
	if (l.count(value)>0):
		return True
	else:
		return False		
	return False

def ClearLog():
	if os.path.exists("HdHomerun.log"):
  		os.remove("HdHomerun.log")
	if os.path.exists("hdhomerun.xml"):
  		os.remove("hdhomerun.xml")		  

def DateTimeToEpisode():
	timestamp = time.time()
	time_now = datetime.fromtimestamp(timestamp)
	season = time_now.strftime('%Y')
	episode = time_now.strftime('%m%d%H')
	return (season + " . " + episode  + " . 0/1")

def DateTimeToEpisodeFriendly():
	timestamp = time.time()
	time_now = datetime.fromtimestamp(timestamp)
	season = time_now.strftime('%Y')
	episode = time_now.strftime('%m%d%H')
	return ("S" + season + "E" + episode)

def WriteLog(message):
	timestamp = time.time()
	time_now = datetime.fromtimestamp(timestamp)
	timeString = time_now.strftime('%Y%m%d%H%M%S')

	with open ('HdHomerun.log','a') as logfile:
		logfile.write(str(timeString) + " " + str(message) + "\n")
	#print(str(timeString) + " " + str(message))


def main():
	ClearLog()
	print("Downloading Content...  Please wait.")
	print("Check the log for progress.")
	WriteLog("Starting...")

	xml = ET.Element("tv")
	
	devices = GetHdConnectDevices()

	processedChannelList = ["empty","empty"]

	for device in devices: 

		if 'DeviceID' in device:

			WriteLog("Processing Device: " + device["DeviceID"])

			deviceAuth = GetHdConnectDiscover(device["DiscoverURL"])

			lineUpUrl = GetHdConnectDiscoverLineUpUrl(device["DiscoverURL"])

			LineUp = GetHdConnectLineUp(lineUpUrl)

			if ( len(LineUp) > 0):
				WriteLog("Line Up Exists for device")
				channels = GetHdConnectChannels(deviceAuth)
				for chan in channels:
					ch =str( chan.get('GuideName') )
					if (InList( processedChannelList, ch) == False):
						WriteLog ("Processing Channel: " + ch)
						processedChannelList.append(ch)
						processChannel( xml, chan, deviceAuth)
					else:
						WriteLog ("Skipping Channel " + ch + ", already processed.")
			else:
				WriteLog ("No Lineup for device!")
		else:
			WriteLog ("Must be storage...")
	
	reformed_xml = minidom.parseString(ET.tostring(xml))
	xmltv = reformed_xml.toprettyxml(encoding='utf-8')	
	WriteLog ("Finished compiling information.  Saving...")	
	saveStringToFile(xmltv, "hdhomerun.xml")
	WriteLog ("Finished.")
 
if __name__== "__main__":
  main()
