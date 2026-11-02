# Git and GitHub routine ("push per my routine")

> **Part of The Protocol** (master: `C:\Users\Andre\Desktop\AIGames\the-protocol`). Every game repo has a copy in `protocol/`. A project's `CLAUDE.md` can override it, outside its Protocol section.

Every prompt and `CLAUDE.md` that says "push per my routine" means this.

1. **Branch:** `main` only, no feature branches. Commit after each working step with a clear message. Never force-push, never rewrite history.
2. **Before the first push:** check `.gitignore` covers dependencies and builds (`node_modules/`, `dist/`, `Library/`...), and scan for secrets (keys, tokens, `.env`). Nothing secret goes to GitHub, private or not.
3. **No repo yet?** Claude Code makes it with the GitHub CLI (`gh`, already signed in as Eggzn), after checking with Andre:
   `gh repo create <folder-name> --private --source=. --remote=origin --push`
   then confirms with `gh repo view --json visibility` that it says **PRIVATE**. (By hand instead: GitHub Desktop → File → New repository, keep "private" ticked.)
4. **Push:** `git push` to `origin main`.
5. **CI:** if the repo has `.github/workflows/`, watch the run (`gh run watch`) and report pass/fail. A red run gets fixed before moving on.
6. **Before a deploy:** say what on `main` hasn't been playtested yet.
7. **Only commit your own work.** Files Andre is working on (untracked art, models, scenes) stay out of a commit unless he says so.
8. **The Protocol itself** follows the same routine: every change gets a `CHANGELOG.md` line, a commit and a push.

**Checking a web page in the browser:** Claude in Chrome can't open `file://` pages. Serve the folder instead (`npx vite` in a Vite project, otherwise `npx serve`) and open the `http://localhost:...` address; stop the server afterwards.
