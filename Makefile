SLN=src/LpjGuess.sln

.PHONY: clean build run

build:
	dotnet build $(SLN)

clean:
	dotnet clean $(SLN)

run:
	dotnet run --project src/LpjGuess.Frontend $(SLN)
