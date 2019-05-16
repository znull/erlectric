# Developers Guide

## Buind Environment Setup

### Windows-Mono
Follow only if you're a cli-geek.
> NOTE: Commands below uses [chocolatey project](https://chocolatey.org) to install packages on windows environment
* install `mono`  
```
choco install -y mono --version 5.20.1.19
```
* Install nunit3 console runner   
```
choco install -y nunit-console-runner --version 3.10.0
```
* Install `nuget` binaries for command line    
```
choco install -y nuget.commandline --version 5.0.2
```
* Install `msys2` binaries to run make. 
> **NOTE:** You may also need to update path enviroment to use gnu tools from windows command line    
```
choco install -y msys2
# Upgradle to latest binaries distribution
msys2_shell.cmd -msys -c "pacman -S -u --noconfirm"
# install make
msys2_shell.cmd -msys -c "pacman -S -y --noconfirm msys/make"
```

### Windows-VisualStudio
* Install visual studio (Latest verified version in `2017 Community`)
* Follow instructions to download additionl worksets
* Open `Erlectric.sln`
* ...
* GL HF

## Build
```
make
```
