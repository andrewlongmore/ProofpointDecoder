# ProofpointDecoder Project

## Producing self-contained executables

Prerequisite: .NET 10 SDK installed.

```dos
dotnet publish -c Release -r win-x86
dotnet publish -c Release -r win-x64
```

## Usage

```dos
bin\Release\net10.0\win-x86\publish\ProofpointDecoder.exe ^
  "https://urldefense.proofpoint.com/v1/url?u=http://www.example.com&k=0"
```
