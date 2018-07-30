# Quiver Engine


### Features!

  - Complete abstraction of the game from the engine! (a single engine build can be used on many, many games)
  - A whole lot more..


### Tech

QUIVER is a raycasting video game, and being such a ancient and magical technique is based upon implementation and algorithms described in the following resources:

* [Lode's Computer Graphics Blog] - A look at raycasting and a examplar C implementation.


### Getting Started

#####Quiver requires SFML.Net, so go ahead and copy the binaries into the lib/ folder.

Provided in this source are a number of Visual Studio solutions. Find engine.sln and build all.

Make any changes to the game module project (denoted as 'game'), and before compiling, change the output directory to a new subdirectory of your naming. Compile and copy any/all assets from the quiver directory.

In order to load the game, run the following windows command inside of the engine directory, where 'cstrike' is the name of the game folder.

```sh
quiver.exe +game cstrike
```


### Building
For any changes to the source, the corresponding modules must be built. As a safety (to prevent hair-ripping oversights) feature, use the 'Build all' option in Visual Studio where able.


### Todos

 - Write MORE


[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)


   [Lode's Computer Graphics Blog]: <https://lodev.org/cgtutor/raycasting.html>
