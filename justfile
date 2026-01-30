FW := "net10.0"

# Run static analysis for the project (check CA rule violations)
analyze:
    dotnet build EasyVCR/EasyVCR.csproj -c Release -t:Rebuild -restore -p:EnableNETAnalyzers=true -p:CodeAnalysisTreatWarningsAsErrors=true -p:RunAnalyzersDuringBuild=true -p:AnalysisLevel=latest -p:AnalysisMode=Minimum

# Build the project in Debug mode
build:
    dotnet build EasyVCR/EasyVCR.csproj -c Debug -t:Rebuild -restore -p:EnableNETAnalyzers=false

# Build the project in Debug mode for a specific framework
build-fw fw=FW:
    dotnet build EasyVCR/EasyVCR.csproj -c Debug -t:Rebuild -restore -f {{fw}} -p:EnableNETAnalyzers=false

# Build the project in Release mode
build-prod:
    dotnet build EasyVCR/EasyVCR.csproj -c Release -t:Rebuild -restore -p:EnableNETAnalyzers=false

# Clean the project
clean:
    dotnet clean

# Generate coverage reports (unit tests, not integration) for the project
coverage:
    ./scripts/unix/generate_test_reports.sh

# Check if the coverage is above the minimum threshold
coverage-check:
    ./scripts/unix/check_coverage.sh 85

# Generates library documentation
docs:
    dotnet tool run docfx docs/docfx.json

# Install required dotnet tools
install-tools:
    dotnet new tool-manifest --force || exit 0
    dotnet tool install --local security-scan --version 5.6.3 || exit 0
    dotnet tool install --local dotnet-format || exit 0
    dotnet tool install --local docfx --version 2.60.2 || exit 0

# Install requirements
install: install-tools

# Lints the solution (EasyVCR + Tests + F#/VB compatibilities) (check IDE and SA rule violations)
lint fw=FW:
    dotnet tool run dotnet-format --no-restore --check
    dotnet build EasyVCR/EasyVCR.csproj -c Linting -t:Rebuild -restore -p:EnforceCodeStyleInBuild=true -f {{fw}}

# Formats the project
lint-fix:
    dotnet tool run dotnet-format --no-restore

# Lint and validate the Batch scripts (Windows only)
lint-scripts:
    scripts\win\lint_scripts.bat

# Publish the project to NuGet
# key: The NuGet API key to use for publishing.
publish key:
    dotnet nuget push *.nupkg --source https://api.nuget.org/v3/index.json --api-key {{key}} --skip-duplicate

# Cuts a release for the project on GitHub (requires GitHub CLI)
# tag: The associated tag title of the release
# target: Target branch or full commit SHA
release tag target:
    gh release create {{tag}} --target {{target}}

# Restore the project
restore:
    dotnet restore

# Scan the solution (EasyVCR + Tests + F#/VB compatibilities) for security issues (must run install-scanner first)
scan:
    dotnet tool run security-scan --verbose --no-banner --ignore-msbuild-errors EasyVCR.sln

# Install required .NET versions and tools (Windows only)
setup-win:
    scripts\win\setup.bat

# Install required .NET versions and tools (Unix only)
setup-unix:
    ./scripts/unix/setup.sh

# Run all tests in all projects in all configured frameworks (unit + compatibility)
test:
    dotnet test

# Run the unit tests for a specific framework
# Always run unit tests in Debug mode to allow access to internal members
unit-test fw=FW:
    dotnet test EasyVCR.Tests/EasyVCR.Tests.csproj -f {{fw}} -c Debug

# Run the F# compatibility tests for a specific framework
fs-compat-test fw=FW:
    dotnet test EasyVCR.Compatibility.FSharp/EasyVCR.Compatibility.FSharp.fsproj -f {{fw}} -restore

# Run the VB compatibility tests for a specific framework
vb-compat-test fw=FW:
    dotnet test EasyVCR.Compatibility.VB/EasyVCR.Compatibility.VB.vbproj -f {{fw}} -restore

# Run the Net Standard compatibility tests for a specific framework
netstandard-compat-test fw=FW:
    dotnet test EasyVCR.Compatibility.NetStandard/EasyVCR.Compatibility.NetStandard.csproj -f {{fw}} -restore
