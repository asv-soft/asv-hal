![linkedin](https://github.com/user-attachments/assets/4fa5221e-7ae5-4b6b-98a8-1c1e39b49afb)

# asv-hal

[![NuGet version](https://img.shields.io/nuget/v/Asv.Hal)](https://www.nuget.org/packages/Asv.Hal)
[![Build and tests](https://github.com/asv-soft/asv-hal/actions/workflows/code-quality.yml/badge.svg?branch=main)](https://github.com/asv-soft/asv-hal/actions/workflows/code-quality.yml)
[![Documentation](https://github.com/asv-soft/asv-hal/actions/workflows/docs.yml/badge.svg?branch=main)](https://asv.me/asv-hal/)
[![GitHub Packages](https://img.shields.io/badge/GitHub-Packages-blue?logo=github)](https://github.com/orgs/asv-soft/packages/nuget/package/Asv.Hal)

A .NET hardware abstraction library for displays, keyboards, GPIO, LEDs, and switches.
It includes screen and keyboard implementations, controls, routed events, and GPIO providers.

## Documentation

Read the [Asv.Hal documentation](https://asv.me/asv-hal/) for an overview and development instructions.

## Installation

```bash
dotnet add package Asv.Hal
```

Stable packages are available on [NuGet](https://www.nuget.org/packages/Asv.Hal).
Development packages are published to [GitHub Packages](https://github.com/orgs/asv-soft/packages/nuget/package/Asv.Hal).

## Build and test

The solution targets .NET 10. Shared build settings and `ProductVersion` are defined
in `src/Directory.Build.props`.

```bash
dotnet restore src/Asv.Hal.slnx
dotnet build src/Asv.Hal.slnx -c Release --no-restore
dotnet test src/Asv.Hal.slnx -c Release --no-build --no-restore
```

## Documentation development

Documentation sources live in `src/Writerside`. See the
[development guide](src/Writerside/topics/development.md) for the build and publishing process.
