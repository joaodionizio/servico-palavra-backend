#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

echo "[verify] restoring solution"
dotnet restore ServicoPalavra.sln

echo "[verify] building solution"
dotnet build ServicoPalavra.sln --no-restore --configuration Release

echo "[verify] running tests"
dotnet test ServicoPalavra.sln --no-build --configuration Release

echo "[verify] backend gate passed"
