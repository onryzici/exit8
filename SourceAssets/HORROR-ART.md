# Horror encounter assets

The active standing and ceiling creatures use the existing MIT-licensed Microsoft Rocketbox human mesh (see Assets/Commuter/LICENSE.md), with altered proportions and procedural poses in HollowActor.cs. The ceiling pose drives the arm and leg chains. DetailedDoorHand.fbx is extracted from the same weighted mesh; its original body UVs are retained.

The active model copy is Assets/Resources/Horror/HollowCommuter.fbx. The face texture is Assets/Resources/Horror/HollowHead.png. Original commuter textures and its model are preserved.

The face was produced with the built-in image_gen tool, editing a PNG conversion of Assets/Commuter/m014_head_color.tga. No fallback CLI was used.

Final image prompt:

> Use case: precise-object-edit. This is a production UV albedo atlas for a 3D horror game character, not a portrait. Edit the supplied head texture into an extremely unsettling dead-eyed supernatural subway commuter. Preserve EXACT UV layout, all feature positions, silhouette, face proportions, mouth shape, ears, hairline, black background, the bottom-left mouth interior island, and the bottom eyeball island. Do not move or resize any islands or add perspective. Same square atlas framing. Photoreal skin: cadaverous pale gray ivory, sunken dark purple eye rims with thin black tear residue down cheeks, fine blue veins at temples and neck, unhealthy mottled pores, thin cracked almost-black lips, dry discolored skin. Keep lips closed in exactly the same location; no open mouth painted on flat face. Make bottom eye island a cloudy milky eye with a tiny indistinct gray pupil. Hair remains dark and realistically textured. Skin should be bright enough to read in dim lighting, not black. Menacing forensic realism, no cartoon, no exaggerated painted eyebrows, no smiling, no bright glowing eyes, no graphic wounds or exposed tissue. Flat diffuse/albedo illumination only; no added dramatic shadows, no labels or text. Output a square high-detail texture atlas.

Blood is an animated procedural shader with separate wall trails, falling drops, and an expanding floor pool. Threat cues and heartbeat are synthesized by HorrorEncounters.cs with fades and limited volume.

The initial sculpted studies LongWatcher, CeilingCrawler and DoorHand are retained as editable Blender drafts in SourceAssets; the active creatures use the more detailed human anatomy.
