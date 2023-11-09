# OpenMcDesktop
A desktop native client for the open-mc game, written with SFML and C#. Aims to provide almost 1:1 functionality with web
client, with the goal being to support platforms, such as outdated browser engines or mobile, where the game would not be
able to run otherwise.

![image](https://user-images.githubusercontent.com/73035340/226137372-7dfd48f6-5d94-46fe-9763-a9cacb168030.png)

### Dpendencies:
All dependencies should be included with this project via the csproj.
 - There is a known bug in SFML.NET that may cause the csfml packages to appear as unavailable on linux systems due to
the `csfml` library not being bundled into the C# package. This is being tracked and can be rectified with the solutions
linked in [this github issue](https://github.com/SFML/SFML.Net/issues/197).
 - There is also a known bug in webview_csharp that may cause similar broken behaviour, due to the `webkit2gtk` libraries
not being bundled into the C# package. This is being tracked and can be rectified with the solutions
linked in [this github issue](https://github.com/webview/webview_csharp/issues/9).
- There is an unverified bug where `libFLAC.so.8` is currently needed by sfml-audio. If you have a similarly compatible
version. `ln` can be used on linux to create a symlink to the similarly compatible version
`sudo ln -s /usr/lib/libFLAC.so.XX /usr/lib/libFLAC.so.8`. Otherwise a v8 compatible version will be required
[AUR version](https://aur.archlinux.org/packages/flac1.3).

### Native depdendencies:
All native library depdencies are included as git submodules on this repository, however prebuilt versions of these libraries are provided and dynamically linked for ease within the [NativeLibraries](OpenMcDesktop/Resources/NativeLibraries/) directory.
 - Motion should be built on linux with `cd Motion/Motion/ && mkdir build && cd build && cmake -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=TRUE -DMOTION_LINK_SFML_STATICALLY=FALSE .. && make` to ensure it will work properly.
   - See the motion repo to confirm you have all the necessary dependencies to compile the library, for example `sfml` and `ffmpeg` packages may be rquired for sucessful compilation.
   - Motion will require libsfml-graphics.so.2.5, libsfml-audio.so.2.5, libsfml-window.so.2.5
 - For cross compiling native libraries, an x86_64 minGW compilation toolchain is provided in [CrossWindowsToolchain.cmake](CrossWindowsToolchain.cmake).
   - Run cmake in the openmc desktop root directory with the additional flags `-DCMAKE_TOOLCHAIN_FILE=CrossWindowsToolchain.cmake /path/to/library` in order to cross compile a library.
   - Keep note that this has only been tested on arch linux, with the `mingw-w64-gcc` package being required to compile with the toolchain.
   - For example, to compile motion, the command should look like `export CC=x86_64-w64-mingw32-gcc && cmake -DCMAKE_TOOLCHAIN_FILE=CrossWindowsToolchain.cmake Motion/Motion/ -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=TRUE -DMOTION_LINK_SFML_STATICALLY=FALSE` 

### TODO:
 - Dynamically link motion to .NET SFML.NET dependencies