# Quiver

An extensible pseudo-3d game engine complete scripting modularity as well as full lighting, rgb perhiperal lighting and much more. Originally written for my a-level Computer Science coursework.

![Quiver Level 2 gameplay](/screenshots/1.png)

---
### Build status

| Windows | Linux / Mac | Download |
|---------|-------------|----------|
|![](https://github.com/solwllms/quiver/workflows/build/badge.svg)| None |  |

``
A legacy build of Quiver is available for download [here](https://github.com/solwllms/quiver/releases/tag/v1.0). To play the latest build, please compile from source.
``

---

### Features

  - Decoupled game and engine! (a single engine build can be used on many different games)
  - Fully coloured lightmapped envionments
  - Map descriptor/theme dataset system
  - OpenGL front-end for internal pixel buffer
  - Fully extensible console, command & variable system
  - OpenAL 3d audio
  - Compressed data archive support
  - Keyboard-driven, extensible GUI system
  - RGB perhiperal lighting engine (using [RGB.NET](https://github.com/DarthAffe/RGB.NET))
  - (& much more)

---

### Tech and Reading

Quiver is a raycasting game engine, and being such a ancient and magical technique is based upon implementation and algorithms described in the following resources:

* [Lode's Computer Graphics Blog] - A look at raycasting and a examplar C implementation.

---

### Getting Started

``
Quiver requires OpenTK and OpenAL (and RGB.NET if you fancy), these are packaged with the solution through NuGet so should just install for you.
``

Provided in this source are a number of Visual Studio solutions. Find games.sln and build all.

Make any changes to the game module project (denoted as 'game'), and before compiling, change the output directory to a new subdirectory of your naming. Compile and copy any/all assets from the quiver directory.

In order to load the game, run the following windows command inside of the engine directory, where 'cstrike' is the name of the game folder. If there is no game argument, then the directory defaults to 'quiver/'.

```sh
quiver.exe +game cstrike
```

---

### Building
For any changes to the source, the corresponding modules must be built. As a safety (to prevent hair-ripping oversights) feature, use the 'Build all' option in Visual Studio where able.

---

### Future plans

  - Improved AI
  - Map editor
  - Better Animation system
  - Improved GUI system?
  - Multiplayer
  - Extended scripting capabilities
  - (& more)

---

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)


   [Lode's Computer Graphics Blog]: <https://lodev.org/cgtutor/raycasting.html>
