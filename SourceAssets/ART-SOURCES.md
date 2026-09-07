# Station V2 art sources

- Ceramic: ambientCG Tiles002, https://ambientcg.com/a/Tiles002 . 2K color, OpenGL normal, roughness and displacement maps.
- Floor: ambientCG Tiles040, https://ambientcg.com/a/Tiles040 . 2K maps, subdued aggregate contrast in the surface shader.
- Source archives retained here; only texture maps used by the scene are imported.
- Poster atlas: generated using the built-in image_gen tool, saved as `Assets/Textures/StationPosters.png`. Prompt: one square texture atlas containing a 3-column, 2-row grid of flat, portrait Japanese metro advertisements: red dental clinic, blush photographic makeup exhibition, blue/black music festival, blue recruitment, yellow cafe, green garden campaign. Front-facing artwork with photographic imagery, realistic print colors and Japanese typography; no walls, frames, gutters or perspective.
- Sign lettering uses locally installed Arial and MS Gothic fonts.
- All environment meshes and lighting are authored in `Assets/Editor/StationV2.cs`.

Useful rendering reference: Unity LightingSettings API, https://docs.unity.cn/ScriptReference/LightingSettings.html . Post Processing 3.5.4 sources are included in the locally installed Unity editor package cache.
