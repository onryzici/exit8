# Exit 8 structure research and implementation

Research date: 2026-09-07.
Primary source: https://store.steampowered.com/app/2653790/The_Exit_8/?l=english
Additional official listing: https://www.nintendo.com/us/store/products/the-exit-8-switch/

The official premise is an apparently endless underground passage: inspect the environment, turn back when an anomaly is present, continue when it is normal, and find Exit 8. The source also documents configurable camera and motion-blur settings.

Implemented for this project:
- Flat entry and return corridors replace the immediate stair route.
- Neighbouring corridor geometry continues beyond both portal planes, so the camera sees the matching passage before a position wrap.
- Portal translation preserves the player's position within the passage and heading; post-processing history resets on a wrap.
- A normal first pass lets the player learn the environment.
- Anomaly decisions advance an eight-step streak; a wrong decision resets it.
- Eight authored environmental anomalies restore fully between rounds.
- The exit passage appears only after the streak; walking through it completes the run.
- Right mouse button holds a smooth 1.2x digital zoom.

This is an original implementation of that observation-and-turn-back loop, not a reproduction of every anomaly or character in the commercial game. A walking commuter NPC is not included in this version.

## 2026-09-07: consistent orientation and horror pass
Primary sources reviewed:
- https://www.nintendo.com/jp/topics/article/e4d0a1ce-990a-4562-9bc8-51beb1189fa2 (Nintendo: learn the initial passage, compare the same-looking passage, correct decisions increase the exit number.)
- https://gamemakers.jp/article/2024_07_09_72386/ (Kotake interview: Exit 8 recycled two passage instances; three consecutive normal passages force an anomaly. Platform 8 examples in the interview are NOT treated as Exit 8's anomaly list.)
- https://store.steampowered.com/app/2653790/The_Exit_8/?l=english (official turn-back/continue rules).

Implementation decisions: an observation game requires a stable baseline. Retreat wraps through a half-turn portal into the canonical entrance, with a correspondingly rotated neighbour preview. Advancing also arrives at that same entrance. Thus posters, doors and signage retain the same positions in the player's view. The sign-flipping anomaly is removed. After three normal rounds an anomaly is guaranteed.

Authored horror encounters: knocking service door, a sculpted watcher that approaches while off-screen, a ceiling apparition, footsteps behind the player during a blackout, and a refracting flood wave advancing from the far end. These are our own encounters, not a claim to reproduce all commercial-game anomalies. Watcher and tactile-tile Blender source files are included. Impact, breathing and flood-rush audio are synthesized for this project; the existing licensed subway recordings supply footsteps.