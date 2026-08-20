#!/usr/bin/env bash

set -euo pipefail

main_ref="${1:-upstream/main}"
starter_ref="${2:-upstream/starter-no-infra}"

allowed_difference() {
    case "$1" in
        Program.cs|README.md|azure.yaml|infra/*)
            return 0
            ;;
        *)
            return 1
            ;;
    esac
}

unexpected=()
while IFS= read -r path; do
    if ! allowed_difference "$path"; then
        unexpected+=("$path")
    fi
done < <(git diff --name-only "$main_ref" "$starter_ref")

if ((${#unexpected[@]} > 0)); then
    printf 'Unexpected branch differences:\n' >&2
    printf '  %s\n' "${unexpected[@]}" >&2
    exit 1
fi

printf 'Branch differences match the synchronization allowlist.\n'
