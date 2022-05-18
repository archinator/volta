#install Chocolatey
[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

#install git
choco install -y git

#install cmake
choco install -y cmake

#update env PATH variable
$env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine") + ";C:\Program Files\Cmake\bin;C:\Program Files\Git\bin;" + [System.Environment]::GetEnvironmentVariable("Path","User")

#install Visual Studio 2022 (required to run .net6 apps)
choco install -y visualstudio2022community

#install development of desktop C++ apps
choco install -y visualstudio2022-workload-nativedesktop

#install .net6 sdk
choco install -y dotnet-6.0-sdk

#install ffmpeg
choco install -y ffmpeg

cd C:\
mkdir Tools
cd ./Tools

#clone local Telegram bot api server sources
git clone --recursive https://github.com/tdlib/telegram-bot-api.git
cd telegram-bot-api
git clone https://github.com/Microsoft/vcpkg.git
cd vcpkg
./bootstrap-vcpkg.bat
./vcpkg.exe install gperf:x64-windows openssl:x64-windows zlib:x64-windows
cd ..
Remove-Item build -Force -Recurse -ErrorAction SilentlyContinue
mkdir build
cd build
cmake -A x64 -DCMAKE_INSTALL_PREFIX:PATH=.. -DCMAKE_TOOLCHAIN_FILE:FILEPATH=../vcpkg/scripts/buildsystems/vcpkg.cmake ..
cmake --build . --target install --config Release
cd ../..
dir telegram-bot-api/bin/telegram-bot-api*

#start new cmd
start cmd.exe

#start bot server
cd ./telegram-bot-api/bin
./telegram-bot-api --api-id=9521606 --api-hash=92cc0318fdfb4588448678d5fe0ff81f --local

#clone volta api sources
cd C:\
mkdir Projects
cd ./Projects

git clone https://github.com/archinator/volta.git
cd ./volta/Volta.Bot/Volta.Bot.ConsoleApp
dotnet build
dotnet run