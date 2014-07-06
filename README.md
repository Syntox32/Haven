Haven
=====

A class for downloading wallpapers from http://alpha.wallhaven.cc<br>
The site is currently in `Alpha` so changes will be made.

A fully working example is provided in `Program.cs`<br>

Config
------

For the class to build using a Json config you need to configure the following template:
```
{
	"SaveLocation": "",
	"Url": "http://alpha.wallhaven.cc/wallpaper/search?categor...",
	"Pages": 1,
	"PageOffset": 0,
	"MinWidth": 1200,
	"MinHeight": 1920,
	"Username": "",
	"Password": ""
}
```

It is worth noting the authentication on the site does <strong>not</strong> carry over HTTPS

<h4>Getting a Url is pretty straight forward:</h4>
 - Go to the site and adjust the filter to your liking
 - Refresh and copy the Url into the config
 - ???
 - Profit

Requirements
------------
 Both <strong>HtmlAgilityPack</strong> and <strong>Newtonsoft.Json</strong> are required for the class to build
 
 If you choose to download the project, NuGet comes configured to download the required packages on the first build.
 
<blockquote>Make sure to have <a href="http://www.microsoft.com/en-us/download/details.aspx?id=30653">.NET 4.5 </a> or newer installed</blockquote> 
 
License
-------
See the GPLv3 license provided with this repo
