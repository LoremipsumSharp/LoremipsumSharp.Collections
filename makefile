PROJ=./LoremipsumSharp.Collections.sln

clean:
	rm -rf ./dist

build:clean
	dotnet build $(PROJ) -c Release
	
pack:build
	dotnet pack -c Release $(PROJ) -o ./dist

push:pack
	$(foreach file, $(wildcard ./dist/*.nupkg), dotnet nuget push $(file) -k $(NUGET_APIKEY) -s https://api.nuget.org/v3/index.json ;)
	rm -rf ./dist

test1:
	echo $(NUGET_APIKEY)