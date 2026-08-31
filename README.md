# ttgame (archived)

This repository is **read-only**. Mini-game work moved to [BrainlessMinigames/BrainlessMinis](https://github.com/BrainlessMinigames/BrainlessMinis).

- Shared modules: `comm` → [BrainlessMinigames/brainless-comm](https://github.com/BrainlessMinigames/brainless-comm)
- Bordy: [BrainlessMinigames/Bordy](https://github.com/BrainlessMinigames/Bordy)
- WordTT: [BrainlessMinigames/WordTT](https://github.com/BrainlessMinigames/WordTT)
- Almost Perfect: [BrainlessMinigames/AlmostPerfect](https://github.com/BrainlessMinigames/AlmostPerfect)
- FishOff: [BrainlessMinigames/FishOff](https://github.com/BrainlessMinigames/FishOff)

Do not merge new game code here. Clone the superproject and pull the mini you need:

```bash
git clone --recurse-submodules git@github.com:BrainlessMinigames/BrainlessMinis.git
cd BrainlessMinis
./scripts/mini pull Bordy
# ./scripts/mini pull AlmostPerfect
# ./scripts/mini pull FishOff
```
