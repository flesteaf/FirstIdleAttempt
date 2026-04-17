# Terminal UX — Behaviour Contracts

These contracts describe the observable input/output behaviour of each terminal UX
feature. They are the basis for edit-mode unit tests (CommandHistory) and play-mode
integration tests (TerminalController key handling, TerminalOutputView scroll).

---

## Contract 1: CommandHistory — Add and Cap

```
GIVEN  a CommandHistory with MaxEntries = 50
WHEN   50 commands are added one by one
THEN   Count == 50

WHEN   a 51st command is added
THEN   Count == 50
AND    the oldest entry (original index 0) is gone
AND    the newest entry is the 51st command
```

---

## Contract 2: CommandHistory — NavigateBack

```
GIVEN  history contains ["scan", "ls", "inject miner"]  (newest = "inject miner")
AND    _navIndex == -1

WHEN   NavigateBack() is called once
THEN   returns "inject miner"
AND    _navIndex == 2

WHEN   NavigateBack() is called again
THEN   returns "ls"

WHEN   NavigateBack() is called at oldest entry (index 0)
THEN   returns "scan"
AND    _navIndex does not go below 0

GIVEN  history is empty
WHEN   NavigateBack() is called
THEN   returns null
```

---

## Contract 3: CommandHistory — NavigateForward

```
GIVEN  history = ["scan", "ls", "inject miner"]
AND    _navIndex == 0  (user navigated to oldest entry)

WHEN   NavigateForward() is called once
THEN   returns "ls"

WHEN   NavigateForward() is called at index == Count-1 (newest)
THEN   returns null  (signals: clear the input field)
AND    _navIndex == -1  (navigation reset)
```

---

## Contract 4: CommandHistory — ResetNavigation

```
GIVEN  _navIndex == 1  (mid-history browse)
WHEN   ResetNavigation() is called
THEN   _navIndex == -1
AND    history entries are unchanged
```

---

## Contract 5: CommandHistory — Add Resets Navigation

```
GIVEN  _navIndex == 2
WHEN   Add("new command") is called
THEN   _navIndex == -1
AND    "new command" is the last entry
```

---

## Contract 6: CommandParser — GetRegisteredVerbs

```
GIVEN  verbs "inject", "scan", "ls", "crack" are registered
WHEN   GetRegisteredVerbs() is called
THEN   returns ["crack", "inject", "ls", "scan"]  (sorted alphabetically)
AND    the returned collection is read-only (no Add/Remove)
```

---

## Contract 7: TerminalOutputView — Scroll Pending Guard

```
GIVEN  _scrollPending == false
WHEN   AppendLine() is called
THEN   _scrollPending becomes true
AND    exactly one scroll coroutine is scheduled

WHEN   AppendLine() is called again before the coroutine fires
THEN   _scrollPending is still true
AND    no additional coroutine is scheduled
AND    after the coroutine completes, _scrollPending == false
```

---

## Contract 8: Autocomplete — Single Match

```
GIVEN  registered verbs: ["crack", "inject", "ls", "scan", "show"]
AND    input field text == "sc"
WHEN   Tab is pressed
THEN   input field text == "scan"
AND    no suggestion line is appended to output
```

---

## Contract 9: Autocomplete — Multiple Matches

```
GIVEN  registered verbs: ["scan", "show"]
AND    input field text == "s"
WHEN   Tab is pressed
THEN   input field text is unchanged ("s")
AND    a suggestion line is appended to output containing "scan" and "show"
```

---

## Contract 10: Autocomplete — No Match

```
GIVEN  input field text == "xyz"
WHEN   Tab is pressed
THEN   input field text is unchanged ("xyz")
AND    no suggestion line is appended to output
```

---

## Contract 11: Autocomplete — Empty Input

```
GIVEN  input field text == ""
WHEN   Tab is pressed
THEN   input field text is unchanged ("")
AND    a suggestion line listing all registered verbs is appended
```

---

## Contract 12: History Navigation — Up Key

```
GIVEN  history = ["scan", "inject miner"]
AND    input field is focused
WHEN   Up arrow is pressed once
THEN   input field text == "inject miner"

WHEN   Up arrow is pressed again
THEN   input field text == "scan"
```

---

## Contract 13: History Navigation — Down Key to Clear

```
GIVEN  _navIndex == 0  (at oldest entry "scan")
WHEN   Down arrow is pressed enough times to reach newest then one more
THEN   input field text == ""  (cleared)
AND    _navIndex == -1
```

---

## Contract 14: On Submit — History Entry Saved

```
GIVEN  player types "ls" and submits
THEN   "ls" is added to CommandHistory
AND    input field is cleared
AND    output shows "> ls" echo and command result
```
