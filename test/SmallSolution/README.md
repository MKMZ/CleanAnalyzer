# Small Solution

Script that initialized projects:
```ps
mkdir SmallSolution
cd SmallSolution
dotnet new sln
@(
    'App'
    'DeviceConnector'
    'WebApi'
    'StorageConnector'
    'DatabaseAdapter'
    'CoreAbstractions'
) | % {
    dotnet new classlib -o $_
    dotnet sln add "$_/$_.csproj"
}
dotnet add App reference WebApi
dotnet add App reference DatabaseAdapter
dotnet add App reference DeviceConnector
dotnet add App reference StorageConnector
dotnet add DatabaseAdapter reference CoreAbstractions
dotnet add DeviceConnector reference CoreAbstractions
dotnet add StorageConnector reference CoreAbstractions
dotnet add WebApi reference CoreAbstractions
```