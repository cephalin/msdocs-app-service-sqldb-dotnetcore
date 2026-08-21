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
if ! diff_paths="$(git diff --name-only "$main_ref" "$starter_ref")"; then
    printf 'Failed to diff refs: %s and %s\n' "$main_ref" "$starter_ref" >&2
    exit 1
fi

while IFS= read -r path; do
    if [[ -z "$path" ]]; then
        continue
    fi

    if ! allowed_difference "$path"; then
        unexpected+=("$path")
    fi
done <<< "$diff_paths"

if ((${#unexpected[@]} > 0)); then
    printf 'Unexpected branch differences:\n' >&2
    printf '  %s\n' "${unexpected[@]}" >&2
    exit 1
fi

printf 'Branch differences match the synchronization allowlist.\n'
