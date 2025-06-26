SLN=src/LpjGuess.sln

.PHONY: clean build run check coverage

build:
	dotnet build $(SLN)

clean:
	dotnet clean $(SLN)
	rm -rf coverage src/LpjGuess.Tests/TestResults

run:
	dotnet run --project src/LpjGuess.Frontend $(SLN)

check:
	dotnet test --collect:"XPlat Code Coverage" $(SLN)

# dotnet tool install -g dotnet-reportgenerator-globaltool
coverage: check
	reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
	xdg-open coverage/index.html