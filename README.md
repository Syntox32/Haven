Haven
=====

Thing created for batch downloading wallpapers from http://alpha.wallhaven.cc.

The site is currently in `alpha` so things will change and stuff will break.

In this repo you will find the following:

| Project | Description | Status              
| --- | --- | ---|
| Haven.Cli | Command-line-interface version, usefull for scripts. | `working`           
| Haven.Forms | A more user friendly graphical interface. | `work in progress`  
| Haven.Core | Main haven core framework, used in the other projects. | `working`          

Download
--------

For now only binaries for the CLI version is available.

[You cant get it here, under releases](https://github.com/Syntox32/Haven/releases)

Usage
-----

#### Acquiring a filter URL
Wallpapers are downloaded through a filter URL, which you can
configure on the site.

 1. Go to wallhaven and [search](http://alpha.wallhaven.cc/latest)
 2. Set the desired parameters, if you choose none that's fine.
 3. Click the blue spinning refresh icon.
    ![](http://i.imgur.com/WTVhaTU.png)
 4. A new URL will appear in your address bar.
    ![](http://i.imgur.com/YW7sbla.png)
 4. Paste your new filter URL into the config or the winforms app.
 5. Download all your new magnificant wallpapers.

#### Using the CLI

The CLI is config based and a config path will have to be passed as the first argument.

```
havencli.exe "c:/path/to/config.yaml"
```

At startup the program will try to locate a file called `config.yaml` in the execute 
directory if no config path was passed as an argument. If it does not find any config
it will terminate.

#### Using the GUI

 step 1: Point and click.
 
#### Configuring the config

Configs are written in YAML. 

An example of this comes with the repo under the name `config.example.yaml`, you may edit 
these properties as you wish. The properties of the config are explained in the example config.

When the CLI version starts up it will search the current directory for a file name `config.yaml`
if no config is given.

Before configuring a username and password, it is worth noting 
the authentication on the site does *not* carry over HTTPS.

Requirements
------------
If you choose to download the project, NuGet will download the required packages automagically.

Both <strong>HtmlAgilityPack</strong> and <strong>YamlDotNet</strong> are required for the `Haven.Core` project to build.
 
Make sure to have `.NET Framework v4.5.2` or newer installed.

License
-------

The whole thing is licenced under an MIT license.
