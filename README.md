# OpenMcDesktop
A desktop native client for the open-mc game, written with SFML and C#. Aims to provide almost 1:1 functionality with web client, with the goal being to support platforms, such as outdated browser engines or mobile, where the game would not be able to run otherwise.

![image](https://user-images.githubusercontent.com/73035340/226137372-7dfd48f6-5d94-46fe-9763-a9cacb168030.png)

### Native depdendencies:
All native library depdencies are included as git submodules on this repository, however prebuilt versions of these libraries are provided and dynamically linked for ease within the [NativeLibraries](OpenMcDesktop/Resources/NativeLibraries/) directory.
 - Motion should be built on linux with `cd Motion/Motion/ && mkdir build && cd build && cmake -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=TRUE -DMOTION_LINK_SFML_STATICALLY=FALSE .. && make` to ensure it will work properly.
   - See the motion repo to confirm you have all the necessary dependencies to compile the library, for example `sfml` and `ffmpeg` packages may be rquired for sucessful compilation.
 - For cross compiling native libraries, an x86_64 minGW compilation toolchain is provided in [CrossWindowsToolchain.cmake](CrossWindowsToolchain.cmake).
   - Run cmake in the openmc desktop root directory with the additional flags `-DCMAKE_TOOLCHAIN_FILE=CrossWindowsToolchain.cmake /path/to/library` in order to cross compile a library.
   - Keep note that this has only been tested on arch linux, with the `mingw-w64-gcc` package being required to compile with the toolchain.
   - For example, to compile motion, the command should look like `export CC=x86_64-w64-mingw32-gcc && cmake -DCMAKE_TOOLCHAIN_FILE=CrossWindowsToolchain.cmake Motion/Motion/ -DCMAKE_BUILD_TYPE=Release -DBUILD_SHARED_LIBS=TRUE -DMOTION_LINK_SFML_STATICALLY=FALSE`
