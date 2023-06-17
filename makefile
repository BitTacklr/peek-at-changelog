ci:
	dotnet tool restore
	dotnet restore
	dotnet build --configuration Release --no-restore
	dotnet test --configuration Release --no-build --no-restore TryOutPeekAtChangelog --logger "trx;logfilename=tests.trx"

cd:
	dotnet tool restore
	dotnet restore
	dotnet build --configuration Release --no-restore
	dotnet pack --configuration Release --no-build --no-restore --include-symbols --include-source PeekAtChangelog/PeekAtChangelog.fsproj -o .artifacts/
	dotnet nuget push .artifacts/*.nupkg --api-key $(NUGET_APIKEY) --source https://api.nuget.org/v3/index.json --skip-duplicate --no-symbols

format:
	dotnet fantomas .