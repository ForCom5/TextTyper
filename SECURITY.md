# Security policy

## Supported versions

| Version | Supported |
| --- | --- |
| 2.x | Yes |
| 1.x | No. That build is the old SendKeys app and is not maintained. |

## Reporting a vulnerability

Do not open a public issue for a suspected vulnerability.

Use the repository's private vulnerability report if it is enabled: Security and quality → Report a vulnerability. That keeps the report off the public issue list until it is triaged.

Otherwise email github@forcom5.com. Include the version or commit, what you expected, what happened, and a minimal way to reproduce it. A Windows version and whether the target window was elevated helps for typing bugs.

TextTyper types into the focused window. A report that it can type keystrokes after you start it is the feature, not a vulnerability. In scope: unexpected code execution, unsafe handling of the text you ask it to type, or a release artifact that does not match the tagged source.

You should get an acknowledgement within 7 days. A fix or a wontfix, with a reason, should follow within 30 days for anything that is actually in scope.
