# quickup - quick backup tool

[![NuGet](https://img.shields.io/nuget/v/quickup.svg)](https://www.nuget.org/packages/quickup/) [![NuGet](https://img.shields.io/nuget/dt/quickup.svg)](https://www.nuget.org/stats/packages/quickup?groupby=Version) [![Twitter Follow](https://img.shields.io/twitter/follow/Sergio0694.svg?style=flat&label=Follow)](https://twitter.com/SergioPedri)

![quickup](https://i.imgur.com/rDSM31g.gif)

A .NET Core 2.1 CLI tool to create one-way backups from one folder to another. This can be used to keep an up-to-date copy of a given folder on another hard-drive.

## Installing from DotGet

Make sure to get the [.NET Core 2.1 SDK](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300), then just run this command:

```
dotnet tool install quickup -g
```

And that's it, you're ready to go!

## Quick start

**quickup** is super easy to use: just pick the source and target folders and you-re ready to go.

Other options include:
* `-i` | `--include`: a list of file extensions to use to filter the files in the source directory.
* `-e` | `--exclude`: an optional list of file extensions to ignore (this option and `include` are mutually exclusive).
* `--ignore-dir`: an optional list of directory names to ignore from the source tree.
* `-p` | `--preset`: An optional preset to quickly filter common file types [documents|images|music|videos|code|VS|UWP].
* `-b` | `--beep`: play a short feedback sound when the requested operation completes.
* `-v` | `--verbose`: display additional info after analyzing the source directory.
* `--id`: an optional backup id, to keep multiple backups in the same directory with different versions.
* `--source-current`: use the current working directory as the source path.
* `--multithread` : automatically parallelize the backup creation on the available CPU threads.
* `--threads` : when combined with `multithread`, specifies the maximum number of threads to use.

### Examples

Create a backup of folder A on another drive, notify when the operation finishes and play a notification sound:

```
quickup -s C:\Users\MyName\Documents\A -t D:\Backups -b -v
```

Backup the current directory on another drive, using the Visual Studio preset (ignores \.vs, \bin, \obj folders):

```
quickup --source-current -t D:\Backups -p vs
```

## Dependencies

The libraries use the following libraries and NuGet packages:

* [CommandLineParser](https://www.nuget.org/packages/commandlineparser/)
* [JetBrains.Annotations](https://www.nuget.org/packages/JetBrains.Annotations/)
