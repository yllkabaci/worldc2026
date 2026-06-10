The holdout scenarios live **outside this repo** at `~/Developer/Hackathon-holdout/holdout-scenarios/`. Open that vault, find scenario $ARGUMENTS (e.g. SC-11), and:

1. Describe exactly what must happen to test it.
2. Test it against the running app over HTTP only — never by reading the implementation.
3. Report PASS or FAIL.
4. If FAIL: identify which part of `SPEC.md` / the feature spec is ambiguous or missing, quote it, and propose the exact spec fix.

Do not patch code to make a scenario pass — fix the specification and let the build rerun. Mark the scenario PASS/FAIL in the vault.
